using System;

using Microsoft.Xna.Framework;

using CoreEngine;
using CloudberryKingdom.Bobs;
using CloudberryKingdom.Blocks;
using CloudberryKingdom.Menus;

namespace CloudberryKingdom
{
    public class CustomHero_GUI : CkBaseMenu
    {
        public static BobPhsx.CustomPhsxData HeroPhsxData;

        public static BobPhsx Hero;
        BobPhsxNormal NormalHero { get { return Hero as BobPhsxNormal; } }

        protected CustomLevel_GUI CustomLevel;
        public CustomHero_GUI(CustomLevel_GUI CustomLevel)
        {
            this.CustomLevel = CustomLevel;

            CustomLevel.CallingPanel = this;
        }

        public override void OnAdd()
        {
            base.OnAdd();

            MyGame.ForceTargetZoom = .001f;
            MyGame.DoForceZoom = true;
            MyGame.ForceLevelZoomBeforeDraw = .0007f;
        }

        protected override void ReleaseBody()
        {
			//MyGame.ForceTargetZoom = .0007f;
			MyGame.ForceTargetZoom = 0;
            base.ReleaseBody();
        }

        PhsxSlider GravitySlider, MaxSpeedSlider, AccelSlider;
        PhsxSlider MaxFallSpeedSlider, JumpLengthSlider, JumpAccelSlider, DoubleJumpLengthSlider, DoubleJumpAccelSlider, FrictionSlider, JetPackSlider, JetPackFuelSlider, NumJumpsSlider;
        PhsxSlider SizeSlider, PhasedSizeSlider, PhasedGravitySlider, PhasePeriodSlider;
        private void MakeSliders()
        {
            ItemPos = new Vector2(-807.5f, 698.1083f);
            PosAdd = new Vector2(0, -93.45f);

            PhsxSlider.Font = ItemFont;
            PhsxSlider.Process = AddItem;

            AccelSlider = new PhsxSlider(Localization.Words.Acceleration, BobPhsx.CustomData.accel);
            MaxSpeedSlider = new PhsxSlider(Localization.Words.MaxVelocity, BobPhsx.CustomData.maxspeed);
            SizeSlider = new PhsxSlider(Localization.Words.Size, BobPhsx.CustomData.size);
            GravitySlider = new PhsxSlider(Localization.Words.Gravity, BobPhsx.CustomData.gravity);
            MaxFallSpeedSlider = new PhsxSlider(Localization.Words.MaxFallSpeed, BobPhsx.CustomData.maxfall);
            FrictionSlider = new PhsxSlider(Localization.Words.Friction, BobPhsx.CustomData.friction);
            JumpLengthSlider = new PhsxSlider(Localization.Words.JumpLength, BobPhsx.CustomData.jumplength);
            JumpAccelSlider = new PhsxSlider(Localization.Words.JumpAcc, BobPhsx.CustomData.jumpaccel);

            NumJumpsSlider = new PhsxSlider(Localization.Words.NumJumps, BobPhsx.CustomData.numjumps); NumJumpsSlider.Discrete = true;
            DoubleJumpLengthSlider = new PhsxSlider(Localization.Words.DoubleJumpLength, BobPhsx.CustomData.jumplength2);
            DoubleJumpAccelSlider = new PhsxSlider(Localization.Words.DoubleJumpAccel, BobPhsx.CustomData.jumpaccel2);

            JetPackSlider = new PhsxSlider(Localization.Words.JetpackAcc, BobPhsx.CustomData.jetpackaccel);
            JetPackFuelSlider = new PhsxSlider(Localization.Words.JetpackFuel, BobPhsx.CustomData.jetpackfuel);

            PhasedSizeSlider = new PhsxSlider(Localization.Words.PhasedSize, BobPhsx.CustomData.size2);
            PhasedGravitySlider = new PhsxSlider(Localization.Words.PhasedGravity, BobPhsx.CustomData.gravity2);
            PhasePeriodSlider = new PhsxSlider(Localization.Words.PhasePeriod, BobPhsx.CustomData.phaseperiod);
        }

