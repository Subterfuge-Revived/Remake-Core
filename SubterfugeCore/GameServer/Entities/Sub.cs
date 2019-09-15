using Microsoft.Xna.Framework;
using SubterfugeCore.Entities.Base;

namespace SubterfugeCore.Entities
{
    public class Sub : GameObject
    {
        private int drillerCount = 42;
        private float[] polynomial;

        public Sub(Vector2 initialPosition, Vector2 direction) : base()
        {
            this.position = initialPosition;
            this.polynomial = this.determinePoly(direction);
        }

        private float[] determinePoly(Vector2 direction)
        {
            // Determine the polynomial equation for the sub based on it's initial position and heading. 
            Vector2 secondPoint = this.position + direction;

            float slope = (secondPoint.Y - this.position.Y) / (secondPoint.X - this.position.X);
            float intercept = secondPoint.Y - slope * secondPoint.X;

            float[] eq = { slope, intercept };

            return eq;
        }

        public int getDrillerCount()
        {
            return this.drillerCount;
        }
    }
}
