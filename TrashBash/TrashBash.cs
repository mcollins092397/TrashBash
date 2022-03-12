using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace TrashBash
{

    public enum State
    {
        MainMenu = 0,
        LevelWaves = 1,
        GameOver = 2,
        Level0 = 3

    }
    public class TrashBash : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private PlayerController player;

        private Texture2D fence;
        private Texture2D fenceVerticle;
        private Texture2D title;
        private Texture2D healthCan;
        private Texture2D halfHealthCan;
        private Texture2D emptyHealthCan;
        private Texture2D background;

        private SoundEffect hit;
        private Song bossMusic;

        private PlayBtn playBtn;
        private ExitBtn exitBtn;

        private SpriteFont spriteFont;

        private State gameState = 0;

        public List<TrashSpiderSprite> livingSpiders = new List<TrashSpiderSprite>();
        private List<TrashSpiderSprite> deadSpiders = new List<TrashSpiderSprite>();

        public List<RaccoonSprite> livingRaccoons = new List<RaccoonSprite>();
        private List<RaccoonSprite> deadRaccoons = new List<RaccoonSprite>();

        private List<FenceTop> fenceTops = new List<FenceTop>();
        private List<FenceBottom> fenceBottoms = new List<FenceBottom>();
        private List<FenceSide> fenceSides = new List<FenceSide>();

        private Random rnd = new Random();


        private int score = 0;
        private int enemySpawn = 2;
        private int scaler = 0;

        public GasParticleSystem Gas;

        MouseState _priorMouse;

        //2d array representing the screen, used for A* pathfinding
        public int[,] Grid = new int[77,137];

        public TrashBash()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.IsFullScreen = true;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.PreferredBackBufferWidth = 1366;
            _graphics.PreferredBackBufferHeight = 768;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            //trashSpider = new TrashSpiderSprite() { Position = new Vector2(400, 192) };
            player = new PlayerController() { Position = new Vector2((GraphicsDevice.Viewport.Width / 2) -32, (GraphicsDevice.Viewport.Height / 2)) };
            playBtn = new PlayBtn(new Vector2((GraphicsDevice.Viewport.Width / 4) - 80, GraphicsDevice.Viewport.Height / 2));
            exitBtn = new ExitBtn(new Vector2((float)(GraphicsDevice.Viewport.Width * 0.75) - 80, GraphicsDevice.Viewport.Height / 2));

            Gas = new GasParticleSystem(this, 40);
            Components.Add(Gas);

            base.Initialize();
        }

        private void InitializeLevel0()
        {
            fenceBottoms.Clear();
            fenceSides.Clear();
            fenceTops.Clear();

            fenceTops.Add(new FenceTop(new Vector2(4, 0)));
            fenceTops.Add(new FenceTop(new Vector2(260, 0)));

            fenceTops.Add(new FenceTop(new Vector2(850, 0)));
            fenceTops.Add(new FenceTop(new Vector2(1106, 0)));

            foreach (FenceTop fence in fenceTops)
            {
                fence.LoadContent(Content);
                for(int y = (int)fence.Position.Y / 10; y < (int)((fence.Position.Y + fence.Height) / 10); y++)
                {
                    for (int x = (int)fence.Position.X / 10; x < (int)((fence.Position.X + fence.Width)/10); x++)
                    {
                        Grid[y, x] += 10;
                    }
                }
            }

            fenceBottoms.Add(new FenceBottom(new Vector2(4, 676)));
            fenceBottoms.Add(new FenceBottom(new Vector2(260, 676)));
            fenceBottoms.Add(new FenceBottom(new Vector2(516, 676)));
            fenceBottoms.Add(new FenceBottom(new Vector2(772, 676)));
            fenceBottoms.Add(new FenceBottom(new Vector2(850, 676)));
            fenceBottoms.Add(new FenceBottom(new Vector2(1106, 676)));



            foreach (FenceBottom fence in fenceBottoms)
            {
                fence.LoadContent(Content);
                for (int y = (int)fence.Position.Y / 10; y < (int)((fence.Position.Y + fence.Height) / 10); y++)
                {
                    for (int x = (int)fence.Position.X / 10; x < (int)((fence.Position.X + fence.Width) / 10); x++)
                    {
                        Grid[y, x] += 10;
                    }
                }
            }

            fenceSides.Add(new FenceSide(new Vector2(0, 4)));
            fenceSides.Add(new FenceSide(new Vector2(0, 260)));
            fenceSides.Add(new FenceSide(new Vector2(0, 516)));

            fenceSides.Add(new FenceSide(new Vector2(1354, 4)));
            fenceSides.Add(new FenceSide(new Vector2(1354, 260)));
            fenceSides.Add(new FenceSide(new Vector2(1354, 515)));

            

            foreach (FenceSide fence in fenceSides)
            {
                fence.LoadContent(Content);
                for (int y = (int)fence.Position.Y / 10; y < (int)((fence.Position.Y + fence.Height) / 10); y++)
                {
                    for (int x = (int)fence.Position.X / 10; x < (int)((fence.Position.X + fence.Width) / 10); x++)
                    {
                        Grid[y, x] += 10;
                    }
                }
            }

            livingRaccoons.Add(new RaccoonSprite(new Vector2(1200, 100), Content));
            livingRaccoons.Add(new RaccoonSprite(new Vector2(575, 250), Content));
            //livingRaccoons.Add(new RaccoonSprite(new Vector2(50, 100), Content));
            livingRaccoons.Add(new RaccoonSprite(new Vector2(50, 600), Content));

            gameState = State.Level0;
        }

        private void InitializeLevel1()
        {
            fenceBottoms.Clear();
            fenceSides.Clear();
            fenceTops.Clear();

            fenceTops.Add(new FenceTop(new Vector2(4, 0)));
            fenceTops.Add(new FenceTop(new Vector2(260, 0)));

            fenceTops.Add(new FenceTop(new Vector2(850, 0)));
            fenceTops.Add(new FenceTop(new Vector2(1106, 0)));

            foreach(FenceTop fence in fenceTops)
            {
                fence.LoadContent(Content);
            }

            fenceBottoms.Add(new FenceBottom(new Vector2(4, 676)));
            fenceBottoms.Add(new FenceBottom(new Vector2(260, 676)));

            fenceBottoms.Add(new FenceBottom(new Vector2(850, 676)));
            fenceBottoms.Add(new FenceBottom(new Vector2(1106, 676)));

            

            foreach(FenceBottom fence in fenceBottoms)
            {
                fence.LoadContent(Content);
            }

            fenceSides.Add(new FenceSide(new Vector2(0, 4)));
            fenceSides.Add(new FenceSide(new Vector2(0, 260)));
            fenceSides.Add(new FenceSide(new Vector2(0, 516)));

            fenceSides.Add(new FenceSide(new Vector2(1354, 4)));
            fenceSides.Add(new FenceSide(new Vector2(1354, 260)));
            fenceSides.Add(new FenceSide(new Vector2(1354, 515)));

            foreach (FenceSide fence in fenceSides)
            {
                fence.LoadContent(Content);
            }

            MediaPlayer.Play(bossMusic);

            gameState = State.LevelWaves;
        }

        

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

            player.LoadContent(Content);
            playBtn.LoadContent(Content);
            exitBtn.LoadContent(Content);
            title = Content.Load<Texture2D>("Logo");
            spriteFont = Content.Load<SpriteFont>("arial");
            healthCan = Content.Load<Texture2D>("HealthCans(hud)/Can");
            emptyHealthCan = Content.Load<Texture2D>("HealthCans(hud)/TransparentCan");
            halfHealthCan = Content.Load<Texture2D>("HealthCans(hud)/HalfCan");
            background = Content.Load<Texture2D>("background");
            fence = Content.Load<Texture2D>("Fence");
            fenceVerticle = Content.Load<Texture2D>("FenceVerticle");
            hit = Content.Load<SoundEffect>("hit");

            bossMusic = Content.Load<Song>("heavy_metal_looping");
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = .1f;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            player.LastMove = player.Position;
            player.Update(gameTime, Content);

            //player.Color = Color.White;

            foreach (FenceTop fence in fenceTops)
            {
                if (player.Bounds.CollidesWith(fence.Bounds))
                {
                    player.Position = player.LastMove;
                }
                foreach (PlayerProjectile proj in player.PlayerProjectile)
                {
                    if (proj.Bounds.CollidesWith(fence.Bounds))
                    {
                        player.ProjectileRemove.Add(proj);
                    }
                }
            }

            foreach (FenceBottom fence in fenceBottoms)
            {
                if (player.Bounds.CollidesWith(fence.Bounds))
                {
                    player.Position = player.LastMove;
                }
                foreach (PlayerProjectile proj in player.PlayerProjectile)
                {
                    if (proj.Bounds.CollidesWith(fence.Bounds))
                    {
                        player.ProjectileRemove.Add(proj);
                    }
                }
            }

            foreach (FenceSide fence in fenceSides)
            {
                if (player.Bounds.CollidesWith(fence.Bounds))
                {
                    player.Position = player.LastMove;
                }
                foreach (PlayerProjectile proj in player.PlayerProjectile)
                {
                    if (proj.Bounds.CollidesWith(fence.Bounds))
                    {
                        player.ProjectileRemove.Add(proj);
                    }
                }
            }

            foreach (TrashSpiderSprite spider in livingSpiders)
            {
                spider.Update(gameTime, player);

                if (spider.Health <= 0)
                {
                    deadSpiders.Add(spider);
                    score++;
                    scaler++;
                }

                if (player.Hit == false)
                {
                    if (player.Bounds.CollidesWith(spider.Bounds))
                    {
                        player.Hit = true;
                        player.PlayerCurrentHealth--;
                        hit.Play(.3f, 0, 0);
                    }
                }
            }

            foreach (RaccoonSprite raccoon in livingRaccoons)
            {
                raccoon.Update(gameTime, player, Gas, Content);

                if (raccoon.Health <= 0)
                {
                    deadRaccoons.Add(raccoon);
                    score++;
                    scaler++;
                }

                if (player.Hit == false)
                {
                    if (player.Bounds.CollidesWith(raccoon.Bounds) && raccoon.Direction != RaccoonDirection.Asleep)
                    {
                        player.Hit = true;
                        player.PlayerCurrentHealth--;
                        hit.Play(.3f, 0, 0);
                    }
                }
               

                foreach(GasProjectile proj in raccoon.GasProjectile)
                {
                    if (player.Hit == false)
                    {
                        if (player.Bounds.CollidesWith(proj.Bounds))
                        {
                            player.Hit = true;
                            player.PlayerCurrentHealth--;
                            hit.Play(.3f, 0, 0);
                        }
                    }
                }
            }


            foreach (TrashSpiderSprite spider in deadSpiders)
            {
                livingSpiders.Remove(spider);
            }
            deadSpiders.Clear();

            foreach (RaccoonSprite raccoon in deadRaccoons)
            {
                livingRaccoons.Remove(raccoon);
            }
            deadRaccoons.Clear();

            if (player.PlayerCurrentHealth <= 0)
            {
                MediaPlayer.Pause();
                gameState = State.GameOver;
            }

            if (gameState == State.MainMenu)
            {
                playBtn.Color = Color.White;

                if (player.Bounds.CollidesWith(playBtn.Bounds) && (Keyboard.GetState().IsKeyDown(Keys.Space) || GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed))
                {
                    playBtn.Color = Color.Red;
                    InitializeLevel0();
                }
                if (player.Bounds.CollidesWith(exitBtn.Bounds) && (Keyboard.GetState().IsKeyDown(Keys.Space) || GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed))
                {
                    Exit();
                }
            }

            
            if(gameState == State.Level0)
            {
                if(player.Position.Y < 0)
                {
                    player.Position = new Vector2(GraphicsDevice.Viewport.Width / 2, 760);
                    InitializeLevel1();
                }
            }
            
            
            if(gameState == State.LevelWaves)
            {
                if(livingSpiders.Count < enemySpawn)
                {
                    int side = rnd.Next(0, 4);
                    
                    if (side == 0)
                    {
                        livingSpiders.Add(new TrashSpiderSprite(new Vector2(rnd.Next(0, _graphics.PreferredBackBufferWidth), 0), Content));
                    }
                    else if (side == 1)
                    {
                        livingSpiders.Add(new TrashSpiderSprite(new Vector2(GraphicsDevice.Viewport.Width, rnd.Next(0, _graphics.PreferredBackBufferHeight)), Content));
                    }
                    else if (side == 2)
                    {
                        livingSpiders.Add(new TrashSpiderSprite(new Vector2(rnd.Next(0, _graphics.PreferredBackBufferWidth), GraphicsDevice.Viewport.Height), Content));
                    }
                    else if (side == 3)
                    {
                        livingSpiders.Add(new TrashSpiderSprite(new Vector2(0, rnd.Next(0, _graphics.PreferredBackBufferHeight)), Content));
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

            if(gameState == State.GameOver)
            {
                if ((Keyboard.GetState().IsKeyDown(Keys.Space) || GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed))
                {
                    livingSpiders.Clear();
                    player.Position = new Vector2((GraphicsDevice.Viewport.Width / 2) - 50, (GraphicsDevice.Viewport.Height / 2) - 30);
                    player.PlayerCurrentHealth = player.PlayerMaxHealth;
                    score = 0;
                    scaler = 0;
                    enemySpawn = 2;
                    player.ProjFireRate = .75f;
                    InitializeLevel1();
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.BurlyWood);

            // TODO: Add your drawing code here
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            if(gameState == State.MainMenu)
            {
                _spriteBatch.Draw(title, new Vector2(70, 20), Color.White);
                playBtn.Draw(gameTime, _spriteBatch);
                exitBtn.Draw(gameTime, _spriteBatch);
                _spriteBatch.DrawString(spriteFont, "             WASD/Left stick to Move \n                 Space/A to interact\n         Arrow keys/Right stick to shoot\nEsc/Back or interact with Exit button to quit", new Vector2((GraphicsDevice.Viewport.Width /2 - 225), GraphicsDevice.Viewport.Height - 125), Color.White);
                player.Draw(gameTime, _spriteBatch);
            }

            if(gameState == State.Level0)
            {
                _spriteBatch.Draw(background, new Vector2(0, 0), Color.White);
                //top of screen fences
                foreach (FenceTop fence in fenceTops)
                {
                    fence.Draw(gameTime, _spriteBatch);
                }

                //left and rigth side fences
                foreach (FenceSide fence in fenceSides)
                {
                    fence.Draw(gameTime, _spriteBatch);
                }

                foreach (TrashSpiderSprite spider in livingSpiders)
                {
                    spider.Draw(gameTime, _spriteBatch);
                }

                foreach (RaccoonSprite raccoon in livingRaccoons)
                {
                    raccoon.Draw(gameTime, _spriteBatch);
                }

                player.Draw(gameTime, _spriteBatch);

                int cans = player.PlayerCurrentHealth;
                int maxCans = player.PlayerMaxHealth;
                Vector2 currentPos = new Vector2(20, 20);
                while (maxCans > 0)
                {
                    if (maxCans >= 2)
                    {
                        _spriteBatch.Draw(emptyHealthCan, currentPos, Color.White);
                        currentPos += new Vector2(45, 0);
                        maxCans -= 2;
                    }
                }

                currentPos = new Vector2(20, 20);
                while (cans > 0)
                {
                    if (cans >= 2)
                    {
                        _spriteBatch.Draw(healthCan, currentPos, Color.White);
                        currentPos += new Vector2(45, 0);
                        cans -= 2;
                    }
                    else if (cans == 1)
                    {
                        _spriteBatch.Draw(halfHealthCan, currentPos, Color.White);
                        cans--;
                    }
                }

                //bottom of screen fences
                foreach (FenceBottom fence in fenceBottoms)
                {
                    fence.Draw(gameTime, _spriteBatch);
                }
            }


            if (gameState == State.LevelWaves)
            {
                _spriteBatch.Draw(background, new Vector2(0, 0), Color.White);
                //top of screen fences
                foreach (FenceTop fence in fenceTops)
                {
                    fence.Draw(gameTime, _spriteBatch);
                }

                //left and rigth side fences
                foreach (FenceSide fence in fenceSides)
                {
                    fence.Draw(gameTime, _spriteBatch);
                }

                foreach (TrashSpiderSprite spider in livingSpiders)
                {
                    spider.Draw(gameTime, _spriteBatch);
                }
                //_spriteBatch.DrawString(spriteFont, "Health: " + player.PlayerCurrentHealth + "/" + player.PlayerMaxHealth + "\nScore: " + score, new Vector2(20, 20), Color.White);
                player.Draw(gameTime, _spriteBatch);

                int cans = player.PlayerCurrentHealth;
                int maxCans = player.PlayerMaxHealth;
                Vector2 currentPos = new Vector2(20, 20);
                while(maxCans > 0)
                {
                    if (maxCans >= 2)
                    {
                        _spriteBatch.Draw(emptyHealthCan, currentPos, Color.White);
                        currentPos += new Vector2(45, 0);
                        maxCans -= 2;
                    }
                }

                currentPos = new Vector2(20, 20);
                while (cans > 0)
                {
                    if (cans >= 2)
                    {
                        _spriteBatch.Draw(healthCan, currentPos, Color.White);
                        currentPos += new Vector2(45, 0);
                        cans -= 2;
                    }
                    else if (cans == 1)
                    {
                        _spriteBatch.Draw(halfHealthCan, currentPos, Color.White);
                        cans--;
                    }
                }

                //bottom of screen fences
                foreach (FenceBottom fence in fenceBottoms)
                {
                    fence.Draw(gameTime, _spriteBatch);
                }
            }

            if(gameState == State.GameOver)
            {
                _spriteBatch.DrawString(spriteFont, "          GAME OVER\n   Esc/Back button to exit\nPress Space or A to restart", new Vector2((GraphicsDevice.Viewport.Width / 2) - 140, (GraphicsDevice.Viewport.Height / 2) - 30), Color.White);
            }

            var source = new Rectangle(4 * 64, 0, 64, 64);
            Vector2 Pos = new Vector2(300, 300);


            //_spriteBatch.Draw(rat, new Vector2(100, 215), null, Color.White, 0, new Vector2(0, 0), .08f, SpriteEffects.None, 0);

            _spriteBatch.End();



            base.Draw(gameTime);
        }
    }
}
