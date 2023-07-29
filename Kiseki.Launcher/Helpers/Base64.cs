using System.Text;

namespace Kiseki.Launcher.Helpers
{
    public static class Base64
    {
        // https://stackoverflow.com/a/54143400
        public static bool IsBase64String(string base64)
        {
            Span<byte> buffer = new(new byte[base64.Length]);
            
            return Convert.TryFromBase64String(base64, buffer, out _);
        }

        public static string ConvertBase64ToString(string base64)
        {
            Span<byte> buffer = new(new byte[base64.Length]);
            Convert.TryFromBase64String(base64, buffer, out _);

            return Encoding.UTF8.GetString(buffer);
        }
    }
}