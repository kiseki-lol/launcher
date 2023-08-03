namespace Kiseki.Launcher.Models;

public readonly struct WebResponse
{
    public WebResponse(int status, object? data)
    {
        Status = status;
        Data = data;
    }

    public int Status { get; }
    public object? Data { get; }
}