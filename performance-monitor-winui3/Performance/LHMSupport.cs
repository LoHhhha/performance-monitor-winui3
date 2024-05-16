// LibreHardwareMonitor.Hardware:https://github.com/LibreHardwareMonitor/LibreHardwareMonitor
// this file is help to use LibreHardwareMonitor
using System.Collections.ObjectModel;
using LibreHardwareMonitor.Hardware;
using HardwareStruct;
using System.Net;
using Hardware.Info;
using System.ComponentModel.Design;

namespace LHMSupport;
// LibreHardwareMonitor`s Visitor
// use to update LibreHardwareMonitor.Hardware.Computer object
internal class UpdateVisitor : IVisitor
{
    public void VisitComputer(IComputer computer)
    {
        computer.Traverse(this);
    }
    public void VisitHardware(IHardware hardware)
    {
        hardware.Update();
        foreach (IHardware subHardware in hardware.SubHardware)
            subHardware.Accept(this);
    }
    public void VisitSensor(ISensor sensor) { }
    public void VisitParameter(IParameter parameter) { }
}

class LHMReader
{
    public readonly List<CpuInformation> CPU;
    public readonly List<GpuInformation> GPU;
    public NetworkSpeedInformation Network;
    public readonly List<BatteryInformation> Battery;
    public readonly List<DiskInformation> Disk;
    public readonly List<MemoryInformation> Memory;


    //private readonly Semaphore dataReady = new Semaphore(0, 1);
    //private readonly Semaphore dataNeed = new Semaphore(1, 1);
    //private Thread? collecter;

    readonly Computer computer;
    readonly UpdateVisitor updateVisitor;

    // key: type in LibreHardwareMonitor, value: type in us
    readonly Dictionary<string, string> TypeMapping;
    // key: type in us, value: how to get information
    readonly Dictionary<string, Func<IHardware, bool>> DataUpdate;
    Dictionary<string, float> GetSensorValue(IHardware hardware)
    {
        Dictionary<string, float> getInfo = [];
        foreach (ISensor sensor in hardware.Sensors)
        {
            float sensorValue = -1;
            try
            {
                sensorValue = sensor.Value.Value;
            }
            catch { }

            if (!getInfo.ContainsKey(sensor.Name))
            {
                getInfo.Add(sensor.Name, sensorValue);
            }
            else
            {
                getInfo[sensor.Name] = float.Max(sensorValue, getInfo[sensor.Name]);
            }
        }
        return getInfo;
    }
    void Init()
    {
        // CPU
        TypeMapping.Add("Cpu", "CPU");
        DataUpdate.Add("CPU",
            (IHardware hardware) =>
            {
                Dictionary<string, float> getInfo = GetSensorValue(hardware);

                CpuInformation thisCPU = new();

                // name
                thisCPU.Name = hardware.Name;

                // usage
                int threadCount = 0;
                while (getInfo.ContainsKey($"CPU Core #{threadCount + 1}"))
                {
                    thisCPU.Usage[threadCount++] = (int)getInfo[$"CPU Core #{threadCount}"];
                }
                thisCPU.MaxUsage = (int)getInfo.GetValueOrDefault("CPU Core Max", -1);
                thisCPU.TotalUsage = (int)getInfo.GetValueOrDefault("CPU Total", -1);
                thisCPU.ThreadCount = threadCount;

                // frequency
                int coreCount = 0;
                while (getInfo.ContainsKey($"Core #{coreCount + 1}"))
                {
                    thisCPU.Freq[coreCount++] = (int)getInfo[$"Core #{coreCount}"];
                }
                thisCPU.CoreCount = coreCount;

                // power
                thisCPU.Power = (int)getInfo.GetValueOrDefault("Package", -1);

                // temperature
                thisCPU.Temperature = (int)getInfo.GetValueOrDefault("Core (Tctl/Tdie)", -1);

                CPU.Add(thisCPU);
                return true;
            }
        );

        // GPU
        TypeMapping.Add("Gpu", "GPU");
        // TypeMapping.Add("GpuNvidia", "GPU");     // nvidia gpu use nvml.
        TypeMapping.Add("GpuAmd", "GPU");
        DataUpdate.Add("GPU",
            (IHardware hardware) =>
            {
                GpuInformation thisGPU = new();

                // name
                thisGPU.Name = hardware.Name;

                try
                {
                    // GPU Core
                    // GPU Core
                    // GPU Memory
                    // GPU Core
                    // GPU Memory Controller
                    // GPU Video Engine
                    // GPU Bus
                    // GPU Hot Spot
                    // GPU Memory Total
                    // GPU Memory Free
                    // GPU Memory Used
                    // GPU Memory
                    // GPU Package
                    // GPU PCIe Rx
                    // GPU PCIe Tx

                    // temperature
                    thisGPU.Temperature = (int)hardware.Sensors[0].Value;

                    // core frequency
                    thisGPU.CoreFreq = (int)hardware.Sensors[1].Value;

                    // memory frequency
                    thisGPU.MemoryFreq = (int)hardware.Sensors[2].Value;

                    // 3d load
                    thisGPU.Usage = (int)hardware.Sensors[3].Value;

                    // video engine load
                    thisGPU.VideoUsage = (int)hardware.Sensors[5].Value;

                    // memory total
                    thisGPU.MemoryTotal = (int)hardware.Sensors[8].Value;

                    // memory used
                    thisGPU.MemoryUsed = (int)hardware.Sensors[10].Value;

                    // power
                    thisGPU.Power = (int)hardware.Sensors[12].Value;

                    // PCIe RX
                    thisGPU.PCIeRx = (int)hardware.Sensors[13].Value;

                    // PCIe TX
                    thisGPU.PCIeTx = (int)hardware.Sensors[14].Value;
                }
                catch { }

                GPU.Add(thisGPU);

                return true;
            }
        );

        // NetWork
        TypeMapping.Add("Network", "Network");
        DataUpdate.Add("Network",
            (IHardware hardware) =>
            {
                foreach(var sensor in hardware.Sensors)
                {
                    if(sensor.Name == "Upload Speed")
                    {
                        // upload
                        Network.UploadSpeed += (long)sensor.Value;
                    }
                    else if(sensor.Name == "Download Speed")
                    {
                        // download
                        Network.DownloadSpeed += (long)sensor.Value;
                    }
                }
                
                return true;
            }
        );

        // Battery
        TypeMapping.Add("Battery", "Battery");
        DataUpdate.Add("Battery",
            (IHardware hardware) =>
            {
                // Charge Level
                // Voltage
                // Charge / Discharge Current
                // Designed Capacity
                // Full Charged Capacity
                // Remaining Capacity
                // Charge / Discharge Rate
                // Degradation Level
                BatteryInformation thisBattery = new ();

                thisBattery.Name = hardware.Name;

                foreach( var sensor in hardware.Sensors)
                {
                    if(sensor.Name== "Charge Level")
                    {
                        thisBattery.Level = (float)sensor.Value;
                    }
                    else if (sensor.Name == "Voltage")
                    {
                        thisBattery.Voltage = (float)sensor.Value;
                    }
                }

                Battery.Add(thisBattery);

                return true;
            }
        );

        // Disk
        TypeMapping.Add("Storage", "Disk");
        DataUpdate.Add("Disk",
            (IHardware hardware) =>
            {
                DiskInformation thisDisk = new ();

                thisDisk.Name = hardware.Name;
                try
                {
                    // temperature
                    thisDisk.Temperature = (int)hardware.Sensors[0].Value;
                }
                catch { }

                Disk.Add(thisDisk);

                return true;
            }
        );

        // Memory
        TypeMapping.Add("Memory", "Memory");
        DataUpdate.Add("Memory",
            (IHardware hardware) =>
            {
                MemoryInformation thisMemory = new();

                thisMemory.Name = hardware.Name;
                try
                {
                    // used
                    thisMemory.Used = (int)(hardware.Sensors[0].Value*1024);

                    // available
                    thisMemory.Available = (int)(hardware.Sensors[1].Value * 1024);

                    // used
                    thisMemory.VirtualUsed = (int)(hardware.Sensors[3].Value * 1024);

                    // used
                    thisMemory.VirtualAvailable = (int)(hardware.Sensors[4].Value * 1024);
                }
                catch { }

                Memory.Add(thisMemory);

                return true;
            }
        );
    }

