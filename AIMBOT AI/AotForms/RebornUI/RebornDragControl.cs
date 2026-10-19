using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Reborn_UI.Components
{
    public class RebornDragControl : Component
    {
        private Control _handleControl;

        public Control SelectControl
        {
            get { return _handleControl; }
            set
            {
                _handleControl = value;
                _handleControl.MouseDown += new MouseEventHandler(DragControl_MouseDown);
            }
        }

        private void DragControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(_handleControl.FindForm().Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
    }
}
