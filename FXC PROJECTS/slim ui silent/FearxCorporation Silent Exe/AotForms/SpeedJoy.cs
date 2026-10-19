using System;
using System.Threading.Tasks;
using AotForms;

namespace Client
{
    internal class SpeedJoy
    {
        public static async Task ToggleSpeedJoy(bool enable)
        {
            if (Core.LocalPlayer == 0) return;

            uint attributesAddr;
            if (InternalMemory.Read<uint>(Core.LocalPlayer + Offsets.PlayerAttributes, out attributesAddr) && attributesAddr != 0)
            {
                float speedValue = enable ? 9.0f : 1.0f;
                InternalMemory.Write<float>(attributesAddr + 0x188, speedValue);
                InternalMemory.Write<float>(attributesAddr + 0x1B0, speedValue);
                InternalMemory.Write<float>(attributesAddr + 0x140, speedValue);
            }

            await Task.CompletedTask;
        }

        public static void SetDashSpeed(bool enable, float speedMultiplier = 9.0f)
        {
            if (Core.LocalPlayer == 0) return;
            uint attributesAddr;
            if (InternalMemory.Read<uint>(Core.LocalPlayer + Offsets.PlayerAttributes, out attributesAddr) && attributesAddr != 0)
            {
                float val = enable ? speedMultiplier : 1.0f;
                InternalMemory.Write<float>(attributesAddr + 0x188, val);
            }
        }
    }
}
