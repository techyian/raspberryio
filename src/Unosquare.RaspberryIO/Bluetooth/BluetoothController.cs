using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Unosquare.Swan;
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

        private static Process GetBluetoothCtlProcess()
        {
            return new Process
            {
                EnableRaisingEvents = true,
                StartInfo = new ProcessStartInfo("bluetoothctl")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true
                }
            };
        }

        #region Public methods

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
        /// Pairs the specified mac address.
        /// </summary>
        /// <param name="macAddress">The mac address.</param>
        /// <param name="passKey">The pass key. Default to 0000</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        public bool Pair(string macAddress, string passKey = "0000")
        {
            if (Devices.ContainsKey(macAddress) == false)
                throw new InvalidOperationException($"You need to use scan first to discover device {macAddress}");

            var data = new List<string>();
            var controlBt = GetBluetoothCtlProcess();

            controlBt.ErrorDataReceived += (s, e) => e.Data.Error();

            controlBt.OutputDataReceived += (s, e) =>
            {
                try
                {
                    // TODO: this needs a little rework
                    if (e.Data.Contains("[bluetooth]#"))
                    {
                        controlBt.StandardInput.Flush();
                        // TODO: Send trust before?
                        controlBt.StandardInput.WriteLine($"pair {macAddress}");
                        Task.Delay(500).Wait();
                        // TODO: Check if passkey was requested
                    }

                    data.Add(e.Data);
                }
                catch (IOException)
                {
                    // Ignored
                }
            };


            controlBt.Start();
            controlBt.BeginOutputReadLine();
            controlBt.BeginErrorReadLine();
            controlBt.WaitForExit((int)TimeSpan.FromSeconds(3).TotalMilliseconds);

            // TODO: Check if data contains a successful message
            return true;
        }

        /// <summary>
        /// Scans this bluetooths devices.
        /// </summary>
        /// <param name="scanTime">The scan time (in seconds).</param>
        public void Scan(int scanTime = 5)
        {
            var data = new List<string>();
            var state = false;

            var controlBt = GetBluetoothCtlProcess();

            controlBt.ErrorDataReceived += (s, e) => e.Data.Error();

            controlBt.OutputDataReceived += (s, e) =>
            {
                try
                {
                    // TODO: this needs a little rework
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
                                Task.Delay(TimeSpan.FromSeconds(scanTime)).Wait();
                                controlBt.StandardInput.WriteLine("scan off");
                            }
                        }
                    }

                    data.Add(e.Data);
                }
                catch (IOException)
                {
                    // Ignored
                }
            };

            controlBt.Start();
            controlBt.BeginOutputReadLine();
            controlBt.BeginErrorReadLine();
            // Wait the double of scan time
            controlBt.WaitForExit((int)TimeSpan.FromSeconds(scanTime * 2).TotalMilliseconds);

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