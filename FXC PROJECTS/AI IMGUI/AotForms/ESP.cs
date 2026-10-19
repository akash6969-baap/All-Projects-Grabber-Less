using ImGuiNET;
using System;
using System.Drawing;
using System.Numerics;
using System.Linq;
using System.Threading.Tasks;
using static AotForms.WinAPI;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static AotForms.Config;
using Vortice.Mathematics;
using Size = System.Drawing.Size;
using Color = System.Drawing.Color;
using System.Collections.Concurrent;
using Memory;
using System.Windows.Forms;
using AotForms;


namespace AotForms
{
    internal class ESP : ClickableTransparentOverlay.Overlay
    {
        IntPtr hWnd;
        IntPtr HDPlayer;
        private CX memoryfast = new CX();
        private bool anticheat = false;
        private bool camera = false;
        private bool vision = false;
        private bool speedEnabled = false;
        private bool Wall = false;
        private IEnumerable<long> visionResult;
        private IEnumerable<long> cameraResult;
        private IEnumerable<long> speedResult;
        private IEnumerable<long> WallHack1;
        private float previousCameraVal = Config.cameraVal;
        private float previousVisionVal = Config.visionVal;

        private const short DefaultMaxHealth = 200;
        private Vector4 lineColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        private Vector4 fovColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        private Vector4 boxColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        private Vector4 box3DColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        private Vector4 skeletonColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        private Vector4 crosshairColor = new Vector4(1.0f, 0.0f, 0.0f, 1.0f); // Default red
        private Vector4 fillBoxColor = new Vector4(1.0f, 0.0f, 0.0f, 0.5f); // Default semi-transparent red

        // Glowing effect parameters
        internal static float GlowRadius = 20f;
        internal static float FeatherAmount = 2f;

