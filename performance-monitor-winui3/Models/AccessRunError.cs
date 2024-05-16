namespace performance_monitor_winui3.Models;

public class AccessRunError(string taskName, string error)
{
    public string TaskName = taskName;
    public string Error = error;
    public DateTime Time = DateTime.Now;
    public string Detail => $"[{Time:yyyy-MM-dd HH:mm:ss}]" + Error;
}