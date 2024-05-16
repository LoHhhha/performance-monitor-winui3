using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using performance_monitor_winui3.ViewModels;
using performance_monitor_winui3.Models;
using performance_monitor_winui3.Tools;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Input;

namespace performance_monitor_winui3.Views;

struct ToDoPageItemDialogControlStruct(StackPanel stackPanel, InfoBar infoBar, TextBox nameTextBox, TextBox detailTextBox, DatePicker datePicker, TimePicker timePicker)
{
    public readonly StackPanel StackPanel = stackPanel;
    public readonly InfoBar infoBar = infoBar;
    public readonly TextBox NameTextBox = nameTextBox;
    public readonly TextBox DetailTextBox = detailTextBox;
    public readonly DatePicker DatePicker = datePicker;
    public readonly TimePicker TimePicker = timePicker;

    public bool Check()
    {
        if (NameTextBox.Text == "")
        {
            infoBar.Message = StringResource.Get("ToDoListPage_DialogInputTaskNameError");
            infoBar.IsOpen = true;
            return false;
        }

        if (DatePicker.SelectedDate == null)
        {
            infoBar.Message = StringResource.Get("ToDoListPage_DialogInputDeadlineDateError");
            infoBar.IsOpen = true;
            return false;
        }

        if (TimePicker.SelectedTime == null)
        {
            infoBar.Message = StringResource.Get("ToDoListPage_DialogInputDeadlineTimeError");
            infoBar.IsOpen = true;
            return false;
        }

        return true;
    }
}

public sealed partial class ToDoListPage : Page
{
    public ToDoListViewModel ViewModel
    {
        get;
    }

    public ToDoListPage()
    {
        ViewModel = App.GetService<ToDoListViewModel>();
        InitializeComponent();
    }

    private async void ToDoListCancelButton(object sender, RoutedEventArgs e)
    {
        var menuFlyoutItem = sender as MenuFlyoutItem;
        var item = menuFlyoutItem.DataContext as ToDoListItem;

        // set dialog
        ContentDialog dialog = new ContentDialog()
        {
            XamlRoot = this.XamlRoot,
            Title = StringResource.Get("ToDoListPage_DialogTaskCancel"),
            PrimaryButtonText = StringResource.Get("ToDoListPage_DialogConfirm"),
            CloseButtonText = StringResource.Get("ToDoListPage_DialogCancel")
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            ToDoListViewModel.ToDoListDeleteItem(item);
        }
    }

    private void ToDoListDoneButton(object sender, RoutedEventArgs e)
    {
        var menuFlyoutItem = sender as MenuFlyoutItem;
        var item = menuFlyoutItem.DataContext as ToDoListItem;
        ToDoListViewModel.ToDoListDeleteItem(item);
        ToDoListViewModel.FinishListAddItem(item);
    }

    private void ToDoListDownButton(object sender, RoutedEventArgs e)
    {
        var menuFlyoutItem = sender as MenuFlyoutItem;
        var item = menuFlyoutItem.DataContext as ToDoListItem;
        ToDoListViewModel.ToDoListDeleteItem(item);
        item.Id = 0;
        ToDoListViewModel.ToDoListAddItem(item);
    }

    private async void ToDoListAddButton(object sender, RoutedEventArgs e)
    {
        // get Controls
        ToDoPageItemDialogControlStruct itemDialogStruct = GetItemDialogControlStruct();

        // set dialog
        ContentDialog dialog = new ContentDialog()
        {
            XamlRoot = this.XamlRoot,
            Title = StringResource.Get("ToDoListPage_DialogTaskAdd"),
            Content = itemDialogStruct.StackPanel,
            PrimaryButtonText = StringResource.Get("ToDoListPage_DialogConfirm"),
            CloseButtonText = StringResource.Get("ToDoListPage_DialogCancel")
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
            ToDoListItem newItem = new ToDoListItem(
                itemDialogStruct.NameTextBox.Text,
                itemDialogStruct.DetailTextBox.Text,
                new DateTime(
                    itemDialogStruct.DatePicker.Date.Year,
                    itemDialogStruct.DatePicker.Date.Month,
                    itemDialogStruct.DatePicker.Date.Day,
                    itemDialogStruct.TimePicker.Time.Hours,
                    itemDialogStruct.TimePicker.Time.Minutes,
                    itemDialogStruct.TimePicker.Time.Seconds
                )
            );
            ToDoListViewModel.ToDoListAddItem(newItem);
            break;
        }
        
    }

