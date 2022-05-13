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
        RatBoss = 96,
        GameOverWin = 97,
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

        public RatBossSprite RatBoss;

        //a list of fence tops, bottoms, and sides that are present in the current level. List is cleared and re-populated when lvl is changed
        private List<FenceTop> fenceTops = new List<FenceTop>();
        private List<FenceBottom> fenceBottoms = new List<FenceBottom>();
        private List<FenceSide> fenceSides = new List<FenceSide>();

        //list of health pickups
        public List<HealthPickup> healthPickups = new List<HealthPickup>();
        public List<HealthPickup> hpPickedUp = new List<HealthPickup>();

        //list of Item pickups
        public List<ItemPickup> ItemPickups = new List<ItemPickup>();
        public List<ItemPickup> ItemPickedUp = new List<ItemPickup>();

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

        private bool showControls = true;
        private Texture2D controlScreen;
        private double startGameTimer;

        private bool choosingItem = true;

        private double bossDeadTimer;

        private Texture2D test;
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

        /// <summary>
        /// Initilization logic
        /// </summary>
        protected override void Initialize()
        {
            //initialize the player object and have their starting location default to center screen 
            player = new PlayerController() { Position = new Vector2((GraphicsDevice.Viewport.Width / 2) -120, (GraphicsDevice.Viewport.Height / 2)) };

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
            levelList.Add(new LevelInfo((int)State.RatBoss, false, false, false, false));

            //then begin adding random levels until the level count has been filled,
            //need to still add a level count variable to adjust based on what stage the player is in
            //levelList.Add(new LevelInfo(1, false, false, false, false));
            //levelList.Add(new LevelInfo(RandomHelper.Next(1, 4), false, false, false, false));
            //levelList.Add(new LevelInfo(RandomHelper.Next(1, 4), false, false, false, false));
            //levelList.Add(new LevelInfo(RandomHelper.Next(1, 4), false, false, false, false));
            //levelList.Add(new LevelInfo(RandomHelper.Next(1, 4), false, false, false, false));
            //levelList.Add(new LevelInfo(RandomHelper.Next(1, 4), false, false, false, false));
            //levelList.Add(new LevelInfo(RandomHelper.Next(1, 4), false, false, false, false));

            //select a random level in the list of levels and set it as the stages item room/shop
            //levelList[RandomHelper.Next(1, levelList.Count)].ItemRoom = true;
            //levelList[RandomHelper.Next(1, levelList.Count)].Shop = true;

            //initialize pathfinder
            pathfinder = new AStarPathfinder(Grid);


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
                    trashBags.Add(new TrashBagSprite(new Vector2(1210, 605), Content, levelIndex));

                    trashBags.Add(new TrashBagSprite(new Vector2(25, 360), Content, levelIndex));
                    trashBags.Add(new TrashBagSprite(new Vector2(25, 390), Content, levelIndex));
                    trashBags.Add(new TrashBagSprite(new Vector2(45, 375), Content, levelIndex));
                    trashBags.Add(new TrashBagSprite(new Vector2(25, 420), Content, levelIndex));
                    trashBags.Add(new TrashBagSprite(new Vector2(25, 450), Content, levelIndex));
                    trashBags.Add(new TrashBagSprite(new Vector2(120, 245), Content, levelIndex));
                    trashBags.Add(new TrashBagSprite(new Vector2(150, 220), Content, levelIndex));
                    trashBags.Add(new TrashBagSprite(new Vector2(143, 240), Content, levelIndex));

                    trashBags.Add(new TrashBagSprite(new Vector2(1185, 665), Content, levelIndex));
                    trashBags.Add(new TrashBagSprite(new Vector2(1185, 635), Content, levelIndex));
                    trashBags.Add(new TrashBagSprite(new Vector2(1155, 665), Content, levelIndex));

                    trashBags.Add(new TrashBagSprite(new Vector2(1310, 105), Content, levelIndex));
                    trashBags.Add(new TrashBagSprite(new Vector2(1310, 75), Content, levelIndex));
                    trashBags.Add(new TrashBagSprite(new Vector2(1285, 75), Content, levelIndex));

                    ItemPickups.Add(new ItemPickup(new Vector2(360, 300), Content, levelIndex, 0));
                    ItemPickups.Add(new ItemPickup(new Vector2(660, 300), Content, levelIndex, 1));
                    ItemPickups.Add(new ItemPickup(new Vector2(960, 300), Content, levelIndex, 2));
                }

                

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
                    //livingSpiders.Add(new TrashSpiderSprite(new Vector2(450, 455), Content, pathfinder));
                    //livingSpiders.Add(new TrashSpiderSprite(new Vector2(450, 455), Content, pathfinder));
                    livingSpiders.Add(new TrashSpiderSprite(new Vector2(650, 426), Content, pathfinder));
                    livingSpiders.Add(new TrashSpiderSprite(new Vector2(676, 426), Content, pathfinder));
                    livingSpiders.Add(new TrashSpiderSprite(new Vector2(456, 456), Content, pathfinder));
                }

                if(!loaded)
                {
                    trashBags.Add(new TrashBagSprite(new Vector2(650, 455), Content, levelIndex));
                    trashBags.Add(new TrashBagSprite(new Vector2(675, 455), Content, levelIndex));
                    trashBags.Add(new TrashBagSprite(new Vector2(700, 455), Content, levelIndex));
                    trashBags.Add(new TrashBagSprite(new Vector2(625, 455), Content, levelIndex));
                    trashBags.Add(new TrashBagSprite(new Vector2(425, 455), Content, levelIndex));
                    trashBags.Add(new TrashBagSprite(new Vector2(425, 425), Content, levelIndex));
                    trashBags.Add(new TrashBagSprite(new Vector2(400, 425), Content, levelIndex));
                    trashBags.Add(new TrashBagSprite(new Vector2(750, 310), Content, levelIndex));
                    trashBags.Add(new TrashBagSprite(new Vector2(775, 310), Content, levelIndex));
                }


                MediaPlayer.Play(bossMusic);

                //walls
                #region
                walls.Add(new Wall(new Vector2(980, 470), Content, 2));
                walls.Add(new Wall(new Vector2(950, 470), Content, 16));
                walls.Add(new Wall(new Vector2(920, 470), Content, 0));
                walls.Add(new Wall(new Vector2(920, 500), Content, 13));
                walls.Add(new Wall(new Vector2(890, 500), Content, 1));
                walls.Add(new Wall(new Vector2(860, 500), Content, 1));
                walls.Add(new Wall(new Vector2(830, 500), Content, 1));
                walls.Add(new Wall(new Vector2(800, 500), Content, 1));
                walls.Add(new Wall(new Vector2(770, 500), Content, 1));
                walls.Add(new Wall(new Vector2(740, 500), Content, 1));
                walls.Add(new Wall(new Vector2(710, 500), Content, 1));
                walls.Add(new Wall(new Vector2(680, 500), Content, 1));
                walls.Add(new Wall(new Vector2(650, 500), Content, 1));
                walls.Add(new Wall(new Vector2(620, 500), Content, 1));
                walls.Add(new Wall(new Vector2(590, 500), Content, 1));
                walls.Add(new Wall(new Vector2(560, 500), Content, 1));
                walls.Add(new Wall(new Vector2(530, 500), Content, 1));
                walls.Add(new Wall(new Vector2(500, 500), Content, 1));
                walls.Add(new Wall(new Vector2(470, 500), Content, 1));
                walls.Add(new Wall(new Vector2(440, 500), Content, 1));
                walls.Add(new Wall(new Vector2(410, 500), Content, 13));
                walls.Add(new Wall(new Vector2(410, 470), Content, 2));
                walls.Add(new Wall(new Vector2(380, 470), Content, 16));
                walls.Add(new Wall(new Vector2(350, 470), Content, 0));

                walls.Add(new Wall(new Vector2(980, 500), Content, 4));
                walls.Add(new Wall(new Vector2(950, 500), Content, 17));
                walls.Add(new Wall(new Vector2(920, 500), Content, 13));
                walls.Add(new Wall(new Vector2(920, 530), Content, 4));
                walls.Add(new Wall(new Vector2(890, 530), Content, 17));
                walls.Add(new Wall(new Vector2(860, 530), Content, 17));
                walls.Add(new Wall(new Vector2(830, 530), Content, 17));
                walls.Add(new Wall(new Vector2(800, 530), Content, 17));
                walls.Add(new Wall(new Vector2(770, 530), Content, 17));
                walls.Add(new Wall(new Vector2(740, 530), Content, 17));
                walls.Add(new Wall(new Vector2(710, 530), Content, 17));
                walls.Add(new Wall(new Vector2(680, 530), Content, 17));
                walls.Add(new Wall(new Vector2(650, 530), Content, 17));
                walls.Add(new Wall(new Vector2(620, 530), Content, 17));
                walls.Add(new Wall(new Vector2(590, 530), Content, 17));
                walls.Add(new Wall(new Vector2(560, 530), Content, 17));
                walls.Add(new Wall(new Vector2(530, 530), Content, 17));
                walls.Add(new Wall(new Vector2(500, 530), Content, 17));
                walls.Add(new Wall(new Vector2(470, 530), Content, 17));
                walls.Add(new Wall(new Vector2(440, 530), Content, 17));
                walls.Add(new Wall(new Vector2(410, 530), Content, 6));
                walls.Add(new Wall(new Vector2(410, 500), Content, 13));
                walls.Add(new Wall(new Vector2(380, 500), Content, 17));
                walls.Add(new Wall(new Vector2(350, 500), Content, 6));

                walls.Add(new Wall(new Vector2(920, 300), Content, 9));
                walls.Add(new Wall(new Vector2(890, 300), Content, 17));
                walls.Add(new Wall(new Vector2(860, 300), Content, 17));
                walls.Add(new Wall(new Vector2(830, 300), Content, 17));
                walls.Add(new Wall(new Vector2(800, 300), Content, 17));
                walls.Add(new Wall(new Vector2(770, 300), Content, 17));
                walls.Add(new Wall(new Vector2(740, 300), Content, 17));
                walls.Add(new Wall(new Vector2(710, 300), Content, 17));
                walls.Add(new Wall(new Vector2(680, 300), Content, 17));
                walls.Add(new Wall(new Vector2(650, 300), Content, 17));
                walls.Add(new Wall(new Vector2(620, 300), Content, 17));
                walls.Add(new Wall(new Vector2(590, 300), Content, 17));
                walls.Add(new Wall(new Vector2(560, 300), Content, 17));
                walls.Add(new Wall(new Vector2(530, 300), Content, 17));
                walls.Add(new Wall(new Vector2(500, 300), Content, 17));
                walls.Add(new Wall(new Vector2(470, 300), Content, 17));
                walls.Add(new Wall(new Vector2(440, 300), Content, 17));
                walls.Add(new Wall(new Vector2(410, 300), Content, 11));

                walls.Add(new Wall(new Vector2(890, 270), Content, 0));
                walls.Add(new Wall(new Vector2(860, 270), Content, 13));
                walls.Add(new Wall(new Vector2(830, 270), Content, 13));
                walls.Add(new Wall(new Vector2(800, 270), Content, 13));
                walls.Add(new Wall(new Vector2(770, 270), Content, 13));
                walls.Add(new Wall(new Vector2(740, 270), Content, 13));
                walls.Add(new Wall(new Vector2(710, 270), Content, 13));
                walls.Add(new Wall(new Vector2(680, 270), Content, 13));
                walls.Add(new Wall(new Vector2(650, 270), Content, 13));
                walls.Add(new Wall(new Vector2(620, 270), Content, 13));
                walls.Add(new Wall(new Vector2(590, 270), Content, 13));
                walls.Add(new Wall(new Vector2(560, 270), Content, 13));
                walls.Add(new Wall(new Vector2(530, 270), Content, 13));
                walls.Add(new Wall(new Vector2(500, 270), Content, 13));
                walls.Add(new Wall(new Vector2(470, 270), Content, 13));
                walls.Add(new Wall(new Vector2(440, 270), Content, 2));

                walls.Add(new Wall(new Vector2(860, 240), Content, 0));
                walls.Add(new Wall(new Vector2(830, 240), Content, 16));
                walls.Add(new Wall(new Vector2(800, 240), Content, 16));
                walls.Add(new Wall(new Vector2(770, 240), Content, 16));
                walls.Add(new Wall(new Vector2(740, 240), Content, 16));
                walls.Add(new Wall(new Vector2(710, 240), Content, 16));
                walls.Add(new Wall(new Vector2(680, 240), Content, 16));
                walls.Add(new Wall(new Vector2(650, 240), Content, 16));
                walls.Add(new Wall(new Vector2(620, 240), Content, 16));
                walls.Add(new Wall(new Vector2(590, 240), Content, 16));
                walls.Add(new Wall(new Vector2(560, 240), Content, 16));
                walls.Add(new Wall(new Vector2(530, 240), Content, 16));
                walls.Add(new Wall(new Vector2(500, 240), Content, 16));
                walls.Add(new Wall(new Vector2(470, 240), Content, 2));

                walls.Add(new Wall(new Vector2(1320, 90), Content, 2));
                walls.Add(new Wall(new Vector2(1320, 120), Content, 8));
                walls.Add(new Wall(new Vector2(1290, 90), Content, 11));
                #endregion

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
                    livingSpiders.Add(new TrashSpiderSprite(new Vector2(220, 500), Content, pathfinder));
                    livingSpiders.Add(new TrashSpiderSprite(new Vector2(1100, 500), Content, pathfinder));

                    livingRaccoons.Add(new RaccoonSprite(new Vector2((GraphicsDevice.Viewport.Width / 2) - 32, (GraphicsDevice.Viewport.Height / 2)), Content));
                }

                if (!loaded)
                {
                    trashBags.Add(new TrashBagSprite(new Vector2(210, 470), Content, levelIndex));
                    trashBags.Add(new TrashBagSprite(new Vector2(235, 445), Content, levelIndex));
                    trashBags.Add(new TrashBagSprite(new Vector2(260, 420), Content, levelIndex));

                    trashBags.Add(new TrashBagSprite(new Vector2(1100, 475), Content, levelIndex));
                    trashBags.Add(new TrashBagSprite(new Vector2(1100, 425), Content, levelIndex));
                    trashBags.Add(new TrashBagSprite(new Vector2(1100, 450), Content, levelIndex));

                    //form left side wall of bags
                    trashBags.Add(new TrashBagSprite(new Vector2(100, 270), Content, levelIndex));
                    trashBags.Add(new TrashBagSprite(new Vector2(125, 270), Content, levelIndex));
                    trashBags.Add(new TrashBagSprite(new Vector2(150, 270), Content, levelIndex));
                    trashBags.Add(new TrashBagSprite(new Vector2(75, 270), Content, levelIndex));
                    trashBags.Add(new TrashBagSprite(new Vector2(50, 270), Content, levelIndex));
                    trashBags.Add(new TrashBagSprite(new Vector2(25, 270), Content, levelIndex));
                    trashBags.Add(new TrashBagSprite(new Vector2(0, 270), Content, levelIndex));

                    //form right side wall of bags
                    trashBags.Add(new TrashBagSprite(new Vector2(1300, 470), Content, levelIndex));
                    trashBags.Add(new TrashBagSprite(new Vector2(1275, 470), Content, levelIndex));
                    trashBags.Add(new TrashBagSprite(new Vector2(1250, 470), Content, levelIndex));
                    trashBags.Add(new TrashBagSprite(new Vector2(1225, 470), Content, levelIndex));
                    trashBags.Add(new TrashBagSprite(new Vector2(1200, 470), Content, levelIndex));
                    trashBags.Add(new TrashBagSprite(new Vector2(1175, 470), Content, levelIndex));
                }

                //walls
                #region

                walls.Add(new Wall(new Vector2(490, 470), Content, 6));
                walls.Add(new Wall(new Vector2(520, 470), Content, 1));
                walls.Add(new Wall(new Vector2(550, 470), Content, 1));
                walls.Add(new Wall(new Vector2(580, 470), Content, 1));
                walls.Add(new Wall(new Vector2(610, 470), Content, 1));
                walls.Add(new Wall(new Vector2(640, 470), Content, 1));
                walls.Add(new Wall(new Vector2(670, 470), Content, 1));
                walls.Add(new Wall(new Vector2(700, 470), Content, 1));
                walls.Add(new Wall(new Vector2(730, 470), Content, 1));
                walls.Add(new Wall(new Vector2(760, 470), Content, 1));
                walls.Add(new Wall(new Vector2(790, 470), Content, 1));
                walls.Add(new Wall(new Vector2(820, 470), Content, 1));
                walls.Add(new Wall(new Vector2(850, 470), Content, 4));
                
                
                walls.Add(new Wall(new Vector2(850, 440), Content, 3));
                walls.Add(new Wall(new Vector2(850, 410), Content, 3));
                walls.Add(new Wall(new Vector2(850, 380), Content, 15));
                walls.Add(new Wall(new Vector2(850, 350), Content, 15));
                walls.Add(new Wall(new Vector2(850, 320), Content, 15));
                walls.Add(new Wall(new Vector2(850, 290), Content, 0));
                
                walls.Add(new Wall(new Vector2(490, 440), Content, 6));
                walls.Add(new Wall(new Vector2(490, 410), Content, 3));
                walls.Add(new Wall(new Vector2(490, 380), Content, 3));
                walls.Add(new Wall(new Vector2(490, 350), Content, 3));
                walls.Add(new Wall(new Vector2(490, 320), Content, 3));
                walls.Add(new Wall(new Vector2(490, 290), Content, 3));
                walls.Add(new Wall(new Vector2(490, 260), Content, 3));
                
                walls.Add(new Wall(new Vector2(460, 260), Content, 16));
                walls.Add(new Wall(new Vector2(430, 260), Content, 16));
                walls.Add(new Wall(new Vector2(400, 260), Content, 16));
                walls.Add(new Wall(new Vector2(370, 260), Content, 16));
                walls.Add(new Wall(new Vector2(340, 260), Content, 16));
                walls.Add(new Wall(new Vector2(310, 260), Content, 16));
                walls.Add(new Wall(new Vector2(280, 260), Content, 16));
                walls.Add(new Wall(new Vector2(250, 260), Content, 16));
                walls.Add(new Wall(new Vector2(220, 260), Content, 16));
                walls.Add(new Wall(new Vector2(190, 260), Content, 0));
                
                walls.Add(new Wall(new Vector2(880, 260), Content, 16));
                walls.Add(new Wall(new Vector2(910, 260), Content, 16));
                walls.Add(new Wall(new Vector2(940, 260), Content, 16));
                walls.Add(new Wall(new Vector2(970, 260), Content, 16));
                walls.Add(new Wall(new Vector2(1000, 260), Content, 16));
                walls.Add(new Wall(new Vector2(1030, 260), Content, 16));
                walls.Add(new Wall(new Vector2(1060, 260), Content, 16));
                walls.Add(new Wall(new Vector2(1090, 260), Content, 16));
                walls.Add(new Wall(new Vector2(1120, 260), Content, 2));
                
                
                walls.Add(new Wall(new Vector2(190, 290), Content, 0));
                walls.Add(new Wall(new Vector2(190, 320), Content, 15));
                walls.Add(new Wall(new Vector2(190, 350), Content, 15));
                walls.Add(new Wall(new Vector2(190, 380), Content, 15));
                walls.Add(new Wall(new Vector2(190, 410), Content, 3));
                walls.Add(new Wall(new Vector2(190, 440), Content, 3));
                walls.Add(new Wall(new Vector2(190, 470), Content, 3));
                walls.Add(new Wall(new Vector2(190, 500), Content, 3));
                walls.Add(new Wall(new Vector2(190, 530), Content, 3));
                walls.Add(new Wall(new Vector2(190, 560), Content, 8));
                
                walls.Add(new Wall(new Vector2(1150, 290), Content, 2));
                walls.Add(new Wall(new Vector2(1150, 320), Content, 14));
                walls.Add(new Wall(new Vector2(1150, 350), Content, 14));
                walls.Add(new Wall(new Vector2(1150, 380), Content, 14));
                walls.Add(new Wall(new Vector2(1150, 410), Content, 3));
                walls.Add(new Wall(new Vector2(1150, 440), Content, 3));
                walls.Add(new Wall(new Vector2(1150, 470), Content, 3));
                walls.Add(new Wall(new Vector2(1150, 500), Content, 3));
                walls.Add(new Wall(new Vector2(1150, 530), Content, 3));
                walls.Add(new Wall(new Vector2(1150, 560), Content, 8));
                
                
                walls.Add(new Wall(new Vector2(880, 320), Content, 13));
                walls.Add(new Wall(new Vector2(910, 320), Content, 13));
                walls.Add(new Wall(new Vector2(940, 320), Content, 17));
                walls.Add(new Wall(new Vector2(970, 320), Content, 17));
                walls.Add(new Wall(new Vector2(1000, 320), Content, 17));
                walls.Add(new Wall(new Vector2(1030, 320), Content, 17));
                walls.Add(new Wall(new Vector2(1060, 320), Content, 17));
                walls.Add(new Wall(new Vector2(1090, 320), Content, 13));
                walls.Add(new Wall(new Vector2(1120, 320), Content, 13));

                walls.Add(new Wall(new Vector2(880, 290), Content, 13));
                walls.Add(new Wall(new Vector2(910, 290), Content, 13));
                walls.Add(new Wall(new Vector2(940, 290), Content, 13));
                walls.Add(new Wall(new Vector2(970, 290), Content, 13));
                walls.Add(new Wall(new Vector2(1000, 290), Content, 13));
                walls.Add(new Wall(new Vector2(1030, 290), Content, 13));
                walls.Add(new Wall(new Vector2(1060, 290), Content, 13));
                walls.Add(new Wall(new Vector2(1090, 290), Content, 13));
                walls.Add(new Wall(new Vector2(1120, 290), Content, 13));

                
                walls.Add(new Wall(new Vector2(1120, 350), Content, 13));
                walls.Add(new Wall(new Vector2(1090, 350), Content, 6));
                walls.Add(new Wall(new Vector2(1120, 380), Content, 6));
                walls.Add(new Wall(new Vector2(880, 350), Content, 13));
                walls.Add(new Wall(new Vector2(910, 350), Content, 4));
                walls.Add(new Wall(new Vector2(880, 380), Content, 4));
                #endregion

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

                //walls
                #region
                //top wall
                
                
                walls.Add(new Wall(new Vector2(185, 205), Content, 1));
                walls.Add(new Wall(new Vector2(215, 205), Content, 1));
                walls.Add(new Wall(new Vector2(245, 205), Content, 1));
                walls.Add(new Wall(new Vector2(275, 205), Content, 1));
                walls.Add(new Wall(new Vector2(305, 205), Content, 1));
                walls.Add(new Wall(new Vector2(335, 205), Content, 1));
                walls.Add(new Wall(new Vector2(365, 205), Content, 1));
                walls.Add(new Wall(new Vector2(395, 205), Content, 1));
                walls.Add(new Wall(new Vector2(425, 205), Content, 1));
                walls.Add(new Wall(new Vector2(455, 205), Content, 1));
                walls.Add(new Wall(new Vector2(485, 205), Content, 1));
               
               
                walls.Add(new Wall(new Vector2(845, 205), Content, 1));
                walls.Add(new Wall(new Vector2(875, 205), Content, 1));
                walls.Add(new Wall(new Vector2(905, 205), Content, 1));
                walls.Add(new Wall(new Vector2(935, 205), Content, 1));
                walls.Add(new Wall(new Vector2(965, 205), Content, 1));
                walls.Add(new Wall(new Vector2(995, 205), Content, 1));
                walls.Add(new Wall(new Vector2(1025, 205), Content, 1));
                walls.Add(new Wall(new Vector2(1055, 205), Content, 1));
                walls.Add(new Wall(new Vector2(1085, 205), Content, 1));
                walls.Add(new Wall(new Vector2(1115, 205), Content, 1));
                walls.Add(new Wall(new Vector2(1145, 205), Content, 1));
                
               

                //right side wall
                walls.Add(new Wall(new Vector2(845, 35), Content, 3));
                walls.Add(new Wall(new Vector2(845, 65), Content, 3));
                walls.Add(new Wall(new Vector2(845, 95), Content, 3));
                walls.Add(new Wall(new Vector2(845, 125), Content, 3));
                walls.Add(new Wall(new Vector2(845, 155), Content, 3));
                walls.Add(new Wall(new Vector2(845, 185), Content, 3));
                walls.Add(new Wall(new Vector2(845, 215), Content, 3));
                walls.Add(new Wall(new Vector2(845, 245), Content, 3));
                walls.Add(new Wall(new Vector2(845, 275), Content, 3));
                walls.Add(new Wall(new Vector2(845, 305), Content, 3));
                walls.Add(new Wall(new Vector2(845, 335), Content, 3));
                
                walls.Add(new Wall(new Vector2(845, 485), Content, 3));
                walls.Add(new Wall(new Vector2(845, 515), Content, 3));
                walls.Add(new Wall(new Vector2(845, 545), Content, 3));
                walls.Add(new Wall(new Vector2(845, 575), Content, 3));
                walls.Add(new Wall(new Vector2(845, 605), Content, 3));
                walls.Add(new Wall(new Vector2(845, 635), Content, 3));
                walls.Add(new Wall(new Vector2(845, 665), Content, 3));
                walls.Add(new Wall(new Vector2(845, 695), Content, 3));
                walls.Add(new Wall(new Vector2(845, 725), Content, 4));

                //left side wall
                walls.Add(new Wall(new Vector2(485, 35), Content, 3));
                walls.Add(new Wall(new Vector2(485, 65), Content, 3));
                walls.Add(new Wall(new Vector2(485, 95), Content, 3));
                walls.Add(new Wall(new Vector2(485, 125), Content, 3));
                walls.Add(new Wall(new Vector2(485, 155), Content, 3));
                walls.Add(new Wall(new Vector2(485, 185), Content, 3));
                walls.Add(new Wall(new Vector2(485, 215), Content, 3));
                walls.Add(new Wall(new Vector2(485, 245), Content, 3));
                walls.Add(new Wall(new Vector2(485, 275), Content, 3));
                walls.Add(new Wall(new Vector2(485, 305), Content, 3));
                
                walls.Add(new Wall(new Vector2(485, 455), Content, 3));
                walls.Add(new Wall(new Vector2(485, 485), Content, 3));
                walls.Add(new Wall(new Vector2(485, 515), Content, 3));
                walls.Add(new Wall(new Vector2(485, 545), Content, 3));
                walls.Add(new Wall(new Vector2(485, 575), Content, 3));
                walls.Add(new Wall(new Vector2(485, 605), Content, 3));
                walls.Add(new Wall(new Vector2(485, 635), Content, 3));
                walls.Add(new Wall(new Vector2(485, 665), Content, 3));
                walls.Add(new Wall(new Vector2(485, 695), Content, 3));
                walls.Add(new Wall(new Vector2(485, 725), Content, 6));

                //Bottom Wall
                
                walls.Add(new Wall(new Vector2(185, 625), Content, 1));
                walls.Add(new Wall(new Vector2(215, 625), Content, 1));
                walls.Add(new Wall(new Vector2(245, 625), Content, 1));
                walls.Add(new Wall(new Vector2(275, 625), Content, 1));
                walls.Add(new Wall(new Vector2(305, 625), Content, 1));
                walls.Add(new Wall(new Vector2(335, 625), Content, 1));
                walls.Add(new Wall(new Vector2(365, 625), Content, 1));
                walls.Add(new Wall(new Vector2(395, 625), Content, 1));
                walls.Add(new Wall(new Vector2(425, 625), Content, 1));
                walls.Add(new Wall(new Vector2(455, 625), Content, 1));
                walls.Add(new Wall(new Vector2(485, 625), Content, 1));
                          
                
                walls.Add(new Wall(new Vector2(845, 625), Content, 1));
                walls.Add(new Wall(new Vector2(875, 625), Content, 1));
                walls.Add(new Wall(new Vector2(905, 625), Content, 1));
                walls.Add(new Wall(new Vector2(935, 625), Content, 1));
                walls.Add(new Wall(new Vector2(965, 625), Content, 1));
                walls.Add(new Wall(new Vector2(995, 625), Content, 1));
                walls.Add(new Wall(new Vector2(1025, 625), Content, 1));
                walls.Add(new Wall(new Vector2(1055, 625), Content, 1));
                walls.Add(new Wall(new Vector2(1085, 625), Content, 1));
                walls.Add(new Wall(new Vector2(1115, 625), Content, 1));
                walls.Add(new Wall(new Vector2(1145, 625), Content, 1));
                
                #endregion

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

            //this level is rat boss room
            if (level == State.RatBoss)
            #region
            {
                //adds the boss
                RatBoss = new RatBossSprite(new Vector2(615, 30), Content);

                //walls
                #region
                //top wall
                walls.Add(new Wall(new Vector2(5, 5), Content, 0));
                walls.Add(new Wall(new Vector2(35, 5), Content, 1));
                walls.Add(new Wall(new Vector2(65, 5), Content, 1));
                walls.Add(new Wall(new Vector2(95, 5), Content, 1));
                walls.Add(new Wall(new Vector2(125, 5), Content, 1));
                walls.Add(new Wall(new Vector2(155, 5), Content, 1));
                walls.Add(new Wall(new Vector2(185, 5), Content, 1));
                walls.Add(new Wall(new Vector2(215, 5), Content, 1));
                walls.Add(new Wall(new Vector2(245, 5), Content, 1));
                walls.Add(new Wall(new Vector2(275, 5), Content, 1));
                walls.Add(new Wall(new Vector2(305, 5), Content, 1));
                walls.Add(new Wall(new Vector2(335, 5), Content, 1));
                walls.Add(new Wall(new Vector2(365, 5), Content, 1));
                walls.Add(new Wall(new Vector2(395, 5), Content, 1));
                walls.Add(new Wall(new Vector2(425, 5), Content, 1));
                walls.Add(new Wall(new Vector2(455, 5), Content, 1));
                walls.Add(new Wall(new Vector2(485, 5), Content, 1));
                walls.Add(new Wall(new Vector2(515, 5), Content, 1));
                walls.Add(new Wall(new Vector2(545, 5), Content, 1));
                walls.Add(new Wall(new Vector2(575, 5), Content, 1));
                walls.Add(new Wall(new Vector2(605, 5), Content, 1));
                walls.Add(new Wall(new Vector2(635, 5), Content, 1));
                walls.Add(new Wall(new Vector2(665, 5), Content, 1));
                walls.Add(new Wall(new Vector2(695, 5), Content, 1));
                walls.Add(new Wall(new Vector2(725, 5), Content, 1));
                walls.Add(new Wall(new Vector2(755, 5), Content, 1));
                walls.Add(new Wall(new Vector2(785, 5), Content, 1));
                walls.Add(new Wall(new Vector2(815, 5), Content, 1));
                walls.Add(new Wall(new Vector2(845, 5), Content, 1));
                walls.Add(new Wall(new Vector2(875, 5), Content, 1));
                walls.Add(new Wall(new Vector2(905, 5), Content, 1));
                walls.Add(new Wall(new Vector2(935, 5), Content, 1));
                walls.Add(new Wall(new Vector2(965, 5), Content, 1));
                walls.Add(new Wall(new Vector2(995, 5), Content, 1));
                walls.Add(new Wall(new Vector2(1025, 5), Content, 1));
                walls.Add(new Wall(new Vector2(1055, 5), Content, 1));
                walls.Add(new Wall(new Vector2(1085, 5), Content, 1));
                walls.Add(new Wall(new Vector2(1115, 5), Content, 1));
                walls.Add(new Wall(new Vector2(1145, 5), Content, 1));
                walls.Add(new Wall(new Vector2(1175, 5), Content, 1));
                walls.Add(new Wall(new Vector2(1205, 5), Content, 1));
                walls.Add(new Wall(new Vector2(1235, 5), Content, 1));
                walls.Add(new Wall(new Vector2(1265, 5), Content, 1));
                walls.Add(new Wall(new Vector2(1295, 5), Content, 1));
                walls.Add(new Wall(new Vector2(1325, 5), Content, 2));

                //right side wall
                walls.Add(new Wall(new Vector2(1325, 35), Content, 3));
                walls.Add(new Wall(new Vector2(1325, 65), Content, 3));
                walls.Add(new Wall(new Vector2(1325, 95), Content, 3));
                walls.Add(new Wall(new Vector2(1325, 125), Content, 3));
                walls.Add(new Wall(new Vector2(1325, 155), Content, 3));
                walls.Add(new Wall(new Vector2(1325, 185), Content, 3));
                walls.Add(new Wall(new Vector2(1325, 215), Content, 3));
                walls.Add(new Wall(new Vector2(1325, 245), Content, 3));
                walls.Add(new Wall(new Vector2(1325, 275), Content, 3));
                walls.Add(new Wall(new Vector2(1325, 305), Content, 3));
                walls.Add(new Wall(new Vector2(1325, 335), Content, 3));
                walls.Add(new Wall(new Vector2(1325, 365), Content, 3));
                walls.Add(new Wall(new Vector2(1325, 395), Content, 3));
                walls.Add(new Wall(new Vector2(1325, 425), Content, 3));
                walls.Add(new Wall(new Vector2(1325, 455), Content, 3));
                walls.Add(new Wall(new Vector2(1325, 485), Content, 3));
                walls.Add(new Wall(new Vector2(1325, 515), Content, 3));
                walls.Add(new Wall(new Vector2(1325, 545), Content, 3));
                walls.Add(new Wall(new Vector2(1325, 575), Content, 3));
                walls.Add(new Wall(new Vector2(1325, 605), Content, 3));
                walls.Add(new Wall(new Vector2(1325, 635), Content, 3));
                walls.Add(new Wall(new Vector2(1325, 665), Content, 3));
                walls.Add(new Wall(new Vector2(1325, 695), Content, 3));
                walls.Add(new Wall(new Vector2(1325, 725), Content, 4));

                //left side wall
                walls.Add(new Wall(new Vector2(5, 35), Content, 3));
                walls.Add(new Wall(new Vector2(5, 65), Content, 3));
                walls.Add(new Wall(new Vector2(5, 95), Content, 3));
                walls.Add(new Wall(new Vector2(5, 125), Content, 3));
                walls.Add(new Wall(new Vector2(5, 155), Content, 3));
                walls.Add(new Wall(new Vector2(5, 185), Content, 3));
                walls.Add(new Wall(new Vector2(5, 215), Content, 3));
                walls.Add(new Wall(new Vector2(5, 245), Content, 3));
                walls.Add(new Wall(new Vector2(5, 275), Content, 3));
                walls.Add(new Wall(new Vector2(5, 305), Content, 3));
                walls.Add(new Wall(new Vector2(5, 335), Content, 3));
                walls.Add(new Wall(new Vector2(5, 365), Content, 3));
                walls.Add(new Wall(new Vector2(5, 395), Content, 3));
                walls.Add(new Wall(new Vector2(5, 425), Content, 3));
                walls.Add(new Wall(new Vector2(5, 455), Content, 3));
                walls.Add(new Wall(new Vector2(5, 485), Content, 3));
                walls.Add(new Wall(new Vector2(5, 515), Content, 3));
                walls.Add(new Wall(new Vector2(5, 545), Content, 3));
                walls.Add(new Wall(new Vector2(5, 575), Content, 3));
                walls.Add(new Wall(new Vector2(5, 605), Content, 3));
                walls.Add(new Wall(new Vector2(5, 635), Content, 3));
                walls.Add(new Wall(new Vector2(5, 665), Content, 3));
                walls.Add(new Wall(new Vector2(5, 695), Content, 3));
                walls.Add(new Wall(new Vector2(5, 725), Content, 6));

                //Bottom Wall
                walls.Add(new Wall(new Vector2(35, 725), Content, 1));
                walls.Add(new Wall(new Vector2(65, 725), Content, 1));
                walls.Add(new Wall(new Vector2(95, 725), Content, 1));
                walls.Add(new Wall(new Vector2(125, 725), Content, 1));
                walls.Add(new Wall(new Vector2(155, 725), Content, 1));
                walls.Add(new Wall(new Vector2(185, 725), Content, 1));
                walls.Add(new Wall(new Vector2(215, 725), Content, 1));
                walls.Add(new Wall(new Vector2(245, 725), Content, 1));
                walls.Add(new Wall(new Vector2(275, 725), Content, 1));
                walls.Add(new Wall(new Vector2(305, 725), Content, 1));
                walls.Add(new Wall(new Vector2(335, 725), Content, 1));
                walls.Add(new Wall(new Vector2(365, 725), Content, 1));
                walls.Add(new Wall(new Vector2(395, 725), Content, 1));
                walls.Add(new Wall(new Vector2(425, 725), Content, 1));
                walls.Add(new Wall(new Vector2(455, 725), Content, 1));
                walls.Add(new Wall(new Vector2(485, 725), Content, 1));
                walls.Add(new Wall(new Vector2(515, 725), Content, 1));
                walls.Add(new Wall(new Vector2(545, 725), Content, 1));
                walls.Add(new Wall(new Vector2(575, 725), Content, 1));
                walls.Add(new Wall(new Vector2(605, 725), Content, 1));
                walls.Add(new Wall(new Vector2(635, 725), Content, 1));
                walls.Add(new Wall(new Vector2(665, 725), Content, 1));
                walls.Add(new Wall(new Vector2(695, 725), Content, 1));
                walls.Add(new Wall(new Vector2(725, 725), Content, 1));
                walls.Add(new Wall(new Vector2(755, 725), Content, 1));
                walls.Add(new Wall(new Vector2(785, 725), Content, 1));
                walls.Add(new Wall(new Vector2(815, 725), Content, 1));
                walls.Add(new Wall(new Vector2(845, 725), Content, 1));
                walls.Add(new Wall(new Vector2(875, 725), Content, 1));
                walls.Add(new Wall(new Vector2(905, 725), Content, 1));
                walls.Add(new Wall(new Vector2(935, 725), Content, 1));
                walls.Add(new Wall(new Vector2(965, 725), Content, 1));
                walls.Add(new Wall(new Vector2(995, 725), Content, 1));
                walls.Add(new Wall(new Vector2(1025, 725), Content, 1));
                walls.Add(new Wall(new Vector2(1055, 725), Content, 1));
                walls.Add(new Wall(new Vector2(1085, 725), Content, 1));
                walls.Add(new Wall(new Vector2(1115, 725), Content, 1));
                walls.Add(new Wall(new Vector2(1145, 725), Content, 1));
                walls.Add(new Wall(new Vector2(1175, 725), Content, 1));
                walls.Add(new Wall(new Vector2(1205, 725), Content, 1));
                walls.Add(new Wall(new Vector2(1235, 725), Content, 1));
                walls.Add(new Wall(new Vector2(1265, 725), Content, 1));
                walls.Add(new Wall(new Vector2(1295, 725), Content, 1));
                walls.Add(new Wall(new Vector2(1325, 725), Content, 2));
                #endregion

                gameState = State.RatBoss;
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
                for (int y = (int)(wall.Bounds.Y - 10) / 10; y < (int)(((wall.Bounds.Y + wall.Bounds.Height) + 10) / 10); y++)
                {
                    for (int x = (int)(wall.Bounds.X - 10) / 10; x < (int)(((wall.Bounds.X + wall.Bounds.Width) + 10) / 10); x++)
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
                if(bag.Level == levelIndex)
                {
                    for (int y = (int)(bag.Position.Y + 21) / 10; y < (int)(((bag.Position.Y + 21) + (bag.Bounds.Radius * 2)) / 10); y++)
                    {
                        for (int x = (int)(bag.Position.X + 23) / 10; x < (int)(((bag.Position.X + 23) + (bag.Bounds.Radius * 2)) / 10); x++)
                        {
                            Grid[y, x] += 10;
                        }
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

            controlScreen = Content.Load<Texture2D>("Controls");

            //load music content and set its volume
            bossMusic = Content.Load<Song>("heavy_metal_looping");
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = .1f;


            test = Content.Load<Texture2D>("test160");
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
                    
                    for (int y = (int)(spider.Bounds.Center.Y - spider.Bounds.Radius) / 10; y < (int)((spider.Bounds.Center.Y + spider.Bounds.Radius) / 10); y++)
                    {
                        for (int x = (int)(spider.Bounds.Center.X - spider.Bounds.Radius) / 10; x < (int)(spider.Bounds.Center.X + spider.Bounds.Radius) / 10; x++)
                        {
                            if(Grid[y,x] < 10)
                            {
                                Grid[y, x] = 0;
                            }
                            
                        }
                    }
                    

                    spider.Update(gameTime, player);

                    
                    for (int y = (int)(spider.Bounds.Center.Y - spider.Bounds.Radius) / 10; y < (int)((spider.Bounds.Center.Y + spider.Bounds.Radius) / 10); y++)
                    {
                        for (int x = (int)(spider.Bounds.Center.X - spider.Bounds.Radius) / 10; x < (int)(spider.Bounds.Center.X + spider.Bounds.Radius) / 10; x++)
                        {
                            if (Grid[y, x] < 10)
                            {
                                Grid[y, x] = 9;
                            }
                        }
                    }
                    

                    if (spider.Health <= 0)
                    {
                        for (int y = (int)(spider.Bounds.Center.Y - spider.Bounds.Radius) / 10; y < (int)((spider.Bounds.Center.Y + spider.Bounds.Radius) / 10); y++)
                        {
                            for (int x = (int)(spider.Bounds.Center.X - spider.Bounds.Radius) / 10; x < (int)(spider.Bounds.Center.X + spider.Bounds.Radius) / 10; x++)
                            {
                                if (Grid[y, x] < 10)
                                {
                                    Grid[y, x] = 0;
                                }

                            }
                        }

                        spider.Dead = true;
                        spider.AnimationFrame = 0;
                        if(RandomHelper.Next(0, 25) == 0)
                        {
                            healthPickups.Add(new HealthPickup(spider.Position, Content, levelIndex));
                        }
                        else if (RandomHelper.Next(0, 50) == 0)
                        {
                            ItemPickups.Add(new ItemPickup(spider.Position, Content, levelIndex, null));
                        }
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
                        if (RandomHelper.Next(0, 25) == 0)
                        {
                            healthPickups.Add(new HealthPickup(bag.Position, Content, levelIndex));
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
            #region
            foreach (HealthPickup hp in healthPickups)
            {
                if (hp.Level == (float)gameState)
                {
                    hp.Update(gameTime, player);

                    if (player.Bounds.CollidesWith(hp.Bounds) && (Keyboard.GetState().IsKeyDown(Keys.Space) || GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed))
                    {
                        if(player.PlayerCurrentHealth + 2 < player.PlayerMaxHealth)
                        {
                            player.PlayerCurrentHealth+= 2;
                        }
                        else if(player.PlayerCurrentHealth + 2 >= player.PlayerMaxHealth)
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
            #endregion

            //item pickup update logic
            #region
            foreach (ItemPickup item in ItemPickups)
            {
                if (item.Level == (float)gameState)
                {
                    item.Update(gameTime, player);

                    if (player.Bounds.CollidesWith(item.Bounds) && (Keyboard.GetState().IsKeyDown(Keys.Space) || GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed))
                    {
                        item.Pickup(player);
                        ItemPickedUp.Add(item);

                        if(gameState == State.Level0)
                        {
                            foreach(ItemPickup i in ItemPickups)
                            {
                                ItemPickedUp.Add(i);
                            }
                            choosingItem = false;
                        }

                    }
                }
            }

            foreach (ItemPickup item in ItemPickedUp)
            {
                ItemPickups.Remove(item);
            }

            hpPickedUp.Clear();
            #endregion

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
                    if (RandomHelper.Next(0, 25) == 0)
                    {
                        healthPickups.Add(new HealthPickup(raccoon.Position, Content, levelIndex));
                    }
                    else if (RandomHelper.Next(0, 50) == 0)
                    {
                        ItemPickups.Add(new ItemPickup(raccoon.Position, Content, levelIndex, null));
                    }
                }

                if(player.CenterBounds.CollidesWith(raccoon.ThrowingBounds))
                {
                    player.Position = player.LastMove;
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

                if(!showControls)
                {
                    startGameTimer += gameTime.ElapsedGameTime.TotalSeconds;
                    if (startGameTimer > .5 && player.Bounds.CollidesWith(playBtn.Bounds) && (Keyboard.GetState().IsKeyDown(Keys.Space) || GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed))
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
                else
                {
                    if(Keyboard.GetState().IsKeyDown(Keys.Space) || GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed)
                    {
                        showControls = false;
                    }
                }

                
            }
            #endregion

            //update logic for RatBoss
            #region
            if(gameState == State.RatBoss)
            {
                RatBoss.Update(gameTime, player);

                if (player.Hit == false)
                {
                    if (player.CenterBounds.CollidesWith(RatBoss.Bounds) || player.CenterBounds.CollidesWith(RatBoss.SlamHitBox))
                    {
                        player.Hit = true;
                        player.PlayerCurrentHealth--;
                        hit.Play(.3f, 0, 0);
                        shakeViewport = true;
                        shakeStart = (float)gameTime.TotalGameTime.TotalSeconds;
                        player.Position = player.LastMove;
                    }

                }

                if(RatBoss.CurrentHealth <= 0)
                {
                    RatBoss.Dead = true;
                    bossDeadTimer += gameTime.ElapsedGameTime.TotalSeconds;

                    if(bossDeadTimer > 1.5)
                    {
                        gameState = State.GameOverWin;
                    }
                }

                if(RatBoss.slamAnimationPlayed)
                {
                    shakeViewport = true;
                    shakeStart = (float)gameTime.TotalGameTime.TotalSeconds;
                }
            }

            
            #endregion

            //update logic for level 0
            #region
            if (gameState == State.Level0)
            {
                if(player.Position.Y < -10)
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
                if (player.Position.Y < -10)
                {
                    player.Position = new Vector2(GraphicsDevice.Viewport.Width / 2, 760);
                    levelIndex++;
                    if (levelIndex < levelList.Count)
                    {
                        InitializeLevelX((State)levelList[levelIndex].LevelNum, levelList[levelIndex].Cleared, levelList[levelIndex].Shop, levelList[levelIndex].ItemRoom, levelList[levelIndex].Loaded);
                        levelList[levelIndex].Loaded = true;
                        if((State)levelList[levelIndex].LevelNum == State.RatBoss)
                        {
                            player.Position = new Vector2(GraphicsDevice.Viewport.Width / 2, 660);
                        }
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
            if (gameState == State.GameOver || gameState == State.GameOverWin)
            {
                if ((Keyboard.GetState().IsKeyDown(Keys.Space) || GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed))
                {
                    livingSpiders.Clear();
                    player.Position = new Vector2((GraphicsDevice.Viewport.Width / 2) - 50, (GraphicsDevice.Viewport.Height / 2) - 30);
                    player.PlayerCurrentHealth = player.PlayerMaxHealth;
                    player.ProjFireRate = .75f;
                    player.ProjSpeed = 3;
                    player.ProjDmg = 1;
                    player.ProjRange = 250;
                    player.MovementSpeed = 3f;
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
                
                if(showControls)
                {
                    _spriteBatch.Draw(controlScreen, Vector2.Zero, Color.White);
                }

                player.Draw(gameTime, _spriteBatch);
            }

            //otherwise go through the lists of gameplay objects and draw them to the screen
            if(gameState != State.MainMenu && gameState != State.GameOver && gameState != State.GameOverWin)
            {

                //default background
                _spriteBatch.Draw(background, new Vector2(0, 0), Color.White);


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
                    if (bag.Level == levelIndex)
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

                //draw walls below the player
                foreach(Wall wall in walls)
                {
                    if (wall.Bounds.Y + 16 < player.CenterBounds.Y)
                    {
                        wall.Draw(gameTime, _spriteBatch);
                    }
                }

                //living raccoons
                foreach (RaccoonSprite raccoon in livingRaccoons)
                {
                    raccoon.Draw(gameTime, _spriteBatch);
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

                //draw rat boss if in his stage
                if (gameState == State.RatBoss)
                {
                    RatBoss.Draw(gameTime, _spriteBatch);
                }

                //trash bags that need drawn above the player
                foreach (TrashBagSprite bag in trashBags)
                {
                    if (bag.Level == levelIndex)
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

                foreach (HealthPickup hp in healthPickups)
                {
                    if (hp.Level == levelIndex)
                    {
                        hp.Draw(gameTime, _spriteBatch);
                    }
                }

                foreach (ItemPickup item in ItemPickups)
                {
                    if (item.Level == levelIndex)
                    {
                        item.Draw(gameTime, _spriteBatch);
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

            if (gameState == State.GameOverWin)
            {
                _spriteBatch.DrawString(spriteFont, "            You WIN!\n   Esc/Back button to exit\nPress Space or A to restart", new Vector2((GraphicsDevice.Viewport.Width / 2) - 140, (GraphicsDevice.Viewport.Height / 2) - 30), Color.White);
            }

            if (gameState == State.Level0 && choosingItem)
            {
                _spriteBatch.DrawString(spriteFont, "Choose Starting Item!", new Vector2(450, 200), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0);
            }

            //end of spritebatch draws
            _spriteBatch.End();
            

            base.Draw(gameTime);
        }
    }
}
