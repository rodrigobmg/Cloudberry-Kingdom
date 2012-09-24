using System;
using System.IO;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
#if PC_VERSION
#elif XBOX || XBOX_SIGNIN
using Microsoft.Xna.Framework.GamerServices;
#endif
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using Drawing;

using CloudberryKingdom.Bobs;
using CloudberryKingdom.Levels;
using CloudberryKingdom.Blocks;
using CloudberryKingdom.Awards;

#if XBOX
#else
using CloudberryKingdom.Viewer;
#endif

#if WINDOWS
using Forms = System.Windows.Forms;
#endif

namespace CloudberryKingdom
{
    public partial class CloudberryKingdomGame : Game
    {
        /// <summary>
        /// The version of the game we are working on now (+1 over the last uploaded to Steam).
        /// MajorVersion is 0 for beta, 1 for release.
        /// MinorVersion increases with substantial change.
        /// SubVersion increases with any pushed change.
        /// </summary>
        public static Version GameVersion = new Version(0, 1, 5);

        /// <summary>
        /// The command line arguments.
        /// </summary>
        public static string[] args;

        public static bool StartAsBackgroundEditor = false;
        public static bool StartAsTestLevel = false;
        public static bool StartAsBobAnimationTest = false;
        public static bool StartAsFreeplay = false;
#if INCLUDE_EDITOR
        public static bool LoadDynamic = true;
#else
        public static bool LoadDynamic = false;
#endif
        public static bool ShowSongInfo = true;

        public static string TileSetToTest = "cave";
        public static string ModRoot = "Standard";
        public static bool AlwaysSkipDynamicArt = false;

        public static bool HideGui = false;
        public static bool HideForeground = false;
        public static bool UseNewBob = false;

        //public static SimpleGameFactory TitleGameFactory = TitleGameData_Intense.Factory;
        public static SimpleGameFactory TitleGameFactory = TitleGameData_MW.Factory;
        //public static SimpleGameFactory TitleGameFactory = TitleGameData_Forest.Factory;

        /// <summary>
        /// Process the command line arguments.
        /// This is used to load different tools, such as the background editor, instead of the main game.
        /// </summary>
        /// <param name="args"></param>
        public static void ProcessArgs(string[] args)
        {
#if DEBUG
            // Artifically simulate different command line arguments.
            //args = new string[] { "test_bob_animation", "mod_root", "Bob" };
            //args = new string[] { "test_level" }; AlwaysSkipDynamicArt = true;
            //args = new string[] { "background_editor" }; //AlwaysSkipDynamicArt = true;
            //args = new string[] { "test_all" }; AlwaysSkipDynamicArt = false;
            //StartAsTestLevel = true;
#endif
            //args = new string[] { "test_all" }; AlwaysSkipDynamicArt = false;
            
            LoadDynamic = true;
            AlwaysSkipDynamicArt = false;


            CloudberryKingdomGame.args = args;

            var list = new List<string>(args); list.Reverse();
            var stack = new Stack<string>(list);
            
            while (stack.Count > 0)
            {
                var arg = stack.Pop();

                switch (arg)
                {
                    case "background_editor": StartAsBackgroundEditor = true; LoadDynamic = true; break;
                    case "test_level": StartAsTestLevel = true; LoadDynamic = true; break;
                    case "test_bob_animation": StartAsBobAnimationTest = true; LoadDynamic = false; break;
                    case "test_all":
                        ShowSongInfo = false;
                        UseNewBob = true;
                        StartAsFreeplay = true; LoadDynamic = true; break;
                    case "test_all_old_bob":
                        ShowSongInfo = false;
                        StartAsFreeplay = true; LoadDynamic = true; break;

                    case "mod_root":
                        LoadDynamic = true;
                        if (stack.Count > 0)
                            ModRoot = stack.Pop();
                        break;

                    default: break;
                }
            }
        }


        bool ShowFPS = false;
        public static float fps;

        public int DrawCount, PhsxCount;

        public ResolutionGroup Resolution;
        public ResolutionGroup[] Resolutions = new ResolutionGroup[4];

#if WINDOWS
        QuadClass MousePointer, MouseBack;
        bool _DrawMouseBackIcon = false;
        public bool DrawMouseBackIcon { get { return _DrawMouseBackIcon; } set { _DrawMouseBackIcon = value; } }
#endif

#if DEBUG || INCLUDE_EDITOR
        public static bool AlwaysGiveTutorials = true;
        public static bool UnlockAll = true;
        public static bool SimpleLoad = true;
        public static bool SimpleAiColors = false;
        public static bool BuildDebug = false;
#else
        public static bool AlwaysGiveTutorials = false;
        public static bool SimpleAiColors = false;
        public static bool RecordIntro = false;
        public static bool UnlockAll = true;
        public static bool SimpleLoad = false;
        public static bool BuildDebug = false;
#endif


        bool LogoScreenUp;

        /// <summary>
        /// When true the initial loading screen is drawn even after loading is finished
        /// </summary>
        public bool LogoScreenPropUp;

        /// <summary>
        /// True when we are still loading resources during the game's initial load.
        /// This is wrapped in a class so that it can be used as a lock.
        /// </summary>
        public WrappedBool LoadingResources;
        public int LoadingOffset;
        public WrappedFloat ResourceLoadedCountRef;
        
        /// <summary>
        /// The game's initial loading screen. Different than the in-game loading screens seen before levels.
        /// </summary>
        public InitialLoadingScreen LoadingScreen;

        public GraphicsDeviceManager graphics;

        RenderTarget2D ScreenShotRenderTarget;

        GraphicsDevice device;
        int screenWidth, screenHeight;

        /// <summary>
        /// Font used to display debug info on the screen.
        /// </summary>
        SpriteFont DebugFont;

        Camera MainCamera;

        public CloudberryKingdomGame()
        {
#if PC_VERSION
#elif XBOX || XBOX_SIGNIN
            Components.Add(new GamerServicesComponent(this));
#endif
            ResourceLoadedCountRef = new WrappedFloat();

            graphics = new GraphicsDeviceManager(this);
            graphics.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>(graphics_PreparingDeviceSettings);

            Content.RootDirectory = "Content";

            Tools.TheGame = this;
        }

        void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            //graphics.PreferMultiSampling = false;
            //graphics.GraphicsDevice.PresentationParameters.MultiSampleCount = 16;

            //graphics.PreferMultiSampling = true;
            //graphics.GraphicsDevice.PresentationParameters.MultiSampleCount = 16;
        }

