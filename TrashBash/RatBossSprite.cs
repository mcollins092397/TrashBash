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
    public class RatBossSprite
    {
        //sound triggered on the bag being shot by the player
        private SoundEffect hitSound;

        //the spiders current position
        public Vector2 Position;
        private Vector2 center;

        //variables keeping track of the spiders animation and texture
        private double animationTimer;
        public short AnimationFrame = 0;
        private Texture2D texture;

        //the bounds of spider, used in collision detection
        private BoundingCircle bounds;

        /// <summary>
        /// getter for the spider bounds
        /// </summary>
        public BoundingCircle Bounds => bounds;

        //the spiders default health. Starting at 2 initially but may increase once items are added into the game,
        //may also have this scale based on lvl
        public double Health = 3;

        //bool gets set to true when the spider health drops to 0
        public bool Dead = false;
        private bool deathAnimationPlayed = false;

        //spider move speed
        private float moveSpeed = 3f;

        //used to keep track of the spiders invulnerability frames after being hit. May remove later so that higher fire rate feels good
        private double iFrameTimer = 0;
        public bool Hit = false;

        /// <summary>
        /// color blend of the raccoon
        /// </summary>
        public Color Color { get; set; } = Color.White;

        //bool swaps to true whenever the boss is activly preforming an attackh
        private bool attacking = false;

        /// <summary>
        /// constructor for raccoon object
        /// </summary>
        /// <param name="position">raccoon spawn position</param>
        /// <param name="content">games content manager</param>
        public RatBossSprite(Vector2 position, ContentManager content)
        {
            this.Position = position;
            center = Position + new Vector2(80, 80);
            this.bounds = new BoundingCircle(position + new Vector2(80, 80), 75);
            LoadContent(content);
        }

        /// <summary>
        /// Loads the raccoon textures bounds and sound effects
        /// </summary>
        /// <param name="content"></param>
        public void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("test160");
            //hitSound = content.Load<SoundEffect>("raccoonHit");
        }

        public void Update(GameTime gameTime, PlayerController player)
        {
            if(!attacking)
            {
                if (Position.X + 160 < 1325)
                {
                    //check to see if boss needs to walk right
                    if (player.Position.X + 30 > center.X + moveSpeed || player.Position.X + 30 > center.X - moveSpeed)
                    {
                        Position.X += moveSpeed;
                    }
                }

                if(Position.X > 35)
                {
                    //check to see if the boss needs to walk left
                    if (player.Position.X + 30 < center.X + moveSpeed || player.Position.X + 30 < center.X - moveSpeed)
                    {
                        Position.X -= moveSpeed;
                    }
                }
                    
                
                    
            }

            //check if the boss was hit by a player projectile
            foreach (PlayerProjectile proj in player.PlayerProjectile)
            {
                if (proj.Bounds.CollidesWith(Bounds))
                {
                    Hit = true;
                    Health -= proj.Damage;
                    //hitSound.Play(.5f, 0, 0);
                    player.ProjectileRemove.Add(proj);
                }
                else if (proj.Bounds.CollidesWith(Bounds))
                {
                    player.ProjectileRemove.Add(proj);
                }
            }

            //reset color if no longer in i-frames
            if (Hit == false)
            {
                Color = Color.White;
            }

            //set i-frames and take damage when hit
            if (Hit)
            {
                iFrameTimer += gameTime.ElapsedGameTime.TotalSeconds;
                Color = Color.Red;

                if (iFrameTimer > .2)
                {
                    Hit = false;
                    iFrameTimer -= .2;
                }
            }

            center = Position + new Vector2(80, 80);
            bounds.Center = new Vector2(Position.X + 80, Position.Y + 80);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Position, Color);
        }
    }
}