        public void StartTest()
        {
            Hide();

            MakeBobPhsx();

            CreateHeroes();
            
            RemovePreviousGround();
            CreateGround();

            Testing = true;

            var custom = CustomLevel as StartMenu_Clouds_CustomLevel;
            custom.Title.BackPanel.SetState(TitleBackgroundState.None);
        }

        bool Testing = false;
        void TestingPhsx()
        {
			if (CloudberryKingdomGame.AsianButtonSwitch)
			{
				if (ButtonCheck.State(ControllerButtons.A, -1).Pressed ||
					ButtonCheck.State(ControllerButtons.Back, -1).Pressed ||
					ButtonCheck.State(ControllerButtons.X, -1).Pressed ||
					ButtonCheck.State(ControllerButtons.Y, -1).Pressed)
					EndTest();
			}
			else
			{
				if (ButtonCheck.State(ControllerButtons.B, -1).Pressed)
					EndTest();
			}
        }

        void EndTest()
        {
            var custom = CustomLevel as StartMenu_Clouds_CustomLevel;
            custom.Title.BackPanel.SetState(TitleBackgroundState.Scene_Blur_Dark);

            Testing = false;
            Show();

            foreach (Bob bob in MyGame.MyLevel.Bobs)
            {
                ParticleEffects.AddPop(MyGame.MyLevel, bob.Pos);
                bob.CollectSelf();
            }

            MyGame.AddToDo((Func<bool>)MoveBlockAndKill, "MoveOut", true, true);
        }

        bool MoveBlockAndKill()
        {
            if (MyGame == null || MyGame.MyLevel == null) return true;

            foreach (BlockBase block in MyGame.MyLevel.Blocks)
                block.Move(new Vector2(0, -20));
            return false;
        }

        void KillBobs()
        {
            MyGame.WaitThenDo(20, () =>
            {
                foreach (Bob bob in MyGame.MyLevel.Bobs)
                    bob.Die(Bob.BobDeathType.None);
            }, "RemoveBobs", false, true);
        }

        void MakeBobPhsx()
        {
            // Error check
            if (Base is BobPhsxSpaceship)
            {
                Jump = BobPhsxNormal.Instance;
            }

            // Make the hero
            Hero = MyGame.MyLevel.DefaultHeroType = BobPhsx.MakeCustom(Base, Size, Jump, BobPhsxNormal.Instance, false);

            Hero.SetCustomPhsx(HeroPhsxData);
        }


        void CreateHeroes()
        {
            // Remove any previous bobs
            MyGame.KillToDo("RemoveBobs");
            foreach (Bob bob in MyGame.MyLevel.Bobs)
                bob.CollectSelf();

            // Make new bobs
            MyGame.MakeBobs(MyGame.MyLevel);
            
            // Position bobs
            Vector2 shift = new Vector2(-700, 100);
            foreach (Bob bob in MyGame.MyLevel.Bobs)
            {
                Tools.MoveTo(bob, MyGame.CamPos + shift);
                shift += new Vector2(100, 20);

                bob.ScreenWrap = true;
            }
        }

        void RemovePreviousGround()
        {
            MyGame.KillToDo("MoveOut");

            foreach (BlockBase block in MyGame.MyLevel.Blocks)
                block.CollectSelf();
        }

        void CreateGround()
        {
            MyGame.MyLevel.MyTileSet = "castle";

            NormalBlock block;

            foreach (NormalBlock _block in MyGame.MyLevel.Blocks)
                if (_block is NormalBlock)
                    _block.CollectSelf();

            block = (NormalBlock)MyGame.Recycle.GetObject(ObjectType.NormalBlock, false);
            block.Init(new Vector2(-1000, -3100), new Vector2(1100, 2500), MyGame.MyLevel.MyTileSetInfo);
            block.MyBox.TopOnly = false;
            MyGame.MyLevel.AddBlock(block);

            block = (NormalBlock)MyGame.Recycle.GetObject(ObjectType.NormalBlock, false);
            block.Init(new Vector2(1050, -2950), new Vector2(1000, 2500), MyGame.MyLevel.MyTileSetInfo);
            block.MyBox.TopOnly = false;
            MyGame.MyLevel.AddBlock(block);
        }

