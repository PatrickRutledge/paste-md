using System;
using System.Windows.Forms;
using PasteMd.Core.Services;

namespace PasteMd.Core.Utilities
{
    public static class ErrorHandler
    {
        public static void HandleError(Exception ex, string context, bool showUser = false)
        {
            // Always log the error
            Logger.Log($"ERROR in {context}: {ex.GetType().Name} - {ex.Message}");
            Logger.Log($"Stack Trace: {ex.StackTrace}");

            // Log inner exceptions
            var innerEx = ex.InnerException;
            int depth = 1;
            while (innerEx != null && depth < 5)
            {
                Logger.Log($"Inner Exception {depth}: {innerEx.GetType().Name} - {innerEx.Message}");
                innerEx = innerEx.InnerException;
                depth++;
            }

            // Show user-friendly message if requested
            if (showUser)
            {
                ShowUserError(ex, context);
            }
        }

        private static void ShowUserError(Exception ex, string context)
        {
            string userMessage;

            // Provide specific messages for common errors
            if (ex is UnauthorizedAccessException)
            {
                userMessage = "Access denied. Please run as Administrator or check file permissions.";
            }
            else if (ex is System.IO.IOException)
            {
                userMessage = "File operation failed. The file may be in use by another program.";
            }
            else if (ex is OutOfMemoryException)
            {
                userMessage = "The Markdown content is too large to process. Please try with smaller content.";
            }
            else if (ex.Message.Contains("clipboard"))
            {
                userMessage = "Unable to access clipboard. Please try copying the content again.";
            }
            else if (ex.Message.Contains("format"))
            {
                userMessage = "Unable to convert Markdown to the required format. The target application may not support rich text.";
            }
            else
            {
                userMessage = $"An error occurred while {context}. Please check the logs for details.";
            }

            MessageBox.Show(
                userMessage + $"\n\nError details: {ex.Message}\n\nLog location: %LOCALAPPDATA%\\paste-md\\paste-md.log",
                "paste-md Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }

        public static T SafeExecute<T>(Func<T> action, string context, T defaultValue = default(T))
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                HandleError(ex, context);
                return defaultValue;
            }
        }

        public static void SafeExecute(Action action, string context, bool showUserOnError = false)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                HandleError(ex, context, showUserOnError);
            }
        }

        public static int SafeComExecute(Func<int> action, string context)
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                HandleError(ex, context);
                return -2147467259; // E_FAIL
            }
        }
    }
}