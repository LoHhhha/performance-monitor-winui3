using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Input;
using performance_monitor_winui3.Models;
using performance_monitor_winui3.ViewModels;
using performance_monitor_winui3.Tools;
using Microsoft.UI.Text;

namespace performance_monitor_winui3.Views;

struct AccessPageItemDialogControlStruct(StackPanel stackPanel, InfoBar infoBar, TextBox nameTextBox, TextBox commandTextBox, CheckBox showWindowsCheckBox, CheckBox exitAfterFinishCheckBox)
{
    public readonly StackPanel StackPanel = stackPanel;
    public readonly InfoBar infoBar = infoBar;
    public readonly TextBox NameTextBox = nameTextBox;
    public readonly TextBox CommandTextBox = commandTextBox;
    public readonly CheckBox ShowWindowsCheckBox = showWindowsCheckBox;
    public readonly CheckBox ExitAfterFinishCheckBox = exitAfterFinishCheckBox;

    public bool Check()
    {
        if (NameTextBox.Text == "")
        {
            infoBar.Message = StringResource.Get("AccessPage_DialogInputTaskNameError");
            infoBar.IsOpen = true;
            return false;
        }

        if (ShowWindowsCheckBox.IsChecked == null)
        {
            infoBar.Message = StringResource.Get("AccessPage_DialogInputParametersError");
            infoBar.IsOpen = true;
            return false;
        }

        if (ExitAfterFinishCheckBox.IsChecked == null)
        {
            infoBar.Message = StringResource.Get("AccessPage_DialogInputParametersError");
            infoBar.IsOpen = true;
            return false;
        }

        return true;
    }
}


public sealed partial class AccessPage : Page
{
    public AccessViewModel ViewModel
    {
        get;
    }

    public AccessPage()
    {
        ViewModel = App.GetService<AccessViewModel>();
        InitializeComponent();
    }

    public void InfoBarClose(InfoBar sender, InfoBarClosingEventArgs args)
    {
        if (args.Reason == InfoBarCloseReason.CloseButton)
        {
            if (sender.DataContext is AccessRunError runError)
            {
                AccessViewModel.ErrorMessagesDelete(runError);
            }

            // because it is a DataTemplate, we should avoid to set sender.IsOpen=false
            sender.IsOpen = true;
        }
    }

    public async void AddTaskButton(object sender, RoutedEventArgs e)
    {
        var itemDialogStruct = GetItemDialogControlStruct();
        itemDialogStruct.ShowWindowsCheckBox.IsChecked = false;
        itemDialogStruct.ExitAfterFinishCheckBox.IsChecked = true;
        itemDialogStruct.ExitAfterFinishCheckBox.IsEnabled = false;

        // set dialog
        ContentDialog dialog = new ContentDialog()
        {
            XamlRoot = this.XamlRoot,
            Title = StringResource.Get("AccessPage_DialogTaskAdd"),
            Content = itemDialogStruct.StackPanel,
            PrimaryButtonText = StringResource.Get("AccessPage_DialogConfirm"),
            CloseButtonText = StringResource.Get("AccessPage_DialogCancel")
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
            AccessItem accessItem = new AccessItem(
                itemDialogStruct.NameTextBox.Text,
                itemDialogStruct.CommandTextBox.Text,
                (bool)itemDialogStruct.ShowWindowsCheckBox.IsChecked,
                (bool)itemDialogStruct.ExitAfterFinishCheckBox.IsChecked
            );
            AccessViewModel.AccessItemsAdd(accessItem);

            break;
        }
    }

    public async void ShowAccessItem(AccessItem accessItem)
    {
        // get Controls
        var itemDialogStruct = GetItemDialogControlStruct();
        itemDialogStruct.NameTextBox.Text = accessItem.TaskName;
        itemDialogStruct.CommandTextBox.Text = accessItem.Command;
        itemDialogStruct.NameTextBox.IsReadOnly = true;
        itemDialogStruct.CommandTextBox.IsReadOnly = true;
        itemDialogStruct.ShowWindowsCheckBox.IsChecked = accessItem.ShowWindow;
        itemDialogStruct.ExitAfterFinishCheckBox.IsChecked = accessItem.ExitAfterFinish;
        itemDialogStruct.ShowWindowsCheckBox.IsEnabled = false;
        itemDialogStruct.ExitAfterFinishCheckBox.IsEnabled = false;

        // set dialog
        ContentDialog dialog = new ContentDialog()
        {
            XamlRoot = this.XamlRoot,
            Title = StringResource.Get("AccessPage_DialogTaskMore"),
            Content = itemDialogStruct.StackPanel,
            CloseButtonText = StringResource.Get("AccessPage_DialogConfirm")
        };

        await dialog.ShowAsync();
    }

