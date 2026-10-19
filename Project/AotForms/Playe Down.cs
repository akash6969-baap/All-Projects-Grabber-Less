using AotForms;
using AotForms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AotForms
{
    internal static class DownPlayer
    {
        private static Thread aimbotThread;
        private static CancellationTokenSource cancellationTokenSource;
        private static Task DownPlayerTask;




        private static Task downTask;
        private static CancellationTokenSource cts = new();
        private static bool isRunning = true;

        private static Dictionary<ulong, float> originalYDict = new();
        private static bool forceRefreshY = true;

        public static bool isFiring = false;



        internal static void Start()
        {
            cancellationTokenSource = new CancellationTokenSource();
            aimbotThread = new Thread(() => Work(cancellationTokenSource.Token))
            {
                IsBackground = true
            };
            aimbotThread.Start();
        }

        internal static void Stop()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = null;
        }
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool AllocConsole();
        internal static void Work(CancellationToken cancellationToken)
        {
            bool wasInMatch = false;

            DownPlayerTask = Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    //if (!wasInMatch)
                    //{
                    //    AllocConsole();
                    //    wasInMatch = true;
                    //}
                    //if (!Config.DownPlayerX)
                    //{
                    //    await Task.Delay(1, cancellationTokenSource.Token);
                    //    continue;
                    //}
                    if (Core.Width == -1 || Core.Height == -1 || !Core.HaveMatrix)
                    {
                        Thread.Sleep(1);
                        continue;
                    }
                    RefreshOriginalY();
                    //if ((WinAPI.GetAsyncKeyState(Config.AimBotKeys) & 0x8000) == 0)
                    //{

                    //    isFiring = false;
                    //    await Task.Delay(5, cancellationTokenSource.Token);
                    //    continue;
                    //}

                    if (!InternalMemory.Read(Core.LocalPlayer + 0x474, out bool tempFiring) || !tempFiring)
                    {
                        // Console.WriteLine("Not Pressed");
                        isFiring = false;
                        await Task.Delay(5, cancellationTokenSource.Token);
                        continue;
                    }


                    isFiring = true;



                    Entity target = FindBestTarget();
                    if (target != null)
                    {
                        AimAtTarget(target);

                    }


                    if (forceRefreshY)
                        forceRefreshY = false;

                    await Task.Delay(1, cancellationTokenSource.Token);

                }
            }, cancellationTokenSource.Token);

        }


        private static void AimAtTarget(Entity target)
        {
            try
            {
                if (!InternalMemory.Read(target.Address + (uint)Bones.Root, out uint enemyRootBonePtr)) return;
                if (!InternalMemory.Read(enemyRootBonePtr + 0x8, out uint enemyTransformValue)) return;
                if (!InternalMemory.Read(enemyTransformValue + 0x8, out uint enemyTransformObjPtr)) return;
                if (!InternalMemory.Read(enemyTransformObjPtr + 0x20, out uint enemyMatrixValue)) return;
                if (!InternalMemory.Read(enemyMatrixValue + 0x80, out Vector3 enemyPos)) return;

                if (forceRefreshY || !originalYDict.ContainsKey(target.Address))
                {
                    originalYDict[target.Address] = enemyPos.Y;
                }

                float originalY = originalYDict[target.Address];
                float desiredY = originalY + Config.CameraD;

                if (Math.Abs(enemyPos.Y - desiredY) > 0.2f)
                {
                    enemyPos.Y = desiredY;
                    InternalMemory.Write(enemyMatrixValue + 0x80, enemyPos);
                }
            }
            catch { }
        }



        private static bool IsInsideFOV(int x, int y)
        {
            int fov = 1500;
            int centerX = Core.Width / 2;
            int centerY = Core.Height / 2;
            return (x - centerX) * (x - centerX) + (y - centerY) * (y - centerY) <= fov * fov;
        }

        internal static void RefreshOriginalY()
        {
            if (isFiring) return;
            originalYDict.Clear();
            forceRefreshY = true;
        }

        private static Entity FindBestTarget()
        {

            Entity bestTarget = null;
            float closestDistance = float.MaxValue;



            Vector2 screenCenter = new Vector2(Core.Width / 2f, Core.Height / 2f);

            foreach (var entity in Core.Entities.Values)
            {

                if (entity.IsDead) continue;

                if (Config.IgnoreKnocked)
                {
                    if (entity.IsKnocked) continue;
                }
                var head2D = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
                if (head2D.X < 1 || head2D.Y < 1) continue;

                float playerDistance = Vector3.Distance(Core.LocalMainCamera, entity.Head);
                if (playerDistance > 200) continue;

                var crosshairDistance = Vector2.Distance(screenCenter, head2D);


                if (crosshairDistance > 500) continue;

                if (crosshairDistance < closestDistance)
                {
                    closestDistance = crosshairDistance;
                    bestTarget = entity;
                }


            }
            return bestTarget;
        }
    }
}