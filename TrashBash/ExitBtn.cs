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

    public class ExitBtn
    {
        private Vector2 position;

        private Texture2D texture;

        private BoundingRectangle bounds;


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

        /// <summary>
        /// color blend of the exit btn
        /// </summary>
        public Color Color { get; set; } = Color.White;

        /// <summary>
        /// Creates a new exit btn sprite
        /// </summary>
        /// <param name="position">The position of the sprite in the game</param>
        public ExitBtn(Vector2 position)
        {
            this.position = position;
            this.bounds = new BoundingRectangle(position.X, position.Y, 160, 64);
        }

        /// <summary>
        /// Loads the sprite texture using the provided ContentManager
        /// </summary>
        /// <param name="content">The ContentManager to load with</param>
        public void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("ExitBtn");
        }

        /// <summary>
        /// Draws the sprite using the supplied SpriteBatch
        /// </summary>
        /// <param name="gameTime">The game time</param>
        /// <param name="spriteBatch">The spritebatch to render with</param>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, Color);
        }
    }
}
