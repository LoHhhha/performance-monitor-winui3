using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using performance_monitor_winui3.Models;
using performance_monitor_winui3.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Input;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Text;
using performance_monitor_winui3.Tools;
using System.Diagnostics;
using ColorCode.Common;
using performance_monitor_winui3.Dao;


namespace performance_monitor_winui3.Views;

struct SchedulePageItemDialogControlStruct(StackPanel stackPanel, InfoBar infoBar, TextBox nameTextBox, TextBox detailTextBox, ComboBox daySelector, TimePicker beginTimePicker, TimePicker endTimePicker)
{
    public readonly StackPanel StackPanel = stackPanel;
    public readonly InfoBar infoBar = infoBar;
    public readonly TextBox NameTextBox = nameTextBox;
    public readonly TextBox DetailTextBox = detailTextBox;
    public readonly ComboBox DaySelector = daySelector;
    public readonly TimePicker BeginTimePicker = beginTimePicker;
    public readonly TimePicker EndTimePicker = endTimePicker;

    public bool Check()
    {
        if (NameTextBox.Text == "")
        {
            infoBar.Message = StringResource.Get("SchedulePage_DialogInputNameError");
            infoBar.IsOpen = true;
            return false;
        }

        if (BeginTimePicker.SelectedTime == null)
        {
            infoBar.Message = StringResource.Get("SchedulePage_DialogInputBTimeError");
            infoBar.IsOpen = true;
            return false;
        }

        if (EndTimePicker.SelectedTime == null)
        {
            infoBar.Message = StringResource.Get("SchedulePage_DialogInputETimeError");
            infoBar.IsOpen = true;
            return false;
        }

        if (BeginTimePicker.SelectedTime > EndTimePicker.SelectedTime) 
        {
            infoBar.Message = StringResource.Get("SchedulePage_DialogInputTimeError");
            infoBar.IsOpen = true;
            return false;
        }

        if (DaySelector.SelectedIndex == -1)
        {
            infoBar.Message = StringResource.Get("SchedulePage_DialogInputDayError");
            infoBar.IsOpen = true;
            return false;
        }

        return true;
    }
}

public sealed partial class SchedulePage : Page
{
    public ObservableCollection<ScheduleItem> ViewItems=[];

    public ScheduleViewModel ViewModel
    {
        get;
    }

    public async void FlushViewItems(Models.DayOfWeek day)
    {
        LoadProgressRing.Visibility = Visibility.Visible;
        LoadProgressRing.IsActive = true;

        var res = ScheduleViewModel.GetScheduleItems(day);

        ViewItems.Clear();
        foreach (var item in res)
        {
            ViewItems.Add(item);
        }

        LoadProgressRing.Visibility = Visibility.Collapsed;
        LoadProgressRing.IsActive = false;
    }

    public void SortViewItems()
    {
        ViewItems.SortStable((x, y) =>
        {
            if (x.BeginTime == y.BeginTime)
            {
                return x.EndTime.Hours * 60 + x.EndTime.Minutes - y.EndTime.Hours * 60 - y.EndTime.Minutes;
            }
            return x.BeginTime.Hours * 60 + x.BeginTime.Minutes - y.BeginTime.Hours * 60 - y.BeginTime.Minutes;
        });
    }

    // 0-6 -> Mon-Sun
    // not exists All
    public void SelectChanged(object sender, RoutedEventArgs e)
    {
        var segmented = sender as Segmented;

        if ((Models.DayOfWeek)segmented.SelectedIndex == Models.DayOfWeek.All) return;
        FlushViewItems((Models.DayOfWeek)segmented.SelectedIndex);
    }

