using AotForms;
using System;
using System.Numerics;
using System.Threading;

namespace AotForms
{
    internal static class ClimbUp
    {
        private static Thread climbThread;
        private static CancellationTokenSource cts;
        private static bool isRunning = false;

        internal static void Start()
        {
            if (isRunning) return; // Already running

            isRunning = true;
            cts = new CancellationTokenSource();
            climbThread = new Thread(() => Run(cts.Token));
            climbThread.IsBackground = true;
            climbThread.Start();
        }

        internal static void Stop()
        {
            if (!isRunning) return;

            cts.Cancel();
            climbThread.Join();
            isRunning = false;
        }

        private static void Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (!Config.ClimbUpEnabled)
                {
                    Thread.Sleep(5);
                    continue;
                }

                if (Core.Width == -1 || Core.Height == -1 || !Core.HaveMatrix)
                {
                    Thread.Sleep(5);
                    continue;
                }

                try
                {
                    // Get local player root bone
                    if (!InternalMemory.Read(Core.LocalPlayer + (uint)Bones.Root, out uint rootBonePtr)) continue;
                    if (!InternalMemory.Read(rootBonePtr + 0x8, out uint transformValue)) continue;
                    if (!InternalMemory.Read(transformValue + 0x8, out uint transformObjPtr)) continue;
                    if (!InternalMemory.Read(transformObjPtr + 0x20, out uint matrixPtr)) continue;

                    if (!Transform.GetNodePosition(rootBonePtr, out var currentPos)) continue;

                    // STEP 1: Hold downward position
                    Vector3 lowerPos = new Vector3(currentPos.X, currentPos.Y - 1.5f, currentPos.Z);
                    for (int d = 0; d < 20 && !token.IsCancellationRequested; d++)
                    {
                        InternalMemory.Write(matrixPtr + 0x80, lowerPos);
                        Thread.Sleep(5);
                    }

                    if (!Transform.GetNodePosition(rootBonePtr, out currentPos)) continue;

                    // STEP 2: Move up
                    float stepSize = 1.5f;
                    int safetyLimit = 50;
                    Vector3 climbPos = currentPos;

                    for (int i = 0; i < safetyLimit && Config.ClimbUpEnabled && !token.IsCancellationRequested; i++)
                    {
                        Vector3 nextPos = new Vector3(climbPos.X, climbPos.Y + stepSize, climbPos.Z);
                        InternalMemory.Write(matrixPtr + 0x80, nextPos);
                        Thread.Sleep(3);

                        if (!Transform.GetNodePosition(rootBonePtr, out var updatedPos)) break;
                        climbPos = updatedPos;
                    }

                    // STEP 3: Smooth landing
                    Vector3 finalPos = new Vector3(climbPos.X, climbPos.Y + 1.0f, climbPos.Z);
                    InternalMemory.Write(matrixPtr + 0x80, finalPos);
                }
                catch
                {
                    // Keep loop alive
                }
            }
        }
    }
}
