using CommunityToolkit.Mvvm.ComponentModel;
using performance_monitor_winui3.Dao;
using performance_monitor_winui3.Models;
namespace performance_monitor_winui3.ViewModels;

public partial class ScheduleViewModel : ObservableRecipient
{
    public ScheduleViewModel()
    {

    }

    /* Begin: ScheduleItem */
    public static List<ScheduleItem> GetAllScheduleItems()
    {
        return ScheduleDao.GetAllScheduleItemData();
    }

    public static List<ScheduleItem> GetScheduleItems(Models.DayOfWeek dayOfWeek)
    {
        return ScheduleDao.GetScheduleItemDataFromDayOfWeek(dayOfWeek);
    }

    public static void AddScheduleItem(ScheduleItem item)
    {
        ScheduleDao.ScheduleItemsAdd(item);
    }

    public static void DeleteScheduleItem(ScheduleItem item)
    {
        ScheduleDao.ScheduleItemsDelete(item);
    }

    public static void UpdateScheduleItem(ScheduleItem oldItem, ScheduleItem newItem)
    {
        try
        {
            newItem.Id = oldItem.Id;
            ScheduleDao.ScheduleItemsUpdate(newItem);
        } catch { }
    }
    /* End: ScheduleItem */
}
