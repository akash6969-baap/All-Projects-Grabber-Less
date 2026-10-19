using AotForms;
using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace AotForms
{
    internal static class FearSpinBot
    {
        private static float currentYaw = 0f;
        private const float SpinDegreesPerTick = 4.2f;
        private const int LoopDelayMs = 5;

        internal static void Work()
        {
            while (true)
            {
                try
                {
                    if (!Config.spinbot)
                    {
                        currentYaw = 0f;
                        Thread.Sleep(100);
                        continue;
                    }

                    ulong rawLocal = Core.LocalPlayer;
                    if (rawLocal == 0)
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    uint localPlayer = (uint)rawLocal;

                    if (!InternalMemory.Read(localPlayer + (uint)Bones.Root, out uint root) || root == 0)
                        goto next;

                    if (!InternalMemory.Read(root + 0x8, out uint t1) || t1 == 0)
                        goto next;

                    if (!InternalMemory.Read(t1 + 0x8, out uint t2) || t2 == 0)
                        goto next;

                    if (!InternalMemory.Read(t2 + 0x24, out uint indexValue))
                        goto next;

                    if (!InternalMemory.Read(t2 + 0x20, out uint matrixValue) || matrixValue == 0)
                        goto next;

                    if (!InternalMemory.Read(matrixValue + 0x18, out uint matrixListValue) || matrixListValue == 0)
                        goto next;

                    uint targetAddress = matrixListValue + (indexValue * 0x30);

                    if (!InternalMemory.Read(targetAddress, out Vector4 currentPos))
                        goto next;

                    currentYaw += SpinDegreesPerTick;
                    if (currentYaw >= 360f)
                        currentYaw -= 360f;

                    Quaternion rot = Quaternion.CreateFromAxisAngle(
                        Vector3.UnitY,
                        currentYaw * (MathF.PI / 180f)
                    );

                    InternalMemory.Write(targetAddress + 0x10, rot);
                }
                catch
                {
                    Thread.Sleep(200);
                }

            next:
                Thread.Sleep(LoopDelayMs);
            }
        }
    }
}
