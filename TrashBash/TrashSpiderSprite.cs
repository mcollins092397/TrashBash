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
    /// <summary>
    /// enum representing the direction the spider is moving
    /// </summary>
    public enum SpiderDirection
    {
        Left = 0,
        Right = 1,
        Asleep = 2
    }

    public class TrashSpiderSprite
    {

        private SoundEffect bagHit;

        public SpiderDirection Direction = SpiderDirection.Asleep;

        public Vector2 Position;

        private double animationTimer;

        private short animationFrame = 0;

        private Texture2D texture;

        private BoundingCircle bounds;

        public BoundingCircle Bounds => bounds;

        public double Health = 2;

        private double iFrameTimer = 0;

        public bool Hit = false;

        private bool awake = false;
        private bool awakeAnimationPlayed = false;

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
            bagHit = content.Load<SoundEffect>("BagHit");
        }

        /// <summary>
        /// Update the trash spider
        /// </summary>
        /// <param name="gameTime">game time</param>
        public void Update(GameTime gameTime, PlayerController player)
        {
            if (Hit == false)
            {
                Color = Color.White;
            }

            if(Math.Abs(player.Position.X - Position.X) < 250 && Math.Abs(player.Position.Y - Position.Y) < 250)
            {
                awake = true;
            }

            if (awakeAnimationPlayed)
            {
                if (Position.X < player.Position.X)
                {
                    Position += new Vector2(1, 0);
                    Direction = SpiderDirection.Right;
                }
                if (Position.X > player.Position.X)
                {
                    Position += new Vector2(-1, 0);
                    Direction = SpiderDirection.Left;
                }
                if (Position.Y < player.Position.Y)
                {
                    Position += new Vector2(0, 1);
                }
                if (Position.Y > player.Position.Y)
                {
                    Position += new Vector2(0, -1);
                }
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

            foreach (PlayerProjectile proj in player.PlayerProjectile)
            {
                if (proj.Bounds.CollidesWith(Bounds))
                {
                    Hit = true;
                    awake = true;
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
            if(!awake && animationTimer > .1)
            {
                animationFrame = 0;
                animationTimer -= 0.1;
            }

            if(awake && awakeAnimationPlayed == false && animationTimer > .1)
            {
                if (animationFrame == 2 && Direction == SpiderDirection.Left)
                {
                    awakeAnimationPlayed = true;
                }

                animationFrame++;

                if (animationFrame > 3)
                {
                    animationFrame = 0;
                    Direction = SpiderDirection.Left;
                }



                animationTimer -= 0.1;
            }

            if (awake && awakeAnimationPlayed == true && animationTimer > .1)
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
