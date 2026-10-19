using AotForms;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;

namespace AotForms
{
    internal static class EnemyPull360
    {
        private static Thread pullThread;
        private static CancellationTokenSource cts;
        private static bool isRunning = false;

        private static readonly Dictionary<uint, Vector3> originalPositions = new();
        private static Entity currentTarget = null;

        private static uint    _lockedEntityId;
        private static Vector3 _lockedPullPos;
        private static bool    _hasLockedPos;
        private static int     _lockRefreshCounter = 0;
        private const  int     LockRefreshEvery    = 5; // recompute every 5 ticks — original stable feel

        // Original values (proven working — kick was from Y-explosion bug, not these limits)
        // + Stale lock removed = now follows camera properly every tick
        private const float MaxSide         = 4.7f;   // lateral
        private const float MaxForward      = 5.0f;   // aage piche kam ← (tha 20f, kam kiya)
        private const float MaxUp           = 0.55f;  // upar niche
        private const float PullYOffset     = 0.010f;
        private const float MinProjDistance = 2.5f;
        private const int   LockWritesPerTick = 4;

        internal static void Start()
        {
            if (isRunning) return;

            cts = new CancellationTokenSource();
            pullThread = new Thread(() => Work(cts.Token))
            {
                IsBackground = true,
                Priority = ThreadPriority.AboveNormal
            };
            pullThread.Start();
            isRunning = true;
        }

        internal static void Stop()
        {
            if (!isRunning) return;

            cts?.Cancel();
            ClearLockState();
            RestoreAllPositions();
            isRunning = false;
        }

        private static void Work(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                bool pullingThisFrame = false;
                try
                {
                    if (!Config.EnemyPullEnabled)
                    {
                        if (originalPositions.Count > 0 || _hasLockedPos)
                            RestoreAllPositions();
                        currentTarget = null;
                        Thread.Sleep(1);
                        continue;
                    }

                    if (Core.Width == -1 || Core.Height == -1 || !Core.HaveMatrix)
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    if (EnemyPullHelpers.IsLocalFiring())
                    {
                        pullingThisFrame = true;

                        if (currentTarget == null || !IsTargetStillValid(currentTarget))
                            currentTarget = FindBestTarget();

                        if (currentTarget != null && IsTargetStillValid(currentTarget))
                            ApplyStableLock(currentTarget);
                        else
                        {
                            currentTarget = null;
                            ClearLockState();
                        }
                    }
                    else
                    {
                        RestoreAllPositions();
                        currentTarget = null;
                    }
                }
                catch { }

                int delay = Config.EnemyPullTickMs > 0 ? Config.EnemyPullTickMs : 2;
                if (pullingThisFrame)
                    delay = 0;
                if (delay > 0)
                    Thread.Sleep(delay);
            }
        }

        private static bool IsTargetStillValid(Entity entity)
        {
            if (entity == null || entity.IsDead) return false;
            if (!entity.IsKnown) return false;
            if (Config.IgnoreKnocked && entity.IsKnocked) return false;
            if (!Core.Entities.ContainsKey(entity.Address)) return false;

            float dist3D = Vector3.Distance(Core.LocalMainCamera, entity.Head);
            if (dist3D > Config.EnemyPullMaxDistance) return false;

            return true;
        }

        private static Entity FindBestTarget()
        {
            Entity bestTarget = null;
            float closestDist = float.MaxValue;
            var screenCenter = new Vector2(Core.Width / 2f, Core.Height / 2f);

            // ✅ Thread-safe snapshot — prevents crash on concurrent entity update
            var snapshot = Core.Entities.Values.ToArray();

            foreach (var entity in snapshot)
            {
                if (!entity.IsKnown || entity.IsDead) continue;
                if (entity.IsTeam == Bool3.True) continue; // ✅ Skip teammates
                if (Config.IgnoreKnocked && entity.IsKnocked) continue;

                var head2D = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
                if (head2D.X < 1 || head2D.Y < 1) continue;

                float dist3D = Vector3.Distance(Core.LocalMainCamera, entity.Head);
                if (dist3D > Config.EnemyPullMaxDistance) continue;

                float crosshairDist = Vector2.Distance(screenCenter, head2D);
                if (crosshairDist > EnemyPullHelpers.GetPullFovRadius()) continue;

                if (crosshairDist < closestDist)
                {
                    closestDist = crosshairDist;
                    bestTarget = entity;
                }
            }
            return bestTarget;
        }

