using AotForms;
using ImGuiNET;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using static AotForms.WinAPI;
using static TheArtOfDevHtmlRenderer.Adapters.RGraphicsPath;

namespace AotForms
{
    internal class EntityData
    {
        public Vector2 headScreenPos;
        public bool IsValid;
    }
    internal class ESP : ClickableTransparentOverlay.Overlay
    {
        IntPtr hWnd;
        IntPtr HDPlayer;
        private Dictionary<int, EntityData> processedEntities = new Dictionary<int, EntityData>();
        string fontpath1 = "C:\\Windows\\Fonts\\taileb.ttf";
        string fontpath2 = "C:\\Windows\\obs.ttf";

        private static ImFontPtr WatermarkFont;
        private static ImFontPtr CustomFont;

        private static DateTime lastEspRefreshTime = DateTime.MinValue;
        private static bool isEspVisible = false;
        private const double espVisibleDurationSeconds = 1.0;
        private const double espRefreshIntervalSeconds = 5.0;
        private static uint randomHackerEntityAddress = 0;
        private static DateTime lastHackerEntityUpdate = DateTime.MinValue;
        private const double hackerEntityUpdateIntervalSeconds = 3.0;
        private static bool isInMatch = false;
        private static DateTime lastEntitySeenTime = DateTime.MinValue;

