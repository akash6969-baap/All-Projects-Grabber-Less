using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FearAuth;
using System.Media;

namespace FREE_LIFETIME
{
    public partial class Form1 : Form
    {

        public static api FearAuthapp = new api(
            name: "EXTERNAL",
            ownerid: "798d0dfe-64b1-47ff-a94e-deea39c075ad",
            secret: "a5f8a9191f9663c064ee648ce5a2ff4982b66fdc51c8f34c",
            version: "1.0"
        //path: @"Your_Path_Here" // (OPTIONAL) if you use token validation, example: Token.txt or anything.txt and paste token in txt file
        );
        private long expiryTimestamp;
        private string user;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Initialize the LicenseAuth SDK
            FearAuthapp.init();

        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private async void guna2Button1_Click(object sender, EventArgs e)
        {
            FearAuthapp.init();
            FearAuthapp.login(guna2TextBox1.Text, guna2TextBox2.Text);

            if (FearAuthapp.response.success)
            {
                // ---- ACCESS GRANTED SOUND ----
               // SoundPlayer accessGranted = new SoundPlayer("ACCESS GRANTED.wav");
               // accessGranted.Play(); // background me play hoga

                // ---- OPEN MAIN FORM ----
                Main Main = new Main(user, expiryTimestamp);
                Main.Show();
                this.Hide();
            }
            else
            {
                
            }

        }

        private void guna2CirclePictureBox1_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void guna2CirclePictureBox2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            FearAuthapp.init();
            FearAuthapp.register(guna2TextBox1.Text, guna2TextBox2.Text, guna2TextBox3.Text);


            if (FearAuthapp.response.success)
            {
                MessageBox.Show("User created successfully...");





            }
            else
            {
                MessageBox.Show("Registration failed, try again.");




            }
        }

        private void guna2Panel1_Paint_1(object sender, PaintEventArgs e)
        {

        }
    }
}
