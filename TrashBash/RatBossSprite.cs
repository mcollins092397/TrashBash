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
        private enum state
        {
            idle = 0,
            moveLeft = 1,
            moveRight = 2,
            charge = 3,
            slam = 4,
        }

        //sound triggered on being shot by the player
        private SoundEffect hitSound;

        //the boss's current position
        public Vector2 Position;
        private Vector2 center;

        //variables keeping track of the boss animation and texture
        private double animationTimer;
        public short AnimationFrame = 0;
        private Texture2D texture;
        private Rectangle source;

        //warning textures
        private Texture2D arrowWarning;
        private Texture2D arrowWarningUp;
        private Texture2D warningCircle;

        private Texture2D test;

        //health bar textures
        private Texture2D healthEmpty;
        private Texture2D healthFull;

        //the bounds of boss, used in collision detection
        private BoundingCircle bounds;

        /// <summary>
        /// getter for the boss bounds
        /// </summary>
        public BoundingCircle Bounds => bounds;

        //the boss default health. Starting at 30 initially but may increase once items are added into the game,
        //may also have this scale based on lvl
        private double maxHealth = 60;
        public double CurrentHealth;

        //bool gets set to true when the boss health drops to 0
        public bool Dead = false;
        private bool deathAnimationPlayed = false;

        //boss move speed
        private float moveSpeed = 3f;

        //used to keep track of the boss invulnerability frames after being hit. May remove later so that higher fire rate feels good
        private double iFrameTimer = 0;
        public bool Hit = false;

        /// <summary>
        /// color blend of the boss
        /// </summary>
        public Color Color { get; set; } = Color.White;

        //bool swaps to true whenever the boss is activly preforming an attack
        private bool attacking = false;
        private double attackTimer = 0;


        //what state the boss is currently in (what attack)
        private state actionState = state.idle;

        //stall before charge
        private double stall = 0;

        //if the boss is currently huggin the top of the screen
        private bool top = true;

        //spritefont used in the main menu controls explanation and the game over screen
        private SpriteFont spriteFont;

        public bool slamAnimationPlayed;
        public BoundingCircle SlamHitBox;
        private double slamTimer;

        /// <summary>
        /// constructor for boss object
        /// </summary>
        /// <param name="position">raccoon spawn position</param>
        /// <param name="content">games content manager</param>
        public RatBossSprite(Vector2 position, ContentManager content)
        {
            this.Position = position;
            center = Position + new Vector2(80, 80);
            this.bounds = new BoundingCircle(position + new Vector2(80, 80), 75);
            LoadContent(content);
            CurrentHealth = maxHealth;
        }

        /// <summary>
        /// Loads the boss textures bounds and sound effects
        /// </summary>
        /// <param name="content"></param>
        public void LoadContent(ContentManager content)
        {
            arrowWarning = content.Load<Texture2D>("warningArrow");
            arrowWarningUp = content.Load<Texture2D>("warningArrowUp");
            warningCircle = content.Load<Texture2D>("WarningCircle");
            texture = content.Load<Texture2D>("ratBossSheet");
            healthEmpty = content.Load<Texture2D>("healthEmpty");
            healthFull = content.Load<Texture2D>("healthFull");
            spriteFont = content.Load<SpriteFont>("arial");
            test = content.Load<Texture2D>("EnemyProjectile");
            //hitSound = content.Load<SoundEffect>("raccoonHit");
            source = new Rectangle(0 * 160, 0 * 160, 160, 160);
        }

        public void Update(GameTime gameTime, PlayerController player)
        {
            if (!Dead)
            {
                if (actionState == state.idle || actionState == state.moveLeft || actionState == state.moveRight)
                {
                    attacking = false;
                }
                else
                {
                    attacking = true;
                }

                if (!attacking)
                {
                    if (attackTimer > 5)
                    {
                        int temp = RandomHelper.Next(3, 5);
                        actionState = (state)temp;
                        attackTimer -= 5;
                    }
                    else if (player.Position.X + 30 <= Position.X + 120 && player.Position.X + 30 >= Position.X + 40)
                    {
                        actionState = state.idle;
                    }
                    //check to see if boss needs to walk right
                    else if (player.Position.X + 30 > Position.X + 120)
                    {
                        if (Position.X + 160 < 1325)
                        {
                            actionState = state.moveRight;

                            Position.X += moveSpeed;
                        }

                    }
                    //check to see if the boss needs to walk left
                    else if (player.Position.X + 30 < Position.X + 40)
                    {
                        if (Position.X > 35)
                        {
                            actionState = state.moveLeft;

                            Position.X -= moveSpeed;
                        }

                    }

                    attackTimer += gameTime.ElapsedGameTime.TotalSeconds;

                }
                else
                {
                    if (actionState == state.charge)
                    {
                        if (stall >= .6)
                        {
                            if (top)
                            {
                                if (Position.Y + 160 < 725)
                                {
                                    Position.Y += 5 * moveSpeed;
                                    if (Position.Y + 160 >= 725)
                                    {
                                        actionState = state.idle;
                                        attackTimer -= 6;
                                        stall = 0;
                                        top = false;
                                    }
                                }
                            }
                            else
                            {
                                if (Position.Y > 30)
                                {
                                    Position.Y -= 5 * moveSpeed;
                                    if (Position.Y <= 30)
                                    {
                                        actionState = state.idle;
                                        attackTimer -= 6;
                                        stall = 0;
                                        top = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (actionState == state.slam)
                    {
                        if (slamAnimationPlayed)
                        {
                            SlamHitBox = new BoundingCircle(Position + new Vector2(80, 80), 500);
                        }
                    }
                }



                //check if the boss was hit by a player projectile
                foreach (PlayerProjectile proj in player.PlayerProjectile)
                {
                    if (proj.Bounds.CollidesWith(Bounds))
                    {
                        Hit = true;
                        CurrentHealth -= proj.Damage;
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

                attackTimer += gameTime.ElapsedGameTime.TotalSeconds;

                center = Position + new Vector2(80, 80);
                bounds.Center = new Vector2(Position.X + 80, Position.Y + 80);
            }
            else
            {
                bounds = new BoundingCircle(Vector2.Zero, 0);


            }

        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            //draw the hud (grows and shrinks based on the player's stats
            double full = CurrentHealth;
            double empty = maxHealth;
            Vector2 currentPos = new Vector2(400, 720);
            //draw the max health with lower opacity cans
            while (empty > 0)
            {
                if (empty > 0)
                {
                    spriteBatch.Draw(healthEmpty, currentPos, Color.White);
                    currentPos += new Vector2(10, 0);
                    empty--;
                }
            }

            //draw the current health with full and half cans over top of that
            currentPos = new Vector2(400, 720);
            while (full > 0)
            {
                if (full > 0)
                {
                    spriteBatch.Draw(healthFull, currentPos, Color.White);
                    currentPos += new Vector2(10, 0);
                    full--;
                }
            }

            spriteBatch.DrawString(spriteFont, $"Rat Boss      {CurrentHealth}/{maxHealth}", new Vector2(400, 720), Color.White);

            animationTimer += gameTime.ElapsedGameTime.TotalSeconds;

            if (actionState == state.idle)
            {
                if (animationTimer > .4)
                {

                    AnimationFrame++;

                    if (AnimationFrame > 1)
                    {
                        AnimationFrame = 0;
                    }
                    if (top)
                    {
                        source = new Rectangle(AnimationFrame * 160, 0 * 160, 160, 160);
                    }
                    else
                    {
                        source = new Rectangle(AnimationFrame * 160, 5 * 160, 160, 160);
                    }

                    animationTimer -= .4;
                }
            }
            else if (actionState == state.moveRight)
            {
                if (animationTimer > .1)
                {
                    AnimationFrame++;

                    if (AnimationFrame > 3)
                    {
                        AnimationFrame = 0;
                    }

                    if (top)
                    {
                        source = new Rectangle(AnimationFrame * 160, 1 * 160, 160, 160);
                    }
                    else
                    {
                        source = new Rectangle(AnimationFrame * 160, 6 * 160, 160, 160);
                    }
                    animationTimer -= .1;
                }

            }
            else if (actionState == state.moveLeft)
            {
                if (animationTimer > .1)
                {
                    AnimationFrame++;

                    if (AnimationFrame > 3)
                    {
                        AnimationFrame = 0;
                    }

                    if (top)
                    {
                        source = new Rectangle(AnimationFrame * 160, 2 * 160, 160, 160);
                    }
                    else
                    {
                        source = new Rectangle(AnimationFrame * 160, 7 * 160, 160, 160);
                    }

                    animationTimer -= .1;
                }

            }
            else if (actionState == state.charge)
            {
                if (animationTimer > .6)
                {

                    AnimationFrame++;


                    if (AnimationFrame > 2)
                    {
                        AnimationFrame = 2;
                        stall += .6;
                    }

                    if (stall >= .6)
                    {
                        if (AnimationFrame > 1)
                        {
                            AnimationFrame = 0;
                        }
                    }

                    if (top && stall < .6)
                    {
                        source = new Rectangle(AnimationFrame * 160, 3 * 160, 160, 160);
                    }
                    else if (!top && stall < .6)
                    {
                        source = new Rectangle(AnimationFrame * 160, 8 * 160, 160, 160);
                    }
                    else if (top && stall >= .6)
                    {
                        source = new Rectangle(AnimationFrame * 160, 4 * 160, 160, 160);
                    }
                    else if (!top && stall >= .6)
                    {
                        source = new Rectangle(AnimationFrame * 160, 9 * 160, 160, 160);
                    }



                    animationTimer -= .6;
                }

                if (top)
                {
                    spriteBatch.Draw(arrowWarning, Position + new Vector2(40, 160), Color.White);
                }
                else
                {
                    spriteBatch.Draw(arrowWarningUp, Position + new Vector2(40, -160), Color.White);
                }

            }
            else if (actionState == state.slam)
            {
                if (animationTimer > .3)
                {

                    AnimationFrame++;

                    if (AnimationFrame > 7)
                    {
                        slamAnimationPlayed = true;
                        AnimationFrame = 7;
                        slamTimer++;
                    }

                    if(slamTimer > 2)
                    {
                        actionState = state.idle;
                        slamAnimationPlayed = false;
                        slamTimer = 0;
                        SlamHitBox = new BoundingCircle();
                    }

                    if (top)
                    {
                        source = new Rectangle(AnimationFrame * 160, 10 * 160, 160, 160);
                    }
                    else
                    {
                        source = new Rectangle(AnimationFrame * 160, 10 * 160, 160, 160);
                    }

                    animationTimer -= .3;
                }
                spriteBatch.Draw(warningCircle, Position - new Vector2(420, 420), Color.White);
            }
                spriteBatch.Draw(texture, Position, source, Color);
        }
        }
    }