    private async void ToDoListEditButton(object sender, RoutedEventArgs e)
    {
        var menuFlyoutItem = sender as MenuFlyoutItem;
        var item = menuFlyoutItem.DataContext as ToDoListItem;

        // get Controls
        ToDoPageItemDialogControlStruct itemDialogStruct = GetItemDialogControlStruct();
        itemDialogStruct.NameTextBox.Text = item.TaskName;
        itemDialogStruct.DetailTextBox.Text = item.TaskDetail;
        itemDialogStruct.DatePicker.Date = item.Deadline;
        itemDialogStruct.TimePicker.Time = item.Deadline - new DateTime(item.Deadline.Year, item.Deadline.Month, item.Deadline.Day);

        // set dialog
        ContentDialog dialog = new ContentDialog()
        {
            XamlRoot = this.XamlRoot,
            Title = StringResource.Get("ToDoListPage_DialogTaskEdit"),
            Content = itemDialogStruct.StackPanel,
            PrimaryButtonText = StringResource.Get("ToDoListPage_DialogConfirm"),
            CloseButtonText = StringResource.Get("ToDoListPage_DialogCancel")
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
            ToDoListItem newItem = new ToDoListItem(
                itemDialogStruct.NameTextBox.Text,
                itemDialogStruct.DetailTextBox.Text,
                new DateTime(
                    itemDialogStruct.DatePicker.Date.Year,
                    itemDialogStruct.DatePicker.Date.Month,
                    itemDialogStruct.DatePicker.Date.Day,
                    itemDialogStruct.TimePicker.Time.Hours,
                    itemDialogStruct.TimePicker.Time.Minutes,
                    itemDialogStruct.TimePicker.Time.Seconds
                )
            );
            newItem.CreateTime = item.CreateTime;
            ToDoListViewModel.ToDoListReplaceItem(item, newItem);
            break;
        }
    }

    private async void ShowToDoListItem(ToDoListItem item)
    {
        // get Controls
        ToDoPageItemDialogControlStruct itemDialogStruct = GetItemDialogControlStruct();
        itemDialogStruct.NameTextBox.Text = item.TaskName;
        itemDialogStruct.DetailTextBox.Text = item.TaskDetail;
        itemDialogStruct.DatePicker.Date = item.Deadline;
        itemDialogStruct.TimePicker.Time = item.Deadline - new DateTime(item.Deadline.Year, item.Deadline.Month, item.Deadline.Day);
        itemDialogStruct.NameTextBox.IsReadOnly = true;
        itemDialogStruct.DetailTextBox.IsReadOnly = true;
        itemDialogStruct.DatePicker.IsEnabled = false;
        itemDialogStruct.TimePicker.IsEnabled = false;

        // set dialog
        ContentDialog dialog = new ContentDialog()
        {
            XamlRoot = this.XamlRoot,
            Title = StringResource.Get("ToDoListPage_DialogTaskMore"),
            Content = itemDialogStruct.StackPanel,
            CloseButtonText = StringResource.Get("ToDoListPage_DialogConfirm")
        };

        await dialog.ShowAsync();
    }

    private void ToDoListMoreButton(object sender, RoutedEventArgs e)
    {
        var menuFlyoutItem = sender as MenuFlyoutItem;
        var item = menuFlyoutItem.DataContext as ToDoListItem;

        ShowToDoListItem(item);
    }

    /* Begin: ToDo Item Pointer */
    private bool ToDoListItemIsDown = false;
    private void ToDoListItemPressed(object sender, PointerRoutedEventArgs e) 
    {
        if ((e.Pointer.PointerDeviceType == PointerDeviceType.Mouse && e.GetCurrentPoint(null).Properties.IsLeftButtonPressed) || e.Pointer.PointerDeviceType == PointerDeviceType.Touch) 
        {
            ToDoListItemIsDown = true;
        }
    }
    private void ToDoListItemReleased(object sender, PointerRoutedEventArgs e)
    {
        // TODO: fix when left button pressed exit, other button pressed enter will excute this.
        if ((e.Pointer.PointerDeviceType == PointerDeviceType.Mouse || e.Pointer.PointerDeviceType == PointerDeviceType.Touch))
        {
            if (ToDoListItemIsDown)
            {
                Border border = sender as Border;
                var item = border.DataContext as ToDoListItem;
                ShowToDoListItem(item);
            }
        }
        ToDoListItemIsDown = false;
    }

