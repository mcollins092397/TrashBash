using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using TrashBash.Collisions;

namespace TrashBash
{
    public class FenceSide
    {
        public Vector2 Position;

        private BoundingRectangle bounds;

        private Texture2D texture;

        public int Width = 12;
        public int Height = 256;

        /// <summary>
        /// bounding volume of the sprite
        /// </summary>
        public BoundingRectangle Bounds
        {
            get
            {
                return bounds;
            }
        }

        public FenceSide(Vector2 position, ContentManager content)
        {
            this.Position = position;
            this.bounds = new BoundingRectangle(position.X, position.Y, 12, 256);
            LoadContent(content);
        }

        public void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("FenceVerticle");
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Position, Color.White);
        }
    }
}