        protected override void Initialize()
        {
#if WINDOWS
            KeyboardHandler.EventInput.Initialize(this.Window);
#endif
            Globals.ContentDirectory = Content.RootDirectory;

            Tools.LoadEffects(Content, true);

            ButtonString.Init();
            ButtonCheck.Reset();

            // Volume control
            Tools.SoundVolume = new WrappedFloat();
            Tools.SoundVolume.MinVal = 0;
            Tools.SoundVolume.MaxVal = 1;
            Tools.SoundVolume.Val = .7f;

            Tools.MusicVolume = new WrappedFloat();
            Tools.MusicVolume.MinVal = 0;
            Tools.MusicVolume.MaxVal = 1;
            Tools.MusicVolume.Val = 1;
            Tools.MusicVolume.SetCallback = () => Tools.UpdateVolume();

#if DEBUG || INCLUDE_EDITOR
            Tools.SoundVolume.Val = 0;
            Tools.MusicVolume.Val = 0;
#endif

#if PC_VERSION
            // The PC version let's the player specify resolution, key mapping, and so on.
            // Try to load these now.
            PlayerManager.RezData rez;
            try
            {
                rez = PlayerManager.LoadRezAndKeys();
            }
            catch
            {
                rez = new PlayerManager.RezData();
                rez.Custom = false;
            }
#elif WINDOWS
            PlayerManager.RezData rez = new PlayerManager.RezData();
            rez.Custom = true;
#if DEBUG || INCLUDE_EDITOR
            rez.Fullscreen = false;
#else
            rez.Fullscreen = true;
#endif
            rez.Width = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            rez.Height = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
#endif


            // Some possible resolutions.
            Resolutions[0] = new ResolutionGroup();
            Resolutions[0].Backbuffer = new IntVector2(1280, 800);
            Resolutions[0].Bob = new IntVector2(135, 0);
            Resolutions[0].TextOrigin = Vector2.Zero;
            Resolutions[0].LineHeightMod = 1f;

            Resolutions[0].CopyTo(ref Resolutions[1]);
            Resolutions[1].Backbuffer = new IntVector2(1280, 786);

            Resolutions[0].CopyTo(ref Resolutions[2]);
            Resolutions[2].Backbuffer = new IntVector2(1280, 720);

            Resolutions[0].CopyTo(ref Resolutions[3]);
            Resolutions[3].Backbuffer = new IntVector2(640, 360);

            // Set the default resolution
            Resolution = Resolutions[2];

            graphics.PreferredBackBufferWidth = Resolution.Backbuffer.X;
            graphics.PreferredBackBufferHeight = Resolution.Backbuffer.Y;
            graphics.SynchronizeWithVerticalRetrace = true;

            // Set the actual graphics device,
            // based on the resolution preferences established above.
#if PC_VERSION || WINDOWS
            if (rez.Custom)
            {
                if (!rez.Fullscreen)
                {
                    rez.Height = (int)((720f / 1280f) * rez.Width);
                }

                graphics.PreferredBackBufferWidth = rez.Width;
                graphics.PreferredBackBufferHeight = rez.Height;
                graphics.IsFullScreen = rez.Fullscreen;
            }
            else
            {
                graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                graphics.IsFullScreen = false;
            }
#if DEBUG || INCLUDE_EDITOR
            if (!graphics.IsFullScreen)
            {
                graphics.PreferredBackBufferWidth = 1280;
                graphics.PreferredBackBufferHeight = 720;
            }
#endif
#endif

            graphics.ApplyChanges();
            Window.Title = "Cloudberry Kingdom ";

            // Fill the pools
            ComputerRecording.InitPool();

            fps = 0;

            base.Initialize();
        }

#if NOT_PC && (XBOX || XBOX_SIGNIN)
        void SignedInGamer_SignedOut(object sender, SignedOutEventArgs e)
        {
            SaveGroup.SaveAll();

            if (Tools.CurGameData != null)
                Tools.CurGameData.OnSignOut(e);
        }

        void SignedInGamer_SignedIn(object sender, SignedInEventArgs e)
        {
            int Index = (int)e.Gamer.PlayerIndex;
            string Name = e.Gamer.Gamertag;

            PlayerData data = new PlayerData();
            data.Init(Index);
            PlayerManager.Players[Index] = data;

            SaveGroup.LoadGamer(Name, data);
        }
#endif

