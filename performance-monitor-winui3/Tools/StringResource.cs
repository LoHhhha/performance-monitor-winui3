
using System.Reflection;
using System.Resources;


namespace performance_monitor_winui3.Tools;

public class StringResource
{
    public static String Get(string key)
    {
        return Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse().GetString(key);
    }
}
