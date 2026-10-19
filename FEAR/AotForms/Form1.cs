using Reborn;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Client
{

    public partial class Form1 : Form
    {
        IntPtr mainHandle;
        public static api KeyAuthApp = new api(
     name: "Krishnakumad444's Application",
    ownerid: "bzp59x6jMH",
    secret: "f83546b74fc90018cc32b184168c000f075c1f6e3698449f504c36f2dfcae0b5",
    version: "1.0");

        public Form1(IntPtr handle)
        {
            mainHandle = handle;
            InitializeComponent();

        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button5_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button6_Click(object sender, EventArgs e)
        {


        }

        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2TextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2TextBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2TextBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2TextBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            KeyAuthApp.init();
            //400, 124
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button7_Click(object sender, EventArgs e)
        {

        }

        private void guna2ControlBox1_Click(object sender, EventArgs e)
        {
            Application.Exit();
            Environment.Exit(0);
        }

        private void guna2Button4_Click_1(object sender, EventArgs e)
        {
            KeyAuthApp.login(usr.Text, pass.Text);
            if (KeyAuthApp.response.success)
            {
                hack abc = new hack(mainHandle);
                abc.Show();
                this.Hide();

            }
            else
            {
                MessageBox.Show("Invalid Information. Contact kiseko be");
                guna2Panel3.Location = new Point(9999, 9999);
                guna2Panel2.Location = new Point(10, 46);
            }
        }

        private void guna2Button2_Click_1(object sender, EventArgs e)
        {
            guna2Panel2.Location = new Point(9999, 9999);
            guna2Panel3.Location = new Point(10, 46);
        }

        private void guna2Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
