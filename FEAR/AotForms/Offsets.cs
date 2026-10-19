namespace AotForms
{
    internal static class Offsets
    {
        internal static uint Il2Cpp;
        internal static uint InitBase = 0x983FF10;
        internal static uint StaticClass = 0x5C;

        // General
        internal static uint CurrentMatch = 0x50;
        internal static uint MatchStatus = 0x3c;
        internal static uint LocalPlayer = 0x44;
        internal static uint DictionaryEntities = 0x68;

        // Player
        internal static uint Player_IsDead = 0xcc;
        internal static uint Player_Name = 0x24c;
        internal static uint Player_Data = 0x4c;
        internal static uint Player_ShadowBase = 0x149c;
        internal static uint XPose = 0x78;
        internal static uint AvatarManager = 0x420;
        internal static uint Avatar = 0x94;
        internal static uint Avatar_IsVisible = 0x7c;
        internal static uint Avatar_Data = 0x10;
        internal static uint Avatar_Data_IsTeam = 0x51;

        // Camera
        internal static uint FollowCamera = 0x3b0;
        internal static uint Camera = 0x14;
        internal static uint AimRotation = 0x368;
        internal static uint MainCameraTransform = 0x1bc;

        // Armas
        internal static uint Weapon = 0x35c;
        internal static uint WeaponData = 0x54;
        internal static uint WeaponRecoil = 0xc;
        internal static uint ViewMatrix = 0x98 + 0x24;

        // Silent Aim
        internal static uint sAim1 = 0x4a0;
        internal static uint sAim2 = 0x874;
        internal static uint sAim3 = 0x38;
        internal static uint sAim4 = 0x2c;

        // ESP IN OBSERVE
        internal static uint CurrentObserver = 0x64;
        internal static uint ObserverPlayer = 0x28;

        // No Reload
        internal static uint NoReload = 0x89;
        internal static uint LocalPlayerAttributes = 0x41c;



        // Speed Internal
        internal static uint GameTimer = 0x10;
        internal static uint FixedDeltaTime = 0x24;
        internal static ulong weaponinfo;
        internal static uint guntipposition;
        internal static uint bullet_hit;
        internal static ulong pomba;
        internal static ulong bisteca;
        internal static uint arma;
        internal static uint tiro;

        public static ulong IS_FIRING { get; internal set; }
    }
}