        protected override void SetHeaderProperties(Text text)
        {
            base.SetHeaderProperties(text);

            text.ShadowColor = new Color(.2f, .2f, .2f, .6f);
            text.Shadow = true;

            //text.Angle = CoreMath.Radians(23);
        }

        void SetSuperHeader(Text text)
        {
            base.SetHeaderProperties(text);
            text.MyFloatColor = new Vector4(1, 1, 1, 1);
            text.Scale = FontScale * 1.42f;
            
            text.Shadow = false;

            CkColorHelper._x_x_HappyBlueColor(text); 
            text.Scale *= 1.25f;
        }

        protected override void SetItemProperties(MenuItem item)
        {
            base.SetItemProperties(item);
            StartMenu.SetItemProperties_Red(item);
        }

        MenuList MakeList()
        {
            var list = new MenuList();

			list.Center = false;
            list.GrayOutOnUnselectable = true;

            return list;
        }

        protected override void AddItem(MenuItem item)
        {
            base.AddItem(item);

            MenuList list = item as MenuList;
            if (null != list)
            {
                list.SetIndex(0);
            }
        }

        BobPhsx Base, Jump, Size;

        MenuItem Start, Back;
        Text HeroText;
        MenuList BaseList, JumpList, SizeList;
        Text BaseHeader, JumpHeader, SizeHeader;

        void SetListItemProperties(MenuItem item)
        {
            SetItemProperties(item);
        }

        static int BaseListIndex = 0;
        static int SizeListIndex = 0;
        static int JumpListIndex = 0;

        public override void Init()
        {
            ItemShadows = false;

            SlideInFrom = SlideOutTo = PresetPos.Right;

            FontScale = .5f;

            MyPile = new DrawPile();
            MyMenu = new Menu(false);
            EnsureFancy();

            base.Init();

            Control = -1;
            MyMenu.OnB = null;

            ReturnToCallerDelay = 18;
            SlideInLength = 25;
            SlideOutLength = 24;

            CallDelay = 5;

            SelectedItemShift = new Vector2(0, 0);

            // Header
            Text HeaderText = new Text(Localization.Words.HeroFactory, Resources.Font_Grobold42);
            HeaderText.Name = "Header";
            SetSuperHeader(HeaderText);
            HeaderText.Pos = new Vector2(-1169.842f, 985.7144f);
            MyPile.Add(HeaderText);

            // Backdrop
            var backdrop = new QuadClass("Backplate_1500x900", 1500, true);
            backdrop.Name = "Backdrop";
            MyPile.Add(backdrop);
			EpilepsySafe(.5f);

            MenuItem item;

            // Hero lists
            BaseHeader = HeroText = new Text(Localization.Words.Base, ItemFont);
            HeroText.Name = "base";
            SetHeaderProperties(HeroText);
            MyPile.Add(HeroText);

            BaseList = MakeList();
			BaseList.MyExpandPos = new Vector2(-1139.167f, 499.4444f);
            BaseList.Name = "base";
            foreach (Hero_BaseType _hero in Tools.GetValues<Hero_BaseType>())
            {
				if (_hero == Hero_BaseType.Meat) continue;

                BobPhsx hero = BobPhsx.GetPhsx(_hero);

                item = new MenuItem(new Text(hero.Name, ItemFont, false, true));
                SetListItemProperties(item);
                BaseList.AddItem(item, hero);
            }
            AddItem(BaseList);

            // Hero jump
            JumpHeader = HeroText = new Text(Localization.Words.Jump, ItemFont);
            HeroText.Name = "jump";
            SetHeaderProperties(HeroText);
            MyPile.Add(HeroText);

            JumpList = MakeList();
			JumpList.MyExpandPos = new Vector2(-1130.278f, 152.5f);
            JumpList.Name = "jump";
            foreach (Hero_MoveMod _hero in Tools.GetValues<Hero_MoveMod>())
            {
                BobPhsx hero = BobPhsx.GetPhsx(_hero);

                item = new MenuItem(new Text(hero.Name, ItemFont, false, true));
                SetListItemProperties(item);
                JumpList.AddItem(item, hero);
            }
            AddItem(JumpList);

            // Hero shape
            SizeHeader = HeroText = new Text(Localization.Words.Shape, ItemFont);
            HeroText.Name = "size";
            SetHeaderProperties(HeroText);
            MyPile.Add(HeroText);
            
            SizeList = MakeList();
			SizeList.MyExpandPos = new Vector2(-1118.333f, -173.3333f);
            SizeList.Name = "size";
            foreach (Hero_Shape _hero in Tools.GetValues<Hero_Shape>())
            {
                BobPhsx hero = BobPhsx.GetPhsx(_hero);

                item = new MenuItem(new Text(hero.Name, ItemFont, false, true));
                SetListItemProperties(item);
                SizeList.AddItem(item, hero);
            }
            
            AddItem(SizeList);

            SetListActions();

            BaseList.SetIndex(BaseListIndex);
            SizeList.SetIndex(SizeListIndex);
            JumpList.SetIndex(JumpListIndex);

            FontScale = 1f;

            MakeSliders();

            // Start/test/back/advanced
            MakeOptionItems();

            // Select the first item in the menu to start
            MyMenu.SelectItem(0);
            MyMenu.OnB = MenuReturnToCaller;

            UpdateSliders();

            SetPos();

			foreach (MenuItem _item in BaseList.MyList) ScaleListItem(_item);
			foreach (MenuItem _item in SizeList.MyList) ScaleListItem(_item);
			foreach (MenuItem _item in JumpList.MyList) ScaleListItem(_item);
        }

