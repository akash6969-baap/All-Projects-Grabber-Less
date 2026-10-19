using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AotForms
{
    internal static class TeleKillChain
    {
        private static Task task;
        private static CancellationTokenSource cts = new();
        private static bool isRunning = false;

        internal static void Work()
        {
            if (isRunning) return;
            isRunning = true;

            task = Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    if (!Config.Tele) { await Task.Delay(1, cts.Token); continue; }

                    Entity nearest = null;
                    float minDistance = float.MaxValue;

                    foreach (var entity in Core.Entities.Values)
                    {
                        if (!entity.IsKnown || entity.IsDead || (Config.IgnoreKnocked && entity.IsKnocked))
                            continue;

                        float dist = Vector3.Distance(Core.LocalMainCamera, entity.Head);
                        if (dist < minDistance && dist < Config.MaxEngageDistance)
                        {
                            nearest = entity;
                            minDistance = dist;
                        }
                    }

                    if (nearest == null)
                    {
                        await Task.Delay(1, cts.Token);
                        continue;
                    }
                    // READ enemy position
                    var enemyRootBone = InternalMemory.Read<uint>(nearest.Address + (uint)Bones.Root, out var enemyRootBonePtr);
                    Transform.GetNodePosition(enemyRootBonePtr, out Vector3 enemyRootPos); // <-- Fixed here

                    // TELEPORT YOU behind the enemy
                    var yourRootBone = InternalMemory.Read<uint>(Core.LocalPlayer + (uint)Bones.Root, out var yourRootBonePtr);
                    var yourTransform = InternalMemory.Read<uint>(yourRootBonePtr + 0x8, out var yourTransformVal);
                    var yourObj = InternalMemory.Read<uint>(yourTransformVal + 0x8, out var yourObjPtr);
                    var yourMatrix = InternalMemory.Read<uint>(yourObjPtr + 0x20, out var yourMatrixVal);

                    Vector3 behindEnemy = enemyRootPos;
                    behindEnemy.Z -= Config.BehindOffsetZ;
                    behindEnemy.X -= Config.BehindOffsetX;
                    behindEnemy.Y += Config.test;

                    InternalMemory.Write<Vector3>(yourMatrixVal + 0x80, behindEnemy);


                    // AIM at enemy's head (optional silent)
                    //if (Config.EnableAim)
                    //{
                    //    Vector3 targetHead = nearest.Head;
                    //    AimAt(targetHead);
                    //}

                    //// AUTO-SHOOT or MELEE (mock damage or fire real weapon)
                    //if (Config.Trigger)
                    //{
                    //    TriggerBot.Fire(); // Your function that simulates fire
                    //}

                    //// Optional: Write fake health or damage if spoofing
                    //if (Config.FakeKill)
                    //{
                    //    InternalMemory.Write<int>(nearest.Address + Offsets.Health, 0); // set health to zero
                    //}

                    await Task.Delay(Config.TeleDelay, cts.Token);
                }
            }, cts.Token);
        }

        internal static void Stop()
        {
            if (!isRunning) return;
            cts.Cancel();
            isRunning = false;
        }

        //private static void AimAt(Vector3 target)
        //{
        //    Vector3 direction = Vector3.Normalize(target - Core.LocalMainCamera);
        //    float yaw = (float)Math.Atan2(direction.X, direction.Z) * (180f / (float)Math.PI);
        //    float pitch = -(float)Math.Asin(direction.Y) * (180f / (float)Math.PI);

        //    Core.LocalViewAngles.Yaw = yaw;
        //    Core.LocalViewAngles.Pitch = pitch;
        //}
    }
}
