using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
namespace AotForms
{
    internal static class Config
    {
        internal static float CameraD = 0f;
        internal static bool DownPlayer = false;
        internal static bool Down1 = false;
        internal static bool Up1 = false;
        internal static float AimBotSmooth = 16f;
        internal static AimBotType AimBotType;
        internal static bool enableAimBot = false;
        internal static bool AimBotRage = false;
        internal static bool UpPlayer = false;
        internal static bool teli = false;
        internal static float test = 0.400f;
        internal static bool SilentAim = false;
        internal static Keys AimbotKey = Keys.LButton;
        public static float AimBotMaxDistance = 200f; // default 200 meter ya jo bhi unit
        internal static bool IgnoreKnocked = false;
        internal static bool UpdateEntities = false;
        internal static bool NoRecoil = false;
        internal static bool NoCache = false;
        internal static TargetingMode TargetingMode = TargetingMode.ClosestToCrosshair;
        internal static bool ESPLine = false;
        internal static Color ESPLineColor = Color.White;
        internal static float AimFov = 200f;
        internal static bool StreamMode = false;

        internal static Color NameCheat = Color.Cyan;
        internal static bool ESPBox = false;
        internal static Color ESPBoxColor = Color.White;
        internal static bool ESPName = false;
        internal static Color ESPNameColor = Color.White;
        internal static bool ESPHealth = false;
        internal static bool ESPHealthText = false;
        internal static Color ESPHealthColor = Color.Green;
        internal static bool ESPSkeleton = false;
        internal static Color ESPSkeletonColor = Color.White;
        internal static bool FOVEnabled = false;
       
        internal static Color FOVColor = Color.White;
        internal static LinePosition ESPLinePosition = LinePosition.Top;
        internal static float cameraVal = 1.0f;
        internal static float visionVal = 3.141592741f;
        internal static bool ESPCrosshair = false;
        internal static Color ESPCrosshairColor = Color.Red;
        internal static bool ESPCrosshairRGB = false;
        internal static float ESPCrosshairSize = 10f;
        internal static float ESPCrosshairThickness = 1.2f;
        //glowing line
        internal static bool GlowingLines = false;

        // Crosshair glow and rotation controls
        internal static bool CrosshairGlowing = false;
        internal static float CrosshairGlowRadius = 10f;
        internal static float CrosshairFeatherAmount = 1.5f;
        internal static float CrosshairRotationSpeed = 30f; // RPM (rotations per minute)

        // New properties from second codebase
        internal static bool ESPFillBox = false;
        internal static Color ESPFillBoxColor = Color.FromArgb(128, 255, 0, 0); // Semi-transparent red
        internal static bool ESP3DBox = false;
        internal static bool ESPMoco = false;
        internal static bool ESPBG = false;
        internal static float espran = 200f; // ESP Range
        internal static bool AimbotVisible = true;
        internal static float AimSmoothness = 0f;
        internal static int thread = 0;
        internal static int AimFovCircle = 500;
        internal static int AimbotSmoothness = 0;
        internal static int AimBotFov = 200;
        internal static bool SILENTMAX = false;

        // EnemyPull360 settings
        internal static bool EnemyPullEnabled = false;
        internal static int EnemyPullTickMs = 6;
        internal static float EnemyPullMaxDistance = 500f; // Increased max distance for brutal pull
        internal static float EnemyPullStrength = 1.0f; // Increased strength for brutal pull


        public static bool EnableForwardTeleport = false;


        // FlyHack settings
        internal static bool flyme = false;
        public static float flymeHeight = 10.0f;
        internal static Keys ToggleFlyHackKey = Keys.None;

        //SPEED HACK
        public static bool SpeedEnabled = false;
        public static float SpeedValue = 1.0f;
        //down player
       
        internal static float downSpeed = 0.8f;
        internal static bool Divekill = false;
        internal static bool down = false;
        internal static bool teliport = false;

        


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
        Rage,
        MAX,
        VISIBLE,
    }
    public enum LinePosition
    {
        Top,
        Center,
        Bottom
    }

}