using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace PasteMd.Core.Services
{
    public class PasteDispatcher
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll")]
        private static extern IntPtr GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        public enum TargetApplication
        {
            Unknown,
            MicrosoftWord,
            OneNote,
            Outlook,
            PowerPoint,
            Excel,
            Browser,
            TextEditor,
            Other
        }

        public static TargetApplication DetectTargetApplication()
        {
            try
            {
                IntPtr foregroundWindow = GetForegroundWindow();
                if (foregroundWindow == IntPtr.Zero)
                    return TargetApplication.Unknown;

                // Get window title
                StringBuilder windowTitle = new StringBuilder(256);
                GetWindowText(foregroundWindow, windowTitle, 256);
                string title = windowTitle.ToString().ToLower();

                // Get window class
                StringBuilder className = new StringBuilder(256);
                GetClassName(foregroundWindow, className, 256);
                string windowClass = className.ToString();

                // Get process name
                uint processId;
                GetWindowThreadProcessId(foregroundWindow, out processId);
                string processName = string.Empty;

                try
                {
                    Process process = Process.GetProcessById((int)processId);
                    processName = process.ProcessName.ToLower();
                }
                catch
                {
                    // Process might have ended
                }

                Logger.Log($"Target app - Process: {processName}, Title: {title}, Class: {windowClass}");

                // Detect Microsoft Office applications
                if (processName.Contains("winword") || title.Contains("word"))
                    return TargetApplication.MicrosoftWord;

                if (processName.Contains("onenote") || title.Contains("onenote"))
                    return TargetApplication.OneNote;

                if (processName.Contains("outlook") || title.Contains("outlook"))
                    return TargetApplication.Outlook;

                if (processName.Contains("powerpnt") || title.Contains("powerpoint"))
                    return TargetApplication.PowerPoint;

                if (processName.Contains("excel"))
                    return TargetApplication.Excel;

                // Detect browsers
                if (processName.Contains("chrome") || processName.Contains("firefox") ||
                    processName.Contains("edge") || processName.Contains("msedge") ||
                    processName.Contains("iexplore") || processName.Contains("opera"))
                    return TargetApplication.Browser;

                // Detect text editors
                if (processName.Contains("notepad") || processName.Contains("code") ||
                    processName.Contains("sublime") || processName.Contains("atom"))
                    return TargetApplication.TextEditor;

                return TargetApplication.Other;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error detecting target application: {ex.Message}");
                return TargetApplication.Unknown;
            }
        }

        public static void OptimizeFormatForApplication(TargetApplication app, ref string html, ref string rtf, ref string plainText)
        {
            switch (app)
            {
                case TargetApplication.MicrosoftWord:
                    // Word handles RTF best
                    Logger.Log("Optimizing for Microsoft Word - prioritizing RTF");
                    // RTF is already well-formatted
                    break;

                case TargetApplication.OneNote:
                    // OneNote prefers HTML
                    Logger.Log("Optimizing for OneNote - prioritizing HTML");
                    // Ensure HTML has proper structure for OneNote
                    html = EnsureCompleteHtml(html);
                    break;

                case TargetApplication.Outlook:
                    // Outlook email composer needs HTML
                    Logger.Log("Optimizing for Outlook - prioritizing HTML with email-safe styles");
                    html = ConvertToEmailSafeHtml(html);
                    break;

                case TargetApplication.PowerPoint:
                    // PowerPoint needs simplified RTF
                    Logger.Log("Optimizing for PowerPoint - simplifying RTF");
                    rtf = SimplifyRtfForPowerPoint(rtf);
                    break;

                case TargetApplication.Browser:
                    // Browsers need clean HTML
                    Logger.Log("Optimizing for Browser - clean HTML");
                    html = EnsureCompleteHtml(html);
                    break;

                case TargetApplication.TextEditor:
                    // Text editors only support plain text
                    Logger.Log("Optimizing for Text Editor - plain text only");
                    // Plain text is already set
                    break;

                default:
                    Logger.Log("Using default formatting for unknown application");
                    break;
            }
        }

        private static string EnsureCompleteHtml(string html)
        {
            if (!html.StartsWith("<!DOCTYPE"))
            {
                return $@"<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'>
<style>
body {{ font-family: 'Segoe UI', Arial, sans-serif; }}
pre {{ background: #f6f8fa; padding: 10px; border-radius: 4px; }}
code {{ font-family: Consolas, monospace; background: #f3f4f6; padding: 2px 4px; }}
</style>
</head>
<body>
{html}
</body>
</html>";
            }
            return html;
        }

        private static string ConvertToEmailSafeHtml(string html)
        {
            // Inline all styles for email compatibility
            return html.Replace("<style>", "<style type='text/css'>")
                      .Replace("pre {", "pre { display: block; font-family: Consolas, monospace;")
                      .Replace("code {", "code { display: inline; font-family: Consolas, monospace;");
        }

        private static string SimplifyRtfForPowerPoint(string rtf)
        {
            // PowerPoint doesn't handle complex RTF well, simplify it
            // Remove advanced formatting that PowerPoint doesn't support
            return rtf.Replace(@"\cbpat3", "") // Remove background colors
                      .Replace(@"\highlight3", ""); // Remove highlighting
        }

        public static void ExecutePaste()
        {
            try
            {
                // Small delay to ensure clipboard is ready
                Thread.Sleep(50);

                // Send Ctrl+V
                SendKeys.SendWait("^v");

                Logger.Log("Paste command sent successfully");
            }
            catch (Exception ex)
            {
                Logger.Log($"Error executing paste: {ex.Message}");
                throw;
            }
        }

        public static bool ValidatePasteOperation()
        {
            try
            {
                // Check if we're in a valid text input context
                IntPtr focusedControl = GetForegroundWindow();
                if (focusedControl == IntPtr.Zero)
                {
                    Logger.Log("No focused window found");
                    return false;
                }

                // Additional validation could be added here
                // For example, checking if the control accepts text input

                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error validating paste operation: {ex.Message}");
                return false;
            }
        }
    }
}