        /// <summary>
        /// Micro-correction pull — moves enemy a small amount (≤2m) onto the camera ray.
        /// Small correction = within server hit-validation tolerance = real damage registers.
        /// Large correction = server rejects hit = fake damage + game kick.
        /// </summary>
        private static Vector3 ComputeLockPosition(Vector3 currentPos, Vector3 originalPos)
        {
            Vector3 camPos = Core.LocalMainCamera;

            Vector3 fireDir = new Vector3(
                Core.CameraMatrix.M13,
                Core.CameraMatrix.M23,
                Core.CameraMatrix.M33
            );
            if (fireDir.LengthSquared() < 1e-8f)
                return currentPos;
            fireDir = Vector3.Normalize(fireDir);

            // Dot-product projection — safe, no Y explosion when camera is tilted
            Vector3 toEnemy   = currentPos - camPos;
            float projLength  = Vector3.Dot(toEnemy, fireDir);
            if (projLength < MinProjDistance)
                projLength = MinProjDistance;

            Vector3 idealPos  = camPos + fireDir * projLength;
            idealPos.Y       += PullYOffset;

            // Clamp XZ displacement — keeps correction within server tolerance
            Vector3 delta     = idealPos - originalPos;
            Vector3 forwardXZ = new Vector3(fireDir.X, 0f, fireDir.Z);

            if (forwardXZ.LengthSquared() > 1e-8f)
            {
                forwardXZ         = Vector3.Normalize(forwardXZ);
                Vector3 sideXZ    = new Vector3(-forwardXZ.Z, 0f, forwardXZ.X);

                float fwd  = Math.Clamp(Vector3.Dot(delta, forwardXZ), -MaxForward, MaxForward);
                float side = Math.Clamp(Vector3.Dot(delta, sideXZ),   -MaxSide,    MaxSide);

                idealPos.X = originalPos.X + forwardXZ.X * fwd + sideXZ.X * side;
                idealPos.Z = originalPos.Z + forwardXZ.Z * fwd + sideXZ.Z * side;
            }
            else
            {
                Vector2 flatDelta = new Vector2(delta.X, delta.Z);
                if (flatDelta.Length() > MaxSide)
                    flatDelta  = Vector2.Normalize(flatDelta) * MaxSide;
                idealPos.X = originalPos.X + flatDelta.X;
                idealPos.Z = originalPos.Z + flatDelta.Y;
            }

            // Vertical clamp — small, safe
            float yDelta  = Math.Clamp(idealPos.Y - originalPos.Y, -MaxUp, MaxUp);
            idealPos.Y    = originalPos.Y + yDelta;

            // No Lerp needed — corrections are small enough to snap directly
            return idealPos;
        }


        private static void ApplyStableLock(Entity entity)
        {
            try
            {
                if (!InternalMemory.Read(entity.Address + (uint)Bones.Root, out uint rootBonePtr)) return;
                if (!InternalMemory.Read(rootBonePtr + 0x8, out uint transformValue)) return;
                if (!InternalMemory.Read(transformValue + 0x8, out uint transformObjPtr)) return;
                if (!InternalMemory.Read(transformObjPtr + 0x20, out uint matrixPtr)) return;

                if (!Transform.GetNodePosition(rootBonePtr, out var currentPos)) return;

                // Store original (pre-pull) position ONCE per target
                if (!originalPositions.ContainsKey(entity.Address))
                    originalPositions[entity.Address] = currentPos;

                Vector3 originalPos = originalPositions[entity.Address];

                // Recompute only when target changes OR every N ticks
                // This gives original stable "snapped" feel while still following camera over time
                bool targetChanged = !_hasLockedPos || _lockedEntityId != entity.Address;
                if (targetChanged || _lockRefreshCounter >= LockRefreshEvery)
                {
                    _lockedPullPos      = ComputeLockPosition(currentPos, originalPos);
                    _lockedEntityId     = entity.Address;
                    _hasLockedPos       = true;
                    _lockRefreshCounter = 0;
                }
                else
                {
                    _lockRefreshCounter++;
                }

                ulong writeAddr = (ulong)(matrixPtr + 0x60);
                for (int i = 0; i < LockWritesPerTick; i++)
                    InternalMemory.Write(writeAddr, _lockedPullPos);
            }
            catch { }
        }

        private static void ClearLockState()
        {
            _hasLockedPos       = false;
            _lockedEntityId     = 0;
            _lockedPullPos      = Vector3.Zero;
            _lockRefreshCounter = 0;
        }

        private static void RestoreAllPositions()
        {
            foreach (var entry in originalPositions)
            {
                try
                {
                    if (!InternalMemory.Read(entry.Key + (uint)Bones.Root, out uint rootBonePtr)) continue;
                    if (!InternalMemory.Read(rootBonePtr + 0x8, out uint transformValue)) continue;
                    if (!InternalMemory.Read(transformValue + 0x8, out uint transformObjPtr)) continue;
                    if (!InternalMemory.Read(transformObjPtr + 0x20, out uint matrixPtr)) continue;

                    InternalMemory.Write((ulong)(matrixPtr + 0x60), entry.Value);
                }
                catch { }
            }
            originalPositions.Clear();
            ClearLockState();
        }
    }
}