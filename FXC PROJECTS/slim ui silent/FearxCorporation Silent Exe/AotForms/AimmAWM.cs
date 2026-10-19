using System;
using System.Numerics;
using System.Threading;

namespace AotForms
{
    internal static class AimmAWM
    {
        internal static bool Enabled = false;
        private static bool _wasPressed = false;

        internal static void Work()
        {
            while (true)
            {
                if (!Config.AimmmAwm)
                {
                    Thread.Sleep(5);
                    _wasPressed = false;
                    continue;
                }

                if (Core.LocalPlayer == 0 || Core.Width == -1 || Core.Height == -1 || !Core.HaveMatrix)
                {
                    Thread.Sleep(5);
                    continue;
                }

                // Config.AwmKey mein wahi key set karo jo BlueStacks macro mein fire pe assign ki hai
                bool firePressed = (WinAPI.GetAsyncKeyState(Config.AwmKey) & 0x8000) != 0;

                if (!firePressed)
                {
                    _wasPressed = false;
                    Thread.Sleep(1);
                    continue;
                }

                if (_wasPressed)
                {
                    Thread.Sleep(1);
                    continue;
                }

                _wasPressed = true;

                // Sabse nazdik enemy dhundho
                Entity target = null;
                float minDist = float.MaxValue;
                var screenCenter = new Vector2(Core.Width / 2f, Core.Height / 2f);

                foreach (var entity in Core.Entities.Values)
                {
                    if (entity == null || !entity.IsKnown) continue;
                    if (entity.IsDead) continue;
                    if (Config.IgnoreKnocked && entity.IsKnocked) continue;

                    var head2D = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);

                    if (head2D.X <= 0 || head2D.Y <= 0 || head2D.X >= Core.Width || head2D.Y >= Core.Height)
                        continue;

                    var screenDist = Vector2.Distance(screenCenter, new Vector2(head2D.X, head2D.Y));

                    if (screenDist < minDist)
                    {
                        minDist = screenDist;
                        target = entity;
                    }
                }

                if (target == null || target.IsDead)
                    continue;

                // Instant aim snap - enemy ke head pe
                var rotation = MathUtils.GetRotationToLocation(target.Head, 1.0f, Core.LocalMainCamera);
                InternalMemory.Write(Core.LocalPlayer + Offsets.AimRotation, rotation);
            }
        }
    }
}
