using System.Diagnostics;
using LHMSupport;
using HardwareStruct;
using Nvml;
using System.Globalization;
using performance_monitor_winui3.Tools;
using HardwareInfoSupport;


namespace Information;

// TotalInformation
// use to get information from every monitor models.
static class TotalInformation
{
    static readonly PerformanceCounter bootTimeCounter;
    static readonly LHMReader LibreReader;
    static readonly HardwareInfoReader HIReader;
    static readonly NvidiaSmi nvidiaSmi;

    public static List<CpuInformation> CPU => LibreReader.CPU;

    public static List<GpuInformation> GPU;
    public static List<GpuInformation> OtherGPU => LibreReader.GPU;
    public static List<GpuInformation> NvidiaGPU => nvidiaSmi.GPU;

    public static NetworkSpeedInformation NetworkSpeed => LibreReader.Network;
    public static List<NetworkInformation> Network => HIReader.Network;

    public static List<BatteryInformation> Battery => LibreReader.Battery;

    public static List<DiskInformation> Disk => LibreReader.Disk;

    public static List<MemoryInformation> Memory => LibreReader.Memory;

    public static List<KeyValuePair<string, string>> Outline;

    public static List<KeyValuePair<string, string>> RunTimeInfo;

    static TotalInformation()
    {
        bootTimeCounter = new PerformanceCounter("System", "System Up Time");
        bootTimeCounter.NextValue();
        LibreReader = new LHMReader();
        HIReader = new HardwareInfoReader();

        GPU = new List<GpuInformation>(8);

        Outline = new List<KeyValuePair<string, string>>(16);
        RunTimeInfo = new List<KeyValuePair<string, string>>(16);

        nvidiaSmi = NvidiaSmi.GetInstance();
    }

    private static void OutlineUpdate()
    {
        Outline.Clear();
        var now = DateTime.Now;
        var currentDateString = now.ToString("MMMM dd, yyyy", CultureInfo.CreateSpecificCulture("en-US"));
        var currentTimeString = now.ToString("HH:mm:ss");
        var bootTime = Utils.TimeSpan2String(TimeSpan.FromSeconds(bootTimeCounter.NextValue()));
        
        Outline.Add(new KeyValuePair<string, string>("Time", currentTimeString));
        Outline.Add(new KeyValuePair<string, string>("Boot Time", bootTime));
        Outline.Add(new KeyValuePair<string, string>("Date", currentDateString));
    }

    private static void RunTimeInfoUpdate()
    {
        RunTimeInfo.Clear();
        var processName = Process.GetCurrentProcess().ProcessName;
        var ps = Process.GetProcessesByName(processName).FirstOrDefault();
        if (ps != null)
        {
            RunTimeInfo.Add(new KeyValuePair<string, string>("WorkThreadCount", Convert.ToString(ps.Threads.Count)));
            RunTimeInfo.Add(new KeyValuePair<string, string>("WorkMemory", string.Format("{0:f5}", Utils.ByteFormat(ps.WorkingSet64, Utils.Unit.BYTE))));
        }
    }

    private static void GpuUpdate()
    {
        // get info from Nvml
        nvidiaSmi.Update();

        GPU.Clear();

        GPU.AddRange(NvidiaGPU);
        GPU.AddRange(OtherGPU);
    }


    // before using Lists, plz update first.
    public static bool Update()
    {
        // get info from LHM
        LibreReader.Update();

        GpuUpdate();

        // get Outline
        OutlineUpdate();

        // get RunTimeInfo
        RunTimeInfoUpdate();

        return true;
    }

    public static bool UpdateNetwork()
    {
        // get info from HI
        HIReader.Update();

        return true;
    }
}