        public void ReloadInfo()
        {
            Tools.Write("Starting to load info...");
            var t = new System.Diagnostics.Stopwatch();
            t.Start();

            PieceQuad c;
            c = PieceQuad.Castle = new PieceQuad();
            c.Init(null, Tools.BasicEffect);
            c.Data.TopWidth = 133 * 1.8f;
            c.Data.LeftWidth = 625 * 1.4f;
            c.Data.RightWidth = 742 * 1.4f;
            c.Data.RepeatWidth = 578 * 1.4f;
            c.Data.RepeatHeight = 362 * 1.8f;
            c.Center.U_Wrap = c.Center.V_Wrap = true;
            c.Top.U_Wrap = true; c.Top.V_Wrap = false;
            c.Bottom.Hide = c.BL.Hide = c.BR.Hide = true;
            c.Data.UV_Multiples = new Vector2(1, 0);
            c.SetTexture("Castle");

            c = PieceQuad.Castle2 = new PieceQuad();
            c.Init(null, Tools.BasicEffect);
            c.Data.TopWidth = 133 * 1.8f;
            c.Data.LeftWidth = 625 * 1.4f;
            c.Data.RightWidth = 742 * 1.4f;
            c.Data.RepeatWidth = 578 * 1.4f;
            c.Data.RepeatHeight = 362 * 1.8f;
            c.Center.U_Wrap = c.Center.V_Wrap = true;
            c.Top.U_Wrap = true; c.Top.V_Wrap = false;
            c.Bottom.Hide = c.BL.Hide = c.BR.Hide = true;
            c.Data.UV_Multiples = new Vector2(1, 0);
            c.Data.Top_TR_Shift.Y = c.Data.TL_TR_Shift.Y = c.Data.TR_TR_Shift.Y =
            c.Data.Top_BL_Shift.Y = c.Data.TL_BL_Shift.Y = c.Data.TR_BL_Shift.Y =
            c.Data.Center_TR_Shift.Y = c.Data.Left_TR_Shift.Y = c.Data.Right_TR_Shift.Y = 130;
            c.SetTexture("Castle");

            // Dark tile
            PieceQuad.DarkPillars = new PieceQuadGroup();
            PieceQuad.DarkPillars.InitPillars("darkpillar");
            PieceQuad.DarkPillars.SetCutoffs(60, 135, 210, 290, 390, 520);
            foreach (PieceQuad piece in PieceQuad.DarkPillars)
            {
                piece.Data.Center_TR_Shift = new Vector2(23, 26);
                piece.Data.Center_BL_Shift = new Vector2(-23, -20);
            }

            // Island
            var p = PieceQuad.Islands = new PieceQuadGroup();
            PieceQuad.Islands.InitPillars("Outside_Platform", new string[] { "xxsmall", "xsmall", "medium", "large" });
            PieceQuad.Islands.SetCutoffs(80, 130, 290);
            p[0].FixedHeight = 66 * 1.6f;
            p[1].FixedHeight = 99 * 1.65f;
            p[2].FixedHeight = 180 * 1.7f;
            p[3].FixedHeight = 209 * 1.75f;


            // Catwalk
            c = PieceQuad.Catwalk = new PieceQuad();
            c.Init(null, Tools.BasicEffect);
            c.Data.TopWidth = 73;
            c.Data.LeftWidth = 102;
            c.Data.RightWidth = 102;
            c.Data.RepeatWidth = 129;
            c.Data.RepeatHeight = 362 * 1.8f;
            c.Data.Top_TR_Shift.Y = c.Data.TL_TR_Shift.Y = c.Data.TR_TR_Shift.Y = 1;// 10;
            c.Center.U_Wrap = c.Center.V_Wrap = true;
            c.Top.U_Wrap = true; c.Top.V_Wrap = false;
            c.Bottom.Hide = c.BL.Hide = c.BR.Hide = true;
            c.Center.Hide = c.Left.Hide = c.Right.Hide = true;
            c.Data.UV_Multiples = new Vector2(1, 0);
            c.SetTexture("ledge");

            c = PieceQuad.SpeechBubble = new PieceQuad();
            c.Data.RepeatWidth = 600;
            c.Data.RepeatHeight = 350;
            c.SetTexture("speechbubble_white");
            c.BL.MyTexture = "speechbubble_white_bottomleft2";
            c.Data.LeftWidth = c.Data.LeftWidth = c.Left.MyTexture.Tex.Width;
            c.Data.RightWidth = c.Data.RightWidth = c.Right.MyTexture.Tex.Width;
            c.Data.TopWidth = c.Data.TopWidth = c.Top.MyTexture.Tex.Width;
            c.Data.BottomWidth = c.Data.BottomWidth = c.Bottom.MyTexture.Tex.Width;
            c.Data.MiddleOnly = false;
            c.Data.UV_Multiples = new Vector2(1, 1);
            c.Top.U_Wrap = false;
            c.Bottom.U_Wrap = false;

            c = PieceQuad.SpeechBubbleRed = new PieceQuad();
            c.Data.RepeatWidth = 600;
            c.Data.RepeatHeight = 350;
            c.SetTexture("speechbubble_red");
            c.Data.LeftWidth = c.Data.LeftWidth = c.Left.MyTexture.Tex.Width;
            c.Data.RightWidth = c.Data.RightWidth = c.Right.MyTexture.Tex.Width;
            c.Data.TopWidth = c.Data.TopWidth = c.Top.MyTexture.Tex.Width;
            c.Data.BottomWidth = c.Data.BottomWidth = c.Bottom.MyTexture.Tex.Width;
            c.Data.MiddleOnly = false;
            c.Data.UV_Multiples = new Vector2(1, 1);
            c.Top.U_Wrap = false;
            c.Bottom.U_Wrap = false;

            // Moving block
            c = PieceQuad.MovingBlock = new PieceQuad();
            c.Data.CenterOnly = true;
            c.Center.MyTexture = "blue_small";

            // Falling block
            var Fall = new AnimationData_Texture("FallingBlock", 1, 4);
            PieceQuad.FallingBlock = new PieceQuad();
            PieceQuad.FallingBlock.Clone(PieceQuad.MovingBlock);
            PieceQuad.FallingBlock.Center.SetTextureAnim(Fall);
            PieceQuad.FallGroup = new BlockGroup();
            PieceQuad.FallGroup.Add(100, PieceQuad.FallingBlock);
            PieceQuad.FallGroup.SortWidths();

            // Bouncy block
            var Bouncy = new AnimationData_Texture("BouncyBlock", 1, 2);
            PieceQuad.BouncyBlock = new PieceQuad();
            PieceQuad.BouncyBlock.Clone(PieceQuad.MovingBlock);
            PieceQuad.BouncyBlock.Center.SetTextureAnim(Bouncy);
            PieceQuad.BouncyGroup = new BlockGroup();
            PieceQuad.BouncyGroup.Add(100, PieceQuad.BouncyBlock);
            PieceQuad.BouncyGroup.SortWidths();

            // Moving block
            PieceQuad.MovingGroup = new BlockGroup();
            PieceQuad.MovingGroup.Add(100, PieceQuad.MovingBlock);
            PieceQuad.MovingGroup.SortWidths();

            // Elevator
            PieceQuad.Elevator = new PieceQuad();
            PieceQuad.Elevator.Clone(PieceQuad.MovingBlock);
            PieceQuad.Elevator.Center.Set("palette");
            PieceQuad.Elevator.Center.SetColor(new Color(210, 210, 210));
            PieceQuad.ElevatorGroup = new BlockGroup();
            PieceQuad.ElevatorGroup.Add(100, PieceQuad.Elevator);
            PieceQuad.ElevatorGroup.SortWidths();


//#if INCLUDE_EDITOR
            //if (LoadDynamic)
            {
                //if (!AlwaysSkipDynamicArt)
                //    Tools.TextureWad.LoadAllDynamic(Content, EzTextureWad.WhatToLoad.Art);
                //Tools.TextureWad.LoadAllDynamic(Content, EzTextureWad.WhatToLoad.Backgrounds);
                //Tools.TextureWad.LoadAllDynamic(Content, EzTextureWad.WhatToLoad.Animations);
                //Tools.TextureWad.LoadAllDynamic(Content, EzTextureWad.WhatToLoad.Tilesets);
            }
//#endif

            t.Stop();
            Tools.Write("... done loading info!");
            Tools.Write("Total seconds: {0}", t.Elapsed.TotalSeconds);
        }

