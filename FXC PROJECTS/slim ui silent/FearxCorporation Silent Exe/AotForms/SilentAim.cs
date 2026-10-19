using AotForms;
using System;
using System.Numerics;
using System.Threading;
using System.Windows.Forms;

namespace Client
{
    internal class SILENT
    {
        private static System.Collections.Generic.Dictionary<uint, Vector3> lastPos = new();
        private static System.Collections.Generic.Dictionary<uint, DateTime> lastTime = new();

        internal static void Work()
        {
            while (true)
            {
                if (!Config.SilentAim)
                {
                    Thread.SpinWait(1);
                    continue;
                }

                Entity target = null;
                float distance = float.MaxValue;

                if (Core.Width == -1 || Core.Height == -1 || !Core.HaveMatrix)
                {
                    Thread.SpinWait(1);
                    continue;
                }

                var screenCenter = new Vector2(Core.Width / 2f, Core.Height / 2f);
                foreach (var entity in Core.Entities.Values)
                {
                    if (!entity.IsKnown || entity.IsDead || (Config.IgnoreKnocked && entity.IsKnocked)) continue;
                    var head2D = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
                    Vector2 headScreenPos2 = new Vector2(head2D.X, head2D.Y);
                    var playerDistance = Vector2.Distance(screenCenter, headScreenPos2);
                    if (playerDistance < distance)
                    {
                        distance = playerDistance;
                        target = entity;
                    }
                }

                if (target != null)
                {
                    var isShooting = InternalMemory.Read<bool>(Core.LocalPlayer + Offsets.isFiring, out var readSuccess);
                    if (readSuccess && isShooting)
                    {
                        var weaponData = InternalMemory.Read<uint>(Core.LocalPlayer + Offsets.LastAimingInfoFromWeapon, out var weaponSuccess);
                        if (weaponData && weaponSuccess != 0)
                        {
                            Vector3 velocity = Vector3.Zero;
                            uint tid = target.Address;

                            if (lastPos.ContainsKey(tid) && lastTime.ContainsKey(tid))
                            {
                                float dt = (float)(DateTime.Now - lastTime[tid]).TotalSeconds;
                                if (dt > 0.04f)
                                {
                                    velocity = (target.Head - lastPos[tid]) / dt;
                                    lastPos[tid] = target.Head;
                                    lastTime[tid] = DateTime.Now;
                                }
                            }
                            else
                            {
                                lastPos[tid] = target.Head;
                                lastTime[tid] = DateTime.Now;
                            }

                            Vector3 TargetHead = target.Head + new Vector3(0, 0.10f, 0) + (velocity * 0.045f);

                            InternalMemory.Read<Vector3>(weaponSuccess + Offsets.StartPosition, out var startPos);

                            Vector3 diff = TargetHead - startPos;
                            float mag = diff.Length();
                            Vector3 direction = mag > 0.001f ? Vector3.Normalize(diff) : Vector3.Zero;

                            InternalMemory.Write<Vector3>(weaponSuccess + Offsets.RayDir, direction);
                        }
                    }
                }
                Thread.SpinWait(1);
            }
        }
    }
}