    private bool ToDoListItemIsFocused = false;
    private void ToDoListItemEntered(object sender, PointerRoutedEventArgs e)
    {
        ToDoListItemIsFocused = true;
        Border border = sender as Border;
        border.BorderThickness = new Thickness(2);
        border.Padding = new Thickness(3);
    }
    private void ToDoListItemExited(object sender, PointerRoutedEventArgs e)
    {
        ToDoListItemIsFocused = false;
        Border border = sender as Border;
        border.BorderThickness = new Thickness(0);
        border.Padding = new Thickness(5);
    }
    /* End: ToDo Item Pointer */

    private async void FinishListDeleteButton(object sender, RoutedEventArgs e)
    {
        var menuFlyoutItem = sender as MenuFlyoutItem;
        var item = menuFlyoutItem.DataContext as FinishListItem;

        // set dialog
        ContentDialog dialog = new ContentDialog()
        {
            XamlRoot = this.XamlRoot,
            Title = StringResource.Get("ToDoListPage_DialogTaskDelete"),
            PrimaryButtonText = StringResource.Get("ToDoListPage_DialogConfirm"),
            CloseButtonText = StringResource.Get("ToDoListPage_DialogCancel")
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            ToDoListViewModel.FinishListDeleteItem(item);
        }
    }

    private void FinishListRecompleteButton(object sender, RoutedEventArgs e)
    {
        var menuFlyoutItem = sender as MenuFlyoutItem;
        var item = menuFlyoutItem.DataContext as FinishListItem;
        ToDoListViewModel.FinishListDeleteItem(item);
        ToDoListViewModel.ToDoListAddItem(item.TaskItem);
    }

    private async void ShowFinishListItem(FinishListItem finishItem)
    {
        var item = finishItem.TaskItem;

        // get Controls
        ToDoPageItemDialogControlStruct itemDialogStruct = GetItemDialogControlStruct();
        itemDialogStruct.NameTextBox.Text = item.TaskName;
        itemDialogStruct.DetailTextBox.Text = item.TaskDetail;
        itemDialogStruct.DatePicker.Date = item.Deadline;
        itemDialogStruct.TimePicker.Time = item.Deadline - new DateTime(item.Deadline.Year, item.Deadline.Month, item.Deadline.Day);
        itemDialogStruct.NameTextBox.IsReadOnly = true;
        itemDialogStruct.DetailTextBox.IsReadOnly = true;
        itemDialogStruct.DatePicker.IsEnabled = false;
        itemDialogStruct.TimePicker.IsEnabled = false;

        // TaskFinishTime
        TextBlock finishTextBlock = new TextBlock();
        finishTextBlock.Margin = new Thickness(0, 10, 0, 0);
        finishTextBlock.FontSize = 16;
        finishTextBlock.FontWeight = FontWeights.SemiBold;
        finishTextBlock.Text = StringResource.Get("ToDoListPage_DialogTaskFinishTime");
        DatePicker datePicker = new DatePicker();
        datePicker.Header = StringResource.Get("ToDoListPage_DialogTaskDate");
        datePicker.HorizontalAlignment = HorizontalAlignment.Stretch;
        TimePicker timePicker = new TimePicker();
        timePicker.Header = StringResource.Get("ToDoListPage_DialogTaskTime");
        timePicker.HorizontalAlignment = HorizontalAlignment.Stretch;
        datePicker.Date = finishItem.FinishTime;
        timePicker.Time = finishItem.FinishTime - new DateTime(finishItem.FinishTime.Year, finishItem.FinishTime.Month, finishItem.FinishTime.Day);
        itemDialogStruct.StackPanel.Children.Add(finishTextBlock);
        itemDialogStruct.StackPanel.Children.Add(datePicker);
        itemDialogStruct.StackPanel.Children.Add(timePicker);
        datePicker.IsEnabled = false;
        timePicker.IsEnabled = false;


        // set dialog
        ContentDialog dialog = new ContentDialog()
        {
            XamlRoot = this.XamlRoot,
            Title = StringResource.Get("ToDoListPage_DialogTaskMore"),
            Content = itemDialogStruct.StackPanel,
            CloseButtonText = StringResource.Get("ToDoListPage_DialogConfirm")
        };

        await dialog.ShowAsync();
    }

    private void FinishListMoreButton(object sender, RoutedEventArgs e)
    {
        var menuFlyoutItem = sender as MenuFlyoutItem;
        var finishItem = menuFlyoutItem.DataContext as FinishListItem;

        ShowFinishListItem(finishItem);
    }

