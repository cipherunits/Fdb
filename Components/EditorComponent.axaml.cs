using System;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using Fdb.Services;
using Fdb.ViewModels;

namespace Fdb.Components;

public partial class EditorComponent : UserControl
{
    private EditorViewModel? _viewModel;
    private CompletionWindow? _completionWindow;

    public EditorComponent()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;

        // 1. Install SQL Keyword Syntax Colorizer (Blue Highlighting)
        CodeEditor.TextArea.TextView.LineTransformers.Add(new SqlKeywordColorizer());

        // 2. Install Code Completion & Keyboard Handlers
        CodeEditor.TextArea.TextEntered += OnTextEntered;
        CodeEditor.TextArea.KeyDown += OnKeyDown;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        _viewModel = DataContext as EditorViewModel;

        if (_viewModel != null)
        {
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            LoadActiveTabContent();
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EditorViewModel.SelectedTab))
        {
            LoadActiveTabContent();
        }
    }

    private void LoadActiveTabContent()
    {
        if (_viewModel?.SelectedTab == null || CodeEditor == null) return;

        if (CodeEditor.Document != _viewModel.SelectedTab.Document)
        {
            CodeEditor.Document = _viewModel.SelectedTab.Document;
            CodeEditor.TextArea.Focus();
        }
    }

    public void OnTabPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if ((sender as Border)?.DataContext is EditorTabItem clickedTab && _viewModel != null)
        {
            _viewModel.SelectTab(clickedTab);
        }
    }

    private void OnTextEntered(object? sender, TextInputEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Text)) return;

        char enteredChar = e.Text[0];
        // Only trigger completion if completion window is NOT already active and typing letters/_
        if (_completionWindow == null && (char.IsLetter(enteredChar) || enteredChar == '_'))
        {
            int caretOffset = CodeEditor.CaretOffset;
            string currentWord = GetWordBeforeOffset(caretOffset);

            // Auto-trigger completion only for keywords with 3 or more letters typed to avoid typing interruption
            if (currentWord.Length >= 3)
            {
                ShowCompletionWindow(false);
            }
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        // 1. Explicit Ctrl+Z Undo
        if (e.Key == Key.Z && e.KeyModifiers == KeyModifiers.Control)
        {
            if (CodeEditor.CanUndo)
            {
                CodeEditor.Undo();
                e.Handled = true;
                return;
            }
        }

        // 2. Explicit Ctrl+Y or Ctrl+Shift+Z Redo
        if ((e.Key == Key.Y && e.KeyModifiers == KeyModifiers.Control) ||
            (e.Key == Key.Z && e.KeyModifiers == (KeyModifiers.Control | KeyModifiers.Shift)))
        {
            if (CodeEditor.CanRedo)
            {
                CodeEditor.Redo();
                e.Handled = true;
                return;
            }
        }

        // 3. Ctrl+Space to trigger IntelliSense manually
        if (e.Key == Key.Space && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            ShowCompletionWindow(true);
            e.Handled = true;
            return;
        }

        // 4. Escape closes completion cleanly if open and restores focus
        if (e.Key == Key.Escape && _completionWindow != null)
        {
            _completionWindow.Close();
            _completionWindow = null;
            CodeEditor.TextArea.Focus();
            e.Handled = true;
        }
    }

    private void ShowCompletionWindow(bool showAll)
    {
        if (CodeEditor == null) return;

        int caretOffset = CodeEditor.CaretOffset;
        string currentWord = GetWordBeforeOffset(caretOffset);

        if (!showAll && currentWord.Length < 1) return;

        var keywordsService = SqlKeywordsService.Instance;
        var matchingKeywords = showAll
            ? keywordsService.Keywords
            : keywordsService.Keywords
                .Where(k => k.StartsWith(currentWord, StringComparison.OrdinalIgnoreCase))
                .ToList();

        // Check matching snippets
        var matchingSnippets = showAll
            ? keywordsService.Snippets
            : keywordsService.Snippets
                .Where(s => s.Keyword.StartsWith(currentWord, StringComparison.OrdinalIgnoreCase))
                .ToList();

        if (matchingKeywords.Count == 0 && matchingSnippets.Count == 0 && !showAll) return;

        // If window already open, close it cleanly first to avoid popup collision
        if (_completionWindow != null)
        {
            _completionWindow.Close();
            _completionWindow = null;
        }

        _completionWindow = new CompletionWindow(CodeEditor.TextArea)
        {
            CloseAutomatically = true,
            CloseWhenCaretAtBeginning = true
        };

        _completionWindow.Closed += (s, e) =>
        {
            _completionWindow = null;
            CodeEditor.TextArea.Focus();
        };

        var completionList = _completionWindow.CompletionList.CompletionData;
        completionList.Clear();

        // 1. Add SQL Keywords
        foreach (var kw in matchingKeywords)
        {
            completionList.Add(new SqlCompletionData(kw, $"SQL Keyword: {kw}", 1.0));
        }

        // 2. Add Snippets
        foreach (var snip in matchingSnippets)
        {
            completionList.Add(new SqlCompletionData(snip.Snippet, $"{snip.Keyword} — {snip.Description}", 2.0));
        }

        if (completionList.Count > 0)
        {
            _completionWindow.Show();
        }
    }

    private string GetWordBeforeOffset(int offset)
    {
        if (CodeEditor?.Document == null || offset <= 0) return string.Empty;

        int start = offset;
        while (start > 0)
        {
            char c = CodeEditor.Document.GetCharAt(start - 1);
            if (!char.IsLetterOrDigit(c) && c != '_')
            {
                break;
            }
            start--;
        }

        return CodeEditor.Document.GetText(start, offset - start);
    }
}
