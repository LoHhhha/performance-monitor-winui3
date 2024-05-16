namespace performance_monitor_winui3.Models;

public class MonitorOrderListItem(string text, int index)
{
    public string Text
    {
        get; set;
    } = text;
    public int Index
    {
        get; set;
    } = index;
}
