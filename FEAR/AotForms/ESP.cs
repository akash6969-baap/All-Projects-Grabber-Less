using AotForms;
using ImGuiNET;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Policy;
using static AotForms.WinAPI;


namespace AotForms
{
    internal class ESP : ClickableTransparentOverlay.Overlay
    {



        private System.Threading.Timer rotationTimer;
        private float angle = 0f;


        IntPtr hWnd;









        IntPtr HDPlayer;
        private const short DefaultMaxHealth = 200; // Default maximum health

        public void DrawGradientBox(float X, float Y, float W, float H, Color topColor, Color bottomColor)
        {
            var vList = ImGui.GetForegroundDrawList();
            int slices = 50;
            float sliceHeight = H / slices;

            for (int i = 0; i < slices; i++)
            {
                float t = (float)i / slices;
                Color sliceColor = Color.FromArgb(
                    (int)(topColor.A * (1 - t) + bottomColor.A * t),
                    (int)(topColor.R * (1 - t) + bottomColor.R * t),
                    (int)(topColor.G * (1 - t) + bottomColor.G * t),
                    (int)(topColor.B * (1 - t) + bottomColor.B * t)
                );
                vList.AddRectFilled(
                    new Vector2(X, Y + i * sliceHeight),
                    new Vector2(X + W, Y + (i + 1) * sliceHeight),
                    ColorToUint32(sliceColor)
                );
            }
        }

        public static float GlowIntensity { get; set; } = 0.20f;
        public void DrawGlowLine(Vector2 start, Vector2 end, uint color, float thickness, float glowRadius, float feather, float glowIntensity)
        {
            var drawList = ImGui.GetForegroundDrawList();
            Vector4 colorVec = ImGui.ColorConvertU32ToFloat4(color);

            // Outer glow layers
            for (float i = glowRadius; i > 0; i -= feather)
            {
                float alpha = colorVec.W * (i / glowRadius) * GlowIntensity; // Adjust glow strength
                uint glowColor = ImGui.ColorConvertFloat4ToU32(new Vector4(colorVec.X, colorVec.Y, colorVec.Z, alpha));
                drawList.AddLine(start, end, glowColor, thickness + i);
            }

            // Main line at the center
            drawList.AddLine(start, end, color, thickness);
        }
       
        
       
        

