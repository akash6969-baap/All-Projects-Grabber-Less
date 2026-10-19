using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AotForms
{
    public static class PipeServer
    {
        private static readonly ESP esp = new ESP();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        // SW_HIDE = 0, SW_SHOW = 5
        static void SetOverlayVisible(bool visible)
        {
            // Try using Core.Handle first (the ImGui render window)
            if (Core.Handle != IntPtr.Zero)
            {
                ShowWindow(Core.Handle, visible ? 5 : 0);
                return;
            }
            // Fallback: find by process main window
            var procs = System.Diagnostics.Process.GetProcessesByName("HD-Player");
            if (procs.Length > 0)
                ShowWindow(procs[0].MainWindowHandle, visible ? 5 : 0);
        }

        public static void Start()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        using var server = new NamedPipeServerStream("esp_pipe", PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                        server.WaitForConnection();

                        using var reader = new StreamReader(server, Encoding.UTF8);
                        string? command = reader.ReadLine();

                        if (string.IsNullOrWhiteSpace(command)) continue;

                        switch (command.Trim())
                        {
                            case "aimbotv1on":
                            case "aimbotaion":
                                Config.AimbotVisible = true;
                                Config.FearCollider  = false;
                                break;
                            case "aimbotv1off":
                            case "aimbotaioff":
                                Config.AimbotVisible = false;
                                break;

                            case "fearcollideron":
                                Config.FearCollider  = true;
                                Config.AimbotVisible = false;
                                break;
                            case "fearcollideroff":
                                Config.FearCollider = false;
                                break;

                            case "norecoilon":
                                Config.NoRecoil = true;
                                break;
                            case "norecoiloff":
                                Config.NoRecoil = false;
                                break;

                            case "ignoreknockon":
                                Config.IgnoreKnocked = true;
                                break;
                            case "ignoreknockoff":
                                Config.IgnoreKnocked = false;
                                break;

                            case "aimfovcircleon":
                                Config.AimFov = true;
                                Config.Aimfovc = true;
                                break;
                            case "aimfovcircleoff":
                                Config.AimFov = false;
                                Config.Aimfovc = false;
                                break;

                            case "streammodon":
                                Config.StreamMode = true;
                                Config.sound = true;
                                break;
                            case "streammodoff":
                                Config.StreamMode = false;
                                Config.sound = false;
                                break;

                            case "mutesoundon":
                                Config.sound = true;
                                break;
                            case "mutesoundoff":
                                Config.sound = false;
                                break;

                            case "esplineon":
                                Config.ESPLine = true;
                                Config.EspUp = true;
                                break;
                            case "esplineoff":
                                Config.ESPLine = false;
                                Config.EspUp = false;
                                break;

                            case "espboxon":
                                Config.ESPBox = true;
                                break;
                            case "espboxoff":
                                Config.ESPBox = false;
                                break;

                            case "espfillboxon":
                                Config.ESPFillBox = true;
                                break;
                            case "espfillboxoff":
                                Config.ESPFillBox = false;
                                break;

                            case "espconfigon":
                                Config.ESPName = true;
                                Config.ESPDistance = true;
                                Config.ESPHealth = true;
                                break;
                            case "espconfigoff":
                                Config.ESPName = false;
                                Config.ESPDistance = false;
                                Config.ESPHealth = false;
                                break;

                            case "espskeletonon":
                                Config.ESPSkeleton = true;
                                break;
                            case "espskeletonoff":
                                Config.ESPSkeleton = false;
                                break;

                            case "espfixon":
                                Config.FixEsp = true;
                                Task.Run(async () =>
                                {
                                    while (Config.FixEsp)
                                    {
                                        await Task.Delay(5000);
                                        if (Config.FixEsp)
                                        {
                                            Core.Entities = new();
                                            InternalMemory.Cache = new();
                                        }
                                    }
                                });
                                break;
                            case "espfixoff":
                                Config.FixEsp = false;
                                break;

                            case "minimapon":
                                Config.minimap = true;
                                break;
                            case "minimapoff":
                                Config.minimap = false;
                                break;

                            case "reload":
                                _ = Client.Form1.InitializeAsync();
                                break;

                            case "environment.exit":
                                SetOverlayVisible(false);
                                Application.Exit();
                                Environment.Exit(0);
                                break;

                            case "hidetaskbar":
                                SetOverlayVisible(false);
                                break;
                            case "showtaskbar":
                                SetOverlayVisible(true);
                                break;

                            default:
                                if (command.StartsWith("aimfovcircle:"))
                                {
                                    string val = command.Split(':')[1];
                                    if (int.TryParse(val, out int fovVal))
                                        Config.AimBotFov = fovVal;
                                }
                                else if (command.StartsWith("aipower:"))
                                {
                                    string val = command.Split(':')[1];
                                    if (int.TryParse(val, out int distance))
                                        Config.AimBotMaxDistance = distance;
                                }
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Pipe error: {ex.Message}");
                    }
                }
            });
        }
    }
}
