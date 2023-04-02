using System.Threading;
using GYKHelper;

namespace PrayTheDayAway;

public partial class Plugin
{
    internal static string GetLocalizedString(string content)
    {
        Thread.CurrentThread.CurrentUICulture = CrossModFields.Culture;
        return content;
    }
    internal static void WriteLog(string message, bool error = false)
    {
        if (error)
        {
            Plugin.Log.LogError($"{message}");
        }
        else
        {
            if (Plugin._debug.Value)
            {
                Plugin.Log.LogInfo($"{message}");
            }
        }
    }
}