using System.Runtime.InteropServices;

namespace Kiseki.Launcher.Windows
{
    public static class Win32
    {
        // Ref: https://learn.microsoft.com/en-us/windows/win32/msi/error-codes
        // Ref: https://i-logic.com/serial/errorcodes.htm
        public enum ErrorCode
        {
            ERROR_SUCCESS = 0,
            ERROR_INSTALL_USEREXIT = 1602,
            ERROR_INSTALL_FAILURE = 1603,
            ERROR_CANCELLED = 1223,
            ERROR_INTERNAL_ERROR = 1359
        }

        // Source: https://www.codeproject.com/Articles/878605/Getting-All-Special-Folders-in-NET
        public static string GetDownloadsPath()
        {
            return SHGetKnownFolderPath(new("374DE290-123F-4565-9164-39C4925E467B"), 0);
        }

        [DllImport("shell32", CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = false)]
        private static extern string SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, nint hToken = default);
    }
}