    public async void ShowItem(ScheduleItem item)
    {
        // get Controls
        SchedulePageItemDialogControlStruct itemDialogStruct = GetItemDialogControlStruct();
        itemDialogStruct.NameTextBox.Text = item.ItemName;
        itemDialogStruct.DetailTextBox.Text = item.ItemDetail;
        itemDialogStruct.BeginTimePicker.Time = item.BeginTime;
        itemDialogStruct.EndTimePicker.Time = item.EndTime;
        itemDialogStruct.DaySelector.SelectedIndex = (int)item.Day;

        itemDialogStruct.NameTextBox.IsReadOnly = true;
        itemDialogStruct.DetailTextBox.IsReadOnly = true;
        itemDialogStruct.BeginTimePicker.IsEnabled = false;
        itemDialogStruct.EndTimePicker.IsEnabled = false;
        itemDialogStruct.DaySelector.IsEnabled = false;

        // set dialog
        ContentDialog dialog = new ContentDialog()
        {
            XamlRoot = this.XamlRoot,
            Title = StringResource.Get("SchedulePage_DialogItemMore"),
            Content = itemDialogStruct.StackPanel,
            CloseButtonText = StringResource.Get("SchedulePage_DialogConfirm")
        };

        await dialog.ShowAsync();
    }

    public void ItemMore(object sender, RoutedEventArgs e)
    {
        var menuFlyoutItem = sender as MenuFlyoutItem;
        var item = menuFlyoutItem.DataContext as ScheduleItem;

        ShowItem(item);
    }

    public async void ItemEdit(object sender, RoutedEventArgs e)
    {
        var menuFlyoutItem = sender as MenuFlyoutItem;
        var item = menuFlyoutItem.DataContext as ScheduleItem;

        // get Controls
        SchedulePageItemDialogControlStruct itemDialogStruct = GetItemDialogControlStruct();
        itemDialogStruct.NameTextBox.Text = item.ItemName;
        itemDialogStruct.DetailTextBox.Text = item.ItemDetail;
        itemDialogStruct.BeginTimePicker.Time = item.BeginTime;
        itemDialogStruct.EndTimePicker.Time = item.EndTime;
        itemDialogStruct.DaySelector.SelectedIndex = (int)item.Day;


        // set dialog
        ContentDialog dialog = new ContentDialog()
        {
            XamlRoot = this.XamlRoot,
            Title = StringResource.Get("SchedulePage_DialogItemEdit"),
            Content = itemDialogStruct.StackPanel,
            PrimaryButtonText = StringResource.Get("SchedulePage_DialogConfirm"),
            CloseButtonText = StringResource.Get("SchedulePage_DialogCancel")
        };

        while (true)
        {
            var result = await dialog.ShowAsync();

            // cancel
            if (result != ContentDialogResult.Primary)
            {
                return;
            }

            // input is invalid
            if (!itemDialogStruct.Check())
            {
                continue;
            }

            // get
            ScheduleItem newItem = new ScheduleItem(
                itemDialogStruct.NameTextBox.Text,
                itemDialogStruct.DetailTextBox.Text,
                itemDialogStruct.BeginTimePicker.Time,
                itemDialogStruct.EndTimePicker.Time,
                (Models.DayOfWeek)itemDialogStruct.DaySelector.SelectedIndex
            );
            ScheduleViewModel.UpdateScheduleItem(item, newItem);

            // here, item had been edited.
            // item.Day == newItem.Day or newItem.Day == Models.DayOfWeek.All this Item should still be there.
            // else it should be removed.
            if (item.Day == newItem.Day || newItem.Day == Models.DayOfWeek.All)
            {
                try
                {
                    ViewItems[ViewItems.IndexOf(item)] = newItem;
                }
                catch { }
                SortViewItems();
            }
            else
            {
                ViewItems.Remove(item);
            }

            break;
        }
    }

    public async void ItemDelete(object sender, RoutedEventArgs e)
    {
        var menuFlyoutItem = sender as MenuFlyoutItem;
        var item = menuFlyoutItem.DataContext as ScheduleItem;

        // set dialog
        ContentDialog dialog = new ContentDialog()
        {
            XamlRoot = this.XamlRoot,
            Title = StringResource.Get("SchedulePage_DialogItemDelete"),
            PrimaryButtonText = StringResource.Get("SchedulePage_DialogConfirm"),
            CloseButtonText = StringResource.Get("SchedulePage_DialogCancel")
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            ScheduleViewModel.DeleteScheduleItem(item);
            // this item should be removed.
            ViewItems.Remove(item);
        }
    }

