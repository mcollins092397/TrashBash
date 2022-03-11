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
        public Vector2 StartPosition;
        public Vector2 EndPosition;
        private float rotation = 0;

        private GasParticleSystem gas;

        public bool ContentLoaded = false;

        private bool gasFired;

        private BoundingCircle bounds;

        public float activeTimer = 0;

        public bool delete;

        public BoundingCircle Bounds => bounds;

        public GasProjectile(Vector2 startPosition, Vector2 endPosition, GasParticleSystem gas)
        {
            this.Position = startPosition;
            this.StartPosition = startPosition;
            this.EndPosition = endPosition;
            EndPosition.Round();
            this.gas = gas;
        }

        public void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("GasCan");
            ContentLoaded = true;
        }

        public void Update(GameTime gameTime)
        {
            if (Position.X < EndPosition.X)
            {
                Position += new Vector2(speed, 0);
            }
            if (Position.X > EndPosition.X)
            {
                Position += new Vector2(-speed, 0);
            }
            if (Position.Y < EndPosition.Y)
            {
                Position += new Vector2(0, speed);
            }
            if (Position.Y > EndPosition.Y)
            {
                Position += new Vector2(0, -speed);
            }

            if(Position.X > EndPosition.X - 2 && Position.X < EndPosition.X + 2
                && Position.Y > EndPosition.Y - 2 && Position.Y < EndPosition.Y + 2
                && gasFired == false)
            {
                gas.PlaceGas(EndPosition);
                bounds = new BoundingCircle(Position, 64);
                gasFired = true;
            }

            if(gasFired)
            {
                activeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if(activeTimer > 2)
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
                spriteBatch.Draw(texture, Position, null, Color.White, (float)rotation, new Vector2(5, 7), 1, SpriteEffects.None, 0);
            }
        }


    }
}
