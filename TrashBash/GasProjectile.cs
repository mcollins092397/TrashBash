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
    public class GasProjectile
    {
        private float speed = 2.0f;
        public Vector2 Position;
        private Texture2D texture;
        private Texture2D warningTexture;
        public Vector2 StartPosition;
        public Vector2 EndPosition;
        private float rotation = 0;

        private double animationTimer;
        private short animationFrame;


        private List<Vector2> path = new List<Vector2>();
        private Vector2 nextPoint = new Vector2();
        int count = 0;

        private GasParticleSystem gas;

        public bool ContentLoaded = false;

        private bool gasFired;

        private bool drawWarning = true;

        private BoundingCircle bounds;

        public float activeTimer = 0;

        public bool delete;

        public BoundingCircle Bounds => bounds;

        public GasProjectile(Vector2 startPosition, Vector2 endPosition, GasParticleSystem gas, Vector2 raccoonPosition)
        {
            this.Position = startPosition;
            this.StartPosition = startPosition;
            this.EndPosition = endPosition;
            EndPosition.Round();
            this.gas = gas;

            for (float i = 0; i <= 1; i += 0.01f)
            {
                path.Add(Vector2.Hermite(startPosition, endPosition + new Vector2(-raccoonPosition.X, -(500)), endPosition, endPosition + new Vector2(-raccoonPosition.X, 500), i));   
            }
            nextPoint = path[count];
            nextPoint.Round();
        }

        public void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("GasCan");
            ContentLoaded = true;
            warningTexture = content.Load<Texture2D>("warning");
        }

        public void Update(GameTime gameTime)
        {
            if (Position.X < nextPoint.X)
            {
                Position += new Vector2(speed, 0);
            }
            if (Position.X > nextPoint.X)
            {
                Position += new Vector2(-speed, 0);
            }
            if (Position.Y < nextPoint.Y)
            {
                Position += new Vector2(0, speed);
            }
            if (Position.Y > nextPoint.Y)
            {
                Position += new Vector2(0, -speed);
            }

            if (Position.X > EndPosition.X - 2 && Position.X < EndPosition.X + 2
                && Position.Y > EndPosition.Y - 2 && Position.Y < EndPosition.Y + 2
                && gasFired == false)
            {
                gas.PlaceGas(EndPosition);
                bounds = new BoundingCircle(Position, 64);
                gasFired = true;
            }

            if (Position.X > nextPoint.X - 2 && Position.X < nextPoint.X + 2
            && Position.Y > nextPoint.Y - 2 && Position.Y < nextPoint.Y + 2
            && gasFired == false && count < path.Count-1)
            {
                count++;
                nextPoint = path[count];
                nextPoint.Round();
            }


            if (gasFired)
            {
                drawWarning = false;
                activeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if(activeTimer > 2.4)
                {
                    delete = true;
                }
            }



            rotation += .1f;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if(!gasFired)
            {
                spriteBatch.Draw(texture, Position, null, Color.White, (float)rotation, new Vector2(5, 7), 1, SpriteEffects.None, 1);
            }

            if(drawWarning)
            {
                animationTimer += gameTime.ElapsedGameTime.TotalSeconds;

                if (animationTimer > .5)
                {
                    animationFrame++;

                    if(animationFrame > 1)
                    {
                        animationFrame = 0;
                    }

                    animationTimer -= .5;
                }

                var source = new Rectangle(animationFrame * 64, 0, 64, 64);
                spriteBatch.Draw(warningTexture, EndPosition - new Vector2(32,32), source, Color.White);
            }

            //draws the curve of the projectile
            for (int i = 0; i < path.Count ; i ++)
            {
                //spriteBatch.Draw(texture, path[i], null, Color.White, (float)rotation, new Vector2(5, 7), 1, SpriteEffects.None, 0);
            }
        }


    }
}
