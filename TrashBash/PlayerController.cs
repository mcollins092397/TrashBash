using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

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

        private Vector2 position = new Vector2(200, 200);

        private double animationTimer;

        private double sipTimer;

        private short animationFrame = 0;

        public Direction Direction = Direction.Idle;

        public void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("DudeBro");
        }

        public void Update(GameTime gameTime)
        {
            keyboardState = Keyboard.GetState();
            //Keyboard Movement
            if (keyboardState.IsKeyDown(Keys.W))
            {
                position += new Vector2(0, -1);
                Direction = Direction.Up;
                sipTimer = 0;
            }
            if (keyboardState.IsKeyDown(Keys.S))
            {
                position += new Vector2(0, 1);
                Direction = Direction.Down;
                sipTimer = 0;
            }
            if (keyboardState.IsKeyDown(Keys.D))
            {
                position += new Vector2(1, 0);
                Direction = Direction.Right;
                sipTimer = 0;
            }
            if (keyboardState.IsKeyDown(Keys.A))
            {
                position += new Vector2(-1, 0);
                Direction = Direction.Left;
                sipTimer = 0;
            }
            if (keyboardState.IsKeyUp(Keys.W) && keyboardState.IsKeyUp(Keys.A) && keyboardState.IsKeyUp(Keys.S) && keyboardState.IsKeyUp(Keys.D))
            {
                Direction = Direction.Idle;
            }
            
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
            spriteBatch.Draw(texture, position, source, Color.White);
        }
    }
}
