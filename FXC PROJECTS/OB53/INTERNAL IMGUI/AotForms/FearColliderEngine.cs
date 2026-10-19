using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;

namespace AotForms
{
    internal static class FearColliderEngine
    {
        private static Thread _workThread = null;
        private static bool _isRunning = false;
        
        private static readonly Dictionary<nuint, uint> _patchedMemory = new();
        private static readonly object _lockObj = new();

        private static DateTime _lastClickStart = DateTime.MinValue;
        private static bool _isClickHeld = false;

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        internal static void Start()
        {
            if (_isRunning) return;

            _isRunning = true;
            _workThread = new Thread(_ProcessLoop)
            {
                IsBackground = true,
                Name = "FearColliderThread"
            };
            _workThread.Start();
        }

        internal static void Stop()
        {
            _isRunning = false;
            try
            {
                _workThread?.Join(300);
            }
            catch { }
            RestorePatchedMemory();
        }

        private static void _ProcessLoop()
        {
            while (_isRunning)
            {
                try
                {
                    if (!Config.FearCollider || Core.Width < 1 || Core.Height < 1 || !Core.HaveMatrix)
                    {
                        RestorePatchedMemory();
                        Thread.Sleep(10);
                        continue;
                    }

                    bool isKeyDown = (GetAsyncKeyState((int)Config.AimbotKey) & 0x8000) != 0;

                    if (isKeyDown)
                    {
                        if (!_isClickHeld)
                        {
                            _lastClickStart = DateTime.Now;
                            _isClickHeld = true;
                        }

                        TimeSpan heldTime = DateTime.Now - _lastClickStart;
                        if (heldTime.TotalSeconds >= 4)
                        {
                            RestorePatchedMemory();
                            Thread.Sleep(5);
                            continue;
                        }

                        Entity target = FindBestTarget();
                        if (target != null && target.Address != 0)
                        {
                            uint headCollider = 0;
                            if (InternalMemory.Read<uint>(target.Address + Offsets.AimbotVisible, out headCollider) && headCollider != 0)
                            {
                                nuint patchAddr = target.Address + Offsets.LockedAimingCollider;

                                lock (_lockObj)
                                {
                                    if (!_patchedMemory.ContainsKey(patchAddr))
                                    {
                                        if (InternalMemory.Read<uint>(patchAddr, out var originalVal))
                                        {
                                            _patchedMemory[patchAddr] = originalVal;
                                        }
                                    }
                                }

                                InternalMemory.Write(patchAddr, headCollider);
                            }
                        }
                    }
                    else
                    {
                        if (_isClickHeld)
                        {
                            _isClickHeld = false;
                            _lastClickStart = DateTime.MinValue;
                            RestorePatchedMemory();
                        }
                    }

                    Thread.Sleep(1);
                }
                catch (Exception)
                {
                    Thread.Sleep(50);
                }
            }

            RestorePatchedMemory();
        }

        public static void RestorePatchedMemory()
        {
            lock (_lockObj)
            {
                if (_patchedMemory.Count == 0) return;

                foreach (var kvp in _patchedMemory)
                {
                    try
                    {
                        InternalMemory.Write(kvp.Key, kvp.Value);
                    }
                    catch { }
                }
                _patchedMemory.Clear();
            }
        }

        private static Entity FindBestTarget()
        {
            try
            {
                Entity bestTarget = null;
                float closestDistance = float.MaxValue;
                var screenCenter = new Vector2(Core.Width / 2f, Core.Height / 2f);
                var snapshot = Core.Entities.Values.ToArray();

                foreach (var entity in snapshot)
                {
                    if (entity == null || !entity.IsKnown || entity.IsDead) continue;
                    if (entity.IsTeam == Bool3.True) continue;
                    if (Config.IgnoreKnocked && entity.IsKnocked) continue;
                    if (Config.AimbotVisible && !entity.isVisible) continue;

                    var head2D = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
                    if (head2D.X < 1 || head2D.Y < 1) continue;

                    float dist3D = Vector3.Distance(Core.LocalMainCamera, entity.Head);
                    if (dist3D > Config.AimBotMaxDistance) continue;

                    float dist2D = Vector2.Distance(screenCenter, head2D);
                    float maxFov = Config.FOVEnabled ? Config.AimBotFov : 400;

                    if (dist2D <= maxFov && dist2D < closestDistance)
                    {
                        closestDistance = dist2D;
                        bestTarget = entity;
                    }
                }

                return bestTarget;
            }
            catch
            {
                return null;
            }
        }
    }
}
