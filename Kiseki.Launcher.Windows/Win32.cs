namespace Kiseki.Launcher.Windows;

public static class Win32
{
    // REF: https://learn.microsoft.com/en-us/windows/win32/msi/error-codes
    // REF: https://i-logic.com/serial/errorcodes.htm
    public enum ErrorCode
    {
        ERROR_SUCCESS = 0,
        ERROR_INSTALL_USEREXIT = 1602,
        ERROR_INSTALL_FAILURE = 1603,
        ERROR_CANCELLED = 1223,
        ERROR_INTERNAL_ERROR = 1359
    }
}