    public async void ItemAdd(object sender, RoutedEventArgs e)
    {
        // get Controls
        SchedulePageItemDialogControlStruct itemDialogStruct = GetItemDialogControlStruct();

        // set dialog
        ContentDialog dialog = new ContentDialog()
        {
            XamlRoot = this.XamlRoot,
            Title = StringResource.Get("SchedulePage_DialogItemAdd"),
            Content = itemDialogStruct.StackPanel,
            PrimaryButtonText = StringResource.Get("SchedulePage_DialogConfirm"),
            CloseButtonText = StringResource.Get("SchedulePage_DialogCancel")
        };

        while (true)
        {
            var result = await dialog.ShowAsync();

            // cancel
            if (result != ContentDialogResult.Primary)
            {
                break;
            }

            // input is invalid
            if (!itemDialogStruct.Check())
            {
                continue;
            }

            // get
            ScheduleItem newItem = new ScheduleItem(
                itemDialogStruct.NameTextBox.Text,
                itemDialogStruct.DetailTextBox.Text,
                itemDialogStruct.BeginTimePicker.Time,
                itemDialogStruct.EndTimePicker.Time,
                (Models.DayOfWeek)itemDialogStruct.DaySelector.SelectedIndex
            );
            ScheduleViewModel.AddScheduleItem(newItem);

            // here, item had been edited.
            // (int)newItem.Day == DaySelect.SelectedIndex or || newItem.Day == Models.DayOfWeek.All this newItem should be there.
            if ((int)newItem.Day == DaySelect.SelectedIndex || newItem.Day == Models.DayOfWeek.All) 
            {
                ViewItems.Add(newItem);
                SortViewItems();
            }

            break;
        }
    }

