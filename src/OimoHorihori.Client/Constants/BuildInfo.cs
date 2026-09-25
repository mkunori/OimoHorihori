namespace OimoHorihori.Constants;

public static class BuildInfo
{
#if DEBUG
    public const bool IsDebug = true;
#else
    public const bool IsDebug = false;
#endif

    public const string Version = "6.0.2";
}