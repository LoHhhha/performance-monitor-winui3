using System.Runtime.InteropServices;
using HardwareStruct;

namespace Nvml;

// https://docs.nvidia.com/deploy/nvml-api/
using nvmlDevice_t = IntPtr;
public enum nvmlReturn_t
{
    Success = 0,
    ErrorUninitialized = 1,
    ErrorInvalidArgument = 2,
    ErrorNotSupported = 3,
    ErrorNoPermission = 4,
    ErrorAlreadyInitialized = 5,
    ErrorNotFound = 6,
    ErrorInsufficientSize = 7,
    ErrorInsufficientPower = 8,
    ErrorDriverNotLoaded = 9,
    ErrorTimeout = 10,
    ErrorIrqIssue = 11,
    ErrorLibraryNotFound = 12,
    ErrorFunctionNotFound = 13,
    ErrorCorruptedInforom = 14,
    ErrorGpuIsLost = 15,
    ErrorResetRequired = 16,
    ErrorOperatingSystem = 17,
    ErrorLibRmVersionMismatch = 18,
    ErrorInUse = 19,
    ErrorMemory = 20,
    ErrorNoData = 21,
    ErrorVgpuEccNotSupported = 22,
    ErrorInsufficientResources = 23,
    ErrorFreqNotSupported = 24,
    ErrorArgumentVersionMismatch = 25,
    ErrorDeprecated = 26,
    ErrorNotReady = 27,
    ErrorGpuNotFound = 28,
    ErrorInvalidState = 29,
    ErrorUnknown = 999,
}

public enum nvmlTemperatureSensors_t
{
    // Temperature sensor for the GPU die.
    TemperatureGpu = 0,

    TemperatureCount = 1,
}

public enum nvmlPcieUtilCounter_t
{ 
    // PCIe utilization transmit (TX) bytes counter.  
    NVML_PCIE_UTIL_TX_BYTES = 0,
    // PCIe utilization receive (RX) bytes counter.  
    NVML_PCIE_UTIL_RX_BYTES = 1,

    NVML_PCIE_UTIL_COUNT
}

public enum nvmlClockType_t
{
    // Graphics clock domain.  
    NVML_CLOCK_GRAPHICS = 0,
    // SM clock domain.  
    NVML_CLOCK_SM = 1,
    // Memory clock domain.
    NVML_CLOCK_MEM = 2,
    // Video encoder/decoder clock domain. 
    NVML_CLOCK_VIDEO = 3,

    NVML_CLOCK_COUNT = 4,
}

[StructLayout(LayoutKind.Sequential)]
public struct nvmlMemory_t
{
    // all in bytes.
    public ulong total;
    public ulong free;
    public ulong used;
}

[StructLayout(LayoutKind.Sequential)]
public struct nvmlUtilization_t
{
    // percent of time over the past sample period during which...
    public uint gpu;
    public uint memory;
}

