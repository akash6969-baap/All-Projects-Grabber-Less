using System;

namespace AotForms
{
    internal static class Offsets
    {
        internal static uint Il2Cpp;
        internal static uint InitBase = 0x9EC1C48;
        internal static uint StaticClass = 0x5C;

        // Armas
        internal static uint Weapon = 0x3f8;
        internal static uint WeaponData = 0x58;
        internal static uint IntWeaponType = 0x64;
        internal static uint WeaponRecoil = 0xc;
        internal static uint NoReload = 0x99;

        // Camera
        internal static uint FollowCamera = 0x454;
        internal static uint AimRotation = 0x404;

        // ESP
        internal static uint CurrentObserver = 0xb4;
        internal static uint ObserverPlayer = 0x28;

        // General
        internal static uint CurrentMatch = 0x50;
        internal static uint MatchStatus = 0x8c;
        internal static uint LocalPlayer = 0x94;
        internal static uint Camera = 0x18;
        internal static uint Player_Data = 0x44;
        internal static uint Avatar = 0xA0;
        internal static uint AvatarManager = 0x4C4;
        internal static uint Player_ShadowBase = 0x16BC;
        internal static uint UnlimitedAmmo = 0xE0;
        internal static uint Avatar_Data = 0x14;
        internal static uint Avatar_Data_IsTeam = 0x59;
        internal static uint Avatar_IsVisible = 0x95;
        internal static uint IsFiring = 0x544;

        // Player
        internal static uint Player_IsDead = 0x50;
        internal static uint Player_Name = 0x2e4;
        internal static uint XPose = 0x78;
        internal static uint AimbotVisible = 0x4a8;
        internal static uint PlayerAttributes = 0x4C0;
        internal static uint MainCameraTransform = 0x254;
        internal static uint DictionaryEntities = 0x68;
        internal static uint ViewMatrix = 0xE8; // Last OB
        internal static uint pomba = 0x544;

        // Silent Aim
        internal static uint sAim1 = 0x544;
        internal static uint sAim2 = 0x948;
        internal static uint sAim3 = 0x38;
        internal static uint sAim4 = 0x2c;

        // Speed Internal
        internal static uint GameTimer = 0x10;
        internal static uint FixedDeltaTime = 0x24;
    }
}
