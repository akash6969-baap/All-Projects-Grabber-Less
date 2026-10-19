using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ailo;
using Client;

namespace AotForms
{
    public partial class Main : Form
    {
        IntPtr mainHandle;
        public Main(IntPtr handle)
        {
            mainHandle = handle;
            InitializeComponent();
        }

        private void enableaimcheckbox_Click(object sender, EventArgs e)
        {
            Config.enableAimBot = enableaimcheckbox.Checked;
        }

        private void upplayercheckbox_Click(object sender, EventArgs e)
        {
            Config.UpPlayer = upplayercheckbox.Checked;
        }

        private void telekillcheckbox_Click(object sender, EventArgs e)
        {
            Config.teli = telekillcheckbox.Checked;
        }

        private void norecoilcheckbox_Click(object sender, EventArgs e)
        {
            Config.NoRecoil = norecoilcheckbox.Checked;
        }

        private void ignoreknockedcheckbox_Click(object sender, EventArgs e)
        {
            Config.IgnoreKnocked = ignoreknockedcheckbox.Checked;
        }

        private void showcirclefovcheckbox_Click(object sender, EventArgs e)
        {
            Config.FOVEnabled = showcirclefovcheckbox.Checked;
        }

        private void guna2TrackBar1_Scroll(object sender, ScrollEventArgs e)
        {
            var distance = guna2TrackBar1.Value;

            label7.Text = $"AimFov: {distance}";

            Config.AimFov = distance;
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
        private async void enablehookbtn_Click(object sender, EventArgs e)
        {

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

        private void guna2ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void guna2ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            string selectedItem = (string)comboBox.SelectedItem;

            switch (selectedItem)
            {
                case "Rage":
                    Config.AimBotRage = true;
                    Config.SilentAim = false;
                    break;

                case "Silent":
                    Config.AimBotRage = false;
                    Config.SilentAim = true;
                    break;
            }
        }

        private void esplinecheckbox_Click(object sender, EventArgs e)
        {
            Config.ESPLine = esplinecheckbox.Checked;
        }

        private void espboxcheckbox_Click(object sender, EventArgs e)
        {
            Config.ESPBox = espboxcheckbox.Checked;
        }

        private void espcornercheckbox_Click(object sender, EventArgs e)
        {
            Config.ESPBox2 = espcornercheckbox.Checked;
        }

        private void esphealthcheckbox_Click(object sender, EventArgs e)
        {
            Config.ESPHealth = esphealthcheckbox.Checked;
        }

        private void guna2CustomCheckBox1_Click(object sender, EventArgs e)
        {
            Config.linetargetclose = guna2CustomCheckBox1.Checked;
        }

        private void espnamecheckbox_Click(object sender, EventArgs e)
        {
            Config.ESPName = espnamecheckbox.Checked;
        }

        private void guna2ComboBox2_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            string selectedItem = (string)comboBox.SelectedItem;

            switch (selectedItem)
            {
                case "ClosestToCrosshair":
                    Config.TargetingMode = TargetingMode.ClosestToCrosshair;
                    break;

                case "Target360":
                    Config.TargetingMode1 = TargetingMode.Target360;
                    break;

                case "ClosestToPlayer":
                    Config.TargetingMode2 = TargetingMode.ClosestToPlayer;
                    break;

                case "LowestHealth":
                    Config.TargetingMode3 = TargetingMode.LowestHealth;
                    break;
            }
        }

        private void espdistancecheckbox_Click(object sender, EventArgs e)
        {
            Config.ESPDistance = espdistancecheckbox.Checked;
        }

