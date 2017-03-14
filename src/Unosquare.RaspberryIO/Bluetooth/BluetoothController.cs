using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Unosquare.Swan.Abstractions;
using Unosquare.Swan.Components;

namespace Unosquare.RaspberryIO.Bluetooth
{
    /// <summary>
    /// The Raspberry Pi's bluetooth controller wrapping bluetoothctl.
    /// This class is a singleton
    /// </summary>
    public class BluetoothController : SingletonBase<BluetoothController>
    {
        /// <summary>
        /// Gets the devices.
        /// </summary>
        /// <value>
        /// The devices.
        /// </value>
        public Dictionary<string, string> Devices { get; } = new Dictionary<string, string>();

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
        /// Scans this bluetooths devices.
        /// </summary>
        public void Scan()
        {
            var data = new List<string>();
            var state = false;

            var controlBt = new Process
            {
                EnableRaisingEvents = true,
                StartInfo = new ProcessStartInfo("bluetoothctl")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true
                }
            };

            controlBt.OutputDataReceived += (s, e) =>
            {
                if (e.Data.Contains("[bluetooth]#"))
                {
                    state = true;
                    controlBt.StandardInput.Flush();
                    controlBt.StandardInput.WriteLine("agent on");
                    Task.Delay(500).Wait();

                    if (state)
                    {
                        state = false;
                        controlBt.StandardInput.Flush();
                        controlBt.StandardInput.WriteLine("default-agent");
                        Task.Delay(500).Wait();

                        if (!state)
                        {
                            controlBt.StandardInput.Flush();
                            controlBt.StandardInput.WriteLine("scan on");
                            Task.Delay(7000).Wait();
                            controlBt.StandardInput.WriteLine("scan off");
                            Task.Delay(500).Wait();
                        }
                    }
                }

                data.Add(e.Data);
            };

            controlBt.Start();
            controlBt.BeginOutputReadLine();
            controlBt.WaitForExit(10000);

            foreach (var result in data)
            {
                if (result.Contains("NEW") && result.Contains("Device"))
                {
                    var trim = result.Substring(27);
                    var split = trim.Split();

                    if (Devices.ContainsKey(split[0]) == false)
                        Devices.Add(split[0], split[1]);
                }
            }
        }

        #endregion
    }
}