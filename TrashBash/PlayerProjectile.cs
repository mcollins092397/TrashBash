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
    public class PlayerProjectile
    {
        private float speed;
        private float damage;
        public float Range;
        public Vector2 Position;
        private Direction direction;
        private Texture2D texture;

        public bool ContentLoaded = false;

        private BoundingCircle bounds;

        public BoundingCircle Bounds => bounds;

        public PlayerProjectile(float speed, float damage, Direction direction, float range, Vector2 position)
        {
            this.speed = speed;
            this.damage = damage;
            this.direction = direction;
            this.Range = range;
            this.Position = position;
            this.bounds = new BoundingCircle(position + new Vector2(8, 8), 8);
        }

        public void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("PlayerProjectile");
        }

        public void Update(GameTime gameTime)
        {
            if (direction == Direction.Up)
            {
                Position += new Vector2(0, -speed);
            }
            else if (direction == Direction.Down)
            {
                Position += new Vector2(0, speed);
            }
            else if (direction == Direction.Left)
            {
                Position += new Vector2(-speed, 0);
            }
            else if (direction == Direction.Right)
            {
                Position += new Vector2(speed, 0);
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Position, Color.White);
        }


    }
}
