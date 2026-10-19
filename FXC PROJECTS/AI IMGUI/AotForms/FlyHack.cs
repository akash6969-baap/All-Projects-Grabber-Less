using AotForms;
using Memory;
using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    internal static class FlyMe
    {
        private static Task upTask;
        private static CancellationTokenSource cts = new();
        private static bool isRunning = false;

        // Lưu vị trí hiện tại để hiển thị ngoài UI
        public static Vector3 CurrentPosition { get; private set; }

        internal static void Work()
        {
            if (isRunning) return;
            isRunning = true;

            upTask = Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        // Kiểm tra đã bật tính năng bay hay chưa
                        if (!Config.flyme)
                        {
                            await Task.Delay(10, cts.Token);
                            continue;
                        }

                        // Đọc các cấp con trỏ đến ma trận transform
                        if (!InternalMemory.Read(Core.LocalPlayer + (uint)Bones.Root, out uint rootBonePtr) || rootBonePtr == 0) continue;
                        if (!InternalMemory.Read(rootBonePtr + 0x8, out uint transform1) || transform1 == 0) continue;
                        if (!InternalMemory.Read(transform1 + 0x8, out uint transform2) || transform2 == 0) continue;
                        if (!InternalMemory.Read(transform2 + 0x20, out uint matrixPtr) || matrixPtr == 0) continue;

                        // Đọc vị trí hiện tại
                        if (!InternalMemory.Read(matrixPtr + 0x80, out Vector3 currentPos)) continue;

                        // Ép Y thành độ cao do người dùng chọn trong Config
                        currentPos.Y = Config.flymeHeight;

                        // Ghi lại vị trí mới vào game
                        InternalMemory.Write(matrixPtr + 0x80, currentPos);

                        // Cập nhật vị trí để hiển thị ngoài UI
                        CurrentPosition = currentPos;
                    }
                    catch
                    {
                        // Bỏ qua lỗi
                    }

                    await Task.Delay(10, cts.Token); // Delay nhỏ để giữ hiệu suất
                }
            }, cts.Token);
        }

        internal static void Stop()
        {
            if (!isRunning) return;

            cts.Cancel();
            cts.Dispose();
            cts = new CancellationTokenSource();
            isRunning = false;
        }
    }
}
