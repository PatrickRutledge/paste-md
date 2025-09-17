using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PasteMd.Core.Services
{
    public class ClipboardService
    {
        private static readonly Regex[] MarkdownPatterns = new[]
        {
            new Regex(@"^#{1,6}\s+", RegexOptions.Multiline),  // Headers
            new Regex(@"\*\*[^*]+\*\*", RegexOptions.None),    // Bold
            new Regex(@"__[^_]+__", RegexOptions.None),        // Bold alternative
            new Regex(@"\*[^*]+\*", RegexOptions.None),        // Italic
            new Regex(@"_[^_]+_", RegexOptions.None),          // Italic alternative
            new Regex(@"^\s*[-*+]\s+", RegexOptions.Multiline), // Unordered lists
            new Regex(@"^\s*\d+\.\s+", RegexOptions.Multiline), // Ordered lists
            new Regex(@"\[([^\]]+)\]\([^)]+\)", RegexOptions.None), // Links
            new Regex(@"`[^`]+`", RegexOptions.None),          // Inline code
            new Regex(@"^```[\s\S]*?```", RegexOptions.Multiline), // Code blocks
            new Regex(@"^>\s+", RegexOptions.Multiline),       // Blockquotes
            new Regex(@"^\|.*\|", RegexOptions.Multiline),     // Tables
            new Regex(@"^---+$", RegexOptions.Multiline),      // Horizontal rules
        };

        public static string GetClipboardText()
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    return Clipboard.GetText();
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error getting clipboard text: {ex.Message}");
            }
            return string.Empty;
        }

        public static bool ContainsMarkdown(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            int patternMatches = 0;

            foreach (var pattern in MarkdownPatterns)
            {
                if (pattern.IsMatch(text))
                {
                    patternMatches++;
                    if (patternMatches >= 2)
                    {
                        Logger.Log($"Markdown detected with {patternMatches} patterns");
                        return true;
                    }
                }
            }

            Logger.Log($"Markdown detection: Only {patternMatches} pattern(s) found, need 2+");
            return false;
        }

        public static void SetClipboardData(string html, string rtf, string plainText)
        {
            try
            {
                var dataObject = new DataObject();

                // Add plain text (fallback)
                if (!string.IsNullOrEmpty(plainText))
                {
                    dataObject.SetData(DataFormats.Text, plainText);
                    dataObject.SetData(DataFormats.UnicodeText, plainText);
                }

                // Add HTML format (for OneNote, Outlook, browsers)
                if (!string.IsNullOrEmpty(html))
                {
                    var htmlFormat = CreateHtmlDataFormat(html);
                    dataObject.SetData(DataFormats.Html, htmlFormat);
                }

                // Add RTF format (for Word, rich text editors)
                if (!string.IsNullOrEmpty(rtf))
                {
                    dataObject.SetData(DataFormats.Rtf, rtf);
                }

                Clipboard.SetDataObject(dataObject, true);
                Logger.Log("Clipboard data set successfully with multiple formats");
            }
            catch (Exception ex)
            {
                Logger.Log($"Error setting clipboard data: {ex.Message}");
                throw;
            }
        }

        private static string CreateHtmlDataFormat(string html)
        {
            const string headerFormat = @"Version:0.9
StartHTML:00000097
EndHTML:{0:00000000}
StartFragment:00000133
EndFragment:{1:00000000}
<html><body>
<!--StartFragment-->";
            const string footer = @"<!--EndFragment-->
</body></html>";

            var startFragment = 133; // Fixed position after header
            var endFragment = startFragment + html.Length;
            var endHtml = headerFormat.Length + html.Length + footer.Length - 16; // Subtract placeholder chars

            var header = string.Format(headerFormat, endHtml, endFragment);
            var fullHtml = header + html + footer;

            return fullHtml;
        }
    }

    public static class Logger
    {
        private static readonly string LogPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "paste-md",
            "paste-md.log"
        );

        static Logger()
        {
            var dir = System.IO.Path.GetDirectoryName(LogPath);
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }
        }

        public static void Log(string message)
        {
            try
            {
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
                System.IO.File.AppendAllText(LogPath, logEntry);
            }
            catch
            {
                // Silently fail if logging fails
            }
        }
    }
}