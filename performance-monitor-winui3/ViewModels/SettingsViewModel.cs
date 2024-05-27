using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

using performance_monitor_winui3.Contracts.Services;
using performance_monitor_winui3.Helpers;
using performance_monitor_winui3.Models;
using Windows.ApplicationModel;
using Windows.Storage;


namespace performance_monitor_winui3.ViewModels;


public partial class SettingsViewModel : ObservableRecipient
{
    private readonly IThemeSelectorService _themeSelectorService;

    [ObservableProperty]
    private ElementTheme _elementTheme;

    [ObservableProperty]
    private string _versionDescription;

    [ObservableProperty]
    private string _appDisplayName;

    [ObservableProperty]
    private string _appDescription;



    /* begin: code for Monitor order setting */
    // using ObservableCollection can auto redraw the windows
    private static ObservableCollection<MonitorOrderListItem> _MonitorItems = [];
    public  ObservableCollection<MonitorOrderListItem> MonitorItems
    {
        get => _MonitorItems;
        set => _MonitorItems = value;
    }

    public void MonitorOrderAddItem(string text)
    {
        var newItem = new MonitorOrderListItem(text, MonitorItems.Count);
        MonitorItems.Add(newItem);
    }

    public void MonitorOrderMoveItemUp(MonitorOrderListItem item)
    {
        var currentIndex = item.Index;
        if (currentIndex > 0)
        {
            var previousItem = MonitorItems[currentIndex - 1];
            MonitorItems.Move(currentIndex, currentIndex - 1);
            item.Index--;
            previousItem.Index++;
        }
    }

    public void MonitorOrderMoveItemDown(MonitorOrderListItem item)
    {
        var currentIndex = item.Index;
        if (currentIndex < MonitorItems.Count - 1)
        {
            var nextItem = MonitorItems[currentIndex + 1];
            MonitorItems.Move(currentIndex, currentIndex + 1);
            item.Index++;
            nextItem.Index--;
        }
    }

    public void MonitorOrderDeleteItem(MonitorOrderListItem item)
    {
        MonitorItems.Remove(item);
    }
    /* end: code for Monitor order setting */

    /* begin: code fot timer */
    public static uint MonitorTimerIntervalChange(uint value)
    {
        try
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values[MonitorViewModel.TimerIntervalKey] = value;
            return value;
        }
        catch { }
        return MonitorViewModel.TimerIntervalDefaultValue;
    }

    public static uint NetworkTimerIntervalChange(uint value)
    {
        try
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values[NetworkViewModel.TimerIntervalKey] = value;
            return value;
        }
        catch { }
        return NetworkViewModel.TimerIntervalDefaultValue;
    }
    /* end: code fot timer */

    /* begin: code for theme setting */
    public SettingsViewModel(IThemeSelectorService themeSelectorService)
    {
        _themeSelectorService = themeSelectorService;
        _elementTheme = _themeSelectorService.Theme;
        _versionDescription = GetVersionDescription();
        _appDescription = "AppDescription".GetLocalized();
        _appDisplayName = "AppDisplayName".GetLocalized();
    }

    public async Task ChangeThemeAsync(ElementTheme element)
    {
        if (ElementTheme!=element)
        {
            ElementTheme = element;
            await _themeSelectorService.SetThemeAsync(element);
        }
    }
    /* end: code for theme setting */ 

    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }
}
