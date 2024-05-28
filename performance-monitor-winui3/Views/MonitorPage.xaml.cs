using Information;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using performance_monitor_winui3.ViewModels;
using Windows.UI;
using performance_monitor_winui3.Tools;
using performance_monitor_winui3.Models;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI;
using Windows.Storage;
using System.Diagnostics;

namespace performance_monitor_winui3.Views;


public sealed partial class MonitorPage : Page
{
    public static List<MonitorInfoType> selectedTypes
    {
        get => MonitorViewModel._selectedTypes;
        set => MonitorViewModel.SetSelectedTypes(value);
    }

    private readonly DispatcherTimer timer;
    private uint timerInterval = MonitorViewModel.TimerIntervalDefaultValue;
    private static readonly Semaphore timerNotRunning = new Semaphore(1, 1);

    private readonly List<Func<bool>> setter;

    int rowCount = 0;
    Color themeColor = Color.FromArgb(255, 165, 165, 0);

    public MonitorViewModel ViewModel
    {
        get;
    }

    public MonitorPage()
    {
        ViewModel = App.GetService<MonitorViewModel>();
        InitializeComponent();

        // wait from data
        Utils.AddLoadingProgressBar2Grid(DisplayGrid, rowCount++);

        // setter
        {
            setter = [];

            // Outline
            setter.Add(() =>
            {
                Utils.AddTitle2Grid(DisplayGrid, MonitorInfoType.Outline.ToString(), rowCount++, themeColor);

                foreach (var pair in TotalInformation.Outline)
                {
                    Utils.AddPairGrid2Grid(DisplayGrid, pair.Key, pair.Value, rowCount++);
                }

                return true;
            });

            // CPU
            setter.Add(() =>
            {
                // Utils.AddTitle(DisplayGrid, rowCount++, InfoType.CPU.ToString());

                foreach (var cpu in TotalInformation.CPU)
                {
                    Utils.AddTitle2Grid(DisplayGrid, cpu.Name, rowCount++, themeColor);

                    Utils.AddPairWithProgressBarGrid2Grid(DisplayGrid, "Temperature", cpu.Temperature, rowCount++, $"{cpu.Temperature}C");

                    Utils.AddPairWithProgressBarGrid2Grid(DisplayGrid, "Usage", cpu.TotalUsage, rowCount++);
                    Utils.AddPairWithProgressBarGrid2Grid(DisplayGrid, "Max Usage", cpu.MaxUsage, rowCount++);

                    var eachUsage = "";
                    for (int i = 0; i < cpu.ThreadCount; i++)
                    {
                        eachUsage += Utils.GetProgressString(cpu.Usage[i]);
                        if (i + 1 != cpu.ThreadCount && i % 4 == 3)
                        {
                            eachUsage += '\n';
                        }
                        else
                        {
                            eachUsage += ' ';
                        }
                    }
                    Utils.AddPairGrid2Grid(DisplayGrid, "Thread Usage", eachUsage, rowCount++);
                    //Utils.AddCPUThreadUsageGrid2Grid(DisplayGrid, "Thread Usage", cpu, rowCount++);

                    Utils.AddPairGrid2Grid(DisplayGrid, "Power", $"{cpu.Power}W", rowCount++);

                    int maxFreq = int.MinValue, minFreq = int.MaxValue;
                    for(int i = 0; i < cpu.CoreCount; i+=2)
                    {
                        maxFreq = Math.Max(cpu.Freq[i], maxFreq);
                        minFreq = Math.Min(cpu.Freq[i], minFreq);
                    }
                    Utils.AddPairGrid2Grid(DisplayGrid, "Freq", $"{maxFreq}MHz", rowCount++);
                }

                return true;
            });

            // Memory
            setter.Add(() => 
            {
                Utils.AddTitle2Grid(DisplayGrid, "Memory", rowCount++, themeColor);
                foreach (var memory in TotalInformation.Memory)
                {
                    var memoryTotal = memory.Available + memory.Used;
                    Utils.AddPairWithProgressBarGrid2Grid(DisplayGrid, "Physical Usage", memory.Used * 100 / memoryTotal, rowCount++);
                    var virtualTotal = memory.VirtualAvailable + memory.VirtualUsed;
                    Utils.AddPairWithProgressBarGrid2Grid(DisplayGrid, "Virtual Usage", memory.VirtualUsed * 100 / virtualTotal, rowCount++);

                    Utils.AddPairGrid2Grid(DisplayGrid, "Physical", $"{memory.Used}MiB -> {memoryTotal}MiB", rowCount++);
                    Utils.AddPairGrid2Grid(DisplayGrid, "Virtual", $"{memory.VirtualUsed}MiB -> {virtualTotal}MiB", rowCount++);
                }
                return true; 
            });

            // GPU
            setter.Add(() =>
            {
                foreach (var gpu in TotalInformation.GPU)
                {
                    Utils.AddTitle2Grid(DisplayGrid, gpu.Name, rowCount++, themeColor);

                    if (gpu.Temperature != -1) Utils.AddPairWithProgressBarGrid2Grid(DisplayGrid, "Temperature", gpu.Temperature, rowCount++, $"{gpu.Temperature}C");
                    
                    if (gpu.MemoryUsed != -1 && gpu.MemoryTotal != -1)
                    {
                        var memoryUsage = gpu.MemoryUsed * 100 / gpu.MemoryTotal;
                        Utils.AddPairWithProgressBarGrid2Grid(DisplayGrid, "Memory Usage", memoryUsage, rowCount++);
                    }

                    if (gpu.Usage != -1) Utils.AddPairWithProgressBarGrid2Grid(DisplayGrid, "Usage", gpu.Usage, rowCount++);
                    if (gpu.VideoUsage != -1) Utils.AddPairWithProgressBarGrid2Grid(DisplayGrid, "Video Usage", gpu.VideoUsage, rowCount++);

                    if (gpu.CoreFreq != -1) Utils.AddPairGrid2Grid(DisplayGrid, "Core Freq", $"{gpu.CoreFreq}Mhz", rowCount++);
                    if (gpu.MemoryFreq != -1) Utils.AddPairGrid2Grid(DisplayGrid, "Memory Freq", $"{gpu.MemoryFreq}Mhz", rowCount++);

                    if (gpu.Power != -1 && gpu.LimitPower != -1) 
                    {
                        Utils.AddPairGrid2Grid(DisplayGrid, "Power", $"{gpu.Power}W -> {gpu.LimitPower}W", rowCount++);
                    }
                    else if(gpu.Power != -1)
                    {
                        Utils.AddPairGrid2Grid(DisplayGrid, "Power", $"{gpu.Power}W", rowCount++);
                    }

                    if (gpu.MemoryUsed != -1 && gpu.MemoryTotal != -1)
                    {
                        Utils.AddPairGrid2Grid(DisplayGrid, "Memory", $"{gpu.MemoryUsed}MiB -> {gpu.MemoryTotal}MiB", rowCount++);
                    }
                    else if(gpu.MemoryUsed != -1)
                    {
                        Utils.AddPairGrid2Grid(DisplayGrid, "Memory Used", $"{gpu.MemoryUsed}MiB", rowCount++);
                    }

                    if (gpu.PCIeTx != -1&&gpu.PCIeRx != -1) {
                        var transfer = gpu.PCIeRx + gpu.PCIeTx;
                        if (gpu.PCIeTx != -1) Utils.AddPairGrid2Grid(DisplayGrid, "Transfer", Utils.ByteFormat(transfer, Utils.Unit.KiB) + "ps", rowCount++);
                    }
                }

                return true;
            });

            // Network
            setter.Add(() =>
            {
                Utils.AddTitle2Grid(DisplayGrid, MonitorInfoType.Network.ToString(), rowCount++, themeColor);

                Utils.AddPairGrid2Grid(DisplayGrid, "Upload", Utils.ByteFormat(TotalInformation.NetworkSpeed.UploadSpeed, Utils.Unit.BYTE) + "ps", rowCount++);
                Utils.AddPairGrid2Grid(DisplayGrid, "Download", Utils.ByteFormat(TotalInformation.NetworkSpeed.DownloadSpeed, Utils.Unit.BYTE) + "ps", rowCount++);

                return true;
            });

            // Battery
            setter.Add(() =>
            {
                Utils.AddTitle2Grid(DisplayGrid, MonitorInfoType.Battery.ToString(), rowCount++, themeColor);

                foreach (var battery in TotalInformation.Battery)
                {
                    Utils.AddPairGrid2Grid(DisplayGrid, "Level", $"{battery.Level:f2}%", rowCount++);
                    Utils.AddPairGrid2Grid(DisplayGrid, "Voltage", $"{battery.Voltage:f2}V", rowCount++);
                }

                return true;
            });

            // Disk
            setter.Add(() =>
            {
                Utils.AddTitle2Grid(DisplayGrid, MonitorInfoType.Disk.ToString(), rowCount++, themeColor);

                foreach (var disk in TotalInformation.Disk)
                {
                    Utils.AddTitle2Grid(DisplayGrid, disk.Name, rowCount++, themeColor);

                    Utils.AddPairGrid2Grid(DisplayGrid, "Temperature", $"{disk.Temperature}C", rowCount++);
                }

                return true;
            });

            // RunTimeInfo
            setter.Add(() =>
            {
                Utils.AddTitle2Grid(DisplayGrid, MonitorInfoType.RunTimeInfo.ToString(), rowCount++, themeColor);

                foreach (var info in TotalInformation.RunTimeInfo)
                {
                    Utils.AddPairGrid2Grid(DisplayGrid, info.Key, info.Value, rowCount++);
                }

                return true;
            });
        }

        // timer
        {
            timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(MonitorViewModel.TimerIntervalDefaultValue) };
            // System.Timers.Timer and System.Windows.Threading.DispatcherTimer
            // the back is running at UI Threading, so it can change the controls.
            timer.Tick += async (object? sender, object? e) =>
            {
                await PageUpdate();
            };
            TimerIntervalUpdate();
            timer.Start();
        }
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        timer.Stop();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        TimerIntervalUpdate();
        timer.Start();
    }

    private void TimerIntervalUpdate()
    {
        try
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            var newTimerInterval = (uint)localSettings.Values[MonitorViewModel.TimerIntervalKey];
            timerInterval = newTimerInterval;
            timer.Interval = TimeSpan.FromMilliseconds(newTimerInterval);
        }
        catch
        {
            timer.Interval = TimeSpan.FromMilliseconds(timerInterval);
        }
    }

    public async Task PageUpdate()
    {
        // other thread is running!
        if (timerNotRunning.WaitOne(0) == false)
        {
            return;
        }

        var res = await Task.Run(TotalInformation.Update);
        if (!res) return;

        DisplayGrid.Children.Clear();
        DisplayGrid.RowDefinitions.Clear();
        rowCount = 0;

        foreach (var type in selectedTypes)
        {
            setter[(int)type]();
            Utils.AddPairGrid2Grid(DisplayGrid, "", "", rowCount++);
        }

        timerNotRunning.Release();
    }

    // update item selected or display order.
    public static void SetSelectedTypes(List<MonitorInfoType> selected)
    {
        selectedTypes = selected;
    }
}
