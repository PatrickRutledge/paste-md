using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using PasteMd.Core.Interfaces;
using PasteMd.Core.Services;

namespace PasteMd.Core.Extensions
{
    [ComVisible(true)]
    [Guid("B8C4B8A1-7E4D-4F8A-9C3B-1234567890AB")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("PasteMd.ContextMenuExtension")]
    public class PasteMdContextMenu : IContextMenu, IShellExtInit
    {
        private const int CMD_PASTE_RENDERED = 0;
        private const int CMD_PASTE_PLAIN = 1;

        private IntPtr _menuBitmap = IntPtr.Zero;
        private bool _hasMarkdown = false;
        private string _clipboardText = string.Empty;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr InsertMenu(IntPtr hMenu, uint uPosition, uint uFlags, uint uIDNewItem, string lpNewItem);

        [DllImport("user32.dll")]
        private static extern IntPtr CreatePopupMenu();

        [DllImport("user32.dll")]
        private static extern bool DestroyMenu(IntPtr hMenu);

        [DllImport("user32.dll")]
        private static extern int GetMenuItemCount(IntPtr hMenu);

        public PasteMdContextMenu()
        {
            Logger.Log("PasteMdContextMenu constructor called");
        }

        #region IShellExtInit Implementation

        public int Initialize(IntPtr pidlFolder, IntPtr pDataObj, IntPtr hKeyProgID)
        {
            try
            {
                Logger.Log("Initialize called");

                // Check clipboard for Markdown content
                _clipboardText = ClipboardService.GetClipboardText();
                _hasMarkdown = ClipboardService.ContainsMarkdown(_clipboardText);

                Logger.Log($"Markdown detection result: {_hasMarkdown}");
                return 0; // S_OK
            }
            catch (Exception ex)
            {
                Logger.Log($"Initialize error: {ex.Message}");
                return -2147467259; // E_FAIL
            }
        }

        #endregion

        #region IContextMenu Implementation

        public int QueryContextMenu(IntPtr hmenu, uint indexMenu, uint idCmdFirst, uint idCmdLast, uint uFlags)
        {
            try
            {
                Logger.Log($"QueryContextMenu called. Has Markdown: {_hasMarkdown}");

                // Only add menu items if we detected Markdown
                if (!_hasMarkdown)
                {
                    return 0; // No items added
                }

                // Check if we're in a text input context (not file explorer)
                if ((uFlags & 0x0000000F) == 0x0000000F) // CMF_DEFAULTONLY
                {
                    return 0;
                }

                uint itemsAdded = 0;

                // Add separator first for visual separation
                InsertMenu(hmenu, indexMenu++, MenuFlags.MF_SEPARATOR | MenuFlags.MF_BYPOSITION, 0, string.Empty);

                // Add "Paste as Rendered Markdown" option
                InsertMenu(hmenu, indexMenu++, MenuFlags.MF_STRING | MenuFlags.MF_BYPOSITION,
                    idCmdFirst + CMD_PASTE_RENDERED, "Paste as Rendered Markdown");
                itemsAdded++;

                // Add "Paste as Plain Markdown" option
                InsertMenu(hmenu, indexMenu++, MenuFlags.MF_STRING | MenuFlags.MF_BYPOSITION,
                    idCmdFirst + CMD_PASTE_PLAIN, "Paste as Plain Markdown");
                itemsAdded++;

                // Add another separator after our items
                InsertMenu(hmenu, indexMenu, MenuFlags.MF_SEPARATOR | MenuFlags.MF_BYPOSITION, 0, string.Empty);

                Logger.Log($"Added {itemsAdded} menu items");
                return (int)itemsAdded;
            }
            catch (Exception ex)
            {
                Logger.Log($"QueryContextMenu error: {ex.Message}");
                return 0;
            }
        }

        public int InvokeCommand(IntPtr pici)
        {
            try
            {
                var commandInfo = Marshal.PtrToStructure<CMINVOKECOMMANDINFO>(pici);

                // Check if lpVerb is a string or command ID
                if (commandInfo.lpVerb.ToInt64() > 1)
                {
                    // It's a string verb, not supported
                    return -2147467259; // E_FAIL
                }

                int commandId = commandInfo.lpVerb.ToInt32();
                Logger.Log($"InvokeCommand called with command ID: {commandId}");

                switch (commandId)
                {
                    case CMD_PASTE_RENDERED:
                        return ExecutePasteRendered();

                    case CMD_PASTE_PLAIN:
                        return ExecutePastePlain();

                    default:
                        return -2147467259; // E_FAIL
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"InvokeCommand error: {ex.Message}");
                return -2147467259; // E_FAIL
            }
        }

