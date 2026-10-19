using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

namespace AotForms
{
    public static class PipeServer
    {

        private static readonly ESP esp = new ESP();
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
                            case "autoswitch":
                                Config.AutoSwitch = true;
                                break;
                            case "autoswitchoff":
                                Config.AutoSwitch = false;
                                break;

                            case "teleanywhere":
                                Config.TeleportV2 = true;
                                break;
                            case "teleanywhereoff":
                                Config.TeleportV2 = false;
                                break;

                            case "shakekillon":
                                Config.shakekill = true;
                                break;
                            case "shakekilloff":
                                Config.shakekill = false;
                                break;

                            case "downplayeron":
                                Config.DownPlayer = true;
                                if (Config.DownPlayer)
                                {
                                    DownPlayer.Start();
                                }
                                else
                                {
                                    DownPlayer.Stop();
                                }
                                break;
                            case "downplayeroff":
                                Config.DownPlayer = false;
                                if (Config.DownPlayer)
                                {
                                    DownPlayer.Start();
                                }
                                else
                                {
                                    DownPlayer.Stop();
                                }
                                break;

                            case "pullenemyon":
                                Config.EnemyPullEnabled = true;
                                break;
                            case "pullenemyoff":
                                Config.EnemyPullEnabled = false;
                                break;

                            case "esptimeron":
                                Config.EspTimerActive = true;
                                break;
                            case "esptimeroff":
                                Config.EspTimerActive = false;
                                break;

                            case "ignoreknockedon":
                                Config.IgnoreKnocked = true;
                                break;
                            case "ignoreknockedoff":
                                Config.IgnoreKnocked = false;
                                break;

                            case "climbupon":
                                Config.ClimbUpEnabled = true;
                                break;
                            case "climbupoff":
                                Config.ClimbUpEnabled = false;
                                break;


                            case "flyhackon":
                                Config.flyhack = true;
                                esp.flyhackon();
                                break;
                            case "flyhackoff":
                                Config.flyhack = false;
                                esp.flyhackoff();
                                break;

                            case "gfon":
                                esp.glitchfireon();
                                break;
                            case "gfoff":
                                esp.glitchfireoff();
                                break;
                            case "speedon":
                                Config.SpeedHack = true;
                                esp.speedon();
                                break;

                            case "speedoff":
                                Config.SpeedHack = false;
                                esp.speedoff();
                                break;

                            case "speedload":
                                esp.speedon();
                                break;

                            case "wallload":
                                esp.loadwall();
                                break;
                            case "wallon":
                                esp.wallon();
                                break;
                            case "walloff":
                                esp.walloff();
                                break;

                            case "wallbypass":
                                esp.wallbypass();
                                break;

                            case "underkillon":
                                Config.ClimbDownEnabled = true;
                                break;
                            case "underkilloff":
                                Config.ClimbDownEnabled = false;
                                break;

                            case "baseon":
                                Config.sperm = true;
                                break;
                            case "baseoff":
                                Config.sperm = false;
                                break;

                            case "fastreloadon":
                                Config.FastReload = true;
                                break;
                            case "fastreloadoff":
                                Config.FastReload = false;
                                break;

                            case "aimbotvisible":
                                Config.AimbotVisible = true;
                                break;
                            case "aimbotvisibleoff":
                                Config.AimbotVisible = false;
                                break;
                            case "aimbotai":
                                Config.Aimbotai = true;
                                break;
                            case "aimbotaioff":
                                Config.Aimbotai = false;
                                break;
                            case "enablefunction":
                                Config.enableAimBot = true;
                                break;
                            case "enablefunctionoff":
                                Config.enableAimBot = false;
                                break;
                            case "aimbotrage":
                                Config.AimBotRageMax = true;
                                break;
                            case "aimbotrageoff":
                                Config.AimBotRageMax = false;
                                break;

