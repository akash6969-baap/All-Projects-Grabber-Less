using System;
using System.Threading;
using System.Windows.Forms;
using System.Numerics;

namespace AotForms
{
    internal static class FlyRage
    {
        private static Thread _thread;
        private static bool _started = false;

        // Har tick kitna upar/niche jaana hai (matrix increment)
        private const float DELTA_UP    = 0.08f;  // SPACE
        private const float DELTA_TURBO = 0.18f;  // SHIFT+SPACE
        private const float DELTA_DOWN  = 0.06f;  // LMB

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
                if (!Config.FlyRage || Core.LocalPlayer == 0)
                {
                    Thread.Sleep(20);
                    continue;
                }

                try
                {
                    ulong lp = Core.LocalPlayer;

                    // Transform chain traverse karo current position paane ke liye
                    if (!InternalMemory.Read<uint>(lp + (uint)Bones.Root, out uint rootPtr) || rootPtr == 0) { Thread.Sleep(20); continue; }
                    if (!InternalMemory.Read<uint>(rootPtr + 0x8, out uint t1) || t1 == 0)  { Thread.Sleep(20); continue; }
                    if (!InternalMemory.Read<uint>(t1 + 0x8, out uint t2) || t2 == 0)       { Thread.Sleep(20); continue; }
                    if (!InternalMemory.Read<uint>(t2 + 0x20, out uint matrixPtr) || matrixPtr == 0) { Thread.Sleep(20); continue; }
                    if (!InternalMemory.Read<Vector3>(matrixPtr + 0x60, out Vector3 currentPos)) { Thread.Sleep(20); continue; }

                    bool spaceDown = (WinAPI.GetAsyncKeyState((Keys)0x20) & 0x8000) != 0;
                    bool shiftDown = (WinAPI.GetAsyncKeyState((Keys)0x10) & 0x8000) != 0;
                    bool lmbDown   = (WinAPI.GetAsyncKeyState((Keys)0x01) & 0x8000) != 0;

                    if (spaceDown || lmbDown)
                    {
                        // Key dabaya hua — matrix se move karo, gravity off
                        float delta;
                        if (spaceDown && shiftDown)
                            delta = DELTA_TURBO;   // SHIFT+SPACE = turbo upar
                        else if (spaceDown)
                            delta = DELTA_UP;      // SPACE = upar
                        else
                            delta = -DELTA_DOWN;   // LMB = niche

                        // Current Y se increment karo — koi fixed targetY nahi = no jitter
                        Vector3 newPos = currentPos;
                        newPos.Y += delta;

                        InternalMemory.Write<Vector3>(matrixPtr + 0x60, newPos);
                        InternalMemory.Write<float>(matrixPtr + 0x60 + 0xC, 1.0f);

                        // Gravity off + vspeed zero (matrix handle karega movement)
                        if (InternalMemory.Read<ulong>(lp + 0x124C, out ulong mc) && mc != 0)
                        {
                            InternalMemory.Write<byte>(mc + 0x150, 0);   // gravity off
                            InternalMemory.Write<float>(mc + 0x2C, 0.0f); // physics neutral
                        }
                    }
                    else
                    {
                        // Koi key nahi — gravity wapas ON, matrix mat chuo → natural fall
                        if (InternalMemory.Read<ulong>(lp + 0x124C, out ulong mc) && mc != 0)
                        {
                            InternalMemory.Write<byte>(mc + 0x150, 1); // gravity on
                        }
                    }
                }
                catch { }

                Thread.Sleep(20);
            }
        }
    }
}