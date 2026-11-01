using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using Fdb.ViewModels;

namespace Fdb.Services;

public class ConnectionRecord
{
    public int Id { get; set; }
    public string Server { get; set; } = "localhost";
    public int Port { get; set; } = 1433;
    public string DatabaseName { get; set; } = "master";
    public string AuthType { get; set; } = "SQL Server Authentication";
    public string Username { get; set; } = "sa";
    public string LastConnectedAt { get; set; } = string.Empty;
}

public class QueryHistoryRecord
{
    public int Id { get; set; }
    public string QueryText { get; set; } = string.Empty;
    public string ExecutedAt { get; set; } = string.Empty;
    public string ExecutionTime { get; set; } = string.Empty;
    public int RowCount { get; set; }
    public string Status { get; set; } = "Success";
}

public class SavedQueryRecord
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string QueryText { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
}

public class DialectKeywordRecord
{
    public int Id { get; set; }
    public string Dialect { get; set; } = "General";
    public string Keyword { get; set; } = string.Empty;
    public string Category { get; set; } = "Keyword";
}

public class DatabaseStorageService
{
    private static DatabaseStorageService? _instance;
    public static DatabaseStorageService Instance => _instance ??= new DatabaseStorageService();

    private readonly string _connectionString;
    private readonly System.Threading.SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized;

    public DatabaseStorageService()
    {
        string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fdb_studio.db");
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = dbPath,
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString();
    }

    public async Task InitializeAsync()
    {
        await InitializeWithProgressAsync(null);
    }

    public async Task InitializeWithProgressAsync(Action<double, string>? reportProgress)
    {
        if (_initialized)
        {
            reportProgress?.Invoke(100, "Studio Database Ready.");
            return;
        }

        await _initLock.WaitAsync();
        try
        {
            if (_initialized)
            {
                reportProgress?.Invoke(100, "Studio Database Ready.");
                return;
            }

            reportProgress?.Invoke(10, "Opening SQLite Database connection...");
            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            // Locate init_database.sql file
            string sqlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "init_database.sql");
            if (!File.Exists(sqlFilePath))
            {
                sqlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "init_database.sql");
            }

            if (File.Exists(sqlFilePath))
            {
                string fullSql = await File.ReadAllTextAsync(sqlFilePath);
                await ExecuteInitScriptStepsAsync(connection, fullSql, reportProgress);
            }
            else
            {
                // Fallback direct execution if file is missing
                reportProgress?.Invoke(30, "Running core SQLite schema initialization...");
                await ExecuteFallbackSchemaAsync(connection);
            }

            _initialized = true;

            reportProgress?.Invoke(90, "Pre-caching active SQL dialect keywords...");
            await SqlKeywordsService.Instance.LoadKeywordsFromDatabaseAsync();

            reportProgress?.Invoke(100, "Studio Environment Ready — SQLite Engine Initialized.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DatabaseStorageService] Initialization error: {ex.Message}");
            reportProgress?.Invoke(100, "Ready (with fallback mode).");
            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    private static async Task ExecuteInitScriptStepsAsync(SqliteConnection connection, string fullSql, Action<double, string>? reportProgress)
    {
        // Split script into logical steps delimited by step headers
        var stepSections = fullSql.Split(new[] { "-- [STEP " }, StringSplitOptions.RemoveEmptyEntries);

        double baseProgress = 15;
        double progressIncrement = 70.0 / Math.Max(1, stepSections.Length);

        for (int i = 0; i < stepSections.Length; i++)
        {
            string section = stepSections[i].Trim();
            if (string.IsNullOrWhiteSpace(section)) continue;

            // Extract step description from first line
            string firstLine = section.Split('\n')[0].Trim();
            string stepTitle = firstLine.Contains(']') ? firstLine.Substring(firstLine.IndexOf(']') + 1).Trim() : firstLine;
            double currentProgress = baseProgress + (i * progressIncrement);

            reportProgress?.Invoke(Math.Min(88, currentProgress), $"Executing SQL Queries: {stepTitle}...");

            string sqlCode = section.Contains('\n') ? section.Substring(section.IndexOf('\n') + 1) : section;

            // Execute SQL batch via Dapper
            await connection.ExecuteAsync(sqlCode);

            // Small yielding delay to ensure responsive UI progress render
            await Task.Delay(40);
        }
    }

