using System.Diagnostics;
using System.Management;
using Microsoft.UI.Xaml.Controls;

namespace AWCC;

// https://github.com/vinaypamnani/wmie2/
// https://github.com/AlexIII/tcc-g15

public enum ThermalMode
{
    Custom = 0,                     // ok
    Quiet = 150,
    Balanced = 151,                 // ok
    Performance = 152,
    FullSpeed = 153,
    USTT_Balanced = 160,
    USTT_Performance = 161,
    USTT_Cool = 162,
    USTT_Quiet = 163,
    USTT_FullSpeed = 164,
    USTT_BatterySaver = 165,
    G_Mode = 171                    // ok
}


public static class AWCCWMI
{
    private static readonly string WMINamespace = @"root\WMI";
    private static readonly string WMIClassName = "AWCCWmiMethodFunction";
    private static readonly ManagementObject? AWCCMangement;

    public static readonly string ErrorMsg = "";
    public static readonly uint FailureCode = 0xFFFFFFFF;
    public static readonly uint CpuFanId, GpuFanId, CpuSensorId, GpuSensorId;
    public static ThermalMode Mode = ThermalMode.Balanced;

    private static readonly uint FirstSensorId = 1, LastSensorId = 48;
    private static readonly uint FirstFanId = 49, LastFanId = 99;
    private static readonly string InformationFunc = "Thermal_Information";
    private static readonly string ControlFunc = "Thermal_Control";
    private static readonly string FanSensorFunc = "GetFanSensors";

    // the first fan is cpu`s, then is gpu`s.
    private static readonly uint CpuIdx = 0, GpuIdx = 1;

    static AWCCWMI()
    {
        try
        {
            using ManagementObjectSearcher searcher = new ManagementObjectSearcher(WMINamespace, "SELECT * FROM " + WMIClassName);
            ManagementObjectCollection collection = searcher.Get();
            if (collection.Count > 0)
            {
                AWCCMangement = collection.Cast<ManagementObject>().First();
            }

            /* Begin: Init CpuFanIdx, GpuFanIdx, CpuSensorIdx, GpuSensorIdx */
            List<uint> Fans = [], Sensors = [];
            for (var fanId = FirstFanId; fanId <= LastFanId; fanId++)
            {
                var count = GetFanRelatdSensorsCountById(fanId);
                if (count == FailureCode || count == 0) continue;

                var getSensor = false;
                for (uint idx = 0; idx < count; idx++)
                {
                    var sensorId = GetFanRelatdSensorsByIdAndIdx(fanId, idx);
                    if (sensorId == FailureCode || sensorId < FirstSensorId || sensorId > LastSensorId) continue;
                    Sensors.Add(sensorId);
                    getSensor = true;
                    break;
                }
                
                if (getSensor)
                {
                    Fans.Add(fanId);
                }
            }
            if (Fans.Count < 2)
            {
                throw new Exception("Can`t find enough devices!");
            }

            CpuFanId = Fans[(int)CpuIdx];
            CpuSensorId = Sensors[(int)CpuIdx];
            GpuFanId = Fans[(int)GpuIdx];
            GpuSensorId = Sensors[(int)GpuIdx];
            /* End: Init CpuFanIdx, GpuFanIdx, CpuSensorIdx, GpuSensorIdx */
        }
        catch (Exception ex)
        {
            AWCCMangement = null;
            ErrorMsg = ex.ToString();
        }
    }

    public static bool IsAvailable()
    {
        return AWCCMangement != null;
    }

    public static uint GetFanRPM(uint fanId)
    {
        if (fanId < FirstFanId || fanId > LastFanId) return FailureCode;

        var arg = ((fanId & 0xFF) << 8) | 5;
        return Parse(InformationFunc, arg);
    }

    public static uint GetFanRPMPercent(uint fanId)
    {
        if (fanId < FirstFanId || fanId > LastFanId) return FailureCode;

        var arg = (fanId << 8) | 6;
        return Parse(InformationFunc, arg);
    }

    public static uint GetSensorTemperature(uint sensorId)
    {
        if (sensorId < FirstSensorId || sensorId > LastSensorId) return FailureCode;

        var arg = ((sensorId & 0xFF) << 8) | 4;
        return Parse(InformationFunc, arg);
    }

    public static bool ApplyThermalMode(ThermalMode mode)
    {
        var arg = ((uint)mode << 8) | 1;
        if(Parse(ControlFunc, arg) == 0)
        {
            Mode = mode;
            return true;
        }
        return false;
    }

    public static bool SetAddonSpeedPercent(uint fanId, uint addonPercent)
    {
        var arg = (addonPercent << 16) | (fanId << 8) | 2;
        if(Parse(ControlFunc, arg) == 0)
        {
            return true;
        }
        return false;
    }

    /* Begin: Init func */
    private static uint GetFanRelatdSensorsCountById(uint fanId)
    {
        if (fanId < FirstFanId || fanId > LastFanId) return FailureCode;

        var arg = ((fanId & 0xFF) << 8) | 1;
        return Parse(FanSensorFunc, arg);
    }

    private static uint GetFanRelatdSensorsByIdAndIdx(uint fanId, uint sensorIdx)
    {
        if (fanId < FirstFanId || fanId > LastFanId) return FailureCode;

        var arg = ((sensorIdx & 0xFF) << 16) | ((fanId & 0xFF) << 8) | 2;
        return Parse(FanSensorFunc, arg);
    }

    private static uint Parse(string functionName, uint arg)
    {
        if (AWCCMangement == null) return FailureCode;
        var inputParams = AWCCMangement.GetMethodParameters(functionName);
        inputParams["arg2"] = arg;
        var outputParams = AWCCMangement.InvokeMethod(functionName, inputParams, null);
        return (uint)outputParams["argr"];
    }
    /* End: Init func */
}
