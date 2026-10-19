using System;
using System.Numerics;
using System.Threading;

namespace AotForms
{
    internal class Speed
    {
        private static bool isRunning = false;
        private static Thread speedThread;

        // 👇 FlyHack jaisa start point
        public static void Start()
        {
            if (isRunning) return;

            isRunning = true;

            speedThread = new Thread(Loop)
            {
                IsBackground = true
            };
            speedThread.Start();
        }

        public static void Stop()
        {
            isRunning = false;
        }

        private static void Loop()
        {
            while (isRunning)
            {
                try
                {
                    if (Config.SpeedEnabled)
                    {
                        TeleportForward(Config.SpeedValue);
                    }

                    Thread.Sleep(30); // smooth speed
                }
                catch { }
            }
        }

        private static void TeleportForward(float distance)
        {
            try
            {
                // Read aim rotation
                if (!InternalMemory.Read<Quaternion>(Core.LocalPlayer + Offsets.AimRotation, out var rotation))
                    return;

                Vector3 forward = GetForward(rotation);

                // Read root bone
                if (!InternalMemory.Read<uint>(Core.LocalPlayer + (uint)Bones.Root, out var rootBone) || rootBone == 0)
                    return;

                if (!InternalMemory.Read<uint>(rootBone + 0x8, out var t1) || t1 == 0)
                    return;

                if (!InternalMemory.Read<uint>(t1 + 0x8, out var t2) || t2 == 0)
                    return;

                if (!InternalMemory.Read<uint>(t2 + 0x20, out var matrix) || matrix == 0)
                    return;

                // Read position
                if (!InternalMemory.Read<Vector3>(matrix + 0x80, out var pos))
                    return;

                // New position
                Vector3 newPos = pos + (forward * distance);

                // Write position
                InternalMemory.Write<Vector3>(matrix + 0x80, newPos);
            }
            catch { }
        }

        private static Vector3 GetForward(Quaternion q)
        {
            return new Vector3(
                2f * (q.X * q.Z + q.W * q.Y),
                2f * (q.Y * q.Z - q.W * q.X),
                1f - 2f * (q.X * q.X + q.Y * q.Y)
            );
        }
    }
}
