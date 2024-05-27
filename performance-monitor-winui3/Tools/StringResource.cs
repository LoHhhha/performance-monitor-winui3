
using System.Reflection;
using System.Resources;
using Windows.ApplicationModel.Resources;


namespace performance_monitor_winui3.Tools;

public class StringResource
{
    private static readonly ResourceLoader resourceLoader = ResourceLoader.GetForViewIndependentUse();
    public static String Get(string key)
    {
        return resourceLoader.GetString(key);
    }
}
