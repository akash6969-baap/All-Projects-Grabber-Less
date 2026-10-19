using AotForms;
using DarknetHaxor;
using Memory;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace DarknetHaxor
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
                float shakeAmplitude = 0.5f; // power
                float shakeSpeed = 0.15f;    // speed

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

                        // Forward vector
                        Core.UpdateForwardVector();
                        Vector3 forwardNorm = Vector3.Normalize(Core.ForwardVector);

                        // Collect valid targets
                        List<(Entity ent, float dist)> targets = new();

                        foreach (var entity in Core.Entities.Values)
                        {
                            if (entity.IsDead || entity.IsKnocked || !entity.IsKnown)
                                continue;

                            var head2D = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
                            if (head2D.X < 1 || head2D.Y < 1)
                                continue;

                            if (!IsInsideFOV((int)head2D.X, (int)head2D.Y))
                                continue;

                            float dist = DistanceToCrosshair(head2D);
                            targets.Add((entity, dist));
                        }

                        // Sort by closest to crosshair
                        targets.Sort((a, b) => a.dist.CompareTo(b.dist));

                        int count = Math.Min(4, targets.Count);

                        // Shake top 4 players
                        for (int i = 0; i < count; i++)
                        {
                            var entity = targets[i].ent;

                            try
                            {
                                if (!InternalMemory.Read(entity.Address + (uint)Bones.Root, out uint root)) continue;
                                if (!InternalMemory.Read(root + 0x8, out uint t1)) continue;
                                if (!InternalMemory.Read(t1 + 0x8, out uint t2)) continue;
                                if (!InternalMemory.Read(t2 + 0x20, out uint matrix)) continue;
                                if (!InternalMemory.Read(matrix + 0x80, out Vector3 pos)) continue;

                                if (!phaseDict.ContainsKey(entity.Address))
                                    phaseDict[entity.Address] = 0f;

                                phaseDict[entity.Address] += shakeSpeed;

                                float offset = shakeAmplitude * MathF.Sin(phaseDict[entity.Address]);
                                Vector3 finalPos = pos + forwardNorm * offset;

                                InternalMemory.Write(matrix + 0x80, finalPos);
                            }
                            catch { }
                        }

                        await Task.Delay(12, cts.Token); // smooth update
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
            int cx = Core.Width / 2;
            int cy = Core.Height / 2;
            return (x - cx) * (x - cx) + (y - cy) * (y - cy) <= fov * fov;
        }

        private static float DistanceToCrosshair(Vector2 screenPos)
        {
            float cx = Core.Width / 2;
            float cy = Core.Height / 2;
            float dx = screenPos.X - cx;
            float dy = screenPos.Y - cy;
            return MathF.Sqrt(dx * dx + dy * dy);
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