        private uint cyanColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0.8f, 1f, 1f));
        bool showWindow = true;

        private readonly string[] _comboItems2 = { "Closest To Crosshair", "Target360", "Closest To Player", "Lowest Health" };
        private readonly string[] _comboItems1 = {  "Aimbot Rage","Aimbot Ai" };
        private readonly string[] _comboItems = { "Top", "Center", "Bottom" };
        private readonly string[] _headerItems = { "Aim", "Esp", "Colors", "Extras", "Misc" };
        private int _selectedHeader, _comboBox, _comboBox1, _comboBox2;

        private ConcurrentDictionary<int, EntityRenderData> processedEntities = new();
        private Task entityProcessingTask;

        private struct EntityRenderData
        {
            public Vector2 headScreenPos;
            public Vector2 bottomScreenPos;
            public float Distance;
            public bool IsValid;
        }

        private bool isProcessing = false;
        private float progress = 0.0f;

        private string CamReplacePattern => MakeAob(cameraVal);
        private string VisionReplacePattern => MakeAob(visionVal);

        private async void CameraOn()
        {
            string[] processname = { "HD-Player" };
            bool success = memoryfast.SetProcess(processname);
            if (!success)
            {
                return;
            }
            if (cameraResult == null)
            {
                cameraResult = await memoryfast.AoBScan("");
                Console.Beep(2000, 600);
            }
            if (cameraResult != null && cameraResult.Any())
            {
                foreach (long id in cameraResult)
                {
                    memoryfast.AobReplace(id, CamReplacePattern);
                }
            }
        }



        private async void CameraOff()
        {
            foreach (long id in cameraResult)
            {
                memoryfast.AobReplace(id, "");
            }
        }

        private async void VisionOn()
        {
            string[] processname = { "HD-Player" };
            bool success = memoryfast.SetProcess(processname);
            if (!success)
            {
                return;
            }
            if (visionResult == null)
            {
                visionResult = await memoryfast.AoBScan("");
                Console.Beep(2000, 600);
            }
            if (visionResult != null && visionResult.Any())
            {
                foreach (long id in visionResult)
                {
                    memoryfast.AobReplace(id, VisionReplacePattern);
                }
            }
        }

        private async void VisionOff()
        {
            foreach (long id in visionResult)
            {
                memoryfast.AobReplace(id, "");
            }
        }

        static string MakeAob(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return BitConverter.ToString(bytes).Replace("-", " ");
        }

        private async void SpeedOn()
        {
            string[] processname = { "HD-Player" };
            bool success = memoryfast.SetProcess(processname);

            if (!success)
            {
                return;
            }

            if (speedResult == null)
            {
                speedResult = await memoryfast.AoBScan("02 2B 07 3D 02 2B 07 3D 02 2B 07 3D 00 00 00 00 9B 6C");
            }

            if (speedResult != null && speedResult.Any())
            {
                foreach (long id in speedResult)
                {
                    memoryfast.AobReplace(id, "E3 A5 9B 3C E3 A5 9B 3C 02 2B 07 3D 00 00 00 00 9B 6C");
                }
                Console.Beep(2000, 600);
            }
        }

        


        private async void SpeedOff()
        {
            foreach (long id in speedResult)
            {
                memoryfast.AobReplace(id, "02 2B 07 3D 02 2B 07 3D 02 2B 07 3D 00 00 00 00 9B 6C");
            }
            Console.Beep(100, 100);
        }

        private async void Wallon()
        {
            string[] processname = { "HD-Player" };
            bool success = memoryfast.SetProcess(processname);

            if (!success)
            {
                return;
            }

            if (WallHack1 == null)
            {
                WallHack1 = await memoryfast.AoBScan("");
            }

            if (WallHack1 != null && WallHack1.Any())
            {
                foreach (long id in WallHack1)
                {
                    memoryfast.AobReplace(id, "");
                }
                Console.Beep(2000, 600);
            }
        }

        private async void Wallof()
        {
            foreach (long id in WallHack1)
            {
                memoryfast.AobReplace(id, "");
            }
            Console.Beep(100, 100);
        }

        private async void AntiCheat()
        {
            if (!anticheat)
            {
                List<string> searchList = new List<string>
                {
                    "",
                    "",
                    "",
                    "",
                   
                };

                List<string> replaceList = new List<string>
                {
                    "",
                    "",
                    "",
                    "",

                };

                bool k = false;
                memoryfast.SetProcess(new string[] { "HD-Player" });
                int i2 = 22000000;

                int totalSteps = searchList.Count;
                int currentStep = 0;

                for (int j = 0; j < searchList.Count; j++)
                {
                    progress = (float)currentStep / totalSteps;
                    IEnumerable<long> cu = await memoryfast.AoBScan(searchList[j]);
                    string u = "0x" + cu.FirstOrDefault().ToString("X");

                    if (cu.Count() != 0)
                    {
                        for (int i = 0; i < cu.Count(); i++)
                        {
                            i2++;
                            memoryfast.AobReplace(cu.ElementAt(i), replaceList[j]);
                        }
                        k = true;
                    }
                    currentStep++;
                    progress = (float)currentStep / totalSteps;
                }

                if (k == true)
                {
                    anticheat = true;
                    progress = 1.0f;
                    Console.Beep(2000, 600);
                }
                isProcessing = false;
            }
        }

        private void StartEntityProcessing()
        {
            entityProcessingTask = Task.Run(async () =>
            {
                while (true)
                {
                    if (!Core.HaveMatrix)
                    {
                        continue;
                    }

                    var newEntities = new ConcurrentDictionary<int, EntityRenderData>();

                    Parallel.ForEach(Core.Entities.Values, entity =>
                    {
                        if (entity.IsDead || !entity.IsKnown) return;

                        var dist = Vector3.Distance(Core.LocalMainCamera, entity.Head);
                        if (dist > 200) return;

                        var headScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
                        var bottomScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Root, Core.Width, Core.Height);
                        if (headScreenPos.X < 1 || headScreenPos.Y < 1 || bottomScreenPos.X < 1 || bottomScreenPos.Y < 1) return;

                        newEntities[entity.GetHashCode()] = new EntityRenderData
                        {
                            headScreenPos = headScreenPos,
                            bottomScreenPos = bottomScreenPos,
                            Distance = dist,
                            IsValid = true
                        };
                    });

                    processedEntities = newEntities;
                    await Task.Delay(20);
                }
            });
        }

        private void RenderEntities()
        {
            var drawList = ImGui.GetBackgroundDrawList();

            foreach (var entity in Core.Entities.Values)
            {
                if (!processedEntities.TryGetValue(entity.GetHashCode(), out var entityData) || !entityData.IsValid) continue;

                float CornerHeight = Math.Abs(entityData.headScreenPos.Y - entityData.bottomScreenPos.Y);
                float CornerWidth = CornerHeight * 0.65f;

                if (Config.ESPLine)
                {
                    Vector2 endPos = entityData.headScreenPos;
                    Vector2 lineStart = new Vector2(Core.Width / 2f, 10f);

                    switch (Config.ESPLinePosition)
                    {
                        case LinePosition.Top:
                            lineStart = new Vector2(Core.Width / 2f, 10f);
                            endPos = entityData.headScreenPos;
                            break;
                        case LinePosition.Center:
                            lineStart = new Vector2(Core.Width / 2f, Core.Height / 2f);
                            break;
                        case LinePosition.Bottom:
                            lineStart = new Vector2(Core.Width / 2f, Core.Height - 10f);
                            endPos = entityData.bottomScreenPos;
                            break;
                    }

                    DrawGlowingBall(lineStart, Config.ESPLineColor, 3f);

                    if (Config.GlowingLines)
                    {
                        DrawGlowLine(lineStart, endPos, ColorToUint32(Config.ESPLineColor), 1f, GlowRadius, FeatherAmount);
                    }
                    else
                    {
                        drawList.AddLine(lineStart, endPos, ColorToUint32(Config.ESPLineColor), 1f);
                    }
                }

                // ESP Fill Box - drawn first to be underneath the box outline
                if (Config.ESPFillBox)
                {
                    uint fillBoxColor = ColorToUint32(Color.FromArgb((int)(0.2f * 255), Config.ESPFillBoxColor));
                    // Darken the fill box color
                    fillBoxColor = ColorToUint32(Color.FromArgb(
                        (int)(0.2f * 255),
                        (int)(Config.ESPFillBoxColor.R * 0.7f),
                        (int)(Config.ESPFillBoxColor.G * 0.7f),
                        (int)(Config.ESPFillBoxColor.B * 0.7f)
                    ));

                    DrawFilledBox(
                        entityData.headScreenPos.X - (CornerWidth / 2),
                        entityData.headScreenPos.Y,
                        CornerWidth,
                        CornerHeight,
                        fillBoxColor
                    );
                }

                if (Config.ESPBox)
                {
                    uint boxColor = ColorToUint32(Config.ESPBoxColor);
                    DrawCorneredBox(entityData.headScreenPos.X - (CornerWidth / 2), entityData.headScreenPos.Y, CornerWidth, CornerHeight, boxColor, 1f);
                }

                if (Config.ESP3DBox)
                {
                    uint boxColor = ColorToUint32(Config.ESPBoxColor);
                    // Darken the box color for 3D box
                    boxColor = ColorToUint32(Color.FromArgb(
                        150,
                        (int)(Config.ESPBoxColor.R * 0.7f),
                        (int)(Config.ESPBoxColor.G * 0.7f),
                        (int)(Config.ESPBoxColor.B * 0.7f)
                    ));

                    Draw3dBox(
                        entityData.headScreenPos.X - (CornerWidth / 2),
                        entityData.headScreenPos.Y - 5,
                        CornerWidth,
                        CornerHeight,
                        boxColor,
                        1f
                    );
                }

                // Enhanced ESP Name and Distance
                string nameText = string.IsNullOrWhiteSpace(entity.Name) ? "BOT" : entity.Name;
                string distanceText = $"  |  {MathF.Round(entityData.Distance)}M"; // Format distance text

                var nameSize = ImGui.CalcTextSize(nameText);
                var distanceSize = ImGui.CalcTextSize(distanceText);

                // Calculate total width for proper centering
                float totalTextWidth = nameSize.X + distanceSize.X;

                // Set namePosition so that text is centered on the head
                Vector2 namePosition = new Vector2(
                    entityData.headScreenPos.X - (totalTextWidth / 2),
                    entityData.headScreenPos.Y - 35
                );

                // ESP Background for name/distance text
                if (Config.ESPBG)
                {
                    // Draw background with padding
                    drawList.AddRectFilled(
                        namePosition - new Vector2(10, 4), // Padding
                        namePosition + new Vector2(totalTextWidth, nameSize.Y) + new Vector2(6, 4), // Padding
                        ColorToUint32(Color.FromArgb((int)(0.4f * 255), 0, 0, 0)) // Black with 40% opacity
                    );
                }

                // ESP Moco - decorative arrow/indicator under name
                if (Config.ESPMoco)
                {
                    // Calculate the middle bottom of the text for triangle placement
                    Vector2 middleBottom = new Vector2(
                        namePosition.X + (totalTextWidth / 2),
                        namePosition.Y + nameSize.Y + 4
                    );

                    // Define triangle parameters for sword-like appearance
                    float triangleHeight = 18.0f;
                    float triangleBase = 10.0f;

                    // Define the three points for the upside-down triangle
                    Vector2 p1 = new Vector2(middleBottom.X - (triangleBase / 2), middleBottom.Y + 2);
                    Vector2 p2 = new Vector2(middleBottom.X + (triangleBase / 2), middleBottom.Y + 2);
                    Vector2 p3 = new Vector2(middleBottom.X, middleBottom.Y + 2 + triangleHeight);

                    // Draw the triangle
                    drawList.AddTriangleFilled(p1, p2, p3, ColorToUint32(Color.White));
                }

                if (Config.ESPName)
                {
                    // Add text for the name in white
                    drawList.AddText(namePosition, ColorToUint32(Color.White), nameText);

                    // Position for the distance text, right after the name
                    Vector2 distancePosition = new Vector2(
                        namePosition.X + nameSize.X + 4,
                        namePosition.Y
                    );

                    // Draw the distance in yellow
                    drawList.AddText(distancePosition, ColorToUint32(Color.Yellow), distanceText);
                }

                if (Config.ESPHealth)
                {
                    // Enhanced health bar with fixed size and better positioning
                    float healthBarHeight = 5.0f;
                    float healthBarWidth = totalTextWidth + 20.0f;

                    // Position above the name text with proper spacing
                    Vector2 healthBarPosition = new Vector2(
                        namePosition.X - 12,
                        namePosition.Y - healthBarHeight + 25
                    );

                    // Draw background for health bar
                    drawList.AddRectFilled(
                        new Vector2(healthBarPosition.X - 1, healthBarPosition.Y - 1),
                        new Vector2(healthBarPosition.X + healthBarWidth + 2, healthBarPosition.Y + healthBarHeight + 2),
                        ColorToUint32(Color.FromArgb(80, 80, 80)) // Dark grey background
                    );

                    // Draw the health bar with proper coloring
                    if (!entity.IsKnocked)
                    {
                        DrawEnhancedHealthBar(entity.Health, DefaultMaxHealth, healthBarPosition.X, healthBarPosition.Y, healthBarHeight, healthBarWidth);
                    }
                    else
                    {
                        DrawKnockedHealthBar(entity.Health, DefaultMaxHealth, healthBarPosition.X, healthBarPosition.Y, healthBarHeight, healthBarWidth);
                    }
                }

                if (Config.ESPHealthText)
                {
                    if (!entity.IsKnocked)
                    {
                        DrawHealthText(entity, entity.Health);
                    }
                }

                if (Config.ESPSkeleton)
                {
                    DrawSkeleton(entity);
                }
            }
        }

        // New method for drawing filled boxes
        private void DrawFilledBox(float X, float Y, float W, float H, uint color)
        {
            var vList = ImGui.GetBackgroundDrawList();
            vList.AddRectFilled(new Vector2(X, Y), new Vector2(X + W, Y + H), color);
        }

        // New method for drawing 3D boxes
        private void Draw3dBox(float X, float Y, float W, float H, uint color, float thickness)
        {
            var vList = ImGui.GetBackgroundDrawList();

            Vector3[] screentions = new Vector3[]
            {
                new Vector3(X, Y, 0),                // Top-left front
                new Vector3(X, Y + H, 0),            // Bottom-left front
                new Vector3(X + W, Y + H, 0),        // Bottom-right front
                new Vector3(X + W, Y, 0),            // Top-right front
                new Vector3(X, Y, -W),               // Top-left back
                new Vector3(X, Y + H, -W),           // Bottom-left back
                new Vector3(X + W, Y + H, -W),       // Bottom-right back
                new Vector3(X + W, Y, -W)            // Top-right back
            };

            // Draw front face
            vList.AddLine(new Vector2(screentions[0].X, screentions[0].Y), new Vector2(screentions[1].X, screentions[1].Y), color, thickness);
            vList.AddLine(new Vector2(screentions[1].X, screentions[1].Y), new Vector2(screentions[2].X, screentions[2].Y), color, thickness);
            vList.AddLine(new Vector2(screentions[2].X, screentions[2].Y), new Vector2(screentions[3].X, screentions[3].Y), color, thickness);
            vList.AddLine(new Vector2(screentions[3].X, screentions[3].Y), new Vector2(screentions[0].X, screentions[0].Y), color, thickness);

            // Draw back face
            vList.AddLine(new Vector2(screentions[4].X, screentions[4].Y), new Vector2(screentions[5].X, screentions[5].Y), color, thickness);
            vList.AddLine(new Vector2(screentions[5].X, screentions[5].Y), new Vector2(screentions[6].X, screentions[6].Y), color, thickness);
            vList.AddLine(new Vector2(screentions[6].X, screentions[6].Y), new Vector2(screentions[7].X, screentions[7].Y), color, thickness);
            vList.AddLine(new Vector2(screentions[7].X, screentions[7].Y), new Vector2(screentions[4].X, screentions[4].Y), color, thickness);

            // Draw connecting lines
            vList.AddLine(new Vector2(screentions[0].X, screentions[0].Y), new Vector2(screentions[4].X, screentions[4].Y), color, thickness);
            vList.AddLine(new Vector2(screentions[1].X, screentions[1].Y), new Vector2(screentions[5].X, screentions[5].Y), color, thickness);
            vList.AddLine(new Vector2(screentions[2].X, screentions[2].Y), new Vector2(screentions[6].X, screentions[6].Y), color, thickness);
            vList.AddLine(new Vector2(screentions[3].X, screentions[3].Y), new Vector2(screentions[7].X, screentions[7].Y), color, thickness);
        }

        // Enhanced health bar drawing for normal state
        public void DrawEnhancedHealthBar(short health, short maxHealth, float X, float Y, float height, float width)
        {
            var vList = ImGui.GetBackgroundDrawList();

            // Ensure health percentage is valid
            if (maxHealth <= 0) maxHealth = 100; // Fallback value
            float healthPercentage = Math.Clamp((float)health / maxHealth, 0f, 1f);
            float healthWidth = width * healthPercentage;

            // Determine color based on health percentage
            Color healthColor;
            if (healthPercentage < 0.3f)
            {
                healthColor = Color.FromArgb((int)(1f * 255), 255, 0, 0); // Red for low health
            }
            else if (healthPercentage < 0.8f)
            {
                healthColor = Color.FromArgb((int)(1f * 255), 255, 255, 0); // Yellow for medium health
            }
            else
            {
                healthColor = Color.FromArgb((int)(1f * 255), 86, 255, 43); // Green for high health
            }

            // Draw background (unfilled part)
            vList.AddRectFilled(
                new Vector2(X, Y - height),
                new Vector2(X + width, Y),
                ColorToUint32(Color.FromArgb((int)(1f * 255), 99, 0, 0)) // Dark red background
            );

            // Draw filled health portion
            vList.AddRectFilled(
                new Vector2(X, Y - height),
                new Vector2(X + healthWidth, Y),
                ColorToUint32(healthColor)
            );

            // Draw black outline
            vList.AddRect(
                new Vector2(X, Y - height),
                new Vector2(X + width, Y),
                ColorToUint32(Color.Black),
                1f
            );
        }

        // Health bar drawing for knocked state
        public void DrawKnockedHealthBar(short health, short maxHealth, float X, float Y, float height, float width)
        {
            var vList = ImGui.GetBackgroundDrawList();

            // Ensure health percentage is valid
            if (maxHealth <= 0) maxHealth = 100; // Fallback value
            float healthPercentage = Math.Clamp((float)health / maxHealth, 0f, 1f);
            float healthWidth = width * healthPercentage;

            // Use red for knocked down players
            Color healthColor = Color.FromArgb((int)(1f * 255), 255, 0, 0);

            // Draw background (unfilled part)
            vList.AddRectFilled(
                new Vector2(X, Y - height),
                new Vector2(X + width, Y),
                ColorToUint32(Color.FromArgb((int)(1f * 255), 99, 0, 0)) // Dark red background
            );

            // Draw filled health portion
            vList.AddRectFilled(
                new Vector2(X, Y - height),
                new Vector2(X + healthWidth, Y),
                ColorToUint32(healthColor)
            );

            // Draw black outline
            vList.AddRect(
                new Vector2(X, Y - height),
                new Vector2(X + width, Y),
                ColorToUint32(Color.Black),
                1f
            );
        }

        private void UpdateEntities()
        {
            foreach (var entity in Core.Entities.Values)
            {
                if (entity.IsTeam != Bool3.False) continue;

                TreeNode entityNode = new TreeNode(entity.Name);

                entityNode.Nodes.Add(new TreeNode($"IsKnown: {entity.IsKnown}"));
                entityNode.Nodes.Add(new TreeNode($"IsTeam: {entity.IsTeam}"));
                entityNode.Nodes.Add(new TreeNode($"Head: {entity.Head}"));
                entityNode.Nodes.Add(new TreeNode($"Root: {entity.Root}"));
                entityNode.Nodes.Add(new TreeNode($"Health: {entity.Health}"));
                entityNode.Nodes.Add(new TreeNode($"IsDead: {entity.IsDead}"));
                entityNode.Nodes.Add(new TreeNode($"IsKnocked: {entity.IsKnocked}"));
            }
            Thread.Sleep(1000);
        }

        private void NoCache()
        {
            InternalMemory.Cache = new();
            Core.Entities = new();
            Thread.Sleep(1000);
        }

        protected override unsafe void Render()
        {
            CreateHandle();
            if (GetAsyncKeyState(Keys.Insert) < 0)
            {
                showWindow = !showWindow;
                Thread.Sleep(200);
            }

            if (showWindow)
            {
                RenderUI();
            }

            if (!Core.HaveMatrix) return;

            if (Config.FOVEnabled)
            {
                DrawGlowingFOVCircle(Config.AimFov);
            }

            // Draw the rotating crosshair
            if (Config.ESPCrosshair)
            {
                DrawRotatingCrosshair();
            }

            // Premium themed watermark text (NEON PURPLE DSE)
            string text = "FEAR MODS";
            var windowWidth = Core.Width;
            var windowHeight = Core.Height;
            var textSize = ImGui.CalcTextSize(text);

            // Centered X, thoda upar Y
            float textPosX = (windowWidth - textSize.X) / 2f;
            float textPosY = 80f;

            var drawList = ImGui.GetBackgroundDrawList();

            // ===== NEON PURPLE COLORS =====
            uint mainColor = ImGui.ColorConvertFloat4ToU32(
                new Vector4(0.85f, 0.35f, 1.00f, 1.00f)); // Bright neon purple

            uint glowColor = ImGui.ColorConvertFloat4ToU32(
                new Vector4(0.95f, 0.70f, 1.00f, 0.6f));  // Soft purple glow

            uint shadowColor = ImGui.ColorConvertFloat4ToU32(
                new Vector4(0f, 0f, 0f, 0.75f));          // Deep shadow

            // ===== OUTER GLOW (MULTI-LAYER) =====
            for (float offset = 4f; offset > 0; offset -= 1f)
            {
                float alpha = (offset / 4f) * 0.45f;

                uint currentGlow = ImGui.ColorConvertFloat4ToU32(
                    new Vector4(0.95f, 0.70f, 1.00f, alpha));

                drawList.AddText(new Vector2(textPosX + offset, textPosY), currentGlow, text);
                drawList.AddText(new Vector2(textPosX - offset, textPosY), currentGlow, text);
                drawList.AddText(new Vector2(textPosX, textPosY + offset), currentGlow, text);
                drawList.AddText(new Vector2(textPosX, textPosY - offset), currentGlow, text);
            }

            // ===== DEEP SHADOW FOR DEPTH =====
            var shadowOffsets = new[]
            {
    new Vector2(2, 2), new Vector2(-2, 2),
    new Vector2(2, -2), new Vector2(-2, -2),
    new Vector2(0, 2), new Vector2(0, -2),
    new Vector2(2, 0), new Vector2(-2, 0)
};

            foreach (var offset in shadowOffsets)
            {
                drawList.AddText(
                    new Vector2(textPosX + offset.X, textPosY + offset.Y),
                    shadowColor,
                    text
                );
            }

            // ===== MAIN NEON PURPLE TEXT =====
            drawList.AddText(new Vector2(textPosX, textPosY), mainColor, text);

            // ===== OPTIONAL: NEON UNDERLINE (PURPLE) =====
            // drawList.AddLine(
            //     new Vector2(textPosX - 12, textPosY + textSize.Y + 8),
            //     new Vector2(textPosX + textSize.X + 12, textPosY + textSize.Y + 8),
            //     ImGui.ColorConvertFloat4ToU32(new Vector4(0.85f, 0.35f, 1.00f, 0.6f)),
            //     2.5f
            // );

            // ===== WINDOW HANDLE STUFF =====
            string windowName = "Overlay";
            hWnd = FindWindow(null!, windowName);
            HDPlayer = FindWindow("BlueStacksApp", null!);
            if (hWnd != IntPtr.Zero)
            {
                long extendedStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
                SetWindowLong(hWnd, GWL_EXSTYLE,
                    (extendedStyle | WS_EX_TOOLWINDOW) & ~WS_EX_APPWINDOW);
            }

            RenderEntities();
        }

        private void DrawLine(ImDrawListPtr drawList, Vector2 startPos, Vector2 endPos, uint color)
        {
            if (startPos.X > 0 && startPos.Y > 0 && endPos.X > 0 && endPos.Y > 0)
            {
                drawList.AddLine(startPos, endPos, color, 1.5f); // Adjust thickness as needed
            }
        }

        private void DrawSkeleton(Entity entity)
        {
            var drawList = ImGui.GetBackgroundDrawList();
            uint lineColor = ColorToUint32(Config.ESPSkeletonColor);
            uint circleColor = ColorToUint32(Color.White);

            var headScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
           // var leftWristScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightWrist, Core.Width, Core.Height);
            //var spineScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Spine, Core.Width, Core.Height);
            var hipScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Hip, Core.Width, Core.Height);
            var rootScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Root, Core.Width, Core.Height);
          //  var rightCalfScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightCalf, Core.Width, Core.Height);
           // var leftCalfScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftCalf, Core.Width, Core.Height);
            var rightFootScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightFoot, Core.Width, Core.Height);
            var leftFootScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftFoot, Core.Width, Core.Height);
          //  var rightWristScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightWrist, Core.Width, Core.Height);
            var leftHandScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftHand, Core.Width, Core.Height);
            var leftShoulderScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftShoulder, Core.Width, Core.Height);
            var rightShoulderScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightShoulder, Core.Width, Core.Height);
          //  var rightWristJointScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightWristJoint, Core.Width, Core.Height);
          //  var leftWristJointScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftWristJoint, Core.Width, Core.Height);
           // var leftElbowScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftElbow, Core.Width, Core.Height);
            var rightElbowScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightElbow, Core.Width, Core.Height);

            //DrawLine(drawList, spineScreenPos, rightShoulderScreenPos, lineColor);
           // DrawLine(drawList, spineScreenPos, hipScreenPos, lineColor);

           // DrawLine(drawList, spineScreenPos, leftShoulderScreenPos, lineColor);
           // DrawLine(drawList, leftShoulderScreenPos, rightElbowScreenPos, lineColor);
           // DrawLine(drawList, rightShoulderScreenPos, leftElbowScreenPos, lineColor);

           // DrawLine(drawList, leftElbowScreenPos, rightWristJointScreenPos, lineColor);
            //DrawLine(drawList, rightElbowScreenPos, leftWristJointScreenPos, lineColor);

            DrawLine(drawList, hipScreenPos, rightFootScreenPos, lineColor);
            DrawLine(drawList, hipScreenPos, leftFootScreenPos, lineColor);

            float distance = entity.Distance;

            float baseRadius = 50.0f;
            float circleRadius = baseRadius / distance;

            if (headScreenPos.X > 0 && headScreenPos.Y > 0)
            {
                drawList.AddCircle(headScreenPos, circleRadius, circleColor, 30);
            }
        }

        private static uint[] HealthColors = new uint[]
        {
            ColorToUint32(Color.Green),
            ColorToUint32(Color.Yellow),
            ColorToUint32(Color.Red)
        };

        private static uint GetHealthColor(float healthPercentage)
        {
            if (healthPercentage >= 0.7f) return HealthColors[0];
            if (healthPercentage >= 0.3f) return HealthColors[1];
            return HealthColors[2];
        }

        public void DrawHealthText(Entity entity, int espHealthInt)
        {
            Vector2 headScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
            Vector2 pieDerechoScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightFoot, Core.Width, Core.Height);

            string healthText = "Health: " + espHealthInt.ToString();

            Vector2 textPosition = new Vector2(pieDerechoScreenPos.X - 25, pieDerechoScreenPos.Y + 10);

            var drawList = ImGui.GetBackgroundDrawList();

            uint borderColor = ColorToUint32(Color.Black);

            float offset = 1.0f;

            drawList.AddText(new Vector2(textPosition.X - offset, textPosition.Y), borderColor, healthText);
            drawList.AddText(new Vector2(textPosition.X + offset, textPosition.Y), borderColor, healthText);
            drawList.AddText(new Vector2(textPosition.X, textPosition.Y - offset), borderColor, healthText);
            drawList.AddText(new Vector2(textPosition.X, textPosition.Y + offset), borderColor, healthText);

            drawList.AddText(textPosition, ColorToUint32(Color.White), healthText);
        }

        // Glowing line effect
        public void DrawGlowLine(Vector2 start, Vector2 end, uint color, float thickness, float glowRadius, float feather)
        {
            var drawList = ImGui.GetBackgroundDrawList();
            Vector4 colorVec = ImGui.ColorConvertU32ToFloat4(color);

            // Outer glow layers
            for (float i = glowRadius; i > 0; i -= feather)
            {
                // Calculate alpha for the outer layers based on the current radius and feathering
                float alpha = colorVec.W * (i / glowRadius) * 0.02f; // Adjust glow strength with 0.02f factor
                uint glowColor = ImGui.ColorConvertFloat4ToU32(new Vector4(colorVec.X, colorVec.Y, colorVec.Z, alpha));

                // Draw each outer layer with increasing thickness to create a soft glow effect
                drawList.AddLine(start, end, glowColor, thickness + i);
            }

            // Main line at the center
            drawList.AddLine(start, end, color, thickness);
        }

        public void DrawGlowingBall(Vector2 position, Color color, float radius)
        {
            var drawList = ImGui.GetBackgroundDrawList();
            uint ballColor = ColorToUint32(color);

            for (int i = 0; i < 5; i++)
            {
                float glowRadius = radius + (i * 2);
                float alpha = 1.0f - (i * 0.2f);

                drawList.AddCircleFilled(
                    position,
                    glowRadius,
                    ImGui.ColorConvertFloat4ToU32(new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, alpha)),
                    50
                );
            }

            drawList.AddCircleFilled(position, radius, ballColor, 50);
        }

        public void DrawCorneredBox(float X, float Y, float W, float H, uint color, float thickness)
        {
            var vList = ImGui.GetBackgroundDrawList();

            float lineW = W / 3;
            float lineH = H / 3;

            vList.AddLine(new Vector2(X, Y - thickness / 2), new Vector2(X, Y + lineH), color, thickness);
            vList.AddLine(new Vector2(X - thickness / 2, Y), new Vector2(X + lineW, Y), color, thickness);
            vList.AddLine(new Vector2(X + W - lineW, Y), new Vector2(X + W + thickness / 2, Y), color, thickness);
            vList.AddLine(new Vector2(X + W, Y - thickness / 2), new Vector2(X + W, Y + lineH), color, thickness);
            vList.AddLine(new Vector2(X, Y + H - lineH), new Vector2(X, Y + H + thickness / 2), color, thickness);
            vList.AddLine(new Vector2(X - thickness / 2, Y + H), new Vector2(X + lineW, Y + H), color, thickness);
            vList.AddLine(new Vector2(X + W - lineW, Y + H), new Vector2(X + W + thickness / 2, Y + H), color, thickness);
            vList.AddLine(new Vector2(X + W, Y + H - lineH), new Vector2(X + W, Y + H + thickness / 2), color, thickness);
        }

        static uint ColorToUint32(Color color)
        {
            return ImGui.ColorConvertFloat4ToU32(new Vector4(
                color.R / 255.0f,
                color.G / 255.0f,
                color.B / 255.0f,
                color.A / 255.0f));
        }

        private int lastWidth = 0, lastHeight = 0;
        void CreateHandle()
        {
            if (Config.StreamMode)
            {
                SetWindowDisplayAffinity(hWnd, WDA_EXCLUDEFROMCAPTURE);
            }
            else
            {
                SetWindowDisplayAffinity(hWnd, WDA_NONE);
            }
            RECT rect;
            GetWindowRect(Core.Handle, out rect);

            int x = rect.Left;
            int y = rect.Top;
            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;
            if (Core.Width != lastWidth || Core.Height != lastHeight)
            {
                lastWidth = Core.Width;
                lastHeight = Core.Height;
                ImGui.SetWindowSize(new Vector2(Core.Width, Core.Height));
            }

            Size = new Size(width, height);
            Position = new Point(x, y);

            Core.Width = width;
            Core.Height = height;
        }

        public void DrawGlowingFOVCircle(float radius)
        {
            var drawList = ImGui.GetBackgroundDrawList();
            var center = new Vector2(Core.Width / 2f, Core.Height / 2f);
            uint color = ColorToUint32(Config.FOVColor);
            Vector4 colorVec = ImGui.ColorConvertU32ToFloat4(color);

            // Draw outer glow layers
            for (float i = GlowRadius; i > 0; i -= FeatherAmount)
            {
                // Calculate alpha for each layer based on distance from main circle
                float alpha = colorVec.W * (i / GlowRadius) * 0.03f;
                uint glowColor = ImGui.ColorConvertFloat4ToU32(new Vector4(
                    colorVec.X, colorVec.Y, colorVec.Z, alpha));

                // Draw each glow layer with increasing radius
                drawList.AddCircle(center, radius + i, glowColor, 0, 1f);
            }

            // Draw the main circle on top
            drawList.AddCircle(center, radius, color, 0, 1f);
        }

        // Enhanced rotating crosshair implementation with advanced glow effects
        private void DrawRotatingCrosshair()
        {
            if (!Config.ESPCrosshair) return;

            var drawList = ImGui.GetBackgroundDrawList();
            var center = new Vector2(Core.Width / 2f, Core.Height / 2f);

            // Calculate rotation based on time and user-defined speed
            float timeSeconds = (float)DateTime.Now.TimeOfDay.TotalSeconds;
            float rotation = timeSeconds * Config.CrosshairRotationSpeed * 6; // Converting to degrees (RPM * 360° / 60 seconds)

            // Convert to radians
            float rotationRad = rotation * (MathF.PI / 180f);

            // Calculate color (fixed or RGB mode)
            uint crosshairColor;
            if (Config.ESPCrosshairRGB)
            {
                // Cycle through RGB colors
                float r = (MathF.Sin(timeSeconds * 2f) + 1f) / 2f;
                float g = (MathF.Sin(timeSeconds * 2f + 2f * MathF.PI / 3f) + 1f) / 2f;
                float b = (MathF.Sin(timeSeconds * 2f + 4f * MathF.PI / 3f) + 1f) / 2f;
                crosshairColor = ImGui.ColorConvertFloat4ToU32(new Vector4(r, g, b, 1.0f));
            }
            else
            {
                crosshairColor = ColorToUint32(Config.ESPCrosshairColor);
            }

            // Calculate crosshair line endpoints
            float length = Config.ESPCrosshairSize;

            // First line (rotating)
            float x1 = center.X + length * MathF.Cos(rotationRad);
            float y1 = center.Y + length * MathF.Sin(rotationRad);
            float x2 = center.X - length * MathF.Cos(rotationRad);
            float y2 = center.Y - length * MathF.Sin(rotationRad);

            // Second line (perpendicular to first)
            float x3 = center.X + length * MathF.Cos(rotationRad + MathF.PI / 2f);
            float y3 = center.Y + length * MathF.Sin(rotationRad + MathF.PI / 2f);
            float x4 = center.X - length * MathF.Cos(rotationRad + MathF.PI / 2f);
            float y4 = center.Y - length * MathF.Sin(rotationRad + MathF.PI / 2f);

            if (Config.CrosshairGlowing)
            {
                // Advanced glow effect using the same technique as ESP lines
                Vector4 colorVec = ImGui.ColorConvertU32ToFloat4(crosshairColor);

                // First line glow
                DrawGlowingCrosshairLine(
                    new Vector2(x1, y1),
                    new Vector2(x2, y2),
                    crosshairColor,
                    Config.ESPCrosshairThickness,
                    Config.CrosshairGlowRadius,
                    Config.CrosshairFeatherAmount
                );

                // Second line glow
                DrawGlowingCrosshairLine(
                    new Vector2(x3, y3),
                    new Vector2(x4, y4),
                    crosshairColor,
                    Config.ESPCrosshairThickness,
                    Config.CrosshairGlowRadius,
                    Config.CrosshairFeatherAmount
                );
            }
            else
            {
                // Simple non-glowing crosshair
                drawList.AddLine(new Vector2(x1, y1), new Vector2(x2, y2), crosshairColor, Config.ESPCrosshairThickness);
                drawList.AddLine(new Vector2(x3, y3), new Vector2(x4, y4), crosshairColor, Config.ESPCrosshairThickness);
            }
        }

        // Helper method for drawing glowing crosshair lines
        private void DrawGlowingCrosshairLine(Vector2 start, Vector2 end, uint color, float thickness, float glowRadius, float feather)
        {
            var drawList = ImGui.GetBackgroundDrawList();
            Vector4 colorVec = ImGui.ColorConvertU32ToFloat4(color);

            // Outer glow layers
            for (float i = glowRadius; i > 0; i -= feather)
            {
                // Calculate alpha for the outer layers based on the current radius and feathering
                float alpha = colorVec.W * (i / glowRadius) * 0.1f; // Higher multiplier for more visible glow
                uint glowColor = ImGui.ColorConvertFloat4ToU32(new Vector4(colorVec.X, colorVec.Y, colorVec.Z, alpha));

                // Draw each outer layer with increasing thickness to create a soft glow effect
                drawList.AddLine(start, end, glowColor, thickness + i);
            }

            // Main line at the center
            drawList.AddLine(start, end, color, thickness);
        }

        public void RenderUI()
        {
            ImGuiWindowFlags window_flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse |
                                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoTitleBar;

            // Window size & position
            ImGui.SetNextWindowSize(new Vector2(320, 520), ImGuiCond.Once);
            ImGui.SetNextWindowPos(new Vector2(30, 100), ImGuiCond.Once);

            ImGui.Begin("FEAR MODS", window_flags);
            {
                // Header
                ImGui.Text("FEAR MODS");
                ImGui.Separator();
                ImGui.Spacing();

                // Buttons
                Vector2 buttonSize = new Vector2(81, 0); // fixed width and height
                float spacing = 6; // gap between buttons

                // Position start
                float startX = ImGui.GetCursorPosX();

                if (ImGui.Button("Aim", buttonSize)) activeTab = 0;
                ImGui.SameLine(startX + buttonSize.X + spacing);

                if (ImGui.Button("Visuals", buttonSize)) activeTab = 1;
                ImGui.SameLine(startX + (buttonSize.X + spacing) * 2);

           //     if (ImGui.Button("Misc", buttonSize)) activeTab = 2;
          //      ImGui.SameLine(startX + (buttonSize.X + spacing) * 3);

                if (ImGui.Button("Config", buttonSize)) activeTab = 3;

                ImGui.Spacing();
                ImGui.Separator();
            }

                ImGui.BeginChild("Main Content", Vector2.Zero, ImGuiChildFlags.Border, ImGuiWindowFlags.None);
            switch (activeTab)
            {
                case 0:
                    ImGui.Text("Aim");
                    ImGui.Separator();

                    ImGui.Checkbox("Enable Aimbot", ref Config.enableAimBot);
                    Text("Select Aimbot Type", 0.6f);
                    ImGui.PushStyleColor(ImGuiCol.PopupBg,
                    new Vector4(0.1725490242242813f, 0.1921568661928177f, 0.2352941185235977f, 1.0f));
                    ImGui.Combo("", ref _comboBox1, _comboItems1, _comboItems1.Length, 10);
                    ImGui.PopStyleColor();
                    Config.AimBotType = (AimBotType)_comboBox1;

                    switch (Config.AimBotType)
                    {

                        case AimBotType.Rage:  
                            Config.AimBotRage = true;
                            Config.AimbotVisible = false;
                            break;


                        case AimBotType.VISIBLE:
                            Config.AimBotRage = false;
                            Config.AimbotVisible = true;
                            break;
                    }


                 
                    ImGui.PopStyleColor();

                    ImGui.NewLine();
                    ImGui.Checkbox(" Ignore Knocked", ref Config.IgnoreKnocked);                 
                    ImGui.Checkbox("No Recoil", ref Config.NoRecoil);                                  
                    ImGui.Checkbox(" Aimfov Circle", ref Config.FOVEnabled);
                    if (Config.FOVEnabled)
                    {
                        ImGui.SliderFloat("Fov Size", ref Config.AimFov, 0, 1000);
                    }
                    Text("Distance", 0.6f);
                    ImGui.SliderFloat("Di", ref Config.AimBotMaxDistance, 10f, 1000f, "%.0f");
                    Text("Smoothness", 0.6f);
                    ImGui.SliderFloat("Smt", ref Config.AimBotSmooth, 0, 30);

                   // ImGui.Checkbox("Enemy Pull 360", ref Config.EnemyPullEnabled);
                    if (Config.EnemyPullEnabled)
                    {
                        Text("Distance", 0.6f);
                        ImGui.SliderFloat("D", ref Config.EnemyPullMaxDistance, 50f, 500f, "50f");
                        Text("Strength", 0.6f);
                        ImGui.SliderFloat("S", ref Config.EnemyPullStrength, 0.01f, 0.2f);
                    }

                    ImGui.Separator();
                    break;

                case 1:
                    ImGui.Text("Visuals");
                    ImGui.Separator();

                    if (ImGui.CollapsingHeader("ESP"))
                    {
                        Text("Esp Hacks", 0.6f);
                        ImGui.Checkbox(" ESP Line", ref Config.ESPLine);
                        ImGui.SameLine();
                        ImGui.PushStyleColor(ImGuiCol.PopupBg,
                            new Vector4(0.1725490242242813f, 0.1921568661928177f, 0.2352941185235977f, 1.0f));
                        ImGui.Combo("", ref _comboBox, _comboItems, _comboItems.Length, 10);
                        ImGui.PopStyleColor();
                        Config.ESPLinePosition = (LinePosition)_comboBox;

                        // Glowing lines feature toggle
                        ImGui.Checkbox(" Glowing Lines", ref Config.GlowingLines);
                        if (Config.GlowingLines)
                        {
                            ImGui.SliderFloat("Glow Radius", ref GlowRadius, 5f, 50f);
                            ImGui.SliderFloat("Feather Amount", ref FeatherAmount, 0.5f, 5f);
                            Text("Higher radius = wider glow, Higher feather = smoother fade", 0.6f);
                        }

                        ImGui.Checkbox(" ESP Box", ref Config.ESPBox);

                        // New Feature: ESP Fill Box
                        ImGui.Checkbox(" ESP Fill Box", ref Config.ESPFillBox);
                        if (Config.ESPFillBox)
                        {
                            ImGui.ColorEdit4(" Fill Box Color", ref fillBoxColor, ImGuiColorEditFlags.PickerMask | ImGuiColorEditFlags.NoInputs);
                            {
                                Config.ESPFillBoxColor = Color.FromArgb(
                                                        (int)(fillBoxColor.W * 255),
                                                        (int)(fillBoxColor.X * 255),
                                                        (int)(fillBoxColor.Y * 255),
                                                        (int)(fillBoxColor.Z * 255)
                                                                    );
                            }
                        }

                        // New Feature: ESP 3D Box
                        ImGui.Checkbox(" ESP 3D Box", ref Config.ESP3DBox);

                        ImGui.Checkbox(" ESP Name", ref Config.ESPName);

                        // New Feature: ESP Moco (arrow indicator)
                        ImGui.Checkbox(" ESP Moco", ref Config.ESPMoco);

                        // New Feature: ESP Background
                        ImGui.Checkbox(" ESP Background", ref Config.ESPBG);

                        ImGui.Checkbox(" ESP Health", ref Config.ESPHealth);
                        ImGui.Checkbox(" ESP Health Text", ref Config.ESPHealthText);
                        ImGui.Checkbox(" ESP Skeleton ", ref Config.ESPSkeleton);

                        if (ImGui.Button("Refresh ESP"))
                        {
                            NoCache();
                        }

                        if (ImGui.Button("Update Entities"))
                        {
                            UpdateEntities();
                        }
                    }

                    ImGui.Separator();
                    if (ImGui.CollapsingHeader("Crosshair"))
                    {
                        Text("Crosshair Settings", 0.6f);
                        ImGui.Checkbox(" Rotating Crosshair", ref Config.ESPCrosshair);
                        ImGui.Checkbox(" RGB Mode", ref Config.ESPCrosshairRGB);

                        if (!Config.ESPCrosshairRGB)
                        {
                            ImGui.ColorEdit4(" Crosshair Color", ref crosshairColor, ImGuiColorEditFlags.PickerMask | ImGuiColorEditFlags.NoInputs);
                            {
                                Config.ESPCrosshairColor = Color.FromArgb(
                                                            (int)(crosshairColor.W * 255),
                                                            (int)(crosshairColor.X * 255),
                                                            (int)(crosshairColor.Y * 255),
                                                            (int)(crosshairColor.Z * 255)
                                                                        );
                            }
                        }

                        ImGui.SliderFloat("Crosshair Size", ref Config.ESPCrosshairSize, 5f, 50f);
                        ImGui.SliderFloat("Line Thickness", ref Config.ESPCrosshairThickness, 1f, 5f);

                        // New rotation speed control
                        ImGui.SliderFloat("Rotation Speed (RPM)", ref Config.CrosshairRotationSpeed, 5f, 120f);
                        Text("Controls how fast the crosshair rotates in rotations per minute", 0.6f);

                        // New glow effect controls
                        ImGui.Separator();
                        Text("Glow Effects", 0.6f);
                        ImGui.Checkbox(" Enable Glowing Crosshair", ref Config.CrosshairGlowing);

                        if (Config.CrosshairGlowing)
                        {
                            ImGui.SliderFloat("Glow Radius", ref Config.CrosshairGlowRadius, 2f, 30f);
                            ImGui.SliderFloat("Feather Amount", ref Config.CrosshairFeatherAmount, 0.5f, 5f);
                            Text("Higher radius = wider glow, Higher feather = smoother fade", 0.6f);
                        }
                    }

                    ImGui.Separator();

                    if (ImGui.CollapsingHeader("Colors"))
                    {
                        Text("Colors", 0.6f);
                        ImGui.ColorEdit4(" Aimfov Color", ref fovColor, ImGuiColorEditFlags.PickerMask | ImGuiColorEditFlags.NoInputs);
                        {
                            Config.FOVColor = Color.FromArgb(
                                                    (int)(fovColor.W * 255),
                                                    (int)(fovColor.X * 255),
                                                    (int)(fovColor.Y * 255),
                                                    (int)(fovColor.Z * 255)
                                                                );
                        }
                        ImGui.ColorEdit4(" ESP Line Color", ref lineColor, ImGuiColorEditFlags.PickerMask | ImGuiColorEditFlags.NoInputs);
                        {
                            Config.ESPLineColor = Color.FromArgb(
                                                    (int)(lineColor.W * 255),
                                                    (int)(lineColor.X * 255),
                                                    (int)(lineColor.Y * 255),
                                                    (int)(lineColor.Z * 255)
                                                                );
                        }
                        ImGui.ColorEdit4(" ESP Box Color", ref boxColor, ImGuiColorEditFlags.PickerMask | ImGuiColorEditFlags.NoInputs);
                        {
                            Config.ESPBoxColor = Color.FromArgb(
                                                    (int)(boxColor.W * 255),
                                                    (int)(boxColor.X * 255),
                                                    (int)(boxColor.Y * 255),
                                                    (int)(boxColor.Z * 255)
                                                                );
                        }

                        ImGui.ColorEdit4(" ESP Skeleton Color", ref skeletonColor, ImGuiColorEditFlags.PickerMask | ImGuiColorEditFlags.NoInputs);
                        {
                            Config.ESPSkeletonColor = Color.FromArgb(
                                                    (int)(skeletonColor.W * 255),
                                                    (int)(skeletonColor.X * 255),
                                                    (int)(skeletonColor.Y * 255),
                                                    (int)(skeletonColor.Z * 255)
                                                                );
                        }
                    }

                    ImGui.Separator();
                    break;

                case 2:

                    ImGui.Text("Misc");
                    ImGui.Separator();
                    
                    if (ImGui.CollapsingHeader("Extra Hacks"))
                    {
                       
                      

                    }
                    ImGui.Separator();
                    break;
                case 3:
                    ImGui.Text("Config");
                    ImGui.Separator();
                    Text("Miscellaneous", 0.6f);

                    ImGui.Checkbox(" Stream Mode", ref Config.StreamMode);

                    bool close = false;
                    ImGui.Checkbox(" Close Panel", ref close);
                    if (close)
                    {
                        KillProcess("HD-Adb");
                        KillProcess("HD-Player");
                        Environment.Exit(0);
                    }

                    ImGui.Separator();
                    ImGui.Text("Press 'Insert' to Show / Hide Menu");
                    ImGui.Text("FEAR MODS IMGUI MODULE");


                    ImGui.Separator();
                    break;
            }

            ImGui.EndChild();
            ImGui.End();
        }

        private int activeTab = 0;

        private void Text(string text, float opacity = 1)
        {
            var textColor = ImGui.GetColorU32(ImGuiCol.Text);
            var textColorVec = ImGui.ColorConvertU32ToFloat4(textColor);
            textColorVec.W = opacity;
            ImGui.PushStyleColor(ImGuiCol.Text, textColorVec);
            {
                ImGui.Text(text);
            }
            ImGui.PopStyleColor();
        }

        public void KillProcess(string processName)
        {
            var processes = Process.GetProcessesByName(processName);
            foreach (var process in processes)
            {
                process.Kill();
                process.WaitForExit();
            }
        }

        protected override unsafe Task PostInitialized()
        {
            StartEntityProcessing();

            ReplaceFont(config =>
            {
                var io = ImGui.GetIO();

                if (EmbeddedFonts.FontInter.Length > 0 && EmbeddedFonts.FontLucide.Length > 0)
                {
                    fixed (byte* fontPtr = EmbeddedFonts.FontInter)
                    {
                        io.Fonts.AddFontFromMemoryTTF((IntPtr)fontPtr, EmbeddedFonts.FontInter.Length, 20, config, io.Fonts.GetGlyphRangesDefault());
                    }
                    ushort[] glyphRanges = new ushort[] { Lucide.IconMin, Lucide.IconMax16, 0 };
                    config->MergeMode = 1;
                    config->OversampleH = 1;
                    config->OversampleV = 1;
                    config->PixelSnapH = 1;

                    fixed (byte* fontPtr = EmbeddedFonts.FontLucide)
                    fixed (ushort* p = &glyphRanges[0])
                    {
                        io.Fonts.AddFontFromMemoryTTF((IntPtr)fontPtr, EmbeddedFonts.FontLucide.Length, 20, config, new IntPtr(p));
                    }
                }
            });
            Style();
            return base.PostInitialized();
        }

        private void Style()
        {
            ImGuiStylePtr style = ImGui.GetStyle();

            // ========== SHAPE & SPACING ==========
            style.WindowBorderSize = 1.6f;
            style.WindowRounding = 14f;
            style.ChildRounding = 12f;
            style.FrameRounding = 10f;
            style.PopupRounding = 10f;
            style.GrabRounding = 12f;
            style.ScrollbarRounding = 12f;
            style.TabRounding = 10f;

            style.WindowPadding = new Vector2(15, 13);
            style.FramePadding = new Vector2(11, 8);
            style.ItemSpacing = new Vector2(13, 11);
            style.ItemInnerSpacing = new Vector2(9, 7);
            style.ScrollbarSize = 15f;
            style.GrabMinSize = 13f;

            var colors = style.Colors;

            // ========== BACKGROUND (AMOLED PURPLE GLASS) ==========
            colors[(int)ImGuiCol.WindowBg] = new Vector4(0.030f, 0.020f, 0.050f, 0.98f);
            colors[(int)ImGuiCol.ChildBg] = new Vector4(0.055f, 0.040f, 0.080f, 1.00f);
            colors[(int)ImGuiCol.PopupBg] = new Vector4(0.050f, 0.035f, 0.075f, 0.98f);

            // ========== NEON PURPLE PALETTE ==========
            Vector4 neonMain = new Vector4(0.85f, 0.35f, 1.00f, 1.00f);   // Main purple
            Vector4 neonDark = new Vector4(0.45f, 0.15f, 0.65f, 1.00f);   // Base purple
            Vector4 neonGlow = new Vector4(0.95f, 0.70f, 1.00f, 1.00f);   // Glow
            Vector4 neonSoft = new Vector4(0.30f, 0.10f, 0.45f, 1.00f);   // Soft hover

            // ========== TEXT ==========
            colors[(int)ImGuiCol.Text] = new Vector4(0.97f, 0.96f, 0.99f, 1f);
            colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.50f, 0.45f, 0.60f, 1f);

            // ========== BORDERS & TITLE ==========
            colors[(int)ImGuiCol.Border] = neonDark;
            colors[(int)ImGuiCol.BorderShadow] = new Vector4(0, 0, 0, 0);

            colors[(int)ImGuiCol.TitleBg] = new Vector4(0.045f, 0.030f, 0.065f, 1f);
            colors[(int)ImGuiCol.TitleBgActive] = neonDark;
            colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.045f, 0.030f, 0.065f, 0.85f);

            // ========== TABS ==========
            colors[(int)ImGuiCol.Tab] = new Vector4(0.080f, 0.055f, 0.110f, 1f);
            colors[(int)ImGuiCol.TabHovered] = neonGlow;
            colors[(int)ImGuiCol.TabActive] = neonMain;
            colors[(int)ImGuiCol.TabUnfocused] = new Vector4(0.065f, 0.045f, 0.090f, 1f);
            colors[(int)ImGuiCol.TabUnfocusedActive] = neonDark;

            // ========== FRAMES ==========
            colors[(int)ImGuiCol.FrameBg] = new Vector4(0.090f, 0.060f, 0.130f, 0.95f);
            colors[(int)ImGuiCol.FrameBgHovered] = neonSoft;
            colors[(int)ImGuiCol.FrameBgActive] = neonDark;

            // ========== BUTTONS ==========
            colors[(int)ImGuiCol.Button] = new Vector4(0.100f, 0.070f, 0.140f, 1f);
            colors[(int)ImGuiCol.ButtonHovered] = neonGlow;
            colors[(int)ImGuiCol.ButtonActive] = neonMain;

            // ========== HEADERS ==========
            colors[(int)ImGuiCol.Header] = new Vector4(0.100f, 0.070f, 0.140f, 1f);
            colors[(int)ImGuiCol.HeaderHovered] = neonGlow;
            colors[(int)ImGuiCol.HeaderActive] = neonMain;

            // ========== CHECKMARK & SLIDER ==========
            colors[(int)ImGuiCol.CheckMark] = neonMain;
            colors[(int)ImGuiCol.SliderGrab] = neonDark;
            colors[(int)ImGuiCol.SliderGrabActive] = neonGlow;

            // ========== SEPARATOR ==========
            colors[(int)ImGuiCol.Separator] = neonDark;
            colors[(int)ImGuiCol.SeparatorHovered] = neonGlow;
            colors[(int)ImGuiCol.SeparatorActive] = neonMain;

            // ========== SCROLLBAR ==========
            colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.035f, 0.020f, 0.060f, 1f);
            colors[(int)ImGuiCol.ScrollbarGrab] = neonDark;
            colors[(int)ImGuiCol.ScrollbarGrabHovered] = neonGlow;
            colors[(int)ImGuiCol.ScrollbarGrabActive] = neonMain;

            // ========== RESIZE ==========
            colors[(int)ImGuiCol.ResizeGrip] = neonDark;
            colors[(int)ImGuiCol.ResizeGripHovered] = neonGlow;
            colors[(int)ImGuiCol.ResizeGripActive] = neonMain;
        }


    }
}