        protected void LoadMusic(bool CreateNewWad)
        {
            if (CreateNewWad)
                Tools.SongWad = new EzSongWad();

            Tools.SongWad.PlayerControl = Tools.SongWad.DisplayInfo = true;

            string path = Path.Combine(Globals.ContentDirectory, "Music");
            string[] files = Directory.GetFiles(path);

            foreach (String file in files)
            {
                int i = file.IndexOf("Music") + 5 + 1;
                if (i < 0) continue;
                int j = file.IndexOf(".", i);
                if (j <= i) continue;
                String name = file.Substring(i, j - i);
                String extension = file.Substring(j + 1);

                if (extension == "xnb")
                {
                    if (CreateNewWad)
                        Tools.SongWad.AddSong(Content.Load<Song>("Music\\" + name), name);
                    else
                        Tools.SongWad.FindByName(name).song = Content.Load<Song>("Music\\" + name);
                }

                ResourceLoadedCountRef.MyFloat++;
            }


            Tools.Song_Happy = Tools.SongWad.FindByName("Happy");
            Tools.Song_Happy.DisplayInfo = false;

            Tools.Song_140mph = Tools.SongWad.FindByName("140 Mph in the Fog^Blind Digital");
            Tools.Song_140mph.Volume = .7f;

            Tools.Song_BlueChair = Tools.SongWad.FindByName("Blue Chair^Blind Digital");
            Tools.Song_BlueChair.Volume = .7f;

            Tools.Song_Evidence = Tools.SongWad.FindByName("Evidence^Blind Digital");
            Tools.Song_Evidence.Volume = .7f;

            Tools.Song_GetaGrip = Tools.SongWad.FindByName("Get a Grip^Peacemaker");
            Tools.Song_GetaGrip.Volume = 1f;

            Tools.Song_House = Tools.SongWad.FindByName("House^Blind Digital");
            Tools.Song_House.Volume = .7f;

            Tools.Song_Nero = Tools.SongWad.FindByName("Nero's Law^Peacemaker");
            Tools.Song_Nero.Volume = 1f;

            Tools.Song_Ripcurl = Tools.SongWad.FindByName("Ripcurl^Blind Digital");
            Tools.Song_Ripcurl.Volume = .7f;

            Tools.Song_FatInFire = Tools.SongWad.FindByName("The Fat is in the Fire^Peacemaker");
            Tools.Song_FatInFire.Volume = .9f;

            Tools.Song_Heavens = Tools.SongWad.FindByName("The Heavens Opened^Peacemaker");
            Tools.Song_Heavens.Volume = 1f;

            Tools.Song_TidyUp = Tools.SongWad.FindByName("Tidy Up^Peacemaker");
            Tools.Song_TidyUp.Volume = 1f;

            Tools.Song_WritersBlock = Tools.SongWad.FindByName("Writer's Block^Peacemaker");
            Tools.Song_WritersBlock.Volume = 1f;

            // Create the standard playlist
            Tools.SongList_Standard.AddRange(Tools.SongWad.SongList);
            Tools.SongList_Standard.Remove(Tools.Song_Happy);
            Tools.SongList_Standard.Remove(Tools.Song_140mph);
        }

        protected void LoadSound(bool CreateNewWad)
        {
            ContentManager manager = new ContentManager(Content.ServiceProvider, Content.RootDirectory);

            if (CreateNewWad)
            {
                Tools.SoundWad = new EzSoundWad(4);
                Tools.PrivateSoundWad = new EzSoundWad(4);
            }

            string path = Path.Combine(Globals.ContentDirectory, "Sound");
            string[] files = Directory.GetFiles(path);
            foreach (String file in files)
            {
                int i = file.IndexOf("Sound") + 5 + 1;
                if (i < 0) continue;
                int j = file.IndexOf(".", i);
                if (j <= i) continue;
                String name = file.Substring(i, j - i);
                String extension = file.Substring(j + 1);

                if (extension == "xnb")
                {
                    if (CreateNewWad)
                        Tools.SoundWad.AddSound(manager.Load<SoundEffect>("Sound\\" + name), name);
                    else
                    {
                        SoundEffect NewSound = manager.Load<SoundEffect>("Sound\\" + name);

                        EzSound CurSound = Tools.SoundWad.FindByName(name);
                        foreach (EzSound ezsound in Tools.PrivateSoundWad.SoundList)
                            if (ezsound.sound == CurSound.sound)
                                ezsound.sound = NewSound;
                        CurSound.sound = NewSound;
                    }
                }

                ResourceLoadedCountRef.MyFloat++;
            }

            if (Tools.SoundContentManager != null)
            {
                Tools.SoundContentManager.Unload();
                Tools.SoundContentManager.Dispose();
            }

            Tools.SoundContentManager = manager;
        }

        protected void LoadAssets(bool CreateNewWads)
        {
            // Load the art!
            Tools.PreloadArt(Content);

            Tools.Write("Art done...");

            // Load the music!
            LoadMusic(CreateNewWads);
            Tools.Write("Music done...");

            // Load the sound!
            LoadSound(CreateNewWads);
            Tools.Write("Sound done...");
        }

        bool DoVideoTest = false;
        Video TestVideo;
        VideoPlayer VPlayer;
        Texture2D VTexture;
        EzTexture VEZTexture = new EzTexture();
        WrappedBool VBool;

        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;

            Tools.LoadBasicArt(Content);

            Tools.QDrawer = new QuadDrawer(device, 1000);
            Tools.QDrawer.DefaultEffect = Tools.EffectWad.FindByName("NoTexture");
            Tools.QDrawer.DefaultTexture = Tools.TextureWad.FindByName("White");

            Tools.Device = device;
            Tools.t = 0;

            LoadingResources = new WrappedBool(false);
            LoadingResources.MyBool = true;
            LogoScreenUp = true;

            Tools.spriteBatch = new SpriteBatch(GraphicsDevice);

            screenWidth = device.PresentationParameters.BackBufferWidth;
            screenHeight = device.PresentationParameters.BackBufferHeight;

            PresentationParameters pp = Tools.Device.PresentationParameters;
            ScreenShotRenderTarget = new RenderTarget2D(Tools.Device,
            pp.BackBufferWidth, pp.BackBufferHeight, false,
                                   pp.BackBufferFormat, pp.DepthStencilFormat, pp.MultiSampleCount,
                                   RenderTargetUsage.DiscardContents);



            MainCamera = new Camera(screenWidth, screenHeight);

            MainCamera.Update();

