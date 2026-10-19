using System.Runtime.InteropServices;

namespace AotForms
{
    internal static class EnemyPullHelpers
    {
        private const int VK_LBUTTON = 0x01;

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        internal static bool IsLocalFiring()
        {
            if ((GetAsyncKeyState(VK_LBUTTON) & 0x6000) != 0)
                return true;

            if (Core.LocalPlayer == 0)
                return false;

            if (InternalMemory.Read(Core.LocalPlayer + Offsets.isFiring, out bool isFiring) && isFiring)
                return true;

            if (InternalMemory.Read(Core.LocalPlayer + Offsets.isFiring, out bool isFiringAlt) && isFiringAlt)
                return true;

            return false;
        }

        internal static float GetPullFovRadius()
        {
            return 200f; // Aap apne hisaab se radius set kar sakte ho 100f, 200f etc.
        }
    }
}
