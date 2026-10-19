using Client;
using CXMem;
using Guna.UI2.WinForms;
using Helper;
using ImGuiNET;
using FearAuth;
using Memory;
using Microsoft.VisualBasic.ApplicationServices;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Imaging;
using System.Management;
using System.Media;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
namespace AotForms
{
    public partial class FearMenu : Form
    {
        private const string WEBHOOK_URL = "";

        private string _username;

        CaXMem memory = new CaXMem();
        private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

        private System.Windows.Forms.Timer espTimer = new System.Windows.Forms.Timer();
        private int espTimerSeconds = 180;

        private string RunAdbCommand(string adbPath, string arguments)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = adbPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = startInfo })
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();
                return output;
            }
        }

        private bool teleportKeyPressed = false;
        private bool speedint = false;
        private bool Spee123 = false;

        private bool UNDERKILL = false;
        private bool MAPTELE = false;

        private Keys selectedKeyAimbotVisible = Keys.None;
        private Keys selectedKeyClimbUp = Keys.None;
        private bool downPlayerKeyPressed = false;
        private Keys selectedKeyspeedint = Keys.None;
        private Keys selectedKeySpeed123 = Keys.None;

        private Keys selectedKeyunderkill = Keys.None;
        private Keys selectedKeymaptele = Keys.None;

        private bool listeningForKeyClimbUp = false;
        private Keys selectedKeyTeleport = Keys.None;
        private bool listening = false;
        private bool climbUpKeyPressed = false;
        private bool listeningForKeyAimbotVisible = false;
        private bool aimbotVisibleKeyPressed = false;
        private Keys selectedKeyDownPlayer = Keys.None;
        private bool speedKeyPressed = false;
        private bool speed123KeyPressed = false;

        private bool underkillkeypressed = false;
        private bool maptelekeypressed = false;

        private bool listeningg = false;
        private Keys selectedKeyFlyHack = Keys.None;
        private bool flyHackKeyPressed = false;
        private bool listeningForKeyFlyHack = false;
        private Label flyHackKeybindLabel = null;

        string fontpath = "C:\\Windows\\Fonts\\taileb.ttf";

        private Keys selectedKey18 = Keys.None;
        private Keys selectedKey19 = Keys.None;
        private bool listeningForKey18 = false;
        private bool listeningForKey19 = false;
        private System.Windows.Forms.Timer keyCheckTimer;
        private bool key18Pressed = false;
        private bool key19Pressed = false;

        private int scrollY = 0;

        private string configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "config.txt");

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);

        private Thread keyListenerThread;

        private LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;
        private bool isPlaying = true;
        private SoundPlayer soundPlayer;

        private const int MOD_NOREPEAT = 0x4000;
        private const int MOD_ALT = 0x0001;
        private const int HOTKEY_ID = 1;

        private readonly string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "credentials.txt");

        private bool isF5Toggled = false;
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private Keys HideKey;
        private Keys SpeedHotKey;
        private Keys CameraHotKey;
        private Keys AwmHotKey = Keys.LButton; // AWM keybind default: Left Click
        private bool waitingForHidehackKey = false;
        private bool waitingForSpeedhackKey = false;
        private bool waitingForCamerahackKey = false;
        private bool waitingForAwmKey = false;
        private bool formularioVisible = true;
        private const int WM_HOTKEY = 0x0312;
        private const int MYACTION_HOTKEY_ID = 1;
        private const int Hide_HOTKEY_ID = 2;
        private const int Speed_HOTKEY_ID = 3;
        private const int Camera_HOTKEY_ID = 4;

        static float gameTimer = 180f;
        static bool gameEnded = false;
        static DateTime lastFrameTime = DateTime.Now;

        IntPtr mainHandle;
        KeyHelper kh = new KeyHelper();
        bool keybind1 = false;
        bool shift;
        bool aimbothks;
        bool esphks;
        bool confighks;
        public FearMenu(IntPtr handle)
        {
            mainHandle = handle;
            InitializeComponent();



            FearAuthApp.init();
            this.KeyPreview = true;
            this.KeyDown += Form_KeyDown;

            // Manual Slider Hooks
            if (trackBarFloatClient1 != null)
            {
                trackBarFloatClient1.Max = 800;
                trackBarFloatClient1.Event_0 += trackBarFloatClient1_ValueChanged;
            }
            if (trackBarFloatClient2 != null) trackBarFloatClient2.Event_0 += trackBarFloatClient2_ValueChanged;
            if (trackBarFloatClient3 != null) trackBarFloatClient3.Event_0 += trackBarFloatClient3_ValueChanged;
            if (trackBarFloatClient4 != null) trackBarFloatClient4.Event_0 += trackBarFloatClient4_ValueChanged;
            if (trackBarFloatClient5 != null) trackBarFloatClient5.Event_0 += trackBarFloatClient5_ValueChanged;

            keyCheckTimer = new System.Windows.Forms.Timer();
            keyCheckTimer.Interval = 10;
            keyCheckTimer.Tick += KeyListenerLoop;
            keyCheckTimer.Start();

            _proc = HookCallback;
            _hookID = SetHook(_proc);

            kh.KeyDown += Kh_KeyDown;
            kh.KeyUp += Kh_KeyUp;
            this.KeyPreview = true;
            ;
            keyListenerThread = new Thread(KeyListener);
            keyListenerThread.IsBackground = true;
            keyListenerThread.Start();

            espTimer.Interval = 1000;

            espTimer.Start();


        }
        private void Form_KeyDown(object sender, KeyEventArgs e)
        {

            if (listeningForKey18)
            {
                selectedKey18 = e.KeyCode;

                listeningForKey18 = false;
            }
            else if (listeningForKey19)
            {
                selectedKey19 = e.KeyCode;

                listeningForKey19 = false;
            }
            else if (listeningg)
            {
                selectedKeyTeleport = e.KeyCode;

                listeningg = false;
            }
            else if (speedint)
            {
                selectedKeyspeedint = e.KeyCode;

                speedint = false;
            }
            else if (Spee123)
            {
                selectedKeySpeed123 = e.KeyCode;

                Spee123 = false;
            }

            else if (UNDERKILL)
            {
                selectedKeyunderkill = e.KeyCode;

                UNDERKILL = false;
            }
            else if (MAPTELE)
            {
                selectedKeymaptele = e.KeyCode;

                MAPTELE = false;
            }
            else if (listening)
            {
                selectedKeyDownPlayer = e.KeyCode;

                listening = false;
            }
            else if (listeningForKeyClimbUp)
            {
                selectedKeyClimbUp = e.KeyCode;

                listeningForKeyClimbUp = false;
            }
            else if (listeningForKeyAimbotVisible)
            {
                selectedKeyAimbotVisible = e.KeyCode;

                listeningForKeyAimbotVisible = false;
            }
            else if (listeningForKeyFlyHack)
            {
                selectedKeyFlyHack = e.KeyCode;
                if (flyHackKeybindLabel != null)
                {
                    flyHackKeybindLabel.Text = selectedKeyFlyHack.ToString();
                }
                listeningForKeyFlyHack = false;
            }

        }

        public static void UpdateGameTimer()
        {
            if (!Config.EspTimerEnabled)
                return;

            DateTime now = DateTime.Now;
            float deltaTime = (float)(now - lastFrameTime).TotalSeconds;
            lastFrameTime = now;

            if (!gameEnded)
            {
                gameTimer -= deltaTime;

                if (gameTimer <= 0f)
                {
                    gameTimer = 0f;
                    gameEnded = true;
                }
            }
        }

        public static void DrawGameText()
        {
            if (!Config.EspTimerEnabled)
                return;

            string displayText;
            Vector2 textPosition;

            if (!gameEnded)
            {
                displayText = $"INDIAN HACKER 790  On Top  | Time Left: {(int)gameTimer}s";
                textPosition = new Vector2(Core.Width / 2f - 180f, Core.Height - 50f);
            }
            else
            {
                displayText = "END THIS GAME";
                textPosition = new Vector2(Core.Width / 2f - 100f, Core.Height / 2f);
            }

            var draw = ImGui.GetForegroundDrawList();

            uint mainColor = ImGui.ColorConvertFloat4ToU32(
                new Vector4(1f, 0f, 0f, 1f)
            );

            uint outlineColor = ImGui.ColorConvertFloat4ToU32(
                new Vector4(0f, 0f, 0f, 1f)
            );
            draw.AddText(textPosition + new Vector2(1, 1), outlineColor, displayText);
            draw.AddText(textPosition + new Vector2(-1, -1), outlineColor, displayText);
            draw.AddText(textPosition + new Vector2(1, -1), outlineColor, displayText);
            draw.AddText(textPosition + new Vector2(-1, 1), outlineColor, displayText);
            draw.AddText(textPosition, mainColor, displayText);
        }

        public static void ResetGameTimer()
        {
            if (!Config.EspTimerEnabled)
                return;

            gameTimer = 180f;
            gameEnded = false;
            lastFrameTime = DateTime.Now;
        }

        private void KeyListenerLoop(object sender, EventArgs e)
        {
            if (selectedKey18 != Keys.None)
            {
                bool isKeyPressed = (GetAsyncKeyState(selectedKey18) & 0x8000) != 0;
                if (isKeyPressed && !key18Pressed)
                {
                    key18Pressed = true;
                    guna2CustomCheckBox18.Checked = !guna2CustomCheckBox18.Checked;
                    guna2CustomCheckBox18_Click(null, null);
                }
                else if (!isKeyPressed)
                {
                    key18Pressed = false;
                }
            }

            if (selectedKey19 != Keys.None)
            {
                bool isKeyPressed = (GetAsyncKeyState(selectedKey19) & 0x8000) != 0;
                if (isKeyPressed && !key19Pressed)
                {
                    key19Pressed = true;

                    guna2CustomCheckBox19_Click(null, null);
                }
                else if (!isKeyPressed)
                {
                    key19Pressed = false;
                }
            }

            if (selectedKeyTeleport != Keys.None)
            {
                bool isKeyPressed = (GetAsyncKeyState(selectedKeyTeleport) & 0x8000) != 0;
                if (isKeyPressed && !teleportKeyPressed)
                {
                    teleportKeyPressed = true;
                    guna2ToggleSwitch10.Checked = !guna2ToggleSwitch10.Checked;
                    guna2CustomCheckBox19_Click_3(null, null);
                }
                else if (!isKeyPressed)
                {
                    teleportKeyPressed = false;
                }
            }
            if (selectedKeyspeedint != Keys.None)
            {
                bool isKeyPressed = (GetAsyncKeyState(selectedKeyspeedint) & 0x8000) != 0;
                if (isKeyPressed && !speedKeyPressed)
                {
                    speedKeyPressed = true;
                    guna2ToggleSwitch13.Checked = !guna2ToggleSwitch13.Checked;
                    guna2CustomCheckBox20_Click(null, null);
                }
                else if (!isKeyPressed)
                {
                    speedKeyPressed = false;
                }
            }
            if (selectedKeySpeed123 != Keys.None)
            {
                bool isKeyPressed = (GetAsyncKeyState(selectedKeyspeedint) & 0x8000) != 0;
                if (isKeyPressed && !speed123KeyPressed)
                {
                    speed123KeyPressed = true;
                    guna2ToggleSwitch13.Checked = !guna2ToggleSwitch13.Checked;
                    guna2CustomCheckBox49_Click(null, null);
                }
                else if (!isKeyPressed)
                {
                    speed123KeyPressed = false;
                }
            }
            if (selectedKeyunderkill != Keys.None)
            {
                bool isKeyPressed = (GetAsyncKeyState(selectedKeyunderkill) & 0x8000) != 0;
                if (isKeyPressed && !underkillkeypressed)
                {
                    underkillkeypressed = true;
                    guna2ToggleSwitch5.Checked = !guna2ToggleSwitch5.Checked;
                    guna2CustomCheckBox48_Click(null, null);
                }
                else if (!isKeyPressed)
                {
                    underkillkeypressed = false;
                }
            }
            if (selectedKeymaptele != Keys.None)
            {
                bool isKeyPressed = (GetAsyncKeyState(selectedKeymaptele) & 0x8000) != 0;
                if (isKeyPressed && !maptelekeypressed)
                {
                    maptelekeypressed = true;
                    guna2ToggleSwitch12.Checked = !guna2ToggleSwitch12.Checked;
                    guna2CustomCheckBox8_Click_1(null, null);
                }
                else if (!isKeyPressed)
                {
                    maptelekeypressed = false;
                }
            }
            if (selectedKeyDownPlayer != Keys.None)
            {
                bool isKeyPressed = (GetAsyncKeyState(selectedKeyDownPlayer) & 0x8000) != 0;
                if (isKeyPressed && !downPlayerKeyPressed)
                {
                    downPlayerKeyPressed = true;
                    guna2ToggleSwitch11.Checked = !guna2ToggleSwitch11.Checked;
                    flyhack_Click(null, null);
                }
                else if (!isKeyPressed)
                {
                    downPlayerKeyPressed = false;
                }
            }
            if (selectedKeyClimbUp != Keys.None)
            {
                bool isKeyPressed = (GetAsyncKeyState(selectedKeyClimbUp) & 0x8000) != 0;
                if (isKeyPressed && !climbUpKeyPressed)
                {
                    climbUpKeyPressed = true;

                    guna2CustomCheckBox24_Click_1(null, null);
                }
                else if (!isKeyPressed)
                {
                    climbUpKeyPressed = false;
                }
            }
            if (selectedKeyAimbotVisible != Keys.None)
            {
                bool isKeyPressed = (GetAsyncKeyState(selectedKeyAimbotVisible) & 0x8000) != 0;
                if (isKeyPressed && !aimbotVisibleKeyPressed)
                {
                    aimbotVisibleKeyPressed = true;

                    guna2CustomCheckBox40_Click(null, null);
                }
                else if (!isKeyPressed)
                {
                    aimbotVisibleKeyPressed = false;
                }
            }
            if (selectedKeyAimbotVisible != Keys.None)
            {
                bool isKeyPressed = (GetAsyncKeyState(selectedKeyAimbotVisible) & 0x8000) != 0;
                if (isKeyPressed && !aimbotVisibleKeyPressed)
                {
                    aimbotVisibleKeyPressed = true;

                    guna2CustomCheckBox40_Click(null, null);
                }
                else if (!isKeyPressed)
                {
                    aimbotVisibleKeyPressed = false;
                }
            }
            if (selectedKeyFlyHack != Keys.None)
            {
                bool isKeyPressed = (GetAsyncKeyState(selectedKeyFlyHack) & 0x8000) != 0;
                if (isKeyPressed && !flyHackKeyPressed)
                {
                    flyHackKeyPressed = true;
                    guna2ToggleSwitch11.Checked = !guna2ToggleSwitch11.Checked;
                    flyhack_Click(null, null);
                }
                else if (!isKeyPressed)
                {
                    flyHackKeyPressed = false;
                }
            }

        }
        private bool isManualMode = false;
        private bool isKeyActive = false;
        private void KeyListener()
        {
            while (true)
            {
                Thread.Sleep(100);

                if (InvokeRequired)
                {
                    Invoke((MethodInvoker)delegate
                    {
                    });
                }
                else
                {
                }

                if ((GetAsyncKeyState(Keys.D6) & 0x8000) != 0)
                {
                    if (isManualMode)
                    {
                        Invoke((MethodInvoker)delegate
                        {
                            isKeyActive = !isKeyActive;
                            guna2CustomCheckBox18.Checked = isKeyActive;
                            guna2CustomCheckBox18_Click(null, EventArgs.Empty);
                        });
                    }
                    else
                    {
                        Invoke((MethodInvoker)delegate
                        {
                            guna2CustomCheckBox18.Checked = true;
                            guna2CustomCheckBox18_Click(null, EventArgs.Empty);
                        });

                        Thread.Sleep(4000);

                        Invoke((MethodInvoker)delegate
                        {
                            guna2CustomCheckBox18.Checked = false;
                            guna2CustomCheckBox18_Click(null, EventArgs.Empty);
                            Console.Beep(500, 500);
                        });
                    }

                    Thread.Sleep(500);
                }
            }
        }
        private void Panel1_MouseWheel(object sender, MouseEventArgs e)
        {
            int scrollAmount = 20;
            scrollY += (e.Delta > 0 ? scrollAmount : -scrollAmount);
            scrollY = Math.Min(0, scrollY);

        }

        private int GetMaxControlHeight()
        {
            int maxHeight = 0;

            return maxHeight;
        }
        private void SaveSettings()
        {
            try
            {
                File.WriteAllText(configFilePath, txtLicense.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving key: " + ex.Message);
            }
        }


        private void ToggleNetworkFor4Sec()
        {
            string ruleName = "TemporaryBlock2";
            BlockNetwork(ruleName);
            SystemSounds.Beep.Play();

            Thread.Sleep(4000);
            UnblockNetwork(ruleName);
            SystemSounds.Beep.Play();
        }

        private void BlockNetwork(string ruleName)
        {
            string[] programs =
            {
                @"C:\Program Files\BlueStacks\HD-Player.exe",
                @"C:\Program Files\BlueStacks_nxt\HD-Player.exe",
                @"C:\Program Files\BlueStacks_msi2\HD-Player.exe",
                @"C:\Program Files\BlueStacks_msi5\HD-Player.exe"
            };

            foreach (var program in programs)
            {
                RunCommand($"netsh advfirewall firewall add rule name=\"{ruleName}\" dir=in action=block program=\"{program}\"");
                RunCommand($"netsh advfirewall firewall add rule name=\"{ruleName}\" dir=out action=block program=\"{program}\"");
            }
        }

        private void UnblockNetwork(string ruleName)
        {
            RunCommand($"netsh advfirewall firewall delete rule name=\"{ruleName}\"");
        }

        private void RunCommand(string command)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            });
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if (vkCode == (int)Keys.F2)
                {
                    new Thread(ToggleNetworkFor4Sec).Start();
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private const int WH_KEYBOARD_LL = 13;

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        private void Kh_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.LShiftKey || e.KeyCode == Keys.RShiftKey) shift = false;
        }
        private async void Kh_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.LShiftKey || e.KeyCode == Keys.RShiftKey) shift = true;

            if (e.KeyCode == Keys.F1)
            {
                if (Config.FixEsp)
                {
                    Core.Entities = new();
                    InternalMemory.Cache = new();
                }
                else { }
            }

            if (e.KeyCode == Keys.Insert)
            {
                Config.Notif();
                if (keybind1 == false)
                {
                    aimbothks = false;
                    esphks = false;
                    confighks = false;
                    this.Hide();
                    keybind1 = true;
                }
                else
                {
                    this.Show();

                    SetStreamMode();

                    void SetStreamMode()
                    {
                        foreach (var obj in Application.OpenForms)
                        {
                            var form = obj as Form;

                            if (Config.StreamMode)
                            {
                                SetWindowDisplayAffinity(form.Handle, WDA_NONE);
                                SetWindowDisplayAffinity(form.Handle, WDA_EXCLUDEFROMCAPTURE);
                            }
                            else
                            {
                            }
                        }
                    }

                    keybind1 = false;
                }
            }
        }
        private async void autorefresh_Tick(object sender, EventArgs e)
        {
            InternalMemory.Cache = new();
            Core.Entities = new();
        }
        private void Form2_Load(object sender, EventArgs e)
        {


            this.TopMost = true;

        }

        public static void ResetMatchTimer()
        {
            Config.EnemyTimerStart = DateTime.Now;
            Config.EnemyTimerActive = true;
            Config.EnemyTimerEnded = false;
            Config.MatchTimerReset = false;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Match Timer Reset to 3:00");
        }

        bool startk = false;

        private CancellationTokenSource _cts;

        private void UpdateStatuss(string statusText, Color Color)
        {
            status.Text = statusText;

        }

        static IntPtr FindRenderWindow(IntPtr parent)
        {
            IntPtr renderWindow = IntPtr.Zero;
            WinAPI.EnumChildWindows(parent, (hWnd, lParam) =>
            {
                StringBuilder sb = new StringBuilder(256);
                WinAPI.GetWindowText(hWnd, sb, sb.Capacity);
                string windowName = sb.ToString();
                if (!string.IsNullOrEmpty(windowName))
                {
                    if (windowName != "HD-Player")
                    {
                        renderWindow = hWnd;
                    }
                }
                return true;
            }, IntPtr.Zero);

            return renderWindow;
        }

        [DllImport("user32.dll")]
        static extern uint SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);

        const uint WDA_NONE = 0x00000000;
        const uint WDA_MONITOR = 0x00000001;
        const uint WDA_EXCLUDEFROMCAPTURE = 0x00000011;

        public static api FearAuthApp = new api(
    name: "SILENTEXE",
    ownerid: "a0ff35d7-d709-49ef-aed8-4c0ab769477d",
    secret: "c5d4d39f75186945fd1d99f036e4aa8beee07f6e3ed8f522",
    version: "1.0"

 );

        private async void guna2Button6_Click(object sender, EventArgs e)
        {
            string key = txtLicense.Text.Trim();

            if (string.IsNullOrEmpty(key))
            {
                MessageBox.Show("Please enter your license key!");
                return;
            }

            try
            {

                FearAuthApp.init();

                FearAuthApp.license(key);

                if (FearAuthApp.response.success)
                {

                    guna2Panel1.Hide();


                }
                else
                {
                    MessageBox.Show("Invalid License Key!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }



        private void passwordtxt_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_HOTKEY)
            {
                int id = (int)m.WParam;

                if (id == Hide_HOTKEY_ID)
                {
                    HotkeyHide();
                }
                else if (id == Speed_HOTKEY_ID)
                {

                }
                else if (id == Camera_HOTKEY_ID)
                {

                }

            }
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                guna2CustomCheckBox19_Click(null, null);
            }

        }

        private void HotkeyHide()
        {
            if (formularioVisible)
            {
                this.Hide();
                formularioVisible = false;
            }
            else
            {
                this.Show();
                formularioVisible = true;
            }
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (waitingForHidehackKey)
            {
                UnregisterHotKey(this.Handle, Hide_HOTKEY_ID);
                HideKey = keyData;

                RegisterHotKey(this.Handle, Hide_HOTKEY_ID, 0, (int)HideKey);
                waitingForHidehackKey = false;
                return true;
            }
            else if (waitingForSpeedhackKey)
            {
                SpeedHotKey = keyData;
                RegisterHotKey(this.Handle, Speed_HOTKEY_ID, 0, (int)SpeedHotKey);
                waitingForSpeedhackKey = false;
                return true;
            }
            else if (waitingForCamerahackKey)
            {
                CameraHotKey = keyData;
                RegisterHotKey(this.Handle, Speed_HOTKEY_ID, 0, (int)CameraHotKey);
                waitingForCamerahackKey = false;
                return true;
            }
            else if (waitingForAwmKey)
            {
                AwmHotKey = keyData;
                Config.AwmKey = keyData;
                waitingForAwmKey = false;
                status.Text = $"AWM Key: {keyData}";
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (waitingForHidehackKey)
            {
                UnregisterHotKey(this.Handle, Hide_HOTKEY_ID);
            }
            if (waitingForSpeedhackKey)
            {
                UnregisterHotKey(this.Handle, Speed_HOTKEY_ID);
            }
            if (waitingForCamerahackKey)
            {
                UnregisterHotKey(this.Handle, Camera_HOTKEY_ID);
            }

        }

        private void label33_Click(object sender, EventArgs e)
        {

        }

        private string GetProcessPath(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length == 0)
                throw new Exception($"Process {processName} not found.");

            return processes[0].MainModule.FileName;
        }

        private void statuslbl_Click(object sender, EventArgs e)
        {

        }

        private async Task<bool> ApplyMemoryPatch(CX memory, string[] searchPatterns, string[] replacePatterns)
        {
            bool success = false;

            for (int i = 0; i < searchPatterns.Length; i++)
            {
                if (string.IsNullOrEmpty(searchPatterns[i]) || string.IsNullOrEmpty(replacePatterns[i])) continue;

                var matches = await memory.AoBScan(searchPatterns[i]);
                if (matches.Any())
                {
                    foreach (long id in matches)
                    {
                        memory.AobReplace(id, replacePatterns[i]);
                    }
                    UpdateStatus($"Memory Patch Applied", Color.Green);
                    success = true;
                }
                else
                {
                    UpdateStatus($"Memory Patch Failed", Color.Red);
                }
            }

            return success;
        }

        private async void guna2Button4_Click(object sender, EventArgs e)
        {

        }
        private void UpdateStatus(string message, Color color)
        {
            if (status.InvokeRequired)
            {
                status.Invoke(new Action(() =>
                {
                    status.Text = message;
                    status.ForeColor = color;
                }));
            }
            else
            {
                status.Text = message;
                status.ForeColor = color;
            }
        }

        private void Streammodecheckbox_Click(object sender, EventArgs e)
        {

        }

        private bool isProcessRunning = false;

        private void UpdateUI(Control activePanel, params Guna.UI2.WinForms.Guna2ImageButton[] activeButtons)
        {

        }

        private void homebutton_Click(object sender, EventArgs e)
        {

        }

        private void AimbotButton_Click(object sender, EventArgs e)
        {

        }

        private void EspButton_Click(object sender, EventArgs e)
        {

        }

        private void EspColorButton_Click(object sender, EventArgs e)
        {

        }

        private void BrutalButton_Click(object sender, EventArgs e)
        {

        }

        private async void guna2CustomCheckBox20_Click(object sender, EventArgs e)

        {

        }

        private void aimbotmodesbox_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void AimbotCheckBox_Click(object sender, EventArgs e)
        {

        }

        private void AimbotLegitCheckBox_Click(object sender, EventArgs e)
        {

        }

        private void NoRecoilCheckBox_Click(object sender, EventArgs e)
        {

        }

        private void IgnoreDownCheckBox_Click(object sender, EventArgs e)
        {

        }

        private void EspBoxComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void EspBoxFillCheckBox_Click(object sender, EventArgs e)
        {

        }

        private void EspInfoCheckBox_Click(object sender, EventArgs e)
        {

        }

        private void EspLineCheckBox_Click(object sender, EventArgs e)
        {

        }

        private void EspHealthCheckBox_Click(object sender, EventArgs e)
        {

        }

        private void EspMatchTimerCheckBox_Click(object sender, EventArgs e)
        {

        }

        private void MatchTimerDurationNumericUpDown_ValueChanged(object sender, EventArgs e)
        {

        }

        private void EspInfoV2CheckBox_Click(object sender, EventArgs e)
        {

        }

        private void EspLineRGBCheckBox_Click(object sender, EventArgs e)
        {

        }

        private void EspFovCheckBox_Click(object sender, EventArgs e)
        {

        }

        private void UpdateStatus(bool isEnabled, string feature)
        {
            UpdateStatus($"Status : {feature} {(isEnabled ? "Activated" : "Disabled")}", isEnabled ? Color.Green : Color.Red);
        }

        private void EspBoxColorPicker_Click(object sender, EventArgs e)
        {


        }

        private void EspBoxFillColorPicker_Click(object sender, EventArgs e)
        {

        }

        private void EspLineColorPicker_Click(object sender, EventArgs e)
        {

        }

        private void EspFovColorPicker_Click(object sender, EventArgs e)
        {

        }

        private void DrawMinimapCheckBox_Click(object sender, EventArgs e)
        {
        }

        private void AimFovCheckBox_Click(object sender, EventArgs e)
        {

        }

        private void guna2TrackBar1_Scroll(object sender, ScrollEventArgs e)
        {

        }

        private void EspSkeletonCheckBox_Click(object sender, EventArgs e)
        {

        }

        private void EspSkeletonColorPicker_Click(object sender, EventArgs e)
        {

        }

        private async void FixEspGlitchCheckBox_Click(object sender, EventArgs e)
        {

        }

        private void AutoFixEspGlitchCheckBox_Click(object sender, EventArgs e)
        {
            Config.Notif();
            autorefresh.Interval = 1000;

        }

        private void AimbotSleepComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label101_Click(object sender, EventArgs e)
        {
            Config.Notif();
            waitingForSpeedhackKey = true;
        }

        private void guna2Panel12_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2CustomCheckBox2_Click(object sender, EventArgs e)
        {

        }

        private void guna2Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            Environment.Exit(-1);
            Application.Exit();
        }

        private void HomePanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {

        }

        private async void guna2CustomCheckBox1_Click(object sender, EventArgs e)
        {

        }

        private void labelKeybind_Click(object sender, EventArgs e)
        {

        }

        private async void guna2CustomCheckBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private async void guna2CustomCheckBox1_Click_1(object sender, EventArgs e)
        {

        }

        private void chkRememberMe_Click(object sender, EventArgs e)
        {


        }

        private async void guna2CustomCheckBox1_Click_2(object sender, EventArgs e)
        {

        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            UnregisterHotKey(this.Handle, HOTKEY_ID);
        }

        private void label65_Click(object sender, EventArgs e)
        {

        }

        private async void guna2CustomCheckBox2_Click_1(object sender, EventArgs e)
        {

        }

        private async void guna2CustomCheckBox3_Click(object sender, EventArgs e)
        {
        }

        private async void guna2CustomCheckBox4_Click(object sender, EventArgs e)
        {

        }

        private void guna2Separator1_Click(object sender, EventArgs e)
        {

        }

        private void guna2Panel16_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {


        }

        private void guna2Button5_Click(object sender, EventArgs e)
        {

        }

        private void label38_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button4_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox17_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox16_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox15_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox14_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button2_Click_1(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private async void guna2CustomCheckBox3_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void guna2CustomCheckBox4_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox5_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox6_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox7_Click(object sender, EventArgs e)
        {

        }

        private void guna2TrackBar4_Scroll(object sender, ScrollEventArgs e)
        {

        }

        private void guna2TrackBar2_Scroll(object sender, ScrollEventArgs e)
        {

        }

        private void guna2Panel33_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox13_Click(object sender, EventArgs e)
        {

        }

        private async void guna2CustomCheckBox21_Click(object sender, EventArgs e)
        {

        }

        private async void label93_Click(object sender, EventArgs e)
        {

        }

        private void label94_Click(object sender, EventArgs e)
        {

        }

        private async void Speed30X_Click(object sender, EventArgs e)
        {

        }

        private void label58_Click(object sender, EventArgs e)
        {
            Config.Notif();
            waitingForSpeedhackKey = true;
        }

        private bool processOpened = false;
        private bool isProcessing = false;
        private async void guna2CustomCheckBox19_Click(object sender, EventArgs e)
        {
        }

        private void guna2Panel54_Paint(object sender, PaintEventArgs e)
        {

        }

        private async void guna2CustomCheckBox18_Click(object sender, EventArgs e)
        {
        }

        private void guna2Panel51_Paint(object sender, PaintEventArgs e)
        {

        }

        private async void guna2CustomCheckBox26_Click(object sender, EventArgs e)
        {

        }

        private async void guna2CustomCheckBox25_Click(object sender, EventArgs e)
        {

        }

        private async void guna2CustomCheckBox27_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button7_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void guna2Button8_Click(object sender, EventArgs e)
        {
            Config.Notif();
            try
            {
                var processes = Process.GetProcessesByName("HD-Player");

                foreach (var process in processes)
                {
                    process.Kill();
                    process.WaitForExit();
                }

            }
            catch (Exception ex)
            {

            }

            Application.Exit();
            Environment.Exit(10);
        }

        private void guna2Button1_Click_1(object sender, EventArgs e)
        {

            Config.Notif();
            try
            {
                var processes = Process.GetProcessesByName("HD-Player");

                foreach (var process in processes)
                {
                    process.Kill();
                    process.WaitForExit();
                }

            }
            catch (Exception ex)
            {

            }

            Application.Exit();
            Environment.Exit(10);
        }

        private void guna2Panel46_Paint(object sender, PaintEventArgs e)
        {

        }
        static void DeleteFirewallRule(string programPath)
        {
            ExecuteCommand("netsh advfirewall firewall delete rule name=all program=\"" + programPath + "\"");
        }
        static void ExecuteCommand(string command)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c " + command,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process = new Process { StartInfo = startInfo };
            process.Start();
            process.WaitForExit();
        }
        private void trackDrawFov_Event_1(object sender, ScrollEventArgs e)
        {
            var aimfov1 = trackDrawFov.Value;
            Config.AimBotFov = aimfov1;
            trackDrawFov.Text = trackDrawFov.Value.ToString();
        }

        private async void guna2CustomCheckBox11_Click(object sender, EventArgs e)
        {
        }

        private void guna2CustomCheckBox8_Click(object sender, EventArgs e)
        {
        }

        private void guna2CustomCheckBox9_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox10_Click(object sender, EventArgs e)
        {
        }

        private void label103_Click(object sender, EventArgs e)
        {

        }

        private async void guna2CustomCheckBox21_Click_1(object sender, EventArgs e)
        {

        }

        private async void guna2CustomCheckBox23_Click(object sender, EventArgs e)
        {
        }

        private async void guna2CustomCheckBox24_Click(object sender, EventArgs e)
        {

        }

        private async void guna2CustomCheckBox29_Click(object sender, EventArgs e)
        {

        }

        private async void guna2CustomCheckBox28_Click(object sender, EventArgs e)
        {

        }

        private async void guna2CustomCheckBox31_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button10_Click(object sender, EventArgs e)
        {
            listeningForKey18 = true;

        }

        private void guna2TextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2Panel30_Paint(object sender, PaintEventArgs e)
        {

        }

        private void silent_Click(object sender, EventArgs e)
        {

        }

        private void rgb_Click(object sender, EventArgs e)
        {

        }




        public void ResetEspTimer()
        {
            espTimerSeconds = 180;

        }

        private void guna2Panel55_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Panel41_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Button11_Click(object sender, EventArgs e)
        {



        }

        private void guna2vSeparator1_Click(object sender, EventArgs e)
        {

        }

        private void guna2vSeparator6_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox4_Click_2(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox32_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox4_Click_3(object sender, EventArgs e)
        {

        }

        private void guna2TrackBar3_Scroll(object sender, ScrollEventArgs e)
        {

        }

        private async void guna2CustomCheckBox4_Click_4(object sender, EventArgs e)
        {
        }

        private async Task fastfire()
        {

        }
        private async Task fastfir22e()
        {

        }

        private async Task sniperswitch()
        {

        }

        private void guna2CustomCheckBox12_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox19_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox19_Click_2(object sender, EventArgs e)
        {

        }

        private bool upPlayerWasOnBeforeDown = false;

        private async void guna2CustomCheckBox22_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox26_Click_1(object sender, EventArgs e)
        {
        }

        public FearMenu()
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.KeyDown += MainMenu_KeyDown;
            espTimer.Interval = 1000;

            espTimer.Start();

        }

        private void MainMenu_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void guna2Panel31_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Panel29_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2CustomCheckBox19_Click_3(object sender, EventArgs e)
        {

        }

        private void comboBoxMode_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void guna2Button9_Click(object sender, EventArgs e)
        {

        }

        private void label108_Click(object sender, EventArgs e)
        {

        }

        private void label112_Click(object sender, EventArgs e)
        {

        }

        private void label98_Click(object sender, EventArgs e)
        {

        }

        private async void guna2CustomCheckBox21_Click_2(object sender, EventArgs e)
        {

        }

        private void label98_Click_2(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox21_Click_3(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox24_Click_1(object sender, EventArgs e)
        {
        }

        private void label121_Click(object sender, EventArgs e)
        {

        }

        private async void guna2CustomCheckBox26_Click_2(object sender, EventArgs e)
        {

        }
        private async Task FLYHACKX999()
        {

        }
        private void guna2Panel41_Paint_1(object sender, PaintEventArgs e)
        {

        }

        private async void guna2CustomCheckBox30_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button9_Click_1(object sender, EventArgs e)
        {


        }

        private void guna2CustomCheckBox39_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox38_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox37_Click(object sender, EventArgs e)
        {

        }

        async void fastfireactivate()
        {

        }
        private void guna2CustomCheckBox36_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox35_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox34_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox33_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox32_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2ComboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private async void guna2CustomCheckBox40_Click(object sender, EventArgs e)
        {

        }

        private void guna2vSeparator7_Click(object sender, EventArgs e)
        {

        }

        private void label137_Click(object sender, EventArgs e)
        {

        }

        private void guna2Panel59_Paint(object sender, PaintEventArgs e)
        {

        }

        private void flyhack_Click(object sender, EventArgs e)
        {

        }

        private void labelFlyHackKeybind_Click(object sender, EventArgs e)
        {
            Config.Notif();
            if (sender is Label label)
            {
                flyHackKeybindLabel = label;
                label.Text = "...";
            }
            else if (flyHackKeybindLabel != null)
            {
                flyHackKeybindLabel.Text = "...";
            }
            listeningForKeyFlyHack = true;
        }

        private void guna2Panel57_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Panel56_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Panel63_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label42_Click(object sender, EventArgs e)
        {

        }

        private void guna2Panel30_Paint_1(object sender, PaintEventArgs e)
        {

        }

        private void guna2CustomCheckBox41_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox42_Click(object sender, EventArgs e)
        {

        }

        private void label37_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button10_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2Button12_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button13_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button14_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox42_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox43_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox44_Click(object sender, EventArgs e)
        {

        }

        private void label83_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox45_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox29_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox45_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox44_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox46_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox47_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox48_Click(object sender, EventArgs e)
        {

        }

        private void label118_Click(object sender, EventArgs e)
        {

        }

        private void guna2Panel30_Paint_2(object sender, PaintEventArgs e)
        {

        }

        private void guna2CustomCheckBox49_Click(object sender, EventArgs e)
        {

        }

        private void label140_Click(object sender, EventArgs e)
        {

        }

        private IEnumerable<long> speedResult;

        private void speedonext()
        {

        }
        private void guna2CustomCheckBox50_Click(object sender, EventArgs e)
        {

        }

        private async Task guna2CustomCheckBox51_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox51_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox50_Click_1(object sender, EventArgs e)
        {

        }

        private void label138_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox53_Click(object sender, EventArgs e)
        {

        }

        private void espkamenu_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Button15_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox52_Click(object sender, EventArgs e)
        {

        }

        private void label152_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox54_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox55_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox56_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox56_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox57_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox58_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox59_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox60_Click(object sender, EventArgs e)
        {

        }

        private void guna2CircleButton1_Click(object sender, EventArgs e)
        {

        }

        private void guna2CircleButton2_Click(object sender, EventArgs e)
        {

        }

        private void guna2CircleButton3_Click(object sender, EventArgs e)
        {

        }

        private void guna2CircleButton4_Click(object sender, EventArgs e)
        {

        }

        private void guna2CircleButton5_Click(object sender, EventArgs e)
        {

        }

        private void guna2CircleButton6_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button18_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button21_Click(object sender, EventArgs e)
        {
            ESP.Visible = false;
            AIM.Visible = true;
            BRUTAL.Visible = false;
            MISK.Visible = false;
            utility.Visible = false;

        }

        private void guna2Button22_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button20_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button23_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button12_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2Button24_Click(object sender, EventArgs e)
        {

            AIM.Visible = false;
            ESP.Visible = true;
            BRUTAL.Visible = false;
            MISK.Visible = false;
            utility.Visible = false;

        }
        private static Bitmap CapturePrimaryScreen()
        {
            var bounds = Screen.PrimaryScreen.Bounds;
            var bmp = new Bitmap(bounds.Width, bounds.Height);
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
            }
            return bmp;
        }
        private static byte[] CompressBitmapToTargetSize(Bitmap bmp, long targetSizeBytes)
        {
            var jpegCodec = ImageCodecInfo.GetImageEncoders().FirstOrDefault(c => c.FormatID == ImageFormat.Jpeg.Guid);
            if (jpegCodec == null) return null;
            for (int quality = 90; quality >= 30; quality -= 10)
            {
                var bytes = SaveJpegToBytes(bmp, quality, jpegCodec);
                if (bytes.Length <= targetSizeBytes) return bytes;
            }
            int[] scalePercents = new int[] { 90, 80, 70, 60, 50, 40, 30 };
            foreach (var scale in scalePercents)
            {
                int newW = bmp.Width * scale / 100;
                int newH = bmp.Height * scale / 100;
                using (var resized = new Bitmap(bmp, new Size(newW, newH)))
                {
                    for (int quality = 80; quality >= 30; quality -= 10)
                    {
                        var bytes = SaveJpegToBytes(resized, quality, jpegCodec);
                        if (bytes.Length <= targetSizeBytes) return bytes;
                    }
                }
            }
            return null;
        }
        private static byte[] SaveJpegToBytes(Bitmap bmp, int quality, ImageCodecInfo jpegCodec)
        {
            using (var ms = new MemoryStream())
            {
                var encoder = System.Drawing.Imaging.Encoder.Quality;
                using (var ep = new EncoderParameters(1))
                {
                    ep.Param[0] = new EncoderParameter(encoder, quality);
                    bmp.Save(ms, jpegCodec, ep);
                }
                return ms.ToArray();
            }
        }

        private static async Task<bool> SendFileToDiscordWebhook(Stream fileStream, string filename, string message)
        {
            try
            {
                using (var client = new HttpClient())
                using (var content = new MultipartFormDataContent())
                {
                    var payload = "{\"content\":\"" + JsonEscape(message) + "\"}";
                    content.Add(new StringContent(payload, Encoding.UTF8, "application/json"), "payload_json");

                    var fileContent = new StreamContent(fileStream);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    content.Add(fileContent, "file", filename);

                    var response = await client.PostAsync(WEBHOOK_URL, content);
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }
        private static string JsonEscape(string s)
        {
            if (s == null) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
        }



        private void guna2Button13_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox8_Click_1(object sender, EventArgs e)
        {

        }

        private void keybindmenuthis_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label79_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox9_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox8_Click_2(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox10_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox66_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox62_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox65_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox54_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2Panel50_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2ToggleSwitch1_CheckedChanged(object sender, EventArgs e)
        {
            Config.FearSilentKill = guna2ToggleSwitch1.Checked;
            Config.NoBulletTracking = guna2ToggleSwitch1.Checked;
            status.Text = guna2ToggleSwitch1.Checked ? "Silent Aim ENABLED ! " : "Silent Aim DISABLED !";
        }

        private void guna2ToggleSwitch2_CheckedChanged(object sender, EventArgs e)
        {
            Config.IgnoreKnocked = guna2ToggleSwitch2.Checked;
            status.Text = guna2ToggleSwitch2.Checked ? "IGNORE KNOCK ENABLED ! " : "IGNORE KNOCK DISABLED !";

        }

        private void guna2ToggleSwitch4_CheckedChanged(object sender, EventArgs e)
        {
            Config.NoRecoil = guna2ToggleSwitch4.Checked;
            status.Text = guna2ToggleSwitch4.Checked ? "NO RECOIL ENABLED ! " : "NO RECOIL DISABLED !";

        }

        private void guna2ToggleSwitch3_CheckedChanged(object sender, EventArgs e)
        {
            Config.FastReload = guna2ToggleSwitch3.Checked;
            status.Text = guna2ToggleSwitch3.Checked ? "FAST RELOAD ENABLED ! " : "FAST RELOAD DISABLED !";
        }

        private void guna2ToggleSwitch8_CheckedChanged(object sender, EventArgs e)
        {
            Config.EnemyPullEnabled = guna2ToggleSwitch8.Checked;
            status.Text = guna2ToggleSwitch8.Checked ? "MEGNET PULL ENABLED ! " : "MEGNET PULL DISABLED !";
        }

        private void guna2ToggleSwitch7_CheckedChanged(object sender, EventArgs e)
        {
            Config.RapidFire = guna2ToggleSwitch7.Checked;
            status.Text = guna2ToggleSwitch7.Checked ? "RAPID FIRE ENABLED ! " : "RAPID FIRE DISABLED !";
        }

        private void guna2ToggleSwitch6_CheckedChanged(object sender, EventArgs e)
        {
            Config.forceaim = guna2ToggleSwitch6.Checked;
            status.Text = guna2ToggleSwitch6.Checked ? "FORCE AIM ENABLED ! " : "FORCE AIM DISABLED !";
        }

        private void guna2ToggleSwitch5_CheckedChanged(object sender, EventArgs e)
        {
            Config.underkill = guna2ToggleSwitch5.Checked;
            Config.down = guna2ToggleSwitch5.Checked;
            status.Text = guna2ToggleSwitch5.Checked ? "UNDERKILL ENABLED ! " : "UNDERKILL DISABLED !";
        }

        private async Task flyahackk()
        {
            Config.Notif();
            string processName = "HD-Player";
            UpdateStatus("Applying, Please Wait...", Color.Green);
            CX memoryfast = new CX();

            if (!memoryfast.SetProcess(new[] { processName }))
            {
                UpdateStatus("Status : Process Not Found", Color.Red);
                return;
            }

            string[] searchPatterns =
            {
    "AC C5 27 37 00 10 A0 E1"
};

            string[] replacePatterns =
            {
    "AC C5 A9 3F 00 10 A0 E1"
};

            try
            {
                if (guna2ToggleSwitch11.Checked)
                {
                    bool success = await ApplyMemoryPatch(memoryfast, searchPatterns, replacePatterns);
                }
                else
                {
                    bool success = await ApplyMemoryPatch(memoryfast, replacePatterns, searchPatterns);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Status : Error - {ex.Message}", Color.Red);
            }
        }

        private async void guna2ToggleSwitch11_CheckedChanged(object sender, EventArgs e)
        {
            await flyahackk();
        }

        private void guna2ToggleSwitch10_CheckedChanged(object sender, EventArgs e)
        {
            Config.teliport = guna2ToggleSwitch10.Checked;
            status.Text = guna2ToggleSwitch10.Checked ? "TELEPORT ENABLED ! " : "TELEPORT DISABLED !";
        }

        private void guna2ToggleSwitch13_CheckedChanged(object sender, EventArgs e)
        {
            Config.speedint = guna2ToggleSwitch13.Checked;
            status.Text = guna2ToggleSwitch13.Checked ? "SPEED ENABLE ! " : "SPEED DISABLED !";
        }

        private void guna2ToggleSwitch12_CheckedChanged(object sender, EventArgs e)
        {
            if (guna2ToggleSwitch12.Checked)
            {

                Config.teleportmap = true;
            }
            else
            {
                Config.teleportmap = false;
            }
        }

        private void guna2ToggleSwitch9_CheckedChanged(object sender, EventArgs e)
        {
            if (guna2ToggleSwitch9.Checked)
            {
                Config.ESPLine = true;
                Config.EspUp = true;
            }
            else
            {
                Config.ESPLine = false;
                Config.EspUp = false;
            }
        }

        private void guna2ToggleSwitch14_CheckedChanged(object sender, EventArgs e)
        {

            if (guna2ToggleSwitch14.Checked)
            {
                Config.ESPBox = true;
            }
            else
            {
                Config.ESPBox = false;
            }
        }

        private void guna2ToggleSwitch16_CheckedChanged(object sender, EventArgs e)
        {
            if (guna2ToggleSwitch16.Checked)
            {
                Config.ESPName = true;
                Config.ESPDistance = true;
                Config.ESPHealth = true;
                Config.espbg = true;
            }
            else
            {
                Config.ESPName = false;
                Config.ESPDistance = false;
                Config.ESPHealth = false;
                Config.espbg = false;
            }
        }

        private void guna2ToggleSwitch15_CheckedChanged(object sender, EventArgs e)
        {
            Config.ESPHealth = guna2ToggleSwitch15.Checked;
        }

        private void guna2ToggleSwitch20_CheckedChanged(object sender, EventArgs e)
        {
            if (guna2ToggleSwitch20.Checked)
            {
                Config.MatchTimer = true;
                Config.EnemyTimerActive = true;

            }
            else
            {
                Config.MatchTimer = false;
                Config.EnemyTimerActive = false;

            }
        }

        private void guna2ToggleSwitch19_CheckedChanged(object sender, EventArgs e)
        {
            ResetMatchTimer();
        }

        private void guna2ToggleSwitch18_CheckedChanged(object sender, EventArgs e)
        {
            Config.HackerTag = guna2ToggleSwitch18.Checked;
        }

        private void guna2ToggleSwitch17_CheckedChanged(object sender, EventArgs e)
        {
            Config.Notif();
            SetStreamMode(guna2ToggleSwitch17.Checked);
            Config.StreamMode = guna2ToggleSwitch17.Checked;
            Config.sound = guna2ToggleSwitch17.Checked;
            void SetStreamMode(bool state)
            {
                foreach (var obj in Application.OpenForms)
                {
                    var form = obj as Form;

                    if (state)
                    {
                        SetWindowDisplayAffinity(form.Handle, WDA_EXCLUDEFROMCAPTURE);

                    }
                    else
                    {

                        SetWindowDisplayAffinity(form.Handle, WDA_NONE);

                    }
                }
            }
        }

        private void guna2CircleButton8_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void guna2CircleButton7_Click(object sender, EventArgs e)
        {
            Config.Notif();
            try
            {
                var processes = Process.GetProcessesByName("HD-Player");

                foreach (var process in processes)
                {
                    process.Kill();
                    process.WaitForExit();
                }

            }
            catch (Exception ex)
            {

            }

            Application.Exit();
            Environment.Exit(10);
        }



        private async void guna2ToggleSwitch21_CheckedChanged(object sender, EventArgs e)
        {

            Config.Notif();

            if (!guna2ToggleSwitch21.Checked)
                return;

            try
            {
                if (isProcessRunning)
                {
                    UpdateStatuss("Already running...", Color.Orange);
                    return;
                }

                isProcessRunning = true;
                UpdateStatuss("ACTIVATING..", Color.Yellow);
                var processes = Process.GetProcessesByName("HD-Player");
                if (processes.Length == 0)
                {
                    UpdateStatuss("HD-Player process not found. Start the emulator first.", Color.Red);
                    isProcessRunning = false;
                    return;
                }

                var process = processes[0];
                var mainModulePath = Path.GetDirectoryName(process.MainModule.FileName);
                var adbPath = Path.Combine(mainModulePath, "HD-Adb.exe");

                if (!File.Exists(adbPath))
                {
                    UpdateStatuss("ADB not found! Ensure emulator is properly installed.", Color.Red);
                    isProcessRunning = false;
                    return;
                }
                var adb = new Adb(adbPath);
                await adb.Kill();
                await Task.Delay(800);
                UpdateStatuss("Restarting ADB service...", Color.Yellow);

                var started = await adb.Start();
                if (!started)
                {
                    UpdateStatuss("Failed to start ADB!", Color.Red);
                    isProcessRunning = false;
                    return;
                }
                string pkg = "com.dts.freefireth";
                string lib = "libil2cpp.so";
                bool isFreeFireMax = false;
                if (isFreeFireMax) pkg = "com.dts.freefiremax";

                UpdateStatuss("Waiting for game to start...", Color.Yellow);
                bool gameRunning = false;
                for (int i = 0; i < 40; i++)
                {
                    string pidCheck = RunAdbCommand(adbPath, $"shell pidof {pkg}");
                    if (!string.IsNullOrWhiteSpace(pidCheck))
                    {
                        gameRunning = true;
                        break;
                    }
                    await Task.Delay(500);
                }

                if (!gameRunning)
                {
                    UpdateStatuss("Game process not detected. Please start the game.", Color.Red);
                    isProcessRunning = false;
                    return;
                }
                UpdateStatuss("Waiting for game module to load...", Color.Yellow);
                ulong moduleAddr = 0;
                for (int i = 0; i < 40; i++)
                {
                    moduleAddr = await adb.FindModule(pkg, lib);
                    if (moduleAddr > 0 && moduleAddr <= uint.MaxValue)
                        break;
                    await Task.Delay(500);
                }

                if (moduleAddr == 0)
                {
                    UpdateStatuss("Module not found! Make sure the game is loaded.", Color.Red);
                    isProcessRunning = false;
                    return;
                }

                Offsets.Il2Cpp = (uint)moduleAddr;
                Core.Handle = FindRenderWindow(mainHandle);
                UpdateStatuss("INITIALIZING..", Color.Yellow);
                var esp = new ESP();
                await esp.Start();

                await Task.Delay(300);
                UpdateStatuss("ACTIVATING", Color.Yellow);

                new Thread(Data.Work) { IsBackground = true }.Start();
                new Thread(Aimbot.Work) { IsBackground = true }.Start();
                new Thread(SILENT.Work) { IsBackground = true }.Start();
                new Thread(AimbotAi.Work) { IsBackground = true }.Start();
                new Thread(KCBRUTALSILENT.Work) { IsBackground = true }.Start();
                new Thread(ClimbUp.Start) { IsBackground = true }.Start();
                new Thread(Tele.Work) { IsBackground = true }.Start();
                new Thread(divekill.Work) { IsBackground = true }.Start();
                new Thread(Teleport.Work) { IsBackground = true }.Start();
                new Thread(fprceaim.Start) { IsBackground = true }.Start();
                new Thread(underkill.Work) { IsBackground = true }.Start();
                new Thread(FLY.Start) { IsBackground = true }.Start();
                new Thread(Map_Teleport.Work) { IsBackground = true }.Start();
                new Thread(Teleport.Work) { IsBackground = true }.Start();
                new Thread(UpPlayer.Work) { IsBackground = true }.Start();
                new Thread(EnemyPull360.Start) { IsBackground = true }.Start();
                new Thread(ShakeKill.Work) { IsBackground = true }.Start();
                new Thread(RapidSpin.Activate) { IsBackground = true }.Start();
                new Thread(AutoTeleport.Work) { IsBackground = true }.Start();
                new Thread(AimmAWM.Work) { IsBackground = true }.Start();

                ResetEspTimer();



                UpdateStatuss("SETUP COMPLETE!", Color.Lime);
            }
            catch (Exception ex)
            {
                UpdateStatuss($"Error: {ex.Message}", Color.Red);
                isProcessRunning = false;
            }
        }

        private void guna2ToggleSwitch22_CheckedChanged(object sender, EventArgs e)
        {
            Config.spinbot = guna2ToggleSwitch22.Checked;
            status.Text = guna2ToggleSwitch22.Checked ? "SPIN BOT ENABLE! " : "SPIN BOT DISABLE!";

        }

        private void guna2ToggleSwitch23_CheckedChanged(object sender, EventArgs e)
        {
            Config.AimbotVisible = guna2ToggleSwitch23.Checked;
            status.Text = guna2ToggleSwitch23.Checked ? "AIMBOT AI ENABLE! " : "AIMBOT AI DISABLE!";
        }

        private void guna2Button14_Click_1(object sender, EventArgs e)
        {
            Core.Entities = new();
            InternalMemory.Cache = new();
        }

        private void guna2TextBox2_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void guna2ToggleSwitch24_CheckedChanged(object sender, EventArgs e)
        {
            Config.AimBot = guna2ToggleSwitch24.Checked;
            status.Text = guna2ToggleSwitch23.Checked ? "AIMBOT INTERNAL ENABLE ! " : "AIMBOT INTERNAL DISABLE !";
        }

        private void guna2ToggleSwitch25_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void guna2ToggleSwitch27_CheckedChanged(object sender, EventArgs e)
        {
            if (guna2ToggleSwitch27.Checked)
            {
                TopMost = true;
            }
            else
            {
                TopMost = false;
            }
        }

        private void guna2ToggleSwitch26_CheckedChanged(object sender, EventArgs e)
        {
            if (guna2ToggleSwitch26.Checked)
            {
                ShowInTaskbar = false;
            }
            else
            {
                ShowInTaskbar = true;
            }
        }

        private void guna2ToggleSwitch28_CheckedChanged(object sender, EventArgs e)
        {
            SetStreamMode(guna2ToggleSwitch28.Checked);
            void SetStreamMode(bool state)
            {
                foreach (var obj in Application.OpenForms)
                {
                    var form = obj as Form;

                    if (state)
                    {
                        SetWindowDisplayAffinity(form.Handle, WDA_EXCLUDEFROMCAPTURE);

                    }
                    else
                    {

                        SetWindowDisplayAffinity(form.Handle, WDA_NONE);

                    }
                }
            }
        }

        private void guna2ToggleSwitch30_CheckedChanged(object sender, EventArgs e)
        {
            Config.Notif();
            if (guna2ToggleSwitch30.Checked == true)
            {
                ExecuteCommand("netsh advfirewall firewall add rule name=\"TemporaryBlock0\" dir=in action=block profile=any program=\"C:\\Program Files\\BlueStacks_nxt\\HD-Player.exe\"");
                ExecuteCommand("netsh advfirewall firewall add rule name=\"TemporaryBlock0\" dir=out action=block profile=any program=\"C:\\Program Files\\BlueStacks_nxt\\HD-Player.exe\"");
                ExecuteCommand("netsh advfirewall firewall add rule name=\"TemporaryBlock1\" dir=in action=block profile=any program=\"C:\\Program Files\\BlueStacks_msi5\\HD-Player.exe\"");
                ExecuteCommand("netsh advfirewall firewall add rule name=\"TemporaryBlock1\" dir=out action=block profile=any program=\"C:\\Program Files\\BlueStacks_msi5\\HD-Player.exe\"");
                ExecuteCommand("netsh advfirewall firewall add rule name=\"TemporaryBlock2\" dir=in action=block profile=any program=\"C:\\Program Files\\BlueStacks\\HD-Player.exe\"");
                ExecuteCommand("netsh advfirewall firewall add rule name=\"TemporaryBlock2\" dir=out action=block profile=any program=\"C:\\Program Files\\BlueStacks\\HD-Player.exe\"");
                ExecuteCommand("netsh advfirewall firewall add rule name=\"TemporaryBlock3\" dir=in action=block profile=any program=\"C:\\Program Files\\BlueStacks_msi2\\Bluestacks.exe\"");
                ExecuteCommand("netsh advfirewall firewall add rule name=\"TemporaryBlock3\" dir=in action=block profile=any program=\"C:\\Program Files\\BlueStacks_msi2\\HD-Player.exe\"");
                ExecuteCommand("netsh advfirewall firewall add rule name=\"TemporaryBlock3\" dir=out action=block profile=any program=\"C:\\Program Files\\BlueStacks_msi2\\HD-Player.exe\"");

            }
            else
            {
                DeleteFirewallRule("C:\\Program Files\\BlueStacks_nxt\\HD-Player.exe");
                DeleteFirewallRule("C:\\Program Files\\BlueStacks_msi5\\HD-Player.exe");
                DeleteFirewallRule("C:\\Program Files\\BlueStacks\\HD-Player.exe");
                DeleteFirewallRule("C:\\Program Files\\BlueStacks_msi2\\HD-Player.exe");

            }
        }

        private void guna2ToggleSwitch29_CheckedChanged(object sender, EventArgs e)
        {
            Config.Notif();
            Config.sound = guna2ToggleSwitch29.Checked;
        }

        private void guna2ToggleSwitch31_CheckedChanged(object sender, EventArgs e)
        {
            Config.ESPRefresh = guna2ToggleSwitch31.Checked;

            if (guna2ToggleSwitch31.Checked)
            {
                Core.Entities = new();
                InternalMemory.Cache = new();

                status.Text = "ESP REFRESH ENABLE !";
            }
            else
            {
                status.Text = "ESP REFRESH DISABLE !";
            }
        }

        private void guna2Button18_Click_1(object sender, EventArgs e)
        {
            AIM.Visible = false;
            ESP.Visible = false;
            BRUTAL.Visible = true;
            MISK.Visible = false;
            utility.Visible = false;
        }

        private void guna2Button15_Click_1(object sender, EventArgs e)
        {
            AIM.Visible = false;
            ESP.Visible = false;
            BRUTAL.Visible = false;
            MISK.Visible = true;
            utility.Visible = false;
        }

        private void guna2ControlBox1_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
            Application.Exit();
        }

        private void guna2ControlBox2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void status_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomGradientPanel16_Paint(object sender, PaintEventArgs e)
        {

        }

        private async void guna2ToggleSwitch32_CheckedChanged(object sender, EventArgs e)
        {

        }

        // Slider Event Handlers
        private void trackBarFloatClient1_ValueChanged(object sender, EventArgs e)
        {
            if (trackBarFloatClient1 != null)
            {
                Config.AimBotFov = (int)trackBarFloatClient1.Value;
                if (status != null) status.Text = $"Aim FOV: {Config.AimBotFov}";
            }
        }

        private void trackBarFloatClient2_ValueChanged(object sender, EventArgs e)
        {
            if (trackBarFloatClient2 != null)
            {
                Config.AimBotMaxDistance = (int)trackBarFloatClient2.Value;
                if (status != null) status.Text = $"Aim Distance: {Config.AimBotMaxDistance}";
            }
        }

        private void trackBarFloatClient3_ValueChanged(object sender, EventArgs e)
        {
            if (trackBarFloatClient3 != null)
            {
                Config.AimbotSmoothness = (int)trackBarFloatClient3.Value;
                if (status != null) status.Text = $"Aim Smoothness: {Config.AimbotSmoothness}";
            }
        }

        private void trackBarFloatClient4_ValueChanged(object sender, EventArgs e)
        {
            if (trackBarFloatClient4 != null)
            {
                Config.RapidFireSpeed = trackBarFloatClient4.Value;
                if (status != null) status.Text = $"Rapid Fire: {Config.RapidFireSpeed:F1}";
            }
        }

        private void trackBarFloatClient5_ValueChanged(object sender, EventArgs e)
        {
            if (trackBarFloatClient5 != null)
            {
                Config.SpinSpeed = trackBarFloatClient5.Value;
                if (status != null) status.Text = $"Spinbot Speed: {Config.SpinSpeed:F1}";
            }
        }

        private void guna2Button2_Click_2(object sender, EventArgs e)
        {
            AIM.Visible = false;
            ESP.Visible = false;
            BRUTAL.Visible = false;
            MISK.Visible = false;
            utility.Visible = true;
            keybind.Visible = false;
        }

        private void guna2Button1_Click_2(object sender, EventArgs e)
        {
            AIM.Visible = false;
        }



        private void guna2ToggleSwitch25_CheckedChanged_2(object sender, EventArgs e)
        {
            if (guna2ToggleSwitch25 != null)
            {
                Config.AimFov = guna2ToggleSwitch25.Checked;
                Config.Aimfovc = guna2ToggleSwitch25.Checked; // This variable actually controls drawing in ESP.cs
                Config.Aimfovcolor = Color.White;             // Set color to Pure White
            }
        }

        private void label7_Click(object sender, EventArgs e)
        {
            // Auto-generated from Designer click on AIM FOV label
        }

        private void guna2Panel2_Paint_1(object sender, PaintEventArgs e)
        {

        }

        private void txtLicense_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2ToggleSwitch32_CheckedChanged_1(object sender, EventArgs e)
        {
            Config.AimmmAwm = guna2ToggleSwitch32.Checked;
            status.Text = guna2ToggleSwitch32.Checked ? "AWM SNIPER AIM ENABLE !" : "AWM SNIPER AIM DISABLE !";
        }

        // AWM Keybind button - Designer mein kisi button/label pe double click karke is method se link karo
        // Click karo → "Press Any Key..." dikhega → jo bhi key dabao woh AWM key set ho jaayegi
        internal void AwmKeyBind_Click(object sender, EventArgs e)
        {
            waitingForAwmKey = true;
            status.Text = $"AWM Key: Press Any Key... (Current: {Config.AwmKey})";
        }
    }
}
