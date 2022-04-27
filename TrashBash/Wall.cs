using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using TrashBash.Collisions;

namespace TrashBash
{
    public class Wall
    {
        //the walls position
        public Vector2 Position;

        //wall texture
        private Texture2D texture;

        //a float value representing what shape of wall to draw
        private float type;

        //the bounds of bag, used in collision detection
        private BoundingRectangle bounds;

        /// <summary>
        /// getter for the bag bounds
        /// </summary>
        public BoundingRectangle Bounds => bounds;

        /// <summary>
        /// base constructor for the bag sprite
        /// </summary>
        /// <param name="position">The position the bag will be constucted</param>
        /// <param name="content">the content manager</param>
        public Wall(Vector2 position, ContentManager content, float type)
        {
            this.Position = position;
            this.bounds = new BoundingRectangle(position, 32, 32);
            this.type = type;
            LoadContent(content);
        }

        /// <summary>
        /// Loads the trash bag texture
        /// </summary>
        /// <param name="content">content manager</param>
        public void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("wall");
        }

        /// <summary>
        /// Draws and animates the trash spider sprite
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="spriteBatch"></param>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {

            var source = new Rectangle((int)type * 32, 0, 32, 32);

            spriteBatch.Draw(texture, Position, source, Color.White);
        }
    }
}
