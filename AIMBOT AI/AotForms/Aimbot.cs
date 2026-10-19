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
                // Check if Aimbot is enabled
                if (!Config.AimBot)
                {
                    Thread.Sleep(5); // Slightly higher sleep to reduce CPU load when disabled
                    continue;
                }

                // Check if the aimbot key is being pressed
                if ((WinAPI.GetAsyncKeyState(Config.AimbotKey) & 0x8000) == 0)
                {
                    Thread.Sleep(1);
                    continue;
                }

                // Execute the appropriate aimbot logic based on the selected mode
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
                    Thread.Sleep(Config.thread); // Default sleep if no valid mode is selected
                }
            }
        }


        private static void ClosestTarget()
        {
            // Cache frequently used values
            var screenCenter = new Vector2(Core.Width / 2f, Core.Height / 2f);
            var maxDistanceSquared = Config.AimBotMaxDistance * Config.AimBotMaxDistance;
            var fov = Config.AimFov ? Config.AimBotFov : 50; // Use configured FOV if enabled, otherwise default to 50
            var fovSquared = fov * fov;

            Entity target = null;
            float minDistanceSquared = float.MaxValue; // Track closest distance squared

            // Cache entities to reduce overhead
            var entities = Core.Entities.Values;

            // Iterate through all available entities (players)
            foreach (var entity in entities)
            {
                // Skip invalid, dead, or knocked players
                if (entity == null || entity.IsDead || entity.IsKnocked)
                    continue;

                // Check 3D distance squared first (cheaper than W2S calculation)
                var playerDistanceSquared = Vector3.DistanceSquared(Core.LocalMainCamera, entity.Head);
                if (playerDistanceSquared > maxDistanceSquared)
                    continue;

                // Convert 3D world coordinates to 2D screen coordinates
                var head2D = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);

                // Ensure the entity is visible on screen
                if (head2D.X <= 0 || head2D.Y <= 0 || head2D.X >= Core.Width || head2D.Y >= Core.Height)
                    continue;

                // Calculate 2D screen distance squared from the crosshair center
                var x = head2D.X - screenCenter.X;
                var y = head2D.Y - screenCenter.Y;
                var crosshairDistSquared = x * x + y * y;

                // Skip if outside FOV
                if (crosshairDistSquared > fovSquared)
                    continue;

                // Select the closest player who is also inside the FOV and not dead or knocked and Visible
                if (playerDistanceSquared < minDistanceSquared && !entity.IsDead && !entity.IsKnocked)
                {
                    minDistanceSquared = playerDistanceSquared;
                    target = entity;
                }
            }

            // If a valid target is found, adjust aim towards the target's head
            if (target != null && !target.IsDead && !target.IsKnocked)
            {
                // Get the target's head position in world space
                var EnemyHead = target.Head;

                // Calculate the required rotation to aim at the target's head
                var targetRotation = MathUtils.GetRotationToLocation(EnemyHead, 0.1f, Core.LocalMainCamera);

                InternalMemory.Write(Core.LocalPlayer + Offsets.AimRotation, targetRotation);

                Thread.Sleep(Config.thread);

            }
        }

        private static void NormalTarget()
        {
            Entity target = null;
            float distanceSquared = float.MaxValue; // Use squared distance for accuracy
            var screenCenter = new Vector2(Core.Width / 2f, Core.Height / 2f);

            var fov = Config.AimFov ? Config.AimBotFov : 50;
            var fovSquared = fov * fov;

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

                Thread.Sleep(Config.thread); // Slightly increased sleep time
            }
        }


    }
}