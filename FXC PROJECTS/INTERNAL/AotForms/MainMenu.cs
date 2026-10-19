using AotForms;
using Client;
using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AotForms.MouseHook;
using System.Runtime.InteropServices;
using System.Media;
using System.Reflection;
using Microsoft.VisualBasic.Logging;
using FearAuth;
using Beyondmem;
using System.Net;
using CXMem;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace AotForms
{
    public partial class MainMenu : Form
    {

        public static api FearAuthApp = new api(
          name: ("INTERNAL"),
             ownerid: ("74fd614b-01ec-4d17-aa39-e7fe953ef00f"),
             secret: ("307e1bed14cf5e33f021dc35c4dfdead01fedebd285702d4"),
             version: ("1.0"));
        IntPtr mainHandle;
        bool keybind1 = false;
        bool shift;
        //private AimbotForm aimform = new AimbotForm();


        bool aimbothks;
        bool esphks;
        bool confighks;

        private void PlaySoundFromUrl(string urlSom)
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    // Baixar o som para um stream de memória
                    byte[] data = client.DownloadData(urlSom);

                    // Criar um stream a partir dos dados baixados
                    using (MemoryStream ms = new MemoryStream(data))
                    {
                        // Tocar o som diretamente do stream
                        SoundPlayer player = new SoundPlayer(ms);
                        player.Play(); // Ou player.PlaySync() se quiser esperar o som terminar
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro ao carregar o som: " + ex.Message);
                }
            }
        }

        public MainMenu(IntPtr handle)
        {
            FearAuthApp.init();
            mainHandle = handle;
            InitializeComponent();

            // Set the mouse hook
            MouseHook.SetHook();
            MouseHook.LeftButtonDown += MouseHook_LButtonDown;
            MouseHook.LeftButtonUp += MouseHook_LButtonUp;
            Application.ApplicationExit += (sender, e) => MouseHook.Unhook();


        }
        CaXMem memory = new CaXMem();
        MemMirza MemLib = new MemMirza();


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
                Config.Notif(); // Substitua pela URL correta

                if (keybind1 == false)
                {
                    aimbothks = false;
                    esphks = false;
                    confighks = false;
                    this.Hide();
                    //aimform.Hide();


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

        private void Form2_Load(object sender, EventArgs e)
        {
            FearAuthApp.init();
            FearAuthApp.check();
            this.TopMost = true;
            guna2BorderlessForm1.BorderRadius = 0;
        }
        bool startk = false;

        private CancellationTokenSource _cts;
        private async void MouseHook_LButtonDown()
        {
            if (startk)
            {
                _cts = new CancellationTokenSource();
                try
                {
                    await Task.Delay(Config.Aimbotype, _cts.Token); // Wait for 5 seconds or until canceled
                    Config.AimBotLeft = true;
                }
                catch (TaskCanceledException)
                {
                    // Task was canceled, do nothing
                }
            }

        }

        private void MouseHook_LButtonUp()
        {
            if (startk)
            {
                if (_cts != null)
                {
                    _cts.Cancel(); // Cancel the delay when the button is released
                    Config.AimBotLeft = false;
                }
            }
        }






        private async void guna2Button1_Click(object sender, EventArgs e)
        {
            var processes = Process.GetProcessesByName("HD-Player");

            if (processes.Length != 1)
            {
                MessageBox.Show("Open Your Emulator !");
                return;
            }

            var process = processes[0];
            var mainModulePath = Path.GetDirectoryName(process.MainModule.FileName);
            var adbPath = Path.Combine(mainModulePath, "HD-Adb.exe");

            if (!File.Exists(adbPath))
            {
                MessageBox.Show("Reinstall Emulator");
                return;
            }


            var adb = new Adb(adbPath);
            await adb.Kill();

            var started = await adb.Start();
            if (!started)
            {

                MessageBox.Show("Adb ERROR");
                Application.Exit();
                return;
            }

            var moduleAddr = await adb.FindModule("com.dts.freefireth", "libil2cpp.so");

            Offsets.Il2Cpp = moduleAddr;
            Core.Handle = FindRenderWindow(mainHandle);

            var esp = new ESP();
            await esp.Start();

            new Thread(Data.Work) { IsBackground = true }.Start();
            new Thread(Aimbot.Work) { IsBackground = true }.Start();

            //lblStatus.Text = "Online - " + moduleAddr.ToString("X");

            Console.Beep(800, 400);


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
        private void guna2ToggleSwitch1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox1_Click(object sender, EventArgs e)
        {

        }


        private void guna2Button2_Click(object sender, EventArgs e)
        {
            //Config.Notif();
            //if (!aimbothks)
            //{
            //    aimform.StartPosition = FormStartPosition.Manual;
            //    int spacing = 10;
            //    int form2X = this.Location.X + this.Width + spacing;
            //    int form2Y = this.Location.Y;
            //    aimform.Location = new Point(form2X, form2Y);
            //    aimform.Show();

            //    aimbothks = true;
            //}
            //else
            //{
            //    aimform.Hide();

            //    aimbothks = false;
            //}
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            Config.Notif();
            if (!esphks)
            {

                int spacing = 10;
                int form2X = this.Location.X + this.Width + spacing;
                int form2Y = this.Location.Y;


                esphks = true;
            }
            else
            {


                esphks = false;
            }

        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {
            Config.Notif();
            if (!confighks)
            {

                int spacing = 10;
                int form2X = this.Location.X + this.Width + spacing;
                int form2Y = this.Location.Y;


                confighks = true;
            }
            else
            {

                confighks = false;
            }
        }

        private void guna2Button5_Click(object sender, EventArgs e)
        {
            Config.Notif();
            Core.Entities = new();
            InternalMemory.Cache = new();
        }




        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {
            try
            {
                Config.Notif();
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "",
                    UseShellExecute = true // Ensures that it opens the URL in the default browser
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private async void guna2Button6_Click(object sender, EventArgs e)
        {
            Config.Notif();
            statuslbl.Text = "Status: Verifying User...";

            await Task.Delay(300);
            var loginTask = Task.Run(() => FearAuthApp.login(username.Text, password.Text));
            await loginTask;
            FearAuthApp.login(username.Text, password.Text);
            if (FearAuthApp.response.success)
            {
                // Cria uma nova instância do Form1 com o handle apropriado e mostra o formulário
                AimbotForm ML = new AimbotForm(mainHandle);
                ML.Show();
                // Esconde o formulário atual
                this.Hide();
            }
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
        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2CustomCheckBox3_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void guna2Button9_Click(object sender, EventArgs e)
        {
            WindowState ^= FormWindowState.Minimized;
        }

        private void guna2Panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void guna2PictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox34_Click(object sender, EventArgs e)
        {
           
        }

        private void password_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {






        }

        private void statuslbl_Click(object sender, EventArgs e)
        {

        }
    }
}

