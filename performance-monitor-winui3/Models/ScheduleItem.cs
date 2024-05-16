using LiteDB;

namespace performance_monitor_winui3.Models;

public enum DayOfWeek
{
    Mon=0,
    Tue=1,
    Wed=2,
    Thu=3,
    Fri=4,
    Sat=5,
    Sun=6,
    All=7
}

public class ScheduleItem
{
    [BsonId]
    public int Id
    {
        get; set;
    }
    public string ItemName
    {
        get; set;
    }
    public string ItemDetail
    {
        get; set;
    }
    public TimeSpan BeginTime
    {
        get; set;
    }
    public TimeSpan EndTime
    {
        get; set;
    }
    public DayOfWeek Day
    {
        get; set;
    }

    public ScheduleItem(string itemName, string detail, TimeSpan beginTime, TimeSpan endTime, DayOfWeek day = DayOfWeek.Mon)
    {
        ItemName=itemName;
        ItemDetail=detail;
        BeginTime=beginTime;
        EndTime=endTime;
        Day=day;
    }

    public ScheduleItem()
    {
        ItemName = "";
        ItemDetail = "";
        BeginTime = TimeSpan.Zero;
        EndTime = TimeSpan.Zero;
        Day = DayOfWeek.Mon;
    }

    public string TimeFormat => $"{BeginTime.Hours:D2}:{BeginTime.Minutes:D2}-{EndTime.Hours:D2}:{EndTime.Minutes:D2}";
}