    public void AccessItemMore(object sender, RoutedEventArgs e)
    {
        var menuFlyoutItem = sender as MenuFlyoutItem;
        var item = menuFlyoutItem.DataContext as AccessItem;
        ShowAccessItem(item);
    }

    public void AccessItemRun(object sender, RoutedEventArgs e)
    {
        AccessItem? item = null;
        if(sender is MenuFlyoutItem)
        {
            var menuFlyoutItem = sender as MenuFlyoutItem;
            item = menuFlyoutItem.DataContext as AccessItem;
        }
        else if(sender is Button)
        {
            var button = sender as Button;
            item = button.DataContext as AccessItem;
        }

        if(item != null)
        {
            AccessViewModel.Run(item);
        }
    }

    public void AccessItemDown(object sender, RoutedEventArgs e)
    {

        var menuFlyoutItem = sender as MenuFlyoutItem;
        var item = menuFlyoutItem.DataContext as AccessItem;

        AccessViewModel.AccessItemsDelete(item);
        AccessViewModel.AccessItemsAdd(new AccessItem(item.TaskName, item.Command, item.ShowWindow, item.ExitAfterFinish));
    }

    public async void AccessItemDelete(object sender, RoutedEventArgs e)
    {
        var menuFlyoutItem = sender as MenuFlyoutItem;
        var item = menuFlyoutItem.DataContext as AccessItem;

        // set dialog
        ContentDialog dialog = new ContentDialog()
        {
            XamlRoot = this.XamlRoot,
            Title = StringResource.Get("AccessPage_DialogTaskDelete"),
            PrimaryButtonText = StringResource.Get("AccessPage_DialogConfirm"),
            CloseButtonText = StringResource.Get("AccessPage_DialogCancel")
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            AccessViewModel.AccessItemsDelete(item);
        }
    }

    public async void AccessItemEdit(object sender, RoutedEventArgs e)
    {
        var menuFlyoutItem = sender as MenuFlyoutItem;
        var item = menuFlyoutItem.DataContext as AccessItem;

        var itemDialogStruct = GetItemDialogControlStruct();
        itemDialogStruct.NameTextBox.Text = item.TaskName;
        itemDialogStruct.CommandTextBox.Text = item.Command;
        itemDialogStruct.ShowWindowsCheckBox.IsChecked = item.ShowWindow;
        itemDialogStruct.ExitAfterFinishCheckBox.IsChecked = item.ExitAfterFinish;

        if (itemDialogStruct.ShowWindowsCheckBox.IsChecked == false)
        {
            itemDialogStruct.ExitAfterFinishCheckBox.IsChecked = true;
            itemDialogStruct.ExitAfterFinishCheckBox.IsEnabled = false;
        }
        

        // set dialog
        ContentDialog dialog = new ContentDialog()
        {
            XamlRoot = this.XamlRoot,
            Title = StringResource.Get("AccessPage_DialogTaskAdd"),
            Content = itemDialogStruct.StackPanel,
            PrimaryButtonText = StringResource.Get("AccessPage_DialogConfirm"),
            CloseButtonText = StringResource.Get("AccessPage_DialogCancel")
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
            AccessItem accessItem = new AccessItem(
                itemDialogStruct.NameTextBox.Text,
                itemDialogStruct.CommandTextBox.Text,
                (bool)itemDialogStruct.ShowWindowsCheckBox.IsChecked,
                (bool)itemDialogStruct.ExitAfterFinishCheckBox.IsChecked
            );
            AccessViewModel.AccessItemsReplace(item, accessItem);

            break;
        }
    }

