using System.Text.RegularExpressions;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;

namespace Fdb.Services;

public class SqlKeywordColorizer : DocumentColorizingTransformer
{
    // Modern royal blue accent for SQL keywords
    private static readonly IBrush KeywordBlueBrush = new SolidColorBrush(Color.Parse("#2563EB"));

    protected override void ColorizeLine(DocumentLine line)
    {
        var regex = SqlKeywordsService.Instance.KeywordRegex;
        if (regex == null) return;

        int lineStartOffset = line.Offset;
        string text = CurrentContext.Document.GetText(line);

        var matches = regex.Matches(text);
        foreach (Match match in matches)
        {
            int start = lineStartOffset + match.Index;
            int end = start + match.Length;

            ChangeLinePart(start, end, element =>
            {
                element.TextRunProperties.SetForegroundBrush(KeywordBlueBrush);
            });
        }
    }
}
