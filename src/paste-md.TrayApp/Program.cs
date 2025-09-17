using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using PasteMd.Core.Services;
using PasteMd.Core.Formatters;

namespace PasteMd.TrayApp
{
    public class Program
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID = 9001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint VK_V = 0x56; // V key

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TrayApplicationContext());
        }
    }

    public class TrayApplicationContext : ApplicationContext
    {
        private NotifyIcon trayIcon;
        private HotkeyWindow hotkeyWindow;

        public TrayApplicationContext()
        {
            // Create hidden window to receive hotkey messages
            hotkeyWindow = new HotkeyWindow();
            hotkeyWindow.HotkeyPressed += OnHotkeyPressed;

            // Initialize Tray Icon
            trayIcon = new NotifyIcon()
            {
                Icon = SystemIcons.Application,
                Text = "paste-md - Press Ctrl+Shift+V to paste rendered Markdown",
                Visible = true,
                ContextMenuStrip = new ContextMenuStrip()
            };

            // Add menu items
            trayIcon.ContextMenuStrip.Items.Add("Paste Rendered Markdown", null, (s, e) => PasteRenderedMarkdown());
            trayIcon.ContextMenuStrip.Items.Add("Paste Plain Markdown", null, (s, e) => PastePlainMarkdown());
            trayIcon.ContextMenuStrip.Items.Add("-");
            trayIcon.ContextMenuStrip.Items.Add("Test with Sample", null, (s, e) => TestWithSample());
            trayIcon.ContextMenuStrip.Items.Add("-");
            trayIcon.ContextMenuStrip.Items.Add("Exit", null, (s, e) => Exit());

            // Double-click to paste
            trayIcon.DoubleClick += (s, e) => PasteRenderedMarkdown();

            // Show balloon tip only once on startup
            // trayIcon.ShowBalloonTip(3000, "paste-md", "Press Ctrl+Shift+V to paste rendered Markdown", ToolTipIcon.Info);
        }

        private void OnHotkeyPressed(object sender, EventArgs e)
        {
            PasteRenderedMarkdown();
        }

        private string lastMarkdown = string.Empty;

        private void PasteRenderedMarkdown()
        {
            try
            {
                string clipboardText = ClipboardService.GetClipboardText();

                // Check if we have Markdown (either new or cached)
                if (!ClipboardService.ContainsMarkdown(clipboardText))
                {
                    // Try to use last known Markdown if available
                    if (!string.IsNullOrEmpty(lastMarkdown) && ClipboardService.ContainsMarkdown(lastMarkdown))
                    {
                        clipboardText = lastMarkdown;
                        // Silent - no notification for cached use
                    }
                    else
                    {
                        // Only show warning if truly no Markdown available
                        trayIcon.ShowBalloonTip(2000, "paste-md", "No Markdown found. Copy Markdown text first.", ToolTipIcon.Warning);
                        return;
                    }
                }
                else
                {
                    // Cache the Markdown for repeated use
                    lastMarkdown = clipboardText;
                }

                // Convert Markdown to HTML
                var processor = new MarkdownProcessor();
                var html = processor.ConvertToHtml(clipboardText);

                // Convert HTML to RTF
                var rtfFormatter = new RtfFormatter();
                var rtf = rtfFormatter.ConvertHtmlToRtf(html);

                // Get plain text version
                var plainText = processor.ExtractPlainText(clipboardText);

                // Save current clipboard content
                var originalClipboard = Clipboard.GetDataObject();

                // Temporarily set clipboard with formatted data
                ClipboardService.SetClipboardData(html, rtf, plainText);

                // Paste using Ctrl+V
                SendKeys.SendWait("^v");

                // Immediately restore original Markdown to clipboard
                System.Threading.Tasks.Task.Run(async () =>
                {
                    await System.Threading.Tasks.Task.Delay(50); // Shorter delay
                    try
                    {
                        // Restore the original Markdown text
                        Clipboard.SetText(lastMarkdown);
                    }
                    catch
                    {
                        // If restoration fails, just continue
                    }
                });

                // Silent - no notification after successful paste
            }
            catch (Exception ex)
            {
                Logger.Log($"Error pasting: {ex.Message}");
                trayIcon.ShowBalloonTip(3000, "paste-md Error", ex.Message, ToolTipIcon.Error);
            }
        }

        private void PastePlainMarkdown()
        {
            try
            {
                string clipboardText = ClipboardService.GetClipboardText();
                Clipboard.SetText(clipboardText);
                SendKeys.SendWait("^v");
                // Silent - no notification after paste
            }
            catch (Exception ex)
            {
                Logger.Log($"Error pasting plain: {ex.Message}");
                trayIcon.ShowBalloonTip(3000, "paste-md Error", ex.Message, ToolTipIcon.Error);
            }
        }

        private void TestWithSample()
        {
            string sampleMarkdown = @"# Test paste-md

This is **bold** and *italic* text.

## Code Block (should have gray background)

```python
def hello_world():
    print(""Hello from paste-md!"")
    return True
```

And some `inline code` too.

- Bullet 1
- Bullet 2

1. Number 1
2. Number 2";

            Clipboard.SetText(sampleMarkdown);
            lastMarkdown = sampleMarkdown; // Cache it for repeated use
            trayIcon.ShowBalloonTip(1500, "paste-md", "Sample ready - press Ctrl+Shift+V", ToolTipIcon.Info);
        }

        private void Exit()
        {
            hotkeyWindow?.Dispose();
            trayIcon.Visible = false;
            Application.Exit();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                trayIcon?.Dispose();
                hotkeyWindow?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    public class HotkeyWindow : NativeWindow, IDisposable
    {
        private const int WM_HOTKEY = 0x0312;
        private const int HOTKEY_ID = 9001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint VK_V = 0x56;

        public event EventHandler HotkeyPressed;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public HotkeyWindow()
        {
            CreateHandle(new CreateParams());
            RegisterHotKey(Handle, HOTKEY_ID, MOD_CONTROL | MOD_SHIFT, VK_V);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                HotkeyPressed?.Invoke(this, EventArgs.Empty);
            }
            base.WndProc(ref m);
        }

        public void Dispose()
        {
            UnregisterHotKey(Handle, HOTKEY_ID);
            DestroyHandle();
        }
    }
}