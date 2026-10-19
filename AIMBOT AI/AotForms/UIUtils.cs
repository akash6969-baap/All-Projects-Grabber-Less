using System;
using System.Drawing;
using System.Windows.Forms;

namespace AotForms
{
    internal static class UIUtils
    {
        // 48; 48; 47
        internal static void ElipseControl(Control control, int elipse)
        {
            var region = WinAPI.CreateRoundRectRgn(0, 0, control.Width, control.Height, elipse, elipse);
            control.Region = Region.FromHrgn(region);
        }

        // Updated MovableForm method to disable resizing
        internal static void MovableForm(Form form)
        {
            // Disable resizing by setting the border style to FixedDialog (you can also use FixedSingle, Fixed3D, etc.)
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MaximizeBox = false;  // Disable the maximize button
            form.MinimizeBox = false;  // Disable the minimize button (optional)

            MovableControl(form, form);
        }

        internal static void MovableControl(Control control, Control target)
        {
            control.MouseDown += delegate (object sender, MouseEventArgs e) {
                if (e.Button == MouseButtons.Left)
                {
                    WinAPI.ReleaseCapture();
                    WinAPI.SendMessage(target.Handle, WinAPI.WM_NCLBUTTONDOWN, WinAPI.HT_CAPTION, 0);
                }
            };
        }
    }
}
