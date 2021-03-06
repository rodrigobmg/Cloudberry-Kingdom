using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

using CloudberryKingdom.Levels;

namespace CloudberryKingdom
{
    public class Challenge_Freeplay : Challenge
    {
        static readonly Challenge_Freeplay instance = new Challenge_Freeplay();
        public static new Challenge_Freeplay Instance { get { return instance; } }

        protected Challenge_Freeplay()
        {
            GameTypeId = -1;
            MenuName = Name = Localization.Words.FreePlay;

			SetGameId();
        }
        
        public override LevelSeedData GetSeed(int Index)
        {
            throw new NotImplementedException();
        }
    }

    public class CustomLevel_GUI : CkBaseMenu
    {
        public static List<TileSet> FreeplayTilesets = new List<TileSet>();
        public static List<BobPhsx> FreeplayHeroes = new List<BobPhsx>();

        public static bool IsMaxLength = false;
        public static int Difficulty = 0;

        /// <summary>
        /// This is the level seed being edited.
        /// </summary>
        public LevelSeedData LevelSeed;
        public PieceSeedData PieceSeed;

        public ObjectIcon HeroIcon, MiniCheckpoint;

        static Localization.Words CustomHeroString = Localization.Words.Factory;
        
        public CustomLevel_GUI()
        {
            BobPhsx.CustomPhsxData.InitBounds();

            CustomHero_GUI.HeroPhsxData = new BobPhsx.CustomPhsxData();
            CustomHero_GUI.HeroPhsxData.Init();

            NoBackIfNoCaller = true;

            SeedStringToLoad = null;
            ExitFreeplay = false;
        }

        public void StartLevelFromMenuData()
        {
            LevelSeedData data = new LevelSeedData(LevelSeed);
            data.Seed = Tools.GlobalRnd.Rnd.Next();

            // Debug: set hero type
            //data.DefaultHeroType = BobPhsx.MakeCustom(Hero_BaseType.Spaceship, Hero_Shape.Small, Hero_MoveMod.Classic);

            // Vertical levels have no checkpoints
            if (LevelSeed.MyGeometry != LevelGeometry.Right)
                LevelSeed.NumPieces = 1;

            // Copy Upgrade1 to Upgrade2
            PieceSeed.MyUpgrades1.UpgradeLevels.CopyTo(PieceSeed.MyUpgrades2.UpgradeLevels, 0);

            // Custom difficulty
            if (IsCustomDifficulty())
            {
                data.Initialize(piece =>
                {
                    piece.CopyUpgrades(PieceSeed);

                    piece.StandardClose();
                });
            }
            // Preset difficulty
            else
            {
                int diff = new int[] { 0, 2, 4, 6, 9 }[DiffList.ListIndex];
                LevelSeedData.CustomDifficulty custom;
                
                if (data.DefaultHeroType is BobPhsxSpaceship)
                    custom = SpaceshipLevel.StandardPieceMod(diff, data);
                else
                    custom = RegularLevel.FixedPieceMod(DiffList.ListIndex - 1, data);

                LevelSeedData.CustomDifficulty modcustom = p =>
                {
                    custom(p);

                    p.StandardClose();
                };

                data.Initialize(custom);
            }

            // Wall
            if (HasWall) data.HasWall = true;

            data.ProcessSpecial();

            // Dark bottom
            if (data.MyGeometry == LevelGeometry.Down)
                data.PieceSeeds[0].Style.MyFinalPlatsType = StyleData.FinalPlatsType.DarkBottom;

            data.PostMake += data.PostMake_StandardLoad;

            StartLevel(data);
        }

        public void StartLevel(LevelSeedData data)
        {
			HelpMenu.CostMultiplier = 1;
			HelpMenu.SetCostGrowthType(HelpMenu.CostGrowthTypes.None);

			Tools.PlayHappyMusic(MyGame);

            data.PostMake += data.PostMake_EnableLoad;

            PlayerManager.CoinsSpent = -999;

            GameData game = data.Create();
            game.ParentGame = MyGame;
            game.Freeplay = true;
            game.DefaultHeroType = data.DefaultHeroType;
            Difficulty = DiffList.ListIndex - 1;
            IsMaxLength = length.IsMaxed;
        }

        public override void OnAdd()
        {
			CloudberryKingdomGame.SetPresence(CloudberryKingdomGame.Presence.Freeplay);

            base.OnAdd();

            MyGame.ClearPreviousLoadFunction();

            MyPile.Pos += RightPanelCenter;
            MyMenu.FancyPos.RelVal += RightPanelCenter;

            // Register for when the created level ends
            MyGame.OnReturnTo += OnReturnFromLevel;

            SetPos();

			if (Localization.CurrentLanguage.MyLanguage == Localization.Language.German)
			{
				foreach (MenuItem item in MyMenu.Items)
				{
					MenuList mlist = item as MenuList;
					if (null != mlist)
					{
						foreach (MenuItem _item in mlist.MyList)
						{
							_item.MyText.Scale = .6f;
							_item.MySelectedText.Scale = .6f;
						}
					}
				}
			}
        }

        protected override void ReleaseBody()
        {
            // Unregister 
            MyGame.OnReturnTo -= OnReturnFromLevel;

            base.ReleaseBody();
        }

        void OnReturnFromLevel()
        {
            if (ExitFreeplay)
            {
                if (CallingPanel != null)
                {
                    SlideOut(PresetPos.Left);
                    CallingPanel.SlideOut(PresetPos.Left);
                    CallingPanel.ReleaseWhenDone = true;
                }

                MenuReturnToCaller(MyMenu);
                MyGame.PhsxStepsToDo += 20;
                ExitFreeplay = false;
                return;
            }

            // If we started the level from the basic menu
            if (CallingPanel == null)
            {
                if (Active)
                {
                    SlideOut(PresetPos.Right, 0);
                    SlideIn();
                }
            }
            // If we started the level from the obstacle upgrade menu
            else
            {
                SlideOut(PresetPos.Right, 0);
                CallingPanel.SlideOut(PresetPos.Right, 0);
                CallingPanel.SlideIn();
                Active = false;
            }
        }

        void AnyHero()
        {
            foreach (var item in HeroList.MyList)
                item.Selectable = true;
        }

        void UpHero_ModShown()
        {
            foreach (var item in HeroList.MyList)
            {
                if (item.MyObject is BobPhsxSpaceship || item.MyObject is BobPhsxRocketbox)
                    item.Selectable = false;
                else
                    item.Selectable = true;
            }
        }

        void UpHero_NoSpaceship()
        {
            if (LevelSeed.DefaultHeroType == BobPhsxSpaceship.Instance ||
                LevelSeed.DefaultHeroType == BobPhsxRocketbox.Instance)
            {
                int HoldIndex = DesiredHeroIndex;
                HeroList.SetIndex(1);
                if (HeroList.Show)
                    DesiredHeroIndex = HoldIndex;
            }
        }

        void ShowHeros(bool Show)
        {
            HeroList.Include = Show;
            HeroText.Show = Show;

            if (Show)
                HeroList.SetIndex(DesiredHeroIndex);
            else
            {
                int HoldIndex = DesiredHeroIndex;
                HeroList.SetIndex(0);
                DesiredHeroIndex = HoldIndex;
            }
        }

        float HoldNumCheckpoints = 1;
        int HoldDesiredNumCheckpoints;
        void ShowCheckpoints(bool Show)
        {
			Show = !NoCheckpoints_Rocketbox && !NoCheckpoints_WallLevel;

            if (Show)
            {
                if (!checkpoints.Show)
                {
                    checkpoints.Val = HoldNumCheckpoints;
                    DesiredNumCheckpoints = HoldDesiredNumCheckpoints;
                }
            }
            else
            {
				if (checkpoints.Show)
				{
					HoldNumCheckpoints = checkpoints.Val;
					HoldDesiredNumCheckpoints = DesiredNumCheckpoints;
					checkpoints.Val = 0;
					DesiredNumCheckpoints = 0;
				}
            }

            checkpoints.Show = Show;
            CheckpointsText.Show = Show;
        }

