using System;
using System.Collections.Generic;
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
                return statusOutput.Contains("Running") ? true : false;
            }
        }

        /// <summary>
        /// Initialize Bluetooth Control.
        /// </summary>
        /// <returns></returns>
        public string InitializeBT()
        {
            var commandInput = ProcessRunner.GetProcessOutputAsync("echo", "agent on > test1.txrt");
            var stringOutput = ProcessRunner.GetProcessOutputAsync("bluetoothctl", "< test1.txrt");
            var Output = ProcessRunner.GetProcessOutputAsync("cat", "test1.txrt").Result;
            return Output;
        }


        #endregion
    }
}
