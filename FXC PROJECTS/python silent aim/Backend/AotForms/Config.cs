using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Vortice.DXGI;

namespace AotForms
{
    internal static class Config
    {

        internal static bool AimbotVisible = false;
        internal static bool AimBotRage = false;
        internal static bool AimBotRageMainId = false;  // NEW: bones-based rage aimbot toggle
        internal static int Smoothness = 1;              // NEW: sleep ms between writes (1 = fast rage)
        internal static bool SilentAim = false;
        internal static bool Aimbotai = false;
        internal static bool AimSilent = false;
        internal static bool TELE = false;
        internal static bool TELEFIRE = false;
        internal static bool SpeedHack = false;
        internal static float visionVal = 10f;
        internal static float cameraVal = 10f;
        internal static bool Speed = false;
        internal static float AimBotSmooth = 16f;
        internal static bool AimBotRageMax = false;
        internal static bool ClimbUpEnabled = false;
        internal static bool TeleportV2 = false;
        internal static float CameraD = -3.0f;
        internal static bool ClimbDownEnabled = false;
        internal static bool shakekill = false;
    
        internal static bool DownPlayer = false;
        internal static bool flyhack = false;
        internal static bool FLYHACKX400 = false;
        internal static bool FlyHack80x = false;
        internal static bool FlyHack50x = false;
        internal static bool ClimbUpV2Enabled = false;
        internal static bool TeleportAnywhere = false;
        internal static bool teleportmap = false;
        internal static bool directteleport = false;
        //pull360
        public static bool EnemyPullEnabled = false;
        public static float EnemyPullStrength = 0.65f;
        public static int EnemyPullMaxDistance = 250;
        public static int EnemyPullTickMs = 6;
        internal static bool sperm = false;
        internal static bool FastReload = false;
        
        // Added for Data.cs compatibility
        internal static bool UnlimitedAmmo = false;
        internal static bool speedint = false;
        internal static bool RapidFire = false;
        internal static float RapidFireSpeed = 0.5f;
        internal static bool espweapon = false;

        // RapidSpin / Spinbot
        internal static bool spinbot = false;
        internal static float SpinSpeed = 8f;  // degrees per tick (5ms loop)

        public static bool EspTimerActive = false;
        public static DateTime EspTimerStart;
        public static DateTime LastEspSeen;
        public static int EspTimerDuration = 180;
        public static int ResetAfterNoEsp = 3;

        public static bool MarkEspSeen()
        {
            LastEspSeen = DateTime.Now;
            if (!EspTimerActive)
            {
                EspTimerActive = true;
                EspTimerStart = DateTime.Now;
            }
            return EspTimerActive;
        }


        public static void UpdateEspTimer()
        {
            if (!EspTimerActive) return;

            if ((DateTime.Now - LastEspSeen).TotalSeconds > ResetAfterNoEsp)
            {
                EspTimerActive = false;
                return;
            }

            var elapsed = (int)(DateTime.Now - EspTimerStart).TotalSeconds;
            if (elapsed >= EspTimerDuration)
            {
                EspTimerActive = false;
            }
        }

        public static string AimTargetPart { get; set; } = "HEAD";
        internal static bool Aimbot = false;
        internal static bool ESPWeaponIcon = false;

        internal static Keys AimbotKey = Keys.LButton; 

        internal static int AimBotMaxDistance = 200;

        internal static bool IgnoreKnocked = false;
        internal static bool UpdateEntities = false;
        internal static bool NoRecoil = false;
        internal static bool NoCache = false;

        internal static float UpPlayerHeight = 10f;

        internal static bool GlowingLines = false;
        internal static bool ESPLine = false;
        internal static bool ESPDistance = false;
        internal static Color ESPInformationColor = Color.AliceBlue;
        internal static bool minimap = false;
        internal static bool ESPBox3D = false;
        internal static bool ModernESP = false;
        internal static bool AimTrackLine = false;
        internal static Color ESPLineColor = Color.White;
        internal static Color MocoColor = Color.White;
        internal static float AimFov = 200f;
        internal static bool StreamMode = false;
        internal static bool enmyteleport = false;
        internal static bool ESPBox = false;
        internal static bool ESPMoco = false;
        internal static bool enableAimBot = false;
        internal static Color ESPBoxColor = Color.White;
        internal static AimBotType AimBotType;
        internal static bool ESPName = false;

        internal static Color ESPNameColor = Color.White;
        internal static Color AimTrackLineColor = Color.Red;
        
        internal static bool ESPHealth = false;
        internal static bool ESPHealthText = false;
        internal static Color ESPHealthColor = Color.Green;
        internal static bool ESPSkeleton = false;  
        internal static Color ESPSkeletonColor = Color.White;
        internal static Color ESPBox3DColor = Color.White;
        internal static bool FOVEnabled = false;
        internal static bool UpPlayer = false;
        internal static Color FOVColor = Color.White;
        internal static LinePosition ESPLinePosition = LinePosition.Top;
        internal static float fly = 15;

        public static bool AimKill { get; set; }
        public static bool UltraSwitch { get; set; }
        public static bool AutoSwitch { get; set; }
        public static bool Tele = false;
        public static bool teli = false;

        public static bool Trigger = true;
        public static bool FakeKill = true;
        public static bool EnableAim = true;

        public static float test = 0.5f;
        public static float test1 = 60f;
        public static float BehindOffsetZ = 1.5f;
        public static float BehindOffsetX = 0.1f;
        public static float MaxEngageDistance = 100f;
        public static int TeleDelay = 10; // in ms
        public static float NoRecoilPercentage = 10;
    }
    public enum AimBotType
    {
        Silent,
        Rage
    }
    public enum LinePosition
    {
        Top,
        Center,
        Bottom
    }
   
}
