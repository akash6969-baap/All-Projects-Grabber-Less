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
using Vortice;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text;
using System.Configuration;
using SixLabors.ImageSharp;
using System.IO;


namespace AotForms
{
    internal class ESP : ClickableTransparentOverlay.Overlay
    {
        IntPtr hWnd;
        IntPtr HDPlayer;
        private CX memoryfast = new CX();
        private ImFontPtr nkvtFont;
        private bool anticheat = false;
        private IEnumerable<long> wallResult;
        public static float GlowOpacity = 0.032f;

        private static System.Windows.Forms.Timer autorefresh = new System.Windows.Forms.Timer();

        internal static float GlowRadius = 20f;
        internal static float FeatherAmount = 2f;
        public static bool EnableGlow = false;

        private List<long> FoundAddresses = new List<long>();

        private uint purpleColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.58f, 0.44f, 0.86f, 1f));
        private const short DefaultMaxHealth = 200;
        private Vector4 lineColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        private Vector4 fovColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        private Vector4 boxColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        private Vector4 box3DColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        private Vector4 skeletonColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        private uint cyanColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0.8f, 1f, 1f));
        bool showWindow = true;

        private readonly string[] _comboItems2 = { "Closest To Crosshair", "Target360", "Closest To Player", "Lowest Health" };
        private readonly string[] _comboItems1 = { "Silent Aim", "Mouse Bot" };
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

        static string MakeAob(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return BitConverter.ToString(bytes).Replace("-", " ");
        }

        public async void speedon()
        {
            string[] processname = { "HD-Player" };
            bool success = memoryfast.SetProcess(processname);

            if (!success) return;

            IEnumerable<long> result = await memoryfast.AoBScan("40 02 2B 07 3D 02 2B 07 3D 02 2B 07 3D 00 00 00");

            foreach (long id in result)
            {
                memoryfast.AobReplace(id, "40 02 2B 9B 3C 02 2B 9B 3C 02 2B 07 3D 00 00 00");
            }
            Console.Beep(2000, 600);
        }

        public async void speedoff()
        {
            string[] processname = { "HD-Player" };
            bool success = memoryfast.SetProcess(processname);

            if (!success) return;

            IEnumerable<long> result = await memoryfast.AoBScan("40 02 2B 9B 3C 02 2B 9B 3C 02 2B 07 3D 00 00 00");

            foreach (long id in result)
            {
                memoryfast.AobReplace(id, "40 02 2B 07 3D 02 2B 07 3D 02 2B 07 3D 00 00 00");
            }
            Console.Beep(2000, 600);
        }


        public async void flyhackon()
        {
            string[] pocessname = { "HD-Player" };
            bool success = memoryfast.SetProcess(pocessname);

            if (!success)
            {
                return;
            }

            IEnumerable<long> result = await memoryfast.AoBScan("AC C5 27 37 00 10 A0 E1");

            foreach (long id in result)
            {
                memoryfast.AobReplace(id, "AC C5 A9 3F 00 10 A0 E1");
            }
            Console.Beep(2000, 600);

        }



        public async void flyhackoff()
        {
            string[] pocessname = { "HD-Player" };
            bool success = memoryfast.SetProcess(pocessname);

            if (!success)
            {
                return;
            }

            IEnumerable<long> result = await memoryfast.AoBScan("AC C5 A9 3F 00 10 A0 E1");

            foreach (long id in result)
            {
                memoryfast.AobReplace(id, "AC C5 27 37 00 10 A0 E1");
            }
            Console.Beep(2000, 600);

        }

        public async void wallbypass()
        {
            string[] processname = { "HD-Player" };
            bool success = memoryfast.SetProcess(processname);

            if (!success)
            {
                return;
            }

            // ---- पहला Search & Replace ----
            IEnumerable<long> result1 = await memoryfast.AoBScan("10 8D E5 00 A0 A0 E3 14 00 8D E5 09 00 A0 E1 18 10 8D E5 1C 10 8D E5 10 17 02 E3 35 FF 2F E1");
            foreach (long id in result1)
            {
                memoryfast.AobReplace(id, "10 8D E5 00 A0 A0 E3 14 00 8D E5 09 00 A0 E1 18 10 8D E5 1C 10 8D E5 10 17 02 E3 00");
            }

            // ---- दूसरा Search & Replace ----
            IEnumerable<long> result2 = await memoryfast.AoBScan("35 FF 2F E1 40 81");
            foreach (long id in result2)
            {
                memoryfast.AobReplace(id, "00");
            }

            Console.Beep(2000, 600);
        }

        public async void loadwall()

        {
            string[] pocessname = { "HD-Player" };
            bool success = memoryfast.SetProcess(pocessname);

            if (!success)
            {
                return;
            }

            wallResult = await memoryfast.AoBScan("AE 47 81 3F AE 47 81 3F AE 47 81 3F AE 47 81 3F 00 1A B7 EE DC 3A 9F ED");
            Console.Beep(2000, 600);

        }
        public async void wallon()
        {
            foreach (long id in wallResult)
            {
                memoryfast.AobReplace(id, "00 00 EF C1 AE 47 81 3F AE 47 81 3F AE 47 81 3F 00 1A B7 EE DC 3A 9F ED");
            }
            Console.Beep(2000, 600);
        }
        public async void walloff()
        {
            foreach (long id in wallResult)
            {
                memoryfast.AobReplace(id, "AE 47 81 3F AE 47 81 3F AE 47 81 3F AE 47 81 3F 00 1A B7 EE DC 3A 9F ED");
            }
            Console.Beep(2000, 600);
        }

        public async void glitchfireon()
        {
            string[] pocessname = { "HD-Player" };
            bool success = memoryfast.SetProcess(pocessname);

            if (!success)
            {
                return;
            }

            IEnumerable<long> result = await memoryfast.AoBScan("C0 3F 00 00 00 3F 00 00 80 3F 00 00");

            foreach (long id in result)
            {
                memoryfast.AobReplace(id, "00 00 00 00 00 3F 00 00 80 3F 00 00");
            }
            Console.Beep(2000, 600);

        }

        public async void glitchfireoff()
        {
            string[] pocessname = { "HD-Player" };
            bool success = memoryfast.SetProcess(pocessname);

            if (!success)
            {
                return;
            }

            IEnumerable<long> result = await memoryfast.AoBScan("REPLACE");

            foreach (long id in result)
            {
                memoryfast.AobReplace(id, "SEARCH");
            }
            Console.Beep(2000, 600);

        }

        private async void AntiCheat()
        {
            if (!anticheat)
            {
                List<string> searchList = new List<string>
{
                "00 48 2D E9 0D B0 A0 E1 58 D0 4D E2 08 13 9F E5",
                "00 48 2D E9 0D B0 A0 E1 50 D0 4D E2 FC 32 9F E5",

};

                List<string> replaceList = new List<string>
{
                "00 00 A0 E3 1E FF 2F E1",
                "00 00 A0 E3 1E FF 2F E1",


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
                    await Task.Delay(5);
                }
            });
        }
        private void RenderEntities()
        {
            var drawList = ImGui.GetBackgroundDrawList();

            Entity nearestEnemy = null;
            float nearestDistance = float.MaxValue;
            Vector2 screenCenter = new Vector2(Core.Width / 2f, Core.Height / 2f);

            if (Config.AimTrackLine)
            {
                foreach (var entity in Core.Entities.Values)
                {
                    if (!processedEntities.TryGetValue(entity.GetHashCode(), out var entityData) || !entityData.IsValid) continue;
                    if (entity.IsTeam != Bool3.False || entity.IsDead || entity.IsKnocked) continue;

                    // Calculate distance from crosshair to enemy head on screen
                    float distToCrosshair = Vector2.Distance(screenCenter, entityData.headScreenPos);

                    if (distToCrosshair < nearestDistance)
                    {
                        nearestDistance = distToCrosshair;
                        nearestEnemy = entity;
                    }
                }

                // Draw aim track line to nearest enemy if found
                if (nearestEnemy != null && processedEntities.TryGetValue(nearestEnemy.GetHashCode(), out var nearestData))
                {
                    // Replace standard line with glowing line for aim tracking
                    uint trackLineColor = ColorToUint32(Config.AimTrackLineColor);
                    DrawGlowLine(screenCenter, nearestData.headScreenPos, trackLineColor, 1.0f, 20.0f, 2.0f);
                }
            } 
            
                 Vector2 displaySize = ImGui.GetIO().DisplaySize;
                float windowWidth = displaySize.X;
                float windowHeight = displaySize.Y;

                if (Config.EspTimerActive)
                {
                    DrawEspTimer(windowWidth, windowHeight);
                }

            foreach (var entity in Core.Entities.Values)
            {
                if (!processedEntities.TryGetValue(entity.GetHashCode(), out var entityData) || !entityData.IsValid) continue;

                float CornerHeight = Math.Abs(entityData.headScreenPos.Y - entityData.bottomScreenPos.Y);
                float CornerWidth = CornerHeight * 0.65f;

                var headScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);

                var bottomScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Root, Core.Width, Core.Height);

                var dist = Vector3.Distance(Core.LocalMainCamera, entity.Head);


                if (headScreenPos.X < 1 || headScreenPos.Y < 1) continue;
                if (bottomScreenPos.X < 1 || bottomScreenPos.Y < 1) continue;







                if (Config.ESPLine)
                {
                    uint lineColor = ColorToUint32(Config.ESPLineColor);

                    // Top circle
                    DrawFilledCircle(25f, 5.0f);

                    if (!entity.IsKnocked)
                        ImGui.GetBackgroundDrawList().AddLine(
                            new Vector2(Core.Width / 2f, 25f),
                            headScreenPos,
                            lineColor,
                            0.4f
                        );
                    else
                        ImGui.GetBackgroundDrawList().AddLine(
                            new Vector2(Core.Width / 2f, 25f),
                            headScreenPos,
                            ColorToUint32(Color.Red),
                            0.4f
                        );
                }

                if (Config.ESPMoco)
                {
                    uint colorUint32 = ColorToUint32(Config.MocoColor);
                    uint outlineColor = ColorToUint32(Color.Black);
                    float reducedCornerWidth = CornerWidth * 0.5f;
                    float reducedHeight = 10f;
                    DrawGlowTriangle(
                        new Vector2(headScreenPos.X - (reducedCornerWidth / 2f), headScreenPos.Y - reducedHeight),
                        new Vector2(headScreenPos.X + (reducedCornerWidth / 2f), headScreenPos.Y - reducedHeight),
                        new Vector2(headScreenPos.X, headScreenPos.Y),
                        colorUint32,
                        1f,
                        GlowRadius,
                        FeatherAmount
                    );
                    ImGui.GetBackgroundDrawList().AddTriangle(
                        new Vector2(headScreenPos.X - (reducedCornerWidth / 2f), headScreenPos.Y - reducedHeight),
                        new Vector2(headScreenPos.X + (reducedCornerWidth / 2f), headScreenPos.Y - reducedHeight),
                        new Vector2(headScreenPos.X, headScreenPos.Y),
                        outlineColor,
                        2f
                    );
                    ImGui.GetBackgroundDrawList().AddTriangleFilled(
                        new Vector2(headScreenPos.X - (reducedCornerWidth / 2f), headScreenPos.Y - reducedHeight),
                        new Vector2(headScreenPos.X + (reducedCornerWidth / 2f), headScreenPos.Y - reducedHeight),
                        new Vector2(headScreenPos.X, headScreenPos.Y),
                        colorUint32
                    );
                }


                if (Config.ESPBox)
                {
                    uint boxColor = ColorToUint32(Config.ESPBoxColor);
                    Vector2 topLeft = new Vector2(entityData.headScreenPos.X - (CornerWidth / 2), entityData.headScreenPos.Y);
                    DrawBox(topLeft, CornerWidth, CornerHeight, boxColor, 0.5f);
                }



                if (Config.ESPBox3D)
                {

                    uint box3DColor = ColorToUint32(Config.ESPBox3DColor);
                    Draw3DBox(entity.Head, entity.Root, 0.5f, 1.3f, 0.5f, Core.CameraMatrix, Core.Width, Core.Height, box3DColor, 0.5f);
                }



                var nameText = string.IsNullOrWhiteSpace(entity.Name) ? "Bot" : entity.Name;
                var namePosition = new Vector2(entityData.headScreenPos.X - (CornerWidth / 2), entityData.headScreenPos.Y - 30);
                var nameSize = ImGui.CalcTextSize($"      {MathF.Round(entityData.Distance)}M" + nameText);
                Vector2 fixedNameSize = new Vector2(95, 16);


                if (entity.Name == "")
                    entity.Name = "Bot";
                if (entityData.headScreenPos.X >= 0 && entityData.headScreenPos.Y >= 0 && entityData.headScreenPos.X <= Core.Width && entityData.headScreenPos.Y <= Core.Height)
                {
                    Vector2 namePos = new Vector2(entityData.headScreenPos.X - fixedNameSize.X / 2, entityData.headScreenPos.Y - fixedNameSize.Y - 15);



                    Vector2 textSizeName = ImGui.CalcTextSize(entity.Name);

                    Vector2 textSizeDistance = ImGui.CalcTextSize($" ({MathF.Round(Vector3.Distance(Core.LocalMainCamera, entity.Head))}m)");

                    Vector2 textPosName = new Vector2(namePos.X + 5, namePos.Y + (fixedNameSize.Y - textSizeName.Y) / 2);
                    Vector2 textPosDistance = new Vector2(namePos.X + fixedNameSize.X - textSizeDistance.X + 5, namePos.Y + (fixedNameSize.Y - textSizeDistance.Y) / 2);


                    if (headScreenPos.X < 1 || headScreenPos.Y < 1) continue;
                    if (bottomScreenPos.X < 1 || bottomScreenPos.Y < 1) continue;



                    var namePosition1 = new Vector2(headScreenPos.X - (CornerWidth / 2), headScreenPos.Y - 20);
                    string customName = $"{nameText}";



                    if (Config.ESPName)
                        DrawSmallTextWithOutline(new Vector2(headScreenPos.X - (CornerWidth / 2), headScreenPos.Y - 28),
                            string.IsNullOrWhiteSpace(entity.Name) ? "Bot" : entity.Name,
                            (ColorToUint32(Color.White)), ColorToUint32(Color.Black));


                    if (Config.ESPDistance)
                    {
                        string distanceText = $"{MathF.Round(dist)} M";
                        float estimatedTextWidth = distanceText.Length * 8f;
                        Vector2 distancePosition = new Vector2(bottomScreenPos.X + 1 - (estimatedTextWidth / 2), bottomScreenPos.Y + 5f);
                        DrawSmallTextWithOutline(distancePosition, distanceText, (ColorToUint32(Config.ESPSkeletonColor)), ColorToUint32(Color.Black));
                    }

                    if (Config.ESPHealth)
                    {
                        float healthBarHeight = CornerHeight;
                        float healthBarWidth = 6f;
                        float healthBarOffset = 5f;
                        Vector2 healthBarPos = new Vector2(headScreenPos.X - (CornerWidth / 2f) - healthBarWidth - healthBarOffset, headScreenPos.Y);

                        DrawVerticalHealthBar(entity.Health, DefaultMaxHealth, healthBarPos, healthBarHeight);
                    }


                    if (Config.ESPWeaponIcon)
                    {

                        DrawWeaponIcon(entity);
                    }

                }


            

                

                


                if (Config.minimap)
                {
                    DrawMinimap();
                }

                if (Config.ESPSkeleton)
                {
                    DrawSkeleton(entity);
                }


            }
        }

        public void DrawVerticalHealthBar(float currentHealth, float maxHealth, Vector2 position, float totalHeight, int glowRadius = 10)
        {
            var drawList = ImGui.GetForegroundDrawList();
            if (maxHealth <= 0) maxHealth = 200;

            float healthPct = Math.Clamp(currentHealth / maxHealth, 0f, 1f);
            float filledHeight = totalHeight * healthPct;
            float barWidth = 2.5f;

            // Gradient colors
            Vector4 green = new Vector4(0f, 1f, 0f, 1f);
            Vector4 yellow = new Vector4(1f, 1f, 0f, 1f);
            Vector4 red = new Vector4(1f, 0f, 0f, 1f);

            Vector4 healthColor = healthPct > 0.5f
                ? Vector4.Lerp(green, yellow, (1f - healthPct) * 2f)
                : Vector4.Lerp(yellow, red, (0.5f - healthPct) * 2f);

            Vector4 glowColor = healthPct > 0.5f ? green : (healthPct > 0.25f ? yellow : red);
            uint barColor = ImGui.ColorConvertFloat4ToU32(healthColor);

            // Background
            drawList.AddRectFilled(position, new Vector2(position.X + barWidth, position.Y + totalHeight), ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 0.5f)));

            // Glow effect
            for (int i = glowRadius; i > 0; i--)
            {
                float alpha = 25f * (1f - (float)i / glowRadius);
                drawList.AddRectFilled(
                    new Vector2(position.X - i, position.Y + (totalHeight - filledHeight) - i),
                    new Vector2(position.X + barWidth + i, position.Y + totalHeight + i),
                    ImGui.ColorConvertFloat4ToU32(new Vector4(glowColor.X, glowColor.Y, glowColor.Z, alpha / 255f))
                );
            }

            // Filled bar
            drawList.AddRectFilled(
                new Vector2(position.X, position.Y + (totalHeight - filledHeight)),
                new Vector2(position.X + barWidth, position.Y + totalHeight),
                barColor
            );
        }

        private float GetCameraYaw()
        {
            return MathF.Atan2(Core.CameraMatrix.M31, Core.CameraMatrix.M33);
        }

        void DrawSmallTextWithOutline(Vector2 pos, string text, uint textColor, uint outlineColor)
        {
            var vList = ImGui.GetForegroundDrawList();
            float outlineThickness = 1.2f;  // Smaller outline for smoothness
            float boldOffset = 0.4f;        // Adjusted for smaller text
            float spacing = 2.0f;           // Adds space between characters

            Vector2 adjustedPos = pos;

            foreach (char c in text)
            {
                string character = c.ToString(); // Convert char to string

                // Smooth outline
                for (float x = -outlineThickness; x <= outlineThickness; x += 1.0f)
                {
                    for (float y = -outlineThickness; y <= outlineThickness; y += 1.0f)
                    {
                        if (x == 0 && y == 0) continue;
                        vList.AddText(new Vector2(adjustedPos.X + x, adjustedPos.Y + y), outlineColor, character);
                    }
                }

                // Bold effect
                vList.AddText(new Vector2(adjustedPos.X - boldOffset, adjustedPos.Y), textColor, character);
                vList.AddText(new Vector2(adjustedPos.X + boldOffset, adjustedPos.Y), textColor, character);
                vList.AddText(new Vector2(adjustedPos.X, adjustedPos.Y - boldOffset), textColor, character);
                vList.AddText(new Vector2(adjustedPos.X, adjustedPos.Y + boldOffset), textColor, character);

                // Main text layer
                vList.AddText(adjustedPos, textColor, character);

                // Move position for next character with spacing
                adjustedPos.X += ImGui.CalcTextSize(character).X + spacing;
            }
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


        void DrawGlowTriangle(Vector2 vertex1, Vector2 vertex2, Vector2 vertex3, uint color, float thickness, float glowRadius, float featherAmount)
        {
        }


        public void DrawFilledCircle(float centerY, float radius, int numSegments = 64)
        {
            var vList = ImGui.GetBackgroundDrawList();

            // Set the center of the circle at the middle of the screen horizontally (Core.Width / 2f)

            float centerX = Core.Width / 2f;

            uint colorR = ColorToUint32(Color.FromArgb((int)(1f * 255), 225, 0, 0)); // Red color with full opacity
            uint colorG = ColorToUint32(Color.FromArgb((int)(1f * 255), 0, 255, 0)); // LimeGreen color with full opacity

            // Shadow parameters
            float shadowOffset = 1.08f; // The subtle offset of the shadow from the circle
            uint shadowColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 1f)); // Semi-transparent black for a soft shadow

            // Draw shadow (a larger circle slightly offset behind the main one)
            vList.AddCircleFilled(new Vector2(centerX, centerY), radius + shadowOffset, shadowColor, numSegments);

        }


        public static void DrawEspTimer(float windowWidth, float windowHeight)
        {
            // Update logic before drawing
            Config.UpdateEspTimer();

            if (!Config.EspTimerActive) return;

            var drawList = ImGui.GetForegroundDrawList();

            int elapsed = (int)(DateTime.Now - Config.EspTimerStart).TotalSeconds;
            int remaining = Config.EspTimerDuration - elapsed;
            string displayText = remaining > 0 ? $"ESP Timer {remaining}s" : "Timer Finished";

            Vector2 textSize = ImGui.CalcTextSize(displayText);

            // Draw at top right corner
            float margin = 10f;
            float x = windowWidth - textSize.X - margin;
            float y = margin;

            // Optional rainbow effect
            float rainbowSpeed = 2f;
            float hue = (float)(DateTime.Now.TimeOfDay.TotalSeconds * rainbowSpeed % 1.0);
            Vector4 rainbow = ColorFromHSV(hue, 1f, 1f);
            uint textColor = ImGui.ColorConvertFloat4ToU32(rainbow);

            // Draw shadow
            drawList.AddText(new Vector2(x + 1, y + 1),
                ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 0.7f)),
                displayText);

            // Draw text
            drawList.AddText(new Vector2(x, y), textColor, displayText);
        }

        // Simple HSV → RGBA converter
        public static Vector4 ColorFromHSV(float h, float s, float v)
        {
            int i = (int)(h * 6);
            float f = h * 6 - i;
            float p = v * (1 - s);
            float q = v * (1 - f * s);
            float t = v * (1 - (1 - f) * s);

            switch (i % 6)
            {
                case 0: return new Vector4(v, t, p, 1f);
                case 1: return new Vector4(q, v, p, 1f);
                case 2: return new Vector4(p, v, t, 1f);
                case 3: return new Vector4(p, q, v, 1f);
                case 4: return new Vector4(t, p, v, 1f);
                default: return new Vector4(v, p, q, 1f);
            }
        }

        public void DrawGlowLine(Vector2 start, Vector2 end, uint color, float thickness, float glowRadius, float feather, float glowOpacityMultiplier)
        {
            var drawList = ImGui.GetBackgroundDrawList();
            Vector4 colorVec = ImGui.ColorConvertU32ToFloat4(color);


            for (float i = glowRadius; i > 0; i -= feather)
            {
                float alpha = colorVec.W * (i / glowRadius) * glowOpacityMultiplier;
                alpha = Clamp(alpha, 0, 1);

                uint glowColor = ImGui.ColorConvertFloat4ToU32(new Vector4(colorVec.X, colorVec.Y, colorVec.Z, alpha));


                drawList.AddLine(start, end, glowColor, thickness + (glowRadius - i) * 0.5f);
            }


            drawList.AddLine(start, end, color, thickness);


            for (float i = glowRadius; i > 0; i -= feather)
            {
                float alpha = colorVec.W * (i / glowRadius) * glowOpacityMultiplier;
                alpha = Clamp(alpha, 0, 1);

                uint glowColor = ImGui.ColorConvertFloat4ToU32(new Vector4(colorVec.X, colorVec.Y, colorVec.Z, alpha));


                float radius = thickness / 2 + (glowRadius - i) * 0.5f;
                drawList.AddCircleFilled(start, radius, glowColor);
                drawList.AddCircleFilled(end, radius, glowColor);
            }


            drawList.AddCircleFilled(start, thickness / 2, color);
            drawList.AddCircleFilled(end, thickness / 2, color);
        }


        private float Clamp(float value, float min, float max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }





        private static Vector4 ColorFromHSV1(double h, double s, double v)
        {
            int i = (int)(h * 6);
            double f = h * 6 - i;
            double p = v * (1 - s);
            double q = v * (1 - f * s);
            double t = v * (1 - (1 - f) * s);

            switch (i % 6)
            {
                case 0: return new Vector4((float)v, (float)t, (float)p, 1f);
                case 1: return new Vector4((float)q, (float)v, (float)p, 1f);
                case 2: return new Vector4((float)p, (float)v, (float)t, 1f);
                case 3: return new Vector4((float)p, (float)q, (float)v, 1f);
                case 4: return new Vector4((float)t, (float)p, (float)v, 1f);
                default: return new Vector4((float)v, (float)p, (float)q, 1f);
            }
        }

        public void DrawESPName(Entity entity)
        {
            // Convert 3D world position of head to 2D screen position
            Vector2 headScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);

            // Skip drawing if entity is off-screen
            if (headScreenPos.X < 0 || headScreenPos.Y < 0)
                return;

            if (Config.ESPName)
            {
                if (string.IsNullOrEmpty(entity.Name))
                    entity.Name = "BOT";

                // Use the original player name without any modifications to preserve fancy/stylish characters
                string playerName = entity.Name;

                // Only trim very long names to avoid UI issues
                if (playerName.Length > 18)
                {
                    playerName = playerName.Substring(0, 15) + "...";
                }

                // Calculate distance
                int distance = (int)MathF.Round(Vector3.Distance(Core.LocalMainCamera, entity.Head));

                // Separate name and distance for better layout
                Vector2 nameSize = ImGui.CalcTextSize(playerName);
                string distanceText = $"{distance}m";
                Vector2 distanceSize = ImGui.CalcTextSize(distanceText);

                // Modern card-style dimensions with proper padding
                float cardWidth = Math.Max(nameSize.X + 60, 160);
                float cardHeight = 32;
                float cornerRadius = 8.0f;

                // Position the card above the player's head
                Vector2 cardPos = new Vector2(headScreenPos.X - cardWidth / 2, headScreenPos.Y - cardHeight - 15);

                // Calculate health percentage with better scaling
                float healthPercentage = entity.Health > 1000 ? 1f :
                                        entity.Health < 0 ? 0f :
                                        (float)entity.Health / (entity.Health > 230 ? 500 : 200);

                // Modern health bar properties
                float healthBarHeight = 4;
                float healthBarY = cardPos.Y - healthBarHeight - 6;

                // Draw modern card background with glassmorphism effect
                uint cardBgColor = entity.IsKnocked && Config.IgnoreKnocked ?
                    ColorToUint32(Color.FromArgb(200, 40, 20, 20)) : // Dark red tint for knocked
                    ColorToUint32(Color.FromArgb(200, 20, 25, 35));   // Dark blue-gray modern background

                ImGui.GetForegroundDrawList().AddRectFilled(
                    cardPos,
                    new Vector2(cardPos.X + cardWidth, cardPos.Y + cardHeight),
                    cardBgColor,
                    cornerRadius
                );

                // Add subtle border for modern look
                uint borderColor = ColorToUint32(Color.FromArgb(120, 255, 255, 255));
                ImGui.GetForegroundDrawList().AddRect(
                    cardPos,
                    new Vector2(cardPos.X + cardWidth, cardPos.Y + cardHeight),
                    borderColor,
                    cornerRadius,
                    ImDrawFlags.None,
                    1.0f
                );

                // Modern health bar with rounded ends
                float healthBarWidth = cardWidth - 12;
                Vector2 healthBarPos = new Vector2(cardPos.X + 6, healthBarY);

                // Health bar background (subtle)
                ImGui.GetForegroundDrawList().AddRectFilled(
                    healthBarPos,
                    new Vector2(healthBarPos.X + healthBarWidth, healthBarPos.Y + healthBarHeight),
                    ColorToUint32(Color.FromArgb(100, 60, 60, 60)),
                    healthBarHeight / 2
                );

                // Modern health bar color gradient
                uint healthBarColor;
                if (healthPercentage > 0.7f)
                {
                    healthBarColor = ColorToUint32(Color.FromArgb(255, 34, 197, 94)); // Modern green
                }
                else if (healthPercentage > 0.4f)
                {
                    healthBarColor = ColorToUint32(Color.FromArgb(255, 251, 146, 60)); // Modern orange
                }
                else if (healthPercentage > 0.2f)
                {
                    healthBarColor = ColorToUint32(Color.FromArgb(255, 239, 68, 68)); // Modern red
                }
                else
                {
                    healthBarColor = ColorToUint32(Color.FromArgb(255, 220, 38, 127)); // Critical pink
                }

                // Draw filled health bar with smooth corners
                if (healthPercentage > 0)
                {
                    ImGui.GetForegroundDrawList().AddRectFilled(
                        healthBarPos,
                        new Vector2(healthBarPos.X + (healthBarWidth * healthPercentage), healthBarPos.Y + healthBarHeight),
                        healthBarColor,
                        healthBarHeight / 2
                    );
                }

                // Modern player avatar/icon area (left side of card)
                float avatarSize = cardHeight - 8;
                Vector2 avatarPos = new Vector2(cardPos.X + 4, cardPos.Y + 4);

                // Draw avatar background with accent color
                uint avatarBgColor = entity.IsKnocked && Config.IgnoreKnocked ?
                    ColorToUint32(Color.FromArgb(255, 239, 68, 68)) : // Red for knocked
                    healthBarColor; // Use health color for consistency

                ImGui.GetForegroundDrawList().AddRectFilled(
                    avatarPos,
                    new Vector2(avatarPos.X + avatarSize, avatarPos.Y + avatarSize),
                    avatarBgColor,
                    6.0f
                );

                // Player number/ID in avatar area
                int playerNumber = (int)(entity.Distance * 10) % 100;
                string numberText = playerNumber.ToString("00");
                Vector2 numberSize = ImGui.CalcTextSize(numberText);
                Vector2 numberPos = new Vector2(
                    avatarPos.X + (avatarSize - numberSize.X) / 2,
                    avatarPos.Y + (avatarSize - numberSize.Y) / 2
                );

                // Draw player number with high contrast
                ImGui.GetForegroundDrawList().AddText(
                    numberPos,
                    ColorToUint32(Color.FromArgb(255, 255, 255, 255)),
                    numberText
                );

                // Modern typography layout for name and distance
                float textStartX = avatarPos.X + avatarSize + 12;
                float nameY = cardPos.Y + 6;
                float distanceY = cardPos.Y + cardHeight - distanceSize.Y - 6;

                // Draw player name with modern typography
                Vector2 namePos = new Vector2(textStartX, nameY);

                // Subtle text shadow for depth
                ImGui.GetForegroundDrawList().AddText(
                    namePos + new Vector2(0, 1),
                    ColorToUint32(Color.FromArgb(120, 0, 0, 0)),
                    playerName
                );

                // Main name text with high contrast
                ImGui.GetForegroundDrawList().AddText(
                    namePos,
                    ColorToUint32(Color.FromArgb(255, 255, 255, 255)),
                    playerName
                );

                // Distance text with subtle styling
                Vector2 distancePos = new Vector2(cardPos.X + cardWidth - distanceSize.X - 12, distanceY);

                // Distance background pill for modern look
                float pillWidth = distanceSize.X + 12;
                float pillHeight = distanceSize.Y + 4;
                Vector2 pillPos = new Vector2(distancePos.X - 6, distancePos.Y - 2);

                ImGui.GetForegroundDrawList().AddRectFilled(
                    pillPos,
                    new Vector2(pillPos.X + pillWidth, pillPos.Y + pillHeight),
                    ColorToUint32(Color.FromArgb(150, 0, 0, 0)),
                    pillHeight / 2
                );

                // Distance text
                ImGui.GetForegroundDrawList().AddText(
                    distancePos,
                    ColorToUint32(Color.FromArgb(200, 255, 255, 255)),
                    distanceText
                );

                // Optional: Add pulsing effect for low health
                if (healthPercentage < 0.3f)
                {
                    float pulseAlpha = (float)(Math.Sin(ImGui.GetTime() * 8) * 0.3 + 0.7);
                    uint pulseColor = ColorToUint32(Color.FromArgb((int)(pulseAlpha * 100), 239, 68, 68));

                    ImGui.GetForegroundDrawList().AddRect(
                        cardPos,
                        new Vector2(cardPos.X + cardWidth, cardPos.Y + cardHeight),
                        pulseColor,
                        cornerRadius,
                        ImDrawFlags.None,
                        2.0f
                    );
                }
            }
        }


        public void DrawHealthBar(Entity entity, int espHealthInt, Vector2 boxTopLeft, float boxWidth, float boxHeight)
        {
            var drawList = ImGui.GetBackgroundDrawList();

            // Health bar height same as box height
            float healthBarHeight = boxHeight;
            float healthBarWidth = 4.0f; // Thin vertical bar

            // Clamp and calculate percentage
            float healthPercentage = Math.Clamp(espHealthInt / 200.0f, 0.0f, 1.0f);
            float filledHeight = healthBarHeight * healthPercentage;

            // Position bar to the right of the box with 2px spacing
            Vector2 barTopLeft = new Vector2(boxTopLeft.X + boxWidth + 2.0f, boxTopLeft.Y);
            Vector2 barBottomRight = new Vector2(barTopLeft.X + healthBarWidth, barTopLeft.Y + healthBarHeight);

            // Filled bar bottom up (so low health appears at bottom)
            Vector2 filledTop = new Vector2(barTopLeft.X, barBottomRight.Y - filledHeight);
            Vector2 filledBottom = new Vector2(barBottomRight.X, barBottomRight.Y);

            // Calculate health color (red → green)
            uint barColor = ColorToUint32(Color.FromArgb(
                255,
                (int)((1.0f - healthPercentage) * 255),  // Red
                (int)(healthPercentage * 255),           // Green
                0));

            // Draw background bar
            drawList.AddRectFilled(barTopLeft, barBottomRight, ColorToUint32(Color.Black));

            // Draw filled health
            drawList.AddRectFilled(filledTop, filledBottom, barColor);

            // Outline
            drawList.AddRect(barTopLeft, barBottomRight, ColorToUint32(Color.Black), 0f, ImDrawFlags.None, 1.0f);
        }


        public void DrawBox(Vector2 topLeft, float width, float height, uint color, float thickness)
        {
            var vList = ImGui.GetBackgroundDrawList();

            // Draw simple rectangle (no glow)
            vList.AddRect(
                topLeft,
                new Vector2(topLeft.X + width, topLeft.Y + height),
                color,
                0f, // No rounding
                ImDrawFlags.None,
                thickness
            );
        }
        public void DrawHealthBar7(short health, short maxHealth, float X, float Y, float height)
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

        public uint hpcolor(Color color)
        {
            return (uint)((color.A << 24) | (color.B << 16) | (color.G << 8) | color.R);
        }

        public void DrawHealthBar21(Entity entity, int espHealthInt)
        {
            Vector2 headScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);

            // Dimensions for box — adjust as needed
            float boxHeight = 60.0f;       // estimated ESP box height
            float barWidth = 50.0f;        // width of the health bar
            float barHeight = 5.0f;        // height of the health bar
            float barGap = 5.0f;           // small gap below ESP box

            float healthPercentage = Math.Clamp(espHealthInt / 200.0f, 0.0f, 1.0f);

            // Position the bar just below the ESP box
            Vector2 barTopLeft = new Vector2(headScreenPos.X - (barWidth / 2), headScreenPos.Y + boxHeight + barGap);
            Vector2 barBottomRight = new Vector2(barTopLeft.X + barWidth, barTopLeft.Y + barHeight);

            var drawList = ImGui.GetBackgroundDrawList();

            uint barColor = ColorToUint32(Color.FromArgb(
                255,
                (int)((1.0f - healthPercentage) * 255),
                (int)(healthPercentage * 255),
                0));

            // Background
            drawList.AddRectFilled(barTopLeft, barBottomRight, ColorToUint32(Color.Black));

            // Health fill
            Vector2 healthBarRight = new Vector2(barTopLeft.X + (barWidth * healthPercentage), barBottomRight.Y);
            drawList.AddRectFilled(barTopLeft, healthBarRight, barColor);

            // Border
            drawList.AddRect(barTopLeft, barBottomRight, ColorToUint32(Color.Black), 0.0f, ImDrawFlags.None, 1.0f);
        }

        public void DrawHealthBar2(Entity entity, int espHealthInt)
        {

            Vector2 headScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);


            float healthPercentage = Math.Clamp(espHealthInt / 200.0f, 0.0f, 1.0f);

            // Health bar dimensions
            float barWidth = 50.0f;  // Width of health bar
            float barHeight = 5.0f;  // Height of health bar
            float barPadding = 25.0f;  // Distance above character's head

            Vector2 barTopLeft = new Vector2(headScreenPos.X - (barWidth / 2), headScreenPos.Y - barPadding - barHeight);
            Vector2 barBottomRight = new Vector2(headScreenPos.X + (barWidth / 2), headScreenPos.Y - barPadding);

            // Get the background draw list from ImGui
            var drawList = ImGui.GetBackgroundDrawList();

            // Calculate health bar color (green for full health, red for low health)
            uint barColor = ColorToUint32(Color.FromArgb(
                255,
                (int)((1.0f - healthPercentage) * 255),  // Red component (more for low health)
                (int)(healthPercentage * 255),           // Green component (more for high health)
                0));                                     // No blue component

            // Draw background (empty health bar - black background)
            drawList.AddRectFilled(
                barTopLeft,
                barBottomRight,
                ColorToUint32(Color.Black)  // Black background
            );

            // Draw filled portion of health bar
            Vector2 healthBarRight = new Vector2(barTopLeft.X + (barWidth * healthPercentage), barBottomRight.Y);
            drawList.AddRectFilled(
                barTopLeft,
                healthBarRight,
                barColor  // Health color based on percentage
            );

            // Add border around health bar
            drawList.AddRect(
                barTopLeft,
                barBottomRight,
                ColorToUint32(Color.Black),  // Black border
                0.0f,  // No rounding
                ImDrawFlags.None,  // No flags
                1.0f   // Border thickness
            );
        }

        private void DrawRainbowCurve(Vector2 startPoint, Vector2 headPoint, uint baseColor, float thickness,
                             float glowRadius, float feather, float glowOpacityMultiplier)
        {
            var drawList = ImGui.GetBackgroundDrawList();
            Vector4 colorVec = ColorU32ToFloat4(baseColor);

            // Bézier control point calculation
            Vector2 controlPoint = new Vector2(
                (startPoint.X + headPoint.X) / 2 + 50f,  // X offset for curve direction
                (startPoint.Y + headPoint.Y) / 2 - 100f   // Y offset for curve shape
            );

            const int numSteps = 50; // Number of curve segments
            Vector2 prevPoint = startPoint;

            // Time-based color animation
            float timeFactor = Environment.TickCount * 0.003f; // Speed multiplier

            for (int i = 1; i <= numSteps; i++)
            {
                float t = i / (float)numSteps;

                // Quadratic Bézier interpolation
                Vector2 curvePoint = new Vector2(
                    (1 - t) * (1 - t) * startPoint.X + 2 * (1 - t) * t * controlPoint.X + t * t * headPoint.X,
                    (1 - t) * (1 - t) * startPoint.Y + 2 * (1 - t) * t * controlPoint.Y + t * t * headPoint.Y
                );

                // Rainbow color calculation
                float red = 127.5f * (1 + MathF.Sin(2 * MathF.PI * t + timeFactor * 0.5f) * 1.5f);
                float green = 127.5f * (1 + MathF.Sin(2 * MathF.PI * (t + 1f / 3f) + timeFactor * 0.6f) * 1.5f);
                float blue = 127.5f * (1 + MathF.Sin(2 * MathF.PI * (t + 2f / 3f) + timeFactor * 0.7f) * 1.5f);

                // Clamp and convert to color
                uint segmentColor = ImGui.ColorConvertFloat4ToU32(new Vector4(
                    Math.Clamp(red, 0, 255) / 255f,
                    Math.Clamp(green, 0, 255) / 255f,
                    Math.Clamp(blue, 0, 255) / 255f,
                    1f
                ));

                // Draw the segment with glow
                DrawGlowSegment(drawList, prevPoint, curvePoint, segmentColor,
                               thickness, glowRadius, feather, glowOpacityMultiplier);

                prevPoint = curvePoint;
            }
        }
        private Vector4 ColorU32ToFloat4(uint color)
        {
            return new Vector4(
                ((color >> 0) & 0xFF) / 255f,
                ((color >> 8) & 0xFF) / 255f,
                ((color >> 16) & 0xFF) / 255f,
                ((color >> 24) & 0xFF) / 255f
            );
        }

        private uint Float4ToColorU32(Vector4 color)
        {
            return ImGui.ColorConvertFloat4ToU32(color);
        }

        private void DrawGlowSegment(ImDrawListPtr drawList, Vector2 start, Vector2 end, uint color,
                                    float thickness, float glowRadius, float feather, float opacity)
        {
            Vector4 colorVec = ColorU32ToFloat4(color);


            for (float r = glowRadius; r > 0; r -= feather)
            {
                float alpha = colorVec.W * (r / glowRadius) * opacity;
                uint glowColor = ImGui.ColorConvertFloat4ToU32(new Vector4(
                    colorVec.X, colorVec.Y, colorVec.Z, alpha
                ));

                drawList.AddLine(start, end, glowColor, thickness + (glowRadius - r) * 0.5f);
            }


            drawList.AddLine(start, end, color, thickness);


            drawList.AddCircleFilled(start, thickness / 2, color);
            drawList.AddCircleFilled(end, thickness / 2, color);
        }




        private void DrawMinimap()
        {
            var windowWidth = Core.Width;
            var windowHeight = Core.Height;

            int DetectionRange = 250;

            float minimapSize = 180f * (DetectionRange / 250f); // Uniform circular minimap
            Vector2 minimapCenter = new Vector2(100, windowHeight - 100);

            float cameraYaw = -GetCameraYaw();
            float cosYaw = MathF.Cos(cameraYaw);
            float sinYaw = MathF.Sin(cameraYaw);

            ImDrawListPtr drawList = ImGui.GetBackgroundDrawList();
            uint minimapBackgroundColor = ColorToUint32(Color.FromArgb(180, 20, 20, 20));
            uint minimapBorderColor = ColorToUint32(Color.FromArgb(255, 255, 255, 255));

            // Smooth circular minimap
            drawList.AddCircleFilled(minimapCenter, minimapSize / 2, minimapBackgroundColor);
            drawList.AddCircle(minimapCenter, minimapSize / 2 + 2, minimapBorderColor, 100, 2.0f); // Border

            // Grid lines (circular effect)
            uint gridColor = ColorToUint32(Color.FromArgb(80, 200, 200, 200));
            for (float i = 0.25f; i <= 1f; i += 0.25f)
            {
                drawList.AddCircle(minimapCenter, (minimapSize / 2) * i, gridColor, 100, 0.5f);
            }

            // Compass directions with better visibility
            string[] compassDirections = { "N", "E", "S", "W" };
            for (int i = 0; i < 4; i++)
            {
                float angle = cameraYaw + i * MathF.PI / 2;
                Vector2 directionPos = minimapCenter + new Vector2(MathF.Cos(angle), -MathF.Sin(angle)) * (minimapSize / 2 - 10);
                drawList.AddText(directionPos - new Vector2(5, 5), ColorToUint32(Color.White), compassDirections[i]);
            }

            // Player indicator (triangle)
            uint playerColor = ColorToUint32(Color.Cyan);
            Vector2[] playerTriangle = new Vector2[3]
            {
        minimapCenter + new Vector2(0, -8),   // Top
        minimapCenter + new Vector2(-6, 6),  // Left
        minimapCenter + new Vector2(6, 6)    // Right
            };

            for (int i = 0; i < 3; i++) // Rotate based on cameraYaw
            {
                float x = playerTriangle[i].X - minimapCenter.X;
                float y = playerTriangle[i].Y - minimapCenter.Y;
                playerTriangle[i].X = minimapCenter.X + (x * cosYaw - y * sinYaw);
                playerTriangle[i].Y = minimapCenter.Y + (x * sinYaw + y * cosYaw);
            }

            drawList.AddTriangleFilled(playerTriangle[0], playerTriangle[1], playerTriangle[2], playerColor);

            // Draw entities
            foreach (var entity in Core.Entities.Values)
            {
                if (entity.IsDead) continue;

                float distance = Vector3.Distance(Core.LocalMainCamera, entity.Head);
                if (distance > DetectionRange) continue;

                Vector3 relativePosition = entity.Head - Core.LocalMainCamera;
                float scale = minimapSize / (float)DetectionRange;

                float rotatedX = relativePosition.X * cosYaw - relativePosition.Z * sinYaw;
                float rotatedY = relativePosition.X * sinYaw + relativePosition.Z * cosYaw;

                Vector2 enemyOnMinimap = minimapCenter + new Vector2(rotatedX * scale, -rotatedY * scale);

                if (Vector2.Distance(enemyOnMinimap, minimapCenter) <= minimapSize / 2)
                {
                    uint enemyColor = entity.IsKnown
                        ? (entity.IsKnocked ? ColorToUint32(Color.Yellow) : ColorToUint32(Color.Red))
                        : ColorToUint32(Color.Blue);

                    // Dynamic glow effect for better visibility
                    drawList.AddCircleFilled(enemyOnMinimap + new Vector2(2, 2), 6.0f, ColorToUint32(Color.FromArgb(100, 0, 0, 0))); // Shadow
                    drawList.AddCircleFilled(enemyOnMinimap, 5.0f, enemyColor);
                }
            }
        }


        public void DrawWeaponIcon(Entity entity)
        {
            try
            {
                if (entity == null || entity.HoldingWeaponID == 0 || entity.Head == Vector3.Zero || entity.Root == Vector3.Zero)
                    return;

                string weaponIcon = WeaponIconData.GetIconForWeaponId(entity.HoldingWeaponID);

                Vector2 headPos = W2S.WorldToScreen(entity.Head);
                Vector2 footPos = W2S.WorldToScreen(entity.Root);

                if (headPos.X <= 0 || headPos.Y <= 0 || footPos.X <= 0 || footPos.Y <= 0)
                    return;


                float height = Math.Abs(headPos.Y - footPos.Y);
                if (height <= 0) return;

                Vector2 textSize = ImGui.CalcTextSize(weaponIcon);


                Vector2 textPos = new Vector2(
                    headPos.X - (textSize.X / 2),
                    headPos.Y - textSize.Y - 20
                );

                ImGui.PushFont(nkvtFont);

                ImGui.GetBackgroundDrawList().AddText(
                    textPos,
                    ColorToUint32(Color.White),
                    weaponIcon
                );

                ImGui.PopFont();
            }
            catch (Exception ex)
            {
            }
        }

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

            if (GetAsyncKeyState(Keys.F2) < 0)
            {
                Config.teli = true;
            }

            if (GetAsyncKeyState(Keys.F3) < 0)
            {
                Config.UpPlayer = true;

            }

            if (GetAsyncKeyState(Keys.Home) < 0)
            {
                enableAimBot = false;
                IgnoreKnocked = false;
                NoRecoil = false;
                ESPLine = false;
                ESPBox = false;
                ESPBox3D = false;
                ESPName = false;
                ESPHealth = false;
                ESPSkeleton = false;
                FOVEnabled = false;
                ESPHealthText = false;
            }
            if (GetAsyncKeyState(Keys.Delete) < 0)
            {
                enableAimBot = !enableAimBot;
                Thread.Sleep(200);

            }
            if (!Core.HaveMatrix) return;



            if (Config.FOVEnabled)
            {
                DrawFOVCircle(Config.AimFov);
            }





            string windowName = "Overlay";
            hWnd = FindWindow(null!, windowName);
            HDPlayer = FindWindow("BlueStacksApp", null!);

            if (hWnd != IntPtr.Zero)
            {
                long extendedStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
                SetWindowLong(hWnd, GWL_EXSTYLE, (extendedStyle | WS_EX_TOOLWINDOW) & ~WS_EX_APPWINDOW);
            }
            else
            {

            }
            RenderEntities();


        }

        private void DrawLine(ImDrawListPtr drawList, Vector2 startPos, Vector2 endPos, uint color)
        {
            if (startPos.X > 0 && startPos.Y > 0 && endPos.X > 0 && endPos.Y > 0)
            {
                drawList.AddLine(startPos, endPos, color, 0.5f); // Adjust thickness as needed
            }
        }
        private void DrawSkeleton(Entity entity)
        {
            var drawList = ImGui.GetBackgroundDrawList();
            uint lineColor = ColorToUint32(Config.ESPSkeletonColor); // Color for the skeleton lines
            uint circleColor = ColorToUint32(Color.White); // Color for the circle around the head

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
            var leftShoulderScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftShoulder, Core.Width, Core.Height);
            var rightShoulderScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightShoulder, Core.Width, Core.Height);
            var rightWristJointScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightWristJoint, Core.Width, Core.Height);
            var leftWristJointScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftWristJoint, Core.Width, Core.Height);
            var leftElbowScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftElbow, Core.Width, Core.Height);
            var rightElbowScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightElbow, Core.Width, Core.Height); // Adjust if needed

            // Draw skeleton lines


            DrawLine(drawList, spineScreenPos, rightShoulderScreenPos, lineColor); // Spine to Right Shoulder
            DrawLine(drawList, spineScreenPos, hipScreenPos, lineColor);// Spine to hip


            DrawLine(drawList, spineScreenPos, leftShoulderScreenPos, lineColor); // Spine to Left Shoulder
            DrawLine(drawList, leftShoulderScreenPos, rightElbowScreenPos, lineColor); // Left Shoulder to Left Elbow
            DrawLine(drawList, rightShoulderScreenPos, leftElbowScreenPos, lineColor); // Right Shoulder to Left Elbow

            DrawLine(drawList, leftElbowScreenPos, rightWristJointScreenPos, lineColor); // Left Elbow to Left Wrist Joint
            DrawLine(drawList, rightElbowScreenPos, leftWristJointScreenPos, lineColor); // Left Elbow to Left Wrist Joint
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
        private static uint[] HealthColors = new uint[]
{
    ColorToUint32(Color.Green),  // 70%+
    ColorToUint32(Color.Yellow), // 30-69%
    ColorToUint32(Color.Red)     // 0-29%
};
        private static uint GetHealthColor(float healthPercentage)
        {
            if (healthPercentage >= 0.7f) return HealthColors[0];
            if (healthPercentage >= 0.3f) return HealthColors[1];
            return HealthColors[2];
        }


        //public void DrawESPName(Entity entity)
        //{
        //    // Convert 3D world position to 2D screen space
        //    Vector2 headScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
        //    if (headScreenPos.X < 0 || headScreenPos.Y < 0)
        //        return;

        //    // Combine name + distance
        //    string distanceString = $"{MathF.Round(Vector3.Distance(Core.LocalMainCamera, entity.Head))}m";
        //    string combinedText = $"{entity.Name} {distanceString}";

        //    // Text size
        //    Vector2 textSize = ImGui.CalcTextSize(combinedText);

        //    // Position just above ESP box (tweak nameOffset if needed)
        //    float nameOffset = 10.0f;
        //    Vector2 textPos = new Vector2(
        //        headScreenPos.X - (textSize.X / 2),
        //        headScreenPos.Y - nameOffset - textSize.Y
        //    );

        //    // Draw name text using ImGui foreground layer
        //    ImGui.GetForegroundDrawList().AddText(
        //        textPos,
        //        ColorToUint32(Config.ESPNameColor),
        //        combinedText
        //    );
        //}












        public void DrawHealthText(Entity entity, int espHealthInt)
        {
            // Convert 3D world positions to 2D screen coordinates
            Vector2 headScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
            Vector2 pieDerechoScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightFoot, Core.Width, Core.Height);


            string healthText = "HP: " + espHealthInt.ToString();

            // Position of the text below the ESP box
            Vector2 textPosition = new Vector2(pieDerechoScreenPos.X - 25, pieDerechoScreenPos.Y + 10);

            // Get the background draw list from ImGui
            var drawList = ImGui.GetBackgroundDrawList();

            // Border color (black)
            uint borderColor = ColorToUint32(Color.Black);

            // Offset to create the text border
            float offset = 1.0f;

            drawList.AddText(new Vector2(textPosition.X - offset, textPosition.Y), borderColor, healthText);
            drawList.AddText(new Vector2(textPosition.X + offset, textPosition.Y), borderColor, healthText);
            drawList.AddText(new Vector2(textPosition.X, textPosition.Y - offset), borderColor, healthText);
            drawList.AddText(new Vector2(textPosition.X, textPosition.Y + offset), borderColor, healthText);

            // Draw the main text over the outline
            drawList.AddText(textPosition, ColorToUint32(Color.White), healthText);
        }



        public void DrawGlowingBall(Vector2 position, Color color, float radius)
        {
            var drawList = ImGui.GetBackgroundDrawList();
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




        public void Draw3DBox(Vector3 Head, Vector3 Root, float width, float entityHeight, float depth, Matrix4x4 viewMatrix, int screenWidth, int screenHeight, uint color, float thickness)
        {

            var vList = ImGui.GetBackgroundDrawList();

            // Define the eight corners of the 3D box
            Vector3[] boxCorners = new Vector3[]
            {
        new Vector3(Root.X - width / 2, Root.Y + entityHeight, Root.Z - depth / 2),  // Front Top Left
        new Vector3(Root.X + width / 2, Root.Y + entityHeight, Root.Z - depth / 2),  // Front Top Right
        new Vector3(Root.X - width / 2, Root.Y, Root.Z - depth / 2),  // Front Bottom Left
        new Vector3(Root.X + width / 2, Root.Y, Root.Z - depth / 2),  // Front Bottom Right

        new Vector3(Root.X - width / 2, Root.Y + entityHeight, Root.Z + depth / 2),  // Back Top Left
        new Vector3(Root.X + width / 2, Root.Y + entityHeight, Root.Z + depth / 2),  // Back Top Right
        new Vector3(Root.X - width / 2, Root.Y, Root.Z + depth / 2),  // Back Bottom Left
        new Vector3(Root.X + width / 2, Root.Y, Root.Z + depth / 2)   // Back Bottom Right
            };

            // Convert 3D world coordinates to 2D screen coordinates
            Vector2[] screenCorners = new Vector2[8];
            for (int i = 0; i < 8; i++)
            {
                screenCorners[i] = W2S.WorldToScreen(viewMatrix, boxCorners[i], screenWidth, screenHeight);
            }

            // Check if any points are off-screen (basic visibility check)
            if (screenCorners.Any(p => p.X == -1 && p.Y == -1))
                return;

            // Draw 3D Box (12 lines to connect all corners)
            vList.AddLine(screenCorners[0], screenCorners[1], color, thickness); // Front Top
            vList.AddLine(screenCorners[1], screenCorners[3], color, thickness); // Front Right
            vList.AddLine(screenCorners[3], screenCorners[2], color, thickness); // Front Bottom
            vList.AddLine(screenCorners[2], screenCorners[0], color, thickness); // Front Left

            vList.AddLine(screenCorners[4], screenCorners[5], color, thickness); // Back Top
            vList.AddLine(screenCorners[5], screenCorners[7], color, thickness); // Back Right
            vList.AddLine(screenCorners[7], screenCorners[6], color, thickness); // Back Bottom
            vList.AddLine(screenCorners[6], screenCorners[4], color, thickness); // Back Left

            vList.AddLine(screenCorners[0], screenCorners[4], color, thickness); // Connect Front-Back Left
            vList.AddLine(screenCorners[1], screenCorners[5], color, thickness); // Connect Front-Back Top
            vList.AddLine(screenCorners[2], screenCorners[6], color, thickness); // Connect Front-Back Bottom
            vList.AddLine(screenCorners[3], screenCorners[7], color, thickness); // Connect Front-Back Right
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
            //ImGui.SetWindowPos(new Vector2(x, y));
            Size = new Size(width, height);
            Position = new System.Drawing.Point(x, y);

            Core.Width = width;
            Core.Height = height;
        }

        public void DrawFOVCircle(float radius)
        {
            var drawList = ImGui.GetBackgroundDrawList();
            var center = new Vector2(Core.Width / 2f, Core.Height / 2f);
            uint color = ColorToUint32(Config.FOVColor);

            drawList.AddCircle(center, radius, color, 0, 0.4f);
        }

        private List<Notification> _notifications = new();

        public void Show(string message, Vector4 color)
        {
            _notifications.Add(new Notification(message, color));
        }

        public void Render1()
        {
            float bottomY = Core.Height - 40;
            float spacing = 10f;

            for (int i = 0; i < _notifications.Count; i++)
            {
                var n = _notifications[i];
                float elapsed = (float)ImGui.GetTime() - n.TimeCreated;

                // Fade and slide animation
                if (elapsed < 0.2f)
                {
                    n.Alpha = Math.Min(n.Alpha + 0.1f, 1f);
                    n.SlideOffset = Math.Min(n.SlideOffset + 4f, 20f);
                }
                else if (elapsed > n.Duration - 0.5f)
                {
                    n.Alpha = Math.Max(n.Alpha - 0.02f, 0f);
                    n.SlideOffset = Math.Max(n.SlideOffset - 1.5f, 0f);
                }

                if (!n.Visible && n.Alpha <= 0f)
                {
                    _notifications.RemoveAt(i--);
                    continue;
                }

                string icon = "🔔";
                string fullText = $"{icon}  {n.Message}";
                var size = ImGui.CalcTextSize(fullText);
                float x = Core.Width - size.X - 30;
                float y = bottomY - (i * (size.Y + spacing)) + n.SlideOffset;

                var draw = ImGui.GetForegroundDrawList();
                Vector4 bgColor = new Vector4(0f, 0f, 0f, 0.6f * n.Alpha);
                uint bg = ImGui.ColorConvertFloat4ToU32(bgColor);
                uint fg = ImGui.ColorConvertFloat4ToU32(new Vector4(n.Color.X, n.Color.Y, n.Color.Z, n.Alpha));

                draw.AddRectFilled(new Vector2(x - 10, y - 5), new Vector2(x + size.X + 10, y + size.Y + 5), bg, 5);
                draw.AddText(new Vector2(x, y), fg, fullText);
            }
        }


        public static unsafe void LoadUnicodeFont()
        {
            var io = ImGui.GetIO();


            ImFontConfigPtr config = ImGuiNative.ImFontConfig_ImFontConfig();
            config.OversampleH = 3;
            config.OversampleV = 1;
            config.PixelSnapH = true;

            ImFontPtr font = io.Fonts.AddFontFromFileTTF("arialuni.ttf", 16.0f, config, io.Fonts.GetGlyphRangesDefault());

            if (font.NativePtr == null)
            {
                Console.WriteLine("⚠️ Failed to load font: arialuni.ttf not found or unsupported.");
                return;
            }

            io.Fonts.Build();
        }

        public class Notification
        {
            public string Message;
            public float TimeCreated;
            public float Duration = 3f;
            public float Alpha = 0f;
            public float SlideOffset = 0f;
            public Vector4 Color;
            public bool Visible => ImGui.GetTime() - TimeCreated < Duration;

            public Notification(string message, Vector4 color)
            {
                Message = message;
                Color = color;
                TimeCreated = (float)ImGui.GetTime();
            }
        }



        public class Particle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Lifetime;
            public float Age;

            public Particle(Vector2 position, Vector2 velocity, float lifetime)
            {
                Position = position;
                Velocity = velocity;
                Lifetime = lifetime;
                Age = 0f;
            }

            public bool Update(float deltaTime)
            {
                Age += deltaTime;
                Position += Velocity * deltaTime;
                return Age < Lifetime;
            }
        }

        static Dictionary<string, List<Particle>> particleBursts = new Dictionary<string, List<Particle>>();


        static Dictionary<string, float> animationStates = new Dictionary<string, float>();
        static Dictionary<string, float> sliderStates = new Dictionary<string, float>();
        private static Dictionary<string, bool> wasActive = new();
        private static Dictionary<string, List<Particle>> sliderParticles = new();
        public static bool CustomCheckbox(string label, ref bool value)
        {
            ImGui.PushID(label);
            ImGui.BeginGroup();
            Vector2 cursorPos = ImGui.GetCursorScreenPos();
            Vector2 checkboxSize = new Vector2(22, 22);
            float rounding = 5.0f;
            ImGui.SetCursorScreenPos(cursorPos);

            bool isClicked = ImGui.InvisibleButton(label, checkboxSize);
            bool isHovered = ImGui.IsItemHovered();
            bool valueChanged = false;

            if (isClicked)
            {
                value = !value;
                valueChanged = true;

                // Create burst particles
                if (!particleBursts.ContainsKey(label))
                    particleBursts[label] = new List<Particle>();

                var rnd = new Random();
                for (int i = 0; i < 16; i++)
                {
                    float angle = (float)(rnd.NextDouble() * Math.PI * 2);
                    float speed = (float)(rnd.NextDouble() * 80 + 60); // more spread
                    Vector2 vel = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed;
                    particleBursts[label].Add(new Particle(cursorPos + checkboxSize / 2, vel, 0.7f));
                }
            }

            if (!animationStates.ContainsKey(label))
                animationStates[label] = 0f;

            float deltaTime = ImGui.GetIO().DeltaTime;
            animationStates[label] = MathHelper.Lerp(animationStates[label], value ? 1f : 0f, 1f - MathF.Exp(-10f * deltaTime));
            float checkAnim = animationStates[label];

            var drawList = ImGui.GetWindowDrawList();
            uint borderColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.3f, 0.3f, 0.3f, 1.0f));
            uint liquidColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.0f, 0.8f, 1f, 1.0f));
            uint hoverColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.0f, 0.9f, 1.0f, 0.6f));
            uint backgroundColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.1f, 0.1f, 0.1f, 1.0f));

            // Background
            drawList.AddRectFilled(cursorPos, cursorPos + checkboxSize, backgroundColor, rounding);

            // Liquid fill animation
            if (checkAnim > 0.05f)
            {
                float liquidHeight = Math.Max(checkboxSize.Y * checkAnim, 2f);
                Vector2 fillMin = cursorPos + new Vector2(0, checkboxSize.Y - liquidHeight);
                Vector2 fillMax = cursorPos + checkboxSize;
                drawList.AddRectFilled(fillMin, fillMax, liquidColor, rounding);
            }

            // Hover and border
            if (isHovered)
                drawList.AddRect(cursorPos, cursorPos + checkboxSize, hoverColor, rounding, ImDrawFlags.RoundCornersAll, 2.5f);

            drawList.AddRect(cursorPos, cursorPos + checkboxSize, borderColor, rounding, ImDrawFlags.RoundCornersAll, 2.0f);

            // Draw label
            ImGui.SameLine();
            ImGui.Text(label);

            // Update and draw particles
            if (particleBursts.ContainsKey(label))
            {
                var particles = particleBursts[label];
                for (int i = particles.Count - 1; i >= 0; i--)
                {
                    var p = particles[i];
                    if (!p.Update(deltaTime))
                    {
                        particles.RemoveAt(i);
                        continue;
                    }

                    float alpha = 1f - (p.Age / p.Lifetime);
                    uint color = ImGui.ColorConvertFloat4ToU32(new Vector4(0.0f, 0.9f, 1.0f, alpha));
                    float radius = 3.0f * alpha; // Increased radius
                    drawList.AddCircleFilled(p.Position, radius, color);
                }
            }

            ImGui.EndGroup();
            ImGui.PopID();

            return valueChanged;
        }

        public static bool DrawLiquidSlider(string label, ref float value, float min, float max, Vector2 size, uint liquidColor)
        {
            ImGui.BeginGroup();

            Vector2 cursorPos = ImGui.GetCursorScreenPos();
            float sliderWidth = size.X;
            float sliderHeight = size.Y;

            Vector2 sliderMin = cursorPos;
            Vector2 sliderMax = cursorPos + new Vector2(sliderWidth, sliderHeight);
            var drawList = ImGui.GetWindowDrawList();

            drawList.AddRectFilled(sliderMin, sliderMax, ImGui.ColorConvertFloat4ToU32(new Vector4(0.1f, 0.1f, 0.1f, 1.0f)), 5f);

            if (!sliderStates.ContainsKey(label))
                sliderStates[label] = 0f;

            float normalizedValue = (value - min) / (max - min);
            float deltaTime = ImGui.GetIO().DeltaTime;

            sliderStates[label] = MathHelper.Lerp(sliderStates[label], normalizedValue, 1f - MathF.Exp(-10f * deltaTime));
            float displayValue = sliderStates[label];

            if (Math.Abs(displayValue - normalizedValue) < 0.001f)
                sliderStates[label] = normalizedValue;

            float minFillWidth = 8f;
            float liquidWidth = MathF.Max(sliderWidth * displayValue, minFillWidth);
            Vector2 liquidMax = new Vector2(sliderMin.X + liquidWidth, sliderMax.Y);

            drawList.AddRectFilled(sliderMin, liquidMax, liquidColor, 8f);

            ImGui.InvisibleButton(label, new Vector2(sliderWidth, sliderHeight));
            bool isChanged = false;
            bool isActive = ImGui.IsItemActive();

            if (isActive)
            {
                Vector2 mousePos = ImGui.GetMousePos();
                float newValue = min + ((mousePos.X - sliderMin.X) / sliderWidth) * (max - min);
                newValue = Math.Clamp(newValue, min, max);

                if (Math.Abs(newValue - value) > 0.001f)
                {
                    value = newValue;
                    isChanged = true;
                }

                // Emit particles while dragging
                if (!sliderParticles.ContainsKey(label))
                    sliderParticles[label] = new List<Particle>();

                var rnd = new Random();
                Vector2 burstCenter = new Vector2(sliderMin.X + liquidWidth, (sliderMin.Y + sliderMax.Y) / 2f);

                for (int i = 0; i < 4; i++) // Lower count per frame for smoother flow
                {
                    float angle = (float)(rnd.NextDouble() * Math.PI * 2);
                    float speed = (float)(rnd.NextDouble() * 80 + 70);
                    Vector2 vel = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed;
                    sliderParticles[label].Add(new Particle(burstCenter, vel, 0.6f));
                }
            }

            // Draw particle effects
            if (sliderParticles.ContainsKey(label))
            {
                var particles = sliderParticles[label];
                for (int i = particles.Count - 1; i >= 0; i--)
                {
                    var p = particles[i];
                    if (!p.Update(deltaTime))
                    {
                        particles.RemoveAt(i);
                        continue;
                    }

                    float alpha = 1f - (p.Age / p.Lifetime);
                    uint color = ImGui.ColorConvertFloat4ToU32(new Vector4(0.0f, 0.8f, 1f, alpha));
                    float radius = 2.8f * alpha;
                    drawList.AddCircleFilled(p.Position, radius, color);
                }
            }

            ImGui.SetCursorScreenPos(cursorPos + new Vector2(sliderWidth + 10, 0));
            ImGui.Text($"{(int)value}");

            ImGui.EndGroup();
            return isChanged;
        }

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

                if (EmbeddedFonts.FontInter.Length > 0 && EmbeddedFonts.Fontnkvt.Length > 0)
                {

                    fixed (byte* fontPtr = EmbeddedFonts.FontInter)
                    {
                        io.Fonts.AddFontFromMemoryTTF((IntPtr)fontPtr, EmbeddedFonts.FontInter.Length, 20, config, io.Fonts.GetGlyphRangesDefault());
                    }
                    ushort[] glyphRanges = new ushort[] { Lucas.IconMin, Lucas.IconMax16, 0 };
                    config->MergeMode = 1;
                    config->OversampleH = 1;
                    config->OversampleV = 1;
                    config->PixelSnapH = 1;

                    fixed (byte* fontPtr = EmbeddedFonts.Fontnkvt)
                    fixed (ushort* p = &glyphRanges[0])
                    {
                        io.Fonts.AddFontFromMemoryTTF((IntPtr)fontPtr, EmbeddedFonts.Fontnkvt.Length, 20, config, new IntPtr(p));
                    }
                }
                else
                {
                    //Console.WriteLine("Embedded fonts are missing or corrupted.");
                }
            });
            Style();
            return base.PostInitialized();
        }
        private void Style()
        {
            var style = ImGui.GetStyle();

            style.Alpha = 1.0f;
            style.DisabledAlpha = 0.6000000238418579f;
            style.WindowPadding = new Vector2(8.0f, 8.0f);
            style.WindowRounding = 7.0f;
            style.WindowBorderSize = 1.0f;
            style.WindowMinSize = new Vector2(32.0f, 32.0f);
            style.WindowTitleAlign = new Vector2(0.0f, 0.5f);
            style.WindowMenuButtonPosition = ImGuiDir.Left;
            style.ChildRounding = 4.0f;
            style.ChildBorderSize = 1.0f;
            style.PopupRounding = 4.0f;
            style.PopupBorderSize = 1.0f;
            style.FramePadding = new Vector2(5.0f, 2.0f);
            style.FrameRounding = 3.0f;
            style.FrameBorderSize = 1.0f;
            style.ItemSpacing = new Vector2(6.0f, 6.0f);
            style.ItemInnerSpacing = new Vector2(6.0f, 6.0f);
            style.CellPadding = new Vector2(6.0f, 6.0f);
            style.IndentSpacing = 25.0f;
            style.ColumnsMinSpacing = 6.0f;
            style.ScrollbarSize = 15.0f;
            style.ScrollbarRounding = 9.0f;
            style.GrabMinSize = 10.0f;
            style.GrabRounding = 3.0f;
            style.TabRounding = 4.0f;
            style.TabBorderSize = 1.0f;
            style.TabMinWidthForCloseButton = 0.0f;
            style.ColorButtonPosition = ImGuiDir.Right;
            style.ButtonTextAlign = new Vector2(0.5f, 0.5f);
            style.SelectableTextAlign = new Vector2(0.0f, 0.0f);

            style.Colors[(int)ImGuiCol.Text] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            style.Colors[(int)ImGuiCol.TextDisabled] =
                new Vector4(0.4980392158031464f, 0.4980392158031464f, 0.4980392158031464f, 1.0f);
            style.Colors[(int)ImGuiCol.WindowBg] =
                new Vector4(0.09803921729326248f, 0.09803921729326248f, 0.09803921729326248f, 1.0f);
            style.Colors[(int)ImGuiCol.ChildBg] = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            style.Colors[(int)ImGuiCol.PopupBg] = new Vector4(0.1882352977991104f, 0.1882352977991104f, 0.1882352977991104f,
                0.9200000166893005f);
            style.Colors[(int)ImGuiCol.Border] = new Vector4(0.1882352977991104f, 0.1882352977991104f, 0.1882352977991104f,
                0.2899999916553497f);
            style.Colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.0f, 0.0f, 0.0f, 0.239999994635582f);
            style.Colors[(int)ImGuiCol.FrameBg] = new Vector4(0.0470588244497776f, 0.0470588244497776f, 0.0470588244497776f,
                0.5400000214576721f);
            style.Colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.1882352977991104f, 0.1882352977991104f,
                0.1882352977991104f, 0.5400000214576721f);
            style.Colors[(int)ImGuiCol.FrameBgActive] =
                new Vector4(0.2000000029802322f, 0.2196078449487686f, 0.2274509817361832f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBg] = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBgActive] =
                new Vector4(0.05882352963089943f, 0.05882352963089943f, 0.05882352963089943f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            style.Colors[(int)ImGuiCol.MenuBarBg] =
                new Vector4(0.1372549086809158f, 0.1372549086809158f, 0.1372549086809158f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.0470588244497776f, 0.0470588244497776f,
                0.0470588244497776f, 0.5400000214576721f);
            style.Colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.3372549116611481f, 0.3372549116611481f,
                0.3372549116611481f, 0.5400000214576721f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.4000000059604645f, 0.4000000059604645f,
                0.4000000059604645f, 0.5400000214576721f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.5568627715110779f, 0.5568627715110779f,
                0.5568627715110779f, 0.5400000214576721f);
            style.Colors[(int)ImGuiCol.CheckMark] =
                new Vector4(0.3294117748737335f, 0.6666666865348816f, 0.8588235378265381f, 1.0f);
            style.Colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.3372549116611481f, 0.3372549116611481f,
                0.3372549116611481f, 0.5400000214576721f);
            style.Colors[(int)ImGuiCol.SliderGrabActive] =
                new Vector4(0.3294117748737335f, 0.6666666865348816f, 0.8588235378265381f, 1.0f);
            style.Colors[(int)ImGuiCol.Button] = new Vector4(0.0470588244497776f, 0.0470588244497776f, 0.0470588244497776f,
                0.5400000214576721f);
            style.Colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.1882352977991104f, 0.1882352977991104f,
                0.1882352977991104f, 0.5400000214576721f);
            style.Colors[(int)ImGuiCol.ButtonActive] =
                new Vector4(0.2000000029802322f, 0.2196078449487686f, 0.2274509817361832f, 1.0f);
            style.Colors[(int)ImGuiCol.Header] = new Vector4(0.0f, 0.0f, 0.0f, 0.5199999809265137f);
            style.Colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.0f, 0.0f, 0.0f, 0.3600000143051147f);
            style.Colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.2000000029802322f, 0.2196078449487686f,
                0.2274509817361832f, 0.3300000131130219f);
            style.Colors[(int)ImGuiCol.Separator] = new Vector4(0.2784313857555389f, 0.2784313857555389f,
                0.2784313857555389f, 0.2899999916553497f);
            style.Colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.4392156898975372f, 0.4392156898975372f,
                0.4392156898975372f, 0.2899999916553497f);
            style.Colors[(int)ImGuiCol.SeparatorActive] =
                new Vector4(0.4000000059604645f, 0.4392156898975372f, 0.4666666686534882f, 1.0f);
            style.Colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.2784313857555389f, 0.2784313857555389f,
                0.2784313857555389f, 0.2899999916553497f);
            style.Colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.4392156898975372f, 0.4392156898975372f,
                0.4392156898975372f, 0.2899999916553497f);
            style.Colors[(int)ImGuiCol.ResizeGripActive] =
                new Vector4(0.4000000059604645f, 0.4392156898975372f, 0.4666666686534882f, 1.0f);
            style.Colors[(int)ImGuiCol.Tab] = new Vector4(0.0f, 0.0f, 0.0f, 0.5199999809265137f);
            style.Colors[(int)ImGuiCol.TabHovered] =
                new Vector4(0.1372549086809158f, 0.1372549086809158f, 0.1372549086809158f, 1.0f);
            style.Colors[(int)ImGuiCol.TabActive] = new Vector4(0.2000000029802322f, 0.2000000029802322f,
                0.2000000029802322f, 0.3600000143051147f);
            style.Colors[(int)ImGuiCol.TabUnfocused] = new Vector4(0.0f, 0.0f, 0.0f, 0.5199999809265137f);
            style.Colors[(int)ImGuiCol.TabUnfocusedActive] =
                new Vector4(0.1372549086809158f, 0.1372549086809158f, 0.1372549086809158f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotLines] = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotHistogram] = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
            style.Colors[(int)ImGuiCol.TableHeaderBg] = new Vector4(0.0f, 0.0f, 0.0f, 0.5199999809265137f);
            style.Colors[(int)ImGuiCol.TableBorderStrong] = new Vector4(0.0f, 0.0f, 0.0f, 0.5199999809265137f);
            style.Colors[(int)ImGuiCol.TableBorderLight] = new Vector4(0.2784313857555389f, 0.2784313857555389f,
                0.2784313857555389f, 0.2899999916553497f);
            style.Colors[(int)ImGuiCol.TableRowBg] = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            style.Colors[(int)ImGuiCol.TableRowBgAlt] = new Vector4(1.0f, 1.0f, 1.0f, 0.05999999865889549f);
            style.Colors[(int)ImGuiCol.TextSelectedBg] =
                new Vector4(0.2000000029802322f, 0.2196078449487686f, 0.2274509817361832f, 1.0f);
            style.Colors[(int)ImGuiCol.DragDropTarget] =
                new Vector4(0.3294117748737335f, 0.6666666865348816f, 0.8588235378265381f, 1.0f);
            style.Colors[(int)ImGuiCol.NavHighlight] = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
            style.Colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1.0f, 0.0f, 0.0f, 0.699999988079071f);
            style.Colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(1.0f, 0.0f, 0.0f, 0.2000000029802322f);
            style.Colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(1.0f, 0.0f, 0.0f, 0.3499999940395355f);
        }
    }


}