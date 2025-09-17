using System;
using Microsoft.Win32;
using PasteMd.Core.Services;

namespace PasteMd.Core.Utilities
{
    public static class RegistryHelper
    {
        private const string SHELL_EXT_APPROVED = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved";
        private const string CONTEXT_MENU_ALL_FILES = @"*\shellex\ContextMenuHandlers\paste-md";
        private const string CONTEXT_MENU_FOLDERS = @"Folder\shellex\ContextMenuHandlers\paste-md";
        private const string CONTEXT_MENU_BACKGROUND = @"Directory\Background\shellex\ContextMenuHandlers\paste-md";

        public static void RegisterShellExtension(string clsid, string description)
        {
            try
            {
                Logger.Log($"Registering shell extension: {clsid}");

                // Add to approved shell extensions
                using (var key = Registry.LocalMachine.CreateSubKey(SHELL_EXT_APPROVED))
                {
                    if (key != null)
                    {
                        key.SetValue(clsid, description);
                        Logger.Log("Added to approved shell extensions");
                    }
                }

                // Register for all files
                using (var key = Registry.ClassesRoot.CreateSubKey(CONTEXT_MENU_ALL_FILES))
                {
                    if (key != null)
                    {
                        key.SetValue("", clsid);
                        Logger.Log("Registered for all files context menu");
                    }
                }

                // Register for folders
                using (var key = Registry.ClassesRoot.CreateSubKey(CONTEXT_MENU_FOLDERS))
                {
                    if (key != null)
                    {
                        key.SetValue("", clsid);
                        Logger.Log("Registered for folders context menu");
                    }
                }

                // Register for directory background
                using (var key = Registry.ClassesRoot.CreateSubKey(CONTEXT_MENU_BACKGROUND))
                {
                    if (key != null)
                    {
                        key.SetValue("", clsid);
                        Logger.Log("Registered for directory background context menu");
                    }
                }

                Logger.Log("Shell extension registration completed successfully");
            }
            catch (Exception ex)
            {
                Logger.Log($"Error registering shell extension: {ex.Message}");
                throw;
            }
        }

        public static void UnregisterShellExtension(string clsid)
        {
            try
            {
                Logger.Log($"Unregistering shell extension: {clsid}");

                // Remove from approved shell extensions
                using (var key = Registry.LocalMachine.OpenSubKey(SHELL_EXT_APPROVED, true))
                {
                    if (key != null)
                    {
                        key.DeleteValue(clsid, false);
                        Logger.Log("Removed from approved shell extensions");
                    }
                }

                // Remove context menu handlers
                try
                {
                    Registry.ClassesRoot.DeleteSubKeyTree(CONTEXT_MENU_ALL_FILES, false);
                    Logger.Log("Removed all files context menu");
                }
                catch { }

                try
                {
                    Registry.ClassesRoot.DeleteSubKeyTree(CONTEXT_MENU_FOLDERS, false);
                    Logger.Log("Removed folders context menu");
                }
                catch { }

                try
                {
                    Registry.ClassesRoot.DeleteSubKeyTree(CONTEXT_MENU_BACKGROUND, false);
                    Logger.Log("Removed directory background context menu");
                }
                catch { }

                Logger.Log("Shell extension unregistration completed successfully");
            }
            catch (Exception ex)
            {
                Logger.Log($"Error unregistering shell extension: {ex.Message}");
                throw;
            }
        }

        public static bool IsRegistered(string clsid)
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(SHELL_EXT_APPROVED, false))
                {
                    if (key != null)
                    {
                        var value = key.GetValue(clsid);
                        return value != null;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static void RestartExplorer()
        {
            try
            {
                Logger.Log("Restarting Windows Explorer to apply changes");

                // Kill explorer
                foreach (var process in System.Diagnostics.Process.GetProcessesByName("explorer"))
                {
                    process.Kill();
                    process.WaitForExit();
                }

                // Restart explorer
                System.Diagnostics.Process.Start("explorer.exe");
                Logger.Log("Windows Explorer restarted successfully");
            }
            catch (Exception ex)
            {
                Logger.Log($"Error restarting Explorer: {ex.Message}");
            }
        }
    }
}