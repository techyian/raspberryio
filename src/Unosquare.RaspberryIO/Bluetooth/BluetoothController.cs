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
        /// Retrieves current bluetooth status.
        /// </summary>
        /// 
        public bool StatusBT
        {
            get 
            {
                var statusOutput = ProcessRunner.GetProcessOutputAsync("systemctl", "status bluetooth").Result;
                return statusOutput.Contains("Running") ? true : false;
            }
        }

        /// <summary>
        /// Retrieves current bluetooth status.
        /// </summary>
        /// <returns></returns>
        public string InitializeBT
        {
            //Use Pipes for communication
            get; set;
        }


        #endregion
    }
}
