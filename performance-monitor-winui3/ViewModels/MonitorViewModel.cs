using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI;
using performance_monitor_winui3.Dao;
using performance_monitor_winui3.Models;

namespace performance_monitor_winui3.ViewModels;

public partial class MonitorViewModel : ObservableRecipient
{
    public static List<MonitorInfoType> _selectedTypes;

    public static void SetSelectedTypes(List<MonitorInfoType> types)
    {
        _selectedTypes = types;
        MonitorDao.SaveSelectedTypes(types);
    }

    static MonitorViewModel()
    {
        _selectedTypes = MonitorDao.GetSelectedTypes();
    }
}
