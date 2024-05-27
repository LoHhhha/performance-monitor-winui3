using Microsoft.UI.Xaml.Controls;
using performance_monitor_winui3.ViewModels;
using performance_monitor_winui3.Models;
using Microsoft.UI.Xaml;
using performance_monitor_winui3.Tools;
using Windows.Globalization.NumberFormatting;
using Windows.Storage;
using Microsoft.Windows.AppLifecycle;

namespace performance_monitor_winui3.Views;


public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel
    {
        get;
    }

    /* begin: code for Monitor order setting */
    public void MonitorOrderAddItem(String text)
    {
        ViewModel.MonitorOrderAddItem(text);
    }

    public void MonitorOrderMoveItemUp(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var item = button.DataContext as MonitorOrderListItem;
        ViewModel.MonitorOrderMoveItemUp(item);
    }

    public void MonitorOrderMoveItemDown(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var item = button.DataContext as MonitorOrderListItem;
        ViewModel.MonitorOrderMoveItemDown(item);
    }

    public void MonitorOrderDeleteItem(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var item = button.DataContext as MonitorOrderListItem;
        ViewModel.MonitorOrderDeleteItem(item);
    }

    private readonly Dictionary<String, MonitorInfoType> MonitorInfoTypeHashMap = [];
    private readonly string[] MonitorInfoTypes = Enum.GetNames(typeof(MonitorInfoType));
    public void InitMonitorOrderSettings()
    {
        // Init MonitorOrder
        ViewModel.MonitorItems.Clear();
        foreach (var type in MonitorPage.selectedTypes)
        {
            MonitorOrderAddItem(type.ToString());
        }

        // Init MonitorInfoTypeHashMap
        for(var i = 0; i < MonitorInfoTypes.Length; i++)
        {
            MonitorInfoTypeHashMap[MonitorInfoTypes[i]] = (MonitorInfoType)i;
        }
    }

    public async void MonitorOrderAddItemButton(object sender, RoutedEventArgs e)
    {
        RadioButtons radioButtons = new RadioButtons();
        foreach(var type in MonitorInfoTypes)
        {
            radioButtons.Items.Add(type);
        }
        if (MonitorInfoTypes.Length != 0)
        {
            radioButtons.SelectedIndex = 0;
        }
        
        ContentDialog dialog = new ContentDialog()
        {
            XamlRoot = this.XamlRoot,
            Title = StringResource.Get("SettingsPage_DialogMonitorOrderAdd"),
            Content = radioButtons,
            PrimaryButtonText = StringResource.Get("SettingsPage_DialogConfirm"),
            CloseButtonText = StringResource.Get("SettingsPage_DialogCancel")
        };

        var res = await dialog.ShowAsync();
        if (res != ContentDialogResult.Primary) 
        {
            return;
        }

        if (MonitorInfoTypes.Length != 0)
        {
            MonitorOrderAddItem(MonitorInfoTypes[radioButtons.SelectedIndex]);
        }
    }

    public void MonitorOrderConfirm(object sender, RoutedEventArgs e)
    {
        List<MonitorInfoType> selectedTypes=[];
        foreach (var typeName in ViewModel.MonitorItems)
        {
            selectedTypes.Add(MonitorInfoTypeHashMap[typeName.Text]);
        }
        MonitorPage.SetSelectedTypes(selectedTypes);
    }

    public void MonitorOrderClear(object sender, RoutedEventArgs e)
    {
        ViewModel.MonitorItems.Clear();
    }
    /* end: code for Monitor order setting */

    /* begin: code for time */
    private void SetNumberBoxNumber()
    {
        DecimalFormatter formatter = new DecimalFormatter();
        formatter.FractionDigits = 0;

        MonitorTimerNumberBox.NumberFormatter = formatter;
        NetworkTimerNumberBox.NumberFormatter = formatter;

        try
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            MonitorTimerNumberBox.Value = (uint)localSettings.Values[MonitorViewModel.TimerIntervalKey];
            NetworkTimerNumberBox.Value = (uint)localSettings.Values[NetworkViewModel.TimerIntervalKey];
        }
        catch
        {
            MonitorTimerNumberBox.Value = MonitorViewModel.TimerIntervalDefaultValue;
            NetworkTimerNumberBox.Value = NetworkViewModel.TimerIntervalDefaultValue;
            // first time
            try
            {
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                localSettings.Values[MonitorViewModel.TimerIntervalKey] = MonitorViewModel.TimerIntervalDefaultValue;
                localSettings.Values[NetworkViewModel.TimerIntervalKey] = NetworkViewModel.TimerIntervalDefaultValue;
            }
            catch { }
        }
    }

    private void MonitorTimerNumberBoxValueChange(object sender, NumberBoxValueChangedEventArgs e)
    {
        var timerNumberBox = sender as NumberBox;
        var value = (uint)timerNumberBox.Value;
        value = Math.Min(3600000, value);
        var newValue = SettingsViewModel.MonitorTimerIntervalChange(value);
        timerNumberBox.Value = newValue;
    }

    private void NetworkTimerNumberBoxValueChange(object sender, NumberBoxValueChangedEventArgs e)
    {
        var timerNumberBox = sender as NumberBox;
        var value = (uint)timerNumberBox.Value;
        value = Math.Min(3600000, value);
        var newValue = SettingsViewModel.NetworkTimerIntervalChange(value);
        timerNumberBox.Value = newValue;
    }
    /* end: code for time */

    /* begin: code for language*/
    private void SetLanguageComboBox()
    {
        LanguageComboBox.PlaceholderText = Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride;
    }

    private void LanguageComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var language = e.AddedItems[0].ToString();
        Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = language;
        AppInstance.Restart(null);
    }
    /* end: code for language */

    /* begin: code for theme */
    private async void ThemeComboBoxSelectionChanged(object sender, RoutedEventArgs e)
    {
        var combobox = sender as ComboBox;
        switch (combobox.SelectedIndex)
        {
            case 0:
                // light
                await ViewModel.ChangeThemeAsync(ElementTheme.Light);
                break;
            case 1:
                // dark
                await ViewModel.ChangeThemeAsync(ElementTheme.Dark);
                break;
            case 2:
                // default
                await ViewModel.ChangeThemeAsync(ElementTheme.Default);
                break;
            default:
                break;
        }
        UpdateThemeComboBox();
    }

    private void UpdateThemeComboBox()
    {
        if (ViewModel != null)
        {
            switch (ViewModel.ElementTheme)
            {
                case ElementTheme.Light:
                    ThemeComboBox.SelectedIndex= 0; break;
                case ElementTheme.Dark:
                    ThemeComboBox.SelectedIndex = 1; break;
                case ElementTheme.Default:
                    ThemeComboBox.SelectedIndex = 2; break;
                default:
                    break;
            }
        }
    }
    /* end: code for theme */

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();


        UpdateThemeComboBox();
        InitMonitorOrderSettings();

        SetNumberBoxNumber();

        SetLanguageComboBox();
    }
}