		void ScaleListItem(MenuItem item)
		{
			const float max_width = 790; //750

			float ListScale = .9f;
			if (Localization.CurrentLanguage.MyLanguage == Localization.Language.German)
				ListScale = .825f;

			item.MyText.Scale = .45f * ListScale;
			item.MySelectedText.Scale = .45f * ListScale;

			float width = item.MyText.GetWorldWidth();
			if (width > max_width)
				item.ScaleText(max_width / width);
		}

        private void SetListActions()
        {
            BaseList.OnIndexSelect = UpdateBaseHero;
            SizeList.OnIndexSelect = UpdateSizeHero;
            JumpList.OnIndexSelect = UpdateJumpHero;
        }

        void UpdateSliders()
        {
            if (GravitySlider == null) return;

            BaseList.Selectable = true;
            JumpList.Selectable = true;
            SizeList.Selectable = true;

            GravitySlider.Selectable = true;
            MaxSpeedSlider.Selectable = true;
            AccelSlider.Selectable = true;
            MaxFallSpeedSlider.Selectable = true;
            JumpLengthSlider.Selectable = true;
            JumpAccelSlider.Selectable = true;
            DoubleJumpLengthSlider.Selectable = true;
            DoubleJumpAccelSlider.Selectable = true;
            FrictionSlider.Selectable = true;
            JetPackSlider.Selectable = true;
            JetPackFuelSlider.Selectable = true;
            NumJumpsSlider.Selectable = true;
            SizeSlider.Selectable = true;
            PhasedSizeSlider.Selectable = true;
            PhasedGravitySlider.Selectable = true;
            PhasePeriodSlider.Selectable = true;

            // Base
            if (Base == BobPhsxNormal.Instance ||
                Base == BobPhsxWheel.Instance ||
                Base == BobPhsxBox.Instance ||
                Base == BobPhsxBouncy.Instance ||
                Base == BobPhsxMeat.Instance ||
                Base == BobPhsxRocketbox.Instance)
            {

            }
            else if (Base == BobPhsxSpaceship.Instance)
            {
                GravitySlider.Selectable &= false;
                MaxFallSpeedSlider.Selectable &= false;
                JumpLengthSlider.Selectable &= false;
                JumpAccelSlider.Selectable &= false;
                DoubleJumpLengthSlider.Selectable &= false;
                DoubleJumpAccelSlider.Selectable &= false;
                JetPackSlider.Selectable &= false;
                JetPackFuelSlider.Selectable &= false;
                NumJumpsSlider.Selectable &= false;

                JumpList.Selectable &= false;
            }
            else if (Base == BobPhsxMeat.Instance)
            {

            }

            // Jump 
            if (Jump == BobPhsxNormal.Instance)
            {
                DoubleJumpLengthSlider.Selectable &= false;
                DoubleJumpAccelSlider.Selectable &= false;
                NumJumpsSlider.Selectable &= false;
                JetPackSlider.Selectable &= false;
                JetPackFuelSlider.Selectable &= false;
            }
            else if (Jump == BobPhsxDouble.Instance)
            {
                JetPackSlider.Selectable &= false;
                JetPackFuelSlider.Selectable &= false;
            }
            else if (Jump == BobPhsxJetman.Instance)
            {
                DoubleJumpLengthSlider.Selectable &= false;
                DoubleJumpAccelSlider.Selectable &= false;
                NumJumpsSlider.Selectable &= false;
            }
            else if (Jump == BobPhsxInvert.Instance)
            {
                DoubleJumpLengthSlider.Selectable &= false;
                DoubleJumpAccelSlider.Selectable &= false;
                NumJumpsSlider.Selectable &= false;
                JetPackSlider.Selectable &= false;
                JetPackFuelSlider.Selectable &= false;

				BaseList.Selectable &= false;
            }

            // Shape
            if (Size == BobPhsxNormal.Instance ||
                Size == BobPhsxBig.Instance ||
                Size == BobPhsxSmall.Instance)
            {
                PhasedSizeSlider.Selectable &= false;
                PhasedGravitySlider.Selectable &= false;
                PhasePeriodSlider.Selectable &= false;
            }
            else if (Size == BobPhsxScale.Instance)
            {

            }
        }

