using FearAuth;
using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace FearAuth_Test
{
    public partial class Form1 : Form
    {


        public static api FearAuthApp = new api(
    name: "test",
    ownerid: "ab93eeea-9f57-41c6-ae0f-0b56be597a6f",
    secret: "22a20eb69bfe5041c61383dbd6209bbdd911699557e04837",
    version: "1.0"

 );
        private long expiryTimestamp;
        private string user;

        public Form1()
        {
            InitializeComponent();
            FearAuthApp.init();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private async void guna2Button1_Click(object sender, EventArgs e)
        {
            Status.Text = "Initializing...";
            await Task.Delay(200);

            FearAuthApp.init();

            Status.Text = "Checking credentials...";
            await Task.Delay(200);

            FearAuthApp.login(guna2TextBox2.Text, pass.Text);

            if (FearAuthApp.response.success)
            {
                Status.Text = "Login successful! Redirecting...";
                await Task.Delay(500);

                string user = guna2TextBox2.Text;
                Form2 main = new Form2(user, expiryTimestamp);
                main.Show();
                this.Hide();
            }
            else
            {
                Status.Text = "Login failed: " + FearAuthApp.response.message;
            }

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

        private void guna2TextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private async void guna2Button2_Click(object sender, EventArgs e)
        {
            
            guna2Button2.Enabled = false;

            
            if (string.IsNullOrWhiteSpace(key.Text) ||
                string.IsNullOrWhiteSpace(guna2TextBox2.Text) ||
                string.IsNullOrWhiteSpace(pass.Text))
            {
                Status.Text = "Please fill all fields!";
                Status.ForeColor = Color.Red;
                guna2Button2.Enabled = true;
                return;
            }

            try
            {
                Status.Text = "Initializing...";
                Status.ForeColor = Color.White;
                await Task.Delay(200);

                FearAuthApp.init();

                Status.Text = "Creating user...";
                await Task.Delay(200);

                FearAuthApp.register(
                    guna2TextBox2.Text.Trim(), 
                    pass.Text.Trim(),          
                    key.Text.Trim()            
                );

                if (FearAuthApp.response.success)
                {
                    Status.Text = "Registration successful!";
                    Status.ForeColor = Color.Green;

                    await Task.Delay(500);

                    
                    guna2TextBox2.Clear();
                    pass.Clear();
                    key.Clear();
                }
                else
                {
                    Status.Text = "Failed: " + FearAuthApp.response.message;
                    Status.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                Status.Text = "Error: " + ex.Message;
                Status.ForeColor = Color.Red;
            }

            
            guna2Button2.Enabled = true;
        }
    }
}