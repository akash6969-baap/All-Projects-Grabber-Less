using System.Media;
using System.Reflection;

namespace AotForms
{
    internal static class Config
    {
        internal static bool AimBot = false;
        internal static Keys AimbotKey = Keys.LButton;
        internal static bool AimSilent = false;
        internal static bool SilentAim360 = false;
        internal static bool Aimkill = false;
        internal static bool hamba = false;
        internal static bool silentnormal = false;
        internal static bool SilentAimvisible = false;
        internal static bool pullbabu = false;
        internal static bool AimbotVisible = false;
        internal static bool FearCollider = false;
        internal static bool ESPclosest = false;
        internal static bool aimkill = false;
        internal static bool SpeedHackEnabled = false;
        internal static Keys AimSilentKey = Keys.LButton;
        internal static bool flyme = false;
        internal static bool UpPlayer = false;
        internal static Color NameCheat = Color.White;
        internal static bool FastWeaponSwitch = false;
        internal static bool AutoFireEnabled = false;
        public static float YOffset = 0.030f;



        internal static bool AimKillEnabled = true;
        internal static bool TeamCheck = true;
        internal static bool VisibilityCheck = true;
        internal static float AimKillFOV = 30f; // pixels from crosshair
        public static bool speedHackEnabled = false;
        public static float speedMultiplier = 1.0f;



        internal static int AimBotMaxDistance = 2000;
        internal static int expsize = 8;
        internal static int AimBotFov = 400;
        internal static int Aimfov = 400;
        internal static int espran = 150;
        internal static float AimBotSmooth = 0.0f;
        internal static float AutoFireMaxDistance = 360f;

        // Flag to toggle the particle effect on/off
        public static bool PARTICLE_OFF = false;
        internal static int EspLineThickNess = 1;
        public static float GlowRadius = 15;
        public static float FeatherAmount = 2f;
        public static float GlowOpacity = 0.02f;

        internal static bool AimFov = false;
        internal static bool IgnoreKnocked = false;
        internal static bool Speed = false;
        internal static bool proxtelekill = false;
        internal static bool NoRecoil = false;
        internal static bool MagicBullet = false;
        internal static bool NoCache = false;
        internal static bool aimIsVisible = true;
        internal static bool StreamMode = false;
        internal static bool esptotalplyer = false;
        internal static bool FixEsp = false;
        internal static bool espweapon = false;
        internal static bool ESPInformation = false;

        internal static bool minimap = false;

        internal static Color ESPFullBoxColor = Color.White;
        internal static Color ESPFillBoxGlowColor = Color.White;
        internal static Color ESPCorneredBoxColor = Color.White;
        internal static bool ESPCorneredBoxFlowRGB = false;
        internal static bool ESPCorneredBoxFlow = false;
        internal static bool ESPLineGlow = false;
        internal static bool ESPCorneredBox = false;
        internal static bool ESPFillBoxGlow = false;
        internal static bool ESPFillBoxGlowRGB = false;
        internal static bool ESPFullBoxGlowRGB = false;
        internal static bool ESPLineGlowRGB = false;
        internal static bool ESPCorneredBoxRGB = false;
        internal static bool ESPFilllBoxGlowRGB = false; 
        internal static bool ESPFullBoxGlow = false;

        internal static bool ESPLineRGB = false;
        internal static bool ESPBoxRGB = false;
        internal static bool ESPFillBoxRGB = false;
        internal static bool ESPNameRGB = false;
        internal static bool AimfovcRGB = false;

        internal static bool ESPWeapon = false;
        internal static bool ESPWeaponIcon = false;

        //CROSS HAIR
        public static bool CrosshairEnabledRGB = false;
        public static bool CrosshairEnabled = false;
        public static Color CrosshairColor = Color.White;
        public static float CrosshairSize = 15f; // Size in pixels
        public static float CrosshairThickness = 2f; // Line thickness
        public static bool DrawShurikenCrosshair = false;
        internal static int DrawShurikenCrosshairSpeed = 1000;
        public static float CrosshairRotationSpeed { get; set; } = 2f; // Default speed
        public static bool AIMVISIBLE { get; internal set; }
        public static object AimTargetPart { get; internal set; }
        public static int Aimbotype { get; internal set; }
        public static bool RGB { get; internal set; }

        internal static bool ESPLine = false;
        internal static bool BoxGlow = false;
        internal static bool EspBottom = false;
        internal static bool EspUp = false;
        internal static Color ESPLineColor = Color.White;
        internal static Color ESPWeaponTextColor = Color.White;
        internal static bool ESPBox = false;
        internal static Color ESPBoxColor = Color.White;

        internal static bool ESPBox2 = false;
        internal static bool ESPDistance = false;
        internal static Color ESPFillBoxColor = Color.Red;
        internal static bool ESPName = false;
        internal static Color ESPNameColor = Color.White;

        internal static bool ESPHealth = false;
        internal static Color ESPHeath = Color.White;

        internal static bool ESPSkeleton = false;
        internal static Color ESPSkeletonColor = Color.White;

        internal static bool ESPFillBox = false;


        internal static bool ESPCorner = false;
        internal static bool ESPCornerColor = false;

        internal static bool ESPInfo = false;

        internal static bool ESPFove = false;
        internal static bool espbg = false;
        internal static bool Aimfovc = false;
        internal static Color Aimfovcolor = Color.White;

        internal static bool espcfx = false;
        internal static bool sound = false;
        // New configuration option for aimbot mode
        internal static string AimBotMode = "Hex"; // Default to "normal" mode
        internal static float AimSmoothness = 0f;
        internal static int thread = 0;
        internal static int AimFovCircle = 500;
        internal static int AimbotSmoothness = 0;
        internal static int test;

        

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
}
