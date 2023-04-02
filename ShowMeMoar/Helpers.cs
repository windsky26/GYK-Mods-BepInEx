namespace ShowMeMoar;

public static class Helpers
{
    internal static void Log(string message, bool error = false)
    {
        if (error)
        {
            Plugin.Log.LogError($"{message}");
        }
        else
        {
            Plugin.Log.LogInfo($"{message}");
        }
    }
}