    private static async Task ExecuteFallbackSchemaAsync(SqliteConnection connection)
    {
        string fallbackSql = @"
            CREATE TABLE IF NOT EXISTS DialectKeywords (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Dialect TEXT NOT NULL,
                Keyword TEXT NOT NULL,
                Category TEXT NOT NULL DEFAULT 'Keyword',
                UNIQUE(Dialect, Keyword)
            );
            CREATE TABLE IF NOT EXISTS DialectSnippets (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Dialect TEXT NOT NULL,
                Keyword TEXT NOT NULL,
                Snippet TEXT NOT NULL,
                Description TEXT NOT NULL,
                UNIQUE(Dialect, Keyword)
            );
            CREATE TABLE IF NOT EXISTS Connections (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Server TEXT NOT NULL,
                Port INTEGER NOT NULL,
                DatabaseName TEXT NOT NULL,
                AuthType TEXT NOT NULL,
                Username TEXT,
                LastConnectedAt TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS QueryHistory (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                QueryText TEXT NOT NULL,
                ExecutedAt TEXT NOT NULL,
                ExecutionTime TEXT NOT NULL,
                RowCount INTEGER NOT NULL,
                Status TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS SavedQueries (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                QueryText TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS QueryResults (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                RowIndex INTEGER NOT NULL,
                RecordId INTEGER NOT NULL,
                Name TEXT NOT NULL,
                Email TEXT NOT NULL,
                Role TEXT NOT NULL,
                Status TEXT NOT NULL,
                CreatedAt TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS AppSettings (
                Key TEXT PRIMARY KEY,
                Value TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
            );
            INSERT OR IGNORE INTO AppSettings (Key, Value, UpdatedAt) VALUES ('Theme', 'Light', datetime('now'));
        ";

        // Execute fallback schema via Dapper
        await connection.ExecuteAsync(fallbackSql);
    }

    public async Task<List<string>> GetKeywordsAsync(string dialect = "MSSQL")
    {
        try
        {
            await InitializeAsync();
            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = @"
                SELECT DISTINCT Keyword FROM DialectKeywords 
                WHERE Dialect = @dialect OR Dialect = 'General' 
                ORDER BY length(Keyword) DESC, Keyword ASC;";

            var result = await connection.QueryAsync<string>(sql, new { dialect });
            return result.ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DatabaseStorageService] GetKeywordsAsync error: {ex.Message}");
            return new List<string>();
        }
    }

    public async Task<List<SqlSnippet>> GetSnippetsAsync(string dialect = "MSSQL")
    {
        try
        {
            await InitializeAsync();
            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = @"
                SELECT Keyword, Snippet, Description FROM DialectSnippets 
                WHERE Dialect = @dialect OR Dialect = 'General';";

            var result = await connection.QueryAsync<SqlSnippet>(sql, new { dialect });
            return result.ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DatabaseStorageService] GetSnippetsAsync error: {ex.Message}");
            return new List<SqlSnippet>();
        }
    }

    public async Task<List<string>> GetSupportedDialectsAsync()
    {
        try
        {
            await InitializeAsync();
            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = "SELECT DISTINCT Dialect FROM DialectKeywords WHERE Dialect != 'General' ORDER BY Dialect ASC;";
            var result = await connection.QueryAsync<string>(sql);
            return result.ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DatabaseStorageService] GetSupportedDialectsAsync error: {ex.Message}");
            return new List<string>();
        }
    }

    public async Task<List<QueryResultRow>> GetQueryResultsAsync()
    {
        try
        {
            await InitializeAsync();
            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = @"
                SELECT RowIndex, RecordId AS Id, Name, Email, Role, Status, CreatedAt 
                FROM QueryResults 
                ORDER BY RowIndex ASC;";

            var result = await connection.QueryAsync<QueryResultRow>(sql);
            return result.ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DatabaseStorageService] GetQueryResultsAsync error: {ex.Message}");
            return new List<QueryResultRow>();
        }
    }

