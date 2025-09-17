using System;
using System.Net;
using System.Text;
using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Extensions.TaskLists;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Markdig.Renderers.Html;

namespace PasteMd.Core.Services
{
    public class MarkdownProcessor
    {
        private readonly MarkdownPipeline _pipeline;

        public MarkdownProcessor()
        {
            _pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseEmojiAndSmiley()
                .UseTaskLists()
                .UsePipeTables()
                .UseGridTables()
                .UseAutoLinks()
                .Build();
        }

        public string ConvertToHtml(string markdown)
        {
            try
            {
                var html = Markdown.ToHtml(markdown, _pipeline);

                // Wrap in proper HTML structure with styling
                var styledHtml = WrapWithStyling(html);

                Logger.Log($"Converted {markdown.Length} chars of Markdown to HTML");
                return styledHtml;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error converting Markdown to HTML: {ex.Message}");
                return $"<p>{WebUtility.HtmlEncode(markdown)}</p>";
            }
        }

        private string WrapWithStyling(string html)
        {
            var cssStyles = @"
                <style>
                    body {
                        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                        font-size: 14px;
                        line-height: 1.6;
                        color: #333;
                    }
                    h1 { font-size: 28px; font-weight: 600; margin: 16px 0 8px 0; }
                    h2 { font-size: 24px; font-weight: 600; margin: 14px 0 7px 0; }
                    h3 { font-size: 20px; font-weight: 600; margin: 12px 0 6px 0; }
                    h4 { font-size: 16px; font-weight: 600; margin: 10px 0 5px 0; }
                    h5 { font-size: 14px; font-weight: 600; margin: 8px 0 4px 0; }
                    h6 { font-size: 12px; font-weight: 600; margin: 6px 0 3px 0; }

                    /* Code blocks - GitHub style */
                    pre {
                        background-color: #f6f8fa;
                        border: 1px solid #d1d9e0;
                        border-radius: 6px;
                        padding: 16px;
                        overflow-x: auto;
                        margin: 10px 0;
                    }

                    code {
                        font-family: 'Consolas', 'Courier New', monospace;
                        font-size: 85%;
                    }

                    /* Inline code */
                    p code, li code {
                        background-color: #f3f4f6;
                        padding: 2px 4px;
                        border-radius: 3px;
                        font-size: 85%;
                    }

                    /* Code blocks code (no additional background) */
                    pre code {
                        background-color: transparent;
                        padding: 0;
                        border-radius: 0;
                        font-size: 13px;
                    }

                    blockquote {
                        border-left: 4px solid #dfe2e5;
                        margin: 16px 0;
                        padding: 0 16px;
                        color: #6a737d;
                    }

                    table {
                        border-collapse: collapse;
                        width: 100%;
                        margin: 16px 0;
                    }

                    th, td {
                        border: 1px solid #dfe2e5;
                        padding: 6px 13px;
                    }

                    th {
                        background-color: #f6f8fa;
                        font-weight: 600;
                    }

                    tr:nth-child(even) {
                        background-color: #f6f8fa;
                    }

                    ul, ol {
                        margin: 8px 0;
                        padding-left: 24px;
                    }

                    li {
                        margin: 4px 0;
                    }

                    a {
                        color: #0366d6;
                        text-decoration: none;
                    }

                    a:hover {
                        text-decoration: underline;
                    }

                    hr {
                        border: none;
                        border-top: 2px solid #e1e4e8;
                        margin: 24px 0;
                    }

                    /* Task lists */
                    .task-list-item {
                        list-style: none;
                        margin-left: -20px;
                    }

                    .task-list-item input {
                        margin-right: 8px;
                    }
                </style>";

            return $"{cssStyles}<div class='markdown-body'>{html}</div>";
        }

        public string ExtractPlainText(string markdown)
        {
            try
            {
                var document = Markdown.Parse(markdown, _pipeline);
                var plainTextBuilder = new StringBuilder();
                ExtractTextFromDocument(document, plainTextBuilder);
                return plainTextBuilder.ToString();
            }
            catch (Exception ex)
            {
                Logger.Log($"Error extracting plain text: {ex.Message}");
                return markdown;
            }
        }

        private void ExtractTextFromDocument(MarkdownObject obj, StringBuilder builder)
        {
            foreach (var child in obj.Descendants())
            {
                if (child is LiteralInline literal)
                {
                    builder.Append(literal.Content);
                }
                else if (child is LineBreakInline)
                {
                    builder.AppendLine();
                }
                else if (child is ParagraphBlock)
                {
                    builder.AppendLine();
                }
            }
        }
    }
}