        private void espfillboxcheckbox_Click(object sender, EventArgs e)
        {
            Config.ESPFillBox = espfillboxcheckbox.Checked;
        }
        private void NoCache()
        {
            InternalMemory.Cache = new();
            Core.Entities = new();
            Thread.Sleep(1000);
        }
        private void UpdateEntities()
        {
            foreach (var entity in Core.Entities.Values)
            {
                if (entity.IsTeam != Bool3.False) continue;

                TreeNode entityNode = new TreeNode(entity.Name);

                entityNode.Nodes.Add(new TreeNode($"IsKnown: {entity.IsKnown}"));
                entityNode.Nodes.Add(new TreeNode($"IsTeam: {entity.IsTeam}"));
                entityNode.Nodes.Add(new TreeNode($"Head: {entity.Head}"));
                entityNode.Nodes.Add(new TreeNode($"Root: {entity.Root}"));
                entityNode.Nodes.Add(new TreeNode($"Health: {entity.Health}"));
                entityNode.Nodes.Add(new TreeNode($"IsDead: {entity.IsDead}"));
                entityNode.Nodes.Add(new TreeNode($"IsKnocked: {entity.IsKnocked}"));
            }
            Thread.Sleep(1000);
        }
        private void updateentitybtn_Click(object sender, EventArgs e)
        {
            UpdateEntities();
        }
        bool resetCacheChecked = false;
        System.Timers.Timer timer = new System.Timers.Timer(10000); // 15 seconds
        void SetupTimer()
        {
            timer.Elapsed += (sender, e) =>
            {
                resetCacheChecked = true;
            };
            timer.AutoReset = true;
            timer.Start();
        }
        private void resetcachebtn_Click(object sender, EventArgs e)
        {
            NoCache();
        }
        static void ResetCache()
        {
            Core.Entities = new();
            InternalMemory.Cache = new();
        }

        private void guna2CustomCheckBox3_Click(object sender, EventArgs e)
        {
            if (resetCacheChecked)
            {
                Core.HaveMatrix = true;
                SetupTimer();
                ResetCache();
                resetCacheChecked = false;
            }
        }


        private void anticheatbtn_Click(object sender, EventArgs e)
        {

        }


        private void linecolorpicker_Click(object sender, EventArgs e)
        {
            var picker = new ColorDialog();
            var result = picker.ShowDialog();

            if (result == DialogResult.OK)
            {
                linecolorpicker.FillColor = picker.Color;
                Config.ESPLineColor = picker.Color;
            }
        }

        private void boxcolorpicker_Click(object sender, EventArgs e)
        {
            var picker = new ColorDialog();
            var result = picker.ShowDialog();

            if (result == DialogResult.OK)
            {
                boxcolorpicker.FillColor = picker.Color;
                Config.ESPBoxColor = picker.Color;
            }
        }

        private void cornerboxcolorpicker_Click(object sender, EventArgs e)
        {
            var picker = new ColorDialog();
            var result = picker.ShowDialog();

            if (result == DialogResult.OK)
            {
                cornerboxcolorpicker.FillColor = picker.Color;
                Config.ESPBoxColor = picker.Color;
            }
        }

        private void fillboxcolorpicker_Click(object sender, EventArgs e)
        {
            var picker = new ColorDialog();
            var result = picker.ShowDialog();

            if (result == DialogResult.OK)
            {
                fillboxcolorpicker.FillColor = picker.Color;
                Config.ESPFillBoxColor = picker.Color;
            }
        }

        private void namecolorpicker_Click(object sender, EventArgs e)
        {
            var picker = new ColorDialog();
            var result = picker.ShowDialog();

            if (result == DialogResult.OK)
            {
                namecolorpicker.FillColor = picker.Color;
                Config.ESPNameColor = picker.Color;
            }
        }

        private void distancecolorpicker_Click(object sender, EventArgs e)
        {
            var picker = new ColorDialog();
            var result = picker.ShowDialog();

            if (result == DialogResult.OK)
            {
                distancecolorpicker.FillColor = picker.Color;
                Config.ESPDistanceColor = picker.Color;
            }
        }

        private void guna2CustomCheckBox2_Click(object sender, EventArgs e)
        {
            Config.ESPSkeleton = guna2CustomCheckBox2.Checked;
        }

        private void healthcolorpicker_Click(object sender, EventArgs e)
        {
            var picker = new ColorDialog();
            var result = picker.ShowDialog();

            if (result == DialogResult.OK)
            {
                healthcolorpicker.FillColor = picker.Color;
                Config.ESPHealthColor = picker.Color;
            }
        }

        private void skeletoncolorpicker_Click(object sender, EventArgs e)
        {
            var picker = new ColorDialog();
            var result = picker.ShowDialog();

            if (result == DialogResult.OK)
            {
                skeletoncolorpicker.FillColor = picker.Color;
                Config.ESPSkeletonColor = picker.Color;
            }
        }