        //PhsxSlider GravitySlider, MaxSpeedSlider, AccelSlider;
        //PhsxSlider MaxFallSpeedSlider, JumpLengthSlider, JumpAccelSlider, DoubleJumpLengthSlider, DoubleJumpAccelSlider, FrictionSlider, JetPackSlider, JetPackFuelSlider, NumJumpsSlider;
        //PhsxSlider SizeSlider, PhasedSizeSlider, PhasedGravitySlider, PhasePeriodSlider;

        void UpdateBaseHero()
        {
            Base = BaseList.CurObj as BobPhsx;
            BaseListIndex = BaseList.ListIndex;

            UpdateSliders();
        }

        void UpdateSizeHero()
        {
            Size = SizeList.CurObj as BobPhsx;
            SizeListIndex = SizeList.ListIndex;

            UpdateSliders();
        }

        void UpdateJumpHero()
        {
            Jump = JumpList.CurObj as BobPhsx;
            JumpListIndex = JumpList.ListIndex;

            UpdateSliders();
        }

        void SetPos()
        {
			MenuItem _item;
			_item = MyMenu.FindItemByName("base"); if (_item != null) { _item.SetPos = new Vector2(-2252.702f, 575.954f); _item.MyText.Scale = 0.45f; _item.MySelectedText.Scale = 0.45f; _item.SelectIconOffset = new Vector2(0f, 0f); }
			_item = MyMenu.FindItemByName("jump"); if (_item != null) { _item.SetPos = new Vector2(-2252.702f, 254.3771f); _item.MyText.Scale = 0.45f; _item.MySelectedText.Scale = 0.45f; _item.SelectIconOffset = new Vector2(0f, 0f); }
			_item = MyMenu.FindItemByName("size"); if (_item != null) { _item.SetPos = new Vector2(-2252.702f, -60.48386f); _item.MyText.Scale = 0.45f; _item.MySelectedText.Scale = 0.45f; _item.SelectIconOffset = new Vector2(0f, 0f); }
			_item = MyMenu.FindItemByName("test"); if (_item != null) { _item.SetPos = new Vector2(-2189.357f, -132.3662f); _item.MyText.Scale = 0.5196667f; _item.MySelectedText.Scale = 0.5196667f; _item.SelectIconOffset = new Vector2(0f, 0f); }
			_item = MyMenu.FindItemByName("back"); if (_item != null) { _item.SetPos = new Vector2(-2189.357f, -273.889f); _item.MyText.Scale = 0.4939956f; _item.MySelectedText.Scale = 0.4939956f; _item.SelectIconOffset = new Vector2(0f, 0f); }
			_item = MyMenu.FindItemByName("continue"); if (_item != null) { _item.SetPos = new Vector2(-2189.357f, -415.4118f); _item.MyText.Scale = 0.5253342f; _item.MySelectedText.Scale = 0.5253342f; _item.SelectIconOffset = new Vector2(0f, 0f); }
			_item = MyMenu.FindItemByName("reset"); if (_item != null) { _item.SetPos = new Vector2(-2189.357f, -556.9346f); _item.MyText.Scale = 0.4984665f; _item.MySelectedText.Scale = 0.4984665f; _item.SelectIconOffset = new Vector2(0f, 0f); }

			MyMenu.Pos = new Vector2(1166.862f, -69.45605f);

			Text _t;
			_t = MyPile.FindText("Header"); if (_t != null) { _t.Pos = new Vector2(-664.2021f, 960.7101f); _t.Scale = 0.8010691f; }
			_t = MyPile.FindText("base"); if (_t != null) { _t.Pos = new Vector2(-1223.304f, 764.0166f); _t.Scale = 0.5947744f; }
			_t = MyPile.FindText("jump"); if (_t != null) { _t.Pos = new Vector2(-1223.304f, 462.8152f); _t.Scale = 0.5887732f; }
			_t = MyPile.FindText("size"); if (_t != null) { _t.Pos = new Vector2(-1223.304f, 145.6873f); _t.Scale = 0.5638525f; }

			QuadClass _q;
			_q = MyPile.FindQuad("Backdrop"); if (_q != null) { _q.Pos = new Vector2(0f, 0f); _q.Size = new Vector2(1500f, 1083.871f); }
			_q = MyPile.FindQuad("Button_A"); if (_q != null) { _q.Pos = new Vector2(-1094.627f, -311.157f); _q.Size = new Vector2(63.74656f, 63.74656f); }
			_q = MyPile.FindQuad("Button_B"); if (_q != null) { _q.Pos = new Vector2(-1094.627f, -447.2965f); _q.Size = new Vector2(63.74656f, 63.74656f); }
			_q = MyPile.FindQuad("Button_X"); if (_q != null) { _q.Pos = new Vector2(-1094.627f, -583.436f); _q.Size = new Vector2(63.74656f, 63.74656f); }
			_q = MyPile.FindQuad("Button_Y"); if (_q != null) { _q.Pos = new Vector2(-1094.627f, -719.5756f); _q.Size = new Vector2(63.74656f, 63.74656f); }

			MyPile.Pos = new Vector2(0f, 0f);
		}

