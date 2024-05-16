using AWCC;
using CommunityToolkit.Mvvm.ComponentModel;

namespace performance_monitor_winui3.ViewModels;

public partial class OverclockViewModel : ObservableRecipient
{
    public static List<ThermalMode> Index2Mode = [ThermalMode.Balanced, ThermalMode.Custom, ThermalMode.G_Mode];
    public static Dictionary<ThermalMode, int> Mode2Index = new() { { ThermalMode.Balanced, 0 }, { ThermalMode.Custom, 1 }, { ThermalMode.G_Mode, 2 } };

    public OverclockViewModel()
    {
        
    }
}
