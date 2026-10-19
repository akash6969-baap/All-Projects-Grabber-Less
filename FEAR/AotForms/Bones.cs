
namespace AotForms
{
    internal enum Bones : uint
    {
        Root = 0x3CC,           // UPDATED - Base offset
        Head = 0x3B8,           // UPDATED
        Spine = 0x3C0,          // UPDATED
        Hip = 0x3C8,            // UPDATED

        // Arms
        LeftWrist = 0x3B4,      // UPDATED
        RightWrist = 0x3E0,     // UPDATED
        LeftHand = 0x3E4,       // UPDATED

        // Shoulders
        LeftSholder = 0x3EC,    // UPDATED (note: typo 'Sholder' kept for compatibility)
        RightSholder = 0x3F0,   // UPDATED

        // Elbows
        LeftElbow = 0x3FC,      // UPDATED
        RightElbow = 0x400,     // UPDATED

        // Wrist joints
        RightWristJoint = 0x3F4, // UPDATED
        LeftWristJoint = 0x3F8,  // UPDATED

        // Legs
        RightCalf = 0x3D0,      // UPDATED
        LeftCalf = 0x3D4,       // UPDATED
        RightFoot = 0x3D8,      // UPDATED
        LeftFoot = 0x3DC,       // UPDATED
    }
}