    /* Begin: Item Pointer */
    private bool ItemIsDown = false;
    private void ItemPressed(object sender, PointerRoutedEventArgs e)
    {
        if ((e.Pointer.PointerDeviceType == PointerDeviceType.Mouse && e.GetCurrentPoint(null).Properties.IsLeftButtonPressed) || e.Pointer.PointerDeviceType == PointerDeviceType.Touch)
        {
            ItemIsDown = true;
        }
    }
    private void ItemReleased(object sender, PointerRoutedEventArgs e)
    {
        // TODO: fix when left button pressed exit, other button pressed enter will excute this.
        if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse || e.Pointer.PointerDeviceType == PointerDeviceType.Touch)
        {
            if (ItemIsDown)
            {
                Border border = sender as Border;
                var item = border.DataContext as ScheduleItem;

                ShowItem(item);
            }
        }
        ItemIsDown = false;
    }
    private bool ItemIsFocused = false;
    private void ItemEntered(object sender, PointerRoutedEventArgs e)
    {
        ItemIsFocused = true;
        Border border = sender as Border;
        border.BorderThickness = new Thickness(2);
        border.Padding = new Thickness(3);
    }
    private void ItemExited(object sender, PointerRoutedEventArgs e)
    {
        ItemIsFocused = false;
        Border border = sender as Border;
        border.BorderThickness = new Thickness(0);
        border.Padding = new Thickness(5);
    }
    /* End: Item Pointer */

    public SchedulePage()
    {
        ViewModel = App.GetService<ScheduleViewModel>();
        InitializeComponent();

        // set DaySelect
        var currentDayOfWeek = (int)DateTime.Now.DayOfWeek - 1;
        currentDayOfWeek = currentDayOfWeek >= 0 ? currentDayOfWeek : 6;
        DaySelect.SelectedIndex = currentDayOfWeek;
        // set list
        FlushViewItems((Models.DayOfWeek)currentDayOfWeek);
    }

    private SchedulePageItemDialogControlStruct GetItemDialogControlStruct()
    {
        StackPanel stackPanel = new StackPanel();

        // InfoBar
        InfoBar infoBar = new InfoBar();
        infoBar.Title = StringResource.Get("SchedulePage_DialogInputCheck");
        infoBar.Severity = InfoBarSeverity.Error;
        infoBar.IsOpen = false;
        stackPanel.Children.Add(infoBar);

        // Item Name
        TextBlock nameTextBlock = new TextBlock();
        nameTextBlock.FontSize = 16;
        nameTextBlock.FontWeight = FontWeights.SemiBold;
        nameTextBlock.Text = StringResource.Get("SchedulePage_DialogItemName");
        TextBox nameTextBox = new TextBox();
        stackPanel.Children.Add(nameTextBlock);
        stackPanel.Children.Add(nameTextBox);

        // Item Detail
        TextBlock detailTextBlock = new TextBlock();
        detailTextBlock.Margin = new Thickness(0, 10, 0, 0);
        detailTextBlock.FontSize = 16;
        detailTextBlock.FontWeight = FontWeights.SemiBold;
        detailTextBlock.Text = StringResource.Get("SchedulePage_DialogItemDetail");
        TextBox detailTextBox = new TextBox();
        detailTextBox.AcceptsReturn = true;
        detailTextBox.TextWrapping = TextWrapping.Wrap;
        detailTextBox.MaxHeight = 172;
        ScrollViewer.SetVerticalScrollBarVisibility(detailTextBox, ScrollBarVisibility.Auto);
        stackPanel.Children.Add(detailTextBlock);
        stackPanel.Children.Add(detailTextBox);

        // Item day of week
        TextBlock dayOfWeekTextBlock = new TextBlock();
        dayOfWeekTextBlock.Margin = new Thickness(0, 10, 0, 0);
        dayOfWeekTextBlock.FontSize = 16;
        dayOfWeekTextBlock.FontWeight = FontWeights.SemiBold;
        dayOfWeekTextBlock.Text = StringResource.Get("SchedulePage_DialogItemDayOfWeek");
        ComboBox comboBox = new ComboBox();
        foreach(var day in Enum.GetNames(typeof(Models.DayOfWeek)))
        {
            comboBox.Items.Add(day);
        }
        comboBox.SelectedIndex = 0;
        comboBox.HorizontalAlignment = HorizontalAlignment.Stretch;
        stackPanel.Children.Add(dayOfWeekTextBlock);
        stackPanel.Children.Add(comboBox);

        // Item Time
        TextBlock timeTextBlock = new TextBlock();
        timeTextBlock.Margin = new Thickness(0, 10, 0, 0);
        timeTextBlock.FontSize = 16;
        timeTextBlock.FontWeight = FontWeights.SemiBold;
        timeTextBlock.Text = StringResource.Get("SchedulePage_DialogItemTime");
        TimePicker beginTimePicker = new TimePicker();
        beginTimePicker.Header = StringResource.Get("SchedulePage_DialogItemBTime");
        beginTimePicker.HorizontalAlignment = HorizontalAlignment.Stretch;
        beginTimePicker.ClockIdentifier = "24HourClock";
        TimePicker endTimePicker = new TimePicker();
        endTimePicker.Header = StringResource.Get("SchedulePage_DialogItemETime");
        endTimePicker.HorizontalAlignment = HorizontalAlignment.Stretch;
        endTimePicker.ClockIdentifier = "24HourClock";
        stackPanel.Children.Add(timeTextBlock);
        stackPanel.Children.Add(beginTimePicker);
        stackPanel.Children.Add(endTimePicker);

        return new SchedulePageItemDialogControlStruct(stackPanel, infoBar, nameTextBox, detailTextBox, comboBox, beginTimePicker, endTimePicker);
    }
}
