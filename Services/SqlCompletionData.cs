using System;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;

namespace Fdb.Services;

public class SqlCompletionData : ICompletionData
{
    public string Text { get; }
    public string InsertionText { get; }
    public string Category { get; }
    public bool IsSnippet { get; }
    public object Content { get; }
    public object? Description { get; }
    public double Priority { get; }
    public Avalonia.Media.IImage? Image => null;

    public SqlCompletionData(string text, string? description = null, double priority = 0, bool isSnippet = false, string? insertionText = null, string? category = null)
    {
        Text = text;
        IsSnippet = isSnippet;
        InsertionText = insertionText ?? text;
        Category = category ?? (isSnippet ? "Snippet" : "Keyword");
        Priority = priority;
        Description = description ?? (isSnippet ? $"{text}\nCode Snippet Template" : $"SQL Keyword: {text}");

        // Return formatted string for Content to avoid control visual parent conflicts in ListBox
        Content = isSnippet ? $"{text}  [Snippet]" : text;
    }

    public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
    {
        try
        {
            if (completionSegment != null && textArea?.Document != null)
            {
                textArea.Document.Replace(completionSegment, InsertionText);
            }
        }
        catch
        {
        }
    }

    public override string ToString() => Text;
}
