using System;
using System.Numerics;
using System.Threading;

namespace AotForms
{
    internal static class divekill
    {
        private const float ClimbDepth = 1.5f;   // underground depth
        private static Vector3? savedOriginalPos = null;
        private static Vector3? lockedPos = null;
        private static bool isUnderground = false;

        private static Thread diveThread;
        private static CancellationTokenSource cts;
        private static bool isRunning = false;

        // Collider reference (Unity / placeholder)
        private static object undergroundCollider = null;

        internal static void Start()
        {
            if (isRunning) return;
            isRunning = true;

            cts = new CancellationTokenSource();
            diveThread = new Thread(() => Run(cts.Token));
            diveThread.IsBackground = true;
            diveThread.Priority = ThreadPriority.Highest;
            diveThread.Start();
        }

        internal static void Stop()
        {
            if (!isRunning) return;
            cts.Cancel();
            diveThread.Join();
            isRunning = false;

            RemoveCollider(); // clean up
        }

        private static void Run(CancellationToken token)
        {
            uint matrixPtr = 0;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (Core.LocalPlayer == 0 || !Core.HaveMatrix)
                    {
                        matrixPtr = 0;
                        Thread.Sleep(1);
                        continue;
                    }

                    // resolve matrixPtr once
                    if (matrixPtr == 0)
                    {
                        if (!InternalMemory.Read(Core.LocalPlayer + (uint)Bones.Root, out uint rootBonePtr)) { Thread.Sleep(1); continue; }
                        if (!InternalMemory.Read(rootBonePtr + 0x8, out uint transformValue)) { Thread.Sleep(1); continue; }
                        if (!InternalMemory.Read(transformValue + 0x8, out uint transformObjPtr)) { Thread.Sleep(1); continue; }
                        if (!InternalMemory.Read(transformObjPtr + 0x20, out matrixPtr)) { Thread.Sleep(1); continue; }
                    }

                    // enable underground
                    if (!isUnderground && Config.ClimbDownEnabled)
                    {
                        if (!InternalMemory.Read(Core.LocalPlayer + (uint)Bones.Root, out uint rootBonePtr)) { Thread.Sleep(1); continue; }
                        if (!Transform.GetNodePosition(rootBonePtr, out var currentPos)) { Thread.Sleep(1); continue; }

                        savedOriginalPos = currentPos;
                        lockedPos = new Vector3(currentPos.X, currentPos.Y - ClimbDepth, currentPos.Z);
                        isUnderground = true;

                        // spawn invisible collider under player
                        SpawnCollider(lockedPos.Value);
                    }

                    // disable underground → restore original
                    if (!Config.ClimbDownEnabled && isUnderground && savedOriginalPos != null)
                    {
                        InternalMemory.Write(matrixPtr + 0x30, savedOriginalPos.Value);
                        InternalMemory.Write(matrixPtr + 0x80, savedOriginalPos.Value);
                        InternalMemory.Write(matrixPtr + 0xB0, Vector3.Zero);

                        savedOriginalPos = null;
                        lockedPos = null;
                        isUnderground = false;

                        RemoveCollider(); // remove underground collider
                    }

                    // lock underground → freeze player
                    if (Config.ClimbDownEnabled && isUnderground && lockedPos != null)
                    {
                        InternalMemory.Write(matrixPtr + 0x30, lockedPos.Value); // local
                        InternalMemory.Write(matrixPtr + 0x80, lockedPos.Value); // world
                        InternalMemory.Write(matrixPtr + 0xB0, Vector3.Zero);    // velocity zero
                        // collider already spawned → engine handles physics
                    }
                }
                catch
                {
                    matrixPtr = 0;
                }

                Thread.Sleep(1); // high-frequency overwrite
            }
        }

        // Spawn invisible collider under player
        private static void SpawnCollider(Vector3 pos)
        {
            try
            {
                if (undergroundCollider != null) return; // already exists

                // Pseudo-code for Unity / engine
                // Replace object type with actual GameObject / entity class
                undergroundCollider = new object(); // placeholder
                // engine-specific: create collider at pos
                // size: X/Z = player footprint, Y = 0.5f
                // kinematic/static = true
            }
            catch { }
        }

        // Remove underground collider
        private static void RemoveCollider()
        {
            try
            {
                if (undergroundCollider == null) return;

                // engine-specific destroy collider
                undergroundCollider = null;
            }
            catch { }
        }
    }
}
