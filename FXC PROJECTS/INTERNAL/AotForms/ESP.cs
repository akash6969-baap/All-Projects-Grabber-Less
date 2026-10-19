using AotForms;
using ImGuiNET;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using static AotForms.WinAPI;

namespace AotForms
{
    public static class FontManager
    {
        public static ImFontPtr SmallFont;
        public static ImFontPtr BigFont;
    }

    internal class ESP : ClickableTransparentOverlay.Overlay
    {
        int EnemyCount = 0;
        IntPtr hWnd;
        IntPtr HDPlayer;
        private const short DefaultMaxHealth = 200;
        internal static float GlowRadius = 20f;
        internal static float FeatherAmount = 2f;
        protected override unsafe void Render()
        {
            if (!Core.HaveMatrix) return;

            CreateHandle();
            string text = "[NITIN X CHEATS]";

            var windowWidth = Core.Width;
            var windowHeight = Core.Height;
            var textSize = ImGui.CalcTextSize(text);
            var textPosX = (windowWidth - textSize.X) / 2;
            var textPosY = 80;
            uint textColor = ImGui.ColorConvertFloat4ToU32(new Vector4(1.0f, 1.0f, 1.0f, 1.0f)); // White color
            uint shadowColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.0f, 0.0f, 0.0f, 0.5f)); // Shadow color (black with transparency)

            var drawList = ImGui.GetForegroundDrawList();

            // Draw the text multiple times to simulate bold effect
            var offsets = new[] { new Vector2(1, 0), new Vector2(-1, 0), new Vector2(0, 1), new Vector2(0, -1) };
            foreach (var offset in offsets)
            {
                drawList.AddText(new Vector2(textPosX + offset.X, textPosY + offset.Y), shadowColor, text);

            }

            // Draw the main text on top
            drawList.AddText(new Vector2(textPosX, textPosY), textColor, text);



            //  int playerCounter = 1;
            ////  int enemyCount = 0;


            if (Config.Aimfovc)
            {
                DrawSmoothCircle(Config.Aimfov, ColorToUint32(Config.Aimfovcolor), 1.0f);
            }

            if (Config.minimap)
            {
                DrawMinimap();
            }

            // Handle window styles
            string windowName = "Overlay";
            hWnd = FindWindow(null, windowName);
            HDPlayer = FindWindow("BlueStacksApp", null);

            if (hWnd != IntPtr.Zero)
            {
                long extendedStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
                SetWindowLong(hWnd, GWL_EXSTYLE, (extendedStyle | WS_EX_TOOLWINDOW) & ~WS_EX_APPWINDOW);
            }
            else
            {
                Console.WriteLine("The window was not found.");
            }

            var tmp = Core.Entities;
            foreach (var entity in tmp.Values)
            {
                if (entity.IsDead || !entity.IsKnown)
                {
                    continue;
                }
                var dist = Vector3.Distance(Core.LocalMainCamera, entity.Head);

                if (dist > Config.espran) continue;

                var headScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
                var bottomScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Root, Core.Width, Core.Height);

                if (headScreenPos.X < 1 || headScreenPos.Y < 1) continue;
                if (bottomScreenPos.X < 1 || bottomScreenPos.Y < 1) continue;

                float CornerHeight = Math.Abs(headScreenPos.Y - bottomScreenPos.Y);
                float CornerWidth = (float)(CornerHeight * 0.65);


                if (Config.ESPLine)
                {
                    Vector2 lineStart;

                    // Determine line start position based on LinePosition
                    switch (Config.ESPLinePosition)
                    {
                        case LinePosition.Top:
                            lineStart = new Vector2(Core.Width / 2f, 10f); // Top position
                            break;
                        case LinePosition.Center:
                            lineStart = new Vector2(Core.Width / 2f, Core.Height / 2f); // Center position
                            break;
                        case LinePosition.Bottom:
                            lineStart = new Vector2(Core.Width / 2f, Core.Height - 10f); // Bottom position
                            break;
                        default:
                            lineStart = new Vector2(Core.Width / 2f, Core.Height / 2f); // Default to center
                            break;
                    }


                    DrawGlowingBall(lineStart, Config.ESPLineColor, 3f);


                    DrawGlowLine(lineStart, headScreenPos, ColorToUint32(Config.ESPLineColor), 1f, GlowRadius, FeatherAmount);
                }



