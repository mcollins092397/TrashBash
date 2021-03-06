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
    public class GasProjectile
    {
        private float speed = 4.0f;
        public Vector2 Position;
        private Texture2D texture;
        private Texture2D warningTexture;
        public Vector2 StartPosition;
        public Vector2 EndPosition;
        private float rotation = 0;

        private double animationTimer;
        private short animationFrame;

        private SoundEffect sound;

        private List<Vector2> path = new List<Vector2>();
        private Vector2 nextPoint = new Vector2();
        int count = 0;

        private GasParticleSystem gas;

        public bool ContentLoaded = false;

        private bool gasFired;

        private bool drawWarning = true;

        private BoundingCircle bounds;

        public float activeTimer = 0;
        private bool boundsMade;

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
                path.Add(Vector2.Hermite(startPosition, endPosition + new Vector2(-raccoonPosition.X, -(500)), endPosition, endPosition + new Vector2(-raccoonPosition.X, 400), i));   
            }
            nextPoint = path[count];
            nextPoint.Round();
        }

        public void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("GasCan");
            ContentLoaded = true;
            warningTexture = content.Load<Texture2D>("warning");
            sound = content.Load<SoundEffect>("gasSound");
        }

        public void Update(GameTime gameTime)
        {

            if (Position.X > EndPosition.X - speed && Position.X < EndPosition.X + speed
                && Position.Y > EndPosition.Y - speed && Position.Y < EndPosition.Y + speed
                && gasFired == false)
            {
                sound.Play(.2f, 0, 0);
                gas.PlaceGas(EndPosition);
                gasFired = true;
            }

            while (Position.X > nextPoint.X - speed && Position.X < nextPoint.X + speed
            && Position.Y > nextPoint.Y - speed && Position.Y < nextPoint.Y + speed
            && gasFired == false && count < path.Count-1)
            {
                count++;
                nextPoint = path[count];
                nextPoint.Round();
            }

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

            if (gasFired)
            {
                drawWarning = false;
                activeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if(activeTimer > 0.4 && !boundsMade)
                {
                    bounds = new BoundingCircle(Position, 64);
                }
                if(activeTimer > 2.5)
                {
                    delete = true;
                }
            }



            rotation += .1f;
        }

        public void ClearGas()
        {
            gas.ClearGas();
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