public class NvmlApi
{
    [DllImport("nvml.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern nvmlReturn_t nvmlInit_v2();

    [DllImport("nvml.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern nvmlReturn_t nvmlShutdown();

    [DllImport("nvml.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern nvmlReturn_t nvmlDeviceGetCount_v2(out uint deviceCount);

    [DllImport("nvml.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern nvmlReturn_t nvmlDeviceGetHandleByIndex_v2(uint index, out nvmlDevice_t device);

    [DllImport("nvml.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern nvmlReturn_t nvmlDeviceGetMemoryInfo(nvmlDevice_t device, out nvmlMemory_t memory);

    [DllImport("nvml.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern nvmlReturn_t nvmlDeviceGetUtilizationRates(nvmlDevice_t device, out nvmlUtilization_t utilization);

    [DllImport("nvml.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern nvmlReturn_t nvmlDeviceGetEncoderUtilization(nvmlDevice_t device, out uint utilization, out uint samplingPeriodUs);

    [DllImport("nvml.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern nvmlReturn_t nvmlDeviceGetDecoderUtilization(nvmlDevice_t device, out uint utilization, out uint samplingPeriodUs);

    [DllImport("nvml.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern nvmlReturn_t nvmlDeviceGetPowerUsage(nvmlDevice_t device, out uint power);     // mW

    [DllImport("nvml.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern nvmlReturn_t nvmlDeviceGetEnforcedPowerLimit(nvmlDevice_t device, out uint limit);     // mW

    [DllImport("nvml.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern nvmlReturn_t nvmlDeviceGetTemperature(nvmlDevice_t device, nvmlTemperatureSensors_t sensorType, out uint temp);

    [DllImport("nvml.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern nvmlReturn_t nvmlDeviceGetClockInfo(nvmlDevice_t device, nvmlClockType_t type, out uint clock);

    [DllImport("nvml.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern nvmlReturn_t nvmlDeviceGetPcieThroughput(nvmlDevice_t device, nvmlPcieUtilCounter_t counter, out uint value);      // KiBps

    [DllImport("nvml.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern nvmlReturn_t nvmlDeviceGetName(nvmlDevice_t device, byte[] name, uint length);

}


public class NvidiaSmi
{
    public List<GpuInformation> GPU;

    static readonly bool NvidiaSmiIsAvailable = false;


    private static readonly NvidiaSmi nvidiaSmi;

    public static NvidiaSmi GetInstance()
    {
        return nvidiaSmi;
    }

    private NvidiaSmi()
    {
        GPU = new List<GpuInformation>(4);
    }

    static NvidiaSmi()
    {
        nvidiaSmi = new NvidiaSmi();
        try
        {
            nvmlReturn_t ret = NvmlApi.nvmlInit_v2();
            if (ret == nvmlReturn_t.Success)
            {
                NvidiaSmiIsAvailable = true;
            }
        }
        catch { }
    }
    
    ~NvidiaSmi()
    {
        if (NvidiaSmiIsAvailable)
        {
            NvmlApi.nvmlShutdown();
        }
    }

    public void Update()
    {
        if(NvidiaSmiIsAvailable)
        {
            GPU.Clear();

            nvmlReturn_t ret;
            uint nvidiaGpuCount;
            ret = NvmlApi.nvmlDeviceGetCount_v2(out nvidiaGpuCount);
            if (ret != nvmlReturn_t.Success)
            {
                // error from getting count
                return;
            }

            for (uint idx = 0; idx < nvidiaGpuCount; idx++)
            {
                GpuInformation nvidiaGpu = new GpuInformation();

                nvmlDevice_t device;
                ret = NvmlApi.nvmlDeviceGetHandleByIndex_v2(idx, out device);
                if(ret != nvmlReturn_t.Success)
                {
                    // error from getting device
                    continue;
                }

                // name
                byte[] NameBuffer= new byte[127];
                ret = NvmlApi.nvmlDeviceGetName(device, NameBuffer, 127);
                if (ret != nvmlReturn_t.Success)
                {
                    // error from getting name
                    continue;
                }
                string Name="";
                for(int i = 0; i < 127; i++)
                {
                    if (NameBuffer[i] == 0)
                    {
                        break;
                    }
                    Name += (char)NameBuffer[i];
                }
                nvidiaGpu.Name = Name;

                // usage
                nvmlUtilization_t utilization;
                ret = NvmlApi.nvmlDeviceGetUtilizationRates(device, out utilization);
                if (ret == nvmlReturn_t.Success)
                {
                    nvidiaGpu.Usage = (int)utilization.gpu;
                }

                // tmp
                uint tmp;
                ret = NvmlApi.nvmlDeviceGetTemperature(device,nvmlTemperatureSensors_t.TemperatureGpu, out tmp);
                if (ret == nvmlReturn_t.Success)
                {
                    nvidiaGpu.Temperature = (int)tmp;
                }

                // memory
                nvmlMemory_t memory;
                ret = NvmlApi.nvmlDeviceGetMemoryInfo(device, out memory);
                if(ret == nvmlReturn_t.Success)
                {
                    nvidiaGpu.MemoryUsed = (int)(memory.used / 1048576);
                    nvidiaGpu.MemoryTotal = (int)(memory.total / 1048576);
                }

                uint clock;
                ret = NvmlApi.nvmlDeviceGetClockInfo(device, nvmlClockType_t.NVML_CLOCK_GRAPHICS, out clock);
                if(ret == nvmlReturn_t.Success)
                {
                    nvidiaGpu.CoreFreq = (int)clock;
                }

                ret = NvmlApi.nvmlDeviceGetClockInfo(device, nvmlClockType_t.NVML_CLOCK_MEM, out clock);
                if (ret == nvmlReturn_t.Success)
                {
                    nvidiaGpu.MemoryFreq = (int)clock;
                }

                uint power;
                ret = NvmlApi.nvmlDeviceGetPowerUsage(device, out power);
                if (ret == nvmlReturn_t.Success)
                {
                    nvidiaGpu.Power = (int)(power/1000);
                }

                uint limit;
                ret = NvmlApi.nvmlDeviceGetEnforcedPowerLimit(device, out limit);
                if (ret == nvmlReturn_t.Success)
                {
                    nvidiaGpu.LimitPower = (int)(limit/1000);
                }

                uint tx;
                ret = NvmlApi.nvmlDeviceGetPcieThroughput(device, nvmlPcieUtilCounter_t.NVML_PCIE_UTIL_TX_BYTES, out tx);
                if (ret == nvmlReturn_t.Success)
                {
                    nvidiaGpu.PCIeTx = (int)tx;
                }

                uint rx;
                ret = NvmlApi.nvmlDeviceGetPcieThroughput(device, nvmlPcieUtilCounter_t.NVML_PCIE_UTIL_RX_BYTES, out rx);
                if (ret == nvmlReturn_t.Success)
                {
                    nvidiaGpu.PCIeRx = (int)rx;
                }

                GPU.Add(nvidiaGpu);
            }
        }
    }
}