        bool HasWall = false;
        void SelectNormal()
        {
            HasWall = false;

            AnyHero();
            ShowHeros(true);
            
            ShowCheckpoints(true);
        }

        void SelectBuild()
        {
            HasWall = false;

            ShowHeros(false);
            ShowCheckpoints(true);
        }

        void SelectBungee()
        {
            SelectNormal();
        }

        void SelectSurvival()
        {
            SelectNormal();
            ShowCheckpoints(false);
        }

        bool LeftJustify = true;
        float LeftJustifyAddX = -400;

        //static Vector2 RightPanelCenter = new Vector2(-410, 75);
        static Vector2 RightPanelCenter = new Vector2(-285, 0);
        LengthSlider length;
        MenuSliderBase checkpoints;
        MenuItem Start;
        MenuList HeroList, DiffList;
        Text HeroText, CheckpointsText;
        public override void Init()
        {
            ItemShadows = false;

            LevelSeed = new LevelSeedData();
            PieceSeed = new PieceSeedData(null);

            SlideInFrom = SlideOutTo = PresetPos.Right;

            FontScale = .73f;

            MyPile = new DrawPile();

            base.Init();

            ReturnToCallerDelay = 18;
            SlideInLength = 25;
            SlideOutLength = 24;

            CallDelay = 5;


            SelectedItemShift = new Vector2(0, 0);

            // Backdrop
            QuadClass backdrop;

            backdrop = new QuadClass("Backplate_1500x900", 1500, true);
            backdrop.Name = "Backdrop";
            MyPile.Add(backdrop);
			EpilepsySafe(.5f);
            backdrop.Size = new Vector2(1690.477f, 1115.617f);
            backdrop.Pos = new Vector2(287.6977f, 51.58758f);

            // Make the menu
            MyMenu = new Menu(false);

            Control = -1;

            MyMenu.OnB = null;

            MenuItem item;

            // Location screenshot
            //QuadClass Screenshot = new QuadClass();
            //Screenshot.Name = "Screenshot";
            //Screenshot.SetToDefault();
            //Screenshot.TextureName = "Screenshot_Terrace";
            //Screenshot.ScaleYToMatchRatio(500);
            //MyPile.Add(Screenshot);
            //Screenshot.Pos = new Vector2(1340.002f, 497.2222f);
            //Screenshot.SetDefaultShadow(15);

            // Location
            Text LocationText = new Text(Localization.Words.Location, ItemFont);
            LocationText.Name = "Location";
            SetHeaderProperties(LocationText);
            MyPile.Add(LocationText);
            LocationText.Pos = new Vector2(-1050.111f, 933f);
            
            MenuList LocationList = new MenuList();
            LocationList.Name = "Location";
            LocationList.Center = !LeftJustify;
			LocationList.MyExpandPos = new Vector2(-283.0556f, 634.9995f);
            foreach (TileSet tileset in FreeplayTilesets)
            {
                item = new MenuItem(new Text(tileset.NameInGame, ItemFont, false, true));
                SetItemProperties(item);
                LocationList.AddItem(item, tileset);
            }
            AddItem(LocationList);
            if (LeftJustify)
                LocationList.Pos = new Vector2(200f + LeftJustifyAddX, 828f);
            else
                LocationList.Pos = new Vector2(200f, 828f);
            LocationList.OnIndexSelect = () =>
                {
                    TileSet tileset = LocationList.CurObj as TileSet;

                    //Vector2 HoldRelativeSize = Screenshot.GetTextureScaling();
                    //Screenshot.TextureName = tileset.ScreenshotString;
                    //Screenshot.ScaleYToMatchRatio(HoldRelativeSize.X * Screenshot.Quad.MyTexture.Width);

                    LevelSeed.SetTileSet(tileset);
                };
            LocationList.SetIndex(0);

            // Game type
            Text GameText = new Text(Localization.Words.Game, ItemFont);
            GameText.Name = "Game";
            SetHeaderProperties(GameText);
            MyPile.Add(GameText);
            GameText.Pos = new Vector2(-1061.11f, 933f - 222f);
            
            MenuList GameList = new MenuList();
            GameList.Name = "Game";
			GameList.MyExpandPos = new Vector2(-271.1111f, 447.4991f);
            GameList.Center = !LeftJustify;
            Localization.Words[] GameNames;
            if (PlayerManager.NumPlayers <= 1)
				//GameNames = new Localization.Words[] { Localization.Words.ClassicGame, Localization.Words.WallLevel, Localization.Words.Masochistic };
				GameNames = new Localization.Words[] { Localization.Words.ClassicGame, Localization.Words.WallLevel };
            else
                //GameNames = new Localization.Words[] { Localization.Words.ClassicGame, Localization.Words.Bungee, Localization.Words.WallLevel, Localization.Words.Masochistic };
				GameNames = new Localization.Words[] { Localization.Words.ClassicGame, Localization.Words.Bungee, Localization.Words.WallLevel };
            foreach (Localization.Words name in GameNames)
            {
                item = new MenuItem(new Text(name, ItemFont, false, true));
                SetItemProperties(item);
                GameList.AddItem(item, name);
            }
            AddItem(GameList);
            if (LeftJustify)
                GameList.Pos = new Vector2(117 + LeftJustifyAddX, 828f - 222f);
            else
                GameList.Pos = new Vector2(117, 828f - 222f);
            GameList.OnIndexSelect = () =>
                {
					NoCheckpoints_WallLevel = false;

                    LevelSeed.MyGameFlags.SetToDefault();
                    LevelSeed.Masochistic = false;

                    Localization.Words gamename = (Localization.Words)GameList.CurObj;
                    if (gamename == Localization.Words.ClassicGame)
                    {
                        LevelSeed.MyGameType = NormalGameData.Factory;
                        LevelSeed.MyGeometry = LevelGeometry.Right;
                        SelectNormal();
                    }
                    else if (gamename == Localization.Words.Bungee)
                    {
                        LevelSeed.MyGameType = NormalGameData.Factory;
                        LevelSeed.MyGeometry = LevelGeometry.Right;
                        LevelSeed.MyGameFlags.IsTethered = true;
                        SelectBungee();
                    }
                    //else if (gamename.CompareTo("Up Level") == 0)
                    //{
                    //    SelectUpLevel();
                    //}
                    //else if (gamename.CompareTo("Down Level") == 0)
                    //{
                    //    SelectDownLevel();
                    //}
                    else if (gamename == Localization.Words.WallLevel)
                    {
						NoCheckpoints_WallLevel = true;

                        LevelSeed.MyGameType = NormalGameData.Factory;
                        LevelSeed.MyGeometry = LevelGeometry.Right;
                        SelectNormal();
                        ShowHeros(true);
                        ShowCheckpoints(false);
                        HasWall = true;
                    }
                    else if (gamename == Localization.Words.Masochistic)
                    {
                        LevelSeed.MyGameType = NormalGameData.Factory;
                        LevelSeed.MyGeometry = LevelGeometry.Right;
                        LevelSeed.Masochistic = true;
                        SelectNormal();
                    }
                };
            
            // Hero
            HeroText = new Text(Localization.Words.Hero, ItemFont);
            HeroText.Name = "Hero";
            SetHeaderProperties(HeroText);
            MyPile.Add(HeroText);
            HeroText.Pos = new Vector2(-1044.443f, 933 - 2 * 222f);

            HeroList = new MenuList();
            HeroList.Name = "Hero";
            HeroList.Center = !LeftJustify;
			HeroList.MyExpandPos = new Vector2(-274.4442f, 243.0551f);
            foreach (BobPhsx hero in FreeplayHeroes)
                AddHeroItem(hero);

            // Random
            item = new MenuItem(new Text(Localization.Words.Random, ItemFont, false, true));
            SetItemProperties(item);
            HeroList.AddItem(item, BobPhsxRandom.Instance);
            // Custom
            item = new MenuItem(new Text(CustomHeroString, ItemFont, false, true));
            SetItemProperties(item);
            HeroList.AddItem(item, null);

            AddItem(HeroList);
            if (LeftJustify)
                HeroList.Pos = new Vector2(117.2227f + LeftJustifyAddX - 150, 828f - 2 * 222f);
            else
                HeroList.Pos = new Vector2(117.2227f, 828f - 2 * 222f);
            HeroList.OnIndexSelect = HeroList_OnIndex;
            HeroList.SetIndex(0);

            // Difficulty
            Text DiffText = new Text(Localization.Words.Difficulty, ItemFont);
            DiffText.Name = "Diff";
            SetHeaderProperties(DiffText);
            MyPile.Add(DiffText);
            DiffText.Pos = new Vector2(-1233.889f, 40.55557f);

            Localization.Words[] Names = CampaignHelper.DifficultyNames;
            DiffList = new MenuList();
            DiffList.Name = "Diff";
            DiffList.Center = !LeftJustify;
			DiffList.MyExpandPos = new Vector2(-252.2224f, 38.05466f);
            DiffList.DoIndexWrapping = false;
            for (int i = 0; i < 5; i++)
            {
                item = new MenuItem(new Text(Names[i], ItemFont, false, true));
                SetItemProperties(item);
                DiffList.AddItem(item, Names[i]);
            }
            AddItem(DiffList);
            if (LeftJustify)
                DiffList.Pos = new Vector2(242.2246f + LeftJustifyAddX, -73.11105f);
            else
                DiffList.Pos = new Vector2(242.2246f, -73.11105f);
            DiffList.OnIndexSelect = () => { };

            DiffList.OnIndexSelect = DiffList_OnIndex;


            // Length
            Text LengthText = new Text(Localization.Words.Length, ItemFont);
            LengthText.Name = "Length";
            SetHeaderProperties(LengthText);
            MyPile.Add(LengthText);
            LengthText.Pos = new Vector2(-1224.999f, -191.6667f);

            length = new LengthSlider();
            length.Name = "Length";
            length.Go = null;
            AddItem(length);
            length.Pos = new Vector2(-283f, -556.1017f);

            //length.OnSetValue = () => LevelSeed.PieceLength = (int)(length.MyFloat.Val / LevelSeed.NumPieces);
            length.OnSetValue = () =>
                {
                    LevelSeed.Length = (int)(length.MyFloat.Val);
                    switch (LevelSeed.NumPieces)
                    {
                        case 1: LevelSeed.PieceLength = (int)(length.MyFloat.Val); break;
                        case 2: LevelSeed.PieceLength = (int)(length.MyFloat.Val * .7f); break;
                        case 3: LevelSeed.PieceLength = (int)(length.MyFloat.Val * .5f); break;
                        case 4: LevelSeed.PieceLength = (int)(length.MyFloat.Val * .4f); break;

                        default:
                            LevelSeed.PieceLength = (int)(length.MyFloat.Val / LevelSeed.NumPieces); break;
                    }
                };
            length.OnSlide = () =>
                {
                    DesiredLength = length.MyFloat.Val;

                    int MaxCheckpoints = Math.Min(4, (int)(length.MyFloat.Percent / 20));

                    float currentcheckpoints = checkpoints.MyFloat.Val;
                    currentcheckpoints = Math.Max(currentcheckpoints, DesiredNumCheckpoints);
                    currentcheckpoints = Math.Min(currentcheckpoints, MaxCheckpoints);
                    checkpoints.MyFloat.Val = currentcheckpoints;
                };

            // Mini checkpoints
            MiniCheckpoint = ObjectIcon.CheckpointIcon.Clone(ObjectIcon.IconScale.Widget);
            MiniCheckpoint.SetShadow(false);

            // Checkpoints
            CheckpointsText = new Text(Localization.Words.Checkpoints, ItemFont);
            CheckpointsText.Name = "Checkpoints";
            SetHeaderProperties(CheckpointsText);
            MyPile.Add(CheckpointsText);
            CheckpointsText.Pos = new Vector2(-1008.33f, -661.1111f);

            checkpoints = new MenuSliderNoSlide(new Text("x ", ItemFont));
            checkpoints.Name = "Checkpoints";
            checkpoints.MyFloat = new WrappedFloat(1, 0, 4);
            checkpoints.InitialSlideSpeed = 1;
            checkpoints.MaxSlideSpeed = 1;
            checkpoints.Discrete = true;
            checkpoints.SetToShowText();
            checkpoints.Go = null;
            checkpoints.SetIcon(ObjectIcon.CheckpointIcon.Clone());
            checkpoints.Icon.SetShadow(false);
            checkpoints.MyDrawLayer = 1;
            AddItem(checkpoints);
            checkpoints.Pos = checkpoints.SelectedPos = new Vector2(267f, -680.549f);
            checkpoints.Icon.Pos = new Vector2(-22.22266f, -41.66666f);
            checkpoints.OnSlide = () =>
                {
                    DesiredNumCheckpoints = (int)checkpoints.MyFloat.Val;

                    float MinLength = (.11f + .2f * (checkpoints.MyFloat.Val + 1)) * length.MyFloat.Spread;
                    length.MyFloat.Val = Math.Min(length.MyFloat.Val, DesiredLength);
                    length.MyFloat.Val = Math.Max(length.MyFloat.Val, MinLength);
                };

            checkpoints.OnSetValue = () =>
                {
                    int PrevNumPieces = LevelSeed.NumPieces;

                    LevelSeed.NumPieces = 1 + (int)checkpoints.MyFloat.Val;
                    length.Val = LevelSeed.Length;

                    if (PrevNumPieces != LevelSeed.NumPieces)
                        MiniCheckpoint.SetScale(.01f);
                };

            checkpoints.Val = DesiredNumCheckpoints = 1;
            length.Val = 8000;


            FontScale = 1f;

            // Start
            Start = item = new MenuItem(new Text(Localization.Words.Start, ItemFont));
            Start.Name = "Start";
            item.JiggleOnGo = false;
            AddItem(item);
            item.Pos = item.SelectedPos = new Vector2(682.1445f, -238.8095f);
			item.Go = Cast.ToItem(BringNext);
			item.ScaleText(.92f);

#if XBOX
			item.Selectable = false;
#endif

			if (ButtonCheck.ControllerInUse)
			{
				Menu.DefaultMenuInfo.SetNext(item);
				var button_a = new QuadClass(ButtonTexture.Go, 90, "Button_A");
				//button_a.MyShowType = ShowType.ShowForGamepadOnly;
				MyPile.Add(button_a);
			}

            // Select 'Start Level' when the user presses (A)
            MyMenu.OnA = menu => { Start.Go(null); return true; };

            // Load
            MenuItem Load;
            Load = item = new MenuItem(new Text(Localization.Words.LoadLevel, ItemFont));
            Load.Name = "Load";
            Load.Go = me => BringLoad();
            item.JiggleOnGo = false;
            AddItem(item);
            item.Pos = item.SelectedPos = new Vector2(682.1445f, -238.8095f);
			item.ScaleText(.92f);

#if XBOX
			item.Selectable = false;
#endif

			if (ButtonCheck.ControllerInUse)
			{
				Menu.DefaultMenuInfo.SetX(item);
				var button_y = new QuadClass(ButtonTexture.X, 90, "Button_Y");
				//button_y.MyShowType = ShowType.ShowForGamepadOnly;
				MyPile.Add(button_y);
			}

            // Back
            item = new MenuItem(new Text(Localization.Words.Back, ItemFont));
            item.Name = "Back";
            AddItem(item);
            item.SelectSound = null;
            item.Go = me => ReturnToCaller();
            item.Pos = item.SelectedPos = new Vector2(922.9375f, -523.8096f);
			item.ScaleText(.92f);

#if XBOX
			item.Selectable = false;
#endif

			if (ButtonCheck.ControllerInUse)
			{
				Menu.DefaultMenuInfo.SetBack(item);
				var button_b = new QuadClass(ButtonTexture.Back, 90, "Button_B");
				//button_b.MyShowType = ShowType.ShowForGamepadOnly;
				MyPile.Add(button_b);
			}

            

            // Select the first item in the menu to start
            MyMenu.SelectItem(0);
            GameList.SetIndex(0);
            DiffList.SetIndex(0);
            MyMenu.OnB = MenuReturnToCaller;
			//MyMenu.OnY = BringLoad;
			MyMenu.OnX = Cast.ToMenu(BringLoad);
        }

