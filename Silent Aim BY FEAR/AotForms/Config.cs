using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using static Guna.UI2.Native.WinApi;

namespace AotForms
{
    internal static class Config
    {
        internal static bool ESPLineRGB = false;
        internal static bool ESPLineRGBGradient = false;
        internal static float AimBotSmooth = 16f;
        internal static AimBotType AimBotType;
        internal static bool enableAimBot = false;
        internal static bool UpPlayer = false;
        internal static bool Aimkill = false;
        internal static bool teli = false;
        internal static bool AimBotRage = false;
        internal static bool SilentAim = false;
        internal static Color MiniMapColor = Color.White;
        internal static Keys AimbotKey = Keys.LButton;
        internal static bool EspUp = true;
        internal static bool EspBottom = false;
        internal static bool EspMiddle = false;
        internal static int AimBotMaxDistance = 200;
        internal static float test = 10;
        public static float GlowRadius = 15;
        internal static bool IgnoreKnocked = false;
        internal static bool UpdateEntities = false;
        internal static bool NoRecoil = false;
        internal static bool NoCache = false;
        internal static bool ESPShuriken = false;
        internal static bool ESPShurikenRotate = false;
        internal static bool EnemyCount = false;
        internal static bool ESPInfoBox = false;

        internal static TargetingMode TargetingMode  = TargetingMode.ClosestToCrosshair;
        internal static TargetingMode TargetingMode1 = TargetingMode.Target360;
        internal static TargetingMode TargetingMode2 = TargetingMode.ClosestToPlayer;
        internal static TargetingMode TargetingMode3 = TargetingMode.LowestHealth;
        internal static bool ESPDistance = false;
        internal static bool ESPLine = false;
        internal static Color ESPLineColor = Color.White;
        internal static Color ESPTargetAimColor = Color.White;
        internal static float AimFov = 100f;
        internal static bool StreamMode = false;
        internal static Color ESPDistanceColor = Color.White;
        internal static Color NameCheat = Color.Cyan;
        internal static bool ESPBox = false;
        internal static bool EspWeaponIcon = false;
        internal static Color ESPBoxColor = Color.White;
        internal static Color EnemyCountColor = Color.Yellow;
        internal static bool ESPName = false;
        internal static Color ESPNameColor = Color.White;
        internal static bool ESPHealth = false;
        internal static bool ESPHealthText = false;
        internal static Color ESPHealthColor = Color.Lime;
        internal static bool ESPSkeleton = false;  
        internal static Color ESPSkeletonColor = Color.White;
        internal static bool FOVEnabled = false;
        internal static bool Minimap = false;
        internal static bool CrosshairEnabled = false;
        internal static Color CrosshairColor = Color.White;
        internal static float CrosshairSize = 15f;
        public static float CrosshairRotationSpeed = 2f; // Default speed
        internal static Color FOVColor = Color.White;
        internal static LinePosition ESPLinePosition = LinePosition.Top;
        internal static Color ESPCornerBoxColor = Color.White;
        internal static float cameraVal = 1.0f;
        internal static float speedVal = 1.0f;
        internal static float visionVal = 3.141592741f;
        internal static bool ESPBox2 = false;
        internal static bool ESPFillBox = false;
        internal static Color ESPFillBoxColor = Color.White;
        internal static float test1 = 0.01f;
        internal static int espran = 150;
        public static float TeleportRange = 10f;
        internal static bool Aimfovc = false;
        internal static Color Aimfovcolor = Color.White;

        internal static bool linetargetclose = false;



        public static float ESPBoxThickness = 1.0f;





    }
    public enum TargetingMode
    {
        ClosestToCrosshair,
        Target360,
        ClosestToPlayer,
        LowestHealth,
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
