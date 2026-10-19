using System;
using System.Numerics;
using System.Threading;

namespace AotForms
{
    internal class KCBRUTALSILENT
    {
        internal static void Work()
        {
            while (true)
            {
                if (!Config.SilentAim)
                {
                    Thread.SpinWait(0);
                    continue;
                }

                Entity? target = null;
                float distance = float.MaxValue;

                if (Core.Width == -1 || Core.Height == -1 || !Core.HaveMatrix)
                {
                    Thread.SpinWait(1);
                    continue;
                }

                var screenCenter = new Vector2(Core.Width / 2f, Core.Height / 2f);
                foreach (var entity in Core.Entities.Values)
                {
                    if (!entity.IsKnown || entity.IsDead || (Config.IgnoreKnocked && entity.IsKnocked))
                        continue;

                    

                    var head2D = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
                    var playerDistance = Vector2.Distance(screenCenter, new Vector2(head2D.X, head2D.Y));
                    if (playerDistance < distance)
                    {
                        distance = playerDistance;
                        target = entity;
                    }
                }

                if (target != null)
                {
                    if (InternalMemory.Read<bool>(Core.LocalPlayer + Offsets.sAim1, out bool isShooting) && isShooting)
                    {
                        if (InternalMemory.Read<uint>(Core.LocalPlayer + Offsets.sAim2, out uint weaponData) && weaponData != 0)
                        {
                            Vector3 adjustedHead = target.Head + new Vector3(0, 0.1f, 0);
                            if (InternalMemory.Read<Vector3>(weaponData + Offsets.sAim3, out Vector3 startPos))
                            {
                                InternalMemory.Write<Vector3>(weaponData + Offsets.sAim4, adjustedHead - startPos);
                            }
                        }
                    }
                }

                Thread.SpinWait(1);
            }
        }
    }
}
