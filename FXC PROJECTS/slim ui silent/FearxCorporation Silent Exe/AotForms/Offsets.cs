using System;

namespace AotForms
{
    internal static class Offsets
    {
        internal static uint Il2Cpp;
        internal static uint InitBase = 0xA986B7C;
        internal static uint StaticClass = 0x5C;
        internal static uint ViewMatrix = 0xE8;

        // Match Related
        internal static uint CurrentMatch = 0x50;
        internal static uint MatchStatus = 0x8c;
        internal static uint LocalPlayer = 0x94;
        internal static uint DictionaryEntities = 0x68;

        // Player
        internal static uint Player_IsDead = 0x50;
        internal static uint Player_Name = 0x2dc;
        internal static uint Player_Data = 0x48;
        internal static uint Player_ShadowBase = 0x18b8;
        internal static uint XPose = 0x78;
        internal static uint AvatarManager = 0x4c0;
        internal static uint Avatar = 0xa8;
        internal static uint Avatar_IsVisible = 0x95;
        internal static uint Avatar_Data = 0x14;
        internal static uint Avatar_Data_IsTeam = 0x59;
        internal static uint PlayerID = 0x268;
        internal static uint BaseProfileInfo = 0x18cc;
        internal static uint IsClientBot = 0x284;
        internal static uint PlayerAttributes = 0x4bc;
        internal static uint UnlimitedAmmo = 0xE0;

        // Camera
        internal static uint FollowCamera = 0x450;
        internal static uint Camera = 0x18;
        internal static uint MainCameraTransform = 0x24c;
        internal static uint AimRotation = 0x400;

        // Observer
        internal static uint CurrentObserver = 0xb4;
        internal static uint ObserverPlayer = 0x28;

        // Weapon
        internal static uint Weapon = 0x3f4;
        internal static uint WeaponData = 0x58;
        internal static uint WeaponRecoil = 0xc;
        internal static uint UnkPlayerWeaponInfoClass = 0x4a8;
        internal static uint IsCombineWeapon = 0xd8;
        internal static uint WeaponOnHand = 0x54;
        internal static uint CombineWeaponOnHand = 0x58;
        internal static uint WeaponInfo = 0x64;
        internal static uint WeaponID = 0x14;
        internal static uint NoReload = 0x99;

        // Silent Aim
        internal static uint LastAimingInfoFromWeapon = 0x978;
        internal static uint isFiring = 0x540;
        internal static uint StartPosition = 0x38;
        internal static uint RayDir = 0x2c;

        // Aimbot Visible
        internal static uint LockedAimingCollider = 0x54;
        internal static uint Collider = 0x4a4;

        // Misc
        internal static uint RunSpeedUpScale = 0x1d8;
        internal static uint FastFire = 0x18C;
        internal static uint InSnowSlideWayDashing = 0x1480;
        internal static uint GameTimer = 0x10;
        internal static uint FixedDeltaTime = 0x24;
    }
}
