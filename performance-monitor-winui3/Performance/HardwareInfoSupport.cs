using System.Diagnostics;
using Hardware.Info;
using HardwareStruct;

namespace HardwareInfoSupport;
// https://github.com/Jinjinov/Hardware.Info

class HardwareInfoReader
{
    public readonly List<NetworkInformation> Network;

    //private readonly Semaphore dataReady = new Semaphore(0, 1);
    //private readonly Semaphore dataNeed = new Semaphore(1, 1);
    //private Thread? collecter;

    private readonly IHardwareInfo? hardwareInfo=null;

    public HardwareInfoReader()
    {
        try
        {
            hardwareInfo = new HardwareInfo();
        }
        catch { }
        Network = new List<NetworkInformation>(16);

        //InitCollecter();
    }

    //private void InitCollecter()
    //{
    //    collecter = new Thread(() =>
    //    {
    //        if (hardwareInfo == null) return;

    //        while (true)
    //        {
    //            dataNeed.WaitOne();

    //            // refresh
    //            try
    //            {
    //                //hardwareInfo.RefreshOperatingSystem();
    //                //hardwareInfo.RefreshMemoryStatus();
    //                //hardwareInfo.RefreshBatteryList();
    //                //hardwareInfo.RefreshBIOSList();
    //                //hardwareInfo.RefreshCPUList();
    //                //hardwareInfo.RefreshDriveList();
    //                //hardwareInfo.RefreshKeyboardList();
    //                //hardwareInfo.RefreshMemoryList();
    //                //hardwareInfo.RefreshMonitorList();
    //                //hardwareInfo.RefreshMotherboardList();
    //                //hardwareInfo.RefreshMouseList();
    //                hardwareInfo.RefreshNetworkAdapterList(false);
    //                //hardwareInfo.RefreshPrinterList();
    //                //hardwareInfo.RefreshSoundDeviceList();
    //                //hardwareInfo.RefreshVideoControllerList();
    //            }
    //            catch { }

    //            dataReady.Release();
    //        }
    //    });
    //    collecter.IsBackground = true;
    //    collecter.Start();
    //}


    public void Update()
    {
        if (hardwareInfo != null)
        {
            //// data is not ready
            //if (dataReady.WaitOne(0) == false) 
            //{
            //    return;
            //}

            try
            {
                //hardwareInfo.RefreshOperatingSystem();
                //hardwareInfo.RefreshMemoryStatus();
                //hardwareInfo.RefreshBatteryList();
                //hardwareInfo.RefreshBIOSList();
                //hardwareInfo.RefreshCPUList();
                //hardwareInfo.RefreshDriveList();
                //hardwareInfo.RefreshKeyboardList();
                //hardwareInfo.RefreshMemoryList();
                //hardwareInfo.RefreshMonitorList();
                //hardwareInfo.RefreshMotherboardList();
                //hardwareInfo.RefreshMouseList();
                hardwareInfo.RefreshNetworkAdapterList(false);
                //hardwareInfo.RefreshPrinterList();
                //hardwareInfo.RefreshSoundDeviceList();
                //hardwareInfo.RefreshVideoControllerList();
            }
            catch { }

            //Network
            Network.Clear();
            foreach (var hardware in hardwareInfo.NetworkAdapterList)
            {
                NetworkInformation network = new NetworkInformation();
                network.Name = hardware.Name;
                network.Mac = hardware.MACAddress;
                network.DHCPServer = hardware.DHCPServer.ToString();

                foreach (var ip in hardware.IPAddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        network.IPv4.Add(ip.ToString());
                    }
                    else if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                    {
                        network.IPv6.Add(ip.ToString());
                    }
                }
                network.Speed = (long)(hardware.Speed == long.MaxValue ? 0 : hardware.Speed);

                Network.Add(network);
            }

            //dataNeed.Release();
        }
    }
}
