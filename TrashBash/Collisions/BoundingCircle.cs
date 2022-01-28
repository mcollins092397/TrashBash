using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace TrashBash.Collisions
{
    /// <summary>
    /// a struct representing circular bounds
    /// </summary>
    public struct BoundingCircle
    {
        /// <summary>
        /// center of bounding circle
        /// </summary>
        public Vector2 Center;

        /// <summary>
        /// radius of bounding circle
        /// </summary>
        public float Radius;

        /// <summary>
        /// contructs a new bounding circle
        /// </summary>
        /// <param name="center">the center</param>
        /// <param name="radius">the radius</param>
        public BoundingCircle(Vector2 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        /// <summary>
        /// test for collision between this and another bounding circle
        /// </summary>
        /// <param name="other">the other bounding circle</param>
        /// <returns>true for collision false otherwise</returns>
        public bool CollidesWith(BoundingCircle other)
        {
            return CollisionHelper.Collides(this, other);
        }

        public bool CollidesWith(BoundingRectangle other)
        {
            return CollisionHelper.Collides(this, other);
        }
    }
}