    /* Begin: Item Pointer */
    private bool AccessItemIsDown = false;
    private void AccessItemPressed(object sender, PointerRoutedEventArgs e)
    {
        if ((e.Pointer.PointerDeviceType == PointerDeviceType.Mouse && e.GetCurrentPoint(null).Properties.IsLeftButtonPressed) || e.Pointer.PointerDeviceType == PointerDeviceType.Touch)
        {
            AccessItemIsDown = true;
        }
    }
    private void AccessItemReleased(object sender, PointerRoutedEventArgs e)
    {
        // TODO: fix when left button pressed exit, other button pressed enter will excute this.
        if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse || e.Pointer.PointerDeviceType == PointerDeviceType.Touch)
        {
            if (AccessItemIsDown)
            {
                Border border = sender as Border;
                var item = border.DataContext as AccessItem;

                ShowAccessItem(item);
            }
        }
        AccessItemIsDown = false;
    }
    private bool AccessItemIsFocused = false;
    private void AccessItemEntered(object sender, PointerRoutedEventArgs e)
    {
        AccessItemIsFocused = true;
        Border border = sender as Border;
        border.BorderThickness = new Thickness(2);
        border.Padding = new Thickness(3);
    }
    private void AccessItemExited(object sender, PointerRoutedEventArgs e)
    {
        AccessItemIsFocused = false;
        Border border = sender as Border;
        border.BorderThickness = new Thickness(0);
        border.Padding = new Thickness(5);
    }
    /* End: Item Pointer */

    private AccessPageItemDialogControlStruct GetItemDialogControlStruct()
    {
        StackPanel stackPanel = new StackPanel();

        // InfoBar
        InfoBar infoBar = new InfoBar();
        infoBar.Title = StringResource.Get("AccessPage_DialogInputCheck");
        infoBar.Severity = InfoBarSeverity.Error;
        infoBar.IsOpen = false;
        stackPanel.Children.Add(infoBar);

        // TaskName
        TextBlock nameTextBlock = new TextBlock();
        nameTextBlock.FontSize = 16;
        nameTextBlock.FontWeight = FontWeights.SemiBold;
        nameTextBlock.Text = StringResource.Get("AccessPage_DialogTaskName");
        TextBox nameTextBox = new TextBox();
        stackPanel.Children.Add(nameTextBlock);
        stackPanel.Children.Add(nameTextBox);

        // TaskCommond
        TextBlock commandTextBlock = new TextBlock();
        commandTextBlock.Margin = new Thickness(0, 10, 0, 0);
        commandTextBlock.FontSize = 16;
        commandTextBlock.FontWeight = FontWeights.SemiBold;
        commandTextBlock.Text = StringResource.Get("AccessPage_DialogTaskCommand");
        TextBox commandTextBox = new TextBox();
        commandTextBox.AcceptsReturn = true;
        commandTextBox.TextWrapping = TextWrapping.Wrap;
        commandTextBox.MaxHeight = 172;
        ScrollViewer.SetVerticalScrollBarVisibility(commandTextBox, ScrollBarVisibility.Auto);
        stackPanel.Children.Add(commandTextBlock);
        stackPanel.Children.Add(commandTextBox);


        TextBlock parametersTextBlock = new TextBlock();
        parametersTextBlock.Margin = new Thickness(0, 10, 0, 0);
        parametersTextBlock.FontSize = 16;
        parametersTextBlock.FontWeight = FontWeights.SemiBold;
        parametersTextBlock.Text = StringResource.Get("AccessPage_DialogTaskParameters");
        CheckBox showWindowsCheckBox = new CheckBox
        {
            Content = StringResource.Get("AccessPage_DialogTaskRunShowWindow")
        };
        CheckBox exitAfterFinishCheckBox = new CheckBox
        {
            Content = StringResource.Get("AccessPage_DialogTaskRunExitAfterFinish")
        };
        //sShowWindows is false, exitAfterFinish must be true;
        showWindowsCheckBox.Checked += (sender, e) =>
        {
            exitAfterFinishCheckBox.IsEnabled = true;
        };
        showWindowsCheckBox.Unchecked += (sender, e) =>
        {
            exitAfterFinishCheckBox.IsChecked = true;
            exitAfterFinishCheckBox.IsEnabled = false;
        };
        stackPanel.Children.Add(parametersTextBlock);
        stackPanel.Children.Add(showWindowsCheckBox);
        stackPanel.Children.Add(exitAfterFinishCheckBox);

        return new AccessPageItemDialogControlStruct(stackPanel, infoBar, nameTextBox, commandTextBox, showWindowsCheckBox, exitAfterFinishCheckBox);
    }
}
