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
        //sound triggered on the bag being shot by the player
        private SoundEffect bagHit;

        //create a spider direction object and default it to asleep
        public SpiderDirection Direction = SpiderDirection.Asleep;

        //the spiders current position
        public Vector2 Position;

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
        private float moveSpeed = 1.8f;

        //used to keep track of the spiders invulnerability frames after being hit. May remove later so that higher fire rate feels good
        private double iFrameTimer = 0;
        public bool Hit = false;

        //used to keep track if the spider has been awoken (that way they can maybe surprise the player
        private bool awake = false;
        private bool awakeAnimationPlayed = false;

        AStarPathfinder pathfinder;
        Stack<(int, int)> path;

        private Texture2D test;

        private double pathfinderRefreshWait = 0;
        private bool initialPathLoad = false;

        /// <summary>
        /// base constructor for the spider sprite
        /// </summary>
        /// <param name="position">The position the spider will be constucted</param>
        /// <param name="content">the content manager</param>
        public TrashSpiderSprite(Vector2 position, ContentManager content, AStarPathfinder pathfinder)
        {
            this.Position = position;
            this.bounds = new BoundingCircle(position + new Vector2(32, 32), 16);
            this.pathfinder = pathfinder;
            path = new Stack<(int, int)>();
            LoadContent(content);
        }


        /// <summary>
        /// color blend of the trash spider
        /// </summary>
        public Color Color { get; set; } = Color.White;

        /// <summary>
        /// Loads the trash spider texture
        /// </summary>
        /// <param name="content">content manager</param>
        public void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("TrashSpiderLeftRight");
            bounds = new BoundingCircle(Position + new Vector2(32, 32), 16);
            bagHit = content.Load<SoundEffect>("BagHit");

            test = content.Load<Texture2D>("EnemyProjectile");
        }

        /// <summary>
        /// update loop for trash spiders
        /// </summary>
        /// <param name="gameTime">gametime object</param>
        /// <param name="player">the player object from main</param>
        public void Update(GameTime gameTime, PlayerController player)
        {
            pathfinderRefreshWait += gameTime.ElapsedGameTime.TotalSeconds;
            if (!initialPathLoad)
            {
                path = pathfinder.aStarSearch((int)Bounds.Center.X / 10, (int)Bounds.Center.Y / 10, (int)(player.Bounds.X + 16) / 10, (int)(player.Bounds.Y + 32) / 10);
                initialPathLoad = true;
            }
            if (pathfinderRefreshWait > .5)
            {
                path = pathfinder.aStarSearch((int)Bounds.Center.X / 10, (int)Bounds.Center.Y / 10, (int)(player.Bounds.X + 16) / 10, (int)(player.Bounds.Y + 32) / 10);
                pathfinderRefreshWait -= .5;
            }

            //reset the spider color if it is not currently being hit
            if (Hit == false)
            {
                Color = Color.White;
            }

            //check the distance to the player and if the player is close enough wake up this spider
            if(Math.Abs(player.Position.X - Position.X) < 250 && Math.Abs(player.Position.Y - Position.Y) < 250)
            {
                awake = true;
            }


            //if awake start making their way towards the player's position
            if(path != null)
            {
                if (awakeAnimationPlayed && path.Count > 0)
                {
                    Vector2 temp = bounds.Center;
                    temp.Round();

                    if (Bounds.Center.X < path.Peek().Item2*10 + 10)
                    {
                        Position += new Vector2(moveSpeed, 0);
                        Direction = SpiderDirection.Right;
                    }
                    if (Bounds.Center.X > path.Peek().Item2*10 - 10)
                    {
                        Position += new Vector2(-moveSpeed, 0);
                        Direction = SpiderDirection.Left;
                    }
                    if (Bounds.Center.Y < path.Peek().Item1*10 + 10)
                    {
                        Position += new Vector2(0, moveSpeed);
                    }
                    if (Bounds.Center.Y > path.Peek().Item1*10 - 10)
                    {
                        Position += new Vector2(0, -moveSpeed);
                    }
                    if (temp.X > (float)path.Peek().Item2 * 10 - 20 && temp.X < (float)path.Peek().Item2 * 10 + 20
                    && temp.Y > (float)path.Peek().Item1 * 10 - 20 && temp.Y < (float)path.Peek().Item1 * 10 + 20
                    && path.Count > 0)
                    {
                        path.Pop();
                    }
                    //update the bounds
                    bounds.Center = new Vector2(Position.X + 32, Position.Y + 32);
                }
            }
            



            //when the spider is hit set the iframe and change the spider to be red until they have recovered
            if(Hit)
            {
                iFrameTimer += gameTime.ElapsedGameTime.TotalSeconds;
                Color = Color.Red;

                if (iFrameTimer > .2)
                {
                    Hit = false;
                    iFrameTimer -= .2;
                }
            }

            //check if any of the player's projectiles collide with the spider
            //and if they do deal the player's damage and wake the spider if asleep
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

            //test
            if(path != null)
            {
                foreach ((int, int) p in path)
                {
                    //spriteBatch.Draw(test, new Vector2(p.Item2 * 10, p.Item1 * 10), Color.White);
                }
            }
            
            if(!Dead)
            {
                //progress the animation timer while the spider is sleeping
                if (!awake && animationTimer > .1)
                {
                    AnimationFrame = 0;
                    animationTimer -= 0.1;
                }

                //if freshly awakened play the awaken animation and then set spider direction to left
                if(awake && awakeAnimationPlayed == false && animationTimer > .1)
                {
                    AnimationFrame++;
                    if (AnimationFrame == 3 && Direction == SpiderDirection.Left)
                    {
                        awakeAnimationPlayed = true;
                    }

                    

                    if (AnimationFrame > 3)
                    {
                        AnimationFrame = 0;
                        Direction = SpiderDirection.Left;
                    }



                    animationTimer -= 0.1;
                }

                //if already awke have the spider animations progress normally
                if (awake && awakeAnimationPlayed == true && animationTimer > .1)
                {
                    AnimationFrame++;
                
                    if (AnimationFrame > 3)
                    {
                        AnimationFrame = 0;
                    }

                    animationTimer -= 0.1;
                }

                //draw spider based on what frame it is in
                var source = new Rectangle(AnimationFrame * 64, (int)Direction * 64, 64, 64);
                spriteBatch.Draw(texture, Position, source, Color);
            }
            else
            {
                //if freshly killed play the death animation
                if (deathAnimationPlayed == false && animationTimer > .2)
                {
                    AnimationFrame++;

                    if (AnimationFrame == 3)
                    {
                        deathAnimationPlayed = true;
                    }

                    if (AnimationFrame > 3)
                    {
                        AnimationFrame = 3;
                    }



                    animationTimer -= 0.2;
                }
                else if(deathAnimationPlayed == true && animationTimer > .2)
                {
                    AnimationFrame++;

                    if (AnimationFrame > 3)
                    {
                        AnimationFrame = 2;
                    }
                    animationTimer -= 0.2;
                }
                //draw spider based on what frame it is in
                var source = new Rectangle(AnimationFrame * 64, 3 * 64, 64, 64);
                spriteBatch.Draw(texture, Position, source, Color);
            }

        }

    }
}
