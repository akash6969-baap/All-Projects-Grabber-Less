using AotForms;
using System;
using System.Numerics;
using System.Threading;

namespace AotForms
{
    internal static class FlyHack
    {
        private static Thread flyThread;
        private static CancellationTokenSource cts;
        private static bool isRunning = false;

        public static void Start()
        {
            if (isRunning)
                return;

            cts = new CancellationTokenSource();

            flyThread = new Thread(() => Work(cts.Token))
            {
                IsBackground = true,
                Priority = ThreadPriority.Highest
            };

            flyThread.Start();
            isRunning = true;
        }

        public static void Stop()
        {
            if (!isRunning)
                return;

            try
            {
                cts.Cancel();
                flyThread?.Join(300);
            }
            catch { }

            isRunning = false;
        }

        private static void Work(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (!Config.FlyHackEnabled)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    if (Core.LocalPlayer == 0)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    // Read root bone
                    if (!InternalMemory.Read(Core.LocalPlayer + (uint)Bones.Root, out uint rootBonePtr))
                        continue;

                    if (!InternalMemory.Read(rootBonePtr + 0x8, out uint transformValue))
                        continue;

                    if (!InternalMemory.Read(transformValue + 0x8, out uint transformObjPtr))
                        continue;

                    if (!InternalMemory.Read(transformObjPtr + 0x20, out uint matrixPtr))
                        continue;

                    if (!Transform.GetNodePosition(rootBonePtr, out Vector3 currentPos))
                        continue;

                    // Smooth fly (keep X,Z, change Y)
                    float targetY = Config.FlyHeight;

                    float smoothSpeed = 0.15f; // smoothness control
                    float newY = currentPos.Y + (targetY - currentPos.Y) * smoothSpeed;

                    Vector3 newPos = new Vector3(currentPos.X, newY, currentPos.Z);

                    // Write position
                    InternalMemory.Write(matrixPtr + 0x80, newPos);
                }
                catch
                {
                    // ignore memory errors
                }

                Thread.Sleep(5); // smooth update
            }
        }
    }
}
