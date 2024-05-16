using Microsoft.UI.Xaml.Controls;

using performance_monitor_winui3.ViewModels;
using performance_monitor_winui3.Models;
using Microsoft.UI.Xaml;
using performance_monitor_winui3.Tools;

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

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();

        InitMonitorOrderSettings();
    }
}