            // Create the initial loading screen
            FontLoad();
            LoadingScreen = new InitialLoadingScreen(Content, ResourceLoadedCountRef);


            if (DoVideoTest)
                VideoTest();



            // Load resource thread.
            Thread LoadThread = new Thread(
                new ThreadStart(
                    delegate
                    {
#if XBOX
                        Thread.CurrentThread.SetProcessorAffinity(new[] { 5 });
#endif
                        LoadThread = Thread.CurrentThread;

                        // Setup an abort, in case the game exits while loading.
                        EventHandler<EventArgs> abort = (s, e) =>
                        {
                            if (LoadThread != null)
                            {
                                LoadThread.Abort();
                            }
                        };
                        Tools.TheGame.Exiting += abort;

                        Tools.Write("Start");

                        Fireball.PreInit();

                        // Load art
                        LoadAssets(true);
                        if (!SimpleLoad)
                        {
                            Tools.TextureWad.LoadFolder(Content, "Tigar");
                        }

                        Tools.Write("ArtMusic done...");

                        // Load the infowad and boxes
                        Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
                        ReloadInfo();
                        Tools.Write("Infowad done...");

                        TileSets.Init();

                        Fireball.InitRenderTargets(device, device.PresentationParameters, 300, 200);

                        ParticleEffects.Init();

                        PlayerManager.Init();
                        Awardments.Init();
                        
                        // Load saved files
                        SaveGroup.Initialize();

#if NOT_PC && (XBOX || XBOX_SIGNIN)
                        SignedInGamer.SignedIn += new EventHandler<SignedInEventArgs>(SignedInGamer_SignedIn);
                        SignedInGamer.SignedOut += new EventHandler<SignedOutEventArgs>(SignedInGamer_SignedOut);
#endif


#if PC_VERSION
                        // Mouse pointer
                        MousePointer = new QuadClass();
                        MousePointer.Quad.MyTexture = Tools.TextureWad.FindByName("Hand_Open");
                        MousePointer.ScaleToTextureSize();
                        MousePointer.Scale(1.5f);

                        // Mouse back icon
                        MouseBack = new QuadClass();
                        MouseBack.Quad.MyTexture = Tools.TextureWad.FindByName("charmenu_larrow_1");
                        MouseBack.ScaleYToMatchRatio(40);
                        MouseBack.Quad.SetColor(new Color(255, 150, 150, 100));
#endif

                        Prototypes.LoadObjects();
                        ObjectIcon.InitIcons();

                        Tools.Write("Stickmen done...");

                        Tools.padState = new GamePadState[4];
                        Tools.PrevpadState = new GamePadState[4];
                        Tools.SetStandardRenderStates();

                        Tools.Write("Textures done...");

                        Console.WriteLine("Total resources: {0}", ResourceLoadedCountRef.MyFloat);

                        // Note that we are done loading.
                        LoadingResources.MyBool = false;
                        Tools.Write("Loading done!");

                        // Unregister from the game exiting.
                        Tools.TheGame.Exiting -= abort;
                    }))
            {
                Name = "LoadThread",
#if WINDOWS
                Priority = ThreadPriority.Highest,
#endif
            };

