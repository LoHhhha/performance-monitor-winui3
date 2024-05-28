using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Windows.UI.Text;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Documents;
using Windows.UI;
using Windows.Foundation.Collections;
using Hardware.Info;
using HardwareStruct;
using Windows.Security.Cryptography.Certificates;
using Microsoft.Windows.AppNotifications;
using WinUIEx.Messaging;

namespace performance_monitor_winui3.Tools;

static class Utils
{

    // Help to change unit and return a string
    // E.g.: Utils.ByteFormat(1024,Unit.BYTE) will return "1.00MiB"
    // E.g.: Utils.ByteFormat(1024,Unit.GiB) will return "1.00TiB"
    public static int baseNumber = 1024;
    public enum Unit
    {
        BYTE, KiB, MiB, GiB, TiB, PiB
    }

    private static readonly string[] postfix = ["Byte", "KiB", "MiB", "GiB", "TiB", "PiB"];
    public static string ByteFormat(double value, Unit unit)
    {
        var idx = (int)unit;
        while (idx < postfix.Length && value >= baseNumber)
        {
            value /= baseNumber;
            idx++;
        }
        return string.Format("{0:0.##}{1}", value, postfix[idx]);
    }

    // TimeSpan=>string
    // E.g.: from TimeSpan to "2days, 10:00:12"
    public static string TimeSpan2String(TimeSpan value)
    {
        if (value.Days > 0)
        {
            if (value.Days > 1)
            {
                return string.Format("{0}days, {1:D2}:{2:D2}:{3:D2}", value.Days, value.Hours, value.Minutes, value.Seconds);
            }
            return string.Format("{0}day, {1:D2}:{2:D2}:{3:D2}", value.Days, value.Hours, value.Minutes, value.Seconds);
        }
        return string.Format("{0:D2}:{1:D2}:{2:D2}", value.Hours, value.Minutes, value.Seconds);
    }

    // Add a title TextBlock to given grid, and return Title TextBlock.
    public static TextBlock AddTitle2Grid(Grid grid, string text, int row, Color fontColor, int fontSize = 16, double height = 1.2)
    {
        var rowDef = new RowDefinition();
        rowDef.Height = new GridLength(height, GridUnitType.Auto);
        grid.RowDefinitions.Add(rowDef);

        var title = new TextBlock();
        title.Text = text;
        title.VerticalAlignment = VerticalAlignment.Bottom;
        title.FontSize = fontSize;
        title.TextWrapping = TextWrapping.Wrap;
        // title.FontWeight = FontWeights.Bold;
        title.Foreground = new SolidColorBrush(fontColor);  // such Color.FromArgb(255, 170, 170, 0)
        title.FontFamily = new FontFamily("Cascadia Mono");

        Grid.SetRow(title, row);
        Grid.SetColumn(title, 0);

        grid.Children.Add(title);

        return title;
    }

    // Add a pair as a grid to given grid, and return key and value TextBlock.
    // keySize + valueSize = 10, is what need to be confirm. It use to change the size of the key and value TextBlock.
    public static Tuple<TextBlock, TextBlock> AddPairGrid2Grid(Grid grid, string keyStr, string valueStr, int row, int keySize = 4, int valueSize = 6, int fontSize = 14, double height = 1.2)
    {
        // define the sub grid
        var subGrid = new Grid();
        // column
        var keyColumnDefinition = new ColumnDefinition();
        keyColumnDefinition.Width = new GridLength(keySize, GridUnitType.Star);
        var valueColumnDefinition = new ColumnDefinition();
        valueColumnDefinition.Width = new GridLength(valueSize, GridUnitType.Star);
        subGrid.ColumnDefinitions.Add(keyColumnDefinition);
        subGrid.ColumnDefinitions.Add(valueColumnDefinition);
        // row
        var pairRowDefinition = new RowDefinition();
        pairRowDefinition.Height = new GridLength(height, GridUnitType.Auto);
        subGrid.RowDefinitions.Add(pairRowDefinition);

        // define key TextBlock
        var key = new TextBlock();
        key.Text = keyStr;
        key.VerticalAlignment = VerticalAlignment.Top;
        key.FontSize = fontSize;
        key.FontFamily = new FontFamily("Cascadia Mono");
        Grid.SetColumn(key, 0);
        Grid.SetRow(key, 0);
        // define value TextBlock
        var value = new TextBlock();
        value.Text = valueStr;
        value.VerticalAlignment = VerticalAlignment.Top;
        value.FontSize = fontSize;
        value.FontFamily = new FontFamily("Cascadia Mono");
        Grid.SetColumn(value, 1);
        Grid.SetRow(value, 0);

        // push into subGrid
        subGrid.Children.Add(key);
        subGrid.Children.Add(value);

        // grid row
        var subGridRowDefinition = new RowDefinition();
        subGridRowDefinition.Height = new GridLength(height, GridUnitType.Auto);
        grid.RowDefinitions.Add(subGridRowDefinition);

        // push into grid
        Grid.SetRow(subGrid, row);
        Grid.SetColumn(subGrid, 0);
        grid.Children.Add(subGrid);

        return new Tuple<TextBlock, TextBlock>(key, value);
    }