    public async Task SaveQueryHistoryAsync(string queryText, string executionTime, int rowCount, string status = "Success")
    {
        try
        {
            await InitializeAsync();
            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = @"
                INSERT INTO QueryHistory (QueryText, ExecutedAt, ExecutionTime, RowCount, Status)
                VALUES (@queryText, datetime('now'), @executionTime, @rowCount, @status);";

            await connection.ExecuteAsync(sql, new { queryText, executionTime, rowCount, status });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DatabaseStorageService] SaveQueryHistoryAsync error: {ex.Message}");
        }
    }

    public async Task<List<QueryHistoryRecord>> GetQueryHistoryAsync()
    {
        try
        {
            await InitializeAsync();
            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = @"
                SELECT Id, QueryText, ExecutedAt, ExecutionTime, RowCount, Status 
                FROM QueryHistory 
                ORDER BY Id DESC 
                LIMIT 50;";

            var result = await connection.QueryAsync<QueryHistoryRecord>(sql);
            return result.ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DatabaseStorageService] GetQueryHistoryAsync error: {ex.Message}");
            return new List<QueryHistoryRecord>();
        }
    }

    public async Task SaveConnectionAsync(string server, int port, string databaseName, string authType, string username)
    {
        try
        {
            await InitializeAsync();
            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = @"
                INSERT INTO Connections (Server, Port, DatabaseName, AuthType, Username, LastConnectedAt)
                VALUES (@server, @port, @databaseName, @authType, @username, datetime('now'));";

            await connection.ExecuteAsync(sql, new
            {
                server,
                port,
                databaseName,
                authType,
                username = username ?? string.Empty
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DatabaseStorageService] SaveConnectionAsync error: {ex.Message}");
        }
    }

    public async Task<List<ConnectionRecord>> GetRecentConnectionsAsync()
    {
        try
        {
            await InitializeAsync();
            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = @"
                SELECT Id, Server, Port, DatabaseName, AuthType, COALESCE(Username, '') AS Username, LastConnectedAt 
                FROM Connections 
                ORDER BY Id DESC 
                LIMIT 20;";

            var result = await connection.QueryAsync<ConnectionRecord>(sql);
            return result.ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DatabaseStorageService] GetRecentConnectionsAsync error: {ex.Message}");
            return new List<ConnectionRecord>();
        }
    }

    public async Task<string> GetSettingAsync(string key, string defaultValue = "")
    {
        try
        {
            await InitializeAsync();
            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = "SELECT Value FROM AppSettings WHERE Key = @key LIMIT 1;";
            var result = await connection.ExecuteScalarAsync<string>(sql, new { key });
            return result ?? defaultValue;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DatabaseStorageService] GetSettingAsync error: {ex.Message}");
            return defaultValue;
        }
    }

    public async Task SaveSettingAsync(string key, string value)
    {
        try
        {
            await InitializeAsync();
            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = @"
                INSERT INTO AppSettings (Key, Value, UpdatedAt) 
                VALUES (@key, @value, datetime('now'))
                ON CONFLICT(Key) DO UPDATE SET Value = @value, UpdatedAt = datetime('now');";

            await connection.ExecuteAsync(sql, new { key, value });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DatabaseStorageService] SaveSettingAsync error: {ex.Message}");
        }
    }

    public async Task<string> GetThemeAsync() => await GetSettingAsync("Theme", "Light");
    public async Task SaveThemeAsync(string theme) => await SaveSettingAsync("Theme", theme);

    public async Task<string> GetKeywordColorAsync() => await GetSettingAsync("KeywordColor", "Blue");
    public async Task SaveKeywordColorAsync(string color) => await SaveSettingAsync("KeywordColor", color);
}
