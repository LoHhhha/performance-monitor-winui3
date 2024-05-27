using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using HardwareStruct;
namespace performance_monitor_winui3.ViewModels;

public partial class NetworkViewModel : ObservableRecipient
{
    public static uint TimerIntervalDefaultValue = 2000;
    public static string TimerIntervalKey = "NetworkTimerInterval";

    public NetworkViewModel()
    {
    
    }
}
