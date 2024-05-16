using System.Drawing;
using LiteDB;

namespace performance_monitor_winui3.Models;


public class FinishListItem(ToDoListItem taskItem)
{
    [BsonId]
    public int Id
    {
        get; set;
    }
    public ToDoListItem TaskItem
    {
        get; set;
    } = taskItem;
    public DateTime FinishTime
    {
        get; set;
    } = DateTime.Now;

    public string FinishTimeFormat=> FinishTime.ToString("yyyy-MM-dd HH:mm");
}