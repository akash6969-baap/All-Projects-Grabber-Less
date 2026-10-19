using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AotForms
{
    internal static class Core
    {

        private static Matrix4x4 _cameraMatrix;
        internal static Vector3 CameraForward => _cameraForward;
        internal static Vector3 CameraRight => _cameraRight;
        private static Vector3 _cameraForward;
        private static Vector3 _cameraRight;
        public static uint GameInstance;
        internal static IntPtr Handle;
        internal static int Width = -1;
        internal static int Height = -1;
        internal static bool HaveMatrix = false;
        internal static Matrix4x4 CameraMatrix;
        internal static uint LibUnity;

        internal static ulong LocalPlayer;
        internal static Vector3 LocalMainCamera;
        public static ConcurrentDictionary<long, Entity> Entities = new();

        internal static string state = "";
        internal static bool InMatch = false;
        internal static DateTime MatchTimerStart = DateTime.MinValue;
        internal static bool MatchTimerRunning = false;
        internal static bool MatchTimerFinished = false;
        internal static bool LocalPlayerIsKnocked = false;

        internal static Matrix4x4 CameraMatrixx
        {
            get => _cameraMatrix;
            set
            {
                _cameraMatrix = value;
                UpdateCameraVectors();
            }
        }
        private static void UpdateCameraVectors()
        {
            _cameraForward = new Vector3(_cameraMatrix.M13, _cameraMatrix.M23, _cameraMatrix.M33);
            _cameraForward = Vector3.Normalize(_cameraForward);

            _cameraRight = new Vector3(_cameraMatrix.M11, _cameraMatrix.M21, _cameraMatrix.M31);
            _cameraRight = Vector3.Normalize(_cameraRight);
        }
    }
}