    /* Begin: Finish Item Pointer */
    private bool FinishListItemIsDown = false;
    private void FinishListItemPressed(object sender, PointerRoutedEventArgs e)
    {
        if ((e.Pointer.PointerDeviceType == PointerDeviceType.Mouse && e.GetCurrentPoint(null).Properties.IsLeftButtonPressed) || e.Pointer.PointerDeviceType == PointerDeviceType.Touch)
        {
            FinishListItemIsDown = true;
        }
    }
    private void FinishListItemReleased(object sender, PointerRoutedEventArgs e)
    {
        // TODO: fix when left button pressed exit, other button pressed enter will excute this.
        if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse || e.Pointer.PointerDeviceType == PointerDeviceType.Touch)
        {
            if (FinishListItemIsDown)
            {
                Border border = sender as Border;
                var item = border.DataContext as FinishListItem;
                ShowFinishListItem(item);
            }
        }
        FinishListItemIsDown = false;
    }
    private bool FinishListItemIsFocused = false;
    private void FinishListItemEntered(object sender, PointerRoutedEventArgs e)
    {
        FinishListItemIsFocused = true;
        Border border = sender as Border;
        border.BorderThickness = new Thickness(2);
        border.Padding = new Thickness(3);
    }
    private void FinishListItemExited(object sender, PointerRoutedEventArgs e)
    {
        FinishListItemIsFocused = false;
        Border border = sender as Border;
        border.BorderThickness = new Thickness(0);
        border.Padding = new Thickness(5);
    }
    /* End: Finish Item Pointer */

    private ToDoPageItemDialogControlStruct GetItemDialogControlStruct()
    {
        StackPanel stackPanel = new StackPanel();

        // InfoBar
        InfoBar infoBar = new InfoBar();
        infoBar.Title = StringResource.Get("ToDoListPage_DialogInputCheck");
        infoBar.Severity=InfoBarSeverity.Error;
        infoBar.IsOpen = false;
        stackPanel.Children.Add(infoBar);

        // TaskName
        TextBlock nameTextBlock = new TextBlock();
        nameTextBlock.FontSize = 16;
        nameTextBlock.FontWeight = FontWeights.SemiBold;
        nameTextBlock.Text = StringResource.Get("ToDoListPage_DialogTaskName");
        TextBox nameTextBox = new TextBox();
        stackPanel.Children.Add(nameTextBlock);
        stackPanel.Children.Add(nameTextBox);

        // TaskDetail
        TextBlock detailTextBlock = new TextBlock();
        detailTextBlock.Margin = new Thickness(0, 10, 0, 0);
        detailTextBlock.FontSize = 16;
        detailTextBlock.FontWeight = FontWeights.SemiBold;
        detailTextBlock.Text = StringResource.Get("ToDoListPage_DialogTaskDetail");
        TextBox detailTextBox = new TextBox();
        detailTextBox.AcceptsReturn = true;
        detailTextBox.TextWrapping = TextWrapping.Wrap;
        detailTextBox.MaxHeight = 172;
        ScrollViewer.SetVerticalScrollBarVisibility(detailTextBox, ScrollBarVisibility.Auto);
        stackPanel.Children.Add(detailTextBlock);
        stackPanel.Children.Add(detailTextBox);

        // TaskDateTime
        TextBlock deadlineTextBlock = new TextBlock();
        deadlineTextBlock.Margin = new Thickness(0, 10, 0, 0);
        deadlineTextBlock.FontSize = 16;
        deadlineTextBlock.FontWeight = FontWeights.SemiBold;
        deadlineTextBlock.Text = StringResource.Get("ToDoListPage_DialogTaskDealine");
        DatePicker datePicker = new DatePicker();
        datePicker.Header = StringResource.Get("ToDoListPage_DialogTaskDate");
        datePicker.HorizontalAlignment = HorizontalAlignment.Stretch;
        TimePicker timePicker = new TimePicker();
        timePicker.Header = StringResource.Get("ToDoListPage_DialogTaskTime");
        timePicker.HorizontalAlignment = HorizontalAlignment.Stretch;
        timePicker.ClockIdentifier = "24HourClock";
        stackPanel.Children.Add(deadlineTextBlock);
        stackPanel.Children.Add(datePicker);
        stackPanel.Children.Add(timePicker);

        return new ToDoPageItemDialogControlStruct(stackPanel, infoBar, nameTextBox, detailTextBox, datePicker, timePicker);
    }
}
