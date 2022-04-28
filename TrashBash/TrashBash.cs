using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;

namespace TrashBash
{
    //enum representing level state. One for each level with an int reference number
    public enum State
    {
        Level0 = 0,
        Level1 = 1,
        Level2 = 2,
        Level3 = 3,
        Level4 = 4,
        Level5 = 5,
        GameOver = 98,
        MainMenu = 99

    }

    public class TrashBash : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;


        //the player object takes in their input and handles player projectiles
        private PlayerController player;

        //title screen title image
        private Texture2D title;

        //the health can images for the hud
        private Texture2D healthCan;
        private Texture2D halfHealthCan;
        private Texture2D emptyHealthCan;

        //standard cracked dirt backgroudn
        private Texture2D background;

        //the hit sound effect for the player
        private SoundEffect hit;

        //gate open sound effect played at end of level
        private SoundEffect gateOpen;
        private bool gateSoundPlayed = false;

        //some background music
        private Song bossMusic;

        //buttons for the main menu
        private PlayBtn playBtn;
        private ExitBtn exitBtn;

        //spritefont used in the main menu controls explanation and the game over screen
        private SpriteFont spriteFont;

        //gamestate is created and intialized to the main menu, it will be changed as the game is played
        private State gameState = State.MainMenu;

        //a list of living and dead trash spider sprites. Once they are killed they are added to the dead list and then removed
        public List<TrashSpiderSprite> livingSpiders = new List<TrashSpiderSprite>();
        private List<TrashSpiderSprite> deadSpiders = new List<TrashSpiderSprite>();

        //a list of trash bags
        public List<TrashBagSprite> trashBags = new List<TrashBagSprite>();
        public List<TrashBagSprite> deadTrashBags = new List<TrashBagSprite>();

        //a list of living and dead raccoon sprites. Once they are killed they are added to the dead list and then removed
        public List<RaccoonSprite> livingRaccoons = new List<RaccoonSprite>();
        private List<RaccoonSprite> deadRaccoons = new List<RaccoonSprite>();

        //a list of fence tops, bottoms, and sides that are present in the current level. List is cleared and re-populated when lvl is changed
        private List<FenceTop> fenceTops = new List<FenceTop>();
        private List<FenceBottom> fenceBottoms = new List<FenceBottom>();
        private List<FenceSide> fenceSides = new List<FenceSide>();

        //list of health pickups
        public List<HealthPickup> healthPickups = new List<HealthPickup>();
        public List<HealthPickup> hpPickedUp = new List<HealthPickup>();

        //list of gates
        private List<GateTop> gateTops = new List<GateTop>();

        //list of wall blocks
        private List<Wall> walls = new List<Wall>();

        //the gas particle system for the gas projectiles, can probably be moved to the raccoon controller
        public GasParticleSystem Gas;

        //variabes used to shake the viewport when the player takes damage from any source
        private bool shakeViewport = false;
        private float shakeStartAngle = 150;
        private float shakeRadius = 5;
        private float shakeStart;

        //a list of all levels in the current stage. The final version of the game will have a stage consisting of a set number of levels 
        //after this set ammount the player will fight the stage boss and move to the next stage. 
        private List<LevelInfo> levelList = new List<LevelInfo>();
        private int levelIndex = 0;

        //2d array representing the screen, used for A* pathfinding
        public int[,] Grid = new int[77,137];
        AStarPathfinder pathfinder;

        private Tilemap _tilemap;