        protected override unsafe void Render()
        {
            if (!Core.HaveMatrix) return;

            CreateHandle();

            // Texts
            string textTop = "FEAR X CORPORATION";
            string textBottom = "Dev </> !!FEAR.NET";
            string pinIcon = "📌"; // optional pin emoji

            // Colors
            uint greenColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.0f, 1.0f, 0.0f, 1.0f)); // Green
            uint redColor = ImGui.ColorConvertFloat4ToU32(new Vector4(1.0f, 0.0f, 0.0f, 1.0f)); // Red
            uint pinColor = ImGui.ColorConvertFloat4ToU32(new Vector4(1.0f, 1.0f, 0.0f, 1.0f)); // Yellow pin
            uint shadowColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 0.5f));         // Shadow

            var windowWidth = Core.Width;

            // Measure text sizes
            var topSize = ImGui.CalcTextSize(textTop);
            var bottomSize = ImGui.CalcTextSize(textBottom);
            var pinSize = ImGui.CalcTextSize(pinIcon);

            // Center X positions
            float topX = (windowWidth - topSize.X) / 2f;
            float bottomX = (windowWidth - bottomSize.X) / 2f;
            float pinX = topX - pinSize.X - 5f; // pin left of top text

            // Y positions
            float topY = 80f;
            float bottomY = topY + topSize.Y + 5f;
            float pinY = topY;

            // Get overlay draw list
            ImDrawListPtr drawList = ImGui.GetForegroundDrawList();

            // --- Shadow / Bold Effect ---
            Vector2[] offsets = new Vector2[]
            {
    new Vector2(1,0),
    new Vector2(-1,0),
    new Vector2(0,1),
    new Vector2(0,-1)
            };

            // Draw shadows for top text
            foreach (var offset in offsets)
            {
                drawList.AddText(new Vector2(topX + offset.X, topY + offset.Y), shadowColor, textTop);
            }

            // Draw shadows for bottom text
            foreach (var offset in offsets)
            {
                drawList.AddText(new Vector2(bottomX + offset.X, bottomY + offset.Y), shadowColor, textBottom);
            }

            // Draw shadow for pin
            foreach (var offset in offsets)
            {
                drawList.AddText(new Vector2(pinX + offset.X, pinY + offset.Y), shadowColor, pinIcon);
            }

            // --- Draw main texts ---
            drawList.AddText(new Vector2(topX, topY), greenColor, textTop);
            drawList.AddText(new Vector2(bottomX, bottomY), redColor, textBottom);
            drawList.AddText(new Vector2(pinX, pinY), pinColor, pinIcon);




            if (Config.Aimfovc)
            {
                DrawSmoothCircle(Config.Aimfov, ColorToUint32(Config.Aimfovcolor), 1.0f);
            }



            var tmp = Core.Entities;

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

            int enemyCount = 0; // Initialize enemy count

            foreach (var entity in tmp.Values)
            {

                if (entity.IsDead || !entity.IsKnown)
                {
                    continue;
                }
                var dist = Vector3.Distance(Core.LocalMainCamera, entity.Head);

                if (dist > Config.espran) continue;

                var headScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
                enemyCount++; // Increment the enemy count
                var bottomScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Root, Core.Width, Core.Height);

                if (headScreenPos.X < 1 || headScreenPos.Y < 1) continue;
                if (bottomScreenPos.X < 1 || bottomScreenPos.Y < 1) continue;

                float CornerHeight = Math.Abs(headScreenPos.Y - bottomScreenPos.Y);
                float CornerWidth = (float)(CornerHeight * 0.65);

                if (Config.ESPLine)
                {





                    uint linecolor = entity.IsKnocked ?
    ColorToUint32(Color.Red) :
    (Config.ESPLine ? ColorToUint32(Color.White) : ColorToUint32(Config.ESPLineColor));


                    Vector2 startPos = Config.espcfx ?
                        new Vector2(Core.Width / 2f, Core.Height - 35f) :
                        new Vector2(Core.Width / 2f, 35f);




                    DrawGlowLine(
                        start: startPos,
                        end: headScreenPos,
                        color: linecolor,
                        thickness: 1.0f,
                        glowRadius: Config.GlowRadius,
                        feather: Config.FeatherAmount,
                        glowIntensity: ESP.GlowIntensity

                    );




                }

                if (Config.ESPBox)
                {
                    //uint boxColor = ColorToUint32(Config.ESPBoxColor);
                    //DrawCorneredBox(headScreenPos.X - (CornerWidth / 2), headScreenPos.Y, CornerWidth, CornerHeight, boxColor, 1f);

                    uint boxColor = ColorToUint32(Config.ESPBoxColor);
                    ImGui.GetForegroundDrawList().AddRect(
                        new Vector2(headScreenPos.X - (CornerWidth / 2), headScreenPos.Y),
                        new Vector2(headScreenPos.X + (CornerWidth / 2), headScreenPos.Y + CornerHeight),
                        boxColor,
                        0f, // Sem arredondamento
                        ImDrawFlags.None,
                        1f // Espessura
                    );
                }

                


                if (Config.minimap)
                {
                    DrawMiniMap();
                }

                if (Config.ESPFillBox)
                {
                    DrawGradientBox(
                        headScreenPos.X - (CornerWidth / 2),
                        headScreenPos.Y,
                        CornerWidth,
                        CornerHeight,
                        Color.FromArgb(25, Config.ESPFillBoxColor),
                        Color.FromArgb(200, Config.ESPFillBoxColor)
                    );
                }
                





                if (Config.ESPBox2)
                {
                    uint boxColor = ColorToUint32(Config.ESPBoxColor);
                    Draw3dBox(headScreenPos.X - (CornerWidth / 2), headScreenPos.Y, CornerWidth, CornerHeight, boxColor, 1f);
                }

                var nameText = string.IsNullOrWhiteSpace(entity.Name) ? "BOT" : entity.Name;
                var namePosition = new Vector2(headScreenPos.X - (CornerWidth / 2), headScreenPos.Y - 30);
                var nameSize = ImGui.CalcTextSize($"      {MathF.Round(dist)}M" + nameText);


                string namefuck = $"";
                float namefuckTextWidths = namefuck.Length * 6f;
                var namePosition1 = new Vector2(headScreenPos.X - (CornerWidth / 2), headScreenPos.Y - 20);
                string customName = $"{nameText}";

                if (Config.ESPName)
                {
                    string distanceText = $"{dist.ToString("F1")}m"; // 1 decimal place

                    float estimatedTextWidth = distanceText.Length * 6f;

                    Vector2 distancePosition = new Vector2(bottomScreenPos.X + 1 - (estimatedTextWidth / 2), bottomScreenPos.Y + 5f);

                    ImGui.GetForegroundDrawList().AddText(distancePosition, fc(Color.White), distanceText);

                    ImGui.GetForegroundDrawList().AddText(namePosition1, fc(Config.ESPNameColor), customName);
                }
                static uint fc(Color color)
                {
                    return ImGui.ColorConvertFloat4ToU32(new Vector4(
                        (float)(color.R / 255.0),
                        (float)(color.G / 255.0),
                        (float)(color.B / 255.0),
                        (float)(color.A / 255.0)));
                }


                if (Config.ESPHealth)
                {
                    DrawHealthBar(entity.Health, 200, headScreenPos.X - (CornerWidth / 2) - 5, headScreenPos.Y, CornerHeight);

                }












                if (Config.ESPSkeleton)
                {
                    DrawSkeleton(entity);
                    var bones = new Bone[]
                   {
                        new Bone { Name = "Head", Offset = 0x36c},
                        new Bone { Name = "Hip", Offset = 0x37c },
                        new Bone { Name = "RightShoulder", Offset = 0x3a4},
                        new Bone { Name = "LeftShoulder", Offset = 0x3a0},
                        new Bone { Name = "RightHand", Offset = 0x338 },
                        new Bone { Name = "LeftHand", Offset =  0x398},

                   };

                    foreach (var bone in bones)
                    {
                        var bonePosition = W2S.WorldToScreen(Core.CameraMatrix, entity.GetBonePosition(bone.Offset), Core.Width, Core.Height);

                        if (bonePosition.X < 1 || bonePosition.Y < 1) continue;

                        // Desenhar a linha que conecta os ossos
                        ImGui.GetBackgroundDrawList().AddLine(
                            bonePosition,
                            W2S.WorldToScreen(Core.CameraMatrix, entity.GetBonePosition(bones[(Array.IndexOf(bones, bone) + 1) % bones.Length].Offset), Core.Width, Core.Height),
                            ColorToUint32(Config.ESPSkeletonColor),
                            1f
                        );
                    }
                }





                // Draw shadow for "Total Players" text

            }
        }

        public void DrawFilledBox(float X, float Y, float W, float H, uint color)
        {
            var vList = ImGui.GetForegroundDrawList();
            vList.AddRectFilled(new Vector2(X, Y), new Vector2(X + W, Y + H), color);
        }

        public void DrawFilledCircle(float centerY, float radius, int numSegments = 66)
        {
            var vList = ImGui.GetBackgroundDrawList();

            // Set the center of the circle at the middle of the screen horizontally (Core.Width / 2f)
            float centerX = Core.Width / 2f;
            uint color = ColorToUint32(Color.FromArgb((int)(1f * 255), 225, 0, 0)); // Red color with full opacity

            // Shadow parameters
            float shadowOffset = 2.5f; // Increased the offset for a larger shadow effect
            uint shadowColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 1f));

            // Draw shadow (a larger circle slightly offset behind the main one)
            vList.AddCircleFilled(new Vector2(centerX, centerY), radius + shadowOffset, shadowColor, numSegments);

            // Draw main circle (increased the radius a little bit to make it bigger)
            vList.AddCircleFilled(new Vector2(centerX, centerY), radius + 2f, color, numSegments); // Increased the radius by 2 units
        }

        public void DrawHealthBar(short health, short maxHealth, float X, float Y, float height)
        {
            var vList = ImGui.GetForegroundDrawList();

            float healthPercentage = Math.Clamp((float)health / maxHealth, 0f, 1f);
            float barHeight = height * healthPercentage;

            // Background bar (black)
            vList.AddRectFilled(new Vector2(X, Y), new Vector2(X + 4, Y + height), ColorToUint32(Color.Black));

            // Gradient color from red (low) to green (full)
            Color lowHPColor = Color.Red;
            Color highHPColor = Color.LimeGreen;
            Color currentColor = LerpColor(lowHPColor, highHPColor, healthPercentage);

            // Gradient bar fill from bottom to top
            for (int i = 0; i < barHeight; i++)
            {
                float segmentPercent = i / barHeight;
                Color segmentColor = LerpColor(lowHPColor, highHPColor, segmentPercent);
                vList.AddRectFilled(
                    new Vector2(X, Y + height - i),
                    new Vector2(X + 4, Y + height - i - 1),
                    hpcolor(segmentColor)
                );
            }

            // Add pulsing red effect if health is very low
            if (healthPercentage <= 0.25f)
            {
                float pulse = (float)(Math.Sin(Environment.TickCount / 100.0) * 0.5 + 0.5); // from 0 to 1
                Color pulseColor = Color.FromArgb((int)(pulse * 255), 255, 0, 0); // Red with pulsing alpha
                vList.AddRectFilled(new Vector2(X, Y), new Vector2(X + 4, Y + height), ColorToUint32(pulseColor));
            }
        }


        public Color LerpColor(Color a, Color b, float t)
        {
            return Color.FromArgb(
                (int)(a.A + (b.A - a.A) * t),
                (int)(a.R + (b.R - a.R) * t),
                (int)(a.G + (b.G - a.G) * t),
                (int)(a.B + (b.B - a.B) * t)
            );
        }

        // Helper: convert System.Drawing.Color to ImGui uint color
        public uint hpcolor(Color color)
        {
            return (uint)((color.A << 24) | (color.B << 16) | (color.G << 8) | color.R);
        }


        public void DrawPlusIcon(float centerX, float centerY, float baseSize = 10f)
        {
            var vList = ImGui.GetForegroundDrawList();
            float time = Environment.TickCount / 1000f;

            byte r = (byte)(MathF.Sin(time * 2f) * 127 + 128);
            byte g = (byte)(MathF.Sin(time * 2f + 2f) * 127 + 128);
            byte b = (byte)(MathF.Sin(time * 2f + 4f) * 127 + 128);
            uint color = (uint)(0xFF000000 | (r << 16) | (g << 8) | b);

            float angle = time * 2.0f;
            float pulse = 1.0f + 0.1f * MathF.Sin(time * 5f);
            float size = baseSize * pulse;
            float h = size / 2f;

            Vector2[] points = new Vector2[4];
            points[0] = RotatePoint(new Vector2(centerX - h, centerY), centerX, centerY, angle);
            points[1] = RotatePoint(new Vector2(centerX + h, centerY), centerX, centerY, angle);
            points[2] = RotatePoint(new Vector2(centerX, centerY - h), centerX, centerY, angle);
            points[3] = RotatePoint(new Vector2(centerX, centerY + h), centerX, centerY, angle);

            float thickness = 2.0f;
            vList.AddLine(points[0], points[1], color, thickness);
            vList.AddLine(points[2], points[3], color, thickness);
        }

        public Vector2 RotatePoint(Vector2 point, float cx, float cy, float angle)
        {
            float s = MathF.Sin(angle);
            float c = MathF.Cos(angle);
            point.X -= cx;
            point.Y -= cy;
            float xnew = point.X * c - point.Y * s;
            float ynew = point.X * s + point.Y * c;
            return new Vector2(xnew + cx, ynew + cy);
        }




        public void DrawMiniMap()
        {
            var drawList = ImGui.GetForegroundDrawList();

            const float mapRadius = 80f;
            const float zoom = 3.5f; // Larger = more zoomed out
            const float margin = 20f;

            // Map center on screen (bottom-left corner)
            Vector2 mapCenter = new Vector2(margin + mapRadius, Core.Height - margin - mapRadius);

            // Draw black background
            drawList.AddCircleFilled(mapCenter, mapRadius, ColorToUint32(Color.Black), 60);

            // Draw local player (green dot at center)
            drawList.AddCircleFilled(mapCenter, 4f, ColorToUint32(Color.Lime), 12);

            // Draw enemies (red dots)
            foreach (var entity in Core.Entities.Values)
            {
                if (entity.IsDead || !entity.IsKnown) continue;

                Vector3 offset = entity.Root - Core.LocalMainCamera;

                // Relative position in 2D (ignore height/Y)
                float dx = offset.X / zoom;
                float dy = offset.Z / zoom; // Z is usually forward/back in 3D games

                Vector2 dotPos = mapCenter + new Vector2(dx, dy);

                // Stay inside mini-map circle
                if (Vector2.Distance(dotPos, mapCenter) > mapRadius - 4f) continue;

                drawList.AddCircleFilled(dotPos, 3f, ColorToUint32(Color.Red), 12);
            }

            // Optional border ring
            drawList.AddCircle(mapCenter, mapRadius, ColorToUint32(Color.DarkGray), 60, 1.5f);
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
            var rightWristJointScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightWristJoint, Core.Width, Core.Height);
            var leftWristJointScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftWristJoint, Core.Width, Core.Height);
            var leftElbowScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftElbow, Core.Width, Core.Height);
            var rightElbowScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightElbow, Core.Width, Core.Height); // Adjust if needed

            // Draw skeleton lines


            
            DrawLine(drawList, spineScreenPos, hipScreenPos, lineColor);// Spine to hip


            
            DrawLine(drawList, leftElbowScreenPos, rightWristJointScreenPos, lineColor); // Left Elbow to Left Wrist Joint
            // Left Wrist Joint to Left Wrist

            

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

        public void DrawDynamicHealthBar(short health, short maxHealth, float X, float Y, float height, float width)
        {
            var vList = ImGui.GetForegroundDrawList();

            // Prevent division by zero and ensure healthPercentage is between 0 and 1
            if (maxHealth <= 0) maxHealth = 100; // Fallback to a default max health
            float healthPercentage = Math.Clamp((float)health / maxHealth, 0f, 1f);
            float healthHeight = height * healthPercentage;

            // Determine the color based on health percentage
            Color healthColor;
            if (healthPercentage < 0.2f)
            {
                healthColor = Color.FromArgb(255, 255, 0, 0); // Red for health < 20%
            }
            else if (healthPercentage < 0.5f)
            {
                healthColor = Color.FromArgb(255, 255, 255, 0); // Yellow for health < 50%
            }
            else
            {
                healthColor = Color.FromArgb(255, 86, 255, 43); // Green for health >= 50%
            }

            // Draw the full health bar background (unfilled part)
            vList.AddRectFilled(new Vector2(X, Y - height), new Vector2(X + width, Y), ColorToUint32(Color.FromArgb(255, 99, 0, 0))); // Background for health bar

            // Draw the health portion representing current health
            vList.AddRectFilled(new Vector2(X, Y - height + (height - healthHeight)), new Vector2(X + width, Y), ColorToUint32(healthColor)); // Health portion

            // Optionally add an effect like a glowing red color or a flash for low health
            if (healthPercentage < 0.2f)
            {
                // Adding a flashing effect or glow effect for low health
                vList.AddRectFilled(new Vector2(X, Y - height), new Vector2(X + width, Y), ColorToUint32(Color.FromArgb(100, 255, 0, 0))); // Flash effect (faint red)
            }

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