        MenuItem A, B, X, Y;
        private void MakeOptionItems()
        {
            MenuItem item;

            float bigscale = .52f;
            float scale = .52f;

			bool WithButtonPics;
if (ButtonCheck.ControllerInUse)
{
			WithButtonPics = true;
}
else
{
			WithButtonPics = false;            
}

            // Start
            A = Start = item = new MenuItem(new Text(Localization.Words.Play, ItemFont));
            item.Name = "test";
            item.JiggleOnGo = false;
            AddItem(item);
			item.Go = Cast.ToItem(Next);
if (ButtonCheck.ControllerInUse)
{
#if XBOX || PC
			Menu.DefaultMenuInfo.SetNext(item);
#endif
			MyPile.Add(new QuadClass(ButtonTexture.Go, 90, "Button_A"));
            item.Selectable = false;
}

            // Select 'Start Level' when the user presses (A)
            MyMenu.OnA = Cast.ToMenu(Start.Go);


            // Back
            B = Back = item = new MenuItem(new Text(Localization.Words.Back, ItemFont));
            item.Name = "back";
            AddItem(item);
            item.SelectSound = null;
            item.Go = Cast.ToItem(ReturnToCaller);
if (ButtonCheck.ControllerInUse)
{
#if XBOX || PC
			Menu.DefaultMenuInfo.SetBack(item);
#endif
			MyPile.Add(new QuadClass(ButtonTexture.Back, 90, "Button_B"));
            item.Selectable = false;
}

            // Continue
            X = item = new MenuItem(new Text(Localization.Words.Test, ItemFont));
            item.Name = "continue";
            AddItem(item);
            item.SelectSound = null;
			item.Go = Cast.ToItem(StartTest);
if (ButtonCheck.ControllerInUse)
{
#if XBOX || PC
			Menu.DefaultMenuInfo.SetX(item);
#endif
			MyPile.Add(new QuadClass(ButtonTexture.X, 90, "Button_X"));
            item.Selectable = false;
            MyMenu.OnX = Cast.ToMenu(X.Go);
}

            // Reset
            item = ResetButton = new MenuItem(new Text(Localization.Words.Reset, ItemFont));
            item.Name = "reset";
            AddItem(item);
			item.Go = Cast.ToItem(ResetSliders);
if (ButtonCheck.ControllerInUse)
{
#if XBOX || PC
			Menu.DefaultMenuInfo.SetY(item);
#endif
			MyPile.Add(new QuadClass(ButtonTexture.Y, 90, "Button_Y"));
            item.Selectable = false;
            MyMenu.OnY = Cast.ToAction(item.Go);
}
        }