                            case "aimbotragemainson":
                                Config.AimBotRageMainId = true;
                                break;
                            case "aimbotragemainoff":
                                Config.AimBotRageMainId = false;
                                break;
                            case "silentaim":
                                Config.SilentAim = true;
                                break;
                            case "silentaimoff":
                                Config.SilentAim = false;
                                break;
                            case "upplayer":
                                Config.UpPlayer = true;
                                break;
                            case "upplayeroff":
                                Config.UpPlayer = false;
                                break;
                            case "telekil":
                                Config.teli = true;
                                break;
                            case "telekiloff":
                                Config.teli = false;
                                break;
                            case "drawfov":
                                Config.FOVEnabled = true;
                                break;
                            case "drawfovoff":
                                Config.FOVEnabled = false;
                                break;
                            case "espline":
                                Config.ESPLine = true;
                                break;
                            case "esplineoff":
                                Config.ESPLine = false;
                                break;
                            case "espdistanceon":
                                Config.ESPDistance = true;
                                break;
                            case "espdistanceoff":
                                Config.ESPDistance = false;
                                break;
                            case "espbox":
                                Config.ESPBox = true;
                                break;
                            case "espboxoff":
                                Config.ESPBox = false;
                                break;
                            case "esphealth":
                                Config.ESPHealth = true;
                                break;
                            case "esphealthoff":
                                Config.ESPHealth = false;
                                break;
                            case "espname":
                                Config.ESPName = true;
                                break;
                            case "espnameoff":
                                Config.ESPName = false;
                                break;
                            case "espskeleton":
                                Config.ESPSkeleton = true;
                                break;
                            case "espskeletonoff":
                                Config.ESPSkeleton = false;
                                break;
                            case "espaimtrack":
                                Config.AimTrackLine = true;
                                break;
                            case "espaimtrackoff":
                                Config.AimTrackLine = false;
                                break;
                            case "mapon":
                                Config.minimap = true;
                                break;
                            case "mapoff":
                                Config.minimap = false;
                                break;
                            case "streammode":
                                Config.StreamMode = true;
                                break;
                            case "streammodeoff":
                                Config.StreamMode = false;
                                break;
                            case "norecoil":
                                Config.NoRecoil = true;
                                break;
                            case "norecoiloff":
                                Config.NoRecoil = false;
                                break;

                            case "flyhackx40on":
                                Config.FLYHACKX400 = true;
                                break;
                            case "flyhackx40off":
                                Config.FLYHACKX400 = false;
                                break;

                            case "unlimitedammoon":
                                Config.UnlimitedAmmo = true;
                                break;
                            case "unlimitedammooff":
                                Config.UnlimitedAmmo = false;
                                break;

                            case "rapidfireon":
                                Config.RapidFire = true;
                                break;
                            case "rapidfireoff":
                                Config.RapidFire = false;
                                break;

                            case "speedinton":
                                Config.speedint = true;
                                break;
                            case "speedintoff":
                                Config.speedint = false;
                                break;

                            case "espweaponon":
                                Config.espweapon = true;
                                break;
                            case "espweaponoff":
                                Config.espweapon = false;
                                break;

                            case "spinboton":
                                Config.spinbot = true;
                                RapidSpin.Activate();
                                break;
                            case "spinbotoff":
                                Config.spinbot = false;
                                RapidSpin.Deactivate();
                                break;

                            default:
                                if (command.StartsWith("silentaim_mode:"))
                                {
                                    string indexStr = command.Split(':')[1];
                                    if (int.TryParse(indexStr, out int modeIndex))
                                    {
                                        Config.AimBotType = (AimBotType)modeIndex;
                                        Console.WriteLine($"SilentAim mode set to index: {modeIndex}");
                                    }
                                }
                                else if (command.StartsWith("aimfov:"))
                                {
                                    string value = command.Split(':')[1];
                                    if (float.TryParse(value, out float fov))
                                    {
                                        Config.AimFov = fov;
                                        Console.WriteLine($"Aim FOV set to: {fov}");
                                    }
                                }
                                else if (command.StartsWith("rapidfirespeed:"))
                                {
                                    string value = command.Split(':')[1];
                                    if (float.TryParse(value, out float speed))
                                    {
                                        Config.RapidFireSpeed = speed;
                                        Console.WriteLine($"RapidFire Speed set to: {speed}");
                                    }
                                }
                                else if (command.StartsWith("spinspeed:"))
                                {
                                    string value = command.Split(':')[1];
                                    if (float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float spd))
                                    {
                                        Config.SpinSpeed = spd;
                                        Console.WriteLine($"SpinBot Speed set to: {spd}");
                                    }
                                }
                                else if (command.StartsWith("aimtargetpart:"))
                                {
                                    string part = command.Split(':')[1].ToUpper();
                                    Config.AimTargetPart = part;
                                    Console.WriteLine($"AimTargetPart set to: {part}");
                                }
                                else if (command.StartsWith("aimsmooth:"))
                                {
                                    string value = command.Split(':')[1];
                                    if (float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float smooth))
                                    {
                                        Config.AimBotSmooth = smooth;
                                        Console.WriteLine($"AimBotSmooth set to: {smooth}");
                                    }
                                }
                                else if (command.StartsWith("smoothness:"))
                                {
                                    string value = command.Split(':')[1];
                                    if (int.TryParse(value, out int ms))
                                    {
                                        Config.Smoothness = ms;
                                        Console.WriteLine($"Smoothness set to: {ms}ms");
                                    }
                                }
                                else if (command.StartsWith("pullstrength:"))
                                {
                                    string value = command.Split(':')[1];
                                    if (float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float strength))
                                    {
                                        Config.EnemyPullStrength = Math.Clamp(strength, 0.01f, 1.0f);
                                        Console.WriteLine($"EnemyPullStrength set to: {strength}");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"Unknown command: {command}");
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