            LoadThread.Start();
        }

        /// <summary>
        /// Video test. Will be canned once proper video class is implemented.
        /// </summary>
        private void VideoTest()
        {
            if (DoVideoTest)
            {
                VBool = new WrappedBool(false);

                Thread VThread = null;
                VThread = new Thread(
                     new ThreadStart(
                         delegate
                         {
#if XBOX
                            Thread.CurrentThread.SetProcessorAffinity(new[] { 3 });
#endif
                             Tools.TheGame.Exiting += (o, e) =>
                             {
                                 if (VThread != null)
                                     VThread.Abort();
                             };

                             // Test load movie
                             //TestVideo = Content.Load<Video>("Movies//TestMovie");
                             TestVideo = Content.Load<Video>("Movies//TestCinematic");
                             VPlayer = new VideoPlayer();
                             VPlayer.IsLooped = false;
                             VPlayer.Play(TestVideo);

                             while (true)
                             {
                                 lock (VEZTexture)
                                 {
                                     VTexture = VPlayer.GetTexture();
                                     VEZTexture.Tex = VTexture;
                                 }
                             }
                         }))
                {
                    Name = "VideoThread",
#if WINDOWS
                    Priority = ThreadPriority.Lowest,
#endif
                };
                VThread.Start();
            }
        }

        /// <summary>
        /// Load the necessary fonts.
        /// </summary>
        private void FontLoad()
        {
            Tools.Font_Grobold42 = new EzFont("Fonts/Grobold_42", "Fonts/Grobold_42_Outline", -50, 40);
            Tools.Font_Grobold42_2 = new EzFont("Fonts/Grobold_42", "Fonts/Grobold_42_Outline2", -50, 40);

            Tools.LilFont = new EzFont("Fonts/LilFont");

            Tools.TheGame.DebugFont = Tools.LilFont.Font;

            Tools.Write("Fonts done...");
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        public bool RunningSlowly = false;
        static TimeSpan _TargetElapsedTime = new TimeSpan(0, 0, 0, 0, (int)(1000f / 60f));
        protected override void Update(GameTime gameTime)
        {
            //VPlayer.IsLooped = false;
            //if (VPlayer != null && (Tools.PhsxCount % 60 == 0 || VPlayer.State == MediaState.Stopped))
            //{
            //    //VPlayer.IsLooped = true;
            //    VPlayer.Play(TestVideo);
            //}

            //if (VPlayer1 != null)
            //    Console.WriteLine(string.Format("! {0} {1}", VPlayer1.PlayPosition.Ticks, VPlayer2.PlayPosition.Ticks));
            //if (VPlayer1 != null
            //    && VPlayer1.PlayPosition.Ticks >= 55130000)//155100000)
            //{
            //    //&& VPlayer.PlayPosition.TotalMilliseconds == 0)
            //    VPlayer1.Pause();
            //    Tools.Swap(ref VPlayer1, ref VPlayer2);
            //    VPlayer1.Resume();
            //    lock (VBool)
            //    {
            //        VBool.MyBool = true;
            //    }
            //}

            /*
            if (Tools.keybState.IsKeyDown(Keys.D6))
            {
                VPlayer1.Resume();
            }
            if (Tools.keybState.IsKeyDown(Keys.D7))
            {
                VPlayer1.Stop();
                VPlayer1.Play(TestVideo1);
            }*/

            //graphics.SynchronizeWithVerticalRetrace = false;
            //graphics.SynchronizeWithVerticalRetrace = true;
            //graphics.IsFullScreen = false;

            //this.TargetElapsedTime = _TargetElapsedTime;
            //this.TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 100);
            //this.TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 10);
            //this.TargetElapsedTime = new TimeSpan(TimeSpan.TicksPerSecond / 60);

            this.IsFixedTimeStep = Tools.FixedTimeStep;

            RunningSlowly = gameTime.IsRunningSlowly;
            base.Update(gameTime);
        }

        void DoQuickSpawn()
        {
            if (Tools.CurLevel.ResetEnabled() && Tools.CurLevel.PlayMode == 0 && !Tools.CurLevel.Watching && !Tools.CurGameData.PauseGame && Tools.CurGameData.QuickSpawnEnabled())
            {
                // Note that quickspawn was used, hence we should not give hints to the player to use Quick Spawn in the future.
                Hints.QuickSpawn = 999;

                // Don't count reset against player if it happens within the first half second
                if (Tools.CurLevel.CurPhsxStep < 30)
                    Tools.CurLevel.FreeReset = true;

                // Update player stats
                Tools.CurLevel.CountReset();

                Tools.CurLevel.SetToReset = true;
            }
        }

        /// <summary>
        /// The current game being played.
        /// </summary>
        GameData Game { get { return Tools.CurGameData; } }

        /// <summary>
        /// Update the current game.
        /// </summary>
        void DoGameDataPhsx()
        {
#if INCLUDE_EDITOR
            if (Tools.EditorPause) return;
#endif
            Tools.PhsxCount++;

            if (Tools.WorldMap != null)
                Tools.WorldMap.BackgroundPhsx();

            if (Game != null)
            {
                for (int i = 0; i < Game.PhsxStepsToDo; i++)
                {
                    EzSoundWad.SuppressSounds = (Game.SuppressSoundForExtraSteps && i < Game.PhsxStepsToDo - 1);
                    Game.PhsxStep();
                }
                Game.PhsxStepsToDo = 1;
            }
        }

        /// <summary>
        /// A list of actions to perform. Each time an action is peformed it is removed from the list.
        /// </summary>
        public List<Action> ToDo = new List<Action>();

        private void DoToDoList()
        {
            foreach (Action todo in ToDo)
                todo();
            ToDo.Clear();
        }

        protected void PhsxStep()
        {
            DoToDoList();

#if WINDOWS
            // Save the current keyboard state.
            if (Tools.PrevKeyboardState == null) Tools.PrevKeyboardState = Tools.keybState;

            // Debug tools
#if PC_DEBUG || (WINDOWS && DEBUG) || INCLUDE_EDITOR
            if (!Tools.ViewerIsUp && !KeyboardExtension.Freeze)
            {
                if (DebugModePhsx())
                    return;
            }
#endif

            // Do game update.
            if (!Tools.StepControl || (Tools.keybState.IsKeyDownCustom(Keys.Enter) && !Tools.PrevKeyboardState.IsKeyDownCustom(Keys.Enter)))
            {
                DoGameDataPhsx();
            }
            else if (Tools.CurLevel != null)
                Tools.CurLevel.IndependentDeltaT = 0;

            // Quick Spawn
            CheckForQuickSpawn_PC();

            // Determine if the mouse is in the window or not.
            Tools.MouseInWindow =
                Tools.CurMouseState.X > 0 && Tools.CurMouseState.X < Resolution.Backbuffer.X &&
                Tools.CurMouseState.Y > 0 && Tools.CurMouseState.Y < Resolution.Backbuffer.Y;

            // Calculate how much user has scrolled the mouse wheel and moved the mouse.
            Tools.DeltaScroll = Tools.CurMouseState.ScrollWheelValue - Tools.PrevMouseState.ScrollWheelValue;
            Tools.DeltaMouse = Tools.ToWorldCoordinates(new Vector2(Tools.CurMouseState.X, Tools.CurMouseState.Y), Tools.CurLevel.MainCamera) -
                               Tools.ToWorldCoordinates(new Vector2(Tools.PrevMouseState.X, Tools.PrevMouseState.Y), Tools.CurLevel.MainCamera);
            Tools.RawDeltaMouse = new Vector2(Tools.CurMouseState.X, Tools.CurMouseState.Y) -
                                  new Vector2(Tools.PrevMouseState.X, Tools.PrevMouseState.Y);

            Tools.PrevKeyboardState = Tools.keybState;
            Tools.PrevMouseState = Tools.CurMouseState;
#else
            DoGameDataPhsx();
#endif

            // Quick Spawn: Note, we must check this for PC version too, since PC players may use game pads.
            CheckForQuickSpawn_Xbox();

            // Store the previous states of the Xbox controllers.
            for (int i = 0; i < 4; i++)
                if (Tools.PrevpadState[i] != null)
                    Tools.PrevpadState[i] = Tools.padState[i];

            // Update the fireball textures.
            Fireball.TexturePhsx();
        }

        private void CheckForQuickSpawn_PC()
        {
            // Should implement a GameObject that marshalls quickspawns instead.
            Tools.Warning();

            if (!Tools.ViewerIsUp && !KeyboardExtension.Freeze && Tools.CurLevel.ResetEnabled() &&
                Tools.keybState.IsKeyDownCustom(ButtonCheck.Quickspawn_KeyboardKey.KeyboardKey) && !Tools.PrevKeyboardState.IsKeyDownCustom(ButtonCheck.Quickspawn_KeyboardKey.KeyboardKey))
                DoQuickSpawn();
        }

        private void CheckForQuickSpawn_Xbox()
        {
            // Check for quick spawn on Xbox. This allows the player to reset a level rapidly.
            // For XBox this is done by holding both shoulder buttons.
            bool ShortReset = false;
            for (int i = 0; i < 4; i++)
            {
                if (PlayerManager.Get(i).Exists)
                {
                    if (Tools.padState[i].Buttons.LeftShoulder == ButtonState.Pressed && Tools.padState[i].Buttons.RightShoulder == ButtonState.Pressed &&
                        (Tools.PrevpadState[i].Buttons.LeftShoulder != ButtonState.Pressed
                         || Tools.PrevpadState[i].Buttons.RightShoulder != ButtonState.Pressed))
                        ShortReset = true;
                }
            }

            // Do the quick spawn if it was chosen by the player.
            if (ShortReset)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (PlayerManager.Get(i).Exists && PlayerManager.Get(i).IsAlive)
                    {
                        if (Tools.padState[i].Buttons.LeftShoulder != ButtonState.Pressed &&
                            Tools.padState[i].Buttons.RightShoulder != ButtonState.Pressed)
                            ShortReset = false;
                    }
                }

                if (ShortReset && Tools.CurLevel.ResetEnabled())
                    DoQuickSpawn();
            }
        }

