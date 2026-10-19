using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace FREE_LIFETIME
{
    /// <summary>
    /// Animated particle system that renders glowing red/white particles
    /// on a transparent panel layered behind the main UI controls.
    /// </summary>
    public class ParticleSystem : Control
    {
        // ----------------------------------------------------------------
        //  Particle data
        // ----------------------------------------------------------------
        private struct Particle
        {
            public float X, Y;
            public float VX, VY;
            public float Size;
            public float Alpha;
            public float AlphaSpeed;
            public Color Color;
            public float Life;
            public float LifeSpeed;
            public bool IsGlow;
        }

        // ----------------------------------------------------------------
        //  Fields
        // ----------------------------------------------------------------
        private readonly List<Particle> _particles = new List<Particle>();
        private readonly Timer _timer;
        private readonly Random _rng = new Random();

        private static readonly Color[] Palette =
        {
            Color.FromArgb(220, 0,   0),
            Color.FromArgb(180, 0,   0),
            Color.FromArgb(255, 60,  0),
            Color.FromArgb(255, 200, 200),
            Color.FromArgb(255, 255, 255),
        };

        private const int MaxParticles = 90;
        private const int TickMs       = 20;

        // ----------------------------------------------------------------
        //  Constructor
        // ----------------------------------------------------------------
        public ParticleSystem()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.AllPaintingInWmPaint         |
                     ControlStyles.UserPaint                    |
                     ControlStyles.OptimizedDoubleBuffer, true);

            BackColor = Color.Transparent;
            Dock      = DockStyle.Fill;

            for (int i = 0; i < MaxParticles; i++)
                _particles.Add(SpawnParticle(randomY: true));

            _timer = new Timer { Interval = TickMs };
            _timer.Tick += (s, e) => { UpdateParticles(); Invalidate(); };
            _timer.Start();
        }

        // ----------------------------------------------------------------
        //  Particle factory
        // ----------------------------------------------------------------
        private Particle SpawnParticle(bool randomY = false)
        {
            bool isGlow = _rng.NextDouble() < 0.18;
            float w = Width  > 0 ? Width  : 580f;
            float h = Height > 0 ? Height : 365f;

            return new Particle
            {
                X          = (float)(_rng.NextDouble() * w),
                Y          = randomY ? (float)(_rng.NextDouble() * h) : h + 4f,
                VX         = (float)((_rng.NextDouble() - 0.5) * 0.6),
                VY         = (float)(-(_rng.NextDouble() * 0.8 + 0.3)),
                Size       = isGlow
                                 ? (float)(_rng.NextDouble() * 14 + 8)
                                 : (float)(_rng.NextDouble() * 3  + 1),
                Alpha      = 0f,
                AlphaSpeed = (float)(_rng.NextDouble() * 3 + 1.5f),
                Color      = Palette[_rng.Next(Palette.Length)],
                Life       = 0f,
                LifeSpeed  = (float)(_rng.NextDouble() * 0.004 + 0.002),
                IsGlow     = isGlow,
            };
        }

        // ----------------------------------------------------------------
        //  Update loop
        // ----------------------------------------------------------------
        private void UpdateParticles()
        {
            float w = Width  > 0 ? Width  : 580f;
            float h = Height > 0 ? Height : 365f;

            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                Particle p = _particles[i];

                p.X    += p.VX;
                p.Y    += p.VY;
                p.Life += p.LifeSpeed;

                if (p.Life < 0.2f)
                    p.Alpha = Math.Min(255f, p.Alpha + p.AlphaSpeed * 4);
                else
                    p.Alpha = Math.Max(0f, p.Alpha - p.AlphaSpeed * 0.6f);

                _particles[i] = p;

                if (p.Alpha <= 0 || p.Y < -20 || p.X < -20 || p.X > w + 20)
                    _particles[i] = SpawnParticle();
            }
        }

        // ----------------------------------------------------------------
        //  Rendering
        // ----------------------------------------------------------------
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            foreach (var p in _particles)
            {
                int alpha = Math.Max(0, Math.Min(255, (int)p.Alpha));
                if (alpha < 5) continue;

                if (p.IsGlow)
                    DrawGlow(e.Graphics, p, alpha);
                else
                    DrawSpark(e.Graphics, p, alpha);
            }
        }

        private static void DrawGlow(Graphics g, Particle p, int alpha)
        {
            float r = p.Size;
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(p.X - r, p.Y - r, r * 2, r * 2);
                using (var brush = new PathGradientBrush(path))
                {
                    brush.CenterColor    = Color.FromArgb(Math.Min(alpha, 140), p.Color);
                    brush.SurroundColors = new[] { Color.FromArgb(0, p.Color) };
                    g.FillPath(brush, path);
                }
            }
        }

        private static void DrawSpark(Graphics g, Particle p, int alpha)
        {
            float r = p.Size / 2f;
            using (var brush = new SolidBrush(Color.FromArgb(alpha, p.Color)))
                g.FillEllipse(brush, p.X - r, p.Y - r, p.Size, p.Size);
        }

        // ----------------------------------------------------------------
        //  Passthrough: clicks fall through to controls beneath
        // ----------------------------------------------------------------
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0084)   // WM_NCHITTEST
            {
                m.Result = (IntPtr)(-1); // HTTRANSPARENT
                return;
            }
            base.WndProc(ref m);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _timer?.Dispose();
            base.Dispose(disposing);
        }
    }
}
