using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


namespace FearAuth_Test
{
    public partial class Form2 : Form
    {
        string username;
        long expiry;

        public Form2(string user, long expiryTimestamp)
        {
            InitializeComponent();

            username = user;
            expiry = expiryTimestamp;
        }
        public static string GetHWID()
        {
            string raw = Environment.MachineName + Environment.UserName;

            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }


        private void Form2_Load(object sender, EventArgs e)
        {
           
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private async void btnSendFeedback_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFeedback.Text))
            {
                Status.Text = "Please enter feedback!";
                Status.ForeColor = Color.Red;
                return;
            }

            btnSendFeedback.Enabled = false;
            Status.Text = "Sending feedback...";

            try
            {
                string webhookUrl = "Use Your Webhook Url";

                string hwid = GetHWID();
               

                using (HttpClient client = new HttpClient())
                {
                    var json = $@"{{
                ""content"": ""📩 **New Feedback Received**\n👤 User: {username}\n💻 HWID: {hwid}\n📝 Message: {txtFeedback.Text}""
            }}";

                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(webhookUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        Status.Text = "Feedback sent!";
                        Status.ForeColor = Color.Green;
                        txtFeedback.Clear();
                    }
                    else
                    {
                        Status.Text = "Failed to send!";
                        Status.ForeColor = Color.Red;
                    }
                }
            }
            catch (Exception ex)
            {
                Status.Text = "Error: " + ex.Message;
                Status.ForeColor = Color.Red;
            }

            btnSendFeedback.Enabled = true;
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
    }


}
