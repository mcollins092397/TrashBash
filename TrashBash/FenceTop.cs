﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using TrashBash.Collisions;

namespace TrashBash
{
    public class FenceTop
    {
        private Vector2 position;

        private BoundingRectangle bounds;

        private Texture2D texture;

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
            this.position = position;
            this.bounds = new BoundingRectangle(position.X, position.Y, 256, 39);
        }

        public void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("Fence");
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, Color.White);
        }
    }
}
