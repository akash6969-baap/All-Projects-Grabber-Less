using AotForms;
using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    internal static class SilentC
    {
        internal static int AimbotSpeed = 1;

        internal static void Work()
        {
            while (true)
            {
                if (!Config.SILENT)
                {
                    Thread.Sleep(1);
                    continue;
                }

                if (Core.Width == -1 || Core.Height == -1 || !Core.HaveMatrix)
                {
                    Thread.Sleep(1);
                    continue;
                }

                var screenCenter = new Vector2(Core.Width / 2f, Core.Height / 2f);
                Entity target = GetTarget360();
                float minDistance = float.MaxValue;
                object lockObject = new object();

                Parallel.ForEach(Core.Entities.Values, entity =>
                {
                    if (entity.IsDead || entity.IsKnocked || !entity.IsKnown)
                        return;

                    var head2D = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
                    if (head2D.X < 1 || head2D.Y < 1)
                        return;

                   

                    float dist = Vector2.Distance(screenCenter, new Vector2(head2D.X, head2D.Y));
                    if (dist < minDistance)
                    {
                        lock (lockObject)
                        {
                            if (dist < minDistance)
                            {
                                minDistance = dist;
                                target = entity;
                            }
                        }
                    }
                });

                if (target != null)
                {
                    bool fired = InternalMemory.Read<bool>(Core.LocalPlayer + Offsets.pomba, out var isFiring);
                    if (isFiring && fired)
                    {
                        bool gotWeapon = InternalMemory.Read<uint>(Core.LocalPlayer + Offsets.bisteca, out var weaponPtr);
                        if (gotWeapon && weaponPtr != 0)
                        {
                            InternalMemory.Read<Vector3>(weaponPtr + Offsets.arma, out var startPosition);
                            Vector3 direction = target.Head - startPosition;

                            InternalMemory.Write<Vector3>(weaponPtr + Offsets.tiro, direction);
                        }
                    }
                }

                Thread.Sleep(0);
            }
        }

        static Entity GetTarget360()
        {
            Entity target = null;
            float closestDistance = float.MaxValue;

            foreach (var entity in Core.Entities.Values)
            {
                if (!entity.IsKnown || entity.IsDead || (Config.IgnoreKnocked && entity.IsKnocked)) continue;

                Vector3 targetPosition = entity.Head;
                var playerDistance = Vector3.Distance(Core.LocalMainCamera, targetPosition);
                if (playerDistance > Config.siMaxDistance) continue;

                if (playerDistance < closestDistance)
                {
                    closestDistance = playerDistance;
                    target = entity;
                }
            }

            return target;
        }

    }
}

/*
 * static Entity GetTarget(TargetingMode mode)
        {
            if (Core.Width == -1 || Core.Height == -1 || !Core.HaveMatrix) return null;

            switch (mode)
            {
                case TargetingMode.ClosestToCrosshair:
                    return GetTargetByClosestToCrosshair();
                case TargetingMode.Target360:
                    return GetTarget360();
                case TargetingMode.LowestHealth:
                    return GetTargetByLowestHealth();
                case TargetingMode.ClosestToPlayer:
                    return GetTargetByClosestToPlayer();
                default:
                    return GetTargetByClosestToCrosshair();
            }
        }
        static Entity GetTargetByClosestToCrosshair()
        {
            Entity target = null;
            float distance = float.MaxValue;
            var screenCenter = new Vector2(Core.Width / 2f, Core.Height / 2f);

            foreach (var entity in Core.Entities.Values)
            {
                if (!entity.IsKnown || entity.IsDead || (Config.IgnoreKnocked && entity.IsKnocked)) continue;

                Vector3 targetPosition = entity.Head;
                var target2D = W2S.WorldToScreen(Core.CameraMatrix, targetPosition, Core.Width, Core.Height);
                if (target2D.X < 1 || target2D.Y < 1) continue;

                var playerDistance = Vector3.Distance(Core.LocalMainCamera, targetPosition);
                if (playerDistance > Config.AimBotMaxDistance) continue;

                var x = target2D.X - screenCenter.X;
                var y = target2D.Y - screenCenter.Y;
                var crosshairDist = (float)Math.Sqrt(x * x + y * y);

                if (crosshairDist >= distance || crosshairDist > Config.AimFov) continue;

                distance = crosshairDist;
                target = entity;
            }

            return target;
        }


        static Entity GetTargetByLowestHealth()
        {
            Entity target = null;
            float lowestHealth = float.MaxValue;

            foreach (var entity in Core.Entities.Values)
            {
                if (!entity.IsKnown || entity.IsDead || (Config.IgnoreKnocked && entity.IsKnocked)) continue;

                Vector3 targetPosition = entity.Head;
                var target2D = W2S.WorldToScreen(Core.CameraMatrix, targetPosition, Core.Width, Core.Height);
                if (target2D.X < 1 || target2D.Y < 1) continue;

                var playerDistance = Vector3.Distance(Core.LocalMainCamera, targetPosition);
                if (playerDistance > Config.AimBotMaxDistance) continue;


                var screenCenter = new Vector2(Core.Width / 2f, Core.Height / 2f);
                var x = target2D.X - screenCenter.X;
                var y = target2D.Y - screenCenter.Y;
                var crosshairDist = (float)Math.Sqrt(x * x + y * y);
                if (crosshairDist > Config.AimFov) continue;


                if (entity.Health < lowestHealth)
                {
                    lowestHealth = entity.Health;
                    target = entity;
                }
            }

            return target;
        }


        static Entity GetTargetByClosestToPlayer()
        {
            Entity target = null;
            float closestDistance = float.MaxValue;

            foreach (var entity in Core.Entities.Values)
            {
                if (!entity.IsKnown || entity.IsDead || (Config.IgnoreKnocked && entity.IsKnocked)) continue;

                Vector3 targetPosition = entity.Head;
                var target2D = W2S.WorldToScreen(Core.CameraMatrix, targetPosition, Core.Width, Core.Height);
                if (target2D.X < 1 || target2D.Y < 1) continue;


                var playerDistance = Vector3.Distance(Core.LocalMainCamera, targetPosition);
                if (playerDistance > Config.AimBotMaxDistance) continue;


                var screenCenter = new Vector2(Core.Width / 2f, Core.Height / 2f);
                var x = target2D.X - screenCenter.X;
                var y = target2D.Y - screenCenter.Y;
                var crosshairDist = (float)Math.Sqrt(x * x + y * y);
                if (crosshairDist > Config.AimFov) continue;


                if (playerDistance < closestDistance)
                {
                    closestDistance = playerDistance;
                    target = entity;
                }
            }

            return target;
        }
        static Entity GetTarget360()
        {
            Entity target = null;
            float closestDistance = float.MaxValue;

            foreach (var entity in Core.Entities.Values)
            {
                if (!entity.IsKnown || entity.IsDead || (Config.IgnoreKnocked && entity.IsKnocked)) continue;

                Vector3 targetPosition = entity.Head;
                var playerDistance = Vector3.Distance(Core.LocalMainCamera, targetPosition);
                if (playerDistance > Config.AimBotMaxDistance) continue;

                if (playerDistance < closestDistance)
                {
                    closestDistance = playerDistance;
                    target = entity;
                }
            }

            return target;
        }
*/