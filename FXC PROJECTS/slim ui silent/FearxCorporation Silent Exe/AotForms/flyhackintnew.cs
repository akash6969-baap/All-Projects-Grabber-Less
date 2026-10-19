using AotForms;
using System;
using System.Numerics;
using System.Threading;

namespace Client
{
    internal static class FLY
    {
        private static float _targetY = 0f;
        private static float _currentY = 0f;
        private static bool _yCaptured = false;
        private static uint _lastMatrixPtr = 0;

        private static readonly float _smoothSpeed = 0.12f;
        private static readonly float _minElevatedDiff = 0.5f;

        private static Thread flyThread;
        private static CancellationTokenSource cts;

        internal static void Start()
        {
            if (flyThread != null && flyThread.IsAlive)
                return;

            cts = new CancellationTokenSource();
            flyThread = new Thread(() => Work(cts.Token))
            {
                IsBackground = true
            };
            flyThread.Start();
        }

        internal static void Stop()
        {
            if (cts != null)
            {
                cts.Cancel();
                flyThread?.Join();
                flyThread = null;
                cts.Dispose();
                cts = null;
                Reset();
            }
        }

        private static void Work(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (!Config.flyhackEnabled)
                {
                    Reset();
                    Thread.Sleep(20);
                    continue;
                }

                if (Core.Width == -1 || Core.Height == -1 || !Core.HaveMatrix)
                {
                    Thread.Sleep(20);
                    continue;
                }

                try
                {
                    ProcessClimbUp();
                    Thread.Sleep(8);
                }
                catch
                {
                    Thread.Sleep(15);
                }
            }
        }

        private static void Reset()
        {
            _yCaptured = false;
            _lastMatrixPtr = 0;
            _targetY = 0f;
            _currentY = 0f;
        }

        private static void ProcessClimbUp()
        {
            if (!_yCaptured)
            {
                if (!InternalMemory.Read(Core.LocalPlayer + (uint)Bones.Root, out uint rootBonePtr)) return;
                if (!Transform.GetNodePosition(rootBonePtr, out Vector3 currentPos)) return;
                if (!InternalMemory.Read(rootBonePtr + 0x8, out uint transformValue)) return;
                if (!InternalMemory.Read(transformValue + 0x8, out uint transformObjPtr)) return;
                if (!InternalMemory.Read(transformObjPtr + 0x20, out uint matrixPtr)) return;

                float groundY = GetGroundHeightAt(currentPos);

                if (currentPos.Y - groundY < _minElevatedDiff)
                    return;

                _targetY = currentPos.Y + 1.200f;
                _currentY = currentPos.Y;
                _lastMatrixPtr = matrixPtr;
                _yCaptured = true;
            }

            if (!_yCaptured || _lastMatrixPtr == 0) return;

            if (!InternalMemory.Read(Core.LocalPlayer + (uint)Bones.Root, out uint rootBone)) return;
            if (!Transform.GetNodePosition(rootBone, out Vector3 pos)) return;

            if (Math.Abs(_currentY - _targetY) > 0.01f)
                _currentY = Lerp(_currentY, _targetY, _smoothSpeed);
            else
                _currentY = _targetY;

            Vector3 finalPos = new Vector3(pos.X, _currentY, pos.Z);
            InternalMemory.Write(_lastMatrixPtr + 0x80, finalPos);
        }

        private static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        private static float GetGroundHeightAt(Vector3 pos)
        {
            return 0f;
        }

        public static string GetStatus()
        {
            return _yCaptured ? $"Active - Climbing/Holding at +1.5m ({_targetY:F2})" : "Inactive";
        }
    }
}
