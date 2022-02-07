using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace TrashBash
{
    public class TrashBash : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private TrashSpiderSprite trashSpider;

        private PlayerController player;

        private Texture2D ball;
        private Texture2D title;
        private Texture2D rat;

        private PlayBtn playBtn;
        private ExitBtn exitBtn;

        private SpriteFont spriteFont;

        private int state = 1;

        public TrashBash()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.IsFullScreen = true;
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
            _graphics.PreferredBackBufferWidth = 760;
            _graphics.PreferredBackBufferHeight = 480;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            trashSpider = new TrashSpiderSprite() { Position = new Vector2(400, 192) };
            player = new PlayerController() { Position = new Vector2((GraphicsDevice.Viewport.Width / 2) -32, (GraphicsDevice.Viewport.Height / 2)) };
            playBtn = new PlayBtn(new Vector2((GraphicsDevice.Viewport.Width / 4) - 80, GraphicsDevice.Viewport.Height / 2));
            exitBtn = new ExitBtn(new Vector2((float)(GraphicsDevice.Viewport.Width * 0.75) - 80, GraphicsDevice.Viewport.Height / 2));


            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

            trashSpider.LoadContent(Content);
            player.LoadContent(Content);
            playBtn.LoadContent(Content);
            exitBtn.LoadContent(Content);
            ball = Content.Load<Texture2D>("ball");
            title = Content.Load<Texture2D>("TrashBash");
            rat = Content.Load<Texture2D>("Rat");
            spriteFont = Content.Load<SpriteFont>("arial");

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            trashSpider.Update(gameTime);
            player.Update(gameTime, Content);

            trashSpider.Color = Color.White;
            player.Color = Color.White;
            playBtn.Color = Color.White;

            if (player.Bounds.CollidesWith(playBtn.Bounds) && (Keyboard.GetState().IsKeyDown(Keys.Space) || GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed))
            {
                playBtn.Color = Color.Red;
            }
            if (player.Bounds.CollidesWith(exitBtn.Bounds) && (Keyboard.GetState().IsKeyDown(Keys.Space) || GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed))
            {
                Exit();
            }
            
            foreach(PlayerProjectile proj in player.PlayerProjectile)
            {
                if(proj.Bounds.CollidesWith(trashSpider.Bounds))
                {
                    trashSpider.Color = Color.Red;
                }
            }

            if (player.Bounds.CollidesWith(trashSpider.Bounds))
            {
                player.Color = Color.Red;
                player.PlayerCurrentHealth--;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            _spriteBatch.Draw(title, new Vector2(30, 20), null, Color.White, 0, new Vector2(0,0), .80f, SpriteEffects.None, 0);
            _spriteBatch.Draw(rat, new Vector2(100, 215), null, Color.White, 0, new Vector2(0, 0), .08f, SpriteEffects.None, 0);
            trashSpider.Draw(gameTime, _spriteBatch);
            playBtn.Draw(gameTime, _spriteBatch);
            exitBtn.Draw(gameTime, _spriteBatch);
            _spriteBatch.DrawString(spriteFont, "             WASD/Left stick to Move \n                 Space/A to interact\nEsc/Back or interact with Exit button to quit", new Vector2((GraphicsDevice.Viewport.Width /2 - 225), GraphicsDevice.Viewport.Height - 100), Color.White);
            //_spriteBatch.DrawString(spriteFont, "Health: " + player.PlayerCurrentHealth + "/" + player.PlayerMaxHealth, new Vector2(20, 20), Color.White);
            player.Draw(gameTime, _spriteBatch);
            

            _spriteBatch.End();



            base.Draw(gameTime);
        }
    }
}
