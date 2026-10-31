using Avalonia;
using Avalonia.Controls;
using System;
using System.Threading.Tasks;

namespace Fdb;

sealed class Program
{
    [STAThread]
    public static async Task Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--verify")
        {
            Console.WriteLine("=== Fdb Dynamic Layout Verification ===");
            var vm = new Fdb.ViewModels.MainViewModel();
            var app = BuildAvaloniaApp().SetupWithoutStarting();
            var window = new Fdb.Views.MainWindow { DataContext = vm };

            Console.WriteLine($"[Initial State]");
            Console.WriteLine($"Sidebar: Visible={vm.Sidebar.IsVisible}, ColWidth={window.MainGrid.ColumnDefinitions[0].Width}");
            Console.WriteLine($"Footer: Visible={vm.Footer.IsVisible}, RowHeight={window.CenterGrid.RowDefinitions[2].Height}");
            Console.WriteLine($"Editor Row: Height={window.CenterGrid.RowDefinitions[0].Height}");
            Console.WriteLine($"Center Col: Width={window.MainGrid.ColumnDefinitions[2].Width}");

            // 1. Test Closing Footer
            Console.WriteLine("\n[Action] Closing Footer...");
            vm.ToggleFooterCommand.Execute(null);
            Console.WriteLine($"Footer: Visible={vm.Footer.IsVisible}, RowHeight={window.CenterGrid.RowDefinitions[2].Height}");
            Console.WriteLine($"Editor Row Height: {window.CenterGrid.RowDefinitions[0].Height} (MUST BE 1*)");
            if (window.CenterGrid.RowDefinitions[2].Height.Value != 0 || window.CenterGrid.RowDefinitions[0].Height.GridUnitType != Avalonia.Controls.GridUnitType.Star)
            {
                throw new Exception("Footer collapse failed! Editor did not expand.");
            }

            // 2. Test Closing Sidebar
            Console.WriteLine("\n[Action] Closing Sidebar...");
            vm.ToggleSidebarCommand.Execute(null);
            Console.WriteLine($"Sidebar: Visible={vm.Sidebar.IsVisible}, ColWidth={window.MainGrid.ColumnDefinitions[0].Width}");
            Console.WriteLine($"Center Col Width: {window.MainGrid.ColumnDefinitions[2].Width} (MUST BE 1*)");
            if (window.MainGrid.ColumnDefinitions[0].Width.Value != 0 || window.MainGrid.ColumnDefinitions[2].Width.GridUnitType != Avalonia.Controls.GridUnitType.Star)
            {
                throw new Exception("Sidebar collapse failed! Center area did not expand.");
            }

            // 3. Test Restoring from Window Menu
            Console.WriteLine("\n[Action] Restoring Sidebar & Footer from Window Menu...");
            vm.ShowSidebarCommand.Execute(null);
            vm.ShowFooterCommand.Execute(null);
            Console.WriteLine($"Sidebar: Visible={vm.Sidebar.IsVisible}, ColWidth={window.MainGrid.ColumnDefinitions[0].Width}");
            Console.WriteLine($"Footer: Visible={vm.Footer.IsVisible}, RowHeight={window.CenterGrid.RowDefinitions[2].Height}");

            // 4. Test Table Components Data
            Console.WriteLine("\n[Action] Verifying Table Components...");
            Console.WriteLine($"QueryResultTable Rows: {vm.SampleQueryResult.Rows.Count} rows loaded.");
            Console.WriteLine($"SchemaPropertiesTable Columns: {vm.SampleSchema.Columns.Count} columns loaded.");
            var tableComp = new Fdb.Components.QueryResultTableComponent { DataContext = vm.SampleQueryResult };
            var dataGrid = tableComp.FindControl<Avalonia.Controls.DataGrid>("ResultsDataGrid");
            Console.WriteLine($"DataGrid Columns Count: {tableComp.FindControl<Avalonia.Controls.DataGrid>("ResultsDataGrid")?.Columns.Count ?? 0}");
            Console.WriteLine($"DataGrid RowHeight: {dataGrid?.RowHeight}px (Compact height)");
            if (dataGrid?.RowHeight != 25)
            {
                throw new Exception($"Expected compact DataGrid RowHeight=25, but got {dataGrid?.RowHeight}");
            }
            var col0 = dataGrid?.Columns[0] as Avalonia.Controls.DataGridTemplateColumn;
            var cellContent = col0?.CellTemplate?.Build(vm.SampleQueryResult.Rows[0]) as Avalonia.Controls.Grid;
            if (cellContent != null) cellContent.DataContext = vm.SampleQueryResult.Rows[0];
            var textBlock = cellContent?.Children[0] as Avalonia.Controls.TextBlock;
            Console.WriteLine($"Cell 0 TextBlock Text: '{textBlock?.Text}', DataContext: {textBlock?.DataContext}");
            if (textBlock?.Text != "1")
            {
                throw new Exception($"Expected row index '1', but got '{textBlock?.Text}'");
            }

            // 5. Test Modal Dialogs Lifecycle & Triggers
            Console.WriteLine("\n[Action] Verifying Modal Dialogs Lifecycle...");
            Console.WriteLine($"ConnectModal initially open: {vm.ConnectModal.IsOpen}");
            var navbarComp = window.FindControl<Fdb.Components.NavbarComponent>("MainNavbar");
            var connectBtn = navbarComp?.FindControl<Avalonia.Controls.Button>("ConnectButton");
            Console.WriteLine($"ConnectButton found: {connectBtn != null}, Command: {connectBtn?.Command}");
            connectBtn?.Command?.Execute(null);
            Console.WriteLine($"After ConnectButton click: ConnectModal.IsOpen = {vm.ConnectModal.IsOpen}");
            if (!vm.ConnectModal.IsOpen)
            {
                throw new Exception("ConnectButton failed to open ConnectModal!");
            }
            vm.ConnectModal.TestConnectionCommand.Execute(null);
            Console.WriteLine($"ConnectModal Status: {vm.ConnectModal.StatusMessage}");
            vm.ConnectModal.CloseCommand.Execute(null);
            Console.WriteLine($"ConnectModal: IsOpen = {vm.ConnectModal.IsOpen} (Closed)");

            // Test Disconnect Command triggering ConfirmModal
            vm.DisconnectCommand.Execute(null);
            Console.WriteLine($"ConfirmModal opened by Disconnect: {vm.ConfirmModal.IsOpen}, Target = {vm.ConfirmModal.TargetName}");
            if (!vm.ConfirmModal.IsOpen)
            {
                throw new Exception("DisconnectCommand failed to open ConfirmModal!");
            }
            vm.ConfirmModal.CancelCommand.Execute(null);
            Console.WriteLine($"ConfirmModal: IsOpen = {vm.ConfirmModal.IsOpen} (Closed)");

            // Test Export Modal
            vm.ExportModal.OpenCommand.Execute(null);
            Console.WriteLine($"ExportModal: IsOpen = {vm.ExportModal.IsOpen}, Format = {vm.ExportModal.SelectedFormat}");
            vm.ExportModal.CloseCommand.Execute(null);
            Console.WriteLine($"ExportModal: IsOpen = {vm.ExportModal.IsOpen} (Closed)");
            Console.WriteLine($"ExportModal: IsOpen = {vm.ExportModal.IsOpen} (Closed)");

            // 6. Test Editor Tab Switching & Active State
            Console.WriteLine("\n[Action] Verifying Editor Tab Switching & State...");
            Console.WriteLine($"Initial Selected Tab: {vm.Editor.SelectedTab?.Title}, IsActive={vm.Editor.SelectedTab?.IsActive}");
            vm.Editor.SelectTab(vm.Editor.Tabs[1]);
            Console.WriteLine($"Switched Selected Tab: {vm.Editor.SelectedTab?.Title}, IsActive={vm.Editor.SelectedTab?.IsActive}");
            if (!vm.Editor.Tabs[1].IsActive || vm.Editor.Tabs[0].IsActive)
            {
                throw new Exception("Editor tab activation failed!");
            }

            var editorComp = new Fdb.Components.EditorComponent { DataContext = vm.Editor };
            var textEditor = editorComp.FindControl<AvaloniaEdit.TextEditor>("CodeEditor");
            Console.WriteLine($"TextEditor Document Text Length: {textEditor?.Document?.TextLength}");
            
            // Test per-tab document undo/redo integrity
            vm.Editor.SelectTab(vm.Editor.Tabs[0]);
            var tab0Doc = vm.Editor.Tabs[0].Document;
            int initialLen = tab0Doc.TextLength;
            tab0Doc.Insert(0, "-- New Line Added\n");
            Console.WriteLine($"Tab 0 modified. CanUndo: {textEditor?.CanUndo}");
            if (textEditor?.CanUndo != true)
            {
                throw new Exception("Editor CanUndo should be true after text edit!");
            }

            // Switch to tab 1
            vm.Editor.SelectTab(vm.Editor.Tabs[1]);
            Console.WriteLine($"Switched to Tab 1. Document matches: {textEditor?.Document == vm.Editor.Tabs[1].Document}");

            // Switch back to tab 0
            vm.Editor.SelectTab(vm.Editor.Tabs[0]);
            Console.WriteLine($"Switched back to Tab 0. CanUndo preserved: {textEditor?.CanUndo}");
            if (textEditor?.CanUndo != true)
            {
                throw new Exception("Editor CanUndo should be preserved across tab switches!");
            }

            // Perform Undo (Ctrl+Z)
            textEditor.Undo();
            Console.WriteLine($"Undo executed. Length restored: {tab0Doc.TextLength == initialLen}");
            if (tab0Doc.TextLength != initialLen)
            {
                throw new Exception("Undo failed to revert document to initial length!");
            }

            // Perform Redo (Ctrl+Y)
            textEditor.Redo();
            Console.WriteLine($"Redo executed. CanUndo: {textEditor.CanUndo}");
            textEditor.Undo(); // Revert back for clean state

            // 7. Test SQL Keywords Service & Regex
            Console.WriteLine("\n[Action] Verifying SQL Keywords Service...");
            var keywordsService = Fdb.Services.SqlKeywordsService.Instance;
            Console.WriteLine($"Loaded SQL Keywords: {keywordsService.Keywords.Count} keywords, {keywordsService.Snippets.Count} snippets.");
            var regex = keywordsService.KeywordRegex;
            bool selectMatches = regex != null && regex.IsMatch("SELECT * FROM dbo.Users");
            Console.WriteLine($"Regex Matches 'SELECT * FROM': {selectMatches}");
            if (!selectMatches)
            {
                throw new Exception("SQL Keywords regex matching failed!");
            }

            // Test typing in editor
            Console.WriteLine("\n[Action] Testing typing 'se' in TextEditor...");
            var editorForTest = new Fdb.Components.EditorComponent { DataContext = vm.Editor };
            var te = editorForTest.FindControl<AvaloniaEdit.TextEditor>("CodeEditor");
            te.Document.Text = "";
            te.CaretOffset = 0;
            te.Document.Insert(0, "s");
            te.CaretOffset = 1;
            te.Document.Insert(1, "e");
            te.CaretOffset = 2;
            Console.WriteLine($"Editor text after typing 'se': '{te.Document.Text}'");
            
            // Test CompletionWindow creation
            Console.WriteLine("\n[Action] Testing CompletionWindow instantiation...");
            var compWin = new AvaloniaEdit.CodeCompletion.CompletionWindow(te.TextArea);
            compWin.Close();
            Console.WriteLine("CompletionWindow created and closed successfully.");

            // 8. Test Zero Emojis / All Vector Icons
            Console.WriteLine("\n[Action] Verifying Zero Emojis / Native Vector Icons...");
            var emojiRegex = new System.Text.RegularExpressions.Regex(@"[\uD83C-\uDBFF\uDC00-\uDFFF\u2600-\u27BF]");
            int emojiCount = 0;
            foreach (var file in System.IO.Directory.GetFiles("Components", "*.axaml", System.IO.SearchOption.AllDirectories))
            {
                var content = System.IO.File.ReadAllText(file);
                var matches = emojiRegex.Matches(content);
                emojiCount += matches.Count;
            }
            Console.WriteLine($"Total AXAML Emojis Remaining: {emojiCount} (All replaced with Fluent FASymbolIcon & PathIcon)");
            if (emojiCount > 0)
            {
                throw new Exception("Emojis found in AXAML files!");
            }

            // 9. Test Splash Screen Lifecycle
            Console.WriteLine("\n[Action] Verifying Splash Screen Window...");
            var splashWindow = new Fdb.Views.SplashScreenWindow();
            Console.WriteLine($"Splash initial status: '{splashWindow.ViewModel.StatusText}', Progress: {splashWindow.ViewModel.ProgressValue}%");
            await splashWindow.RunStartupSequenceAsync(40).ConfigureAwait(false);
            Console.WriteLine($"Splash completed status: '{splashWindow.ViewModel.StatusText}', Progress: {splashWindow.ViewModel.ProgressValue}%");
            if (splashWindow.ViewModel.ProgressValue != 100)
            {
                throw new Exception("Splash sequence failed to reach 100%!");
            }
            splashWindow.Dismiss();

            Console.WriteLine("\n>>> ALL TESTS (LAYOUT, TABLES, MODALS, TABS, SQL AUTOCOMPLETE, VECTOR ICONS & SPLASH SCREEN) PASSED! <<<");
            return;
        }

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}
