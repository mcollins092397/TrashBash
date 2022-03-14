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
    public enum RaccoonDirection
    {
        Right = 0,
        Left = 1,
        Asleep = 2
    }

    public class RaccoonSprite
    {
        //hit sound effect for the raccoon
        private SoundEffect hitSound;

        //the raccoons position
        public Vector2 Position;

        //timer to keep track of when to advance frames
        private double animationTimer;

        //short representing the frame of the animation the raccoon is on
        private short animationFrame = 0;

        //the raccoons texture
        private Texture2D texture;

        //the raccoons bounds
        private BoundingCircle bounds;

        public BoundingCircle Bounds => bounds;

        //raccons health
        public double Health = 4;

        //a timer that prevents the raccoon from getting hit multiple times by same projectile. Gives i frames
        private double iFrameTimer = 0;

        //bool representing if the raccoon was hit
        public bool Hit = false;

        //direction the raccoon is looking when throwing
        public RaccoonDirection Direction = RaccoonDirection.Asleep;

        private bool throwing = false;
        private double throwWaitTimer = 0;
        private float rand = RandomHelper.NextFloat(1.0f, 5.0f);
        private bool fire = false;

        public List<GasProjectile> GasProjectile = new List<GasProjectile>();
        public List<GasProjectile> GasProjectileRemove = new List<GasProjectile>();

        /// <summary>
        /// constructor for raccoon object
        /// </summary>
        /// <param name="position">raccoon spawn position</param>
        /// <param name="content">games content manager</param>
        public RaccoonSprite(Vector2 position, ContentManager content)
        {
            this.Position = position;
            this.bounds = new BoundingCircle(position + new Vector2(32, 32), 32);
            LoadContent(content);
        }

        /// <summary>
        /// color blend of the raccoon
        /// </summary>
        public Color Color { get; set; } = Color.White;

        /// <summary>
        /// Loads the raccoon textures bounds and sound effects
        /// </summary>
        /// <param name="content"></param>
        public void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("Raccoon");
            hitSound = content.Load<SoundEffect>("raccoonHit");
        }

        /// <summary>
        /// updates the raccoon
        /// </summary>
        /// <param name="gameTime">gametime</param>
        /// <param name="player">player object passed from main</param>
        public void Update(GameTime gameTime, PlayerController player, GasParticleSystem gas, ContentManager content)
        {
            foreach (PlayerProjectile proj in player.PlayerProjectile)
            {
                if (proj.Bounds.CollidesWith(Bounds) && throwing)
                {
                    Hit = true;
                    Health -= proj.Damage;
                    hitSound.Play(.5f, 0, 0);
                    player.ProjectileRemove.Add(proj);
                }
                else if(proj.Bounds.CollidesWith(Bounds))
                {
                    player.ProjectileRemove.Add(proj);
                }
            }

            if (Hit == false)
            {
                Color = Color.White;
            }

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

            if(throwing)
            {
                bounds.Radius = 32;
                bounds.Center = Position + new Vector2(32, 32);
                if(player.Position.X > Position.X)
                {
                    Direction = RaccoonDirection.Right;
                }
                else if (player.Position.X < Position.X)
                {
                    Direction = RaccoonDirection.Left;
                }

                if (fire)
                {
                    if(Direction == RaccoonDirection.Left)
                    {
                        GasProjectile.Add(new GasProjectile(Position + new Vector2(13, 0), player.Position + new Vector2(32,32), gas, Position));
                    }
                    else if(Direction == RaccoonDirection.Right)
                    {
                        GasProjectile.Add(new GasProjectile(Position + new Vector2(40, 0), player.Position + new Vector2(32, 32), gas, Position));
                    }
                    
                    fire = false;
                }
            }
            else
            {
                bounds.Radius = 20;
                bounds.Center = Position + new Vector2(32, 40);
                Direction = RaccoonDirection.Asleep;
                throwWaitTimer += gameTime.ElapsedGameTime.TotalSeconds;
                if (throwWaitTimer > rand)
                {
                    throwing = true;
                    throwWaitTimer -= rand;
                    rand = RandomHelper.NextFloat(1.0f, 5.0f);
                }
            }

            //Load content for every projectile in the list and then update them
            foreach (GasProjectile proj in GasProjectile)
            {
                if (proj.ContentLoaded == false)
                {
                    proj.LoadContent(content);
                    proj.ContentLoaded = true;
                }
                proj.Update(gameTime);

                if (proj.delete)
                {
                    GasProjectileRemove.Add(proj);
                }
            }

            foreach (GasProjectile proj in GasProjectileRemove)
            {
                GasProjectile.Remove(proj);
            }

            GasProjectileRemove.Clear();

        }

        /// <summary>
        /// Draws and animates the raccoon sprite
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="spriteBatch"></param>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {

            foreach (GasProjectile proj in GasProjectile)
            {
                proj.Draw(gameTime, spriteBatch);
            }

            //get spiderss animation frame
            animationTimer += gameTime.ElapsedGameTime.TotalSeconds;

            if (animationTimer > .2)
            {
                if(throwing)
                {
                    animationFrame++;

                    if (animationFrame > 8)
                    {
                        animationFrame = 0;
                        throwing = false;
                    }

                    if(animationFrame == 5)
                    {
                        fire = true;
                    }
                }
                else
                {
                    animationFrame = 0;
                }
                

                animationTimer -= 0.2;
            }

            //draw spider based on what frame it is in
            var source = new Rectangle(animationFrame * 64, (int)Direction * 64, 64, 64);
            spriteBatch.Draw(texture, Position, source, Color);
        }

    }
}
