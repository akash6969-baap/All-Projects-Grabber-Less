using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace AotForms
{
    internal static class Config
    {
        internal static bool speed = false;
        internal static bool rgb = false;
        internal static float iconsize = 1.0f;
        public static Vector4 ICONCOLOR = new Vector4(1f, 1f, 1f, 1f);
        internal static bool espweapon = false;
        internal static float AimBotSmooth = 0f;
        internal static AimBotType AimBotType;
        internal static bool enableAimBot = false;
        internal static bool AimbotVisible = false;
        internal static bool FearCollider = false;
        internal static bool RapidFire = false;
        internal static float RapidFireSpeed = 0.5f;
        internal static bool FastReload = false;

        internal static Keys AimbotKey = Keys.LButton;

        internal static float AimBotMaxDistance = 200f;

        internal static bool IgnoreKnocked = false;
        internal static bool UpdateEntities = false;
        internal static bool NoRecoil = false;
        internal static bool NoCache = false;
        internal static int AimbotSmoothness = 0;
        internal static int Smoothness = 1;
        internal static string AimTargetPart = "HEAD";
        internal static float AimFov = 200f;
        internal static int AimBotFov { get => (int)AimFov; set => AimFov = value; }
        internal static int AimFovRage { get => (int)AimFov; set => AimFov = value; }

        internal static TargetingMode TargetingMode  = TargetingMode.ClosestToCrosshair;
        internal static bool ESPLine = false;
        internal static Color ESPLineColor = Color.White;
        internal static bool StreamMode = false;
        internal static Color NameCheat = Color.Cyan;
        internal static bool ESPBox = false;
        internal static Color ESPBoxColor = Color.White;
        internal static bool wallhack = false;
        internal static bool brsutfiree = false;
        internal static bool FlyHack = false;
        internal static bool spinbot = false;
        internal static float SpinBotSpeed = 4.2f;
        internal static bool speedint = false;

        internal static bool ESPName = false;
        internal static Color ESPNameColor = Color.White;
        internal static bool ESPHealth = false;
        internal static bool ESPHealthText = false;
        internal static Color ESPHealthColor = Color.Green;
        internal static bool ESPSkeleton = false;
        internal static Color ESPSkeletonColor = Color.White;
        internal static bool ESPRGB = false;
        internal static bool AimTrackLine = false;
        internal static Color AimTrackLineColor = Color.Cyan;
        internal static bool IsLoggedIn = false;
        internal static string Username = "";
        internal static string Password = "";
        internal static bool WeaponOverlay = false;
        internal static bool HackerTag = false;
        internal static Color HackerTagColor = Color.Red;
        internal static bool EspTimer = false;
        internal static bool EnemyTimerActive = false;
        internal static System.DateTime EnemyTimerStart = System.DateTime.MinValue;
        internal static System.DateTime LastEnemySeen = System.DateTime.MinValue;
        internal static int EnemyTimerDuration = 60;
        internal static int ResetAfterNoEnemy = 10;
        internal static bool FOVEnabled = false;

        internal static Color FOVColor = Color.White;
        internal static LinePosition ESPLinePosition = LinePosition.Top;

        // ─── GhostLock ────────────────────────────────────────────────────────
        internal static bool GhostLock = false;  // Predictive Adaptive Aim Engine
        internal static bool GhostLockSniperMode = false;  // Force sniper/scope mode
        // ──────────────────────────────────────────────────────────────────────
        internal static bool CombatEnabled = false;
        internal static bool VisualsEnabled = false;

        internal static Keys KeyCombatAssist = Keys.None;
        internal static Keys KeyFOVEnabled = Keys.None;
        internal static Keys KeyNoRecoil = Keys.None;
        internal static Keys KeyRapidFire = Keys.None;
        internal static Keys KeyFastReload = Keys.None;
        internal static Keys KeyBurstFire = Keys.None;
        internal static Keys KeySpeedBoostTimer = Keys.None;
        internal static Keys KeySpinBot = Keys.None;

        internal static Keys KeyESPLine = Keys.None;
        internal static Keys KeyESPBox = Keys.None;
        internal static Keys KeyESPName = Keys.None;
        internal static Keys KeyESPHealth = Keys.None;
        internal static Keys KeyESPHealthText = Keys.None;
        internal static Keys KeyESPSkeleton = Keys.None;
        internal static Keys KeyAimTrackLine = Keys.None;
        internal static Keys KeyHackerTag = Keys.None;
        internal static Keys KeyESPRGB = Keys.None;
        internal static Keys KeyEspTimer = Keys.None;
        internal static Keys KeyWeaponOverlay = Keys.None;
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
