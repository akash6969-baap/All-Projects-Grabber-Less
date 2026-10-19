using AotForms;
using System;
using System.Numerics;
using System.Threading;

namespace Client
{
    internal class AutoTeleport
    {
        static uint staticGameFacade = 0;

        private static Vector3 MarkedPos()
        {
            if (staticGameFacade == 0)
            {
                if (!InternalMemory.Read<uint>(Offsets.Il2Cpp + Offsets.InitBase, out var baseGameFacade))
                    return Vector3.Zero;

                if (!InternalMemory.Read<uint>(baseGameFacade, out var gameFacade))
                    return Vector3.Zero;

                if (!InternalMemory.Read<uint>(gameFacade + Offsets.StaticClass, out staticGameFacade))
                    return Vector3.Zero;
            }

            if (!InternalMemory.Read<uint>(staticGameFacade + 0x0, out var currentGame)) return Vector3.Zero;
            if (!InternalMemory.Read<uint>(currentGame + 0x8, out uint UIInGameScene)) return Vector3.Zero;
            if (!InternalMemory.Read<uint>(UIInGameScene + 0x1E4, out uint m_BigMapCtrl)) return Vector3.Zero;
            if (!InternalMemory.Read<uint>(m_BigMapCtrl + 0x50, out uint m_MapContentCtrl)) return Vector3.Zero;
            if (!InternalMemory.Read<uint>(m_MapContentCtrl + 0x88, out uint m_LocalMapMarkController)) return Vector3.Zero;

            if (!InternalMemory.Read(m_LocalMapMarkController + 0x54, out Vector3 selectedpos))
                return Vector3.Zero;

            return selectedpos;
        }

        public static void Work()
        {
            while (true)
            {
                if (!Config.AutoTeleport)
                {
                    Thread.SpinWait(10);
                    continue;
                }

                if (Core.LocalPlayer == 0)
                {
                    Thread.SpinWait(10);
                    continue;
                }

                Vector3 markerPos = MarkedPos();

                if (markerPos.X == 0 && markerPos.Z == 0)
                {
                    Thread.SpinWait(10);
                    continue;
                }

                if (InternalMemory.Read((uint)Core.LocalPlayer + (uint)Bones.Root, out uint rootBonePtr))
                {
                    if (InternalMemory.Read(rootBonePtr + 0x8, out uint transformValue) &&
                        InternalMemory.Read(transformValue + 0x8, out uint transformObjPtr) &&
                        InternalMemory.Read(transformObjPtr + 0x24, out uint indexValue) &&
                        InternalMemory.Read(transformObjPtr + 0x20, out uint matrixPtr) &&
                        InternalMemory.Read(matrixPtr + 0x18, out uint matrixListValue))
                    {
                        Transform.GetNodePosition(rootBonePtr, out Vector3 currentMyPos);

                        Vector3 teleportPos;
                        teleportPos.X = markerPos.X;
                        teleportPos.Z = markerPos.Z;

                        teleportPos.Y = currentMyPos.Y > 0 ? currentMyPos.Y + 2.0f : 50f;

                        uint targetAddress = matrixListValue + (indexValue * 0x30);
                        InternalMemory.Write((ulong)targetAddress, teleportPos);
                    }
                }

                Thread.Sleep(50);
            }
        }
    }
}
