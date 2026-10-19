using System.Media;
using System.Numerics;
using System.Reflection;

namespace AotForms
{
    internal static class Config
    {
        internal static bool AimBot = false;
        internal static bool SilnetAim = false;
        internal static bool UpPlayer = false;
        internal static bool ShakeKill = false;
        internal static Keys AimbotKey = Keys.LButton;
        internal static Keys AwmKey = Keys.F; // AWM macro hotkey - change kar sakte ho

        internal static int AimBotMaxDistance = 2000;
        internal static int expsize = 8;
        internal static int AimBotFov = 200;
        internal static int espran = 150;
        internal static bool FastReload = false;
        internal static bool ESPMatchTimer = false;
        internal static int MatchTimerDuration = 180;
        internal static bool ESPInfoV2 = false;
        internal static bool ESPLineRGB = false;
        public static bool PARTICLE_OFF = false;
        internal static int EspLineThickNess = 2;
        public static float GlowRadius = 16;
        public static float GlowRadiusdot = 16;
        public static float FeatherAmount = 3f;
        public static float GlowOpacity = 0.02f;
        internal static float dotRadius = 5f;
        internal static bool Tele = false;

        internal static bool ClimbDownEnabled = false;
        public static bool EnemyPullEnabled = false;
        public static float EnemyPullStrength = 0.65f;
        public static int EnemyPullMaxDistance = 250;
        public static int EnemyPullTickMs = 6;

        internal static bool AimFov = false;
        internal static bool IgnoreKnocked = false;
        internal static bool Speed = false;
        internal static bool NoRecoil = false;
        internal static bool AimmmAwm = false; // AWM Sniper Aim Toggle
        internal static bool MagicBullet = false;
        internal static bool NoCache = false;
        internal static bool aimIsVisible = true;
        internal static bool StreamMode = false;
        internal static bool esptotalplyer = false;
        internal static bool FixEsp = false;
        internal static bool speedint = false;

        internal static bool minimap = false;

        internal static bool ESPWeapon = false;

        internal static bool ESPweaponicon = false;

        internal static bool underkill = false;
        internal static bool speed123 = false;

        internal static bool ESPLine = false;
        internal static bool BoxGlow = false;
        internal static bool EspBottom = false;
        internal static bool EspUp = false;
        internal static Color ESPLineColor = Color.White;

        internal static bool ESPBox = false;
        internal static Color ESPBoxColor = Color.White;
        internal static bool MatchTimer = false;

        internal static bool EnemyTimerActive = false;
        internal static bool EnemyTimerEnded = false;

        internal static DateTime EnemyTimerStart = DateTime.Now;
        internal static int EnemyTimerDuration = 180;

        internal static bool ESPBox2 = false;
        internal static bool ESPDistance = false;
        internal static Color ESPFillBoxColor = Color.Red;
        internal static bool ESPName = false;
        internal static Color ESPNameColor = Color.White;
        static float gameTimer = 180f;

        static bool gameEnded = false;
        static DateTime lastFrameTime = DateTime.Now;
        internal static bool Unlimitedammo = false;
        internal static bool RapidFire = false;
        internal static bool ESPWeaponIcon = false;
        internal static bool ESPRefreshMode = false;
        internal static bool HackerTag = false;
        internal static bool ESPLineRainbow = false;

        internal static bool ESPHealth = false;
        internal static Color ESPHeath = Color.White;
        internal static Color WeaponTextColor = Color.White;

        internal static bool AimTrackLine = false;
        internal static Color AimTrackLineColor = Color.Cyan;
        internal static Color WeaponIconColor = Color.Cyan;

        internal static bool ESPSkeleton = false;
        internal static Color ESPSkeletonColor = Color.White;
        internal static bool internalfastfire = false;

        internal static bool ESPFillBox = false;
        internal static bool FlyHack = false;

        internal static bool AimbotVisible = false;

        internal static bool ESPCorner = false;
        internal static bool ESPCornerColor = false;
        internal static bool FastFire = false;
        internal static bool forceaim = false;
        internal static bool flyhackEnabled = false;

        internal static bool ESPInfo = false;
        internal static bool SilentAim = false;
        internal static bool NoBulletTracking = false;
        internal static bool MultiSilent = false;
        internal static bool SilentCoverFear = false;
        internal static bool EspTimerEnabled = false;
        public static DateTime MatchFinishedTime;
        public static bool MatchFinishedNotified;
        public static bool MatchTimerReset = false;

        public static float AimMaxDistance = 200f;
        internal static bool down = false;
        internal static float downSpeed = 0.8f;
        public static float AimFOV = 150f;
        internal static bool spinbot = false;
        internal static float SpinSpeed = 4.2f; // Spinbot speed slider (degrees per tick)

        internal static bool enemypullv2 = false;

        internal static bool ESPFove = false;
        internal static bool espbg = false;
        internal static bool Aimfovc = false;
        internal static Color Aimfovcolor = Color.White;
        internal static bool RGB = false;
        internal static bool ESPRainbow = false;
        internal static Vector4 ESPCOLOR = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);

        internal static bool espcfx = false;
        internal static bool sound = false;
        internal static string AimBotMode = "Hex";
        internal static bool flyhacks;

        internal static float AimSmoothness = 0f;
        internal static int thread = 0;
        internal static int AimFovCircle = 500;
        internal static int AimbotSmoothness = 0;
        internal static int test;

        internal static bool ESPInformation = false;
        internal static bool espweapon = false;
        internal static float RapidFireSpeed = 0.5f;
        internal static bool UnlimitedAmmo = false;
        internal static bool AutoTeleport = false;
        internal static bool SpeedJoystick = false;
        internal static bool FearSilentKill = false;

        public static bool HackerDetect = false;
        public static uint selectedHackerAddress = 0;


        internal static bool FlyHackEnabled = false;
        internal static float FlyHeight = 6f;

        internal static bool teleportmap = false;
        public static bool AimFovEnabled = false;
        public static bool ESPRefresh;
        public static float EnemyPullStrengthNormal = 0.45f;
        public static int EnemyPullMaxDistanceNormal = 150;
        public static int EnemyPullTickMsNormal = 6;
        internal static bool DownPlayer = false;
        internal static float CameraD = 0f;
        internal static bool Down1 = false;
        internal static bool Up1 = false;
        public static float SilentFOV = 120f;
        public static float BulletSpeed = 820f;
        public static float TriggerFOV = 8f;
        internal static bool teliport = false;
        public static bool ClimbUpEnabled = false;

        public static void Notif()
        {

            if (!sound)
            {
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

    public enum TargetingMode
    {
        ClosestToCrosshair,
        Target360,
        ClosestToPlayer,
        LowestHealth,
    }
}
