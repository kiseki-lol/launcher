using System.Runtime.InteropServices;

namespace Kiseki.Launcher.Windows
{
    public static class Win32
    {
        [DllImport("shell32", CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = false)]
        private static extern string SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, nint hToken = default);

        // https://www.codeproject.com/Articles/878605/Getting-All-Special-Folders-in-NET
        public static string GetDownloadsPath()
        {
            return SHGetKnownFolderPath(new("374DE290-123F-4565-9164-39C4925E467B"), 0);
        }
    }
}