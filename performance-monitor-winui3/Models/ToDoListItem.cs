using LiteDB;

namespace performance_monitor_winui3.Models;

public class ToDoListItem(string taskName,string taskDetail, DateTime deadline)
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
    public string TaskDetail
    {
        get; set;
    } = taskDetail;
    public DateTime CreateTime
    {
        get; set;
    } = DateTime.Now;
    public DateTime Deadline
    {
        get; set;
    } = deadline;

    public string DeadlineFormat => Deadline.ToString("yyyy-MM-dd HH:mm");
}