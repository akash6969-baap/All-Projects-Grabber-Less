
using System.Numerics;

namespace AotForms
{
    internal class Entity
    {
        internal uint Address;
        internal bool IsKnown;
        internal Bool3 IsTeam;
        internal Vector3 Head;
        internal Vector3 Neck;
        internal Vector3 LeftWrist;
        internal Vector3 RightWrist;
        internal Vector3 Spine;
        internal Vector3 Root;
        internal Vector3 Hip;
        internal Vector3 RightCalf;
        internal Vector3 LeftCalf;
        internal Vector3 RightFoot;
        internal Vector3 LeftFoot;
        internal Vector3 LeftHand;
        internal Vector3 LeftShoulder;
        internal Vector3 RightShoulder;
        internal Vector3 RightWristJoint;
        internal Vector3 LeftWristJoint;
        internal Vector3 RightElbow;
        internal Vector3 LeftElbow;
        internal bool IsEnemy;
        internal Vector3 Breast;
        internal Vector3 BoneRoot;
        internal Vector3 WeaponMount;
        internal Vector3 LeftWeapon;
        internal Vector3 LeftForeArm;
        internal Vector3 RightForeArm;
        internal Vector3 RightHand;
        internal Vector3 LeftAnkle;
        internal Vector3 RightAnkle;
        internal Vector3 LeftToe;
        internal Vector3 RightToe;
        internal short Health;
        internal bool IsDead;
        internal bool IsKnocked;
        internal string Name;
        internal float Distance;
        internal bool IsFiring;
        internal bool isVisible;
        internal string WeaponName;
        internal uint BaseAddress;

        internal bool target;
        internal short Gun;

    }
}