        private MenuItem AddHeroItem(BobPhsx hero)
        {
            MenuItem item;
            item = new MenuItem(new Text(hero.Name, ItemFont, false, true));
            item.MyObject = hero;
            SetItemProperties(item);
            HeroList.AddItem(item, hero);
            float width = item.MyText.GetWorldWidth();
            const float max_width = 800;
            if (width > max_width)
                item.ScaleText(max_width / width);
            return item;
        }

        private void DiffList_OnIndex()
        {
            if (DiffList.ListIndex == 0)
                Start.SubstituteText(Localization.Words.Continue);
            else
                Start.SubstituteText(Localization.Words.Start);
        }

        void StartLevel()
        {
            CallingPanel = null;
            StartLevelFromMenuData();
        }

		static bool NoCheckpoints_Rocketbox = false;
		static bool NoCheckpoints_WallLevel = false;

        private void HeroList_OnIndex()
        {
            DesiredHeroIndex = HeroList.ListIndex;

            BobPhsx Hero = HeroList.CurObj as BobPhsx;

            if (null != Hero)
            {
                LevelSeed.DefaultHeroType = Hero;
                HeroIcon = Hero.Icon.Clone();
                HeroIcon.SetScale(1.25f);

                if (LevelSeed.MyGeometry == LevelGeometry.Up || LevelSeed.MyGeometry == LevelGeometry.Down)
                    if (Hero == BobPhsxSpaceship.Instance || Hero == BobPhsxRocketbox.Instance)
                        HeroList.SetIndex(HeroList.ListIndex + 1);
            }
            else
                if (IsCustomHero())
                    HeroIcon = ObjectIcon.CustomIcon;

            HeroIcon.FancyPos.SetCenter(Pos);

			switch (Localization.CurrentLanguage.MyLanguage)
			{
				case Localization.Language.German: HeroIcon.Pos = new Vector2(952.7809f, 525.0001f); break;
				case Localization.Language.Chinese: HeroIcon.Pos = new Vector2(713.8889f, 383.3333f); break;
				default: HeroIcon.Pos = new Vector2(1050.003f, 383.3334f); break;
			}

			if (checkpoints != null)
			{
				if (Hero == BobPhsxRocketbox.Instance)
				{
					NoCheckpoints_Rocketbox = true;
					ShowCheckpoints(false);
				}
				else
				{
					NoCheckpoints_Rocketbox = false;
					ShowCheckpoints(true);
				}
			}
        }

