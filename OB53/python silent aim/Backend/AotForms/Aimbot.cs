using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Windows.Forms;

namespace AotForms
{
    internal static class AimbotV2
    {
        private static Quaternion _smoothRot    = Quaternion.Identity;
        private static Entity     _lockedTarget = null;
        private static int        _lockFrames   = 0;
        private const  int        LOCK_MIN_FRAMES = 3;

        // Head vertical offset in 3D: targets forehead for clean headshot
        private const float HEAD_OFFSET_Y = 0.08f;

        // Deadzone in degrees: when camera is on head, freeze writing to eliminate shake & CPU waste
        private const float ANGLE_DEADZONE_DEG = 0.35f;

        // Maximum angular step per tick in degrees (Anti-Blacklist: prevents abnormal server angle spikes)
        private const float MAX_DEGREES_PER_TICK = 9.0f;

        internal static void Work()
        {
            while (true)
            {
                if (!Config.enableAimBot)
                {
                    Thread.Sleep(15);
                    _Reset();
                    continue;
                }

                // Check Left Click (Firing) or Right Click (Scope/ADS) or configured AimbotKey
                bool isKeyHeld = (WinAPI.GetAsyncKeyState(Config.AimbotKey) & 0x8000) != 0 ||
                                 (WinAPI.GetAsyncKeyState(Keys.RButton) & 0x8000) != 0;

                // Also check in-game firing state
                bool isFiring = false;
                if (Core.LocalPlayer != 0)
                {
                    InternalMemory.Read<bool>(Core.LocalPlayer + Offsets.IsFiring, out isFiring);
                }

                if (!isKeyHeld && !isFiring)
                {
                    Thread.Sleep(10);
                    _Reset();
                    continue;
                }

                if (Core.Width <= 0 || Core.Height <= 0 || !Core.HaveMatrix || Core.LocalPlayer == 0)
                {
                    Thread.Sleep(10);
                    continue;
                }

                Entity best = _GetBestTarget();

                if (best != null && best != _lockedTarget)
                {
                    _lockFrames++;
                    if (_lockFrames < LOCK_MIN_FRAMES && _lockedTarget != null
                        && _lockedTarget.IsKnown && !_lockedTarget.IsDead)
                    {
                        best = _lockedTarget;
                    }
                    else
                    {
                        _lockedTarget = best;
                        _lockFrames   = 0;
                    }
                }
                else if (best != null)
                {
                    _lockFrames = 0;
                }

                if (best == null)
                {
                    _Reset();
                    Thread.Sleep(8);
                    continue;
                }

                // Dual-Assist: redirect in-game aim assist collider directly to head
                if (best.Address != 0 && Offsets.AimbotVisible != 0 && Offsets.LockedAimingCollider != 0)
                {
                    if (InternalMemory.Read<uint>(best.Address + Offsets.AimbotVisible, out uint headCol) && headCol != 0)
                    {
                        InternalMemory.Write(best.Address + Offsets.LockedAimingCollider, headCol);
                    }
                }

                // 3D Forehead Position
                Vector3 targetHead = best.Head + new Vector3(0, HEAD_OFFSET_Y, 0);
                Vector3 diff = targetHead - Core.LocalMainCamera;
                float distXZ = MathF.Sqrt(diff.X * diff.X + diff.Z * diff.Z);

                if (distXZ > 0.01f)
                {
                    // Calculate pure Euler Angles (Yaw and Pitch)
                    float yaw   = MathF.Atan2(diff.X, diff.Z);
                    float pitch = -MathF.Atan2(diff.Y, distXZ);

                    // Clamp pitch between -85° and +85° (prevents sky/ground angle corruption)
                    pitch = Math.Clamp(pitch, -1.48f, 1.48f);

                    // Pure rotation with ZERO Roll (Roll = 0 is mandatory to bypass server angle checks!)
                    Quaternion targetRot = Quaternion.CreateFromYawPitchRoll(yaw, pitch, 0f);

                    if (InternalMemory.Read<Quaternion>(Core.LocalPlayer + Offsets.AimRotation, out var curRot))
                    {
                        // Calculate angular difference in degrees
                        float dot = Math.Abs(Quaternion.Dot(curRot, targetRot));
                        if (dot > 1.0f) dot = 1.0f;
                        float angleDiffDeg = 2.0f * MathF.Acos(dot) * (180.0f / MathF.PI);

                        // DEADZONE: Already on head -> Do NOT write (Zero Jitter, Zero Shake, Zero Memory Spam)
                        if (angleDiffDeg > ANGLE_DEADZONE_DEG)
                        {
                            // Responsive humanized Slerp factor
                            float smooth = Math.Clamp(Config.AimBotSmooth, 1.0f, 15.0f);
                            float speedMultiplier = 1.0f / (smooth * 0.45f);

                            // Clamp angular step to humanly possible limits (prevents abnormal angular velocity flag)
                            float maxAllowedRatio = (angleDiffDeg > 0.001f) ? (MAX_DEGREES_PER_TICK / angleDiffDeg) : 1.0f;
                            float slerpT = Math.Min(speedMultiplier, maxAllowedRatio);

                            _smoothRot = Quaternion.Slerp(curRot, targetRot, Math.Clamp(slerpT, 0.10f, 0.85f));

                            // Safety Check: Validate numbers and normalize before memory write
                            if (!float.IsNaN(_smoothRot.X) && !float.IsNaN(_smoothRot.Y) &&
                                !float.IsNaN(_smoothRot.Z) && !float.IsNaN(_smoothRot.W))
                            {
                                _smoothRot = Quaternion.Normalize(_smoothRot);
                                InternalMemory.Write(Core.LocalPlayer + Offsets.AimRotation, _smoothRot);
                            }
                        }
                    }
                }

                // 6ms update tick rate (~160hz: lightning fast, responsive, zero lag)
                Thread.Sleep(6);
            }
        }

        private static Entity _GetBestTarget()
        {
            Entity best      = null;
            float  bestDist  = float.MaxValue;
            var    center    = new Vector2(Core.Width / 2f, Core.Height / 2f);
            var    snapshot  = Core.Entities.Values.ToArray();

            foreach (var e in snapshot)
            {
                if (e == null || e.Address == 0)                     continue;
                if (!e.IsKnown || e.IsDead)                          continue;
                if (e.IsTeam == Bool3.True)                          continue; // Anti-Ban: never target teammates
                if (Config.IgnoreKnocked && e.IsKnocked)             continue;
                if (Config.AimbotVisible && !e.isVisible)           continue;

                float worldDist = Vector3.Distance(Core.LocalMainCamera, e.Head);
                if (worldDist > Config.AimBotMaxDistance)            continue;

                Vector3 targetHead = e.Head + new Vector3(0, HEAD_OFFSET_Y, 0);
                var screen = W2S.WorldToScreen(Core.CameraMatrix, targetHead, Core.Width, Core.Height);
                if (screen.X <= 0 || screen.Y <= 0 || screen.X >= Core.Width || screen.Y >= Core.Height) continue;

                float dx   = screen.X - center.X;
                float dy   = screen.Y - center.Y;
                float dist = MathF.Sqrt(dx * dx + dy * dy);

                float fov = Config.AimFov > 0 ? Config.AimFov : 160f;
                if (dist > fov) continue;

                if (dist < bestDist)
                {
                    bestDist = dist;
                    best     = e;
                }
            }

            return best;
        }

        private static void _Reset()
        {
            _lockedTarget = null;
            _lockFrames   = 0;
        }
    }
}