        protected override unsafe void Render()
        {
            ImGui.SetNextWindowPos(new Vector2(-10000, -10000));
            if (ImGui.Begin("Debug", ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoTitleBar))
            {
                ImGui.End();
            }
            this.ReplaceFont(config =>

            {
                var io = ImGui.GetIO();
                io.NativePtr->IniFilename = null;

                io.Fonts.AddFontFromFileTTF(fontpath1, 13, config, io.Fonts.GetGlyphRangesChineseSimplifiedCommon());
                CustomFont = io.Fonts.AddFontFromFileTTF(fontpath1, 16, config, io.Fonts.GetGlyphRangesDefault());
                WatermarkFont = io.Fonts.AddFontFromFileTTF(fontpath1, 28, config, io.Fonts.GetGlyphRangesDefault());

                config->MergeMode = 1;
                config->OversampleH = 1;
                config->OversampleV = 1;
                config->PixelSnapH = 1;

                var custom2 = new ushort[] { 0xE000, 0xF2FD, 0 };
                fixed (ushort* p = &custom2[0])
                {
                    io.Fonts.AddFontFromFileTTF(fontpath2, 30, config, new IntPtr(p));
                }
            });

            if (!Core.HaveMatrix) return;

            CreateHandle();

            var windowWidth = Core.Width;
            var windowHeight = Core.Height;

            if (Config.Aimfovc)
            {
                uint glowColor = ColorToUint32(Color.FromArgb((int)(1f * 255), Config.Aimfovcolor));
                DrawSmoothCircle(Config.AimBotFov, glowColor, 1.0f);
            }
            var drawList = ImGui.GetForegroundDrawList();
            string windowName = "Overlay";
            hWnd = FindWindow(null, windowName);
            HDPlayer = FindWindow("BlueStacksApp", null);
            if (hWnd != IntPtr.Zero)
            {
                long extendedStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
                SetWindowLong(hWnd, GWL_EXSTYLE, (extendedStyle | WS_EX_TOOLWINDOW) & ~WS_EX_APPWINDOW);
            }

            if (Config.minimap) DrawMinimap();

            bool shouldDrawEspThisFrame;
            if (Config.ESPRefreshMode)
            {
                if (isEspVisible)
                {
                    if ((DateTime.Now - lastEspRefreshTime).TotalSeconds > espVisibleDurationSeconds)
                    {
                        isEspVisible = false;
                        lastEspRefreshTime = DateTime.Now;
                    }
                }
                else
                {
                    if ((DateTime.Now - lastEspRefreshTime).TotalSeconds > espRefreshIntervalSeconds)
                    {
                        isEspVisible = true;
                        lastEspRefreshTime = DateTime.Now;
                    }
                }
                shouldDrawEspThisFrame = isEspVisible;
            }
            else
            {
                shouldDrawEspThisFrame = true;
            }

            if (shouldDrawEspThisFrame)
            {
                var tmp = Core.Entities;
                int enemyCount = 0;
                if ((DateTime.Now - lastHackerEntityUpdate).TotalSeconds > hackerEntityUpdateIntervalSeconds)
                {
                    var validEntities = tmp.Values.Where(e => !e.IsDead && e.IsKnown &&
                        Vector3.Distance(Core.LocalMainCamera, e.Head) <= Config.espran).ToList();

                    if (validEntities.Count > 0)
                    {
                        var random = new Random();
                        randomHackerEntityAddress = validEntities[random.Next(validEntities.Count)].Address;
                        lastHackerEntityUpdate = DateTime.Now;
                    }
                    else
                    {
                        randomHackerEntityAddress = 0;
                    }
                }
                drawList = ImGui.GetForegroundDrawList();

                foreach (var entity in tmp.Values)
                {
                    if (entity.IsDead || !entity.IsKnown) continue;
                    float dist = Vector3.Distance(Core.LocalMainCamera, entity.Head);
                    if (dist > Config.espran) continue;

                    var headScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
                    var bottomScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Root, Core.Width, Core.Height);
                    if (headScreenPos.X < 1 || headScreenPos.Y < 1 || bottomScreenPos.X < 1 || bottomScreenPos.Y < 1) continue;

                    enemyCount++;
                    float cornerHeight = Math.Abs(headScreenPos.Y - bottomScreenPos.Y);
                    float cornerWidth = cornerHeight * 0.65f;
                    var nameText = string.IsNullOrWhiteSpace(entity.Name) ? "BOT" : entity.Name;
                    var distanceText = $"{MathF.Round(dist)}M";

                    float nameTextWidth = ImGui.CalcTextSize(nameText).X;
                    float distanceTextWidth = ImGui.CalcTextSize(distanceText).X;
                    float separatorWidth = ImGui.CalcTextSize(" | ").X;
                    float totalTextWidth = nameTextWidth + separatorWidth + distanceTextWidth;
                    float nameTagWidth = totalTextWidth > 100f ? totalTextWidth + 10f : 100f;
                    var namePosition = new Vector2(headScreenPos.X - (cornerWidth / 2), headScreenPos.Y - 30);
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
                        Vector2 vector2 = new Vector2(headScreenPos.X - (nameTagWidth / 2), headScreenPos.Y - 35);
                        Vector2 iconSize = new Vector2(50, 20);
                        float iconY = Config.ESPName ? (namePos.Y - 25f) : (headScreenPos.Y - 43f);
                        Vector2 iconPos = new Vector2(headScreenPos.X - (iconSize.X / 2f), iconY);
                        string hackerText = "HACKER";
                        ImGui.PushFont(CustomFont);
                        Vector2 hackerSize = ImGui.CalcTextSize(hackerText);
                        ImGui.PopFont();

                        float hackerY = iconY;
                        if (Config.ESPWeaponIcon && !string.IsNullOrWhiteSpace(entity.WeaponName))
                            hackerY = iconY - hackerSize.Y - 5f;
                        else if (Config.ESPName)
                            hackerY = namePos.Y - hackerSize.Y - 5f;
                        else
                            hackerY = headScreenPos.Y - 45f;

                        Vector2 hackerFinalPos = new Vector2(headScreenPos.X - (hackerSize.X / 2f), hackerY);
                        if (Config.ESPLine)
                        {
                            Vector2 lineOrigin = new Vector2(Core.Width / 2f, 65f);
                            Vector2 lineEnd = headScreenPos;

                            if (Config.HackerTag)
                                lineEnd = new Vector2(headScreenPos.X, hackerFinalPos.Y - 2f);
                            else if (Config.ESPWeaponIcon && !string.IsNullOrWhiteSpace(entity.WeaponName))
                                lineEnd = new Vector2(headScreenPos.X, iconY - 2f);
                            else if (Config.ESPName)
                                lineEnd = new Vector2(headScreenPos.X, namePos.Y - 2f);

                            uint finalLineColor = entity.IsKnocked ? ColorToUint32(Color.Red) : ColorToUint32(Config.ESPLineColor);
                            DrawGlowLine(lineOrigin, lineEnd, finalLineColor, 1.1f, Config.GlowRadius * 2f, Config.FeatherAmount, Config.GlowOpacity);
                        }
                        if (Config.HackerTag && entity.Address == randomHackerEntityAddress)
                        {
                            ImGui.PushFont(CustomFont);
                            drawList.AddText(hackerFinalPos, ColorToUint32(Color.Red), hackerText);
                            ImGui.PopFont();
                        }
                        if (Config.ESPWeaponIcon && !string.IsNullOrWhiteSpace(entity.WeaponName))
                        {
                            var imagepath = $"E:\\NEW UI MAKING OBB 53\\Fear x Corporation\\weaponwhitebyFear\\{entity.WeaponName.ToLower()}.png";
                            if (System.IO.File.Exists(imagepath))
                            {
                                AddOrGetImagePointer(imagepath, true, out IntPtr imagehandle, out _, out _);
                                if (imagehandle != IntPtr.Zero)
                                {
                                    drawList.AddImage(imagehandle, iconPos, iconPos + iconSize, Vector2.Zero, Vector2.One, ColorToUint32(Config.WeaponIconColor));
                                }
                            }
                        }
                        if (Config.ESPFillBox)
                        {
                            Color fillboxColor = GetESPColor(Config.ESPFillBoxColor);
                            Color topColor = Color.FromArgb((int)(0.1f * 255), fillboxColor);
                            Color bottomColor = Color.FromArgb((int)(0.75f * 255), fillboxColor);
                            DrawGradientFillBox(headScreenPos.X - (cornerWidth / 2), headScreenPos.Y, cornerWidth, cornerHeight, topColor, bottomColor);
                        }
                        if (Config.ESPBox2)
                        {
                            Draw3dBox(headScreenPos.X - (cornerWidth / 2), headScreenPos.Y, cornerWidth, cornerHeight,
                                GetESPColor(ColorToUint32(Config.ESPBoxColor)), 1f, Config.BoxGlow ? 1f : 0f, 1.7f, 0.02f);
                        }
                        if (Config.ESPBox)
                        {
                            DrawCorneredBox(headScreenPos.X - (cornerWidth / 2), headScreenPos.Y, cornerWidth, cornerHeight,
                                GetESPColor(ColorToUint32(Config.ESPBoxColor)), 1f);
                        }

                        if (Config.ESPName)
                        {
                            drawList.AddRectFilled(namePos, namePos + new Vector2(nameTagWidth, 18), ColorToUint32(Color.FromArgb(200, 0, 0, 0)), 0f);
                            drawList.AddText(namePos + new Vector2(5, 2), ColorToUint32(Color.White), nameText);
                            drawList.AddText(namePos + new Vector2(5 + nameTextWidth, 2), ColorToUint32(Color.White), " | ");
                            drawList.AddText(namePos + new Vector2(5 + nameTextWidth + separatorWidth, 2), ColorToUint32(Color.White), distanceText);
                        }

                        if (Config.ESPHealth)
                        {
                            float healthBarX = namePos.X;
                            float healthBarY = namePos.Y + 18;

                            if (entity.IsKnocked)
                                DrawHorizontalHealthBarK(entity.Health, 200, healthBarX, healthBarY, nameTagWidth, 4f);
                            else
                                DrawHorizontalHealthBar(entity.Health, 200, healthBarX, healthBarY, nameTagWidth, 4f);
                        }

                        if (Config.ESPWeapon)
                            DrawWeaponTextESP(entity, headScreenPos, cornerHeight);

                        if (Config.ESPSkeleton)
                            DrawSkeleton(entity);

                    }
                    if (CustomFont.NativePtr != null) ImGui.PushFont(CustomFont);

                    string countText = enemyCount > 0 ? enemyCount.ToString() : "0";
                    string prefixText = enemyCount > 0 ? "HOSTILES DETECTED: " : "SECTOR CLEAR";
                    string fullText = enemyCount > 0 ? $"{prefixText}{countText}" : prefixText;

                    Vector2 prefixSize = ImGui.CalcTextSize(prefixText);
                    Vector2 countSize = ImGui.CalcTextSize(countText);
                    Vector2 totalTextSize = ImGui.CalcTextSize(fullText);

                    float paddingX = 20.0f;
                    float paddingY = 12.0f;
                    float ewBoxWidth = totalTextSize.X + paddingX * 2 + 25f;
                    float ewBoxHeight = totalTextSize.Y + paddingY * 2;

                    float totalPlayersTextPosX = (windowWidth - ewBoxWidth) / 2;
                    float totalPlayersTextPosY = 60;

                    Vector2 boxMin = new Vector2(totalPlayersTextPosX, totalPlayersTextPosY);
                    Vector2 boxMax = new Vector2(totalPlayersTextPosX + ewBoxWidth, totalPlayersTextPosY + ewBoxHeight);

                    float ewTime = Environment.TickCount * 0.002f;
                    
                    // Box Styling - Sleek Translucent Black with Rounded Corners
                    float rounding = 6.0f;
                    uint bgColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.05f, 0.05f, 0.05f, 0.85f));
                    uint shadowColorBox = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 0.5f));
                    
                    // Drop Shadow
                    drawList.AddRectFilled(new Vector2(boxMin.X + 3, boxMin.Y + 3), new Vector2(boxMax.X + 3, boxMax.Y + 3), shadowColorBox, rounding);
                    // Main Background
                    drawList.AddRectFilled(boxMin, boxMax, bgColor, rounding);
                    
                    Vector4 statusColorFloat;
                    if (enemyCount > 0)
                    {
                        float pulse = 0.6f + 0.4f * (float)Math.Abs(Math.Sin(ewTime * 4f));
                        statusColorFloat = new Vector4(1.0f, 0.15f, 0.15f, pulse);
                    }
                    else
                    {
                        statusColorFloat = new Vector4(0.1f, 1.0f, 0.2f, 0.9f);
                    }
                    uint statusColor = ImGui.ColorConvertFloat4ToU32(statusColorFloat);
                    
                    // Subtle outer border to give it a glassmorphism feel
                    uint borderColor = ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 0.1f));
                    drawList.AddRect(boxMin, boxMax, borderColor, rounding, ImDrawFlags.None, 1.0f);
                    
                    // Inner Accent Line (Left side)
                    drawList.AddRectFilled(new Vector2(boxMin.X + 2, boxMin.Y + 6), new Vector2(boxMin.X + 5f, boxMax.Y - 6), statusColor, 2f);
                    
                    // Animated Glowing Dot
                    Vector2 dotPos = new Vector2(boxMin.X + paddingX + 8f, boxMin.Y + ewBoxHeight / 2f);
                    drawList.AddCircleFilled(dotPos, 4.5f, statusColor);
                    drawList.AddCircleFilled(dotPos, 10.0f, ImGui.ColorConvertFloat4ToU32(new Vector4(statusColorFloat.X, statusColorFloat.Y, statusColorFloat.Z, statusColorFloat.W * 0.2f)));
                    
                    // Text Placement
                    Vector2 textPos = new Vector2(boxMin.X + paddingX + 25f, boxMin.Y + paddingY);
                    
                    // Drop Shadow for text
                    uint shadowColorText = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 1f));
                    drawList.AddText(new Vector2(textPos.X + 1, textPos.Y + 1), shadowColorText, fullText);
                    
                    uint prefixColorId = ImGui.ColorConvertFloat4ToU32(new Vector4(0.95f, 0.95f, 0.95f, 1f));
                    drawList.AddText(textPos, prefixColorId, prefixText);
                    
                    if (enemyCount > 0)
                    {
                        uint countColorId = ImGui.ColorConvertFloat4ToU32(new Vector4(1.0f, 0.3f, 0.3f, 1f));
                        drawList.AddText(new Vector2(textPos.X + prefixSize.X, textPos.Y), countColorId, countText);
                    }

                    if (CustomFont.NativePtr != null) ImGui.PopFont();

                }

                if (Config.MatchTimer)
                {
                    if (Config.EnemyTimerActive || Config.EnemyTimerEnded)
                    {
                        if (Core.Entities.Count > 0)
                        {
                            lastEntitySeenTime = DateTime.Now;
                            if (!isInMatch)
                            {
                                isInMatch = true;
                                Config.EnemyTimerStart = DateTime.Now;
                                Config.EnemyTimerActive = true;
                                Config.EnemyTimerEnded = false;
                            }
                        }
                        else
                        {
                            if (isInMatch && (DateTime.Now - lastEntitySeenTime).TotalSeconds > 15)
                            {
                                isInMatch = false;
                            }
                        }

                        if (Config.MatchTimerReset)
                        {
                            Config.EnemyTimerStart = DateTime.Now;
                            Config.EnemyTimerActive = true;
                            Config.EnemyTimerEnded = false;
                            Config.MatchTimerReset = false;
                        }
                        int totalDuration = 180;
                        int elapsed = (int)(DateTime.Now - Config.EnemyTimerStart).TotalSeconds;
                        int remaining = totalDuration - elapsed;

                        if (remaining <= 0)
                        {
                            remaining = 0;
                            Config.EnemyTimerEnded = true;
                            Config.EnemyTimerActive = false;
                        }

                        int minutes = remaining / 60;
                        int seconds = remaining % 60;
                        if (CustomFont.NativePtr != null)
                            ImGui.PushFont(CustomFont);

                        string titleText = "MATCH TIMER";
                        string timeText = $"Match: {minutes:00}:{seconds:00}";

                        var titleSize = ImGui.CalcTextSize(titleText);
                        var timeSize = ImGui.CalcTextSize(timeText);

                        float boxWidth = Math.Max(titleSize.X, timeSize.X) + 40f;
                        float boxHeight = titleSize.Y + timeSize.Y + 15f;
                        float posX = Core.Width - boxWidth - 40f;
                        float posY = Core.Height / 2f + 20f;

                        Vector2 bgMin = new Vector2(posX, posY);
                        Vector2 bgMax = new Vector2(posX + boxWidth, posY + boxHeight);
                        drawList.AddRectFilled(bgMin, bgMax, ImGui.ColorConvertFloat4ToU32(new Vector4(0.1f, 0.1f, 0.1f, 0.8f)), 0f);
                        float titleX = bgMin.X + (boxWidth - titleSize.X) / 2f;
                        float titleY = bgMin.Y + 5f;
                        drawList.AddText(new Vector2(titleX, titleY), ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f)), titleText);
                        float lineY = titleY + titleSize.Y + 2f;
                        drawList.AddLine(new Vector2(bgMin.X, lineY), new Vector2(bgMax.X, lineY), ImGui.ColorConvertFloat4ToU32(new Vector4(0.8f, 0.0f, 0.8f, 1.0f)), 2.0f);
                        float timeX = bgMin.X + (boxWidth - timeSize.X) / 2f;
                        float timeY = lineY + 5f;
                        Vector4 timeColor = (remaining <= 0) ? new Vector4(1f, 0.2f, 0.2f, 1f) : new Vector4(1f, 0.5f, 0.0f, 1f);
                        drawList.AddText(new Vector2(timeX, timeY), ImGui.ColorConvertFloat4ToU32(timeColor), timeText);

                        if (CustomFont.NativePtr != null)
                            ImGui.PopFont();
                    }

                    uint LightSeaGreenColorUint = ColorToUint32(Color.LightSeaGreen);
                    string waterMarkText = "";

                    if (WatermarkFont.NativePtr != null) ImGui.PushFont(WatermarkFont);

                    Vector2 waterMarkSize = ImGui.CalcTextSize(waterMarkText);
                    Vector2 textPosition = new Vector2((Core.Width - waterMarkSize.X) / 2f, 45f);

                    uint outlineColor = ColorToUint32(Color.Black);
                    DrawSmallTextWithOutline(textPosition, waterMarkText, LightSeaGreenColorUint, outlineColor);

                    if (WatermarkFont.NativePtr != null) ImGui.PopFont();
                    if (Config.AimTrackLine)
                    {
                        Vector2 screenCenter = new Vector2(Core.Width / 2f, Core.Height / 2f);
                        Entity nearestEnemy = null;
                        Vector2 nearestEnemyScreenPos = Vector2.Zero;
                        float nearestDistance = float.MaxValue;
                        foreach (var entity in Core.Entities.Values)
                        {
                            if (entity.IsDead || !entity.IsKnown) continue;
                            var headPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
                            if (headPos.X < 1 || headPos.Y < 1) continue;

                            float distToCrosshair = Vector2.Distance(screenCenter, headPos);
                            if (distToCrosshair < nearestDistance && distToCrosshair <= Config.AimBotFov)
                            {
                                nearestDistance = distToCrosshair;
                                nearestEnemy = entity;
                                nearestEnemyScreenPos = headPos;
                            }
                        }
                        if (nearestEnemy != null)
                        {
                            uint trackLineColor = ColorToUint32(Config.AimTrackLineColor);
                            DrawGlowLine(screenCenter, nearestEnemyScreenPos, trackLineColor, 0.8f, 4.0f, 1.0f, 0.6f);
                        }
                    }

                }
            }
        }

        private void DrawHorizontalHealthBarK(short health, short maxHealth, float x, float y, float width, float barThickness = 10f)
        {
            var drawList = ImGui.GetForegroundDrawList();
            float healthPercentage = (float)health / maxHealth;
            float barWidth = width * healthPercentage;

            for (float i = 0; i < width; i++)
            {
                if (i <= barWidth)
                    drawList.AddLine(new Vector2(x + i, y), new Vector2(x + i, y + barThickness), ColorToUint32(Color.Red));
            }

            float strokeThickness = 0.8f;
            drawList.AddRect(new Vector2(x - strokeThickness, y - strokeThickness),
                new Vector2(x + width + strokeThickness, y + barThickness + strokeThickness),
                ColorToUint32(Color.Black), 1f, ImDrawFlags.RoundCornersAll, strokeThickness);
        }

        private void DrawHorizontalHealthBar(short health, short maxHealth, float x, float y, float width, float barThickness = 10f)
        {
            var drawList = ImGui.GetForegroundDrawList();
            float healthPercentage = (float)health / maxHealth;
            float barWidth = width * healthPercentage;

            for (float i = 0; i < width; i++)
            {
                float lerpFactor = i / width;
                Color color = (lerpFactor < 0.5f)
                    ? LerpColor(Color.Red, Color.Yellow, lerpFactor * 2)
                    : LerpColor(Color.Yellow, Color.LimeGreen, (lerpFactor - 0.5f) * 2);

                if (i <= barWidth)
                    drawList.AddLine(new Vector2(x + i, y), new Vector2(x + i, y + barThickness), ColorToUint32(color));
            }

            float strokeThickness = 0.8f;
            drawList.AddRect(new Vector2(x - strokeThickness, y - strokeThickness),
                new Vector2(x + width + strokeThickness, y + barThickness + strokeThickness),
                ColorToUint32(Color.Black), 1f, ImDrawFlags.RoundCornersAll, strokeThickness);
        }

        void DrawWeaponTextESP(Entity entity, Vector2 headScreenPos, float boxHeight)
        {
            if (!Config.ESPWeapon || entity == null || string.IsNullOrEmpty(entity.WeaponName)) return;

            Vector2 weaponTextPos = new Vector2(
                headScreenPos.X - ImGui.CalcTextSize(entity.WeaponName).X / 2,
                headScreenPos.Y + boxHeight + 5);

            uint outlineColor = ColorToUint32(Color.Black);
            uint textColor = ColorToUint32(Config.WeaponTextColor);
            var fgDrawList = ImGui.GetForegroundDrawList();

            fgDrawList.AddText(weaponTextPos + new Vector2(1, 1), outlineColor, entity.WeaponName);
            fgDrawList.AddText(weaponTextPos, textColor, entity.WeaponName);
        }

        private void DrawMinimap()
        {
            var windowHeight = Core.Height;
            int DetectionRange = 250;
            float minimapSize = 180f * (DetectionRange / 250f);
            Vector2 minimapCenter = new Vector2(100, windowHeight - 100);

            float cameraYaw = -GetCameraYaw();
            float cosYaw = MathF.Cos(cameraYaw);
            float sinYaw = MathF.Sin(cameraYaw);

            ImDrawListPtr drawList = ImGui.GetBackgroundDrawList();

            uint minimapBackgroundColor = ColorToUint32(Color.FromArgb(180, 20, 20, 20));
            uint minimapBorderColor = ColorToUint32(Color.FromArgb(255, 255, 255, 255));

            drawList.AddCircleFilled(minimapCenter, minimapSize / 2, minimapBackgroundColor);
            drawList.AddCircle(minimapCenter, minimapSize / 2 + 2, minimapBorderColor, 100, 2.0f);

            uint gridColor = ColorToUint32(Color.FromArgb(80, 200, 200, 200));
            for (float i = 0.25f; i <= 1f; i += 0.25f)
                drawList.AddCircle(minimapCenter, (minimapSize / 2) * i, gridColor, 100, 0.5f);

            string[] compassDirections = { "N", "E", "S", "W" };
            for (int i = 0; i < 4; i++)
            {
                float angle = cameraYaw + i * MathF.PI / 2;
                Vector2 directionPos = minimapCenter + new Vector2(MathF.Cos(angle), -MathF.Sin(angle)) * (minimapSize / 2 - 10);
                drawList.AddText(directionPos - new Vector2(5, 5), ColorToUint32(Color.White), compassDirections[i]);
            }

            uint playerColor = ColorToUint32(Color.Cyan);
            Vector2[] playerTriangle = new Vector2[3]
            {
                minimapCenter + new Vector2(0, -8),
                minimapCenter + new Vector2(-6, 6),
                minimapCenter + new Vector2(6, 6)
            };

            for (int i = 0; i < 3; i++)
            {
                float x = playerTriangle[i].X - minimapCenter.X;
                float y = playerTriangle[i].Y - minimapCenter.Y;
                playerTriangle[i].X = minimapCenter.X + (x * cosYaw - y * sinYaw);
                playerTriangle[i].Y = minimapCenter.Y + (x * sinYaw + y * cosYaw);
            }
            drawList.AddTriangleFilled(playerTriangle[0], playerTriangle[1], playerTriangle[2], playerColor);

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

                    drawList.AddCircleFilled(enemyOnMinimap + new Vector2(2, 2), 6.0f,
                        ColorToUint32(Color.FromArgb(100, 0, 0, 0)));
                    drawList.AddCircleFilled(enemyOnMinimap, 5.0f, enemyColor);
                }
            }
        }

        public void DrawFilledCircle(float centerY, float radius, int numSegments = 64)
        {
            var vList = ImGui.GetBackgroundDrawList();

            float centerX = Core.Width / 2f;

            uint colorR = ColorToUint32(Color.FromArgb((int)(1f * 255), 225, 0, 0));
            uint colorG = ColorToUint32(Color.FromArgb((int)(1f * 255), 0, 255, 0));
            float shadowOffset = 1.08f;
            uint shadowColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 1f));
            vList.AddCircleFilled(new Vector2(centerX, centerY), radius + shadowOffset, shadowColor, numSegments);

            if (Config.AimBot)
            {
                vList.AddCircleFilled(new Vector2(centerX, centerY), radius, colorR, numSegments);
            }
            else
            {
                vList.AddCircleFilled(new Vector2(centerX, centerY), radius, colorG, numSegments);
            }
        }

        private float GetCameraYaw()
        {
            return MathF.Atan2(Core.CameraMatrix.M31, Core.CameraMatrix.M33);
        }

        void DrawSmallTextWithOutline(Vector2 pos, string text, uint textColor, uint outlineColor)
        {
            var vList = ImGui.GetForegroundDrawList();
            vList.AddText(pos + new Vector2(1, 1), outlineColor, text);
            vList.AddText(pos + new Vector2(-1, -1), outlineColor, text);
            vList.AddText(pos + new Vector2(1, -1), outlineColor, text);
            vList.AddText(pos + new Vector2(-1, 1), outlineColor, text);
            vList.AddText(pos, textColor, text);
        }
        private void DrawGlowText(ImDrawListPtr drawList, Vector2 pos, string text, Color color, float glowRadius = 4f)
        {
            uint main = ColorToUint32(color);
            uint glow = ColorToUint32(Color.FromArgb(160, color.R, color.G, color.B));

            for (float dx = -glowRadius; dx <= glowRadius; dx += 1.5f)
            {
                for (float dy = -glowRadius; dy <= glowRadius; dy += 1.5f)
                {
                    if (dx == 0 && dy == 0) continue;
                    drawList.AddText(pos + new Vector2(dx, dy), glow, text);
                }
            }

            drawList.AddText(pos + new Vector2(1, 1), ColorToUint32(Color.Black), text);
            drawList.AddText(pos + new Vector2(-1, -1), ColorToUint32(Color.Black), text);
            drawList.AddText(pos, main, text);
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
        }

        private float Clamp(float value, float min, float max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public void DrawGradientFillBox(float X, float Y, float W, float H, Color topColor, Color bottomColor)
        {
            var vList = ImGui.GetForegroundDrawList();
            int slices = 50;
            float sliceHeight = H / slices;

            for (int i = 0; i < slices; i++)
            {
                float t = (float)i / slices;
                Color sliceColor = LerpColor(topColor, bottomColor, t);
                uint sliceColorUint = ColorToUint32(sliceColor);
                vList.AddRectFilled(new Vector2(X, Y + i * sliceHeight),
                    new Vector2(X + W, Y + (i + 1) * sliceHeight), sliceColorUint);
            }
        }

        public void DrawCorneredBox(float X, float Y, float W, float H, uint color, float thickness)
        {
            var vList = ImGui.GetForegroundDrawList();
            float lineW = W / 3;
            float lineH = H / 3;

            vList.AddLine(new Vector2(X, Y), new Vector2(X + lineW, Y), color, thickness);
            vList.AddLine(new Vector2(X + W - lineW, Y), new Vector2(X + W, Y), color, thickness);
            vList.AddLine(new Vector2(X, Y), new Vector2(X, Y + lineH), color, thickness);
            vList.AddLine(new Vector2(X + W, Y), new Vector2(X + W, Y + lineH), color, thickness);
            vList.AddLine(new Vector2(X, Y + H - lineH), new Vector2(X, Y + H), color, thickness);
            vList.AddLine(new Vector2(X + W - lineW, Y + H), new Vector2(X + W, Y + H), color, thickness);
            vList.AddLine(new Vector2(X, Y + H), new Vector2(X + lineW, Y + H), color, thickness);
            vList.AddLine(new Vector2(X + W, Y + H), new Vector2(X + W, Y + H - lineH), color, thickness);
        }

        public void Draw3dBox(float X, float Y, float W, float H, uint color, float thickness, float glowRadius, float feather, float glowOpacityMultiplier)
        {
            DrawCorneredBox(X, Y, W, H, color, thickness);
        }

        private void DrawSkeleton(Entity entity)
        {
            var drawList = ImGui.GetForegroundDrawList();
            uint lineColor = ColorToUint32(Config.ESPSkeletonColor);
            uint circleColor = ColorToUint32(Color.Red);

            var headScreenPos          = W2S.WorldToScreen(Core.CameraMatrix, entity.Head,          Core.Width, Core.Height);
            var spineScreenPos         = W2S.WorldToScreen(Core.CameraMatrix, entity.Spine,         Core.Width, Core.Height);
            var hipScreenPos           = W2S.WorldToScreen(Core.CameraMatrix, entity.Hip,           Core.Width, Core.Height);
            var rootScreenPos          = W2S.WorldToScreen(Core.CameraMatrix, entity.Root,          Core.Width, Core.Height);
            var rightCalfScreenPos     = W2S.WorldToScreen(Core.CameraMatrix, entity.RightCalf,     Core.Width, Core.Height);
            var leftCalfScreenPos      = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftCalf,      Core.Width, Core.Height);
            var rightFootScreenPos     = W2S.WorldToScreen(Core.CameraMatrix, entity.RightFoot,     Core.Width, Core.Height);
            var leftFootScreenPos      = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftFoot,      Core.Width, Core.Height);
            var rightWristScreenPos    = W2S.WorldToScreen(Core.CameraMatrix, entity.RightWrist,    Core.Width, Core.Height);
            var leftWristScreenPos     = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftWrist,     Core.Width, Core.Height);
            var leftHandScreenPos      = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftHand,      Core.Width, Core.Height);
            var rightWristJointScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightWristJoint, Core.Width, Core.Height);
            var leftWristJointScreenPos  = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftWristJoint,  Core.Width, Core.Height);
            var leftShoulderScreenPos  = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftShoulder,  Core.Width, Core.Height);
            var rightShoulderScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightShoulder, Core.Width, Core.Height);
            var leftElbowScreenPos     = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftElbow,     Core.Width, Core.Height);
            var rightElbowScreenPos    = W2S.WorldToScreen(Core.CameraMatrix, entity.RightElbow,    Core.Width, Core.Height);

            // Upper body
            DrawLine(drawList, headScreenPos,          spineScreenPos,          lineColor);
            DrawLine(drawList, spineScreenPos,         rightShoulderScreenPos,  lineColor);
            DrawLine(drawList, spineScreenPos,         leftShoulderScreenPos,   lineColor);
            DrawLine(drawList, spineScreenPos,         hipScreenPos,            lineColor);
            // Right arm
            DrawLine(drawList, rightShoulderScreenPos, rightElbowScreenPos,     lineColor);
            DrawLine(drawList, rightElbowScreenPos,    rightWristScreenPos,     lineColor);
            DrawLine(drawList, rightWristScreenPos,    rightWristJointScreenPos, lineColor);
            // Left arm
            DrawLine(drawList, leftShoulderScreenPos,  leftElbowScreenPos,      lineColor);
            DrawLine(drawList, leftElbowScreenPos,     leftWristScreenPos,      lineColor);
            DrawLine(drawList, leftWristScreenPos,     leftHandScreenPos,       lineColor);
            DrawLine(drawList, leftHandScreenPos,      leftWristJointScreenPos, lineColor);
            // Lower body
            DrawLine(drawList, hipScreenPos,           rightCalfScreenPos,      lineColor);
            DrawLine(drawList, hipScreenPos,           leftCalfScreenPos,       lineColor);
            DrawLine(drawList, rightCalfScreenPos,     rightFootScreenPos,      lineColor);
            DrawLine(drawList, leftCalfScreenPos,      leftFootScreenPos,       lineColor);

            float distance = entity.Distance;
            float baseRadius = 50.0f;
            float circleRadius = baseRadius / distance;

            if (headScreenPos.X > 0 && headScreenPos.Y > 0)
                drawList.AddCircle(headScreenPos, circleRadius, circleColor, 30);
        }

        private void DrawLine(ImDrawListPtr drawList, Vector2 startPos, Vector2 endPos, uint color)
        {
            if (startPos.X > 0 && startPos.Y > 0 && endPos.X > 0 && endPos.Y > 0)
                drawList.AddLine(startPos, endPos, color, 1.5f);
        }

        public void DrawSmoothCircle(float radius, uint color, float thickness, int segments = 64)
        {
            var vList = ImGui.GetForegroundDrawList();
            var center = new Vector2(Core.Width / 2f, Core.Height / 2f);
            vList.AddCircle(center, radius, color, segments, thickness);
        }

        private Color LerpColor(Color start, Color end, float amount)
        {
            amount = Math.Max(0, Math.Min(1, amount));
            return Color.FromArgb(
                (int)(start.A + (end.A - start.A) * amount),
                (int)(start.R + (end.R - start.R) * amount),
                (int)(start.G + (end.G - start.G) * amount),
                (int)(start.B + (end.B - start.B) * amount)
            );
        }

        private static T GetESPColor<T>(T defaultColor)
        {
            if (Config.RGB)
            {
                if (typeof(T) == typeof(uint)) return (T)(object)LGBT();
                if (typeof(T) == typeof(Color)) return (T)(object)LGBTColor();
            }
            return defaultColor;
        }

        private static uint LGBT() => ColorToUint32(LGBTColor());
        private static Color LGBTColor()
        {
            float time = (float)ImGui.GetTime();
            return ColorFromHSV(time * 100 % 360, 1.0f, 1.0f);
        }

        public static Color ColorFromHSV(float hue, float saturation, float value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            float f = hue / 60 - (float)Math.Floor(hue / 60);
            value *= 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0) return Color.FromArgb(255, v, t, p);
            if (hi == 1) return Color.FromArgb(255, q, v, p);
            if (hi == 2) return Color.FromArgb(255, p, v, t);
            if (hi == 3) return Color.FromArgb(255, p, q, v);
            if (hi == 4) return Color.FromArgb(255, t, p, v);
            return Color.FromArgb(255, v, p, q);
        }

        static uint ColorToUint32(Color color) =>
            ImGui.ColorConvertFloat4ToU32(new Vector4(
                color.R / 255f,
                color.G / 255f,
                color.B / 255f,
                color.A / 255f));

        [DllImport("user32.dll")]
        static extern bool SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);
        const uint WDA_NONE = 0x00000000;
        const uint WDA_EXCLUDEFROMCAPTURE = 0x00000011;

        void CreateHandle()
        {
            GetWindowRect(Core.Handle, out RECT rect);
            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            ImGui.SetWindowSize(new Vector2(width, height));
            ImGui.SetWindowPos(new Vector2(rect.Left, rect.Top));

            Size = new System.Drawing.Size(width, height);
            Position = new System.Drawing.Point(rect.Left, rect.Top);

            Core.Width = width;
            Core.Height = height;

            if (Config.StreamMode)
                SetWindowDisplayAffinity(hWnd, WDA_EXCLUDEFROMCAPTURE);
            else
                SetWindowDisplayAffinity(hWnd, WDA_NONE);
        }
    }
}
