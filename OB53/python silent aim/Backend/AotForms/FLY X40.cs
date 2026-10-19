using System;
using System.Numerics;
using System.Threading;

namespace AotForms
{
    internal static class FLYHACKX40
    {
        private static Thread _thread;
        private static bool _started = false;

        private const float MAX_ALTITUDE = 2.0f;  // 2.0f = server range mein, bullets register honge
        private const float DEAD_ZONE    = 0.15f; // Sirf tab write karo jab position itna off ho

        private static float groundY       = 0f;
        private static bool  isInitialized = false;

        internal static void Work()
        {
            if (_started) return;
            _started = true;
            _thread = new Thread(Loop) { IsBackground = true };
            _thread.Start();
        }

        // AGENTS.md Rule 1: while(true) — thread kabhi nahi marega
        private static void Loop()
        {
            while (true)
            {
                if (!Config.FLYHACKX400)
                {
                    if (isInitialized)
                    {
                        RestoreNormalPosition();
                        isInitialized = false;
                        groundY       = 0f;
                    }
                    Thread.Sleep(40);
                    continue;
                }

                try
                {
                    ulong localPlayer = Core.LocalPlayer;
                    if (localPlayer == 0) { Thread.Sleep(20); continue; }

                    if (Config.TeleportAnywhere || Config.teleportmap || Config.directteleport)
                    { Thread.Sleep(20); continue; }

                    // Dusre fly hacks band karo (conflict avoid)
                    if (Config.FlyHack80x) Config.FlyHack80x = false;
                    if (Config.FlyHack50x) Config.FlyHack50x = false;

                    // Transform chain traverse
                    if (!InternalMemory.Read<uint>(localPlayer + (uint)Bones.Root, out uint rootPtr) || rootPtr == 0) { Thread.Sleep(30); continue; }
                    if (!InternalMemory.Read<uint>(rootPtr + 0x8, out uint t1) || t1 == 0)  { Thread.Sleep(30); continue; }
                    if (!InternalMemory.Read<uint>(t1 + 0x8, out uint t2) || t2 == 0)       { Thread.Sleep(30); continue; }
                    if (!InternalMemory.Read<uint>(t2 + 0x20, out uint matrixPtr) || matrixPtr == 0) { Thread.Sleep(30); continue; }

                    if (!InternalMemory.Read<Vector3>(matrixPtr + 0x60, out Vector3 currentPos)) { Thread.Sleep(30); continue; }

                    // Pehli baar: ground Y save karo
                    if (!isInitialized)
                    {
                        groundY       = currentPos.Y;
                        isInitialized = true;
                    }

                    float targetY = groundY + MAX_ALTITUDE;
                    float diff    = targetY - currentPos.Y;

                    // MoveComponent se gravity disable + speed-based correction
                    if (InternalMemory.Read<ulong>(localPlayer + 0x124C, out ulong moveComp) && moveComp != 0)
                    {
                        InternalMemory.Write<byte>(moveComp + 0x150, 0); // gravity off

                        // Altitude correct karo vspeed se — matrix se fight nahi hoga
                        float vSpeed;
                        if (diff > 0.3f)
                            vSpeed = 2.0f;   // bahut niche hain — upar jaao
                        else if (diff < -0.3f)
                            vSpeed = -1.5f;  // bahut upar hain — niche aao
                        else
                            vSpeed = 0.02f;  // range mein — hover

                        InternalMemory.Write<float>(moveComp + 0x2C, vSpeed);
                    }

                    // Matrix: sirf tab write karo jab zyada off ho (dead zone = kam jitter)
                    if (Math.Abs(diff) > DEAD_ZONE)
                    {
                        Vector3 targetPos = currentPos;
                        targetPos.Y = targetY;
                        InternalMemory.Write<Vector3>(matrixPtr + 0x60, targetPos);
                        InternalMemory.Write<float>(matrixPtr + 0x60 + 0xC, 1.0f);
                    }

                    // NOTE: InSnowSlideWayDashing HATAAYA — aage bhaagne ka karan tha!
                }
                catch { }

                Thread.Sleep(30);
            }
        }

        private static void RestoreNormalPosition()
        {
            try
            {
                ulong localPlayer = Core.LocalPlayer;
                if (localPlayer == 0) return;

                if (InternalMemory.Read<uint>(localPlayer + (uint)Bones.Root, out uint rootPtr) &&
                    InternalMemory.Read<uint>(rootPtr + 0x8, out uint t1) &&
                    InternalMemory.Read<uint>(t1 + 0x8, out uint t2) &&
                    InternalMemory.Read<uint>(t2 + 0x20, out uint matrixPtr) && matrixPtr != 0)
                {
                    if (InternalMemory.Read<Vector3>(matrixPtr + 0x60, out Vector3 cur))
                    {
                        cur.Y = groundY;
                        InternalMemory.Write<Vector3>(matrixPtr + 0x60, cur);
                    }
                }
            }
            catch { }
        }

        internal static void SetState(bool enable)
        {
            Config.FLYHACKX400 = enable;
            if (!enable)
            {
                RestoreNormalPosition();
                isInitialized = false;
                groundY       = 0f;
            }
        }

        internal static void Stop() => SetState(false);
    }
}