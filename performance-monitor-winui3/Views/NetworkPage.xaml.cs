using Information;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using performance_monitor_winui3.ViewModels;
using Windows.UI;
using performance_monitor_winui3.Tools;
using Microsoft.UI.Xaml.Navigation;


namespace performance_monitor_winui3.Views;

public enum NetworkInfoType
{
    Network
}

public sealed partial class NetworkPage : Page
{
    private readonly DispatcherTimer timer;
    private static readonly Semaphore timerNotRunning = new Semaphore(1, 1);

    public readonly List<Func<bool>> setter;
    public List<NetworkInfoType> selectedTypes;
    int rowCount = 0;
    Color themeColor = Color.FromArgb(255, 165, 165, 0);
    public NetworkViewModel ViewModel
    {
        get;
    }

    public NetworkPage()
    {
        ViewModel = App.GetService<NetworkViewModel>();
        this.InitializeComponent();

        // wait from data
        Utils.AddLoadingProgressBar2Grid(DisplayGrid, rowCount++);

        // setter
        {
            setter = [];

            // Network
            setter.Add(() =>
            {
                foreach (var network in TotalInformation.Network)
                {
                    Utils.AddTitle2Grid(DisplayGrid, network.Name, rowCount++, themeColor);

                    Utils.AddPairGrid2Grid(DisplayGrid, "Mac", network.Mac, rowCount++, keySize: 25, valueSize: 75);
                    foreach (var ip in network.IPv4)
                    {
                        Utils.AddPairGrid2Grid(DisplayGrid, "IPv4", ip, rowCount++, keySize: 25, valueSize: 75);
                    }

                    foreach (var ip in network.IPv6)
                    {
                        Utils.AddPairGrid2Grid(DisplayGrid, "IPv6", ip, rowCount++, keySize: 25, valueSize: 75);
                    }

                    Utils.AddPairGrid2Grid(DisplayGrid, "DHCPServer", network.DHCPServer, rowCount++, keySize: 25, valueSize: 75);

                    Utils.AddPairGrid2Grid(DisplayGrid, "Speed", $"{network.Speed/1000000}Mbps", rowCount++, keySize: 25, valueSize: 75);

                    Utils.AddPairGrid2Grid(DisplayGrid, "", "", rowCount++, keySize: 25, valueSize: 75);
                }

                return true;
            });
        }

        // selectedTypes
        {
            selectedTypes = [NetworkInfoType.Network];
        }

        // timer
        {
            timer = new DispatcherTimer();
            // System.Timers.Timer and System.Windows.Threading.DispatcherTimer
            // the back is running at UI Threading, so it can change the controls.
            timer.Tick += async (object? sender, object? e) =>
            {
                await PageUpdate();
            };
            timer.Interval = TimeSpan.FromMilliseconds(2000);
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
        timer.Start();
    }

    public async Task PageUpdate()
    {
        // other thread is running!
        if (timerNotRunning.WaitOne(0) == false)
        {
            return;
        }

        var res = await Task.Run(TotalInformation.UpdateNetwork);
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
}
