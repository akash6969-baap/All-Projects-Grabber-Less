using System.Diagnostics;
using System.Drawing;
using System.Media;
using System.Reflection;
using System.Windows.Forms;

namespace AotForms
{
    internal static class Config
    {
        internal static bool AimKill = false;
        internal static bool AimBot = false;
        internal static bool AimBotLeft = false;
        internal static float aimlegit = 0.05f;
        internal static int Aimbotype = 0;
        internal static int smooth = 0;
        public static int Smoothness = 1;
        internal static int LagFix = 0;

        internal static Keys AimbotKey = Keys.LButton;
        internal static Keys AimbotKey2 = Keys.None;

        internal static int AimBotMaxDistance = 100;
        internal static int expsize = 8;
        internal static int Aimfov = 50;
        internal static int espran = 100;
        internal static int aimdelay = 0;

        internal static bool GlowESPs = true;
        internal static bool RGB = false;

        internal static bool IgnoreKnocked = false;
        internal static bool Speed = false;
        internal static bool minimap = false;
        internal static bool NoRecoil = false;
        internal static bool MagicBullet = false;
        internal static bool NoCache = false;
        internal static bool aimIsVisible = true;
        internal static bool StreamMode = false;
        internal static bool esptotalplyer = false;
        internal static bool FixEsp = false;

        public static string AimTargetPart { get; set; } = "HEAD"; // Valor padrão


        internal static Color CrossHairColor = Color.White;
        internal static bool CrossHair = false;

        internal static bool ESPLine = false;
        internal static Color ESPLineColor = Color.White;

        internal static bool ESPDistance = false;
        internal static Color ESPDistanceColor = Color.White;

        internal static bool ESPBox = false;
        internal static Color ESPBoxColor = Color.White;

        internal static bool ESPBox2 = false;
        internal static Color ESPFillBoxColor = Color.White;
        internal static bool ESPName = false;
        internal static Color ESPNameColor = Color.White;

        internal static bool ESPHealth = false;
        internal static bool ESPHealthText = false;
        internal static Color ESPHealthColor = Color.Lime;

        internal static bool ESPSkeleton = false;
        internal static Color ESPSkeletonColor = Color.White;

        internal static bool ESPFillBox = false;

        internal static bool ESPCorner = false;
        internal static bool ESPCornerColor = false;

        internal static bool ESPInfo = false;
        internal static bool FastReload = false;
        internal static bool UnlimitedAmmo = false;
        internal static bool ESPInformation = false;
        internal static bool speedint = false;
        internal static bool ESPFove = false;
        internal static bool espbg = false;
        internal static bool Aimfovc = false;
        internal static Color Aimfovcolor = Color.White;

        internal static bool espcfx = false;
        internal static bool sound = false;

        // New field for line position control
        internal static string linePosition = "Up";  // Default to "Up"

        public static LinePosition ESPLinePosition = LinePosition.Top;










        // Método para converter a string selecionada em uma enumeração de teclas
        public static Keys GetKeysEnum(string keyName)
        {
            return keyName switch
            {
                "Mouse 1" => Keys.LButton,
                "Mouse 2" => Keys.RButton,
                "F1" => Keys.F1,
                "F2" => Keys.F2,
                "F3" => Keys.F3,
                "F4" => Keys.F4,
                "F5" => Keys.F5,
                "F6" => Keys.F6,
                "F7" => Keys.F7,
                "F8" => Keys.F8,
                "F9" => Keys.F9,
                "F10" => Keys.F10,
                "F11" => Keys.F11,
                "F12" => Keys.F12,
                "Ctrl" => Keys.Control,
                "Alt" => Keys.Menu,
                "Shift" => Keys.Shift,
                _ => Keys.LButton // Valor padrão se a tecla não for reconhecida
            };
        }

        public static void Notif()
        {


            if (!sound)
            {
                // Replace "YourNamespace.YourMP3File.mp3" with the correct namespace and file name
                Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Client.clicksound.wav");

                if (stream != null)
                {
                    using (SoundPlayer player = new SoundPlayer(stream))
                    {
                        player.Play();
                    }
                }
                else
                {

                }
            }

            else
            {


            }
        }
    }

    public enum LinePosition
    {
        Top,
        Center,
        Bottom
    }

}