        private void targetaimcolorpicker_Click(object sender, EventArgs e)
        {
            var picker = new ColorDialog();
            var result = picker.ShowDialog();

            if (result == DialogResult.OK)
            {
                targetaimcolorpicker.FillColor = picker.Color;
                Config.ESPLineColor = picker.Color;
            }
        }
        private bool Inject(string pattern)
        {
            try
            {
                if (!a.Any())
                { sta.Text = "LOAD FIRST"; return false; }
                var p = Process.GetProcessesByName("HD-Player");
                if (p.Length == 0 || !AiloFuck.OpenProcess(p[0].Id))
                { sta.Text = "HD-PLAYER not found"; return false; }
                foreach (var addr in a)
                    if (!AiloFuck.WriteMemory(addr.ToString("X"), "bytes", pattern, "", null))
                    { sta.Text = $"failed to edit at 0x{addr:X}"; return false; }
                return true;
            }
            catch (Exception ex) { sta.Text = "error" + ex.Message; return false; }
        }
        static DYNX AiloFuck = new DYNX();
        private List<long> a = new List<long>();
        private Dictionary<long, string> m = new Dictionary<long, string>();

        const string speedsearch = "00 01 00 00 00 02 2B 07 3D";
        const string speedscan = speedsearch;
        const string speedreplace = "00 01 00 00 00 92 E4 50 3D";

        private async void guna2CustomCheckBox4_Click(object sender, EventArgs e)
        {
            try
            {
                var p = Process.GetProcessesByName("HD-Player");
                if (p.Length == 0 || !AiloFuck.OpenProcess(p[0].Id))
                {
                    sta.Text = "HD-PLAYER NOT FOUND"; return;
                }
                sta.Text = "Scaning....";
                var f = await AiloFuck.AoBScan(0x0000000000000000, 0x00007fffffffffff, speedscan, true, true, "");
                if (f == null || !f.Any())
                { sta.Text = "Scan not found"; return; }
                a = f.ToList();
                m = a.ToDictionary(x => x, x => speedsearch);
                sta.Text = "Scan found";
            }
            catch (Exception ex) { sta.Text = "error" + ex.Message; }
        }

        private void guna2CustomCheckBox5_Click(object sender, EventArgs e)
        {
            string pat = guna2CustomCheckBox5.Checked ? speedreplace : m.Values.First();
            if (Inject(pat))
            {
                sta.Text = guna2CustomCheckBox5.Checked ? "SPEED ON" : "SPEED OFF";
                Console.Beep(400, 500);
            }

        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private async void Main_Load(object sender, EventArgs e)
        {

            //var processes = Process.GetProcessesByName("HD-Player");

            //if (processes.Length != 1)
            //{
            //    MessageBox.Show("Open emulator.");
            //    return;
            //}

            //var process = processes[0];
            //string mainModulePath = Path.GetDirectoryName(process.MainModule?.FileName);

            //if (string.IsNullOrEmpty(mainModulePath))
            //{
            //    MessageBox.Show("Reinstall emulator.");
            //    return;
            //}

            //var adbPath = Path.Combine(mainModulePath, "HD-Adb.exe");
            //if (!File.Exists(adbPath))
            //{
            //    MessageBox.Show("Adb not Found. Reinstall emulator.");
            //    return;
            //}



            //var adb = new Adb(adbPath);
            //await adb.Kill();
            ////await Task.Delay(500);
            //if (!await adb.Start())
            //{
            //    MessageBox.Show("Adb Error");
            //    Environment.Exit(0);
            //    return;
            //}
            ////await Task.Delay(500);
            //var moduleAddr = await adb.FindModule("com.dts.freefireth", "libil2cpp.so");

            //if (moduleAddr == 0)
            //{
            //    MessageBox.Show("Go to Lobby then Logout then try to apply.");
            //    Environment.Exit(0);
            //    return;
            //}

            //Offsets.Il2Cpp = moduleAddr;
            //Core.Handle = FindRenderWindow(mainHandle);

            //new Thread(Data.Work) { IsBackground = true }.Start();
            //new Thread(AimbotV2.Work) { IsBackground = true }.Start();
            //new Thread(UpPlayer.Work) { IsBackground = true }.Start();
            //new Thread(TeleKil.Work) { IsBackground = true }.Start();
        }

        private void guna2TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {


        }

        private void guna2ControlBox2_Click(object sender, EventArgs e)
        {

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label16_Click(object sender, EventArgs e)
        {

        }

        private void label18_Click(object sender, EventArgs e)
        {

        }

        private void label20_Click(object sender, EventArgs e)
        {

        }

        private void label19_Click(object sender, EventArgs e)
        {

        }

        private void label23_Click(object sender, EventArgs e)
        {

        }
    }
}
