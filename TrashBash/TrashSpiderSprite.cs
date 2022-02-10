using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using TrashBash.Collisions;

namespace TrashBash
{
    /// <summary>
    /// enum representing the direction the spider is moving
    /// </summary>
    public enum SpiderDirection
    {
        Left = 0,
        Right = 1
    }

    public class TrashSpiderSprite
    {

        public SpiderDirection Direction;

        public Vector2 Position;

        private double animationTimer;

        private short animationFrame = 0;

        private Texture2D texture;

        private BoundingCircle bounds;

        public BoundingCircle Bounds => bounds;

        public double Health = 2;

        private double iFrameTimer = 0;

        public bool Hit = false;

        public TrashSpiderSprite(Vector2 position, ContentManager content)
        {
            this.Position = position;
            this.bounds = new BoundingCircle(position + new Vector2(32, 32), 16);
            LoadContent(content);
        }


        /// <summary>
        /// color blend of the trash spider
        /// </summary>
        public Color Color { get; set; } = Color.White;

        /// <summary>
        /// Loads the trash spider texture
        /// </summary>
        /// <param name="content"></param>
        public void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("TrashSpiderLeftRight");
            bounds = new BoundingCircle(Position + new Vector2(32, 32), 16);
        }

        /// <summary>
        /// Update the trash spider
        /// </summary>
        /// <param name="gameTime">game time</param>
        public void Update(GameTime gameTime, Vector2 targetLocation)
        {
            if (Position.X < targetLocation.X)
            {
                Position += new Vector2(1, 0);
            }
            if (Position.X > targetLocation.X)
            {
                Position += new Vector2(-1, 0);
            }
            if (Position.Y < targetLocation.Y)
            {
                Position += new Vector2(0, 1);
            }
            if (Position.Y > targetLocation.Y)
            {
                Position += new Vector2(0, -1);
            }
            //update the bounds
            bounds.Center = new Vector2(Position.X + 32, Position.Y + 32);

            if(Hit)
            {
                iFrameTimer += gameTime.ElapsedGameTime.TotalSeconds;
                Color = Color.Red;

                if (iFrameTimer > .2)
                {
                    Hit = false;
                }
            }
            


        }


        /// <summary>
        /// Draws and animates the trash spider sprite
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="spriteBatch"></param>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            //get spiderss animation frame
            animationTimer += gameTime.ElapsedGameTime.TotalSeconds;

            if (animationTimer > .1)
            {
                animationFrame++;
                
                if (animationFrame > 3)
                {
                    animationFrame = 0;
                }

                animationTimer -= 0.1;
            }

            //draw spider based on what frame it is in
            var source = new Rectangle(animationFrame * 64, (int)Direction * 64, 64, 64);
            spriteBatch.Draw(texture, Position, source, Color);
        }

    }
}