    // Add a pair as a grid to given grid with progressBar, and return key and value TextBlock.
    // keySize + progressBarSize + valueSize = 10, is what need to be confirm. It use to change the size of the key and value TextBlock.
    // the valun must be [0,100]!
    public static Tuple<TextBlock, TextBlock> AddPairWithProgressBarGrid2Grid(Grid grid, string keyStr, int val, int row, string? showValue = null, int keySize = 4, int progressBarSize = 5, int valueSize = 1, int fontSize = 14, double height = 1.2)
    {
        // define the sub grid
        var subGrid = new Grid();
        // column
        var keyColumnDefinition = new ColumnDefinition();
        keyColumnDefinition.Width = new GridLength(keySize, GridUnitType.Star);
        var progressBarColumnDefinition = new ColumnDefinition();
        progressBarColumnDefinition.Width = new GridLength(progressBarSize, GridUnitType.Star);
        var valueColumnDefinition = new ColumnDefinition();
        valueColumnDefinition.Width = new GridLength(valueSize, GridUnitType.Star);
        subGrid.ColumnDefinitions.Add(keyColumnDefinition);
        subGrid.ColumnDefinitions.Add(progressBarColumnDefinition);
        subGrid.ColumnDefinitions.Add(valueColumnDefinition);
        // row
        var pairRowDefinition = new RowDefinition();
        pairRowDefinition.Height = new GridLength(height, GridUnitType.Auto);
        subGrid.RowDefinitions.Add(pairRowDefinition);

        // define key TextBlock
        var key = new TextBlock();
        key.Text = keyStr;
        key.VerticalAlignment = VerticalAlignment.Top;
        key.FontSize = fontSize;
        key.FontFamily = new FontFamily("Cascadia Mono");
        Grid.SetRow(key, 0);
        Grid.SetColumn(key, 0);

        var progressBar = new ProgressBar();
        SetProgressBar(progressBar, val);

        Grid.SetRow(progressBar, 0);
        Grid.SetColumn(progressBar, 1);

        // define value TextBlock
        var value = new TextBlock();
        if (showValue != null)
        {
            value.Text = showValue;
        }
        else
        {
            value.Text = $"{val}%";
        }
        value.VerticalAlignment = VerticalAlignment.Top;
        value.FontSize = fontSize;
        value.FontFamily = new FontFamily("Cascadia Mono");
        value.HorizontalTextAlignment = TextAlignment.Right;
        Grid.SetRow(value, 0);
        Grid.SetColumn(value, 2);

        // push into subGrid
        subGrid.Children.Add(key);
        subGrid.Children.Add(progressBar);
        subGrid.Children.Add(value);

        // grid row
        var subGridRowDefinition = new RowDefinition();
        subGridRowDefinition.Height = new GridLength(height, GridUnitType.Auto);
        grid.RowDefinitions.Add(subGridRowDefinition);

        // push into grid
        Grid.SetRow(subGrid, row);
        Grid.SetColumn(subGrid, 0);
        grid.Children.Add(subGrid);

        return new Tuple<TextBlock, TextBlock>(key, value);
    }

    public static ProgressRing AddLoadingProgressBar2Grid(Grid grid, int row)
    {
        var progressRing = new ProgressRing();
        progressRing.IsIndeterminate = true;
        progressRing.HorizontalAlignment = HorizontalAlignment.Stretch;
        progressRing.VerticalAlignment = VerticalAlignment.Top;


        // grid row
        var rowDefinition = new RowDefinition();
        grid.RowDefinitions.Add(rowDefinition);

        // push into grid
        Grid.SetRow(progressRing, row);
        Grid.SetColumn(progressRing, 0);
        grid.Children.Add(progressRing);

        return progressRing;
    }

    public static void SetProgressBar(ProgressBar progressBar, int value)
    {
        value = value < 0 ? 0 : value;
        value = value > 100 ? 100 : value;
        progressBar.Value = value;
        if (value < 30)
        {
            progressBar.ShowPaused = false;
            progressBar.ShowError = false;
        }
        else
        {
            progressBar.ShowPaused = true;
            progressBar.ShowError = false;
        }
    }

    public static readonly char[] ProgressChars = [' ', '▏', '▎', '▎', '▌', '▋', '▊', '▉', '█'];
    public static string GetProgressString(int value)
    {
        value = Math.Max(Math.Min(value, 100), 0);
        var idx = value * (ProgressChars.Length - 1) / 100;
        value = Math.Min(99, value);
        return $"[{ProgressChars[idx]}{value,2}%]";
    }
}
