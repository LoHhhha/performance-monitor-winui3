using LiteDB;

namespace performance_monitor_winui3.Models;

public class AccessItem(string taskName,string command,bool showWindow, bool exitAfterFinish)
{
    [BsonId]
    public int Id
    {
        get; set;
    }
    public string TaskName
    {
        get; set;
    } = taskName;
    public string Command
    {
        get; set;
    } = command;
    public bool ShowWindow
    {
        get; set;
    } = showWindow;
    public bool ExitAfterFinish
    {
        get; set;
    } = exitAfterFinish;
}
