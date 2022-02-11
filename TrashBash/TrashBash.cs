using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;

namespace TrashBash
{

    public enum State
    {
        MainMenu = 0,
        Level1 = 1,
        GameOver = 2
    }
    public class TrashBash : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private PlayerController player;

        private Texture2D ball;
        private Texture2D title;
        private Texture2D rat;

        private PlayBtn playBtn;
        private ExitBtn exitBtn;

        private SpriteFont spriteFont;

        private State gameState = 0;

        public List<TrashSpiderSprite> Spiders = new List<TrashSpiderSprite>();
        private List<TrashSpiderSprite> deadSpiders = new List<TrashSpiderSprite>();

        private Random rnd = new Random();


        private int score = 0;
        private int enemySpawn = 2;
        private int scaler = 0;

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
            //trashSpider = new TrashSpiderSprite() { Position = new Vector2(400, 192) };
            player = new PlayerController() { Position = new Vector2((GraphicsDevice.Viewport.Width / 2) -32, (GraphicsDevice.Viewport.Height / 2)) };
            playBtn = new PlayBtn(new Vector2((GraphicsDevice.Viewport.Width / 4) - 80, GraphicsDevice.Viewport.Height / 2));
            exitBtn = new ExitBtn(new Vector2((float)(GraphicsDevice.Viewport.Width * 0.75) - 80, GraphicsDevice.Viewport.Height / 2));


            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

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

            player.Update(gameTime, Content);

            //player.Color = Color.White;


            if(gameState == State.MainMenu)
            {
                playBtn.Color = Color.White;

                if (player.Bounds.CollidesWith(playBtn.Bounds) && (Keyboard.GetState().IsKeyDown(Keys.Space) || GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed))
                {
                    playBtn.Color = Color.Red;
                    gameState = State.Level1;
                }
                if (player.Bounds.CollidesWith(exitBtn.Bounds) && (Keyboard.GetState().IsKeyDown(Keys.Space) || GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed))
                {
                    Exit();
                }
            }

            
            
            
            if(gameState == State.Level1)
            {
                if(Spiders.Count < enemySpawn)
                {
                    int side = rnd.Next(0, 4);
                    
                    if (side == 0)
                    {
                        Spiders.Add(new TrashSpiderSprite(new Vector2(rnd.Next(0, 760), 0), Content));
                    }
                    else if (side == 1)
                    {
                        Spiders.Add(new TrashSpiderSprite(new Vector2(GraphicsDevice.Viewport.Width, rnd.Next(0, 480)), Content));
                    }
                    else if (side == 2)
                    {
                        Spiders.Add(new TrashSpiderSprite(new Vector2(rnd.Next(0, 760), GraphicsDevice.Viewport.Height), Content));
                    }
                    else if (side == 3)
                    {
                        Spiders.Add(new TrashSpiderSprite(new Vector2(0, rnd.Next(0, 480)), Content));
                    }
                }

                foreach(TrashSpiderSprite spider in Spiders)
                {
                    spider.Update(gameTime, player.Position);
                    
                    if(spider.Hit == false)
                    {
                        spider.Color = Color.White;
                    }

                    foreach (PlayerProjectile proj in player.PlayerProjectile)
                    {
                        if (proj.Bounds.CollidesWith(spider.Bounds))
                        {
                            spider.Hit = true;
                            spider.Health -= proj.Damage;
                            if(spider.Health <= 0)
                            {
                                deadSpiders.Add(spider);
                                score++;
                                scaler++;
                            }
                            player.ProjectileRemove.Add(proj);
                        }
                    }

                    if(player.Hit == false)
                    {
                        if (player.Bounds.CollidesWith(spider.Bounds))
                        {
                            player.Hit = true;
                            player.PlayerCurrentHealth--;
                        }
                    }
                    

                    if (scaler == 10)
                    {
                        scaler = 0;
                        if (player.ProjFireRate > .1)
                        {
                            player.ProjFireRate -= .1f;
                        }
                        enemySpawn++;
                    }
                }
                
              
                foreach(TrashSpiderSprite spider in deadSpiders)
                {
                    Spiders.Remove(spider);
                }
                deadSpiders.Clear();

                if(player.PlayerCurrentHealth <= 0)
                {
                    gameState = State.GameOver;
                }
            }

            if(gameState == State.GameOver)
            {
                if ((Keyboard.GetState().IsKeyDown(Keys.Space) || GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed))
                {
                    Spiders.Clear();
                    player.Position = new Vector2((GraphicsDevice.Viewport.Width / 2) - 50, (GraphicsDevice.Viewport.Height / 2) - 30);
                    player.PlayerCurrentHealth = player.PlayerMaxHealth;
                    score = 0;
                    scaler = 0;
                    enemySpawn = 2;
                    player.ProjFireRate = .75f;
                    gameState = State.Level1;
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            if(gameState == State.MainMenu)
            {
                _spriteBatch.Draw(title, new Vector2(70, 20), null, Color.White, 0, new Vector2(0,0), .80f, SpriteEffects.None, 0);
                playBtn.Draw(gameTime, _spriteBatch);
                exitBtn.Draw(gameTime, _spriteBatch);
                _spriteBatch.DrawString(spriteFont, "             WASD/Left stick to Move \n                 Space/A to interact\n         Arrow keys/Right stick to shoot\nEsc/Back or interact with Exit button to quit", new Vector2((GraphicsDevice.Viewport.Width /2 - 225), GraphicsDevice.Viewport.Height - 125), Color.White);
                player.Draw(gameTime, _spriteBatch);
            }
            


            if(gameState == State.Level1)
            {
                foreach (TrashSpiderSprite spider in Spiders)
                {
                    spider.Draw(gameTime, _spriteBatch);
                }
                _spriteBatch.DrawString(spriteFont, "Health: " + player.PlayerCurrentHealth + "/" + player.PlayerMaxHealth + "\nScore: " + score, new Vector2(20, 20), Color.White);
                player.Draw(gameTime, _spriteBatch);
            }

            if(gameState == State.GameOver)
            {
                _spriteBatch.DrawString(spriteFont, "          GAME OVER\n   Esc/Back button to exit\nPress Space or A to restart", new Vector2((GraphicsDevice.Viewport.Width / 2) - 140, (GraphicsDevice.Viewport.Height / 2) - 30), Color.White);
            }



            //_spriteBatch.Draw(rat, new Vector2(100, 215), null, Color.White, 0, new Vector2(0, 0), .08f, SpriteEffects.None, 0);

            _spriteBatch.End();



            base.Draw(gameTime);
        }
    }
}