        private bool IsCustomHero()
        {
			return HeroList.ListIndex == HeroList.MyList.Count - 1;
			//return ((BobPhsx)HeroList.CurMenuItem.MyObject) == null;
        }

        public bool IsCustomDifficulty()
        {
            return DiffList.ListIndex == 0;
        }

        public void SetPos()
        {
			if (true)
			//if (ButtonCheck.ControllerInUse)
			{
				if (Localization.CurrentLanguage.MyLanguage == Localization.Language.Portuguese)
				{
					MenuItem _item;
					_item = MyMenu.FindItemByName("Location"); if (_item != null) { _item.SetPos = new Vector2(-106.3601f, 666.0923f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Game"); if (_item != null) { _item.SetPos = new Vector2(-106.3601f, 467.5059f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Hero"); if (_item != null) { _item.SetPos = new Vector2(-106.3601f, 268.9195f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Diff"); if (_item != null) { _item.SetPos = new Vector2(-106.3601f, 70.3331f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Length"); if (_item != null) { _item.SetPos = new Vector2(-221.8892f, -342.2129f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Checkpoints"); if (_item != null) { _item.SetPos = new Vector2(150.3331f, -602.7712f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Start"); if (_item != null) { _item.SetPos = new Vector2(877.7795f, -45.95242f); _item.MyText.Scale = 0.7502491f; _item.MySelectedText.Scale = 0.7502491f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Load"); if (_item != null) { _item.SetPos = new Vector2(877.7795f, -243.5685f); _item.MyText.Scale = 0.7280834f; _item.MySelectedText.Scale = 0.7280834f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Back"); if (_item != null) { _item.SetPos = new Vector2(877.7795f, -441.1846f); _item.MyText.Scale = 0.7764169f; _item.MySelectedText.Scale = 0.7764169f; _item.SelectIconOffset = new Vector2(0f, 0f); }

					MyMenu.Pos = new Vector2(-285f, 0f);

					Text _t;
					_t = MyPile.FindText("Location"); if (_t != null) { _t.Pos = new Vector2(-931.4443f, 778.6294f); _t.Scale = 0.657f; }
					_t = MyPile.FindText("Game"); if (_t != null) { _t.Pos = new Vector2(-689.6452f, 595.5182f); _t.Scale = 0.657f; }
					_t = MyPile.FindText("Hero"); if (_t != null) { _t.Pos = new Vector2(-740.8624f, 407.6481f); _t.Scale = 0.657f; }
					_t = MyPile.FindText("Diff"); if (_t != null) { _t.Pos = new Vector2(-1029.919f, 208.8086f); _t.Scale = 0.657f; }
					_t = MyPile.FindText("Length"); if (_t != null) { _t.Pos = new Vector2(-1166.665f, 36.11107f); _t.Scale = 0.657f; }
					_t = MyPile.FindText("Checkpoints"); if (_t != null) { _t.Pos = new Vector2(-1177.774f, -463.8889f); _t.Scale = 0.657f; }

					QuadClass _q;
					_q = MyPile.FindQuad("Backdrop"); if (_q != null) { _q.Pos = new Vector2(307.143f, -23.41241f); _q.Size = new Vector2(1741.167f, 1044.7f); }
					_q = MyPile.FindQuad("Button_A"); if (_q != null) { _q.Pos = new Vector2(808.3335f, -180.5555f); _q.Size = new Vector2(91.00001f, 91.00001f); }
					_q = MyPile.FindQuad("Button_Y"); if (_q != null) { _q.Pos = new Vector2(805.9504f, -385.3146f); _q.Size = new Vector2(91.99997f, 91.99997f); }
					_q = MyPile.FindQuad("Button_B"); if (_q != null) { _q.Pos = new Vector2(811.5058f, -598.8085f); _q.Size = new Vector2(90f, 90f); }

					MyPile.Pos = new Vector2(-285f, 0f);
				}
				else if (Localization.CurrentLanguage.MyLanguage == Localization.Language.Japanese)
				{
					MenuItem _item;
					_item = MyMenu.FindItemByName("Location"); if (_item != null) { _item.SetPos = new Vector2(-108.335f, 664.111f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Game"); if (_item != null) { _item.SetPos = new Vector2(-105.2212f, 475.4443f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Hero"); if (_item != null) { _item.SetPos = new Vector2(-104.9995f, 286.7778f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Diff"); if (_item != null) { _item.SetPos = new Vector2(-99.44069f, 96.33347f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Length"); if (_item != null) { _item.SetPos = new Vector2(-221.8892f, -342.2129f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Checkpoints"); if (_item != null) { _item.SetPos = new Vector2(150.3331f, -602.7712f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Start"); if (_item != null) { _item.SetPos = new Vector2(877.7795f, -45.95242f); _item.MyText.Scale = 0.6728325f; _item.MySelectedText.Scale = 0.6728325f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Load"); if (_item != null) { _item.SetPos = new Vector2(898.0163f, -255.4761f); _item.MyText.Scale = 0.63375f; _item.MySelectedText.Scale = 0.63375f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Back"); if (_item != null) { _item.SetPos = new Vector2(889.6048f, -447.6191f); _item.MyText.Scale = 0.7010834f; _item.MySelectedText.Scale = 0.7010834f; _item.SelectIconOffset = new Vector2(0f, 0f); }

					MyMenu.Pos = new Vector2(-285f, 0f);

					Text _t;
					_t = MyPile.FindText("Location"); if (_t != null) { _t.Pos = new Vector2(-1002.89f, 802.4446f); _t.Scale = 0.657f; }
					_t = MyPile.FindText("Game"); if (_t != null) { _t.Pos = new Vector2(-747.2218f, 619.3334f); _t.Scale = 0.657f; }
					_t = MyPile.FindText("Hero"); if (_t != null) { _t.Pos = new Vector2(-813.8876f, 422.3335f); _t.Scale = 0.657f; }
					_t = MyPile.FindText("Diff"); if (_t != null) { _t.Pos = new Vector2(-808.889f, 218.3334f); _t.Scale = 0.657f; }
					_t = MyPile.FindText("Length"); if (_t != null) { _t.Pos = new Vector2(-1183.332f, 33.33329f); _t.Scale = 0.657f; }
					_t = MyPile.FindText("Checkpoints"); if (_t != null) { _t.Pos = new Vector2(-1177.774f, -463.8889f); _t.Scale = 0.657f; }

					QuadClass _q;
					_q = MyPile.FindQuad("Backdrop"); if (_q != null) { _q.Pos = new Vector2(307.143f, -23.41241f); _q.Size = new Vector2(1741.167f, 1044.7f); }
					_q = MyPile.FindQuad("Button_A"); if (_q != null) { _q.Pos = new Vector2(800.0002f, -191.6666f); _q.Size = new Vector2(91.00001f, 91.00001f); }
					_q = MyPile.FindQuad("Button_Y"); if (_q != null) { _q.Pos = new Vector2(802.7778f, -397.2222f); _q.Size = new Vector2(91.99997f, 91.99997f); }
					_q = MyPile.FindQuad("Button_B"); if (_q != null) { _q.Pos = new Vector2(811.111f, -600f); _q.Size = new Vector2(90f, 90f); }

					MyPile.Pos = new Vector2(-285f, 0f);
				}
				else if (Localization.CurrentLanguage.MyLanguage == Localization.Language.Korean)
				{
					MenuItem _item;
					_item = MyMenu.FindItemByName("Location"); if (_item != null) { _item.SetPos = new Vector2(-227.7793f, 650.2222f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Game"); if (_item != null) { _item.SetPos = new Vector2(-227.7793f, 467.111f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Hero"); if (_item != null) { _item.SetPos = new Vector2(-227.7793f, 283.9999f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Diff"); if (_item != null) { _item.SetPos = new Vector2(-227.7793f, 100.8887f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Length"); if (_item != null) { _item.SetPos = new Vector2(-221.8892f, -342.2129f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Checkpoints"); if (_item != null) { _item.SetPos = new Vector2(150.3331f, -602.7712f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Start"); if (_item != null) { _item.SetPos = new Vector2(877.7795f, -45.95242f); _item.MyText.Scale = 0.6728325f; _item.MySelectedText.Scale = 0.6728325f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Load"); if (_item != null) { _item.SetPos = new Vector2(898.0163f, -255.4761f); _item.MyText.Scale = 0.63375f; _item.MySelectedText.Scale = 0.63375f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Back"); if (_item != null) { _item.SetPos = new Vector2(889.6048f, -447.6191f); _item.MyText.Scale = 0.7010834f; _item.MySelectedText.Scale = 0.7010834f; _item.SelectIconOffset = new Vector2(0f, 0f); }

					MyMenu.Pos = new Vector2(-285f, 0f);

					Text _t;
					_t = MyPile.FindText("Location"); if (_t != null) { _t.Pos = new Vector2(-994.5568f, 802.4446f); _t.Scale = 0.657f; }
					_t = MyPile.FindText("Game"); if (_t != null) { _t.Pos = new Vector2(-994.5568f, 608.2223f); _t.Scale = 0.657f; }
					_t = MyPile.FindText("Hero"); if (_t != null) { _t.Pos = new Vector2(-994.5568f, 414f); _t.Scale = 0.657f; }
					_t = MyPile.FindText("Diff"); if (_t != null) { _t.Pos = new Vector2(-994.5568f, 219.7777f); _t.Scale = 0.657f; }
					_t = MyPile.FindText("Length"); if (_t != null) { _t.Pos = new Vector2(-1183.332f, 33.33329f); _t.Scale = 0.657f; }
					_t = MyPile.FindText("Checkpoints"); if (_t != null) { _t.Pos = new Vector2(-1177.774f, -463.8889f); _t.Scale = 0.657f; }

					QuadClass _q;
					_q = MyPile.FindQuad("Backdrop"); if (_q != null) { _q.Pos = new Vector2(307.143f, -23.41241f); _q.Size = new Vector2(1741.167f, 1044.7f); }
					_q = MyPile.FindQuad("Button_A"); if (_q != null) { _q.Pos = new Vector2(800.0002f, -191.6666f); _q.Size = new Vector2(91.00001f, 91.00001f); }
					_q = MyPile.FindQuad("Button_Y"); if (_q != null) { _q.Pos = new Vector2(802.7778f, -397.2222f); _q.Size = new Vector2(91.99997f, 91.99997f); }
					_q = MyPile.FindQuad("Button_B"); if (_q != null) { _q.Pos = new Vector2(811.111f, -600f); _q.Size = new Vector2(90f, 90f); }

					MyPile.Pos = new Vector2(-285f, 0f);
				}
				else if (Localization.CurrentLanguage.MyLanguage == Localization.Language.German)
				{
					MenuItem _item;
					_item = MyMenu.FindItemByName("Location"); if (_item != null) { _item.SetPos = new Vector2(-418.7289f, 652.6049f); _item.MyText.Scale = 0.6f; _item.MySelectedText.Scale = 0.6f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Game"); if (_item != null) { _item.SetPos = new Vector2(-405.6959f, 486.9569f); _item.MyText.Scale = 0.6f; _item.MySelectedText.Scale = 0.6f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Hero"); if (_item != null) { _item.SetPos = new Vector2(-401.5051f, 326.0749f); _item.MyText.Scale = 0.6f; _item.MySelectedText.Scale = 0.6f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Diff"); if (_item != null) { _item.SetPos = new Vector2(149.3548f, 107.4511f); _item.MyText.Scale = 0.6f; _item.MySelectedText.Scale = 0.6f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Length"); if (_item != null) { _item.SetPos = new Vector2(-221.8892f, -342.2129f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Checkpoints"); if (_item != null) { _item.SetPos = new Vector2(150.3331f, -602.7712f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Start"); if (_item != null) { _item.SetPos = new Vector2(856.7616f, -35.24284f); _item.MyText.Scale = 0.7128553f; _item.MySelectedText.Scale = 0.7128553f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Load"); if (_item != null) { _item.SetPos = new Vector2(872.6385f, -228.4949f); _item.MyText.Scale = 0.713081f; _item.MySelectedText.Scale = 0.713081f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Back"); if (_item != null) { _item.SetPos = new Vector2(856.7616f, -405.8701f); _item.MyText.Scale = 0.7444999f; _item.MySelectedText.Scale = 0.7444999f; _item.SelectIconOffset = new Vector2(0f, 0f); }

					MyMenu.Pos = new Vector2(-296.9077f, -7.938354f);

					Text _t;
					_t = MyPile.FindText("Location"); if (_t != null) { _t.Pos = new Vector2(-1180.78f, 759.9747f); _t.Scale = 0.5878682f; }
					_t = MyPile.FindText("Game"); if (_t != null) { _t.Pos = new Vector2(-1174.717f, 585.2033f); _t.Scale = 0.5308989f; }
					_t = MyPile.FindText("Hero"); if (_t != null) { _t.Pos = new Vector2(-1168.766f, 429.482f); _t.Scale = 0.5489983f; }
					_t = MyPile.FindText("Diff"); if (_t != null) { _t.Pos = new Vector2(-1164.826f, 216.3521f); _t.Scale = 0.5383968f; }
					_t = MyPile.FindText("Length"); if (_t != null) { _t.Pos = new Vector2(-1166.665f, 36.11107f); _t.Scale = 0.657f; }
					_t = MyPile.FindText("Checkpoints"); if (_t != null) { _t.Pos = new Vector2(-1177.774f, -463.8889f); _t.Scale = 0.657f; }

					QuadClass _q;
					_q = MyPile.FindQuad("Backdrop"); if (_q != null) { _q.Pos = new Vector2(307.143f, -23.41241f); _q.Size = new Vector2(1741.167f, 1044.7f); }
					_q = MyPile.FindQuad("Button_A"); if (_q != null) { _q.Pos = new Vector2(771.4327f, -177.7777f); _q.Size = new Vector2(91.00001f, 91.00001f); }
					_q = MyPile.FindQuad("Button_Y"); if (_q != null) { _q.Pos = new Vector2(773.0122f, -371.4323f); _q.Size = new Vector2(91.99997f, 91.99997f); }
					_q = MyPile.FindQuad("Button_B"); if (_q != null) { _q.Pos = new Vector2(776.1786f, -562.7041f); _q.Size = new Vector2(90f, 90f); }

					MyPile.Pos = new Vector2(-285f, 0f);
				}
				else if (Localization.CurrentLanguage.MyLanguage == Localization.Language.Chinese)
				{
					MenuItem _item;
					_item = MyMenu.FindItemByName("Location"); if (_item != null) { _item.SetPos = new Vector2(-153.5826f, 635.5367f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Game"); if (_item != null) { _item.SetPos = new Vector2(-153.5826f, 454.8084f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Hero"); if (_item != null) { _item.SetPos = new Vector2(-153.5826f, 274.0801f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Diff"); if (_item != null) { _item.SetPos = new Vector2(-153.5826f, 93.35175f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Length"); if (_item != null) { _item.SetPos = new Vector2(-221.8892f, -342.2129f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Checkpoints"); if (_item != null) { _item.SetPos = new Vector2(150.3331f, -602.7712f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Start"); if (_item != null) { _item.SetPos = new Vector2(901.5949f, -34.04483f); _item.MyText.Scale = 0.7502491f; _item.MySelectedText.Scale = 0.7502491f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Load"); if (_item != null) { _item.SetPos = new Vector2(901.5949f, -243.5685f); _item.MyText.Scale = 0.7280834f; _item.MySelectedText.Scale = 0.7280834f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Back"); if (_item != null) { _item.SetPos = new Vector2(901.5949f, -453.0922f); _item.MyText.Scale = 0.7764169f; _item.MySelectedText.Scale = 0.7764169f; _item.SelectIconOffset = new Vector2(0f, 0f); }

					MyMenu.Pos = new Vector2(-285f, 0f);

					Text _t;
					_t = MyPile.FindText("Location"); if (_t != null) { _t.Pos = new Vector2(-728.6802f, 794.1112f); _t.Scale = 0.7056667f; }
					_t = MyPile.FindText("Game"); if (_t != null) { _t.Pos = new Vector2(-728.6802f, 599.8888f); _t.Scale = 0.7056667f; }
					_t = MyPile.FindText("Hero"); if (_t != null) { _t.Pos = new Vector2(-728.6802f, 211.4442f); _t.Scale = 0.7056667f; }
					_t = MyPile.FindText("Diff"); if (_t != null) { _t.Pos = new Vector2(-728.6802f, 405.6665f); _t.Scale = 0.7056667f; }
					_t = MyPile.FindText("Length"); if (_t != null) { _t.Pos = new Vector2(-1213.887f, 33.33327f); _t.Scale = 0.657f; }
					_t = MyPile.FindText("Checkpoints"); if (_t != null) { _t.Pos = new Vector2(-716.663f, -483.3333f); _t.Scale = 0.6756666f; }

					QuadClass _q;
					_q = MyPile.FindQuad("Backdrop"); if (_q != null) { _q.Pos = new Vector2(307.143f, -23.41241f); _q.Size = new Vector2(1741.167f, 1044.7f); }
					_q = MyPile.FindQuad("Button_A"); if (_q != null) { _q.Pos = new Vector2(808.3335f, -180.5555f); _q.Size = new Vector2(91.00001f, 91.00001f); }
					_q = MyPile.FindQuad("Button_Y"); if (_q != null) { _q.Pos = new Vector2(808.3335f, -393.8987f); _q.Size = new Vector2(91.99997f, 91.99997f); }
					_q = MyPile.FindQuad("Button_B"); if (_q != null) { _q.Pos = new Vector2(808.3335f, -607.242f); _q.Size = new Vector2(90f, 90f); }

					MyPile.Pos = new Vector2(-285f, 0f);
				}
                else if (Localization.CurrentLanguage.MyLanguage == Localization.Language.Italian)
                {
                    MenuItem _item;
                    _item = MyMenu.FindItemByName("Location"); if (_item != null) { _item.SetPos = new Vector2(-106.3601f, 666.0923f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
                    _item = MyMenu.FindItemByName("Game"); if (_item != null) { _item.SetPos = new Vector2(-106.3601f, 471.475f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
                    _item = MyMenu.FindItemByName("Hero"); if (_item != null) { _item.SetPos = new Vector2(-106.3601f, 276.8577f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
                    _item = MyMenu.FindItemByName("Diff"); if (_item != null) { _item.SetPos = new Vector2(-106.3601f, 82.24045f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
                    _item = MyMenu.FindItemByName("Length"); if (_item != null) { _item.SetPos = new Vector2(-221.8892f, -342.2129f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
                    _item = MyMenu.FindItemByName("Checkpoints"); if (_item != null) { _item.SetPos = new Vector2(150.3331f, -602.7712f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
                    _item = MyMenu.FindItemByName("Start"); if (_item != null) { _item.SetPos = new Vector2(901.5949f, -45.15594f); _item.MyText.Scale = 0.6549995f; _item.MySelectedText.Scale = 0.6549995f; _item.SelectIconOffset = new Vector2(0f, 0f); }
                    _item = MyMenu.FindItemByName("Load"); if (_item != null) { _item.SetPos = new Vector2(901.5949f, -254.6796f); _item.MyText.Scale = 0.6328338f; _item.MySelectedText.Scale = 0.6328338f; _item.SelectIconOffset = new Vector2(0f, 0f); }
                    _item = MyMenu.FindItemByName("Back"); if (_item != null) { _item.SetPos = new Vector2(901.5949f, -464.2033f); _item.MyText.Scale = 0.6811673f; _item.MySelectedText.Scale = 0.6811673f; _item.SelectIconOffset = new Vector2(0f, 0f); }

                    MyMenu.Pos = new Vector2(-285f, 0f);

                    Text _t;
                    _t = MyPile.FindText("Location"); if (_t != null) { _t.Pos = new Vector2(-987.0131f, 802.4446f); _t.Scale = 0.657f; }
                    _t = MyPile.FindText("Game"); if (_t != null) { _t.Pos = new Vector2(-808.7211f, 619.3334f); _t.Scale = 0.657f; }
                    _t = MyPile.FindText("Hero"); if (_t != null) { _t.Pos = new Vector2(-740.8624f, 423.5249f); _t.Scale = 0.657f; }
                    _t = MyPile.FindText("Diff"); if (_t != null) { _t.Pos = new Vector2(-1021.981f, 216.747f); _t.Scale = 0.657f; }
                    _t = MyPile.FindText("Length"); if (_t != null) { _t.Pos = new Vector2(-1166.665f, 36.11107f); _t.Scale = 0.657f; }
                    _t = MyPile.FindText("Checkpoints"); if (_t != null) { _t.Pos = new Vector2(-1177.774f, -463.8889f); _t.Scale = 0.657f; }

                    QuadClass _q;
                    _q = MyPile.FindQuad("Backdrop"); if (_q != null) { _q.Pos = new Vector2(307.143f, -23.41241f); _q.Size = new Vector2(1741.167f, 1044.7f); }
                    _q = MyPile.FindQuad("Button_A"); if (_q != null) { _q.Pos = new Vector2(808.3335f, -180.5555f); _q.Size = new Vector2(91.00001f, 91.00001f); }
                    _q = MyPile.FindQuad("Button_Y"); if (_q != null) { _q.Pos = new Vector2(808.3335f, -393.8987f); _q.Size = new Vector2(91.99997f, 91.99997f); }
                    _q = MyPile.FindQuad("Button_B"); if (_q != null) { _q.Pos = new Vector2(808.3335f, -607.242f); _q.Size = new Vector2(90f, 90f); }

                    MyPile.Pos = new Vector2(-285f, 0f);
                }
				else if (Localization.CurrentLanguage.MyLanguage == Localization.Language.Russian)
				{
					MenuItem _item;
					_item = MyMenu.FindItemByName("Location"); if (_item != null) { _item.SetPos = new Vector2(-106.3601f, 666.0923f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Game"); if (_item != null) { _item.SetPos = new Vector2(-106.3601f, 471.475f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Hero"); if (_item != null) { _item.SetPos = new Vector2(-106.3601f, 276.8577f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Diff"); if (_item != null) { _item.SetPos = new Vector2(-106.3601f, 82.24045f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Length"); if (_item != null) { _item.SetPos = new Vector2(-221.8892f, -342.2129f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Checkpoints"); if (_item != null) { _item.SetPos = new Vector2(344.7774f, -602.7712f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Start"); if (_item != null) { _item.SetPos = new Vector2(901.5949f, -34.04483f); _item.MyText.Scale = 0.7502491f; _item.MySelectedText.Scale = 0.7502491f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Load"); if (_item != null) { _item.SetPos = new Vector2(901.5949f, -243.5685f); _item.MyText.Scale = 0.7280834f; _item.MySelectedText.Scale = 0.7280834f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Back"); if (_item != null) { _item.SetPos = new Vector2(901.5949f, -453.0922f); _item.MyText.Scale = 0.7764169f; _item.MySelectedText.Scale = 0.7764169f; _item.SelectIconOffset = new Vector2(0f, 0f); }

					MyMenu.Pos = new Vector2(-285f, 0f);

					Text _t;
					_t = MyPile.FindText("Location"); if (_t != null) { _t.Pos = new Vector2(-987.0131f, 802.4446f); _t.Scale = 0.657f; }
					_t = MyPile.FindText("Game"); if (_t != null) { _t.Pos = new Vector2(-808.7211f, 619.3334f); _t.Scale = 0.657f; }
					_t = MyPile.FindText("Hero"); if (_t != null) { _t.Pos = new Vector2(-740.8624f, 423.5249f); _t.Scale = 0.657f; }
					_t = MyPile.FindText("Diff"); if (_t != null) { _t.Pos = new Vector2(-1021.981f, 216.747f); _t.Scale = 0.657f; }
					_t = MyPile.FindText("Length"); if (_t != null) { _t.Pos = new Vector2(-1166.665f, 36.11107f); _t.Scale = 0.657f; }
					_t = MyPile.FindText("Checkpoints"); if (_t != null) { _t.Pos = new Vector2(-1177.774f, -463.8889f); _t.Scale = 0.657f; }

					QuadClass _q;
					_q = MyPile.FindQuad("Backdrop"); if (_q != null) { _q.Pos = new Vector2(307.143f, -23.41241f); _q.Size = new Vector2(1741.167f, 1044.7f); }
					_q = MyPile.FindQuad("Button_A"); if (_q != null) { _q.Pos = new Vector2(808.3335f, -180.5555f); _q.Size = new Vector2(91.00001f, 91.00001f); }
					_q = MyPile.FindQuad("Button_Y"); if (_q != null) { _q.Pos = new Vector2(808.3335f, -393.8987f); _q.Size = new Vector2(91.99997f, 91.99997f); }
					_q = MyPile.FindQuad("Button_B"); if (_q != null) { _q.Pos = new Vector2(808.3335f, -607.242f); _q.Size = new Vector2(90f, 90f); }

					MyPile.Pos = new Vector2(-285f, 0f);
				}
                else
				{
					MenuItem _item;
					_item = MyMenu.FindItemByName("Location"); if (_item != null) { _item.SetPos = new Vector2(-106.3601f, 666.0923f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Game"); if (_item != null) { _item.SetPos = new Vector2(-106.3601f, 471.475f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Hero"); if (_item != null) { _item.SetPos = new Vector2(-106.3601f, 276.8577f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Diff"); if (_item != null) { _item.SetPos = new Vector2(-106.3601f, 82.24045f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Length"); if (_item != null) { _item.SetPos = new Vector2(-221.8892f, -342.2129f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Checkpoints"); if (_item != null) { _item.SetPos = new Vector2(150.3331f, -602.7712f); _item.MyText.Scale = 0.73f; _item.MySelectedText.Scale = 0.73f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Start"); if (_item != null) { _item.SetPos = new Vector2(901.5949f, -34.04483f); _item.MyText.Scale = 0.7502491f; _item.MySelectedText.Scale = 0.7502491f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Load"); if (_item != null) { _item.SetPos = new Vector2(901.5949f, -243.5685f); _item.MyText.Scale = 0.7280834f; _item.MySelectedText.Scale = 0.7280834f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Back"); if (_item != null) { _item.SetPos = new Vector2(901.5949f, -453.0922f); _item.MyText.Scale = 0.7764169f; _item.MySelectedText.Scale = 0.7764169f; _item.SelectIconOffset = new Vector2(0f, 0f); }

					MyMenu.Pos = new Vector2(-285f, 0f);

					Text _t;
					_t = MyPile.FindText("Location"); if (_t != null) { _t.Pos = new Vector2(-987.0131f, 802.4446f); _t.Scale = 0.657f; }
					_t = MyPile.FindText("Game"); if (_t != null) { _t.Pos = new Vector2(-808.7211f, 619.3334f); _t.Scale = 0.657f; }
					_t = MyPile.FindText("Hero"); if (_t != null) { _t.Pos = new Vector2(-740.8624f, 423.5249f); _t.Scale = 0.657f; }
					_t = MyPile.FindText("Diff"); if (_t != null) { _t.Pos = new Vector2(-1021.981f, 216.747f); _t.Scale = 0.657f; }
					_t = MyPile.FindText("Length"); if (_t != null) { _t.Pos = new Vector2(-1166.665f, 36.11107f); _t.Scale = 0.657f; }
					_t = MyPile.FindText("Checkpoints"); if (_t != null) { _t.Pos = new Vector2(-1177.774f, -463.8889f); _t.Scale = 0.657f; }

					QuadClass _q;
					_q = MyPile.FindQuad("Backdrop"); if (_q != null) { _q.Pos = new Vector2(307.143f, -23.41241f); _q.Size = new Vector2(1741.167f, 1044.7f); }
					_q = MyPile.FindQuad("Button_A"); if (_q != null) { _q.Pos = new Vector2(808.3335f, -180.5555f); _q.Size = new Vector2(91.00001f, 91.00001f); }
					_q = MyPile.FindQuad("Button_Y"); if (_q != null) { _q.Pos = new Vector2(808.3335f, -393.8987f); _q.Size = new Vector2(91.99997f, 91.99997f); }
					_q = MyPile.FindQuad("Button_B"); if (_q != null) { _q.Pos = new Vector2(808.3335f, -607.242f); _q.Size = new Vector2(90f, 90f); }

					MyPile.Pos = new Vector2(-285f, 0f);
				}
			}
			else
			{
				MenuItem _item;
				_item = MyMenu.FindItemByName("Location"); if (_item != null) { _item.SetPos = new Vector2(-66.66797f, 677.9999f); }
				_item = MyMenu.FindItemByName("Game"); if (_item != null) { _item.SetPos = new Vector2(-24.66602f, 475.4443f); }
				_item = MyMenu.FindItemByName("Hero"); if (_item != null) { _item.SetPos = new Vector2(-4.999969f, 284f); }
				_item = MyMenu.FindItemByName("Diff"); if (_item != null) { _item.SetPos = new Vector2(-41.10744f, 79.66678f); }
				_item = MyMenu.FindItemByName("Length"); if (_item != null) { _item.SetPos = new Vector2(-221.8892f, -342.2129f); }
				_item = MyMenu.FindItemByName("Checkpoints"); if (_item != null) { _item.SetPos = new Vector2(150.3331f, -602.7712f); }
				_item = MyMenu.FindItemByName("Start"); if (_item != null) { _item.SetPos = new Vector2(580.5577f, 4.047569f); }
				_item = MyMenu.FindItemByName("Load"); if (_item != null) { _item.SetPos = new Vector2(773.0172f, -224.9205f); }
				_item = MyMenu.FindItemByName("Back"); if (_item != null) { _item.SetPos = new Vector2(850.7158f, -461.508f); }

				MyMenu.Pos = new Vector2(-285f, 0f);

				Text _t;
				_t = MyPile.FindText("Location"); if (_t != null) { _t.Pos = new Vector2(-1002.89f, 802.4446f); }
				_t = MyPile.FindText("Game"); if (_t != null) { _t.Pos = new Vector2(-844.444f, 619.3334f); }
				_t = MyPile.FindText("Hero"); if (_t != null) { _t.Pos = new Vector2(-780.5543f, 419.5557f); }
				_t = MyPile.FindText("Diff"); if (_t != null) { _t.Pos = new Vector2(-1033.889f, 212.7778f); }
				_t = MyPile.FindText("Length"); if (_t != null) { _t.Pos = new Vector2(-1166.665f, 36.11107f); }
				_t = MyPile.FindText("Checkpoints"); if (_t != null) { _t.Pos = new Vector2(-1177.774f, -463.8889f); }

				QuadClass _q;
				_q = MyPile.FindQuad("Backdrop"); if (_q != null) { _q.Pos = new Vector2(307.143f, -23.41241f); _q.Size = new Vector2(1741.167f, 1044.7f); }
				_q = MyPile.FindQuad("Screenshot"); if (_q != null) { _q.Pos = new Vector2(1340.002f, 497.2222f); _q.Size = new Vector2(10.81731f, 13.22116f); }

				MyPile.Pos = new Vector2(-285f, 0f);
			}
		}
	

        private void SelectUpLevel()
        {
            LevelSeed.MyGameType = NormalGameData.Factory;
        
            if (LevelSeed.MyGeometry != LevelGeometry.Up && LevelSeed.MyGeometry != LevelGeometry.Down)
                SelectNormal();

            LevelSeed.MyGeometry = LevelGeometry.Up;
            UpHero_NoSpaceship();
            UpHero_ModShown();

            ShowCheckpoints(false);
        }

        private void SelectDownLevel()
        {
            LevelSeed.MyGameType = NormalGameData.Factory;

            if (LevelSeed.MyGeometry != LevelGeometry.Up && LevelSeed.MyGeometry != LevelGeometry.Down)
                SelectNormal();

            LevelSeed.MyGeometry = LevelGeometry.Down;
            UpHero_NoSpaceship();
            UpHero_ModShown();

            ShowCheckpoints(false);
        }

        /// <summary>
        /// The panel that actually starts the level, when it is started.
        /// </summary>
        public GUI_Panel CallingPanel;

        void BringNext()
        {
            if (IsCustomHero())
                BringHero();
			else if (IsCustomDifficulty())
				BringUpgrades();
			else
			{
				CloudberryKingdomGame.Freeplay_Count++;
				if (CloudberryKingdomGame.IsDemo && CloudberryKingdomGame.Freeplay_Count >= CloudberryKingdomGame.Freeplay_Max)
				{
					Call(new UpSellMenu(Localization.Words.UpSell_FreePlay, MenuItem.ActivatingPlayer));
					Hide(PresetPos.Right, 0);

					return;
				}

                CloudberryKingdomGame.PromptForDeviceIfNoneSelected();
				MyGame.PlayGame(StartLevel);
			}
        }

        public override void Show()
        {
            base.Show();
        }

        CustomHero_GUI HeroGui;
        void BringHero()
        {
            HeroGui = new CustomHero_GUI(this);
            Call(HeroGui, 0);
            Hide(PresetPos.Left);
            this.SlideInFrom = PresetPos.Left;
        }

        void BringUpgrades()
        {
            CallingPanel = new PassiveUpgrades_GUI(PieceSeed, this);
            Call(CallingPanel, 0);
            Hide(PresetPos.Left);
            this.SlideInFrom = PresetPos.Left;
        }

        void BringLoad()
        {
            if (CloudberryKingdomGame.IsDemo)
            {
                Call(new UpSellMenu(Localization.Words.UpSell_SaveLoad, MenuItem.ActivatingPlayer));
                Hide(PresetPos.Left, 0);

                return;
            }

            // Create the load menu
			CkBaseMenu seed_panel;
#if PC
            seed_panel = new SaveLoadSeedMenu(MenuItem.ActivatingPlayer, true, false);
            Call(seed_panel, 0);
#else
            seed_panel = new SavedSeedsGUI();
            Call(seed_panel, 0);
#endif
			//seed_panel.UseBounce = false;
			//seed_panel.zoom = null;
			//MyGame.WaitThenDo(13, () => seed_panel.EnableBounce());
			//seed_panel.RegularSlideOut(PresetPos.Right, 0);
			//seed_panel.SlideIn(12);

			Hide(PresetPos.Left);
            this.SlideInFrom = PresetPos.Left;
        }

        public override void OnReturnTo()
        {
            base.OnReturnTo();

            MyMenu.Show = true;
            CallingPanel = null;
        }

        int DesiredNumCheckpoints = 0;
        float DesiredLength = 0;
        int DesiredHeroIndex = 0;
        protected override void MyDraw()
        {
            if (!Active && CallingPanel != null && CallingPanel.Pos != null && !CallingPanel.Pos.Playing)
                MyMenu.Show = false;
            else
                MyMenu.Show = true;
 
            base.MyDraw();

            if (!MyMenu.Show)
            {
                MyMenu.CurDrawLayer = 0;
                MyMenu.PosOffset = MyMenu.FancyPos.Update();
                length.PosOffset = MyMenu.PosOffset;
                length.Draw();
            }

			if (CloudberryKingdomGame.IsDemo && Hid && !Pos.Playing) return;

            if (HeroIcon != null)
                HeroIcon.Draw(false);
 
            int NumCheckpoints = (int)checkpoints.MyFloat.Val;

            MiniCheckpoint.SetScale(.8f * MiniCheckpoint.PrevSetRatio + .2f);
            float step = (length.Slider_TR.X - length.BL.X) / (NumCheckpoints + 1);
            for (int i = 1; i <= NumCheckpoints; i++)
            {
                MiniCheckpoint.Pos = new Vector2(length.BL.X + i * step + 15, length.TR.Y);
                MiniCheckpoint.Draw(false);
            }
        }

        public static string SeedStringToLoad = null;
        public static bool ExitFreeplay = false;
        protected override void MyPhsxStep()
        {
            if (ExitFreeplay) return;

            if (SeedStringToLoad != null)
            {
                string seed = SeedStringToLoad;
                SeedStringToLoad = null;
                
                SavedSeedsGUI.LoadSeed(seed, this);
                return;
            }

            base.MyPhsxStep();
        }
    }
}