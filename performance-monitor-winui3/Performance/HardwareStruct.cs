namespace HardwareStruct;


public struct CpuInformation
{
    public string Name = "";
    public int[] Usage;
    public int MaxUsage = -1;
    public int TotalUsage = -1;
    public int[] Freq;
    public int Temperature = -1;    // C
    public int Power = -1;          // W
    public int CoreCount = -1;
    public int ThreadCount = -1;
    public CpuInformation()
    {
        Usage = new int[128];
        Freq = new int[64];
    }
}

public struct GpuInformation
{
    public string Name = "";
    public int Usage = -1;
    public int VideoUsage = -1;
    public int Temperature = -1;    // C
    public int MemoryTotal = -1;    // MB
    public int MemoryUsed = -1;     // MB
    public int CoreFreq = -1;
    public int MemoryFreq = -1;
    public int Power = -1;          // W
    public int LimitPower = -1;     // W
    public int PCIeTx = -1;         // Kibps
    public int PCIeRx = -1;         // Kibps
    public GpuInformation() { }
}

public struct NetworkSpeedInformation
{
    public long UploadSpeed = 0;    // Bps
    public long DownloadSpeed = 0;  // Bps
    public NetworkSpeedInformation() { }
    public void Clear()
    {
        UploadSpeed = 0; DownloadSpeed = 0;
    }
}

public struct NetworkInformation
{
    public string Name = "";
    public List<string> IPv4 = [];
    public List<string> IPv6 = [];
    public string Mac = "";
    public string DHCPServer = "";
    public long Speed = -1;          // bps
    public NetworkInformation() { }
}

public struct BatteryInformation
{
    public string Name = "";
    public float Level = -1;
    public float Voltage = -1;
    public BatteryInformation() { }
}
public struct DiskInformation
{
    public string Name = "";
    public int Temperature = -1;        // C
    public DiskInformation() { }
}

public struct MemoryInformation
{
    public string Name = "";
    public int Used = -1;               // MB
    public int Available = -1;          // MB
    public int VirtualUsed = -1;        // MB
    public int VirtualAvailable = -1;   // MB
    public MemoryInformation() { }
}