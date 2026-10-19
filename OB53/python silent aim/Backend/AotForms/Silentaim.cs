using System;
using System.Numerics;
using System.Threading;

namespace AotForms
{
    internal class SilentAim
    {
        internal static void Work()
        {
            while (true)
            {
                if (!Config.SilentAim)
                {
                    Thread.SpinWait(1);
                    continue;
                }

                if (Core.Width == -1 || Core.Height == -1 || !Core.HaveMatrix || Core.LocalPlayer == 0)
                {
                    Thread.SpinWait(1);
                    continue;
                }

                Entity target = null;
                float minDistance = float.MaxValue;
                var screenCenter = new Vector2(Core.Width / 2f, Core.Height / 2f);
                float fovLimit = Config.AimFov;

                foreach (var entity in Core.Entities.Values)
                {
                    if (!entity.IsKnown || entity.IsDead) continue;
                    if (Config.IgnoreKnocked && entity.IsKnocked) continue;

                    Vector3 targetPos = entity.Head;
                    var screen2D = W2S.WorldToScreen(Core.CameraMatrix, targetPos, Core.Width, Core.Height);
                    if (screen2D.X < 1 || screen2D.Y < 1) continue;

                    float dist = Vector2.Distance(screenCenter, new Vector2(screen2D.X, screen2D.Y));
                    if (dist > fovLimit) continue;

                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        target = entity;
                    }
                }

                if (target != null)
                {
                    if (InternalMemory.Read<bool>(Core.LocalPlayer + Offsets.sAim1, out bool isShooting) && isShooting)
                    {
                        if (InternalMemory.Read<uint>(Core.LocalPlayer + Offsets.sAim2, out uint weaponData) && weaponData != 0)
                        {
                            Vector3 aimPos = target.Head + new Vector3(0, 0.1f, 0);

                            if (InternalMemory.Read<Vector3>(weaponData + Offsets.sAim3, out Vector3 startPos))
                            {
                                Vector3 aimDir = aimPos - startPos;
                                if (!float.IsNaN(aimDir.X) && !float.IsNaN(aimDir.Y) && !float.IsNaN(aimDir.Z) &&
                                    !float.IsInfinity(aimDir.X) && !float.IsInfinity(aimDir.Y) && !float.IsInfinity(aimDir.Z))
                                {
                                    float dist = aimDir.Length();
                                    if (dist >= 0.1f && dist <= 1000.0f)
                                    {
                                        InternalMemory.Write<Vector3>(weaponData + Offsets.sAim4, aimDir);
                                    }
                                }
                            }
                        }
                    }
                }

                Thread.SpinWait(1);
            }
        }
    }
}