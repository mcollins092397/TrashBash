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
        //sound triggered on the bag being shot by the player
        private SoundEffect hitSound;

        //the spiders current position
        public Vector2 Position;
        private Vector2 center;

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
        private float moveSpeed = 3f;

        //used to keep track of the spiders invulnerability frames after being hit. May remove later so that higher fire rate feels good
        private double iFrameTimer = 0;
        public bool Hit = false;

        /// <summary>
        /// color blend of the raccoon
        /// </summary>
        public Color Color { get; set; } = Color.White;

        /// <summary>
        /// constructor for raccoon object
        /// </summary>
        /// <param name="position">raccoon spawn position</param>
        /// <param name="content">games content manager</param>
        public RatBossSprite(Vector2 position, ContentManager content)
        {
            this.Position = position;
            center = Position + new Vector2(80, 80);
            this.bounds = new BoundingCircle(position + new Vector2(32, 32), 32);
            LoadContent(content);
        }

        /// <summary>
        /// Loads the raccoon textures bounds and sound effects
        /// </summary>
        /// <param name="content"></param>
        public void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("test160");
            //hitSound = content.Load<SoundEffect>("raccoonHit");
        }

        public void Update(GameTime gameTime, PlayerController player)
        {
            if(player.Position.X > center.X)
            {
                Position.X += moveSpeed;
            }

            if (player.Position.X < center.X)
            {
                Position.X -= moveSpeed;
            }

            center = Position + new Vector2(80, 80);
            bounds.Center = new Vector2(Position.X + 80, Position.Y + 80);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Position, Color);
        }
    }
}
