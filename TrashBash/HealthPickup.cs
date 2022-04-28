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
    public class HealthPickup
    {
        //the raccoons texture
        private Texture2D texture;


        //the raccoons position
        public Vector2 Position;

        //the raccoons bounds
        private BoundingRectangle bounds;

        //getter for the raccoon bounds
        public BoundingRectangle Bounds => bounds;

        //hit sound effect for the raccoon
        private SoundEffect hitSound;

        //the level that the bag appears in
        public float Level;

        /// <summary>
        /// constructor for raccoon object
        /// </summary>
        /// <param name="position">raccoon spawn position</param>
        /// <param name="content">games content manager</param>
        public HealthPickup(Vector2 position, ContentManager content, float level)
        {
            this.Position = position;
            this.bounds = new BoundingRectangle(position, 32, 32);
            this.Level = level;
            LoadContent(content);
        }

        /// <summary>
        /// Loads the raccoon textures bounds and sound effects
        /// </summary>
        /// <param name="content"></param>
        public void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("HealthPickup");
            //hitSound = content.Load<SoundEffect>("raccoonHit");
        }

        /// <summary>
        /// update loop for the raccoon
        /// </summary>
        /// <param name="gameTime">gametime object </param>
        /// <param name="player">player object</param>
        /// <param name="gas">the gas particle system that manages the gas explosion</param>
        /// <param name="content">content manager</param>
        public void Update(GameTime gameTime)
        {

        }

        /// <summary>
        /// Draws and animates the raccoon sprite
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="spriteBatch"></param>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            
            spriteBatch.Draw(texture, Position, null, Color.White);
        }
    }
}
