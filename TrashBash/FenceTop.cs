using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using TrashBash.Collisions;
using TrashBash;

namespace TrashBash
{
    public class FenceTop
    {
        public Vector2 Position;

        private BoundingRectangle bounds;

        private Texture2D texture;

        public int Width = 256;
        public int Height = 96;

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

        public FenceTop(Vector2 position)
        {
            this.Position = position;
            this.bounds = new BoundingRectangle(position.X, position.Y, 256, 39);
            
        }

        public void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("Fence");
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Position, Color.White);
        }
    }
}
