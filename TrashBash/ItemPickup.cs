using System;
using System.Collections.Generic;
using System.Text;
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
    public class ItemPickup
    {
        //the health pickup texture and animation variables
        private Texture2D texture;
        private double animationTimer;
        public short AnimationFrame = 0;


        //the pickups position and starting position for its float effect
        public Vector2 Position;
        private Vector2 startPosition;

        //the pickups bounds
        private BoundingRectangle bounds;

        //getter for the pickup bounds
        public BoundingRectangle Bounds => bounds;

        //pickup sound effect for the pickup (coming soon)
        private SoundEffect hitSound;

        //the level that the pickup appears in
        public float Level;

        //if the explaination text should be displayed. Set to false after the first lvl
        private bool displayText = false;

        //spritefont used in the explanation
        private SpriteFont spriteFont;

        private bool goingDown = false;

        private int type = RandomHelper.Next(3);
        private string description;

        /// <summary>
        /// constructor for health pickup object
        /// </summary>
        /// <param name="position">pickup spawn position</param>
        /// <param name="content">games content manager</param>
        public ItemPickup(Vector2 position, ContentManager content, float level, int? type)
        {
            this.Position = position;
            this.startPosition = position;
            this.bounds = new BoundingRectangle(position, 48, 48);
            this.Level = level;
            if(type != null)
            {
                this.type = (int)type;
            }
            LoadContent(content);

        }

        /// <summary>
        /// Loads the pickup textures bounds and sound effects
        /// </summary>
        /// <param name="content"></param>
        public void LoadContent(ContentManager content)
        {
            if(type == 0)
            {
                texture = content.Load<Texture2D>("Items/ExtraFirepowerItem");
                description = "Extra Firepower\n" +
                              "_____________\n" +
                              "  Damage + 0.5    ";
            }
            else if(type == 1)
            {
                texture = content.Load<Texture2D>("Items/NewBootsItem");
                description = "New Boots\n" +
                              "_____________\n" +
                              "  Speed + 1    ";
            }
            else if(type ==2)
            {
                texture = content.Load<Texture2D>("Items/RocketPropelledBulletItem");
                description = "Rocket Propelled Bullets\n" +
                              "_____________\n" +
                              " Projectile Speed + 2\n" +
                              " Projectile Range + 1";
            }
            
            spriteFont = content.Load<SpriteFont>("arial");
            //hitSound = content.Load<SoundEffect>("raccoonHit");
        }

        /// <summary>
        /// update loop for the pickup
        /// </summary>
        /// <param name="gameTime">gametime object </param>
        /// <param name="player">player object</param>
        public void Update(GameTime gameTime, PlayerController player)
        {
            if (player.Bounds.CollidesWith(bounds))
            {
                displayText = true;
            }
            else
            {
                displayText = false;
            }

            if (Position.Y < startPosition.Y - 5)
            {
                goingDown = true;
            }

            if (goingDown)
            {
                Position.Y += 0.1f;
                if (Position.Y == startPosition.Y)
                {
                    goingDown = false;
                }
            }
            else
            {
                Position.Y -= 0.1f;
            }
        }

        public void Pickup(PlayerController player)
        {
            if(type == 0)
            {
                player.ProjDmg += 0.5f;
            }
            else if(type == 1)
            {
                player.MovementSpeed += 1;
            }
            else if (type == 2)
            {
                player.ProjSpeed += 2;
                player.ProjRange += 100;
            }
        }

        /// <summary>
        /// Draws and animates the pickup sprite
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="spriteBatch"></param>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            //get animation frame
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

            if (displayText)
            {
                spriteBatch.DrawString(spriteFont, description, Position + new Vector2(65, -20), Color.White);
            }

            var source = new Rectangle(AnimationFrame * 48, 0, 48, 48);
            spriteBatch.Draw(texture, Position, source, Color.White);
        }
    }
}

