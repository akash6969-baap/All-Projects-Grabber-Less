
using System.Numerics;

namespace AotForms
{
    internal class Entity
    {
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
        internal Vector3 RighFoot;
        internal Vector3 LeftHand;
        internal Vector3 LeftSholder;
        internal Vector3 RightSholder;
        internal Vector3 RightWristJoint;
        internal Vector3 LeftWristJoint;
        internal Vector3 RightElbow;
        internal Vector3 LeftElbow;
        internal short Health;
        internal bool IsDead;
        internal bool IsKnocked;
        internal string Name;
        internal float Distance;
        internal bool IsFiring;
        internal bool isVisible;

        public uint Address { get; internal set; }

        public Vector3 GetBonePosition(int offset)
        {
            switch (offset)
            {
                case 0x2fc: return Head;
                case 0x300: return Hip;
                case 0x310: return RightSholder;
                case 0x330: return LeftSholder;

                case 0x320: return LeftFoot;
                case 0x324: return RightFoot;

                case 0x33c: return LeftHand;
                case 0x340: return RightElbow;
                case 0x344: return LeftElbow;
                default: return Vector3.Zero;
            }
        }

    }
}