        public int GetCommandString(uint idCmd, uint uType, IntPtr pReserved, StringBuilder pszName, uint cchMax)
        {
            try
            {
                string helpText = string.Empty;

                switch (idCmd)
                {
                    case CMD_PASTE_RENDERED:
                        helpText = "Paste Markdown content as formatted rich text";
                        break;

                    case CMD_PASTE_PLAIN:
                        helpText = "Paste Markdown content with original syntax preserved";
                        break;

                    default:
                        return -2147467259; // E_FAIL
                }

                if (uType == GCS.GCS_HELPTEXTW)
                {
                    if (pszName != null && cchMax > 0)
                    {
                        pszName.Clear();
                        pszName.Append(helpText);
                    }
                    return 0; // S_OK
                }

                return -2147467259; // E_FAIL
            }
            catch (Exception ex)
            {
                Logger.Log($"GetCommandString error: {ex.Message}");
                return -2147467259; // E_FAIL
            }
        }

        #endregion

        #region Command Execution

        private int ExecutePasteRendered()
        {
            try
            {
                Logger.Log("Executing Paste as Rendered Markdown");

                if (string.IsNullOrEmpty(_clipboardText))
                {
                    _clipboardText = ClipboardService.GetClipboardText();
                }

                // Convert Markdown to HTML
                var processor = new MarkdownProcessor();
                var html = processor.ConvertToHtml(_clipboardText);

                // Convert HTML to RTF
                var rtfFormatter = new Formatters.RtfFormatter();
                var rtf = rtfFormatter.ConvertHtmlToRtf(html);

                // Get plain text version as fallback
                var plainText = processor.ExtractPlainText(_clipboardText);

                // Set clipboard with multiple formats
                ClipboardService.SetClipboardData(html, rtf, plainText);

                // Simulate paste
                SendKeys.SendWait("^v");

                Logger.Log("Paste as Rendered Markdown completed successfully");
                return 0; // S_OK
            }
            catch (Exception ex)
            {
                Logger.Log($"ExecutePasteRendered error: {ex.Message}");
                MessageBox.Show($"Failed to paste rendered Markdown: {ex.Message}",
                    "paste-md Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -2147467259; // E_FAIL
            }
        }

        private int ExecutePastePlain()
        {
            try
            {
                Logger.Log("Executing Paste as Plain Markdown");

                if (string.IsNullOrEmpty(_clipboardText))
                {
                    _clipboardText = ClipboardService.GetClipboardText();
                }

                // Just set the original Markdown text
                Clipboard.SetText(_clipboardText);

                // Simulate paste
                SendKeys.SendWait("^v");

                Logger.Log("Paste as Plain Markdown completed successfully");
                return 0; // S_OK
            }
            catch (Exception ex)
            {
                Logger.Log($"ExecutePastePlain error: {ex.Message}");
                MessageBox.Show($"Failed to paste plain Markdown: {ex.Message}",
                    "paste-md Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -2147467259; // E_FAIL
            }
        }

        #endregion

        #region COM Registration

        [ComRegisterFunction]
        public static void Register(Type t)
        {
            try
            {
                Logger.Log("Registering paste-md shell extension");

                string guid = t.GUID.ToString("B");

                // Register as approved shell extension
                using (var key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved"))
                {
                    key?.SetValue(guid, "paste-md Context Menu Extension");
                }

                // Register context menu handler for all files
                using (var key = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(
                    @"*\shellex\ContextMenuHandlers\paste-md"))
                {
                    key?.SetValue("", guid);
                }

                // Register for folders
                using (var key = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(
                    @"Folder\shellex\ContextMenuHandlers\paste-md"))
                {
                    key?.SetValue("", guid);
                }

                // Register for directory background
                using (var key = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(
                    @"Directory\Background\shellex\ContextMenuHandlers\paste-md"))
                {
                    key?.SetValue("", guid);
                }

                Logger.Log("Registration completed successfully");
            }
            catch (Exception ex)
            {
                Logger.Log($"Registration error: {ex.Message}");
                throw;
            }
        }

        [ComUnregisterFunction]
        public static void Unregister(Type t)
        {
            try
            {
                Logger.Log("Unregistering paste-md shell extension");

                string guid = t.GUID.ToString("B");

                // Remove from approved list
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved", true))
                {
                    key?.DeleteValue(guid, false);
                }

                // Remove context menu handlers
                Microsoft.Win32.Registry.ClassesRoot.DeleteSubKeyTree(
                    @"*\shellex\ContextMenuHandlers\paste-md", false);

                Microsoft.Win32.Registry.ClassesRoot.DeleteSubKeyTree(
                    @"Folder\shellex\ContextMenuHandlers\paste-md", false);

                Microsoft.Win32.Registry.ClassesRoot.DeleteSubKeyTree(
                    @"Directory\Background\shellex\ContextMenuHandlers\paste-md", false);

                Logger.Log("Unregistration completed successfully");
            }
            catch (Exception ex)
            {
                Logger.Log($"Unregistration error: {ex.Message}");
                throw;
            }
        }

        #endregion
    }
}