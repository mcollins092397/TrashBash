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
        Level0 = 0,
        Level1 = 1,
        Level2 = 2,
        Level3 = 3,
        GameOver = 98,
        MainMenu = 99

    }

    public class TrashBash : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private PlayerController player;

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

        private State gameState = State.MainMenu;

        public List<TrashSpiderSprite> livingSpiders = new List<TrashSpiderSprite>();
        private List<TrashSpiderSprite> deadSpiders = new List<TrashSpiderSprite>();

        public List<RaccoonSprite> livingRaccoons = new List<RaccoonSprite>();
        private List<RaccoonSprite> deadRaccoons = new List<RaccoonSprite>();

        private List<FenceTop> fenceTops = new List<FenceTop>();
        private List<FenceBottom> fenceBottoms = new List<FenceBottom>();
        private List<FenceSide> fenceSides = new List<FenceSide>();

        public GasParticleSystem Gas;

        private bool shakeViewport = false;
        private float shakeStartAngle = 150;
        private float shakeRadius = 5;
        private float shakeStart;

        private List<LevelInfo> levelList = new List<LevelInfo>();
        private int levelIndex = 0;

        //2d array representing the screen, used for A* pathfinding
        public int[,] Grid = new int[77,137];

        public TrashBash()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.IsFullScreen = true;
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
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

            levelList.Add(new LevelInfo(0, false));
            levelList.Add(new LevelInfo(1, false));
            levelList.Add(new LevelInfo(2, false));
            levelList.Add(new LevelInfo(3, false));

            base.Initialize();
        }

        private void InitializeLevelX(State level, bool clear)
        {
            //level 0 needs trash bags whenever those get done
            if(level == State.Level0)
            {
                fenceBottoms.Clear();
                fenceSides.Clear();
                fenceTops.Clear();
                livingRaccoons.Clear();
                deadRaccoons.Clear();
                livingSpiders.Clear();
                deadSpiders.Clear();
                player.PlayerProjectile.Clear();

                fenceTops.Add(new FenceTop(new Vector2(4, 0)));
                fenceTops.Add(new FenceTop(new Vector2(260, 0)));

                fenceTops.Add(new FenceTop(new Vector2(850, 0)));
                fenceTops.Add(new FenceTop(new Vector2(1106, 0)));

                foreach (FenceTop fence in fenceTops)
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
                gameState = State.Level0;
            }

            //level 1 needs trash bags placed with the spiders so the spiders can blend, maybe wont put spiders in random spots once they are added
            if(level == State.Level1)
            {
                fenceBottoms.Clear();
                fenceSides.Clear();
                fenceTops.Clear();
                livingRaccoons.Clear();
                deadRaccoons.Clear();
                livingSpiders.Clear();
                deadSpiders.Clear();
                player.PlayerProjectile.Clear();

                fenceTops.Add(new FenceTop(new Vector2(4, 0)));
                fenceTops.Add(new FenceTop(new Vector2(260, 0)));

                fenceTops.Add(new FenceTop(new Vector2(850, 0)));
                fenceTops.Add(new FenceTop(new Vector2(1106, 0)));

                foreach (FenceTop fence in fenceTops)
                {
                    fence.LoadContent(Content);
                }

                fenceBottoms.Add(new FenceBottom(new Vector2(4, 676)));
                fenceBottoms.Add(new FenceBottom(new Vector2(260, 676)));

                fenceBottoms.Add(new FenceBottom(new Vector2(850, 676)));
                fenceBottoms.Add(new FenceBottom(new Vector2(1106, 676)));



                foreach (FenceBottom fence in fenceBottoms)
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

                if(!clear)
                {
                    livingSpiders.Add(new TrashSpiderSprite(new Vector2(RandomHelper.Next(66, 1300), RandomHelper.Next(80, 688)), Content));
                    livingSpiders.Add(new TrashSpiderSprite(new Vector2(RandomHelper.Next(66, 1300), RandomHelper.Next(80, 688)), Content));
                    livingSpiders.Add(new TrashSpiderSprite(new Vector2(RandomHelper.Next(66, 1300), RandomHelper.Next(80, 688)), Content));
                    livingSpiders.Add(new TrashSpiderSprite(new Vector2(RandomHelper.Next(66, 1300), RandomHelper.Next(80, 688)), Content));
                    livingSpiders.Add(new TrashSpiderSprite(new Vector2(RandomHelper.Next(66, 1300), RandomHelper.Next(80, 688)), Content));
                }
                

                MediaPlayer.Play(bossMusic);

                gameState = State.Level1;
            }

            //level 2 will introduce a raccoon along with some trash spiders
            if (level == State.Level2)
            {
                fenceBottoms.Clear();
                fenceSides.Clear();
                fenceTops.Clear();
                livingRaccoons.Clear();
                deadRaccoons.Clear();
                livingSpiders.Clear();
                deadSpiders.Clear();
                player.PlayerProjectile.Clear();

                fenceTops.Add(new FenceTop(new Vector2(4, 0)));
                fenceTops.Add(new FenceTop(new Vector2(260, 0)));

                fenceTops.Add(new FenceTop(new Vector2(850, 0)));
                fenceTops.Add(new FenceTop(new Vector2(1106, 0)));

                foreach (FenceTop fence in fenceTops)
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

                fenceBottoms.Add(new FenceBottom(new Vector2(4, 676)));
                fenceBottoms.Add(new FenceBottom(new Vector2(260, 676)));

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


                if(!clear)
                {
                    livingSpiders.Add(new TrashSpiderSprite(new Vector2(RandomHelper.Next(66, 1300), RandomHelper.Next(80, 688)), Content));
                    livingSpiders.Add(new TrashSpiderSprite(new Vector2(RandomHelper.Next(66, 1300), RandomHelper.Next(80, 688)), Content));

                    livingRaccoons.Add(new RaccoonSprite(new Vector2((GraphicsDevice.Viewport.Width / 2) - 32, (GraphicsDevice.Viewport.Height / 2)), Content));
                }
                
                gameState = State.Level2;
            }

            if (level == State.Level3)
            {
                fenceBottoms.Clear();
                fenceSides.Clear();
                fenceTops.Clear();
                livingRaccoons.Clear();
                deadRaccoons.Clear();
                livingSpiders.Clear();
                deadSpiders.Clear();
                player.PlayerProjectile.Clear();

                fenceTops.Add(new FenceTop(new Vector2(4, 0)));
                fenceTops.Add(new FenceTop(new Vector2(260, 0)));

                fenceTops.Add(new FenceTop(new Vector2(850, 0)));
                fenceTops.Add(new FenceTop(new Vector2(1106, 0)));

                foreach (FenceTop fence in fenceTops)
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

                fenceBottoms.Add(new FenceBottom(new Vector2(4, 676)));
                fenceBottoms.Add(new FenceBottom(new Vector2(260, 676)));

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


                if (!clear)
                {
                    livingRaccoons.Add(new RaccoonSprite(new Vector2(80, 90), Content));
                    livingRaccoons.Add(new RaccoonSprite(new Vector2(80, 670), Content));
                    livingRaccoons.Add(new RaccoonSprite(new Vector2(1270, 90), Content));
                    livingRaccoons.Add(new RaccoonSprite(new Vector2(1270, 670), Content));
                }

                gameState = State.Level3;
            }

            if (level == State.GameOver)
            {
                fenceBottoms.Clear();
                fenceSides.Clear();
                fenceTops.Clear();
                livingRaccoons.Clear();
                deadRaccoons.Clear();
                livingSpiders.Clear();
                deadSpiders.Clear();
                player.PlayerProjectile.Clear();
                gameState = State.GameOver;
            }
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
            hit = Content.Load<SoundEffect>("hit");

            bossMusic = Content.Load<Song>("heavy_metal_looping");
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = .1f;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            //player update, check if player health is 0 to end game
            #region
            player.LastMove = player.Position;
            player.Update(gameTime, Content);

            if (player.PlayerCurrentHealth <= 0)
            {
                MediaPlayer.Pause();
                InitializeLevelX(State.GameOver, true);
            }

            //player.Color = Color.White;
            #endregion

            //check for fence collisions and move player back if they occur
            #region
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
            #endregion

            //spider update, collisions, and life track
            #region
            foreach (TrashSpiderSprite spider in livingSpiders)
            {
                spider.Update(gameTime, player);

                if (spider.Health <= 0)
                {
                    deadSpiders.Add(spider);
                }

                if (player.Hit == false)
                {
                    if (player.Bounds.CollidesWith(spider.Bounds))
                    {
                        player.Hit = true;
                        player.PlayerCurrentHealth--;
                        hit.Play(.3f, 0, 0);
                        shakeViewport = true;
                        shakeStart = (float)gameTime.TotalGameTime.TotalSeconds;
                    }
                }
            }

            foreach (TrashSpiderSprite spider in deadSpiders)
            {
                livingSpiders.Remove(spider);
            }
            deadSpiders.Clear();
            #endregion

            //raccoon update, collisions, and life track, gas projectile collision detection as well
            #region
            foreach (RaccoonSprite raccoon in livingRaccoons)
            {
                raccoon.Update(gameTime, player, Gas, Content);

                if (raccoon.Health <= 0)
                {
                    deadRaccoons.Add(raccoon);
                }

                if (player.Hit == false)
                {
                    if (player.Bounds.CollidesWith(raccoon.Bounds) && raccoon.Direction != RaccoonDirection.Asleep)
                    {
                        player.Hit = true;
                        player.PlayerCurrentHealth--;
                        hit.Play(.3f, 0, 0);
                        shakeViewport = true;
                        shakeStart = (float)gameTime.TotalGameTime.TotalSeconds;
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
                            shakeViewport = true;
                            shakeStart = (float)gameTime.TotalGameTime.TotalSeconds;
                        }
                    }
                }
            }

            foreach (RaccoonSprite raccoon in deadRaccoons)
            {
                livingRaccoons.Remove(raccoon);
            }
            deadRaccoons.Clear();
            #endregion

            //update logic for mainmenu
            #region
            if (gameState == State.MainMenu)
            {
                playBtn.Color = Color.White;

                if (player.Bounds.CollidesWith(playBtn.Bounds) && (Keyboard.GetState().IsKeyDown(Keys.Space) || GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed))
                {
                    playBtn.Color = Color.Red;
                    //player.Position = new Vector2((GraphicsDevice.Viewport.Width / 2) - 32, (GraphicsDevice.Viewport.Height / 2));
                    InitializeLevelX(State.Level0, false);
                }
                if (player.Bounds.CollidesWith(exitBtn.Bounds) && (Keyboard.GetState().IsKeyDown(Keys.Space) || GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed))
                {
                    Exit();
                }
            }
            #endregion

            //update logic for level 0
            #region
            if (gameState == State.Level0)
            {
                if(player.Position.Y < 0)
                {
                    player.Position = new Vector2(GraphicsDevice.Viewport.Width / 2, 750);
                    levelList[levelIndex].cleared = true;
                    levelIndex++;
                    if(levelIndex < levelList.Count)
                    {
                        InitializeLevelX((State)levelList[levelIndex].levelNum, levelList[levelIndex].cleared);
                    }
                }
            }
            #endregion

            //update logic for level 1
            #region
            if (gameState == State.Level1)
            {
                if (player.Position.Y < 0)
                {
                    player.Position = new Vector2(GraphicsDevice.Viewport.Width / 2, 760);
                    levelIndex++;
                    if (levelIndex < levelList.Count)
                    {
                        levelList[levelIndex-1].cleared = true;
                        InitializeLevelX((State)levelList[levelIndex].levelNum, levelList[levelIndex].cleared);
                    }
                }

                if (player.Position.Y > 768)
                {
                    player.Position = new Vector2(GraphicsDevice.Viewport.Width / 2, 10);
                    levelIndex--;
                    if (levelIndex <= levelList.Count)
                    {
                        InitializeLevelX((State)levelList[levelIndex].levelNum, levelList[levelIndex].cleared);
                    }
                }
            }
            #endregion

            //update logic for level 2
            #region
            if (gameState == State.Level2)
            {
                if (player.Position.Y < 0)
                {
                    player.Position = new Vector2(GraphicsDevice.Viewport.Width / 2, 760);
                    levelIndex++;
                    if (levelIndex < levelList.Count)
                    {
                        levelList[levelIndex-1].cleared = true;
                        InitializeLevelX((State)levelList[levelIndex].levelNum, levelList[levelIndex].cleared);
                    }
                }
                if (player.Position.Y > 768)
                {
                    player.Position = new Vector2(GraphicsDevice.Viewport.Width / 2, 10);
                    levelIndex--;
                    InitializeLevelX((State)levelList[levelIndex].levelNum, levelList[levelIndex].cleared);
                }
            }
            #endregion

            //update logic for level 3
            #region
            if (gameState == State.Level3)
            {
                if (player.Position.Y < 0)
                {
                    //player.Position = new Vector2(GraphicsDevice.Viewport.Width / 2, 760);
                    levelIndex++;
                    //if (levelIndex < levelList.Count)
                    //{
                        levelList[levelIndex-1].cleared = true;
                        //InitializeLevelX((State)levelList[levelIndex].levelNum, levelList[levelIndex].cleared);
                        InitializeLevelX(State.GameOver, false);
                        gameState = State.GameOver;
                    //}
                }
                if (player.Position.Y > 768)
                {
                    player.Position = new Vector2(GraphicsDevice.Viewport.Width / 2, 10);
                    levelIndex--;
                    InitializeLevelX((State)levelList[levelIndex].levelNum, levelList[levelIndex].cleared);
                }
            }
            #endregion

            //update logic for game over screen
            #region
            if (gameState == State.GameOver)
            {
                if ((Keyboard.GetState().IsKeyDown(Keys.Space) || GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed))
                {
                    livingSpiders.Clear();
                    player.Position = new Vector2((GraphicsDevice.Viewport.Width / 2) - 50, (GraphicsDevice.Viewport.Height / 2) - 30);
                    player.PlayerCurrentHealth = player.PlayerMaxHealth;
                    player.ProjFireRate = .75f;
                    levelIndex = 0;

                    for(int i = 0; i < levelList.Count; i++)
                    {
                        levelList[i].cleared = false;
                    }

                    InitializeLevelX(State.Level0, false);
                }
            }
            #endregion

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.BurlyWood);

            Vector2 offset = new Vector2(0, 0);
            
            if(shakeViewport)
            {
                offset = new Vector2((float)(Math.Sin(shakeStartAngle) * shakeRadius), (float)(Math.Cos(shakeStartAngle) * shakeRadius));
                shakeRadius -= 0.25f;
                shakeStartAngle += (150 + RandomHelper.Next(60));

                if(gameTime.TotalGameTime.TotalSeconds - shakeStart > 2 || shakeRadius <= 0)
                {
                    shakeViewport = false;
                    shakeRadius = 5;
                    shakeStartAngle = 150;

                }
            }

            // TODO: Add your drawing code here
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(offset.X, offset.Y, 0));

            if(gameState == State.MainMenu)
            {
                _spriteBatch.Draw(title, new Vector2(70, 20), Color.White);
                playBtn.Draw(gameTime, _spriteBatch);
                exitBtn.Draw(gameTime, _spriteBatch);
                _spriteBatch.DrawString(spriteFont, "             WASD/Left stick to Move \n                 Space/A to interact\n         Arrow keys/Right stick to shoot\nEsc/Back or interact with Exit button to quit", new Vector2((GraphicsDevice.Viewport.Width /2 - 225), GraphicsDevice.Viewport.Height - 125), Color.White);
                player.Draw(gameTime, _spriteBatch);
            }

            if(gameState != State.MainMenu && gameState != State.GameOver)
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
