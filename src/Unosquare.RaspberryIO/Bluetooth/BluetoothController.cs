using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unosquare.RaspberryIO.Models;
using Unosquare.Swan;
using Unosquare.Swan.Abstractions;
using Unosquare.Swan.Components;

namespace Unosquare.RaspberryIO.Bluetooth
{

    /// <summary>
    /// The Raspberry Pi's bluetooth controller wrapping raspistill and raspivid programs.
    /// This class is a singleton
    /// </summary>
    public class BluetoothController : SingletonBase<BluetoothController>
    {
        #region SimpleFunctionBluetooth
    
        /// <summary>
        /// Retrieves current Bluetooth status.
        /// </summary>
        /// 
        public bool IsRunning
        {
            get 
            {
                var statusOutput = ProcessRunner.GetProcessOutputAsync("systemctl", "status bluetooth").Result;
                return statusOutput.Contains("Running");
            }
        }

        /// <summary>
        /// Initialize Bluetooth Control.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> ScanBT()  
        {
            List<string> results = new List<string>();
            List<string> data = new List<string>();
            bool state = false;            
            Dictionary<string, string> devices = new Dictionary<string, string>();
            Process controlBT = new Process();
            controlBT.EnableRaisingEvents = true;
            controlBT.StartInfo = new ProcessStartInfo("bluetoothctl")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = true
            };
            controlBT.OutputDataReceived += (s, e) =>
            {
                if (e.Data.Contains("[bluetooth]#"))
                {
                    state = true;
                    controlBT.StandardInput.Flush();
                    controlBT.StandardInput.WriteLine("agent on");
                    Task.Delay(500).Wait();
                    if (state)
                    {
                        state = false;
                        controlBT.StandardInput.Flush();
                        controlBT.StandardInput.WriteLine("default-agent");
                        Task.Delay(500).Wait();
                        if (!state)
                        {
                            controlBT.StandardInput.Flush();
                            controlBT.StandardInput.WriteLine("scan on");
                            Task.Delay(7000).Wait();
                            controlBT.StandardInput.WriteLine("scan off");
                            Task.Delay(500).Wait();                            
                        }
                    }
                }
                data.Add(e.Data);
            };
            controlBT.Start();
            controlBT.BeginOutputReadLine();
            controlBT.WaitForExit(10000);
            foreach (var result in data)
            {
                if (result.Contains("NEW") && result.Contains("Device"))
                {
                    var trim = result.Substring(27);
                    var split = trim.Split();
                    devices.Add(split[0], split[1]);
                    results.Add(trim);
                }
            }
            return devices;
        }
        #endregion
    }
}
