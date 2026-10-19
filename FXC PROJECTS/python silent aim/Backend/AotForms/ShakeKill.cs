
using Memory;
using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using AotForms;

namespace AotForms
{
    internal static class ShakeKill
    {
        private static Task upTask;
        private static CancellationTokenSource cts = new();
        private static bool isRunning = false;

        // Per-entity sine phase
        private static Dictionary<uint, float> phaseDict = new();

        internal static void Work()
        {
            if (isRunning) return;
            isRunning = true;

            upTask = Task.Run(async () =>
            {
                float shakeAmplitude = 0.5f;   // kitna aage-peeche (meters)
                float shakeSpeed = 0.1f;       // speed of shake (higher = faster)

                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        if (!Config.shakekill)
                        {
                            await Task.Delay(1, cts.Token);
                            continue;
                        }

                        if (Core.Width == -1 || Core.Height == -1 || !Core.HaveMatrix)
                        {
                            await Task.Delay(1, cts.Token);
                            continue;
                        }

                        if (!InternalMemory.Read(Core.LocalPlayer + Offsets.IS_FIRING, out bool isFiring) || !isFiring)
                        {
                            await Task.Delay(1, cts.Token);
                            continue;
                        }

                        // Forward vector update karo
                        Core.UpdateForwardVector();
                        Vector3 forwardNorm = Vector3.Normalize(Core.ForwardVector);

                        Entity closestEntity = null;
                        float closestDistanceToCrosshair = float.MaxValue;

                        foreach (var entity in Core.Entities.Values)
                        {
                            if (entity.IsDead || entity.IsKnocked || !entity.IsKnown)
                                continue;

                            var head2D = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
                            if (head2D.X < 1 || head2D.Y < 1) continue;
                            if (!IsInsideFOV((int)head2D.X, (int)head2D.Y)) continue;

                            float distToCrosshair = DistanceToCrosshair(head2D);
                            if (distToCrosshair < closestDistanceToCrosshair)
                            {
                                closestDistanceToCrosshair = distToCrosshair;
                                closestEntity = entity;
                            }
                        }

                        if (closestEntity != null)
                        {
                            try
                            {
                                if (!InternalMemory.Read(closestEntity.Address + (uint)Bones.Root, out uint enemyRootBonePtr)) continue;
                                if (!InternalMemory.Read(enemyRootBonePtr + 0x8, out uint enemyTransformValue)) continue;
                                if (!InternalMemory.Read(enemyTransformValue + 0x8, out uint enemyTransformObjPtr)) continue;
                                if (!InternalMemory.Read(enemyTransformObjPtr + 0x20, out uint enemyMatrixValue)) continue;
                                if (!InternalMemory.Read(enemyMatrixValue + 0x80, out Vector3 enemyPos)) continue;

                                // Init phase
                                if (!phaseDict.ContainsKey(closestEntity.Address))
                                    phaseDict[closestEntity.Address] = 0f;

                                // Update phase
                                phaseDict[closestEntity.Address] += shakeSpeed;

                                // Forward-direction offset (sin wave)
                                float offset = shakeAmplitude * MathF.Sin(phaseDict[closestEntity.Address]);

                                // Move enemy along player's forward direction
                                Vector3 finalPos = enemyPos + forwardNorm * offset;

                                InternalMemory.Write(enemyMatrixValue + 0x80, finalPos);
                            }
                            catch { }
                        }

                        await Task.Delay(15, cts.Token); // ~66 updates/sec
                    }
                    catch
                    {
                        await Task.Delay(1, cts.Token);
                    }
                }
            }, cts.Token);
        }

        private static bool IsInsideFOV(int x, int y)
        {
            int fov = 1500;
            int centerX = Core.Width / 2;
            int centerY = Core.Height / 2;
            return (x - centerX) * (x - centerX) + (y - centerY) * (y - centerY) <= fov * fov;
        }

        private static float DistanceToCrosshair(Vector2 screenPos)
        {
            float centerX = Core.Width / 2;
            float centerY = Core.Height / 2;
            float dx = screenPos.X - centerX;
            float dy = screenPos.Y - centerY;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        internal static void Stop()
        {
            if (!isRunning) return;
            cts.Cancel();
            cts = new CancellationTokenSource();
            phaseDict.Clear();
            isRunning = false;
        }
    }
}
