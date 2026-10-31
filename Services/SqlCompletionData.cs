using System;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;

namespace Fdb.Services;

public class SqlCompletionData : ICompletionData
{
    public string Text { get; }
    public object Content => Text;
    public object? Description { get; }
    public double Priority { get; }
    public Avalonia.Media.IImage? Image => null;

    public SqlCompletionData(string text, string? description = null, double priority = 0)
    {
        Text = text;
        Description = description ?? $"SQL Keyword: {text}";
        Priority = priority;
    }

    public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
    {
        textArea.Document.Replace(completionSegment, Text);
    }
}
