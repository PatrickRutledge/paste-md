using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using PasteMd.Core.Services;

namespace PasteMd.Core.Formatters
{
    public class RtfFormatter
    {
        private readonly Dictionary<string, string> _fontTable;
        private readonly Dictionary<Color, int> _colorTable;
        private int _colorIndex;

        public RtfFormatter()
        {
            _fontTable = new Dictionary<string, string>
            {
                { "default", @"{\f0\fnil\fcharset0 Segoe UI;}" },
                { "code", @"{\f1\fmodern\fcharset0 Consolas;}" }
            };

            _colorTable = new Dictionary<Color, int>
            {
                { Color.Black, 1 },
                { Color.FromArgb(51, 51, 51), 2 },    // Text color
                { Color.FromArgb(246, 248, 250), 3 }, // Code background
                { Color.FromArgb(3, 102, 214), 4 },   // Link color
                { Color.FromArgb(106, 115, 125), 5 }, // Quote color
                { Color.FromArgb(209, 217, 224), 6 }  // Border color
            };
            _colorIndex = 7;
        }

        public string ConvertHtmlToRtf(string html)
        {
            try
            {
                // Use RichTextBox for conversion as a starting point
                using (var rtb = new RichTextBox())
                {
                    // First, get basic RTF from HTML
                    using (var webBrowser = new WebBrowser())
                    {
                        webBrowser.DocumentText = html;
                        webBrowser.Document?.ExecCommand("SelectAll", false, null);
                        webBrowser.Document?.ExecCommand("Copy", false, null);

                        if (Clipboard.ContainsText(TextDataFormat.Rtf))
                        {
                            rtb.Rtf = Clipboard.GetText(TextDataFormat.Rtf);
                        }
                        else
                        {
                            rtb.Text = StripHtml(html);
                        }
                    }

                    // Enhance the RTF with proper formatting
                    var enhancedRtf = EnhanceRtfFormatting(rtb.Rtf, html);
                    return enhancedRtf;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error converting HTML to RTF: {ex.Message}");
                return ConvertPlainTextToRtf(StripHtml(html));
            }
        }

        private string EnhanceRtfFormatting(string baseRtf, string originalHtml)
        {
            var rtfBuilder = new StringBuilder();

            // Build RTF header with font and color tables
            rtfBuilder.AppendLine(@"{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang1033");

            // Font table
            rtfBuilder.AppendLine(@"{\fonttbl");
            foreach (var font in _fontTable.Values)
            {
                rtfBuilder.AppendLine(font);
            }
            rtfBuilder.AppendLine("}");

            // Color table
            rtfBuilder.AppendLine(@"{\colortbl ;");
            rtfBuilder.AppendLine(@"\red0\green0\blue0;");      // Black
            rtfBuilder.AppendLine(@"\red51\green51\blue51;");   // Text
            rtfBuilder.AppendLine(@"\red246\green248\blue250;"); // Code bg
            rtfBuilder.AppendLine(@"\red3\green102\blue214;");  // Links
            rtfBuilder.AppendLine(@"\red106\green115\blue125;"); // Quotes
            rtfBuilder.AppendLine(@"\red209\green217\blue224;"); // Borders
            rtfBuilder.AppendLine("}");

            // Document defaults
            rtfBuilder.AppendLine(@"\viewkind4\uc1\pard\sa200\sl276\slmult1\cf2\f0\fs22\lang9");

            // Process content based on HTML structure
            var content = ProcessHtmlContent(originalHtml);
            rtfBuilder.Append(content);

            rtfBuilder.AppendLine("}");

            return rtfBuilder.ToString();
        }

        private string ProcessHtmlContent(string html)
        {
            var rtf = new StringBuilder();

            // Process headers
            html = Regex.Replace(html, @"<h1[^>]*>(.*?)</h1>", m =>
            {
                return $@"\pard\sa200\sl276\slmult1\b\fs32 {EscapeRtf(StripHtml(m.Groups[1].Value))}\b0\fs22\par{Environment.NewLine}";
            }, RegexOptions.Singleline);

            html = Regex.Replace(html, @"<h2[^>]*>(.*?)</h2>", m =>
            {
                return $@"\pard\sa200\sl276\slmult1\b\fs28 {EscapeRtf(StripHtml(m.Groups[1].Value))}\b0\fs22\par{Environment.NewLine}";
            }, RegexOptions.Singleline);

            html = Regex.Replace(html, @"<h3[^>]*>(.*?)</h3>", m =>
            {
                return $@"\pard\sa200\sl276\slmult1\b\fs24 {EscapeRtf(StripHtml(m.Groups[1].Value))}\b0\fs22\par{Environment.NewLine}";
            }, RegexOptions.Singleline);

            // Process code blocks with gray background
            html = Regex.Replace(html, @"<pre[^>]*>.*?<code[^>]*>(.*?)</code>.*?</pre>", m =>
            {
                var codeContent = WebUtility.HtmlDecode(StripHtml(m.Groups[1].Value));
                return $@"\pard\sa200\sl276\slmult1\cbpat3\f1\fs20 {EscapeRtf(codeContent)}\f0\fs22\cbpat0\par{Environment.NewLine}";
            }, RegexOptions.Singleline);

            // Process inline code
            html = Regex.Replace(html, @"<code[^>]*>(.*?)</code>", m =>
            {
                var codeContent = WebUtility.HtmlDecode(StripHtml(m.Groups[1].Value));
                return $@"\cbpat3\f1 {EscapeRtf(codeContent)}\f0\cbpat0 ";
            }, RegexOptions.Singleline);

            // Process bold
            html = Regex.Replace(html, @"<strong[^>]*>(.*?)</strong>", m =>
            {
                return $@"\b {EscapeRtf(StripHtml(m.Groups[1].Value))}\b0 ";
            }, RegexOptions.Singleline);

            html = Regex.Replace(html, @"<b[^>]*>(.*?)</b>", m =>
            {
                return $@"\b {EscapeRtf(StripHtml(m.Groups[1].Value))}\b0 ";
            }, RegexOptions.Singleline);

            // Process italic
            html = Regex.Replace(html, @"<em[^>]*>(.*?)</em>", m =>
            {
                return $@"\i {EscapeRtf(StripHtml(m.Groups[1].Value))}\i0 ";
            }, RegexOptions.Singleline);

            html = Regex.Replace(html, @"<i[^>]*>(.*?)</i>", m =>
            {
                return $@"\i {EscapeRtf(StripHtml(m.Groups[1].Value))}\i0 ";
            }, RegexOptions.Singleline);

            // Process blockquotes
            html = Regex.Replace(html, @"<blockquote[^>]*>(.*?)</blockquote>", m =>
            {
                return $@"\pard\li720\sa200\sl276\slmult1\cf5 {EscapeRtf(StripHtml(m.Groups[1].Value))}\cf2\par{Environment.NewLine}";
            }, RegexOptions.Singleline);

            // Process links
            html = Regex.Replace(html, @"<a[^>]*href=[""']([^""']*)[""'][^>]*>(.*?)</a>", m =>
            {
                var linkText = StripHtml(m.Groups[2].Value);
                var url = m.Groups[1].Value;
                return $@"{{\field{{\*\fldinst{{HYPERLINK ""{url}""}}}}{{\fldrslt{{\ul\cf4 {EscapeRtf(linkText)}\cf2\ulnone }}}}}} ";
            }, RegexOptions.Singleline);

            // Process paragraphs
            html = Regex.Replace(html, @"<p[^>]*>(.*?)</p>", m =>
            {
                return $@"\pard\sa200\sl276\slmult1 {m.Groups[1].Value}\par{Environment.NewLine}";
            }, RegexOptions.Singleline);

            // Process line breaks
            html = html.Replace("<br>", @"\line ");
            html = html.Replace("<br/>", @"\line ");
            html = html.Replace("<br />", @"\line ");

            // Process lists
            html = ProcessLists(html);

            // Clean up remaining HTML
            rtf.Append(StripHtml(html));

            return rtf.ToString();
        }

        private string ProcessLists(string html)
        {
            // Process unordered lists
            html = Regex.Replace(html, @"<ul[^>]*>(.*?)</ul>", m =>
            {
                var listContent = m.Groups[1].Value;
                var items = Regex.Matches(listContent, @"<li[^>]*>(.*?)</li>");
                var rtfList = new StringBuilder();

                foreach (Match item in items)
                {
                    rtfList.AppendLine($@"\pard\li360\sa100\sl276\slmult1\bullet\tab {StripHtml(item.Groups[1].Value)}\par");
                }

                return rtfList.ToString();
            }, RegexOptions.Singleline);

            // Process ordered lists
            html = Regex.Replace(html, @"<ol[^>]*>(.*?)</ol>", m =>
            {
                var listContent = m.Groups[1].Value;
                var items = Regex.Matches(listContent, @"<li[^>]*>(.*?)</li>");
                var rtfList = new StringBuilder();
                int index = 1;

                foreach (Match item in items)
                {
                    rtfList.AppendLine($@"\pard\li360\sa100\sl276\slmult1 {index}.\tab {StripHtml(item.Groups[1].Value)}\par");
                    index++;
                }

                return rtfList.ToString();
            }, RegexOptions.Singleline);

            return html;
        }

        private string ConvertPlainTextToRtf(string text)
        {
            var rtf = new StringBuilder();
            rtf.AppendLine(@"{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang1033{\fonttbl{\f0\fnil\fcharset0 Segoe UI;}}");
            rtf.AppendLine(@"{\colortbl ;\red0\green0\blue0;}");
            rtf.AppendLine(@"\viewkind4\uc1\pard\sa200\sl276\slmult1\cf1\f0\fs22\lang9 ");
            rtf.Append(EscapeRtf(text));
            rtf.AppendLine(@"\par}");
            return rtf.ToString();
        }

        private string EscapeRtf(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text
                .Replace(@"\", @"\\")
                .Replace("{", @"\{")
                .Replace("}", @"\}")
                .Replace("\r\n", @"\par" + Environment.NewLine)
                .Replace("\n", @"\par" + Environment.NewLine)
                .Replace("\r", @"\par" + Environment.NewLine)
                .Replace("\t", @"\tab ");
        }

        private string StripHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            // Decode HTML entities
            html = WebUtility.HtmlDecode(html);

            // Remove HTML tags
            html = Regex.Replace(html, @"<[^>]+>", string.Empty);

            // Clean up whitespace
            html = Regex.Replace(html, @"\s+", " ");

            return html.Trim();
        }
    }
}