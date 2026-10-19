using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace LITEXPARTICLES
{
    internal class Particle
    {
        public PointF Position { get; set; }
        public PointF TargetPosition { get; set; }
        public float Speed { get; set; }
        public float Size { get; set; }
        public float Radius { get; set; }
        public float Rotation { get; set; }

        private static Random _random = new Random();

        public Particle(Size screenSize)
        {
            Position = new PointF(0, 0);
            TargetPosition = new PointF(_random.Next(screenSize.Width), screenSize.Height * 2);
            Speed = 1 + _random.Next(25);
            Size = _random.Next(8);
            Radius = _random.Next(4);
            Rotation = 0;
        }

        public void Update(Size screenSize, float deltaTime)
        {
            if (Position.X == 0 || Position.Y == 0)
            {
                Position = new PointF(_random.Next(screenSize.Width + 1), 15f);
                Speed = 1 + _random.Next(25);
                Radius = _random.Next(4);
                Size = _random.Next(8);
                TargetPosition = new PointF(_random.Next(screenSize.Width), screenSize.Height * 2);
            }

            Position = Lerp(Position, TargetPosition, deltaTime * (Speed / 60));
            Rotation += deltaTime;

            if (Position.Y > screenSize.Height)
            {
                Position = new PointF(0, 0);
                Rotation = 0;
            }
        }

        private PointF Lerp(PointF start, PointF end, float t)
        {
            return new PointF(start.X + (end.X - start.X) * t, start.Y + (end.Y - start.Y) * t);
        }
    }

    internal class ParticlesBy_By
    {
        private const int ParticleCount = 30;
        public List<Particle> Particles { get; private set; } = new List<Particle>();

        public ParticlesBy_By()
        {
            InitializeParticles();
        }

        private void InitializeParticles()
        {
            Size screenSize = Screen.PrimaryScreen.Bounds.Size;
            for (int i = 0; i < ParticleCount; i++)
            {
                Particles.Add(new Particle(screenSize));
            }
        }

        public void UpdateParticles()
        {
            Size screenSize = Screen.PrimaryScreen.Bounds.Size;
            float deltaTime = 1.0f / 60; // Assuming 60 FPS

            foreach (var particle in Particles)
            {
                particle.Update(screenSize, deltaTime);
            }
        }
    }
}