                if (Config.ESPBox)
                {
                    uint boxColor = ColorToUint32(Config.ESPBoxColor);
                    DrawCorneredBox(headScreenPos.X - (CornerWidth / 2), headScreenPos.Y, CornerWidth, CornerHeight, boxColor, 1f);
                }

                if (Config.ESPFillBox)
                {
                    Color topColor = Color.FromArgb((int)(0.1f * 255), Config.ESPFillBoxColor);
                    Color bottomColor = Color.FromArgb((int)(0.75f * 255), Config.ESPFillBoxColor);

                    DrawGradientBox(
                        headScreenPos.X - (CornerWidth / 2),
                        headScreenPos.Y,
                        CornerWidth,
                        CornerHeight,
                        topColor,
                        bottomColor
                    );
                }

                if (Config.ESPBox2)
                {
                    uint boxColor = ColorToUint32(Config.ESPBoxColor);
                    Draw3dBox(headScreenPos.X - (CornerWidth / 2), headScreenPos.Y, CornerWidth, CornerHeight, boxColor, 1f);
                }


                if (Config.espbg)
                {
                    //// Draw the black background with 50% opacity
                    //drawList.AddRectFilled(
                    //    namePosition - new Vector2(10, 4), // Padding for background
                    //    namePosition + nameSize + new Vector2(6, 4), // Padding for background
                    //    ColorToUint32(Color.FromArgb((int)(0.8f * 255), 0, 0, 0)) // Black with 50% opacity
                    //);

                    //// Draw the red stroke (border)
                    //drawList.AddRect(
                    //    namePosition - new Vector2(10, 4), // Padding for stroke
                    //    namePosition + nameSize + new Vector2(6, 4), // Padding for stroke
                    //    ColorToUint32(Color.FromArgb((int)(1f * 255), 100, 0, 0)), // Red stroke
                    //    0.2f, // No rounding on corners
                    //    ImDrawFlags.None,
                    //    0.05f // Stroke thickness
                    //);
                }

                var nameText = string.IsNullOrWhiteSpace(entity.Name) ? "BOT" : entity.Name;
                var namePosition = new Vector2(headScreenPos.X - (CornerWidth / 2), headScreenPos.Y - 30);
                var nameSize = ImGui.CalcTextSize($"      {MathF.Round(dist)}M" + nameText);
                Vector2 fixedNameSize = new Vector2(95, 16);