        bool AdvancedAvailable()
        {
            if (Base is BobPhsxSpaceship && !(Size is BobPhsxScale))
                return false;
            else
                return true;
        }

        void ResetAdvancedSliders()
        {
            MaxFallSpeedSlider.Show = JumpLengthSlider.Show = JumpAccelSlider.Show = FrictionSlider.Show = true;

            DoubleJumpAccelSlider.Show = DoubleJumpLengthSlider.Show = NumJumpsSlider.Show = Jump is BobPhsxDouble && Base is BobPhsxNormal;

            JetPackFuelSlider.Show = JetPackSlider.Show = Jump is BobPhsxJetman && Base is BobPhsxNormal;

            SizeSlider.Show = PhasedSizeSlider.Show = PhasedGravitySlider.Show = PhasePeriodSlider.Show = Size is BobPhsxScale;

            GravitySlider.Show = Base is BobPhsxNormal;
        }

        MenuItem ResetButton;

        void Next()
        {
            if (CustomLevel.IsCustomDifficulty())
            {
                MakeBobPhsx();
                CustomLevel.LevelSeed.DefaultHeroType = Hero;

                CustomLevel.CallingPanel = new PassiveUpgrades_GUI(CustomLevel.PieceSeed, CustomLevel);
                Call(CustomLevel.CallingPanel, 0);
                Hide(PresetPos.Left);
                this.SlideInFrom = PresetPos.Left;
            }
            else
            {
				CloudberryKingdomGame.Freeplay_Count++;
				if (CloudberryKingdomGame.IsDemo && CloudberryKingdomGame.Freeplay_Count >= CloudberryKingdomGame.Freeplay_Max)
				{
					Call(new UpSellMenu(Localization.Words.UpSell_FreePlay, MenuItem.ActivatingPlayer));
					Hide(PresetPos.Right, 0);

					return;
				}

                MyGame.PlayGame(StartLevel);
            }
        }

        void StartLevel()
        {
            MakeBobPhsx();
            CustomLevel.LevelSeed.DefaultHeroType = Hero;
            CustomLevel.StartLevelFromMenuData();
        }

        void ResetSliders()
        {
            foreach (var item in MyMenu.Items)
            {
                PhsxSlider slider = item as PhsxSlider;
                if (null != slider)
                    slider.Reset();
            }
        }

        public override void OnReturnTo()
        {
            base.OnReturnTo();
        }

        protected override void MyPhsxStep()
        {
            if (Testing)
            {
                TestingPhsx();
                return;
            }

            base.MyPhsxStep();
        }
    }
}