        public TrashBash()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.IsFullScreen = false;
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
            _graphics.PreferredBackBufferWidth = 1366;
            _graphics.PreferredBackBufferHeight = 768;
            _graphics.ApplyChanges();
        }

        /// <summary>
        /// Initilization logic
        /// </summary>
        protected override void Initialize()
        {
            //initialize the player object and have their starting location default to center screen 
            player = new PlayerController() { Position = new Vector2((GraphicsDevice.Viewport.Width / 2) -32, (GraphicsDevice.Viewport.Height / 2)) };

            //initialize the buttons for the main menu
            playBtn = new PlayBtn(new Vector2((GraphicsDevice.Viewport.Width / 4) - 80, GraphicsDevice.Viewport.Height / 2));
            exitBtn = new ExitBtn(new Vector2((float)(GraphicsDevice.Viewport.Width * 0.75) - 80, GraphicsDevice.Viewport.Height / 2));

            //create the gas particle system and set the max number of explosions to 40 then add it to the game components
            Gas = new GasParticleSystem(this, 40);
            Components.Add(Gas);

            //initialize the level list adding the first 3 levels so the player can learn the game mechanics and the enemy types
            levelList.Add(new LevelInfo(0, false, false, false, false));
            levelList.Add(new LevelInfo(1, false, false, false, false));
            levelList.Add(new LevelInfo(2, false, false, false, false));
            levelList.Add(new LevelInfo(3, false, false, false, false));
            levelList.Add(new LevelInfo(4, false, false, false, false));
            levelList.Add(new LevelInfo(5, false, false, false, false));
            //then begin adding random levels until the level count has been filled,
            //need to still add a level count variable to adjust based on what stage the player is in
            levelList.Add(new LevelInfo(RandomHelper.Next(1, 5), false, false, false, false));
            levelList.Add(new LevelInfo(RandomHelper.Next(1, 5), false, false, false, false));
            levelList.Add(new LevelInfo(RandomHelper.Next(1, 5), false, false, false, false));
            levelList.Add(new LevelInfo(RandomHelper.Next(1, 5), false, false, false, false));
            levelList.Add(new LevelInfo(RandomHelper.Next(1, 5), false, false, false, false));
            levelList.Add(new LevelInfo(RandomHelper.Next(1, 5), false, false, false, false));
            levelList.Add(new LevelInfo(RandomHelper.Next(1, 5), false, false, false, false));

            //select a random level in the list of levels and set it as the stages item room/shop
            //levelList[RandomHelper.Next(1, levelList.Count)].ItemRoom = true;
            //levelList[RandomHelper.Next(1, levelList.Count)].Shop = true;

            //initialize pathfinder
            pathfinder = new AStarPathfinder(Grid);

            _tilemap = new Tilemap("map.txt");

            base.Initialize();
        }

        /// <summary>
        /// Initialize a level based on the given level number (ie if given number 1 it will load all assets and enemies for level 1) 
        /// </summary>
        /// <param name="level">The level to load content for</param>
        /// <param name="clear">If the level has been cleared, if true no enemies are spawned</param>
        /// <param name="shop">If a shop appears in the level</param>
        /// <param name="item">If an item room appears in the level</param>
        private void InitializeLevelX(State level, bool clear, bool shop, bool item, bool loaded)
        {
            //clear all objects from previous rooms
            foreach (RaccoonSprite raccoon in livingRaccoons)
            {
                foreach (GasProjectile gas in raccoon.GasProjectileActive)
                {
                    gas.ClearGas();
                }
            }
            Array.Clear(Grid, 0, Grid.Length);
            fenceBottoms.Clear();
            fenceSides.Clear();
            fenceTops.Clear();
            livingRaccoons.Clear();
            deadRaccoons.Clear();
            livingSpiders.Clear();
            deadSpiders.Clear();
            player.PlayerProjectile.Clear();
            gateTops.Clear();
            deadTrashBags.Clear();
            walls.Clear();

            //level 0 needs trash bags whenever those get done
            if (level == State.Level0)
            #region
            {
                fenceTops.Add(new FenceTop(new Vector2(4, 0), Content));
                fenceTops.Add(new FenceTop(new Vector2(260, 0), Content));

                fenceTops.Add(new FenceTop(new Vector2(850, 0), Content));
                fenceTops.Add(new FenceTop(new Vector2(1106, 0), Content));

                fenceBottoms.Add(new FenceBottom(new Vector2(4, 676), Content));
                fenceBottoms.Add(new FenceBottom(new Vector2(260, 676), Content));
                fenceBottoms.Add(new FenceBottom(new Vector2(516, 676), Content));
                fenceBottoms.Add(new FenceBottom(new Vector2(772, 676), Content));
                fenceBottoms.Add(new FenceBottom(new Vector2(850, 676), Content));
                fenceBottoms.Add(new FenceBottom(new Vector2(1106, 676), Content));

                fenceSides.Add(new FenceSide(new Vector2(0, 4), Content));
                fenceSides.Add(new FenceSide(new Vector2(0, 260), Content));
                fenceSides.Add(new FenceSide(new Vector2(0, 516), Content));

                //fenceSides.Add(new FenceSide(new Vector2(700, 260)));

                fenceSides.Add(new FenceSide(new Vector2(1354, 4), Content));
                fenceSides.Add(new FenceSide(new Vector2(1354, 260), Content));
                fenceSides.Add(new FenceSide(new Vector2(1354, 515), Content));

                if(!loaded)
                {
                    trashBags.Add(new TrashBagSprite(new Vector2(1210, 605), Content, ((float)State.Level0)));

                    trashBags.Add(new TrashBagSprite(new Vector2(25, 360), Content, ((float)State.Level0)));
                    trashBags.Add(new TrashBagSprite(new Vector2(25, 390), Content, ((float)State.Level0)));
                    trashBags.Add(new TrashBagSprite(new Vector2(45, 375), Content, ((float)State.Level0)));
                    trashBags.Add(new TrashBagSprite(new Vector2(25, 420), Content, ((float)State.Level0)));
                    trashBags.Add(new TrashBagSprite(new Vector2(25, 450), Content, ((float)State.Level0)));
                    trashBags.Add(new TrashBagSprite(new Vector2(120, 245), Content, ((float)State.Level0)));
                    trashBags.Add(new TrashBagSprite(new Vector2(150, 220), Content, ((float)State.Level0)));
                    trashBags.Add(new TrashBagSprite(new Vector2(143, 240), Content, ((float)State.Level0)));

                    trashBags.Add(new TrashBagSprite(new Vector2(1185, 665), Content, ((float)State.Level0)));
                    trashBags.Add(new TrashBagSprite(new Vector2(1185, 635), Content, ((float)State.Level0)));
                    trashBags.Add(new TrashBagSprite(new Vector2(1155, 665), Content, ((float)State.Level0)));

                    trashBags.Add(new TrashBagSprite(new Vector2(1310, 105), Content, ((float)State.Level0)));
                    trashBags.Add(new TrashBagSprite(new Vector2(1310, 75), Content, ((float)State.Level0)));
                    trashBags.Add(new TrashBagSprite(new Vector2(1285, 75), Content, ((float)State.Level0)));
                }

                healthPickups.Add(new HealthPickup(new Vector2((GraphicsDevice.Viewport.Width / 2) - 32, (GraphicsDevice.Viewport.Height / 2)), Content, 0));

                //walls
                #region
                walls.Add(new Wall(new Vector2(1320, 500), Content, 10));
                walls.Add(new Wall(new Vector2(1320, 530), Content, 3));
                walls.Add(new Wall(new Vector2(1320, 560), Content, 14));
                walls.Add(new Wall(new Vector2(1320, 590), Content, 14));
                walls.Add(new Wall(new Vector2(1320, 620), Content, 14));
                walls.Add(new Wall(new Vector2(1320, 650), Content, 14));
                walls.Add(new Wall(new Vector2(1320, 680), Content, 14));
                walls.Add(new Wall(new Vector2(1320, 710), Content, 4));

                
                walls.Add(new Wall(new Vector2(1290, 560), Content, 0));
                walls.Add(new Wall(new Vector2(1290, 590), Content, 15));
                walls.Add(new Wall(new Vector2(1290, 620), Content, 13));
                walls.Add(new Wall(new Vector2(1290, 650), Content, 13));
                walls.Add(new Wall(new Vector2(1290, 680), Content, 13));
                walls.Add(new Wall(new Vector2(1290, 710), Content, 17));

                
                walls.Add(new Wall(new Vector2(1260, 620), Content, 0));
                walls.Add(new Wall(new Vector2(1260, 650), Content, 13));
                walls.Add(new Wall(new Vector2(1260, 680), Content, 13));
                walls.Add(new Wall(new Vector2(1260, 710), Content, 17));

                
                walls.Add(new Wall(new Vector2(1230, 650), Content, 0));
                walls.Add(new Wall(new Vector2(1230, 680), Content, 15));
                walls.Add(new Wall(new Vector2(1230, 710), Content, 17));




                walls.Add(new Wall(new Vector2(15, 140), Content, 0));
                walls.Add(new Wall(new Vector2(15, 170), Content, 15));
                walls.Add(new Wall(new Vector2(15, 200), Content, 15));
                walls.Add(new Wall(new Vector2(15, 230), Content, 15));
                walls.Add(new Wall(new Vector2(15, 260), Content, 15));
                walls.Add(new Wall(new Vector2(15, 290), Content, 15));
                walls.Add(new Wall(new Vector2(15, 320), Content, 15));
                walls.Add(new Wall(new Vector2(15, 350), Content, 15));
                walls.Add(new Wall(new Vector2(15, 380), Content, 3));
                walls.Add(new Wall(new Vector2(15, 410), Content, 3));
                walls.Add(new Wall(new Vector2(15, 440), Content, 3));
                walls.Add(new Wall(new Vector2(15, 470), Content, 3));
                walls.Add(new Wall(new Vector2(15, 500), Content, 3));
                walls.Add(new Wall(new Vector2(15, 530), Content, 3));
                walls.Add(new Wall(new Vector2(15, 560), Content, 15));
                walls.Add(new Wall(new Vector2(15, 590), Content, 15));
                walls.Add(new Wall(new Vector2(15, 620), Content, 15));
                walls.Add(new Wall(new Vector2(15, 650), Content, 15));
                walls.Add(new Wall(new Vector2(15, 680), Content, 15));
                walls.Add(new Wall(new Vector2(15, 710), Content, 6));

                walls.Add(new Wall(new Vector2(45, 560), Content, 2));
                walls.Add(new Wall(new Vector2(45, 590), Content, 14));
                walls.Add(new Wall(new Vector2(45, 620), Content, 13));
                walls.Add(new Wall(new Vector2(45, 650), Content, 13));
                walls.Add(new Wall(new Vector2(45, 680), Content, 4));

                walls.Add(new Wall(new Vector2(75, 620), Content, 2));
                walls.Add(new Wall(new Vector2(75, 650), Content, 4));

                walls.Add(new Wall(new Vector2(45, 110), Content, 0));
                walls.Add(new Wall(new Vector2(45, 140), Content, 13));
                walls.Add(new Wall(new Vector2(45, 170), Content, 13));
                walls.Add(new Wall(new Vector2(45, 200), Content, 13));
                walls.Add(new Wall(new Vector2(45, 230), Content, 13));
                walls.Add(new Wall(new Vector2(45, 260), Content, 13));
                walls.Add(new Wall(new Vector2(45, 290), Content, 13));
                walls.Add(new Wall(new Vector2(45, 320), Content, 13));
                walls.Add(new Wall(new Vector2(45, 350), Content, 4));

                walls.Add(new Wall(new Vector2(75, 110), Content, 16));
                walls.Add(new Wall(new Vector2(75, 140), Content, 13));
                walls.Add(new Wall(new Vector2(75, 170), Content, 13));
                walls.Add(new Wall(new Vector2(75, 200), Content, 13));
                walls.Add(new Wall(new Vector2(75, 230), Content, 13));
                walls.Add(new Wall(new Vector2(75, 260), Content, 13));
                walls.Add(new Wall(new Vector2(75, 290), Content, 13));
                walls.Add(new Wall(new Vector2(75, 320), Content, 4));

                walls.Add(new Wall(new Vector2(105, 110), Content, 16));
                walls.Add(new Wall(new Vector2(105, 140), Content, 13));
                walls.Add(new Wall(new Vector2(105, 170), Content, 13));
                walls.Add(new Wall(new Vector2(105, 200), Content, 13));
                walls.Add(new Wall(new Vector2(105, 230), Content, 13));
                walls.Add(new Wall(new Vector2(105, 260), Content, 14));
                walls.Add(new Wall(new Vector2(105, 290), Content, 4));

                walls.Add(new Wall(new Vector2(135, 110), Content, 16));
                walls.Add(new Wall(new Vector2(135, 140), Content, 13));
                walls.Add(new Wall(new Vector2(135, 170), Content, 13));
                walls.Add(new Wall(new Vector2(135, 200), Content, 13));
                walls.Add(new Wall(new Vector2(135, 230), Content, 4));

                walls.Add(new Wall(new Vector2(165, 110), Content, 16));
                walls.Add(new Wall(new Vector2(165, 140), Content, 13));
                walls.Add(new Wall(new Vector2(165, 170), Content, 13));
                walls.Add(new Wall(new Vector2(165, 200), Content, 17));

                walls.Add(new Wall(new Vector2(195, 110), Content, 16));
                walls.Add(new Wall(new Vector2(195, 140), Content, 13));
                walls.Add(new Wall(new Vector2(195, 170), Content, 13));
                walls.Add(new Wall(new Vector2(195, 200), Content, 17));

                walls.Add(new Wall(new Vector2(215, 110), Content, 16));
                walls.Add(new Wall(new Vector2(215, 140), Content, 13));
                walls.Add(new Wall(new Vector2(215, 170), Content, 13));
                walls.Add(new Wall(new Vector2(215, 200), Content, 17));

                walls.Add(new Wall(new Vector2(215, 110), Content, 16));
                walls.Add(new Wall(new Vector2(215, 140), Content, 13));
                walls.Add(new Wall(new Vector2(215, 170), Content, 13));
                walls.Add(new Wall(new Vector2(215, 200), Content, 17));

                walls.Add(new Wall(new Vector2(245, 110), Content, 16));
                walls.Add(new Wall(new Vector2(245, 140), Content, 13));
                walls.Add(new Wall(new Vector2(245, 170), Content, 13));
                walls.Add(new Wall(new Vector2(245, 200), Content, 4));

                walls.Add(new Wall(new Vector2(275, 110), Content, 16));
                walls.Add(new Wall(new Vector2(275, 140), Content, 14));
                walls.Add(new Wall(new Vector2(275, 170), Content, 4));
                #endregion

                gameState = State.Level0;
            }
            #endregion

            //level 1 needs trash bags placed with the spiders so the spiders can blend, maybe wont put spiders in random spots once they are added
            if (level == State.Level1)
            #region
            {
                fenceTops.Add(new FenceTop(new Vector2(4, 0), Content));
                fenceTops.Add(new FenceTop(new Vector2(260, 0), Content));

                if(!clear)
                {
                    gateTops.Add(new GateTop(new Vector2(516, 0), Content));
                    gateSoundPlayed = false;
                }

                fenceTops.Add(new FenceTop(new Vector2(850, 0), Content));
                fenceTops.Add(new FenceTop(new Vector2(1106, 0), Content));

                fenceBottoms.Add(new FenceBottom(new Vector2(4, 676), Content));
                fenceBottoms.Add(new FenceBottom(new Vector2(260, 676), Content));

                fenceBottoms.Add(new FenceBottom(new Vector2(850, 676), Content));
                fenceBottoms.Add(new FenceBottom(new Vector2(1106, 676), Content));

                fenceSides.Add(new FenceSide(new Vector2(0, 4), Content));
                if (!item)
                {
                    fenceSides.Add(new FenceSide(new Vector2(0, 260), Content));
                }
                fenceSides.Add(new FenceSide(new Vector2(0, 516), Content));

                fenceSides.Add(new FenceSide(new Vector2(1354, 4), Content));
                if (!shop)
                {
                    fenceSides.Add(new FenceSide(new Vector2(1354, 260), Content));
                }
                fenceSides.Add(new FenceSide(new Vector2(1354, 515), Content));

                if(!clear)
                {
                    livingSpiders.Add(new TrashSpiderSprite(new Vector2(RandomHelper.Next(66, 1300), RandomHelper.Next(80, 588)), Content, pathfinder));
                    livingSpiders.Add(new TrashSpiderSprite(new Vector2(RandomHelper.Next(66, 1300), RandomHelper.Next(80, 588)), Content, pathfinder));
                    livingSpiders.Add(new TrashSpiderSprite(new Vector2(RandomHelper.Next(66, 1300), RandomHelper.Next(80, 588)), Content, pathfinder));
                    livingSpiders.Add(new TrashSpiderSprite(new Vector2(RandomHelper.Next(66, 1300), RandomHelper.Next(80, 588)), Content, pathfinder));
                    livingSpiders.Add(new TrashSpiderSprite(new Vector2(RandomHelper.Next(66, 1300), RandomHelper.Next(80, 588)), Content, pathfinder));
                }

                if(!loaded)
                {
                    trashBags.Add(new TrashBagSprite(new Vector2(1200, 600), Content, (float)State.Level1));
                    trashBags.Add(new TrashBagSprite(new Vector2(1100, 600), Content, (float)State.Level1));
                    trashBags.Add(new TrashBagSprite(new Vector2(1000, 600), Content, (float)State.Level1));
                    trashBags.Add(new TrashBagSprite(new Vector2(900, 600), Content, (float)State.Level1));
                    trashBags.Add(new TrashBagSprite(new Vector2(800, 600), Content, (float)State.Level1));
                    trashBags.Add(new TrashBagSprite(new Vector2(700, 600), Content, (float)State.Level1));
                    trashBags.Add(new TrashBagSprite(new Vector2(600, 600), Content, (float)State.Level1));
                    trashBags.Add(new TrashBagSprite(new Vector2(500, 600), Content, (float)State.Level1));
                    trashBags.Add(new TrashBagSprite(new Vector2(400, 600), Content, (float)State.Level1));
                }


                MediaPlayer.Play(bossMusic);

                gameState = State.Level1;
            }
#endregion

            //level 2 will introduce a raccoon along with some trash spiders
            if (level == State.Level2)
            #region
            {
                fenceTops.Add(new FenceTop(new Vector2(4, 0), Content));
                fenceTops.Add(new FenceTop(new Vector2(260, 0), Content));

                if (!clear)
                {
                    gateTops.Add(new GateTop(new Vector2(516, 0), Content));
                    gateSoundPlayed = false;
                }

                fenceTops.Add(new FenceTop(new Vector2(850, 0), Content));
                fenceTops.Add(new FenceTop(new Vector2(1106, 0), Content));

                fenceBottoms.Add(new FenceBottom(new Vector2(4, 676), Content));
                fenceBottoms.Add(new FenceBottom(new Vector2(260, 676), Content));

                fenceBottoms.Add(new FenceBottom(new Vector2(850, 676), Content));
                fenceBottoms.Add(new FenceBottom(new Vector2(1106, 676), Content));

                fenceSides.Add(new FenceSide(new Vector2(0, 4), Content));

                if (!item)
                {
                    fenceSides.Add(new FenceSide(new Vector2(0, 260), Content));
                }

                fenceSides.Add(new FenceSide(new Vector2(0, 516), Content));

                fenceSides.Add(new FenceSide(new Vector2(1354, 4), Content));

                if(!shop)
                {
                    fenceSides.Add(new FenceSide(new Vector2(1354, 260), Content));
                }

                fenceSides.Add(new FenceSide(new Vector2(1354, 515), Content));


                if(!clear)
                {
                    livingSpiders.Add(new TrashSpiderSprite(new Vector2(RandomHelper.Next(66, 1300), RandomHelper.Next(80, 588)), Content, pathfinder));
                    livingSpiders.Add(new TrashSpiderSprite(new Vector2(RandomHelper.Next(66, 1300), RandomHelper.Next(80, 588)), Content, pathfinder));

                    livingRaccoons.Add(new RaccoonSprite(new Vector2((GraphicsDevice.Viewport.Width / 2) - 32, (GraphicsDevice.Viewport.Height / 2)), Content));
                }
                
                gameState = State.Level2;
            }
#endregion

            //level 3 will have the player fight 4 raccoons at once
            if (level == State.Level3)
            #region
            {
                fenceTops.Add(new FenceTop(new Vector2(4, 0), Content));
                fenceTops.Add(new FenceTop(new Vector2(260, 0), Content));

                if (!clear)
                {
                    gateTops.Add(new GateTop(new Vector2(516, 0), Content));
                    gateSoundPlayed = false;
                }

                fenceTops.Add(new FenceTop(new Vector2(850, 0), Content));
                fenceTops.Add(new FenceTop(new Vector2(1106, 0), Content));

                fenceBottoms.Add(new FenceBottom(new Vector2(4, 676), Content));
                fenceBottoms.Add(new FenceBottom(new Vector2(260, 676), Content));

                fenceBottoms.Add(new FenceBottom(new Vector2(850, 676), Content));
                fenceBottoms.Add(new FenceBottom(new Vector2(1106, 676), Content));

                fenceSides.Add(new FenceSide(new Vector2(0, 4), Content));
                if (!item)
                {
                    fenceSides.Add(new FenceSide(new Vector2(0, 260), Content));
                }
                fenceSides.Add(new FenceSide(new Vector2(0, 516), Content));

                fenceSides.Add(new FenceSide(new Vector2(1354, 4), Content));
                if (!shop)
                {
                    fenceSides.Add(new FenceSide(new Vector2(1354, 260), Content));
                }
                fenceSides.Add(new FenceSide(new Vector2(1354, 515), Content));


                if (!clear)
                {
                    livingRaccoons.Add(new RaccoonSprite(new Vector2(80, 90), Content));
                    livingRaccoons.Add(new RaccoonSprite(new Vector2(80, 670), Content));
                    livingRaccoons.Add(new RaccoonSprite(new Vector2(1270, 90), Content));
                    livingRaccoons.Add(new RaccoonSprite(new Vector2(1270, 670), Content));
                }

                gameState = State.Level3;
            }
            #endregion

            //level 4 is a ton of trash spiders placed randomly around the room
            if (level == State.Level4)
            #region
            {
                fenceTops.Add(new FenceTop(new Vector2(4, 0), Content));
                fenceTops.Add(new FenceTop(new Vector2(260, 0), Content));

                if (!clear)
                {
                    gateTops.Add(new GateTop(new Vector2(516, 0), Content));
                    gateSoundPlayed = false;
                }

                fenceTops.Add(new FenceTop(new Vector2(850, 0), Content));
                fenceTops.Add(new FenceTop(new Vector2(1106, 0), Content));

                fenceBottoms.Add(new FenceBottom(new Vector2(4, 676), Content));
                fenceBottoms.Add(new FenceBottom(new Vector2(260, 676), Content));

                fenceBottoms.Add(new FenceBottom(new Vector2(850, 676), Content));
                fenceBottoms.Add(new FenceBottom(new Vector2(1106, 676), Content));


                fenceSides.Add(new FenceSide(new Vector2(0, 4), Content));
                if (!item)
                {
                    fenceSides.Add(new FenceSide(new Vector2(0, 260), Content));
                }
                fenceSides.Add(new FenceSide(new Vector2(0, 516), Content));

                fenceSides.Add(new FenceSide(new Vector2(1354, 4), Content));
                if (!shop)
                {
                    fenceSides.Add(new FenceSide(new Vector2(1354, 260), Content));
                }
                fenceSides.Add(new FenceSide(new Vector2(1354, 515), Content));

                if (!clear)
                {
                    livingSpiders.Add(new TrashSpiderSprite(new Vector2(RandomHelper.Next(66, 1300), RandomHelper.Next(80, 588)), Content, pathfinder));
                    livingSpiders.Add(new TrashSpiderSprite(new Vector2(RandomHelper.Next(66, 1300), RandomHelper.Next(80, 588)), Content, pathfinder));
                    livingSpiders.Add(new TrashSpiderSprite(new Vector2(RandomHelper.Next(66, 1300), RandomHelper.Next(80, 588)), Content, pathfinder));
                    livingSpiders.Add(new TrashSpiderSprite(new Vector2(RandomHelper.Next(66, 1300), RandomHelper.Next(80, 588)), Content, pathfinder));
                    livingSpiders.Add(new TrashSpiderSprite(new Vector2(RandomHelper.Next(66, 1300), RandomHelper.Next(80, 588)), Content, pathfinder));
                    livingSpiders.Add(new TrashSpiderSprite(new Vector2(RandomHelper.Next(66, 1300), RandomHelper.Next(80, 588)), Content, pathfinder));
                    livingSpiders.Add(new TrashSpiderSprite(new Vector2(RandomHelper.Next(66, 1300), RandomHelper.Next(80, 588)), Content, pathfinder));
                    livingSpiders.Add(new TrashSpiderSprite(new Vector2(RandomHelper.Next(66, 1300), RandomHelper.Next(80, 588)), Content, pathfinder));
                    livingSpiders.Add(new TrashSpiderSprite(new Vector2(RandomHelper.Next(66, 1300), RandomHelper.Next(80, 588)), Content, pathfinder));
                    livingSpiders.Add(new TrashSpiderSprite(new Vector2(RandomHelper.Next(66, 1300), RandomHelper.Next(80, 588)), Content, pathfinder));
                }

                gameState = State.Level4;
            }
            #endregion

            //level 5 uses a tilemap
            if (level == State.Level5)
            #region
            {
                fenceTops.Add(new FenceTop(new Vector2(4, 0), Content));
                fenceTops.Add(new FenceTop(new Vector2(260, 0), Content));

                if (!clear)
                {
                    gateTops.Add(new GateTop(new Vector2(516, 0), Content));
                    gateSoundPlayed = false;
                }

                fenceTops.Add(new FenceTop(new Vector2(850, 0), Content));
                fenceTops.Add(new FenceTop(new Vector2(1106, 0), Content));

                fenceBottoms.Add(new FenceBottom(new Vector2(4, 676), Content));
                fenceBottoms.Add(new FenceBottom(new Vector2(260, 676), Content));

                fenceBottoms.Add(new FenceBottom(new Vector2(850, 676), Content));
                fenceBottoms.Add(new FenceBottom(new Vector2(1106, 676), Content));


                fenceSides.Add(new FenceSide(new Vector2(0, 4), Content));
                if (!item)
                {
                    fenceSides.Add(new FenceSide(new Vector2(0, 260), Content));
                }
                fenceSides.Add(new FenceSide(new Vector2(0, 516), Content));

                fenceSides.Add(new FenceSide(new Vector2(1354, 4), Content));
                if (!shop)
                {
                    fenceSides.Add(new FenceSide(new Vector2(1354, 260), Content));
                }
                fenceSides.Add(new FenceSide(new Vector2(1354, 515), Content));

                if (!clear)
                {
                    livingSpiders.Add(new TrashSpiderSprite(new Vector2(RandomHelper.Next(66, 1300), RandomHelper.Next(80, 588)), Content, pathfinder));
                    livingSpiders.Add(new TrashSpiderSprite(new Vector2(RandomHelper.Next(66, 1300), RandomHelper.Next(80, 588)), Content, pathfinder));
                    livingSpiders.Add(new TrashSpiderSprite(new Vector2(RandomHelper.Next(66, 1300), RandomHelper.Next(80, 588)), Content, pathfinder));
                    livingSpiders.Add(new TrashSpiderSprite(new Vector2(RandomHelper.Next(66, 1300), RandomHelper.Next(80, 588)), Content, pathfinder));
                    livingSpiders.Add(new TrashSpiderSprite(new Vector2(RandomHelper.Next(66, 1300), RandomHelper.Next(80, 588)), Content, pathfinder));
                }

                gameState = State.Level5;
            }
            #endregion

            //the game over screen presents the user the prompt to leave or reset the game
            if (level == State.GameOver)
            #region
            {
                gameState = State.GameOver;
            }
            #endregion

            //these loops populate the grid used in astar search
            #region
            foreach (Wall wall in walls)
            {
                for (int y = (int)wall.Bounds.Y / 10; y < (int)((wall.Bounds.Y + wall.Bounds.Height) / 10); y++)
                {
                    for (int x = (int)wall.Bounds.X / 10; x < (int)((wall.Bounds.X + wall.Bounds.Width) / 10); x++)
                    {
                        Grid[y, x] += 10;
                    }
                }
            }

            foreach (FenceTop fence in fenceTops)
            {
                for (int y = (int)fence.Bounds.Y / 10; y < (int)((fence.Bounds.Y + fence.Bounds.Height) / 10); y++)
                {
                    for (int x = (int)fence.Bounds.X / 10; x < (int)((fence.Bounds.X + fence.Bounds.Width) / 10); x++)
                    {
                        Grid[y, x] += 10;
                    }
                }
            }

            foreach (FenceSide fence in fenceSides)
            {
                for (int y = (int)fence.Bounds.Y / 10; y < (int)((fence.Bounds.Y + fence.Bounds.Height) / 10); y++)
                {
                    for (int x = (int)fence.Bounds.X / 10; x < (int)((fence.Bounds.X + fence.Bounds.Width) / 10); x++)
                    {
                        Grid[y, x] += 10;
                    }
                }
            }

            foreach (FenceBottom fence in fenceBottoms)
            {
                for (int y = (int)fence.Bounds.Y / 10; y < (int)((fence.Bounds.Y + fence.Bounds.Height) / 10); y++)
                {
                    for (int x = (int)fence.Bounds.X / 10; x < (int)((fence.Bounds.X + fence.Bounds.Width) / 10); x++)
                    {
                        Grid[y, x] += 10;
                    }
                }
            }

            foreach (GateTop gate in gateTops)
            {
                for (int y = (int)gate.Bounds.Y / 10; y < (int)((gate.Bounds.Y + gate.Bounds.Height) / 10); y++)
                {
                    for (int x = (int)gate.Bounds.X / 10; x < (int)((gate.Bounds.X + gate.Bounds.Width) / 10); x++)
                    {
                        Grid[y, x] += 10;
                    }
                }
            }

            foreach (TrashBagSprite bag in trashBags)
            {
                for (int y = (int)(bag.Position.Y + 21) / 10; y < (int)(((bag.Position.Y + 21) + (bag.Bounds.Radius * 2)) / 10); y++)
                {
                    for (int x = (int)(bag.Position.X + 23) / 10; x < (int)(((bag.Position.X + 23) + (bag.Bounds.Radius * 2)) / 10); x++)
                    {
                        Grid[y, x] += 10;
                    }
                }
            }

            if(clear)
            {
                gateSoundPlayed = true;
            }
            #endregion
        }


        /// <summary>
        /// Loads general use content (with a few one use objects)
        /// </summary>
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

            //load player and player dmg sound
            player.LoadContent(Content);
            hit = Content.Load<SoundEffect>("hit");

            //load gate open sound
            gateOpen = Content.Load<SoundEffect>("gateOpen");

            //main menu content
            playBtn.LoadContent(Content);
            exitBtn.LoadContent(Content);
            title = Content.Load<Texture2D>("Logo");
            spriteFont = Content.Load<SpriteFont>("arial");


            //hud content
            healthCan = Content.Load<Texture2D>("HealthCans(hud)/Can");
            emptyHealthCan = Content.Load<Texture2D>("HealthCans(hud)/TransparentCan");
            halfHealthCan = Content.Load<Texture2D>("HealthCans(hud)/HalfCan");

            //the persistant background(may add more or different color versions later)
            background = Content.Load<Texture2D>("background");
            
            //load music content and set its volume
            bossMusic = Content.Load<Song>("heavy_metal_looping");
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = .1f;


            _tilemap.LoadContent(Content);
        }

        /// <summary>
        /// The update loop for the game
        /// </summary>
        /// <param name="gameTime">gametime object</param>
        protected override void Update(GameTime gameTime)
        {
            //Default game exit, may remove when pause menu is added in
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // update the cube 
            //cube.Update(gameTime);

            //player update, check if player health is 0 to end game
            #region
            player.LastMove = player.Position;
            player.Update(gameTime, Content);

            if (player.PlayerCurrentHealth <= 0)
            {
                MediaPlayer.Pause();
                InitializeLevelX(State.GameOver, true, false, false, false);
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

            foreach (GateTop gate in gateTops)
            {
                if (player.Bounds.CollidesWith(gate.Bounds))
                {
                    player.Position = player.LastMove;
                }
                foreach (PlayerProjectile proj in player.PlayerProjectile)
                {
                    if (proj.Bounds.CollidesWith(gate.Bounds))
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
                if(!spider.Dead)
                {
                    spider.Update(gameTime, player);

                    if (spider.Health <= 0)
                    {
                        spider.Dead = true;
                        spider.AnimationFrame = 0;
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
                            player.Position = player.LastMove;
                        }
                    }
                }
                
            }

            foreach (TrashSpiderSprite spider in deadSpiders)
            {
                livingSpiders.Remove(spider);
            }
            //deadSpiders.Clear();
            #endregion

            //trash bag update, collisions, and life track
            #region
            foreach (TrashBagSprite bag in trashBags)
            {
                if(bag.Level == (float)gameState)
                {
                    bag.Update(gameTime, player);

                    if (bag.Health <= 0)
                    {
                        deadTrashBags.Add(bag);
                        for (int y = (int)(bag.Position.Y + 21) / 10; y < (int)(((bag.Position.Y + 21) + (bag.Bounds.Radius * 2)) / 10); y++)
                        {
                            for (int x = (int)(bag.Position.X + 23) / 10; x < (int)(((bag.Position.X + 23) + (bag.Bounds.Radius * 2)) / 10); x++)
                            {
                                Grid[y, x] -= 10;
                            }
                        }
                    }

                    if (player.CenterBounds.CollidesWith(bag.Bounds) && bag.Health > 0)
                    {
                        player.Position = player.LastMove;
                    }
                }
                
            }

            foreach (TrashBagSprite bag in deadTrashBags)
            {
                trashBags.Remove(bag);
            }
            #endregion

            //health pickup update logic
            foreach (HealthPickup hp in healthPickups)
            {
                if (hp.Level == (float)gameState)
                {
                    if (player.Bounds.CollidesWith(hp.Bounds) && (Keyboard.GetState().IsKeyDown(Keys.Space) || GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed))
                    {
                        if(player.PlayerCurrentHealth + 1 < player.PlayerMaxHealth)
                        {
                            player.PlayerCurrentHealth++;
                        }
                        else if(player.PlayerCurrentHealth + 1 >= player.PlayerMaxHealth)
                        {
                            player.PlayerCurrentHealth = player.PlayerMaxHealth;
                        }
                        hpPickedUp.Add(hp);
                    }
                }
            }

            foreach (HealthPickup hp in hpPickedUp)
            {
                healthPickups.Remove(hp);
            }

            hpPickedUp.Clear();

            //wall collisions
            #region
            foreach (Wall wall in walls)
            {
                if (player.CenterBounds.CollidesWith(wall.Bounds))
                {
                    player.Position = player.LastMove;
                }
                foreach (PlayerProjectile proj in player.PlayerProjectile)
                {
                    if (proj.Bounds.CollidesWith(wall.Bounds))
                    {
                        player.ProjectileRemove.Add(proj);
                    }
                }
            }
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
               

                foreach(GasProjectile proj in raccoon.GasProjectileActive)
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
                    InitializeLevelX((State)levelList[0].LevelNum, levelList[0].Cleared, levelList[0].Shop, levelList[0].ItemRoom, levelList[0].Loaded);
                    levelList[0].Loaded = true;
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
                    levelList[levelIndex].Cleared = true;
                    levelIndex++;
                    if(levelIndex < levelList.Count)
                    {
                        InitializeLevelX((State)levelList[levelIndex].LevelNum, levelList[levelIndex].Cleared, levelList[levelIndex].Shop, levelList[levelIndex].ItemRoom, levelList[levelIndex].Loaded);
                        levelList[levelIndex].Loaded = true;
                    }
                }
            }
            #endregion

            //update logic for levels with enemies
            #region
            if (gameState != State.Level0 && gameState != State.GameOver && gameState != State.MainMenu)
            {
                if(livingRaccoons.Count == 0 && livingSpiders.Count ==0)
                {
                    levelList[levelIndex].Cleared = true;

                    if(!gateSoundPlayed)
                    {
                        gateOpen.Play(.3f, 0, 0);
                        gateSoundPlayed = true;
                    }

                    foreach (GateTop gate in gateTops)
                    {
                        gate.IsOpen = true;
                    }
                }
                if (player.Position.Y < 0)
                {
                    player.Position = new Vector2(GraphicsDevice.Viewport.Width / 2, 760);
                    levelIndex++;
                    if (levelIndex < levelList.Count)
                    {
                        InitializeLevelX((State)levelList[levelIndex].LevelNum, levelList[levelIndex].Cleared, levelList[levelIndex].Shop, levelList[levelIndex].ItemRoom, levelList[levelIndex].Loaded);
                        levelList[levelIndex].Loaded = true;
                    }
                    else
                    {
                        InitializeLevelX(State.GameOver, false, false, false, false);
                    }
                }

                if (player.Position.Y > 768)
                {
                    player.Position = new Vector2(GraphicsDevice.Viewport.Width / 2, 10);
                    levelIndex--;
                    if (levelIndex <= levelList.Count)
                    {
                        InitializeLevelX((State)levelList[levelIndex].LevelNum, levelList[levelIndex].Cleared, levelList[levelIndex].Shop, levelList[levelIndex].ItemRoom, levelList[levelIndex].Loaded);
                    }
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
                        levelList[i].Cleared = false;
                    }

                    InitializeLevelX(State.Level0, false, false, false, false);
                    levelList[levelIndex].Loaded = true;
                }
            }
            #endregion

            base.Update(gameTime);
        }

        /// <summary>
        /// draw loop for game
        /// </summary>
        /// <param name="gameTime">gametime object</param>
        protected override void Draw(GameTime gameTime)
        {
            //set the graphics device default color
            GraphicsDevice.Clear(Color.BurlyWood);

            //offset used for viewport shake on player hit
            Vector2 offset = new Vector2(0, 0);
            
            //check and execution of the screen shake when player is hit
            if(shakeViewport)
            {
                //set the offset and then update the radius and start angle variables
                offset = new Vector2((float)(Math.Sin(shakeStartAngle) * shakeRadius), (float)(Math.Cos(shakeStartAngle) * shakeRadius));
                shakeRadius -= 0.25f;
                shakeStartAngle += (150 + RandomHelper.Next(60));

                //after the shake is over (by either time or the radius becoming zero)
                if(gameTime.TotalGameTime.TotalSeconds - shakeStart > 2 || shakeRadius <= 0)
                {
                    //reset all of the values to their default values
                    shakeViewport = false;
                    shakeRadius = 5;
                    shakeStartAngle = 150;

                }
            }

            //begin spritebatch drawing
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(offset.X, offset.Y, 0));

            //if state is main menu draw main menu content
            if (gameState == State.MainMenu)
            {
                _spriteBatch.Draw(title, new Vector2(70, 20), Color.White);
                playBtn.Draw(gameTime, _spriteBatch);
                exitBtn.Draw(gameTime, _spriteBatch);
                _spriteBatch.DrawString(spriteFont, "             WASD/Left stick to Move \n                 Space/A to interact\n         Arrow keys/Right stick to shoot\nEsc/Back or interact with Exit button to quit", new Vector2((GraphicsDevice.Viewport.Width /2 - 225), GraphicsDevice.Viewport.Height - 125), Color.White);
                player.Draw(gameTime, _spriteBatch);
            }

            //otherwise go through the lists of gameplay objects and draw them to the screen
            if(gameState != State.MainMenu && gameState != State.GameOver)
            {
                if(gameState != State.Level5)
                {
                    //default background
                    _spriteBatch.Draw(background, new Vector2(0, 0), Color.White);
                }
                else
                {
                    _tilemap.Draw(gameTime, _spriteBatch);
                }

                //top of screen fences
                foreach (FenceTop fence in fenceTops)
                {
                    fence.Draw(gameTime, _spriteBatch);
                }

                //top of screen gates
                foreach (GateTop gate in gateTops)
                {
                    gate.Draw(gameTime, _spriteBatch);
                }


                //left and rigth side fences
                foreach (FenceSide fence in fenceSides)
                {
                    fence.Draw(gameTime, _spriteBatch);
                }

                //draw all spider corpses
                foreach (TrashSpiderSprite spider in deadSpiders)
                {
                    spider.Draw(gameTime, _spriteBatch);
                } 

                //dead trash bags
                foreach (TrashBagSprite bag in deadTrashBags)
                {
                    bag.Draw(gameTime, _spriteBatch);
                }

                //trash bags that need to be drawn below the player
                foreach (TrashBagSprite bag in trashBags)
                {
                    if (bag.Level == (float)gameState)
                    {
                        if (bag.Bounds.Center.Y < player.CenterBounds.Y)
                        {
                            bag.Draw(gameTime, _spriteBatch);
                        }
                    }
                }

               

                //living trash spiders
                foreach (TrashSpiderSprite spider in livingSpiders)
                {
                    spider.Draw(gameTime, _spriteBatch);
                }

                

                //living raccoons
                foreach (RaccoonSprite raccoon in livingRaccoons)
                {
                    raccoon.Draw(gameTime, _spriteBatch);
                }

                //draw walls below the player
                foreach(Wall wall in walls)
                {
                    if (wall.Bounds.Y + 16 < player.CenterBounds.Y)
                    {
                        wall.Draw(gameTime, _spriteBatch);
                    }
                }

                foreach(HealthPickup hp in healthPickups)
                {
                    if(hp.Level == (float)gameState)
                    {
                        hp.Draw(gameTime, _spriteBatch);
                    }
                }
                //the player
                player.Draw(gameTime, _spriteBatch);

                //draw walls above the player
                foreach (Wall wall in walls)
                {
                    if (wall.Bounds.Y + 16 >= player.CenterBounds.Y)
                    {
                        wall.Draw(gameTime, _spriteBatch);
                    }
                }

                //trash bags that need drawn above the player
                foreach (TrashBagSprite bag in trashBags)
                {
                    if (bag.Level == (float)gameState)
                    {
                        if (bag.Bounds.Center.Y >= player.CenterBounds.Y)
                        {
                            bag.Draw(gameTime, _spriteBatch);
                        }
                    }

                }

                //draw the hud (grows and shrinks based on the player's stats
                int cans = player.PlayerCurrentHealth;
                int maxCans = player.PlayerMaxHealth;
                Vector2 currentPos = new Vector2(20, 20);
                //draw the max health with lower opacity cans
                while (maxCans > 0)
                {
                    if (maxCans >= 2)
                    {
                        _spriteBatch.Draw(emptyHealthCan, currentPos, Color.White);
                        currentPos += new Vector2(45, 0);
                        maxCans -= 2;
                    }
                }

                //draw the current health with full and half cans over top of that
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

            //if game over draw the game over text and prompt to quit/restart. May switch the restart to return to main menu instead of going right back in
            if(gameState == State.GameOver)
            {
                _spriteBatch.DrawString(spriteFont, "          GAME OVER\n   Esc/Back button to exit\nPress Space or A to restart", new Vector2((GraphicsDevice.Viewport.Width / 2) - 140, (GraphicsDevice.Viewport.Height / 2) - 30), Color.White);
            }

            //end of spritebatch draws
            _spriteBatch.End();
            

            base.Draw(gameTime);
        }
    }
}
