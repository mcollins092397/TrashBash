using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using TrashBash.Collisions;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace TrashBash
{
    public enum Direction
    {
        Idle = 0,
        Right = 1,
        Up = 2,
        Left = 3,
        Down = 4
    }

    public class PlayerController
    {
        private GamePadState gamePadState;

        private KeyboardState keyboardState;

        private Texture2D texture;

        public Vector2 Position;

        private double animationTimer;

        private double sipTimer;

        private short animationFrame = 0;

        public Direction Direction = Direction.Idle;

        private BoundingRectangle bounds = new BoundingRectangle(new Vector2(200 + 20, 200), 25, 64);

        public List<PlayerProjectile> PlayerProjectile = new List<PlayerProjectile>();
        public List<PlayerProjectile> ProjectileRemove = new List<PlayerProjectile>();

        public float ProjSpeed = 3;
        public float ProjDmg = 1;
        public float ProjRange = 250;
        public float ProjFireRate = .75f;
        private double lastFire = 0;

        public float MovementSpeed = 2f;
        public int PlayerMaxHealth = 6;
        public int PlayerCurrentHealth;

        public bool Hit = false;
        private float iFrameTimer;
        public float Iframes = 1f;

        public Vector2 LastMove;

        private SoundEffect gunshot;


        /// <summary>
        /// bounding volume of the sprite
        /// </summary>
        public BoundingRectangle Bounds
        {
            get
            {
                return bounds;
            }
        }


        /// <summary>
        /// color blend of the player
        /// </summary>
        public Color Color { get; set; } = Color.White;


        public void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("DudeBro");
            PlayerCurrentHealth = PlayerMaxHealth;
            gunshot = content.Load<SoundEffect>("gunshot");
        }

        public void Update(GameTime gameTime, ContentManager content)
        {
            gamePadState = GamePad.GetState(0);
            keyboardState = Keyboard.GetState();
            //Keyboard Movement
            if (keyboardState.IsKeyDown(Keys.W))
            {
                Position += new Vector2(0, -MovementSpeed);
                Direction = Direction.Up;
            }
            if (keyboardState.IsKeyDown(Keys.S))
            {
                Position += new Vector2(0, MovementSpeed);
                Direction = Direction.Down;
            }
            if (keyboardState.IsKeyDown(Keys.D))
            {
                Position += new Vector2(MovementSpeed, 0);
                Direction = Direction.Right;
            }
            if (keyboardState.IsKeyDown(Keys.A))
            {
                Position += new Vector2(-MovementSpeed, 0);
                Direction = Direction.Left;
            }

            //Controller Movement
            Position += gamePadState.ThumbSticks.Left * new Vector2(MovementSpeed, -MovementSpeed);
            if (gamePadState.ThumbSticks.Left.Y > 0.1f)
            {
                Direction = Direction.Up;
            }
            if (gamePadState.ThumbSticks.Left.Y < -0.1f)
            {
                Direction = Direction.Down;
            }
            if (gamePadState.ThumbSticks.Left.X > 0.1f)
            {
                Direction = Direction.Right;
            }
            if (gamePadState.ThumbSticks.Left.X < -0.1f)
            {
                Direction = Direction.Left;
            }
            
            //check if both the gamepad and controller are not recieving movement then set to idle if so
            if (keyboardState.IsKeyUp(Keys.W) && keyboardState.IsKeyUp(Keys.A) && keyboardState.IsKeyUp(Keys.S) && keyboardState.IsKeyUp(Keys.D) && gamePadState.ThumbSticks.Left.Y == 0 && gamePadState.ThumbSticks.Left.X == 0)
            {
                Direction = Direction.Idle;
            }

            //if the player made a movement reset sip timer
            if (Direction != (Direction.Idle))
            {
                sipTimer = 0;
            }



            //Check for fire commands and add projectile to list
            if(gameTime.TotalGameTime.TotalSeconds > (lastFire + ProjFireRate))
            {
                if (keyboardState.IsKeyDown(Keys.Up) || gamePadState.ThumbSticks.Right.Y > 0.5f)
                {
                    PlayerProjectile.Add(new PlayerProjectile(ProjSpeed, ProjDmg, Direction.Up, ProjRange, Position + new Vector2(22,15)));
                    gunshot.Play(.1f, 0, 0);
                    lastFire = gameTime.TotalGameTime.TotalSeconds;
                }
                else if (keyboardState.IsKeyDown(Keys.Down) || gamePadState.ThumbSticks.Right.Y < -0.5f)
                {
                    PlayerProjectile.Add(new PlayerProjectile(ProjSpeed, ProjDmg, Direction.Down, ProjRange, Position + new Vector2(32, 15)));
                    gunshot.Play(.1f, 0, 0);
                    lastFire = gameTime.TotalGameTime.TotalSeconds;
                }
                else if (keyboardState.IsKeyDown(Keys.Left) || gamePadState.ThumbSticks.Right.X < -0.5f)
                {
                    PlayerProjectile.Add(new PlayerProjectile(ProjSpeed, ProjDmg, Direction.Left, ProjRange, Position + new Vector2(22, 15)));
                    gunshot.Play(.1f, 0, 0);
                    lastFire = gameTime.TotalGameTime.TotalSeconds;
                }
                else if (keyboardState.IsKeyDown(Keys.Right) || gamePadState.ThumbSticks.Right.X > 0.5f)
                {
                    PlayerProjectile.Add(new PlayerProjectile(ProjSpeed, ProjDmg, Direction.Right, ProjRange, Position + new Vector2(52, 15)));
                    gunshot.Play(.1f, 0, 0);
                    lastFire = gameTime.TotalGameTime.TotalSeconds;
                }

            }

            if (keyboardState.IsKeyDown(Keys.Up) || gamePadState.ThumbSticks.Right.Y > 0.5f)
            {
                Direction = Direction.Up;
            }
            else if (keyboardState.IsKeyDown(Keys.Down) || gamePadState.ThumbSticks.Right.Y < -0.5f)
            {
                Direction = Direction.Down;
            }
            else if (keyboardState.IsKeyDown(Keys.Left) || gamePadState.ThumbSticks.Right.X < -0.5f)
            {
                Direction = Direction.Left;
            }
            else if (keyboardState.IsKeyDown(Keys.Right) || gamePadState.ThumbSticks.Right.X > 0.5f)
            {
                Direction = Direction.Right;
            }


            //Load content for every projectile in the list
            foreach (PlayerProjectile proj in PlayerProjectile)
            {
                if (proj.ContentLoaded == false)
                {
                    proj.LoadContent(content);
                    proj.ContentLoaded = true;
                }

            }

            //Update each projectile in list
            foreach (PlayerProjectile proj in PlayerProjectile)
            {
                proj.Update(gameTime);
            }

            foreach (PlayerProjectile proj in PlayerProjectile)
            {
                if (proj.Position.X > proj.StartPosition.X + ProjRange || proj.Position.X < proj.StartPosition.X - ProjRange || proj.Position.Y > proj.StartPosition.Y + ProjRange || proj.Position.Y < proj.StartPosition.Y - ProjRange)
                {
                    //add the projectile to a remove list
                    ProjectileRemove.Add(proj);
                }
            }

            //then for each in the remove list
            foreach (PlayerProjectile proj in ProjectileRemove)
            {
                PlayerProjectile.Remove(proj);
            }
            //remove from the main list

            //clear the remove list
            ProjectileRemove.Clear();

            if (Hit)
            {
                iFrameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                Color = Color.Red;

                if (iFrameTimer > Iframes)
                {
                    Hit = false;
                    iFrameTimer = 0;
                }
            }
            if(!Hit)
            {
                Color = Color.White;
            }

            //update the bounds
            bounds.X = Position.X + 20;
            bounds.Y = Position.Y;

        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            animationTimer += gameTime.ElapsedGameTime.TotalSeconds;

            if (Direction == Direction.Idle)
            {
                sipTimer += gameTime.ElapsedGameTime.TotalSeconds;
            }


            if (sipTimer > 5)
            {
                animationFrame = 3;
                sipTimer -= 5;
                animationTimer -= 0.2;
            }
            else if (animationTimer >.2)
            {
                animationFrame++;
                if (animationFrame > 2)
                {
                    animationFrame = 0;
                }
                animationTimer -= 0.2;
            }

            foreach (PlayerProjectile proj in PlayerProjectile)
            {
                if (proj.Direction == Direction.Up)
                {
                    proj.Draw(gameTime, spriteBatch);
                }

            }

            var source = new Rectangle(animationFrame * 64, (int)Direction * 64, 64, 64);
            spriteBatch.Draw(texture, Position, source, Color);

            foreach (PlayerProjectile proj in PlayerProjectile)
            {
                if (proj.Direction != Direction.Up)
                {
                    proj.Draw(gameTime, spriteBatch);
                }
                
            }
        }
    }
}
