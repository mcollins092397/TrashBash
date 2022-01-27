using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TrashBash
{
    public enum Direction
    {
        Left = 0,
        Right = 1
    }


    public class TrashSpiderSprite
    {

        public Direction Direction = Direction.Right;


        public Vector2 Position;

        private double directionTimer;

        private double animationTimer;

        private short animationFrame = 0;

        private Texture2D texture;


        public void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("TrashSpiderLeftRight");
        }


        public void Update(GameTime gameTime)
        {
            directionTimer += gameTime.ElapsedGameTime.TotalSeconds;

            if(directionTimer > 2.0)
            {
                switch(Direction)
                {
                    case Direction.Right:
                        Direction = Direction.Left;
                        break;
                    case Direction.Left:
                        Direction = Direction.Right;
                        break;
                }
                directionTimer -= 2.0;
            }


            switch (Direction)
            {
                case Direction.Right:
                    Position += new Vector2(1, 0) * 100 * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    break;
                case Direction.Left:
                    Position += new Vector2(-1, 0) * 100 * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    break;
            }
        }


        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            animationTimer += gameTime.ElapsedGameTime.TotalSeconds;

            if (animationTimer > 0.3)
            {
                animationFrame++;
                
                if (animationFrame > 3)
                {
                    animationFrame = 0;
                }

                animationTimer -= 0.3;
            }


            var source = new Rectangle(animationFrame * 64, (int)Direction * 64, 64, 64);
            spriteBatch.Draw(texture, Position, source, Color.White);
        }

    }
}