    public LHMReader()
    {
        updateVisitor = new UpdateVisitor();
        TypeMapping = [];
        DataUpdate = [];

        CPU = new List<CpuInformation>(2);
        GPU = new List<GpuInformation>(4);
        Network = new NetworkSpeedInformation();
        Battery = new List<BatteryInformation>(2);
        Disk = new List<DiskInformation>(4);
        Memory = new List<MemoryInformation>(2);


        Init();

        computer = new Computer()
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            //IsMotherboardEnabled = true,
            IsControllerEnabled = true,
            IsBatteryEnabled = true,
            IsNetworkEnabled = true,
            IsStorageEnabled = true,
            //IsPsuEnabled = true,
        };
        computer.Open();
        Update();

        //InitCollecter();
    }

    //private void InitCollecter()
    //{
    //    collecter = new Thread(() =>
    //    {
    //        while (true)
    //        {
    //            dataNeed.WaitOne();

    //            // refresh
    //            computer.Accept(updateVisitor);

    //            dataReady.Release();
    //        }
    //    });
    //    collecter.IsBackground = true;
    //    collecter.Start();
    //}

    public void Update()
    {
        //// data is not ready
        //if (dataReady.WaitOne(0) == false)
        //{
        //    return;
        //}

        computer.Accept(updateVisitor);

        CPU.Clear();
        GPU.Clear();
        Memory.Clear();
        Battery.Clear();
        Disk.Clear();
        Network.Clear();

        Dictionary<string, List<Dictionary<string, string>>> res = [];

        foreach (IHardware hardware in computer.Hardware)
        {
            string type = TypeMapping.GetValueOrDefault(hardware.HardwareType.ToString(), "");
            if (type != "")
            {
                DataUpdate[type](hardware);
            }
        }

        //dataNeed.Release();
    }

    ~LHMReader()
    {
        computer.Close();
    }
}
