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
    public class TrashBagSprite
    {
        //the trashbags current position
        public Vector2 Position;

        //sound triggered on the bag being shot by the player
        private SoundEffect bagHit;

        //variables keeping track of the spiders animation and texture
        private double animationTimer;
        private short animationFrame = 0;
        private Texture2D texture;

        //the bounds of bag, used in collision detection
        private BoundingCircle bounds;

        /// <summary>
        /// getter for the bag bounds
        /// </summary>
        public BoundingCircle Bounds => bounds;

        //the bags default health
        public double Health = 3;



        //used to keep track of the bags invulnerability frames after being hit. May remove later so that higher fire rate feels good
        public bool Hit = false;

        /// <summary>
        /// base constructor for the bag sprite
        /// </summary>
        /// <param name="position">The position the bag will be constucted</param>
        /// <param name="content">the content manager</param>
        public TrashBagSprite(Vector2 position, ContentManager content)
        {
            this.Position = position;
            this.bounds = new BoundingCircle(position + new Vector2(32, 32), 16);
            LoadContent(content);
        }

        /// <summary>
        /// Loads the trash bag texture
        /// </summary>
        /// <param name="content">content manager</param>
        public void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("TrashBag");
            bounds = new BoundingCircle(Position + new Vector2(32, 32), 16);
            bagHit = content.Load<SoundEffect>("BagHit");
        }

        /// <summary>
        /// update loop for trash spiders
        /// </summary>
        /// <param name="gameTime">gametime object</param>
        /// <param name="player">the player object from main</param>
        public void Update(GameTime gameTime, PlayerController player)
        {

            //check if any of the player's projectiles collide with the spider
            //and if they do deal the player's damage and wake the spider if asleep
            foreach (PlayerProjectile proj in player.PlayerProjectile)
            {
                if (proj.Bounds.CollidesWith(Bounds) && Health > 0)
                {
                    Hit = true;
                    Health -= proj.Damage;
                    bagHit.Play(.5f, 0, 0);
                    player.ProjectileRemove.Add(proj);
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
                if (!Hit)
                {
                    animationFrame = 0;
                }

                if(Hit)
                {
                    animationFrame++;

                    if(animationFrame > 3)
                    {
                        animationFrame = 0;
                        Hit = false;
                    }
                }

                animationTimer -= 0.1;
            }

            var source = new Rectangle(0,0,0,0);

            if(Hit)
            {
                source = new Rectangle(animationFrame * 64, (int)(3 - Health - 1) * 64, 64, 64);
            }
            else if(!Hit && Health > 0)
            {
                source = new Rectangle(0 * 64, (int)(3 - Health) * 64, 64, 64);
            }
            else if(Health <= 0)
            {
                source = new Rectangle(3 * 64, 2 * 64, 64, 64);
            }


           
            
            spriteBatch.Draw(texture, Position, source, Color.White);
        }
    }
}
