using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unosquare.RaspberryIO.Models;
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
        public string InitializeBT()
        {          
            string results = "";
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
                results = e.Data;
                if (results.Contains("Controller B8:27:EB:CA:03:21"))
                {
                    controlBT.StandardInput.WriteLine("agent on");
                    
                }
            };
            controlBT.Start();
            controlBT.BeginOutputReadLine();
            controlBT.WaitForExit(3000);
            return results;
        }
        #endregion
    }
}
