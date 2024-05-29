using System.Diagnostics;
using AWCC;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using performance_monitor_winui3.Tools;
using performance_monitor_winui3.ViewModels;

namespace performance_monitor_winui3.Views;

public sealed partial class OverclockPage : Page
{
    private readonly DispatcherTimer? timer;
    private static readonly Semaphore timerNotRunning = new Semaphore(1, 1);

    public static int FanRPMBound = 6000;
    public static uint MaxRPMPercent = 180;

    public static uint PreCpuFanRPMPercent = 0, PreGpuFanRPMPercent = 0;

    public OverclockViewModel ViewModel
    {
        get;
    }

    public OverclockPage()
    {
        ViewModel = App.GetService<OverclockViewModel>();
        InitializeComponent();

        if (!AWCCWMI.IsAvailable())
        {
            ErrorInfoBar.Message = AWCCWMI.ErrorMsg;
            ErrorInfoBar.IsOpen = true;

            ModeSelect.IsEnabled = false;
            return;
        }

        ModeCheck();

        CpuFanSlider.Value = PreCpuFanRPMPercent;
        GpuFanSlider.Value = PreGpuFanRPMPercent;

        {
            timer = new DispatcherTimer();
            // System.Timers.Timer and System.Windows.Threading.DispatcherTimer
            // the back is running at UI Threading, so it can change the controls.
            timer.Tick += async (object? sender, object? e) =>
            {
                await PageUpdate();
            };
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Start();
        }
    }

    public async Task PageUpdate()
    {
        if (!AWCCWMI.IsAvailable()) return;

        // other thread is running!
        if (timerNotRunning.WaitOne(0) == false)
        {
            return;
        }

        var CpuFanRPM = await Task.Run(() => AWCCWMI.GetFanRPM(AWCCWMI.CpuFanId));
        var GpuFanRPM = await Task.Run(() => AWCCWMI.GetFanRPM(AWCCWMI.GpuFanId));
        var CpuSensorValue = await Task.Run(() => AWCCWMI.GetSensorTemperature(AWCCWMI.CpuSensorId));
        var GpuSensorValue = await Task.Run(() => AWCCWMI.GetSensorTemperature(AWCCWMI.GpuSensorId));

        CpuFanRPM = CpuFanRPM == AWCCWMI.FailureCode ? 0 : CpuFanRPM;
        GpuFanRPM = GpuFanRPM == AWCCWMI.FailureCode ? 0 : GpuFanRPM;
        CpuSensorValue = CpuSensorValue == AWCCWMI.FailureCode ? 0 : CpuSensorValue;
        CpuSensorValue = CpuSensorValue == AWCCWMI.FailureCode ? 0 : CpuSensorValue;
        GpuSensorValue = GpuSensorValue == AWCCWMI.FailureCode ? 0 : GpuSensorValue;
        GpuSensorValue = GpuSensorValue == AWCCWMI.FailureCode ? 0 : GpuSensorValue;

        CpuTmpGauge.Value = CpuSensorValue;
        CpuFanGauge.Value = CpuFanRPM;

        GpuTmpGauge.Value = GpuSensorValue;
        GpuFanGauge.Value = GpuFanRPM;

        ModeCheck();

        timerNotRunning.Release();
    }

    public async void ModeButton(object sender, RoutedEventArgs e)
    {
        if (!AWCCWMI.IsAvailable()) return;
        if (ModeSelect.SelectedIndex == -1 || OverclockViewModel.Index2Mode[ModeSelect.SelectedIndex] == AWCCWMI.Mode) return;
        
        if (AWCCWMI.ApplyThermalMode(OverclockViewModel.Index2Mode[ModeSelect.SelectedIndex]))
        { 
            ModeSelect.IsEnabled = false;
            await Task.Run(() =>
            {
                Thread.Sleep(1000);
                return true;
            });
            ModeSelect.IsEnabled = true;

            Utils.SimpleToast(
                StringResource.Get("OverclockPage_SwitchMode"),
                StringResource.Get("OverclockPage_ModeSwitchTo") + OverclockViewModel.Index2Mode[ModeSelect.SelectedIndex].ToString()
            );
        }
        ModeCheck();
    }

    public void ModeCheck()
    {
        if (AWCCWMI.Mode == ThermalMode.Custom)
        {
            CpuFanSlider.IsEnabled = true;
            GpuFanSlider.IsEnabled = true;
            SliderGrid.Visibility = Visibility.Visible;
        }
        else
        {
            CpuFanSlider.IsEnabled = false;
            GpuFanSlider.IsEnabled = false;
            SliderGrid.Visibility = Visibility.Collapsed;
        }
        ModeSelect.SelectedIndex = OverclockViewModel.Mode2Index[AWCCWMI.Mode];
    }

    public async void CpuFanPercentSet(object sender, RoutedEventArgs e)
    {
        var slider = sender as Slider;
        slider.IsEnabled = false;

        var value = (uint)Math.Min(Math.Max(slider.Value, 0), MaxRPMPercent);
        if (AWCCWMI.SetAddonSpeedPercent(AWCCWMI.CpuFanId, value))
        {
            PreCpuFanRPMPercent = value;
        }
        else
        {
            CpuFanSlider.Value = PreCpuFanRPMPercent;
        }

        await Task.Run(() =>
        {
            Thread.Sleep(500);
            return true;
        });

        slider.IsEnabled = true;
    }

    public async void GpuFanPercentSet(object sender, RoutedEventArgs e)
    {
        var slider = sender as Slider;
        slider.IsEnabled = false;

        var value = (uint)Math.Min(Math.Max(slider.Value, 0), MaxRPMPercent);
        if(AWCCWMI.SetAddonSpeedPercent(AWCCWMI.GpuFanId, value))
        {
            PreGpuFanRPMPercent = value;
        }
        else
        {
            GpuFanSlider.Value = PreGpuFanRPMPercent;
        }
        

        await Task.Run(() =>
        {
            Thread.Sleep(500);
            return true;
        });

        slider.IsEnabled = true;
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        if (timer != null)
        {
            timer.Stop();
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (timer != null)
        {
            timer.Start();
        }
    }
}
