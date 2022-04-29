using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using TrashBash.Collisions;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace TrashBash
{
    public class HealthPickup
    {
        //the raccoons texture
        private Texture2D texture;
        private double animationTimer;
        public short AnimationFrame = 0;


        //the raccoons position
        public Vector2 Position;
        private Vector2 startPosition;

        //the raccoons bounds
        private BoundingRectangle bounds;

        //getter for the raccoon bounds
        public BoundingRectangle Bounds => bounds;

        //hit sound effect for the raccoon
        private SoundEffect hitSound;

        //the level that the bag appears in
        public float Level;

        private bool displayText = false;

        //spritefont used in the main menu controls explanation and the game over screen
        private SpriteFont spriteFont;

        private bool goingDown = false;

        /// <summary>
        /// constructor for raccoon object
        /// </summary>
        /// <param name="position">raccoon spawn position</param>
        /// <param name="content">games content manager</param>
        public HealthPickup(Vector2 position, ContentManager content, float level)
        {
            this.Position = position;
            this.startPosition = position;
            this.bounds = new BoundingRectangle(position, 48, 48);
            this.Level = level;
            LoadContent(content);
        }

        /// <summary>
        /// Loads the raccoon textures bounds and sound effects
        /// </summary>
        /// <param name="content"></param>
        public void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("HealthPickup");
            spriteFont = content.Load<SpriteFont>("arial");
            //hitSound = content.Load<SoundEffect>("raccoonHit");
        }

        /// <summary>
        /// update loop for the raccoon
        /// </summary>
        /// <param name="gameTime">gametime object </param>
        /// <param name="player">player object</param>
        public void Update(GameTime gameTime, PlayerController player)
        {
            if(player.Bounds.CollidesWith(bounds) && Level == 0)
            {
                displayText = true;
            }
            else
            {
                displayText = false;
            }

            if(Position.Y < startPosition.Y - 5)
            {
                goingDown = true;
            }

            if(goingDown)
            {
                Position.Y += 0.1f;
                if(Position.Y == startPosition.Y)
                {
                    goingDown = false;
                }
            }
            else
            {
                Position.Y -= 0.1f;
            }
        }

        /// <summary>
        /// Draws and animates the raccoon sprite
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="spriteBatch"></param>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            //get spiderss animation frame
            animationTimer += gameTime.ElapsedGameTime.TotalSeconds;

            if (animationTimer > .3)
            {
                AnimationFrame++;

                if (AnimationFrame > 1)
                {
                    AnimationFrame = 0;
                }

                animationTimer -= 0.3;
            }

            if(displayText && Level == 0)
            {
                spriteBatch.DrawString(spriteFont, "Space to Pickup", Position - new Vector2(65,20), Color.White);
            }

            var source = new Rectangle(AnimationFrame * 48, 0, 48, 48);
            spriteBatch.Draw(texture, Position, source, Color.White);
        }
    }
}
