using System;
using System.Numerics;
using System.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace AotForms
{
    internal static class Aimbot
    {
        internal static void Work()
        {
            while (true)
            {
                if (!Config.AimBot)
                {
                    Thread.Sleep(5);
                    continue;
                }
                if ((WinAPI.GetAsyncKeyState(Config.AimbotKey) & 0x8000) == 0)
                {
                    Thread.Sleep(1);
                    continue;
                }
                if (Config.AimBotMode == "Rage")
                {
                    ClosestTarget();
                }
                else if (Config.AimBotMode == "Hex")
                {
                    NormalTarget();
                }
                else
                {
                    Thread.Sleep(Config.thread);
                }
            }
        }

        private static void ClosestTarget()
        {
            var screenCenter = new Vector2(Core.Width / 2f, Core.Height / 2f);
            var maxDistanceSquared = Config.AimBotMaxDistance * Config.AimBotMaxDistance;
            var fovSquared = Config.AimBotFov * Config.AimBotFov;

            Entity target = null;
            float minDistanceSquared = float.MaxValue;
            var entities = Core.Entities.Values;
            foreach (var entity in entities)
            {
                if (entity == null || entity.IsDead || entity.IsKnocked)
                    continue;
                var playerDistanceSquared = Vector3.DistanceSquared(Core.LocalMainCamera, entity.Head);
                if (playerDistanceSquared > maxDistanceSquared)
                    continue;
                var head2D = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
                if (head2D.X <= 0 || head2D.Y <= 0 || head2D.X >= Core.Width || head2D.Y >= Core.Height)
                    continue;
                var x = head2D.X - screenCenter.X;
                var y = head2D.Y - screenCenter.Y;
                var crosshairDistSquared = x * x + y * y;
                if (crosshairDistSquared > fovSquared)
                    continue;
                if (playerDistanceSquared < minDistanceSquared && !entity.IsDead && !entity.IsKnocked)
                {
                    minDistanceSquared = playerDistanceSquared;
                    target = entity;
                }
            }
            if (target != null && !target.IsDead && !target.IsKnocked)
            {
                var EnemyHead = target.Head;
                var targetRotation = MathUtils.GetRotationToLocation(EnemyHead, 0.1f, Core.LocalMainCamera);

                InternalMemory.Write(Core.LocalPlayer + Offsets.AimRotation, targetRotation);

                Thread.Sleep(Config.thread);

            }
        }

        private static void NormalTarget()
        {
            Entity target = null;
            float distanceSquared = float.MaxValue;
            var screenCenter = new Vector2(Core.Width / 2f, Core.Height / 2f);

            var fovSquared = Config.AimBotFov * Config.AimBotFov;

            foreach (var entity in Core.Entities.Values)
            {
                if (Core.Width == -1 || Core.Height == -1 || !Core.HaveMatrix) continue;
                if (entity.IsDead || (Config.IgnoreKnocked && entity.IsKnocked)) continue;

                var head2D = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
                if (head2D.X < 1 || head2D.Y < 1) continue;

                var playerDistanceSquared = Vector3.DistanceSquared(Core.LocalMainCamera, entity.Head);
                if (playerDistanceSquared > Config.AimBotMaxDistance * Config.AimBotMaxDistance) continue;

                var x = head2D.X - screenCenter.X;
                var y = head2D.Y - screenCenter.Y;
                var crosshairDistSquared = x * x + y * y;

                if (crosshairDistSquared >= distanceSquared * 0.95f || crosshairDistSquared > fovSquared) continue;

                distanceSquared = crosshairDistSquared;
                target = entity;
            }

            if (target != null && !target.IsDead && !target.IsKnocked)
            {
                var EnemyHead = target.Head;
                var targetRotation = MathUtils.GetRotationToLocation(EnemyHead, 0.1f, Core.LocalMainCamera);
                InternalMemory.Write(Core.LocalPlayer + Offsets.AimRotation, targetRotation);

                Thread.Sleep(Config.thread);
            }
        }

    }
}