using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Fdb.Services;

public class SqlSnippet
{
    public string Keyword { get; set; } = string.Empty;
    public string Snippet { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class SqlKeywordsData
{
    public List<string> Keywords { get; set; } = new();
    public List<SqlSnippet> Snippets { get; set; } = new();
}

public class SqlKeywordsService
{
    private static SqlKeywordsService? _instance;
    public static SqlKeywordsService Instance => _instance ??= new SqlKeywordsService();

    public List<string> Keywords { get; private set; } = new();
    public List<SqlSnippet> Snippets { get; private set; } = new();
    public Regex? KeywordRegex { get; private set; }

    public SqlKeywordsService()
    {
        LoadKeywords();
    }

    private void LoadKeywords()
    {
        try
        {
            // 1. Try to load from Assets/sql_keywords.json on disk
            string localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "sql_keywords.json");
            if (!File.Exists(localPath))
            {
                // Try from project directory
                localPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "sql_keywords.json");
            }

            if (File.Exists(localPath))
            {
                string json = File.ReadAllText(localPath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var data = JsonSerializer.Deserialize<SqlKeywordsData>(json, options);
                if (data != null && data.Keywords.Count > 0)
                {
                    Keywords = data.Keywords;
                    Snippets = data.Snippets;
                    BuildRegex();
                    return;
                }
            }
        }
        catch
        {
            // Fallback below
        }

        // Fallback default keywords
        Keywords = new List<string>
        {
            "SELECT", "FROM", "WHERE", "INSERT", "INTO", "VALUES", "UPDATE", "SET", "DELETE",
            "CREATE", "ALTER", "DROP", "TRUNCATE", "TABLE", "VIEW", "INDEX", "DATABASE",
            "JOIN", "INNER", "LEFT", "RIGHT", "FULL", "CROSS", "ON", "GROUP", "ORDER", "BY",
            "HAVING", "ASC", "DESC", "TOP", "DISTINCT", "UNION", "ALL", "AND", "OR", "NOT",
            "IN", "EXISTS", "BETWEEN", "LIKE", "IS", "NULL", "AS", "CASE", "WHEN", "THEN", "ELSE", "END",
            "PRIMARY", "KEY", "FOREIGN", "REFERENCES", "CONSTRAINT", "DEFAULT", "COUNT", "SUM", "AVG",
            "MIN", "MAX", "INT", "BIGINT", "VARCHAR", "NVARCHAR", "BIT", "DATETIME2"
        };
        BuildRegex();
    }

    private void BuildRegex()
    {
        // Sort by length descending to match composite keywords first (e.g. "PRIMARY KEY" before "PRIMARY")
        var sorted = Keywords.OrderByDescending(k => k.Length).Select(Regex.Escape);
        string pattern = $@"\b({string.Join("|", sorted)})\b";
        KeywordRegex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }
}
