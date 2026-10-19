using ImGuiNET;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AotForms.Config;
using static AotForms.WinAPI;
using FearAuth;
using Color = System.Drawing.Color;
using Size = System.Drawing.Size;
using Point = System.Drawing.Point;

namespace AotForms
{
    internal class ESP : ClickableTransparentOverlay.Overlay
    {
        private bool _styleInitialized = false;
        private IntPtr hWnd;
        private const short DefaultMaxHealth = 200;
        private Vector4 lineColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        private Vector4 fovColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        private Vector4 boxColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        private Vector4 box3DColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        private Vector4 skeletonColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        private uint cyanColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0.8f, 1f, 1f));
        private static Dictionary<string, IntPtr> _weaponIcons = new();
        private IntPtr _brandIcon = IntPtr.Zero;
        private static readonly Random _hackerRandom = new();
        private static uint _hackerEntityAddress = 0;
        private static DateTime _lastHackerUpdate = DateTime.MinValue;
        private const double HackerUpdateIntervalSeconds = 3.0;
        bool showWindow = true;
        private readonly string[] _comboItems2 = { "Closest To Crosshair", "Target360", "Closest To Player", "Lowest Health" };
        private readonly string[] _comboItems1 = { "Silent Aim", "Aimbot Rage" };
        private readonly string[] _comboItems = { "Top", "Center","Bottom" };
        private readonly string[] _headerItems = { "Aim", "Esp", "Colors","Extras","Misc" };
        private int _selectedHeader,_comboBox,_comboBox1,_comboBox2;
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
        private readonly Dictionary<string, float> _toggleAnim = new Dictionary<string, float>();
        private string _bindingLabel = "";

        private struct MenuParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Life;
            public float Alpha;
            public uint Color;
        }
        private List<MenuParticle> particles = new List<MenuParticle>();
        private static readonly Random particleRnd = new Random();

        private void SpawnSplash(Vector2 pos)
        {
            Vector2 spawnPos = ImGui.GetMousePos(); 
            for (int i = 0; i < 40; i++)
            {
                particles.Add(new MenuParticle
                {
                    Position = spawnPos,
                    Velocity = new Vector2((float)particleRnd.NextDouble() * 6f - 3f, (float)particleRnd.NextDouble() * 4f - 3f),
                    Life = 1.2f,
                    Alpha = 1.0f,
                    Color = ImGui.GetColorU32(new Vector4(clr_accent.X, clr_accent.Y, clr_accent.Z, 0.8f))
                });
            }
        }

        private void UpdateParticles()
        {
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                var p = particles[i];
                p.Position += p.Velocity;
                p.Velocity.Y += 0.05f; 
                p.Life -= 0.02f;
                p.Alpha = Math.Max(0, p.Life);
                if (p.Life <= 0) particles.RemoveAt(i);
                else particles[i] = p;
            }
        }

        private void DrawParticles(ImDrawListPtr dl)
        {
            foreach (var p in particles)
            {
                dl.AddCircleFilled(p.Position, 1.5f, ImGui.GetColorU32(new Vector4(clr_accent.X, clr_accent.Y, clr_accent.Z, p.Alpha)));
            }
        }


        Vector4 HsvToRgb(float h, float s, float v)
        {
            int i = (int)(h * 6);
            float f = h * 6 - i;
            float p = v * (1 - s);
            float q = v * (1 - f * s);
            float t = v * (1 - (1 - f) * s);
            float r = 0, g = 0, b = 0;
            switch (i % 6)
            {
                case 0: r = v; g = t; b = p; break;
                case 1: r = q; g = v; b = p; break;
                case 2: r = p; g = v; b = t; break;
                case 3: r = p; g = q; b = v; break;
                case 4: r = t; g = p; b = v; break;
                case 5: r = v; g = p; b = q; break;
            }
            return new Vector4(r, g, b, 1f);
        }

        private void StartEntityProcessing()
        {

        }

        private void RenderEntities()
        {
            var bgDl = ImGui.GetBackgroundDrawList();
            var fgDl = ImGui.GetForegroundDrawList();
            var matrix = Core.CameraMatrix;

            if (Config.HackerTag)
            {
                if ((DateTime.Now - _lastHackerUpdate).TotalSeconds > HackerUpdateIntervalSeconds || _hackerEntityAddress == 0)
                {
                    var valid = Core.Entities.Values.Where(e => !e.IsDead && e.IsKnown).ToList();
                    _hackerEntityAddress = valid.Count > 0 ? valid[_hackerRandom.Next(valid.Count)].Address : 0;
                    _lastHackerUpdate = DateTime.Now;
                }
            }

            foreach (var entity in Core.Entities.Values)
            {
                if (entity.IsDead || !entity.IsKnown) continue;

                var headScreenPos = W2S.WorldToScreen(matrix, entity.Head, Core.Width, Core.Height);
                var bottomScreenPos = W2S.WorldToScreen(matrix, entity.Root, Core.Width, Core.Height);

                if (headScreenPos.X < 1 || headScreenPos.Y < 1) continue;

                float cornerHeight = Math.Abs(headScreenPos.Y - bottomScreenPos.Y);
                float cornerWidth = cornerHeight * 0.65f;
                float distance = Vector3.Distance(Core.LocalMainCamera, entity.Head);

                Config.LastEnemySeen = DateTime.Now;
                if (!Config.EnemyTimerActive) { Config.EnemyTimerActive = true; Config.EnemyTimerStart = DateTime.Now; }

                uint lineColor = ColorToUint32(Config.ESPLineColor);
                uint boxColor = ColorToUint32(Config.ESPBoxColor);

                if (Config.ESPRGB)
                {
                    Color rainbow = ColorFromHSV((float)(ImGui.GetTime() * 100 % 360), 1.0f, 1.0f);
                    lineColor = ColorToUint32(rainbow); boxColor = lineColor;
                }

                if (Config.ESPLine)
                {
                    Vector2 lineStart = new Vector2(Core.Width / 2f, 10f);
                    Vector2 endPos = headScreenPos;
                    switch (Config.ESPLinePosition)
                    {
                        case LinePosition.Top: lineStart = new Vector2(Core.Width / 2f, 10f); break;
                        case LinePosition.Center: lineStart = new Vector2(Core.Width / 2f, Core.Height / 2f); break;
                        case LinePosition.Bottom: lineStart = new Vector2(Core.Width / 2f, Core.Height - 10f); endPos = bottomScreenPos; break;
                    }
                    bgDl.AddLine(lineStart, endPos, lineColor, 1f);
                }

                if (Config.ESPBox) DrawCorneredBox(headScreenPos.X - (cornerWidth / 2), headScreenPos.Y, cornerWidth, cornerHeight, boxColor, 1.5f);

                if (Config.ESPName || Config.ESPHealth || Config.espweapon)
                {
                    var nameText = string.IsNullOrWhiteSpace(entity.Name) ? "BOT" : entity.Name;
                    Vector2 fixedNameSize = new Vector2(95, 16);
                    Vector2 namePos = new Vector2(headScreenPos.X - fixedNameSize.X / 2, headScreenPos.Y - fixedNameSize.Y - 15);

                    if (Config.ESPName)
                    {
                        Vector2 textSizeName = ImGui.CalcTextSize(nameText);
                        Vector2 textSizeDistance = ImGui.CalcTextSize($" {MathF.Round(distance)}m");
                        Vector2 textPosName = new Vector2(namePos.X + 5, namePos.Y + (fixedNameSize.Y - textSizeName.Y) / 2);
                        Vector2 textPosDistance = new Vector2(namePos.X + fixedNameSize.X - textSizeDistance.X + 5, namePos.Y + (fixedNameSize.Y - textSizeDistance.Y) / 2);

                        fgDl.AddRectFilled(namePos, namePos + new Vector2(fixedNameSize.X, fixedNameSize.Y), ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 0.6f)), 4f);
                        fgDl.AddText(textPosName, ColorToUint32(Config.ESPNameColor), nameText);
                        fgDl.AddText(textPosDistance, ColorToUint32(Config.ESPNameColor), $" {MathF.Round(distance)}m");
                    }

                    if (Config.HackerTag && entity.Address == _hackerEntityAddress)
                    {
                        string hackerText = "HACKER"; Vector2 hackerSize = ImGui.CalcTextSize(hackerText);
                        Vector2 hackerPos = new Vector2(namePos.X + (fixedNameSize.X - hackerSize.X) * 0.5f, namePos.Y - hackerSize.Y - 3f);
                        fgDl.AddText(hackerPos + new Vector2(1, 1), ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 1)), hackerText);
                        fgDl.AddText(hackerPos, ColorToUint32(Config.HackerTagColor), hackerText);
                    }

                    if (Config.espweapon)
                    {
                        string weaponFileName = $"{entity.WeaponName?.ToLower()}.png";
                        string resourceName = $"pixel.Resources.Icons.{weaponFileName}";
                        if (!_weaponIcons.TryGetValue(weaponFileName, out var imagehandle))
                        {
                            try {
                                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                                {
                                    if (stream != null) {
                                        using (var bitmap = new Bitmap(stream)) {
                                            string tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");
                                            bitmap.Save(tempPath);
                                            AddOrGetImagePointer(tempPath, true, out imagehandle, out var w, out var h);
                                            _weaponIcons[weaponFileName] = imagehandle;
                                            try { File.Delete(tempPath); } catch { }
                                        }
                                    }
                                }
                            } catch {}
                        }
                        if (imagehandle != IntPtr.Zero)
                        {
                            Vector2 iconSize = new Vector2(60 * Config.iconsize, 20 * Config.iconsize);
                            Vector2 iconPos = new Vector2(namePos.X + (fixedNameSize.X - iconSize.X) / 2, namePos.Y - iconSize.Y - 2);
                            fgDl.AddImage(imagehandle, iconPos, iconPos + iconSize, Vector2.Zero, Vector2.One, ImGui.ColorConvertFloat4ToU32(Config.ICONCOLOR));
                        }
                    }

                    if (Config.ESPHealth)
                    {
                        Vector2 healthBarPos = new Vector2(headScreenPos.X + (cornerWidth / 2) + 5, headScreenPos.Y);
                        float healthPercentage = Math.Clamp((float)entity.Health / (entity.Health > 230 ? 500 : 200), 0, 1);
                        uint hCol = healthPercentage > 0.8f ? ColorToUint32(Color.Lime) : healthPercentage > 0.4f ? ColorToUint32(Color.Yellow) : ColorToUint32(Color.Red);
                        fgDl.AddRectFilled(healthBarPos, healthBarPos + new Vector2(3, cornerHeight), 0x90000000, 2f);
                        fgDl.AddRectFilled(new Vector2(healthBarPos.X, healthBarPos.Y + cornerHeight * (1 - healthPercentage)), healthBarPos + new Vector2(3, cornerHeight), hCol, 2f);
                    }
                }

                if (Config.ESPSkeleton) DrawSkeleton(entity, matrix);
            }

            if (Config.EspTimer && Config.EnemyTimerActive)
            {
                int remaining = Config.EnemyTimerDuration - (int)(DateTime.Now - Config.EnemyTimerStart).TotalSeconds;
                string displayText = remaining > 0 ? $"InValid Timer {remaining}s" : " END THIS GAME ";
                Vector2 tSize = ImGui.CalcTextSize(displayText);
                float hue = (float)(DateTime.Now.TimeOfDay.TotalSeconds * 2 % 360);
                fgDl.AddText(new Vector2((Core.Width - tSize.X) / 2, 100), ColorToUint32(ColorFromHSV(hue, 1f, 1f)), displayText);
            }
        }

        protected override unsafe void Render()
        {
            if (!string.IsNullOrEmpty(_bindingLabel))
            {
                foreach (Keys k in Enum.GetValues(typeof(Keys)))
                {
                    if (k == Keys.None || k == Keys.LButton || k == Keys.Insert) continue;
                    if ((GetAsyncKeyState(k) & 0x8000) != 0)
                    {
                        UpdateKeyBind(_bindingLabel, k);
                        _bindingLabel = "";
                        break;
                    }
                }
            }

            HandleKeybinds();
            CreateHandle();
            if (GetAsyncKeyState(Keys.Insert) < 0) { showWindow = !showWindow; Thread.Sleep(200); }
            if (showWindow) RenderUI();
            if (!Core.HaveMatrix) return;
            if (Config.FOVEnabled) DrawFOVCircle(Config.AimFov);
            if (Config.AimTrackLine)
            {
                Vector2 screenCenter = new Vector2(Core.Width / 2f, Core.Height / 2f);
                Entity target = null; Vector2 targetPos = Vector2.Zero; float minD = float.MaxValue;
                foreach (var e in Core.Entities.Values) {
                    if (e.IsDead || !e.IsKnown) continue;
                    var hp = W2S.WorldToScreen(Core.CameraMatrix, e.Head, Core.Width, Core.Height);
                    if (hp.X < 1) continue;
                    float d = Vector2.Distance(screenCenter, hp);
                    if (d < minD && (!Config.FOVEnabled || d <= Config.AimFov)) { minD = d; target = e; targetPos = hp; }
                }
                if (target != null) DrawGlowLine(screenCenter, targetPos, ColorToUint32(Config.AimTrackLineColor), 0.8f, 4.0f, 1.0f, 0.6f);
            }
            RenderEntities();
        }

        private void DrawGlowLine(Vector2 start, Vector2 end, uint color, float thickness, float glowRadius, float feather, float glowOpacityMultiplier)
        {
            var drawList = ImGui.GetBackgroundDrawList();
            Vector4 colorVec = ImGui.ColorConvertU32ToFloat4(color);
            for (float i = glowRadius; i > 0; i -= feather) {
                float alpha = Math.Clamp(colorVec.W * (i / glowRadius) * glowOpacityMultiplier, 0, 1);
                uint glowColor = ImGui.ColorConvertFloat4ToU32(new Vector4(colorVec.X, colorVec.Y, colorVec.Z, alpha));
                drawList.AddLine(start, end, glowColor, thickness + (glowRadius - i) * 0.5f);
            }
            drawList.AddLine(start, end, color, thickness);
        }

        private void DrawLine(ImDrawListPtr drawList, Vector2 startPos, Vector2 endPos, uint color)
        {
            if (startPos.X > 0 && endPos.X > 0) drawList.AddLine(startPos, endPos, color, 1.5f);
        }

        private void DrawSkeleton(Entity entity, Matrix4x4 matrix)
        {
            var drawList = ImGui.GetBackgroundDrawList();
            uint lineColor = ColorToUint32(Config.ESPSkeletonColor);
            var head = W2S.WorldToScreen(matrix, entity.Head, Core.Width, Core.Height);
            var spine = W2S.WorldToScreen(matrix, entity.Spine, Core.Width, Core.Height);
            var hip = W2S.WorldToScreen(matrix, entity.Hip, Core.Width, Core.Height);
            var rSh = W2S.WorldToScreen(matrix, entity.RightShoulder, Core.Width, Core.Height);
            var lSh = W2S.WorldToScreen(matrix, entity.LeftShoulder, Core.Width, Core.Height);
            var rFt = W2S.WorldToScreen(matrix, entity.RightFoot, Core.Width, Core.Height);
            var lFt = W2S.WorldToScreen(matrix, entity.LeftFoot, Core.Width, Core.Height);
            DrawLine(drawList, spine, rSh, lineColor); DrawLine(drawList, spine, hip, lineColor);
            DrawLine(drawList, spine, lSh, lineColor); DrawLine(drawList, hip, rFt, lineColor); DrawLine(drawList, hip, lFt, lineColor);
            if (head.X > 0) drawList.AddCircle(head, 50.0f / entity.Distance, ColorToUint32(Color.White), 30);
        }

        public void DrawCorneredBox(float X, float Y, float W, float H, uint color, float thickness)
        {
            var vList = ImGui.GetBackgroundDrawList();
            float lineW = W / 3; float lineH = H / 3;
            vList.AddLine(new Vector2(X, Y), new Vector2(X, Y + lineH), color, thickness);
            vList.AddLine(new Vector2(X, Y), new Vector2(X + lineW, Y), color, thickness);
            vList.AddLine(new Vector2(X + W - lineW, Y), new Vector2(X + W, Y), color, thickness);
            vList.AddLine(new Vector2(X + W, Y), new Vector2(X + W, Y + lineH), color, thickness);
            vList.AddLine(new Vector2(X, Y + H - lineH), new Vector2(X, Y + H), color, thickness);
            vList.AddLine(new Vector2(X, Y + H), new Vector2(X + lineW, Y + H), color, thickness);
            vList.AddLine(new Vector2(X + W - lineW, Y + H), new Vector2(X + W, Y + H), color, thickness);
            vList.AddLine(new Vector2(X + W, Y + H - lineH), new Vector2(X + W, Y + H), color, thickness);
        }

        static uint ColorToUint32(Color color) => ImGui.ColorConvertFloat4ToU32(new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f));

        private static Color ColorFromHSV(float hue, float saturation, float value)
        {
            int hi = System.Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            float f = hue / 60 - (float)Math.Floor(hue / 60);
            value *= 255; int v = System.Convert.ToInt32(value);
            int p = System.Convert.ToInt32(value * (1 - saturation));
            int q = System.Convert.ToInt32(value * (1 - f * saturation));
            int t = System.Convert.ToInt32(value * (1 - (1 - f) * saturation));
            return hi switch { 0 => Color.FromArgb(255, v, t, p), 1 => Color.FromArgb(255, q, v, p), 2 => Color.FromArgb(255, p, v, t), 3 => Color.FromArgb(255, p, q, v), 4 => Color.FromArgb(255, t, p, v), _ => Color.FromArgb(255, v, p, q) };
        }

        private int lastWidth = 0, lastHeight = 0;
        void CreateHandle()
        {
            SetWindowDisplayAffinity(hWnd, Config.StreamMode ? WDA_EXCLUDEFROMCAPTURE : WDA_NONE);
            GetWindowRect(Core.Handle, out RECT rect);
            int width = rect.Right - rect.Left, height = rect.Bottom - rect.Top;
            if (width != lastWidth || height != lastHeight) { lastWidth = width; lastHeight = height; ImGui.SetWindowSize(new Vector2(width, height)); }
            Size = new Size(width, height); Position = new Point(rect.Left, rect.Top);
            Core.Width = width; Core.Height = height;
        }

        public void DrawFOVCircle(float radius) => ImGui.GetBackgroundDrawList().AddCircle(new Vector2(Core.Width / 2f, Core.Height / 2f), radius, ColorToUint32(Config.FOVColor), 0, 1f);

        private Vector4 clr_bg_main = new Vector4(0.04f, 0.04f, 0.05f, 1.00f);
        private Vector4 clr_sidebar = new Vector4(0.06f, 0.06f, 0.08f, 1.00f);
        private Vector4 clr_accent = new Vector4(1.0f, 0.078f, 0.0f, 1.00f);
        private Vector4 clr_accent_dim = new Vector4(1.0f, 0.078f, 0.0f, 0.40f);
        private Vector4 clr_text_dim = new Vector4(0.55f, 0.55f, 0.60f, 1.00f);
        private Vector4 clr_text_bright = new Vector4(0.95f, 0.95f, 1.00f, 1.00f);
        private Vector4 clr_item_bg = new Vector4(0.08f, 0.08f, 0.10f, 1.00f);

        private void ApplyPremiumStyle()
        {
            var style = ImGui.GetStyle();
            style.WindowRounding = 12f;
            style.FrameRounding = 6f;
            style.ChildRounding = 8f;
            style.GrabRounding = 6f;
            style.WindowPadding = new Vector2(0, 0);
            style.FramePadding = new Vector2(10, 5);
            style.ItemSpacing = new Vector2(8, 8);

            var colors = style.Colors;
            colors[(int)ImGuiCol.WindowBg] = clr_bg_main;
            colors[(int)ImGuiCol.ChildBg] = new Vector4(0, 0, 0, 0);
            colors[(int)ImGuiCol.Text] = clr_text_bright;
            colors[(int)ImGuiCol.CheckMark] = clr_accent;
            colors[(int)ImGuiCol.SliderGrab] = clr_accent;
            colors[(int)ImGuiCol.SliderGrabActive] = clr_accent;
            colors[(int)ImGuiCol.Button] = new Vector4(0, 0, 0, 0);
            colors[(int)ImGuiCol.ButtonHovered] = new Vector4(clr_accent.X, clr_accent.Y, clr_accent.Z, 0.15f);
            colors[(int)ImGuiCol.ButtonActive] = new Vector4(clr_accent.X, clr_accent.Y, clr_accent.Z, 0.25f);
            colors[(int)ImGuiCol.FrameBg] = clr_item_bg;
            colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.12f, 0.12f, 0.15f, 1.00f);
            colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.15f, 0.15f, 0.18f, 1.00f);
        }

        private Vector4 Lerp(Vector4 a, Vector4 b, float t) => new Vector4(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t, a.Z + (b.Z - a.Z) * t, a.W + (b.W - a.W) * t);

        private string _loginMessage = "";
        private Vector4 _loginMsgColor = new Vector4(1, 1, 1, 1);
        private bool _isLoggingIn = false;
        private int _selectedTab = 0;

        public void RenderUI()
        {
            ApplyPremiumStyle();
            UpdateParticles();
            if (!Config.IsLoggedIn) { RenderLogin(); return; }

            ImGui.SetNextWindowSize(new Vector2(400, 380), ImGuiCond.Once);

            if (ImGui.Begin("##FEAR_MODS_BALANCED", ref showWindow, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoBackground))
            {
                var dl = ImGui.GetWindowDrawList();
                var pos = ImGui.GetWindowPos();
                var size = ImGui.GetWindowSize();

                dl.AddRectFilled(pos, pos + size, ImGui.GetColorU32(clr_bg_main), 12f);
                dl.AddRect(pos, pos + size, ImGui.GetColorU32(new Vector4(clr_accent.X, clr_accent.Y, clr_accent.Z, 0.15f)), 12f, ImDrawFlags.None, 1.5f);

                DrawParticles(dl);

                float tabSize = 28f;
                float tabSpacing = 4f;
                float totalTabsWidth = (tabSize * 3) + (tabSpacing * 2);
                float tabsX = size.X - totalTabsWidth - 15f;

                ImGui.SetCursorPos(new Vector2(0, 0));
                ImGui.InvisibleButton("##HeaderDrag", new Vector2(tabsX - 10, 45));
                if (ImGui.IsItemActive() && ImGui.IsMouseDragging(0))
                    ImGui.SetWindowPos(pos + ImGui.GetIO().MouseDelta);

                DrawShimmerText("FEAR MODS", pos + new Vector2(20, 24), 30f, true);

                ImGui.SetCursorPos(new Vector2(tabsX, 12));

                string[] tab_icons = { "\ue176", "\ue0b5", "\ue156" };
                for (int i = 0; i < tab_icons.Length; i++)
                {
                    if (DrawTopTab(tab_icons[i], i, new Vector2(tabSize, tabSize))) _selectedTab = i;
                    if (i < tab_icons.Length - 1) ImGui.SameLine(0, tabSpacing);
                }

                dl.AddLine(pos + new Vector2(15, 55), pos + new Vector2(size.X - 15, 55), ImGui.GetColorU32(new Vector4(1, 1, 1, 0.05f)));

                ImGui.SetCursorPos(new Vector2(20, 65));
                ImGui.BeginChild("##MainScroll", new Vector2(size.X - 40, size.Y - 110), ImGuiChildFlags.None, ImGuiWindowFlags.None);
                {
                    if (_selectedTab == 0) RenderCombatTab();
                    else if (_selectedTab == 1) RenderVisualsTab();
                    else if (_selectedTab == 2) RenderSettingsTab();
                }
                ImGui.EndChild();

                ImGui.End();
            }
        }

        private bool DrawTopTab(string icon, int id, Vector2 size)
        {
            bool active = _selectedTab == id;
            var pos = ImGui.GetCursorScreenPos();
            var dl = ImGui.GetWindowDrawList();
            bool hovered = ImGui.IsMouseHoveringRect(pos, pos + size);

            ImGui.PushStyleColor(ImGuiCol.Header, new Vector4(0, 0, 0, 0));
            ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0, 0, 0, 0));
            ImGui.PushStyleColor(ImGuiCol.HeaderActive, new Vector4(0, 0, 0, 0));
            bool clicked = ImGui.Selectable("##tab_" + id, active, ImGuiSelectableFlags.None, size);
            ImGui.PopStyleColor(3);

            if (active)
            {
                ImGui.SetWindowFontScale(1.3f);
                Vector2 iconSize = ImGui.CalcTextSize(icon);
                dl.AddText(pos + new Vector2((size.X - iconSize.X) / 2, (size.Y - iconSize.Y) / 2 - 2f), ImGui.GetColorU32(clr_accent), icon);
                ImGui.SetWindowFontScale(1.0f);

                // Sleek active accent indicator line under icon
                dl.AddRectFilled(pos + new Vector2((size.X - 16f) / 2, size.Y - 2f), pos + new Vector2((size.X + 16f) / 2, size.Y), ImGui.GetColorU32(clr_accent), 1f);
            }
            else
            {
                ImGui.SetWindowFontScale(1.2f);
                Vector2 iconSize = ImGui.CalcTextSize(icon);
                dl.AddText(pos + new Vector2((size.X - iconSize.X) / 2, (size.Y - iconSize.Y) / 2), ImGui.GetColorU32(hovered ? clr_text_bright : clr_text_dim), icon);
                ImGui.SetWindowFontScale(1.0f);
            }

            return clicked;
        }

        private void RenderCombatTab()
        {
            DrawCategoryHeader("Internal Aims", Lucide.Target);
            DrawFeatureCard("Enable Combat", "Core functionality", ref Config.CombatEnabled);
            DrawFeatureCard("Aimbot Visible", "Check for visibility", ref Config.AimbotVisible);
            DrawFeatureCard("Fear Collider", "Collider engine aim", ref Config.FearCollider);
            DrawFeatureCard("Ghost Lock", "Ghost Lock", ref Config.GhostLock);
            DrawFeatureCard("Ghost Sniper", "Ghost Lock Sniper Mode", ref Config.GhostLockSniperMode);

            DrawCategoryHeader("Weapon Modifications", Lucide.Zap);
            DrawFeatureCard("No Recoil", "Steady weapon", ref Config.NoRecoil);
            DrawFeatureCard("Fast Reload", "Instant clip change", ref Config.FastReload);

            ImGui.Dummy(new Vector2(0, 2));
            DrawCategoryHeader("Global Settings", Lucide.Settings);
            FilledSliderFloat("Aim Smoothness", ref Config.AimBotSmooth, 0f, 50f);
            FilledSliderFloat("Global FOV", ref Config.AimFov, 10f, 800f);
            FilledSliderFloat("Max Distance", ref Config.AimBotMaxDistance, 10f, 1000f);
        }

        private void RenderVisualsTab()
        {
            DrawCategoryHeader("Master Toggle", Lucide.Eye);
            DrawFeatureCard("Global ESP", "Master visual switch", ref Config.VisualsEnabled);

            ImGui.Dummy(new Vector2(0, 2));
            DrawCategoryHeader("ESP Modules", Lucide.Scan);
            DrawFeatureCard("Bounding Boxes", "Draw player box", ref Config.ESPBox);
            DrawFeatureCard("Bone Skeleton", "Full body track", ref Config.ESPSkeleton);
            DrawFeatureCard("Snap Lines", "Direction line", ref Config.ESPLine);
            DrawFeatureCard("Player Names", "Identity tag", ref Config.ESPName);
            DrawFeatureCard("Health Bars", "Vital tracking", ref Config.ESPHealth);
            DrawFeatureCard("Weapon ESP", "Held weapon info", ref Config.espweapon);

            ImGui.Dummy(new Vector2(0, 2));
            DrawCategoryHeader("Radius Overlay", Lucide.Circle);
            DrawFeatureCard("Show FOV Circle", "Visual aim area", ref Config.FOVEnabled);
        }

        private void RenderMiscTab()
        {
            DrawCategoryHeader("Advanced Exploits", Lucide.ShieldCheck);
            DrawFeatureCard("Speed Boost", "Game timer speed hack", ref Config.speedint);
            DrawFeatureCard("Spin Bot", "Rapid rotation", ref Config.spinbot);
        }

        private void RenderKeybindsTab()
        {
            DrawSectionHeader("COMBAT BINDINGS");
            RenderKeyItem("Aimbot Key", ref Config.AimbotKey);

            ImGui.Dummy(new Vector2(0, 5));
            DrawSectionHeader("MISC BINDINGS");
            RenderKeyItem("Spin Bot", ref Config.KeySpinBot);
            RenderKeyItem("Speed Boost", ref Config.KeySpeedBoostTimer);
            RenderKeyItem("Fast Reload", ref Config.KeyFastReload);
        }

        private void RenderKeyItem(string label, ref Keys key)
        {
            ImGui.Text(label + ":");
            ImGui.SameLine(150);

            string btnText = _bindingLabel == label ? "???" : key.ToString();
            ImGui.PushStyleColor(ImGuiCol.Button, _bindingLabel == label ? clr_accent : clr_item_bg);
            if (ImGui.Button($"{btnText}##{label}", new Vector2(ImGui.GetContentRegionAvail().X, 22)))
            {
                _bindingLabel = label;
            }
            ImGui.PopStyleColor();
            ImGui.Separator();
        }

        private void UpdateKeyBind(string label, Keys newKey)
        {
            switch (label)
            {
                case "Aimbot Key": Config.AimbotKey = newKey; break;
                case "Spin Bot": Config.KeySpinBot = newKey; break;
                case "Speed Boost": Config.KeySpeedBoostTimer = newKey; break;
                case "Fast Reload": Config.KeyFastReload = newKey; break;
            }
        }

        private void RenderSettingsTab()
        {
            DrawCategoryHeader("Menu Customization", Lucide.Palette);
            ImGui.Text("Menu Accent Color");
            ImGui.ColorEdit4("##AccentColor", ref clr_accent, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar);

            ImGui.Dummy(new Vector2(0, 10));
            DrawCategoryHeader("Recording Protect", Lucide.Shield);
            DrawFeatureCard("Stream Mode", "Hides menu from OBS/Discord", ref Config.StreamMode);

            ImGui.Dummy(new Vector2(0, 10));
            DrawCategoryHeader("System Controls", Lucide.Power);

            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.2f, 0.2f, 0.4f));
            if (ImGui.Button("ENVIRONMENT EXIT", new Vector2(ImGui.GetContentRegionAvail().X, 35)))
            {
                Environment.Exit(0);
            }
            ImGui.PopStyleColor();
        }

        private void DrawSectionHeader(string label)
        {
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 2);
            ImGui.TextColored(new Vector4(clr_accent.X, clr_accent.Y, clr_accent.Z, 0.6f), label);
            ImGui.Separator();
        }

        private static bool _keyLoaded = false;

        private void RenderLogin()
        {
            if (!_keyLoaded)
            {
                _keyLoaded = true;
                try
                {
                    if (System.IO.File.Exists("license.key"))
                    {
                        string savedKey = System.IO.File.ReadAllText("license.key").Trim();
                        if (!string.IsNullOrEmpty(savedKey))
                            Config.Password = savedKey;
                    }
                }
                catch { }
            }

            ImGui.SetNextWindowSize(new Vector2(340, 320));
            ImGui.SetNextWindowPos(new Vector2(Core.Width / 2 - 170, Core.Height / 2 - 160), ImGuiCond.Appearing);

            if (ImGui.Begin("##LoginWindow", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoBackground))
            {
                var dl = ImGui.GetWindowDrawList(); var pos = ImGui.GetWindowPos(); var size = ImGui.GetWindowSize();

                dl.AddRectFilled(pos, pos + size, ImGui.GetColorU32(clr_bg_main), 16f);
                dl.AddRect(pos, pos + size, ImGui.GetColorU32(new Vector4(clr_accent.X, clr_accent.Y, clr_accent.Z, 0.15f)), 16f, ImDrawFlags.None, 1.2f);

                ImGui.SetCursorPos(new Vector2(0, 0)); ImGui.InvisibleButton("##DragLogin", new Vector2(size.X, 60));
                if (ImGui.IsItemActive() && ImGui.IsMouseDragging(0)) ImGui.SetWindowPos(pos + ImGui.GetIO().MouseDelta);

                DrawShimmerTitle("FEAR MODS", pos + new Vector2(size.X / 2, 50), 36f);

                // 1. Centered LICENSE AUTHENTICATION
                string t1 = "LICENSE AUTHENTICATION";
                Vector2 s1 = ImGui.CalcTextSize(t1);
                ImGui.SetCursorPos(new Vector2((size.X - s1.X) / 2f, 98));
                ImGui.TextColored(clr_text_dim, t1);

                // 2. Centered Enter License Key
                string t2 = "Enter License Key";
                Vector2 s2 = ImGui.CalcTextSize(t2);
                ImGui.SetCursorPos(new Vector2((size.X - s2.X) / 2f, 132));
                ImGui.TextColored(new Vector4(0.9f, 0.9f, 0.95f, 1f), t2);
                
                // 3. Centered Input Box & Dynamic Centered License Key Text
                float boxWidth = 280f;
                float boxX = (size.X - boxWidth) / 2f;
                ImGui.SetCursorPos(new Vector2(boxX, 158)); 
                ImGui.SetNextItemWidth(boxWidth);

                string currentKey = Config.Password ?? "";
                float keyTextWidth = ImGui.CalcTextSize(currentKey).X;
                float padX = Math.Max(12f, (boxWidth - keyTextWidth) / 2f);

                ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.12f, 0.12f, 0.16f, 1.0f));
                ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, new Vector4(0.16f, 0.16f, 0.22f, 1.0f));
                ImGui.PushStyleColor(ImGuiCol.FrameBgActive, new Vector4(0.14f, 0.14f, 0.20f, 1.0f));
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(clr_accent.X, clr_accent.Y, clr_accent.Z, 0.6f));
                ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 6.0f);
                ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1.2f);
                ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(padX, 8));

                ImGui.InputText("##LicenseKey", ref Config.Password, 100);

                ImGui.PopStyleVar(3);
                ImGui.PopStyleColor(5);

                if (!string.IsNullOrEmpty(_loginMessage)) {
                    Vector2 msgSize = ImGui.CalcTextSize(_loginMessage);
                    ImGui.SetCursorPos(new Vector2((size.X - msgSize.X) / 2f, 210));
                    ImGui.TextColored(_loginMsgColor, _loginMessage);
                }

                ImGui.SetCursorPos(new Vector2(30, 245));
                ImGui.PushStyleColor(ImGuiCol.Button, clr_accent);
                if (ImGui.Button(_isLoggingIn ? "VERIFYING..." : "ACTIVATE LICENSE", new Vector2(280, 45)) && !_isLoggingIn)
                {
                    if (string.IsNullOrEmpty(Config.Password)) { _loginMessage = "Key cannot be empty!"; _loginMsgColor = new Vector4(1f, 0.3f, 0.3f, 1f); return; }
                    _isLoggingIn = true; _loginMessage = "Validating..."; _loginMsgColor = clr_text_dim;
                    Task.Run(() => {
                        try {
                            Loader.FearAuthApp.license(Config.Password);
                            if (Loader.FearAuthApp.response.success) {
                                try { System.IO.File.WriteAllText("license.key", Config.Password); } catch { }
                                _loginMessage = "License Activated!"; _loginMsgColor = new Vector4(0.2f, 1.0f, 0.2f, 1.0f);
                                Thread.Sleep(600); Config.IsLoggedIn = true; Loader.StartCheats();
                            } else { _loginMessage = Loader.FearAuthApp.response.message; _loginMsgColor = new Vector4(1.0f, 0.3f, 0.3f, 1.0f); }
                        } catch (Exception ex) { _loginMessage = "Error: " + ex.Message; _loginMsgColor = new Vector4(1.0f, 0.3f, 0.3f, 1.0f); }
                        finally { _isLoggingIn = false; }
                    });
                }
                ImGui.PopStyleColor();
                ImGui.End();
            }
        }

        private void DrawShimmerText(string text, Vector2 centerPos, float fontSize, bool leftAlign = false)
        {
            var dl = ImGui.GetWindowDrawList(); float time = (float)ImGui.GetTime();
            ImGui.SetWindowFontScale(fontSize / 20f); 
            Vector2 textSize = ImGui.CalcTextSize(text);
            Vector2 pos = leftAlign ? (centerPos - new Vector2(0, textSize.Y / 2)) : (centerPos - new Vector2(textSize.X / 2, textSize.Y / 2));

            dl.AddText(pos + new Vector2(1.5f, 1.5f), ImGui.GetColorU32(new Vector4(0, 0, 0, 0.4f)), text);

            for (int i = 0; i < text.Length; i++) {
                char c = text[i]; string s = c.ToString();
                float shimmer = (float)Math.Sin(time * 3.5 + (float)i / text.Length * 5.0) * 0.5f + 0.5f;
                Vector4 color = Lerp(new Vector4(1f, 1f, 1f, 1f), clr_accent, shimmer);
                dl.AddText(pos, ImGui.GetColorU32(color), s);
                pos.X += ImGui.CalcTextSize(s).X;
            }
            ImGui.SetWindowFontScale(1.0f);
        }

        private void DrawShimmerTitle(string text, Vector2 centerPos, float fontSize)
        {
            DrawShimmerText(text, centerPos, fontSize, false);
        }



        private void DrawCategoryHeader(string label, string icon = "")
        {
            var dl = ImGui.GetWindowDrawList();
            var pos = ImGui.GetCursorScreenPos();
            float width = ImGui.GetContentRegionAvail().X;
            float height = 30f;
            float rounding = 6f;

            // Full width dark container with subtle border
            dl.AddRectFilled(pos, pos + new Vector2(width, height), ImGui.GetColorU32(new Vector4(0.06f, 0.06f, 0.08f, 0.85f)), rounding);
            
            // Left-to-right stylish accent gradient
            dl.AddRectFilledMultiColor(pos, pos + new Vector2(width * 0.5f, height),
                ImGui.GetColorU32(new Vector4(clr_accent.X, clr_accent.Y, clr_accent.Z, 0.16f)),
                ImGui.GetColorU32(new Vector4(0, 0, 0, 0)),
                ImGui.GetColorU32(new Vector4(0, 0, 0, 0)),
                ImGui.GetColorU32(new Vector4(clr_accent.X, clr_accent.Y, clr_accent.Z, 0.16f)));

            // Outer border
            dl.AddRect(pos, pos + new Vector2(width, height), ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 0.06f)), rounding, ImDrawFlags.None, 1f);

            // Left glowing accent bar
            dl.AddRectFilled(pos + new Vector2(2f, 5f), pos + new Vector2(5.5f, height - 5f), ImGui.GetColorU32(clr_accent), 2f);
            dl.AddRect(pos + new Vector2(0.5f, 3.5f), pos + new Vector2(7f, height - 3.5f), ImGui.GetColorU32(new Vector4(clr_accent.X, clr_accent.Y, clr_accent.Z, 0.25f)), 3f, ImDrawFlags.None, 1.5f);

            // Category Label directly after accent bar (no emoji/icon)
            string upperText = label.ToUpper();
            Vector2 textPos = pos + new Vector2(16f, (height - 18f) / 2f);
            dl.AddText(textPos, ImGui.GetColorU32(clr_text_bright), upperText);

            // Cyberpunk horizontal accent trail line
            float textWidth = ImGui.CalcTextSize(upperText).X;
            float lineStartX = textPos.X + textWidth + 12f;
            float lineEndX = pos.X + width - 12f;
            if (lineEndX > lineStartX)
            {
                dl.AddLine(new Vector2(lineStartX, pos.Y + height / 2f), new Vector2(lineEndX, pos.Y + height / 2f), ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 0.08f)), 1f);
            }

            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + height + 6f);
        }

        private void DrawFeatureCard(string label, string desc, ref bool value)
        {
            var dl = ImGui.GetWindowDrawList();
            var pos = ImGui.GetCursorScreenPos();
            float width = ImGui.GetContentRegionAvail().X;
            float height = 48f;
            float rounding = 8f;

            bool hovered = ImGui.IsMouseHoveringRect(pos, pos + new Vector2(width, height));
            float alpha = value ? 0.15f : (hovered ? 0.08f : 0.04f);

            dl.AddRectFilled(pos, pos + new Vector2(width, height), ImGui.GetColorU32(new Vector4(1, 1, 1, alpha)), rounding);

            if (value)
            {
                float time = (float)ImGui.GetTime();
                float shift = (time * 1.5f) % 2.5f - 1.25f;

                dl.PushClipRect(pos, pos + new Vector2(width, height), true);
                dl.AddRectFilled(pos, pos + new Vector2(width, height), ImGui.GetColorU32(new Vector4(clr_accent.X, clr_accent.Y, clr_accent.Z, 0.1f)), rounding);

                Vector2 sStart = pos + new Vector2(width * shift, 0);
                Vector2 sEnd = pos + new Vector2(width * (shift + 0.5f), height);
                dl.AddRectFilledMultiColor(sStart, sEnd, 
                    ImGui.GetColorU32(new Vector4(0, 0, 0, 0)),
                    ImGui.GetColorU32(new Vector4(clr_accent.X, clr_accent.Y, clr_accent.Z, 0.15f)),
                    ImGui.GetColorU32(new Vector4(clr_accent.X, clr_accent.Y, clr_accent.Z, 0.15f)),
                    ImGui.GetColorU32(new Vector4(0, 0, 0, 0)));
                dl.PopClipRect();
                dl.AddRect(pos, pos + new Vector2(width, height), ImGui.GetColorU32(new Vector4(clr_accent.X, clr_accent.Y, clr_accent.Z, 0.4f)), rounding, ImDrawFlags.None, 1.2f);
            }

            dl.AddText(pos + new Vector2(14, 8), ImGui.GetColorU32(value ? clr_accent : clr_text_bright), label);
            dl.AddText(pos + new Vector2(14, 26), ImGui.GetColorU32(clr_text_dim), desc);

            // Modern Sharp Gaming Checkbox
            float boxSize = 18f;
            float boxRounding = 4f;
            Vector2 boxPos = new Vector2(MathF.Round(pos.X + width - boxSize - 14f), MathF.Round(pos.Y + (height - boxSize) / 2f));

            if (value)
            {
                // Crisp solid background (Accent filled)
                dl.AddRectFilled(boxPos, boxPos + new Vector2(boxSize, boxSize), ImGui.GetColorU32(clr_accent), boxRounding);
                
                // Sharp crisp 1px border
                dl.AddRect(boxPos, boxPos + new Vector2(boxSize, boxSize), ImGui.GetColorU32(new Vector4(1f, 0.4f, 0.35f, 1f)), boxRounding, ImDrawFlags.None, 1f);

                // Pixel-aligned crisp vector checkmark tick
                Vector2 p1 = new Vector2(boxPos.X + 4.5f, boxPos.Y + 9.5f);
                Vector2 p2 = new Vector2(boxPos.X + 7.5f, boxPos.Y + 13.0f);
                Vector2 p3 = new Vector2(boxPos.X + 13.5f, boxPos.Y + 5.5f);

                dl.AddLine(p1, p2, ImGui.GetColorU32(Vector4.One), 2.0f);
                dl.AddLine(p2, p3, ImGui.GetColorU32(Vector4.One), 2.0f);
            }
            else
            {
                // Inactive checkbox background
                dl.AddRectFilled(boxPos, boxPos + new Vector2(boxSize, boxSize), ImGui.GetColorU32(new Vector4(0.09f, 0.09f, 0.12f, 1f)), boxRounding);
                dl.AddRect(boxPos, boxPos + new Vector2(boxSize, boxSize), ImGui.GetColorU32(hovered ? new Vector4(0.45f, 0.45f, 0.52f, 1f) : new Vector4(0.24f, 0.24f, 0.30f, 1f)), boxRounding, ImDrawFlags.None, 1f);
            }

            ImGui.SetCursorScreenPos(pos);
            if (ImGui.InvisibleButton("##card_" + label, new Vector2(width, height)))
            {
                value = !value;
                if (value) SpawnSplash(pos);
            }

            ImGui.SetCursorPosY(ImGui.GetCursorPosY());
        }

        private bool FilledSliderInt(string label, ref int value, int min, int max)
        {
            float fVal = (float)value;
            bool changed = FilledSliderFloat(label, ref fVal, (float)min, (float)max);
            value = (int)fVal;
            return changed;
        }

        private bool FilledSliderFloat(string label, ref float value, float min, float max)
        {
            Vector2 pos = ImGui.GetCursorScreenPos();
            float sliderWidth = ImGui.GetContentRegionAvail().X;
            float totalHeight = 30f;
            var dl = ImGui.GetWindowDrawList();

            // Label on left, current value on right
            dl.AddText(pos, ImGui.GetColorU32(clr_text_bright), label);

            string valStr = value >= 10f ? ((int)value).ToString() : value.ToString("0.0");
            Vector2 valSize = ImGui.CalcTextSize(valStr);
            dl.AddText(pos + new Vector2(sliderWidth - valSize.X, 0), ImGui.GetColorU32(clr_accent), valStr);

            Vector2 sPos = pos + new Vector2(0, 20);
            float t = Math.Clamp((value - min) / (max - min), 0f, 1f);

            dl.AddRectFilled(sPos, sPos + new Vector2(sliderWidth, 4), ImGui.GetColorU32(clr_item_bg), 2f);
            dl.AddRectFilled(sPos, sPos + new Vector2(sliderWidth * t, 4), ImGui.GetColorU32(clr_accent), 2f);
            dl.AddCircleFilled(new Vector2(sPos.X + sliderWidth * t, sPos.Y + 2), 5f, ImGui.GetColorU32(Vector4.One));

            ImGui.SetCursorScreenPos(pos);
            ImGui.InvisibleButton("##s_" + label, new Vector2(sliderWidth, totalHeight));
            if (ImGui.IsItemActive())
            {
                value = Math.Clamp(min + (ImGui.GetMousePos().X - sPos.X) / sliderWidth * (max - min), min, max);
            }

            ImGui.Dummy(new Vector2(0, 4));
            return true;
        }

        private void HandleKeybinds()
        {
            CheckAndToggle(Config.KeySpeedBoostTimer, ref Config.speedint);
            CheckAndToggle(Config.KeySpinBot, ref Config.spinbot);
            CheckAndToggle(Config.KeyESPBox, ref Config.ESPBox);
            CheckAndToggle(Config.KeyESPName, ref Config.ESPName);
            CheckAndToggle(Config.KeyESPHealth, ref Config.ESPHealth);
            CheckAndToggle(Config.KeyESPSkeleton, ref Config.ESPSkeleton);
            CheckAndToggle(Config.KeyFOVEnabled, ref Config.FOVEnabled);
        }

        private bool CheckAndToggle(Keys key, ref bool feature)
        {
            if (key == Keys.None) return false;
            bool isPressed = GetAsyncKeyState(key) < 0;
            if (!_toggleAnim.ContainsKey(key.ToString() + "_kb")) _toggleAnim[key.ToString() + "_kb"] = 0;
            if (isPressed && _toggleAnim[key.ToString() + "_kb"] == 0) { feature = !feature; _toggleAnim[key.ToString() + "_kb"] = 1; return true; }
            if (!isPressed) _toggleAnim[key.ToString() + "_kb"] = 0;
            return false;
        }

        protected override unsafe Task PostInitialized()
        {
            StartEntityProcessing();
            ReplaceFont(config => {
                var io = ImGui.GetIO();
                if (EmbeddedFonts.FontInter.Length > 0) {
                    fixed (byte* fPtr = EmbeddedFonts.FontInter) io.Fonts.AddFontFromMemoryTTF((IntPtr)fPtr, EmbeddedFonts.FontInter.Length, 14, config, io.Fonts.GetGlyphRangesDefault());
                }
                if (EmbeddedFonts.FontLucide.Length > 0) {
                    ushort[] glyphRanges = new ushort[] { (ushort)Lucide.IconMin, (ushort)Lucide.IconMax, 0 };
                    config->MergeMode = 1;
                    fixed (byte* fPtr = EmbeddedFonts.FontLucide)
                    fixed (ushort* p = &glyphRanges[0]) io.Fonts.AddFontFromMemoryTTF((IntPtr)fPtr, EmbeddedFonts.FontLucide.Length, 14, config, (IntPtr)p);
                }
            });
            ApplyPremiumStyle();
            return base.PostInitialized();
        }
    }
}