                if (entity.Name == "")
                    entity.Name = "Bot";
                if (headScreenPos.X >= 0 && headScreenPos.Y >= 0 && headScreenPos.X <= Core.Width && headScreenPos.Y <= Core.Height)
                {
                    Vector2 namePos = new Vector2(headScreenPos.X - fixedNameSize.X / 2, headScreenPos.Y - fixedNameSize.Y - 15);



                    Vector2 textSizeName = ImGui.CalcTextSize(entity.Name);

                    Vector2 textSizeDistance = ImGui.CalcTextSize($" ({MathF.Round(Vector3.Distance(Core.LocalMainCamera, entity.Head))}m)");

                    Vector2 textPosName = new Vector2(namePos.X + 5, namePos.Y + (fixedNameSize.Y - textSizeName.Y) / 2);
                    Vector2 textPosDistance = new Vector2(namePos.X + fixedNameSize.X - textSizeDistance.X + 5, namePos.Y + (fixedNameSize.Y - textSizeDistance.Y) / 2);


                    if (Config.ESPName)
                    {
                        ImGui.GetForegroundDrawList().AddRectFilled(namePos, namePos + new Vector2(fixedNameSize.X, fixedNameSize.Y), ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 0.7f)), 3f);
                        ImGui.GetForegroundDrawList().AddText(textPosName, ColorToUint32(Config.ESPNameColor), entity.Name);
                        ImGui.GetForegroundDrawList().AddText(textPosDistance, ColorToUint32(Config.ESPNameColor), $" {MathF.Round(Vector3.Distance(Core.LocalMainCamera, entity.Head))}m");
                    }


                    if (Config.ESPDistance)
                    {
                        // Create the distance string
                        string distanceText = $"{MathF.Round(dist)} M";

                        // Estimate text width (average character width = 8px)
                        float estimatedTextWidth = distanceText.Length * 8f;

                        // Center the text based on screen position
                        Vector2 distancePosition = new Vector2(bottomScreenPos.X + 1 - (estimatedTextWidth / 2), bottomScreenPos.Y + 15f);

                        // Draw the distance text directly (yellow or any other configured color)
                        ImGui.GetForegroundDrawList().AddText(distancePosition, ColorToUint32(Config.ESPLineColor), distanceText);
                    }


                    var vList = ImGui.GetForegroundDrawList();
                    if (Config.ESPHealth)
                    {
                        Vector2 healthBarPosRight = new Vector2(headScreenPos.X + (CornerWidth / 2) + 5, headScreenPos.Y);

                        float healthPercentage = entity.Health > 1000 ? 1f :
                            entity.Health < 0 ? 1f :
                            (float)entity.Health / (entity.Health > 230 ? 500 : 200);

                        float healthBarWidth = 3;  // Thinner health bar
                        float healthBarFullHeight = CornerHeight;

                        uint healthBarColor;
                        if (entity.IsKnocked)
                        {
                            healthBarColor = ColorToUint32(Color.Red); // Full red when knocked
                        }
                        else
                        {
                            if (healthPercentage > 0.8f)
                                healthBarColor = ColorToUint32(Color.Lime); // Bright lime green
                            else if (healthPercentage > 0.4f)
                                healthBarColor = ColorToUint32(Color.Yellow); // Bright yellow
                            else
                                healthBarColor = ColorToUint32(Color.Red); // Bright red
                        }

                        // Draw the outline (black) with rounded corners
                        vList.AddRect(new Vector2(healthBarPosRight.X - 1, healthBarPosRight.Y - 1),
                                      new Vector2(healthBarPosRight.X + healthBarWidth + 1, healthBarPosRight.Y + healthBarFullHeight + 1),
                                      0xFF000000, // Black color for the outline
                                      1.0f);      // Radius for rounded corners

                        // Draw the background bar with radius
                        vList.AddRectFilled(new Vector2(healthBarPosRight.X, healthBarPosRight.Y),
                                            new Vector2(healthBarPosRight.X + healthBarWidth, healthBarPosRight.Y + healthBarFullHeight),
                                            0x90000000, // Semi-transparent black for background
                                            1.0f);      // Radius for rounded corners

                        // Draw the health bar with radius
                        vList.AddRectFilled(new Vector2(healthBarPosRight.X, healthBarPosRight.Y + healthBarFullHeight * (1 - healthPercentage)),
                                            new Vector2(healthBarPosRight.X + healthBarWidth, healthBarPosRight.Y + healthBarFullHeight),
                                            healthBarColor,
                                            1.0f);      // Radius for rounded corners
                                                        //// Define fixed size for the health bar
                                                        //float healthBarHeight = 6; // Fixed height
                                                        //float healthBarWidth = nameSize.X + 16; // Match width to the name text

                        //// Position of the health bar above the ESP name with a clear gap
                        //var healthBarPosition = new Vector2(namePosition.X - 10, namePosition.Y - healthBarHeight + 26); // Increase gap to avoid collapsing
                        //if (!entity.IsKnocked)
                        //{

                        //    DrawHealthBar(entity.Health, DefaultMaxHealth, healthBarPosition.X, healthBarPosition.Y, healthBarHeight, healthBarWidth);
                        //}
                        //else
                        //{
                        //    DrawHealthBarK(entity.Health, DefaultMaxHealth, healthBarPosition.X, healthBarPosition.Y, healthBarHeight, healthBarWidth);
                        //}

                    }

                    if (Config.CrossHair)
                    {
                        float rotationAngle = (float)DateTime.Now.TimeOfDay.TotalSeconds * 2f % (2 * (float)Math.PI); // Ensures angle stays within 0 to 2π
                        var size = 25f;
                        var clr = GetESPColor(ColorToUint32(Config.CrossHairColor));

                        Vector2 center = new Vector2(Core.Width / 2f, Core.Height / 2f);


                        // Draw circle at the center
                        ImGui.GetForegroundDrawList().AddCircle(center, size / 2f, clr, 64, 1f); // Circle with 64 segments

                        // Calculate line endpoints using rotation
                        float cosTheta = (float)Math.Cos(rotationAngle);
                        float sinTheta = (float)Math.Sin(rotationAngle);

                        // Horizontal line (rotated)
                        Vector2 startH = new Vector2(center.X - size / 2f * cosTheta, center.Y - size / 2f * sinTheta);
                        Vector2 endH = new Vector2(center.X + size / 2f * cosTheta, center.Y + size / 2f * sinTheta);
                        ImGui.GetForegroundDrawList().AddLine(startH, endH, clr, 1f);

                        // Vertical line (rotated)
                        Vector2 startV = new Vector2(center.X - size / 2f * sinTheta, center.Y + size / 2f * cosTheta);
                        Vector2 endV = new Vector2(center.X + size / 2f * sinTheta, center.Y - size / 2f * cosTheta);
                        ImGui.GetForegroundDrawList().AddLine(startV, endV, clr, 1f);


                        ImGui.GetForegroundDrawList().AddLine(startH, endH, clr, 1f);
                        ImGui.GetForegroundDrawList().AddLine(startV, endV, clr, 1f);
                    }




                    if (Config.ESPSkeleton)
                    {
                        DrawSkeleton(entity);
                    }

                }
            }
        }

        public void DrawFilledBox(float X, float Y, float W, float H, uint color)
        {
            var vList = ImGui.GetForegroundDrawList();
            vList.AddRectFilled(new Vector2(X, Y), new Vector2(X + W, Y + H), color);
        }

        public void DrawGlowingBall(Vector2 position, Color color, float radius)
        {
            var drawList = ImGui.GetForegroundDrawList();
            uint ballColor = ColorToUint32(color);

            // Outer glow layers
            for (int i = 0; i < 5; i++)
            {
                float glowRadius = radius + (i * 2); // Increase radius for glow layers
                float alpha = 1.0f - (i * 0.2f);    // Gradually reduce alpha for each layer

                drawList.AddCircleFilled(
                    position,
                    glowRadius,
                    ImGui.ColorConvertFloat4ToU32(new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, alpha)),
                    50 // Number of segments for smooth circle
                );
            }

            // Inner solid circle
            drawList.AddCircleFilled(position, radius, ballColor, 50);
        }

        public void DrawGradientBox(float X, float Y, float W, float H, Color topColor, Color bottomColor)
        {
            var vList = ImGui.GetForegroundDrawList();

            int slices = 50; // Number of slices for gradient
            float sliceHeight = H / slices;

            for (int i = 0; i < slices; i++)
            {
                float t = (float)i / slices; // Interpolation factor
                Color sliceColor = Color.FromArgb(
                    (int)(topColor.A * (1 - t) + bottomColor.A * t), // Interpolating opacity
                    (int)(topColor.R * (1 - t) + bottomColor.R * t), // Interpolating Red
                    (int)(topColor.G * (1 - t) + bottomColor.G * t), // Interpolating Green
                    (int)(topColor.B * (1 - t) + bottomColor.B * t)  // Interpolating Blue
                );

                uint sliceColorUint = ColorToUint32(sliceColor);

                // Draw each slice
                vList.AddRectFilled(
                    new Vector2(X, Y + i * sliceHeight),
                    new Vector2(X + W, Y + (i + 1) * sliceHeight),
                    sliceColorUint
                );
            }
        }

        private void DrawMinimap()
        {
            const int detectionRange = 250;
            const float baseRadius = 100f;
            const float playerMarkerSize = 5.0f;
            const float enemyMarkerSize = 4.0f;
            const float margin = 20f;

            float minimapRadius = baseRadius * (detectionRange / 250f);
            Vector2 minimapCenter = new Vector2(margin + minimapRadius, Core.Height - minimapRadius - margin);

            float cameraYaw = -GetCameraYaw(); // Negative to match world rotation
            float cosYaw = MathF.Cos(cameraYaw);
            float sinYaw = MathF.Sin(cameraYaw);

            ImDrawListPtr drawList = ImGui.GetBackgroundDrawList();

            // Background + Border
            drawList.AddCircleFilled(minimapCenter, minimapRadius, ColorToUint32(Color.FromArgb(150, 15, 15, 15)));
            drawList.AddCircle(minimapCenter, minimapRadius, ColorToUint32(Color.White), 64, 1.0f);

            // Grid Circles
            uint gridColor = ColorToUint32(Color.FromArgb(40, 255, 255, 255));
            for (int i = 1; i <= 3; i++)
            {
                drawList.AddCircle(minimapCenter, minimapRadius * i / 3f, gridColor, 64, 0.5f);
            }

            // Compass letters (rotated with yaw)
            string[] compass = { "N", "E", "S", "W" };
            for (int i = 0; i < 4; i++)
            {
                float angle = -MathF.PI / 2 + i * MathF.PI / 2 + cameraYaw; // rotate compass labels
                Vector2 pos = minimapCenter + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (minimapRadius - 12f);
                drawList.AddText(pos - new Vector2(4, 4), ColorToUint32(Color.White), compass[i]);
            }

            // Player center + arrow (fixed upward)
            drawList.AddCircleFilled(minimapCenter, playerMarkerSize, ColorToUint32(Color.Cyan));
            Vector2 arrowTip = minimapCenter + new Vector2(0, -1) * (playerMarkerSize + 6f);
            drawList.AddLine(minimapCenter, arrowTip, ColorToUint32(Color.Cyan), 1.5f);

            // Count detected enemies
            int detectedEnemies = Core.Entities.Values.Count(e =>
                !e.IsDead && Vector3.Distance(Core.LocalMainCamera, e.Head) <= detectionRange);

            // Text above minimap
            string detectedText = $"Detected: {detectedEnemies}";
            ImGui.PushFont(FontManager.BigFont);
            Vector2 textSize = ImGui.CalcTextSize(detectedText);
            Vector2 textPos = new Vector2(minimapCenter.X - textSize.X / 2, minimapCenter.Y - minimapRadius - textSize.Y - 10);

            // Shadow text
            uint shadowColor = ColorToUint32(Color.FromArgb(180, 0, 0, 0));
            Vector2[] offsets = { new Vector2(-1, -1), new Vector2(1, -1), new Vector2(-1, 1), new Vector2(1, 1) };
            foreach (var offset in offsets)
                drawList.AddText(textPos + offset, shadowColor, detectedText);

            drawList.AddText(textPos, ColorToUint32(Color.White), detectedText);
            ImGui.PopFont();

            // Draw enemies on minimap
            foreach (var entity in Core.Entities.Values)
            {
                if (entity.IsDead) continue;

                float distance = Vector3.Distance(Core.LocalMainCamera, entity.Head);
                if (distance > detectionRange) continue;

                Vector3 relative = entity.Head - Core.LocalMainCamera;

                // Apply map rotation
                float rotatedX = relative.X * cosYaw - relative.Z * sinYaw;
                float rotatedY = relative.X * sinYaw + relative.Z * cosYaw;

                float scale = minimapRadius / detectionRange;
                Vector2 minimapPos = new Vector2(
                    minimapCenter.X + rotatedX * scale,
                    minimapCenter.Y - rotatedY * scale
                );

                if (Vector2.Distance(minimapCenter, minimapPos) <= minimapRadius)
                {
                    uint color = entity.IsKnown
                        ? (entity.IsKnocked ? ColorToUint32(Color.Yellow) : ColorToUint32(Color.Red))
                        : ColorToUint32(Color.Blue);

                    drawList.AddCircleFilled(minimapPos, enemyMarkerSize, color);
                }
            }
        }


        private float GetCameraYaw()
        {
            return MathF.Atan2(Core.CameraMatrix.M31, Core.CameraMatrix.M33);
        }


        public void DrawFilledCircle(float centerY, float radius, int numSegments = 64)
        {
            var vList = ImGui.GetBackgroundDrawList();

            // Set the center of the circle at the middle of the screen horizontally (Core.Width / 2f)

            float centerX = Core.Width / 2f;

            uint colorR = ColorToUint32(Color.FromArgb((int)(1f * 255), 225, 0, 0)); // Red color with full opacity
            uint colorG = ColorToUint32(Color.FromArgb((int)(1f * 255), 0, 255, 0)); // LimeGreen color with full opacity

            // Shadow parameters
            float shadowOffset = 1.5f; // The subtle offset of the shadow from the circle
            uint shadowColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 1f)); // Semi-transparent black for a soft shadow

            // Draw shadow (a larger circle slightly offset behind the main one)
            vList.AddCircleFilled(new Vector2(centerX, centerY), radius + shadowOffset, shadowColor, numSegments);

            if (Config.AimBot)
            {
                // Draw main circle
                vList.AddCircleFilled(new Vector2(centerX, centerY), radius, colorR, numSegments);
            }
            else
            {
                // Draw main circle
                vList.AddCircleFilled(new Vector2(centerX, centerY), radius, colorG, numSegments);
            }
        }
        public void DrawCorneredBox(float X, float Y, float W, float H, uint color, float thickness)
        {
            var vList = ImGui.GetForegroundDrawList();

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

        public void Draw3dBox(float X, float Y, float W, float H, uint color, float thickness)
        {

            var vList = ImGui.GetForegroundDrawList();

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

        public void DrawGlowLine(Vector2 start, Vector2 end, uint color, float thickness, float glowRadius, float feather)
        {
            var drawList = ImGui.GetBackgroundDrawList();
            Vector4 colorVec = ImGui.ColorConvertU32ToFloat4(color);

            // Outer glow layers
            for (float i = glowRadius; i > 0; i -= feather)
            {
                // Calculate alpha for the outer layers based on the current radius and feathering
                float alpha = colorVec.W * (i / glowRadius) * 0.02f; // Adjust glow strength with 0.6f factor
                uint glowColor = ImGui.ColorConvertFloat4ToU32(new Vector4(colorVec.X, colorVec.Y, colorVec.Z, alpha));

                // Draw each outer layer with increasing thickness to create a soft glow effect
                drawList.AddLine(start, end, glowColor, thickness + i);
            }

            // Main line at the center
            drawList.AddLine(start, end, color, thickness);
        }

        private void DrawSkeleton(Entity entity)
        {
            var drawList = ImGui.GetForegroundDrawList();
            uint lineColor = ColorToUint32(Config.ESPSkeletonColor); // Color for the skeleton lines
            uint circleColor = ColorToUint32(Color.Red); // Color for the circle around the head

            // Convert entity positions to screen space
            var headScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
            var leftWristScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightWrist, Core.Width, Core.Height); // Adjust as per actual mapping
            var spineScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Spine, Core.Width, Core.Height);
            var hipScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Hip, Core.Width, Core.Height); // Adjust as per actual mapping
            var rootScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Root, Core.Width, Core.Height);
            var rightCalfScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightCalf, Core.Width, Core.Height);
            var leftCalfScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftCalf, Core.Width, Core.Height);
            var rightFootScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightFoot, Core.Width, Core.Height);
            var leftFootScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftFoot, Core.Width, Core.Height);
            var rightWristScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightWrist, Core.Width, Core.Height);
            var leftHandScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftHand, Core.Width, Core.Height);
            var leftShoulderScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftSholder, Core.Width, Core.Height);
            var rightShoulderScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightSholder, Core.Width, Core.Height);
            var rightWristJointScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightWristJoint, Core.Width, Core.Height);
            var leftWristJointScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftWristJoint, Core.Width, Core.Height);
            var leftElbowScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftElbow, Core.Width, Core.Height);
            var rightElbowScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightElbow, Core.Width, Core.Height); // Adjust if needed

            // Draw skeleton lines


            DrawLine(drawList, spineScreenPos, rightShoulderScreenPos, lineColor); // Spine to Right Shoulder
            DrawLine(drawList, spineScreenPos, hipScreenPos, lineColor);// Spine to hip


            DrawLine(drawList, spineScreenPos, leftShoulderScreenPos, lineColor); // Spine to Left Shoulder
            DrawLine(drawList, leftShoulderScreenPos, rightElbowScreenPos, lineColor); // Left Shoulder to Left Elbow
            DrawLine(drawList, leftElbowScreenPos, rightWristJointScreenPos, lineColor); // Left Elbow to Left Wrist Joint
            // Left Wrist Joint to Left Wrist

            DrawLine(drawList, rightShoulderScreenPos, leftElbowScreenPos, lineColor); // Right Shoulder to Left Elbow
                                                                                       //  DrawLine(drawList, rightElbowScreenPos, leftWristJointScreenPos, lineColor); // Right Elbow to Left Wrist Joint
                                                                                       // Right Wrist Joint to Left Wrist

            DrawLine(drawList, hipScreenPos, rightFootScreenPos, lineColor);// Hip to Right Calf
            DrawLine(drawList, hipScreenPos, leftFootScreenPos, lineColor);// Hip to Left Calf


            // Draw a small circle around the head
            float distance = entity.Distance; // Assume entity.Distance is the distance to the player in game units

            // Calculate the circle radius based on distance (e.g., closer = larger, farther = smaller)
            float baseRadius = 50.0f; // Adjust this base value as needed
            float circleRadius = baseRadius / distance;

            // Draw the circle on the head if the head is visible on screen
            if (headScreenPos.X > 0 && headScreenPos.Y > 0)
            {
                drawList.AddCircle(headScreenPos, circleRadius, circleColor, 30); // 30 segments for the circle
            }

            // Add additional code here to draw the rest of the skeleton using the updated bone positions
        }
        private void DrawLine(ImDrawListPtr drawList, Vector2 startPos, Vector2 endPos, uint color)
        {
            if (startPos.X > 0 && startPos.Y > 0 && endPos.X > 0 && endPos.Y > 0)
            {
                drawList.AddLine(startPos, endPos, color, 1.5f); // Adjust thickness as needed
            }
        }

        public void DrawSmoothCircle(float radius, uint color, float thickness, int segments = 64)
        {
            var vList = ImGui.GetForegroundDrawList();
            var io = ImGui.GetIO();
            float centerX = io.DisplaySize.X / 2;
            float centerY = io.DisplaySize.Y / 2;

            vList.AddCircle(new Vector2(centerX, centerY), radius, color, segments, thickness);
        }

        public void DrawHealthBarK(short health, short maxHealth, float X, float Y, float height, float width)
        {
            var vList = ImGui.GetForegroundDrawList();

            // Prevent division by zero and ensure healthPercentage is between 0 and 1
            if (maxHealth <= 0) maxHealth = 100; // Fallback to a default max health
            float healthPercentage = Math.Clamp((float)health / maxHealth, 0f, 1f);
            float healthWidth = width * healthPercentage;

            // Determine the color based on health percentage
            Color healthColor;


            if (healthPercentage < 0.3f)
            {
                healthColor = Color.FromArgb((int)(1f * 255), 255, 0, 0); // Red for health < 20%
            }
            else if (healthPercentage < 0.8f)
            {
                healthColor = Color.FromArgb((int)(1f * 255), 255, 0, 0); // Yellow for health < 70%
            }
            else
            {
                healthColor = Color.FromArgb((int)(1f * 255), 255, 0, 0); // Green for health >= 70%
            }

            // Draw the full health bar background (unfilled part)
            vList.AddRectFilled(new Vector2(X, Y - height), new Vector2(X + width, Y), ColorToUint32(Color.FromArgb((int)(1f * 255), 99, 0, 0))); // Background for health bar

            // Draw the health portion representing current health
            vList.AddRectFilled(new Vector2(X, Y - height), new Vector2(X + healthWidth, Y), ColorToUint32(healthColor)); // Health portion

            // Draw the black outline around the health bar
            vList.AddRect(new Vector2(X, Y - height), new Vector2(X + width, Y), ColorToUint32(Color.Black), 1f); // Black outline
        }


        public void DrawHealthBar(short health, short maxHealth, float X, float Y, float height, float width)
        {
            var vList = ImGui.GetForegroundDrawList();

            // Prevent division by zero and ensure healthPercentage is between 0 and 1
            if (maxHealth <= 0) maxHealth = 100; // Fallback to a default max health
            float healthPercentage = Math.Clamp((float)health / maxHealth, 0f, 1f);
            float healthWidth = width * healthPercentage;

            // Determine the color based on health percentage
            Color healthColor;


            if (healthPercentage < 0.3f)
            {
                healthColor = Color.FromArgb((int)(1f * 255), 255, 0, 0); // Red for health < 20%
            }
            else if (healthPercentage < 0.8f)
            {
                healthColor = Color.FromArgb((int)(1f * 255), 255, 255, 0); // Yellow for health < 70%
            }
            else
            {
                healthColor = Color.FromArgb((int)(1f * 255), 86, 255, 43); // Green for health >= 70%
            }

            // Draw the full health bar background (unfilled part)
            vList.AddRectFilled(new Vector2(X, Y - height), new Vector2(X + width, Y), ColorToUint32(Color.FromArgb((int)(1f * 255), 99, 0, 0))); // Background for health bar

            // Draw the health portion representing current health
            vList.AddRectFilled(new Vector2(X, Y - height), new Vector2(X + healthWidth, Y), ColorToUint32(healthColor)); // Health portion

            // Draw the black outline around the health bar
            vList.AddRect(new Vector2(X, Y - height), new Vector2(X + width, Y), ColorToUint32(Color.Black), 1f); // Black outline
        }
        static uint ColorToUint32(Color color)
        {
            return ImGui.ColorConvertFloat4ToU32(new Vector4(
            (float)(color.R / 255.0),
                (float)(color.G / 255.0),
                (float)(color.B / 255.0),
                (float)(color.A / 255.0)));
        }
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);


        const uint WDA_NONE = 0x00000000;
        const uint WDA_MONITOR = 0x00000001;
        const uint WDA_EXCLUDEFROMCAPTURE = 0x00000011;
        private static T GetESPColor<T>(T defaultColor)
        {
            if (Config.RGB)
            {
                if (typeof(T) == typeof(uint))
                {
                    return (T)(object)LGBT(); // Return uint when needed
                }
                else if (typeof(T) == typeof(Color))
                {
                    return (T)(object)LGBTColor(); // Return Color when needed
                }
            }
            return defaultColor;
        }
        private static uint LGBT()
        {
            float time = (float)ImGui.GetTime();
            Color col = ColorFromHSV(time * 100 % 360, 1.0f, 1.0f);
            return ColorToUint32(col); // Convert RGB color to Uint32
        }
        private static Color LGBTColor()
        {
            float time = (float)ImGui.GetTime();
            return ColorFromHSV(time * 100 % 360, 1.0f, 1.0f);
        }

        public static Color ColorFromHSV(float hue, float saturation, float value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            float f = (float)((hue / 60) - Math.Floor(hue / 60)); float v = value * 255; float p = v * (1 - saturation); float q = v * (1 - f * saturation); float t = v * (1 - (1 - f) * saturation);
            switch (hi) { case 0: return Color.FromArgb(255, (int)v, (int)t, (int)p); case 1: return Color.FromArgb(255, (int)q, (int)v, (int)p); case 2: return Color.FromArgb(255, (int)p, (int)v, (int)t); case 3: return Color.FromArgb(255, (int)p, (int)q, (int)v); case 4: return Color.FromArgb(255, (int)t, (int)p, (int)v); default: return Color.FromArgb(255, (int)v, (int)p, (int)q); }
        }
        void CreateHandle()
        {
            RECT rect;
            GetWindowRect(Core.Handle, out rect);
            int x = rect.Left;
            int y = rect.Top;
            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;
            ImGui.SetWindowSize(new Vector2((float)width, (float)height));
            ImGui.SetWindowPos(new Vector2((float)x, (float)y));
            Size = new Size(width, height);
            Position = new Point(x, y);

            Core.Width = width;
            Core.Height = height;
            if (Config.StreamMode)
            {
                SetWindowDisplayAffinity(hWnd, WDA_EXCLUDEFROMCAPTURE);
            }
            else
            {
                SetWindowDisplayAffinity(hWnd, WDA_NONE);
            }
        }
    }
    public struct Bone
    {
        public string Name;
        public int Offset;
    }
}




