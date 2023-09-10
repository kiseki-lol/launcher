namespace Kiseki.Launcher;

public static class Constants
{
    public const string PROJECT_NAME = "Kiseki";
    public const string PROJECT_REPOSITORY = "kiseki-lol/launcher";

#if DEBUG
    public const string BASE_URL = "kiseki.loc";
#else
    public const string BASE_URL = "kiseki.lol";
#endif

    public const string MAINTENANCE_DOMAIN = "test";

    public const string PROTOCOL_KEY = "kiseki";
}