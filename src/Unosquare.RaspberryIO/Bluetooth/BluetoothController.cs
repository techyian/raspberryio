using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        public List<string> InitializeBT()  
        {
            List<string> result = new List<string>();
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
                if (e.Data.Contains("B8:27:EB:CA:03:21"))
                {
                    state = true;
                    controlBT.StandardInput.Flush();
                    controlBT.StandardInput.WriteLine("agent on");
                    if (state)
                    {
                        state = false;
                        controlBT.StandardInput.Flush();
                        controlBT.StandardInput.WriteLine("default-agent");
                        if (!state)
                        {
                            controlBT.StandardInput.Flush();
                            controlBT.StandardInput.WriteLine("scan on");
                            result.Add(e.Data);
                        }                        
                    }
                }
            };
            controlBT.Start();
            controlBT.BeginOutputReadLine();
            controlBT.WaitForExit(8000);
            return result;
        }
        #endregion
    }
}
