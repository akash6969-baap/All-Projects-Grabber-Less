
using System;

namespace AotForms
{
    internal static class Offsets
    {
        internal static ulong Il2Cpp;

        // ================= STATIC / BASE =================
        internal static uint InitBase           = 0xA342EFC;
        internal static uint StaticClass        = 0x5C;
        internal static uint ViewMatrix         = 0xE8;

        // ================= MATCH / GAME STATE ============
        internal static uint CurrentMatch       = 0x50;
        internal static uint MatchStatus        = 0x8C;
        internal static uint GameTimer          = 0x10;
        internal static uint FixedDeltaTime     = 0x24;

        // ================= PLAYER ========================
        internal static uint LocalPlayer        = 0x94;
        internal static uint DictionaryEntities = 0x68;
        internal static uint Player_IsDead      = 0x50;
        internal static uint Player_Name        = 0x320;
        internal static uint Player_Data        = 0x48;
        internal static uint Player_ShadowBase  = 0x1A60;
        internal static uint XPose              = 0x78;
        internal static uint PlayerID           = 0x2A8;
        internal static uint BaseProfileInfo    = 0x1A74;
        internal static uint IsClientBot        = 0x324;
        internal static uint TeamModeID         = 0x294;
        internal static uint PlayerAttributes   = 0x500;
        internal static uint LocalPlayerAttributes = 0x500;

        // ================= AVATAR ========================
        internal static uint AvatarManager      = 0x504;
        internal static uint Avatar             = 0xA8;
        internal static uint Avatar_IsVisible   = 0x95;
        internal static uint Avatar_Data        = 0x14;
        internal static uint Avatar_Data_IsTeam = 0x59;

        // ================= CAMERA ========================
        internal static uint FollowCamera       = 0x494;
        internal static uint Camera             = 0x18;
        internal static uint MainCameraTransform = 0x28C;
        internal static uint AimRotation        = 0x440;

        // ================= OBSERVER ======================
        internal static uint CurrentObserver    = 0xB4;
        internal static uint ObserverPlayer     = 0x28;

        // ================= WEAPON ========================
        internal static uint Weapon             = 0x434;
        internal static uint WeaponData         = 0x58;
        internal static uint WeaponRecoil       = 0xC;
        internal static uint UnkPlayerWeaponInfoClass = 0x4A8;
        internal static uint IsCombineWeapon    = 0xD8;
        internal static uint WeaponOnHand       = 0x54;
        internal static uint CombineWeaponOnHand = 0x58;
        internal static uint WeaponInfo         = 0x9D0;
        internal static uint WeaponID           = 0x14;
        internal static uint NoReload           = 0xC1;
        internal static uint UnlimitedAmmo      = 0x114;
        internal static uint FireDelay          = 0x24;
        internal static uint RapidfirePtr       = 0x1C8;
        internal static uint FastFire           = 0x1C8;

        // ================= SILENT AIM =====================
        internal static uint LastAimingInfoFromWeapon = 0x9D0;
        internal static uint IsFiring           = 0x58C;
        internal static uint IS_FIRING          = 0x58C;
        internal static uint sAim1              = 0x58C;
        internal static uint sAim2              = 0x9D0;
        internal static uint sAim3              = 0x38;
        internal static uint sAim4              = 0x2C;
        internal static uint StartPosition      = 0x38;
        internal static uint RayDir             = 0x2C;
        internal static uint SilentAim          = 0x444;
        internal static uint pomba              = 0x58C;

        // ================= AIMBOT =================
        internal static uint LockedAimingCollider = 0x54;
        internal static uint AimbotVisible      = 0x4E8;
        internal static uint NeckCollider       = 0x4F0;
        internal static uint kanishkcheat       = 0x54;
        internal static uint adarshcheat        = 0x54;

        // ================= BONE OFFSETS =================
        internal static uint BoneHead           = 0x49C;
        internal static uint BoneLeftWrist      = 0x518;
        internal static uint BoneHip            = 0x4A8;
        internal static uint BoneRoot           = 0x4B0;

        // ================= MISC =================
        internal static uint RunSpeedUpScale    = 0x210;
        internal static uint WeaponSkinMap      = 0x18D8;
        internal static uint InSnowSlideWayDashing = 0x15e8;
        internal static uint upplayeroffset     = 0x494;

        // ================= TRANSFORM =================
        internal static uint TransformComponent = 0x8;
        internal static uint VisualStateMatrix  = 0x20;
        internal static uint MatrixPosition     = 0x60;
        internal static uint MatrixRotation     = 0x70;
        internal static uint MatrixIndices      = 0x1C;

        // ================= MOVEMENT / FLY =================
        internal static uint MovementComponent     = 0x139C;
        internal static uint MoveComp_Position     = 0x20;
        internal static uint MoveComp_VSpeed       = 0x2C;
        internal static uint Player_IsGrounded     = 0x13F0;

        // ================= JUMP =================
        internal static uint JumpCount             = 0x140;
        internal static uint IsDoubleJumpTriggered = 0xF84;

        enum MatchStatusEnum
        {
            MATCH_NOT_STARTED = 0,
            MATCH_RUNNING     = 1,
            MATCH_PAUSED      = 2,
            MATCH_ENDED       = 3
        };
    }
}