#if WINDOWS
        /// <summary>
        /// Whether the mouse should be allowed to be shown, usually only when a menu is active.
        /// </summary>
        public bool ShowMouse = false;

        /// <summary>
        /// Whether the user is using the mouse. False when the mouse hasn't been used since the arrow keys.
        /// </summary>
        public bool MouseInUse = false;
        public bool PrevMouseInUse = false;

        /// <summary>
        /// Draw the mouse cursor.
        /// </summary>
        void MouseDraw()
        {
            if (!MouseInUse) return;
            if (MousePointer == null) return;

            Vector2 Pos = Tools.MouseWorldPos();

            // Draw the mouse hand
            MousePointer.Pos = Pos + new Vector2(.905f * MousePointer.Size.X, -.705f * MousePointer.Size.Y);
            MousePointer.Draw();

            // Draw the mouse dot
            Tools.QDrawer.DrawSquareDot(Pos, Color.Black, 8);

            // Draw the mouse back icon
            if (DrawMouseBackIcon)
            {
                MouseBack.Pos = MousePointer.Pos + new Vector2(44, 98);
                MouseBack.Draw();
            }

            Tools.QDrawer.Flush();
        }

        /// <summary>
        /// Update the boolean flag MouseInUse
        /// </summary>
        void UpdateMouseUse()
        {
            if (Tools.keybState.IsKeyDownCustom(Keys.Up) ||
                Tools.keybState.IsKeyDownCustom(Keys.Down) ||
                Tools.keybState.IsKeyDownCustom(Keys.Left) ||
                Tools.keybState.IsKeyDownCustom(Keys.Right) ||
                Tools.keybState.IsKeyDownCustom(ButtonCheck.Up_Secondary) ||
                Tools.keybState.IsKeyDownCustom(ButtonCheck.Down_Secondary) ||
                Tools.keybState.IsKeyDownCustom(ButtonCheck.Left_Secondary) ||
                Tools.keybState.IsKeyDownCustom(ButtonCheck.Right_Secondary) ||
#if PC_VERSION
                (PlayerManager.Players != null && PlayerManager.Player != null && ButtonCheck.GetMaxDir(false).Length() > .3f)
#else
                (PlayerManager.Players != null && ButtonCheck.GetMaxDir(true).Length() > .3f)
#endif
)
                MouseInUse = false;

            if (Tools.DeltaMouse != Vector2.Zero ||
                Tools.CurMouseState.LeftButton == ButtonState.Pressed ||
                Tools.CurMouseState.RightButton == ButtonState.Pressed)
                MouseInUse = true;

            PrevMouseInUse = MouseInUse;
        }
