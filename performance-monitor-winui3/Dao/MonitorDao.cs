using LiteDB;
using performance_monitor_winui3.Models;

namespace performance_monitor_winui3.Dao;
static class MonitorDao
{
    /* Begin: SelectedTypes */
    // tips: SelectedTypes just use file to store.
    private static readonly string SelectedTypesPath = "SelectedTypes.json";
    public static List<MonitorInfoType> GetSelectedTypes()
    {
        List<MonitorInfoType>? result = null;
        if (File.Exists(SelectedTypesPath))
        {
            try
            {
                var jsonFromFile = File.ReadAllText(SelectedTypesPath);
                result = System.Text.Json.JsonSerializer.Deserialize<List<MonitorInfoType>>(jsonFromFile);
            } catch { }
        }
        return result ?? ([]);
    }
    public static void SaveSelectedTypes(List<MonitorInfoType>types)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(types);
        File.WriteAllText(SelectedTypesPath, json);
    }
    /* End: SelectedTypes */
}
