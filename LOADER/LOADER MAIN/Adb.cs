using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace adb
{
    internal class Adb
    {
        private string _path;
        private StringBuilder _builder;
        private Process _process;

        internal Adb(string path)
        {
            _path = path;
            _builder = new StringBuilder();
        }

        internal async Task Kill()
        {
            await Task.Run(() => {
                var adbProcesses = Process.GetProcessesByName("adb");

                foreach (var adbProcess in adbProcesses)
                {
                    adbProcess.Kill();
                    adbProcess.WaitForExit();
                }

                adbProcesses = Process.GetProcessesByName("HD-Adb");

                foreach (var adbProcess in adbProcesses)
                {
                    adbProcess.Kill();
                    adbProcess.WaitForExit();
                }
            });
        }

        internal async Task<bool> Start()
        {
            return await Task.Run(() => {
                ExecuteAdbCommand("kill-server");
                ExecuteAdbCommand("devices");

                _process = new Process();
                _process.StartInfo.FileName = _path;
                _process.StartInfo.Arguments = "shell \"getprop ro.secure ; /boot/android/android/system/xbin/bstk/su\"";
                _process.StartInfo.RedirectStandardOutput = true;
                _process.StartInfo.RedirectStandardError = true;
                _process.StartInfo.RedirectStandardInput = true;
                _process.StartInfo.UseShellExecute = false;
                _process.StartInfo.CreateNoWindow = true;
                _process.EnableRaisingEvents = true;
                _process.OutputDataReceived += Receiver;
                _process.ErrorDataReceived += Receiver;

                _process.Start();
                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();

                _process.StandardInput.AutoFlush = true;

                while (_builder.Length == 0) Thread.Sleep(1);

                return true;
            });
        }

        private void Receiver(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                _builder.AppendLine(e.Data);
        }

        internal async Task<bool> IsAppRunning(string packageName)
        {
            return await Task.Run(() =>
            {
                _builder.Clear();
                _process.StandardInput.WriteLine($"ps"); // ADB command to list running processes

                while (_builder.Length == 0) Thread.Sleep(1); // Wait for the command to complete

                var procLines = _builder.ToString().Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var procLine in procLines)
                {
                    if (procLine.Contains(packageName))
                    {
                        return true; // Package is running
                    }
                }
                return false; // Package is not running
            });
        }


        internal async Task KillApp(string packageName)
        {
            await Task.Run(() => {
                ExecuteAdbCommand($"shell am force-stop {packageName}");
            });
        }

        internal async Task<bool> StartApp(string packageName)
        {
            return await Task.Run(() => {
                var command = $"shell am start -n {packageName}/.MainActivity"; // Adjust activity name as necessary
                ExecuteAdbCommand($"shell monkey -p {packageName} -c android.intent.category.LAUNCHER 1"); // Restore app

                // Optionally, verify if the app started successfully (could implement more robust checks)
                return true;
            });
        }
        internal async Task MinimizeApp(string packageName)
        {
            await Task.Run(() => {
                ExecuteAdbCommand($"shell input keyevent KEYCODE_HOME"); // Minimize to home
            });
        }

        internal async Task RestoreApp(string packageName)
        {
            await Task.Run(() => {
                ExecuteAdbCommand($"shell monkey -p {packageName} -c android.intent.category.LAUNCHER 1"); // Restore app
            });
        }


        private void ExecuteAdbCommand(string command)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _path,
                    Arguments = command,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            proc.WaitForExit();
        }
    }
}
