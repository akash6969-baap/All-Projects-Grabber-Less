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
using Client;
using Reborn;

namespace AotForms
{
    public partial class Form1 : Form
    {
        IntPtr mainHandle;
        public Form1(IntPtr handle)
        {

            InitializeComponent();
            mainHandle = handle;
        }

        public static api KeyAuthApp = new api(
                 name: "Staffpannel's Application",
                 ownerid: "tDn9wygy1y",
                 secret: "c03e0deba441e23e9ddb5634ab8384956c0f1d54281469d57483f660ee8dd577",
                 version: "1.0"

             );
        private async void Form1_Load(object sender, EventArgs e)
        {
            KeyAuthApp.init();
            KeyAuthApp.check();
            var processes = Process.GetProcessesByName("HD-Player");

            if (processes.Length != 1)
            {
                MessageBox.Show("Open emulator.");
                return;
            }

            var process = processes[0];
            string mainModulePath = Path.GetDirectoryName(process.MainModule?.FileName);

            if (string.IsNullOrEmpty(mainModulePath))
            {
                MessageBox.Show("Reinstall emulator.");
                return;
            }

            var adbPath = Path.Combine(mainModulePath, "HD-Adb.exe");
            if (!File.Exists(adbPath))
            {
                MessageBox.Show("Adb not Found. Reinstall emulator.");
                return;
            }



            var adb = new Adb(adbPath);
            await adb.Kill();
            //await Task.Delay(500);
            if (!await adb.Start())
            {
                MessageBox.Show("Adb Error");
                Environment.Exit(0);
                return;
            }
            //await Task.Delay(500);
            var moduleAddr = await adb.FindModule("com.dts.freefireth", "libil2cpp.so");

            if (moduleAddr == 0)
            {
                MessageBox.Show("Go to Lobby then Logout then try to apply.");
                Environment.Exit(0);
                return;
            }

            Offsets.Il2Cpp = moduleAddr;
            Core.Handle = FindRenderWindow(mainHandle);
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

        private async void guna2Button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Verifying login, wait for a moment...", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);

            KeyAuthApp.login(usernametxt.Text, passwordtxt.Text);

            if (KeyAuthApp.response.success)
            {
                KeyAuthApp.login(usernametxt.Text, passwordtxt.Text);
                if (KeyAuthApp.response.success)
                {
                    var esp = new ESP();
                    await esp.Start();

                    new Thread(Data.Work) { IsBackground = true }.Start();
                    new Thread(AimbotV2.Work) { IsBackground = true }.Start();
                    new Thread(UpPlayer.Work) { IsBackground = true }.Start();
                    new Thread(TeleKil.Work) { IsBackground = true }.Start();
                    Main ML = new Main(mainHandle);
                    ML.Show();
                    this.Hide();


                }
                else
                {
                    MessageBox.Show("Login failed. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void guna2PictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void passwordtxt_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
