using AotForms;
using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    internal static class Downplayer
    {
        private static Task downTask;
        private static CancellationTokenSource cts = new();
        private static bool isRunning = false;

        internal static void Work()
        {
            if (isRunning) return;
            isRunning = true;

            downTask = Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        if (!Config.down || Core.Entities == null || !Core.HaveMatrix) // fire chk add kar sakte ho to make it better kam lag  hoga : )
                        {
                            await Task.Delay(10, cts.Token);
                            continue;
                        }

                        foreach (var entity in Core.Entities.Values)
                        {
                            if (!entity.IsKnown || entity.IsDead || entity.Address == Core.LocalPlayer)
                                continue;

                            // Get Root Bone Pointer
                            if (!InternalMemory.Read(entity.Address + (uint)Bones.Root, out uint rootBonePtr) || rootBonePtr == 0)
                                continue;

                            if (!InternalMemory.Read(rootBonePtr + 0x8, out uint transform1) || transform1 == 0)
                                continue;

                            if (!InternalMemory.Read(transform1 + 0x8, out uint transform2) || transform2 == 0)
                                continue;

                            if (!InternalMemory.Read(transform2 + 0x20, out uint matrixPtr) || matrixPtr == 0)
                                continue;

                            if (!InternalMemory.Read(matrixPtr + 0x80, out Vector3 currentPos))
                                continue;


                            currentPos.Y -= Config.downSpeed;


                            InternalMemory.Write(matrixPtr + 0x80, currentPos);
                        }
                    }
                    catch
                    {

                    }

                    await Task.Delay(10, cts.Token);
                }
            }, cts.Token);
        }

        internal static void Stop()
        {
            if (!isRunning) return;

            cts.Cancel();
            cts.Dispose();
            cts = new CancellationTokenSource();
            isRunning = false;
        }
    }
}