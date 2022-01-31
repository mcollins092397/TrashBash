using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using TrashBash.Collisions;

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

        private BoundingRectangle bounds = new BoundingRectangle(new Vector2(200 + 12, 200), 40, 64);


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
        }

        public void Update(GameTime gameTime)
        {
            gamePadState = GamePad.GetState(0);
            keyboardState = Keyboard.GetState();
            //Keyboard Movement
            if (keyboardState.IsKeyDown(Keys.W))
            {
                Position += new Vector2(0, -1);
                Direction = Direction.Up;
            }
            if (keyboardState.IsKeyDown(Keys.S))
            {
                Position += new Vector2(0, 1);
                Direction = Direction.Down;
            }
            if (keyboardState.IsKeyDown(Keys.D))
            {
                Position += new Vector2(1, 0);
                Direction = Direction.Right;
            }
            if (keyboardState.IsKeyDown(Keys.A))
            {
                Position += new Vector2(-1, 0);
                Direction = Direction.Left;
            }

            //Controller Movement
            Position += gamePadState.ThumbSticks.Left * new Vector2(1, -1);
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



            //update the bounds
            bounds.X = Position.X + 12;
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

            var source = new Rectangle(animationFrame * 64, (int)Direction * 64, 64, 64);
            spriteBatch.Draw(texture, Position, source, Color);
        }
    }
}