#endif

        /// <summary>
        /// Whether a song was playing prior to the game window going inactive
        /// </summary>
        public bool MediaPlaying_HoldState = false;

        /// <summary>
        /// Whether this is the first frame the window has been inactive
        /// </summary>
        public bool FirstInactiveFrame = true;

        /// <summary>
        /// Whether this is the first frame the window has been active
        /// </summary>
        public bool FirstActiveFrame = true;

        public Viewport MainViewport;
        public float SpriteScaling = 1f;

        public double DeltaT = 0;
        protected override void Draw(GameTime gameTime)
        {
#if DEBUG_OBJDATA
            ObjectData.UpdateWeak();
#endif
            DeltaT = gameTime.ElapsedGameTime.TotalSeconds;


            // Set the viewport to the whole screen
            GraphicsDevice.Viewport = new Viewport
            {
                X = 0,
                Y = 0,
                Width = GraphicsDevice.PresentationParameters.BackBufferWidth,
                Height = GraphicsDevice.PresentationParameters.BackBufferHeight,
                MinDepth = 0,
                MaxDepth = 1
            };

            // Clear whole screen to black
            GraphicsDevice.Clear(Color.Black);

#if WINDOWS
            if (!ActiveInactive())
                return;
#endif

            // Make the actual view port we draw to, and clear it
            MakeInnerViewport();
            GraphicsDevice.Clear(Color.Black);

            graphics.GraphicsDevice.Viewport = MainViewport;


            Tools.DrawCount++;

            if (LogoScreenUp) LogoPhsx();
            else if (LogoScreenPropUp) LoadingScreen.PhsxStep();
#if WINDOWS
            if (Tools.Dlg != null || Tools.DialogUp) return;
#endif

            bool DrawBool = true;
#if PC_VERSION
#elif XBOX || XBOX_SIGNIN
            DrawBool = !Guide.IsVisible;
#endif

            bool DoShit = true;
            bool DrawShit = true;

            if (!LogoScreenUp)
                if (!Tools.CurGameData.Loading)
                    if (DrawBool)
                    {
                        if (DoShit)
                        {
                            // Update controller/keyboard states
                            UpdateControllerAndKeyboard();

                            // Update sounds
                            if (!LogoScreenUp)
                                Tools.SoundWad.Update();

                            // Update songs
                            if (Tools.SongWad != null)
                                Tools.SongWad.PhsxStep();
                        }

                        // Track time, changes in time, and FPS
                        Tools.gameTime = gameTime;
                        DrawCount++;

                        float new_t = (float)gameTime.TotalGameTime.TotalSeconds;
                        Tools.dt = new_t - Tools.t;
                        Tools.t = new_t;

                        fps = .3f * fps + .7f * (1000f / (float)Math.Max(.00000231f, gameTime.ElapsedGameTime.TotalMilliseconds));

                        if (DoShit)
                        {
                            // Determine how many phsx steps to take
                            int Reps = 0;
                            if (Tools.PhsxSpeed == 0 && DrawCount % 2 == 0) Reps = 1;
                            else if (Tools.PhsxSpeed == 1) Reps = 1;
                            else if (Tools.PhsxSpeed == 2) Reps = 2;
                            else if (Tools.PhsxSpeed == 3) Reps = 4;

                            // Do the phsx
                            for (int i = 0; i < Reps; i++)
                            {
                                PhsxCount++;
                                PhsxStep();
                            }
                        }
                    }

            // Setup the rendering parameters
            SetupToRender();

            //if (true)
            if (LogoScreenUp || LogoScreenPropUp)
            {
                LoadingScreen.Draw();
                if (DoVideoTest)
                {
                    VideoTest_Draw(Vector2.Zero);
                    Tools.QDrawer.Flush();
                }
                return;
            }
            if (DoVideoTest)
                VideoTest_Draw();
            
            if (DrawShit)
            if (Tools.ShowLoadingScreen)
            {
                Tools.CurrentLoadingScreen.PreDraw();
                Tools.CurrentLoadingScreen.Draw(MainCamera);
                return;
            }
            else
            {
                if (Tools.CurGameData != null)
                {
                    Tools.CurGameData.Draw();
                    Tools.CurGameData.PostDraw();
                }
                else
                    GraphicsDevice.Clear(Color.Black);
            }

            if (DoVideoTest)
                VideoTest_Draw();

#if DEBUG
            if (BuildDebug || ShowFPS || Tools.ShowNums)
                DrawDebugInfo();
#endif

            if (Tools.CurLevel != null)
            {
                Tools.QDrawer.Flush();
                Tools.StartGUIDraw();
            }

#if PC_VERSION
            if (!Tools.ShowLoadingScreen && ShowMouse && !Tools.CapturingVideo)
                MouseDraw();
            ShowMouse = false;
#endif

            base.Draw(gameTime);

            if (!Tools.CapturingVideo)
                if (Tools.SongWad != null)
                    Tools.SongWad.Draw();

            if (Tools.CurLevel != null)
            {
                Tools.EndGUIDraw();
                Tools.QDrawer.Flush();
            }

#if DEBUG && INCLUDE_EDITOR
            if (Tools.background_viewer != null)
                Tools.background_viewer.Draw();
#endif
        }

        private void VideoTest_Draw()
        {
            VideoTest_Draw(Tools.CurCamera.Pos);
        }
        private void VideoTest_Draw(Vector2 pos)
        {
            if (DoVideoTest)
                lock (VEZTexture)
                {
                    if (VEZTexture.Tex != null)
                        Tools.QDrawer.DrawToScaleQuad(pos, Color.White, 2400, VEZTexture, Tools.BasicEffect);
                }
        }

        private void SetupToRender()
        {
            Vector4 cameraPos = new Vector4(MainCamera.Data.Position.X, MainCamera.Data.Position.Y, MainCamera.Zoom.X, MainCamera.Zoom.Y);//.001f, .001f);

            Tools.SetStandardRenderStates();

            Tools.QDrawer.SetInitialState();
            ComputeFire();

            Tools.EffectWad.SetCameraPosition(cameraPos);

            Tools.SetDefaultEffectParams(MainCamera.AspectRatio);

            Tools.SetStandardRenderStates();
        }

        private void ComputeFire()
        {
            if (!LogoScreenUp)
            {
                if (!Tools.CurGameData.Loading && Tools.CurLevel.PlayMode == 0 && Tools.CurGameData != null && !Tools.CurGameData.Loading && (!Tools.CurGameData.PauseGame || CharacterSelectManager.IsShowing))
                {
                    // Compute fireballs textures
                    device.BlendState = BlendState.Additive;
                    Fireball.DrawFireballTexture(device, Tools.EffectWad);
                    Fireball.DrawEmitterTexture(device, Tools.EffectWad);
                    
                    device.BlendState = BlendState.AlphaBlend;
                }
            }
        }

        private void UpdateControllerAndKeyboard()
        {
            // Update controller/keyboard states
            if (!LogoScreenUp)
            {
#if WINDOWS
                Tools.keybState = Keyboard.GetState();
                Tools.CurMouseState = Mouse.GetState();
#endif
                Tools.padState[0] = GamePad.GetState(PlayerIndex.One);
                Tools.padState[1] = GamePad.GetState(PlayerIndex.Two);
                Tools.padState[2] = GamePad.GetState(PlayerIndex.Three);
                Tools.padState[3] = GamePad.GetState(PlayerIndex.Four);

                ButtonStats.Update();

                Tools.UpdateVibrations();
            }

#if PC_VERSION
                UpdateMouseUse();
#endif
        }

#if WINDOWS
        private bool ActiveInactive()
        {
            if (!this.IsActive)
            {
                // The window isn't active, so
                // show the actual mouse (not our custom drawn mouse)
                this.IsMouseVisible = true;

                if (FirstInactiveFrame)
                {
                    // If a song is playing, stop it,
                    // and note that we should resume once the window becomes active
                    if (Tools.SongWad != null && Tools.SongWad.IsPlaying())
                    {
                        MediaPlaying_HoldState = true;
                        MediaPlayer.Pause();
                    }

                    FirstInactiveFrame = false;
                }

                FirstActiveFrame = true;

                return false;
            }
            else
            {
                // The window is active, so
                // hide the actual mouse (we draw our own custom mouse in game)
                this.IsMouseVisible = false;

                if (FirstActiveFrame)
                {
                    // If a song was playing previously when the window was active before,
                    // resume that song
                    if (MediaPlaying_HoldState)
                        MediaPlayer.Resume();

                    FirstActiveFrame = false;
                }

                FirstInactiveFrame = true;

                // If we are editing the background show the mouse
                if (Tools.ViewerIsUp)
                    this.IsMouseVisible = true;

                return true;
            }
        }
#endif

        /// <summary>
        /// If the aspect ratio of the game (1280:720) doesn't match the window, use a letterbox viewport.
        /// </summary>
        public void MakeInnerViewport()
        {
            float targetAspectRatio = 1280f / 720f;
            // figure out the largest area that fits in this resolution at the desired aspect ratio
            int width = GraphicsDevice.PresentationParameters.BackBufferWidth;
            SpriteScaling = width / 1280f;
            int height = (int)(width / targetAspectRatio + .5f);
            if (height > GraphicsDevice.PresentationParameters.BackBufferHeight)
            {
                height = GraphicsDevice.PresentationParameters.BackBufferHeight;
                width = (int)(height * targetAspectRatio + .5f);
            }

            // set up the new viewport centered in the backbuffer
            MainViewport = GraphicsDevice.Viewport = new Viewport
            {
                X = GraphicsDevice.PresentationParameters.BackBufferWidth / 2 - width / 2,
                Y = GraphicsDevice.PresentationParameters.BackBufferHeight / 2 - height / 2,
                Width = width,
                Height = height,
                MinDepth = 0,
                MaxDepth = 1
            };
        }
    }
}