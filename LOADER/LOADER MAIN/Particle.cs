using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LOADER_MAIN
{
    internal class Particle
    {
        public PointF Position { get; set; }
        public PointF Velocity { get; set; }
        public float Size { get; set; }

        public Particle(PointF position, PointF velocity, float size)
        {
            Position = position;
            Velocity = velocity;
            Size = size;
        }

        public void Update()
        {
            Position = new PointF(Position.X + Velocity.X, Position.Y + Velocity.Y);
        }
    }
}
