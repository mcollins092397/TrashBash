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
    public class GateTop
    {
        public Vector2 Position;

        private BoundingRectangle bounds;

        private Texture2D texture;

        public bool IsOpen = false;

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

        public GateTop(Vector2 position, ContentManager content)
        {
            this.Position = position;
            this.bounds = new BoundingRectangle(position.X, position.Y, 334, 39);
            LoadContent(content);
        }

        public void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("GateClosed");
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if(!IsOpen)
            {
                spriteBatch.Draw(texture, Position, Color.White);
            }
            else
            {
                bounds = new BoundingRectangle(0,0,0,0);
            }
            
        }
    }
}
