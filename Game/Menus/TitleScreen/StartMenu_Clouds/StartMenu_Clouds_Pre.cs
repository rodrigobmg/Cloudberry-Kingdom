using Microsoft.Xna.Framework;

namespace CloudberryKingdom
{
    public class StartMenu_Clouds_Pre : StartMenu
    {
        bool GameIsDemo;

        bool CallingOptionsMenu;
        protected override void MenuGo_Options(MenuItem item)
        {
            Title.BackPanel.SetState(TitleBackgroundState.Scene_Blur_Dark);
            Call(new StartMenu_Clouds_Options(Control, true), 0);
            CallingOptionsMenu = true;
        }

        protected override void Exit()
        {
            SelectSound = null;
            Title.BackPanel.SetState(TitleBackgroundState.Scene_Blur_Dark);

			if (CloudberryKingdomGame.IsDemo)
			{
				Call(new UpSellMenu(Localization.Words.UpSell_Exit, MenuItem.ActivatingPlayer));
			}
			else
			{
				Call(new StartMenu_Clouds_Exit(Control), 0);
			}
        }

        protected override void BringNextMenu()
        {
            base.BringNextMenu();

            Hide();
        }

        public TitleGameData_Clouds Title;
        public StartMenu_Clouds_Pre(TitleGameData_Clouds Title) : base()
        {
            this.Title = Title;
            CallingOptionsMenu = false;
        }

        protected override void MyPhsxStep()
        {
			// Check to see if we just unlocked the full game (was demo, now it isn't)
            if (GameIsDemo && !CloudberryKingdomGame.IsDemo)
            {
                GameIsDemo = false;

                // Hide 'Buy' option
                var _item = MyMenu.FindItemByName("Buy");
                if (_item != null)
                {
                    _item.Show = false;
                    _item.Selectable = false;
                    _item.SetPos = new Vector2(200000f, -200000);
                }

				MyMenu.SelectItem(0);

                // Reset position of items
                SetPos();
            }

            base.MyPhsxStep();
        }

        public override void SlideIn(int Frames)
        {
            Title.BackPanel.SetState(TitleBackgroundState.Scene_Title);
            base.SlideIn(0);
        }

        public override void SlideOut(PresetPos Preset, int Frames)
        {
            base.SlideOut(Preset, 0);
        }

        protected override void SetItemProperties(MenuItem item)
        {
            base.SetItemProperties(item);

            item.MySelectedText.Shadow = item.MyText.Shadow = false;
        }

        public override void OnAdd()
        {
			CloudberryKingdomGame.SetPresence(CloudberryKingdomGame.Presence.TitleScreen);

            base.OnAdd();
        }

        public override void OnReturnTo()
        {
			CloudberryKingdomGame.SetPresence(CloudberryKingdomGame.Presence.TitleScreen);

            if (CallingOptionsMenu)
            {
				MyMenu.ReadyToPlaySound = false;
                MyMenu.SelectItem(4);
				MyMenu.ReadyToPlaySound = true;
                
				CallingOptionsMenu = false;
            }

            base.OnReturnTo();
        }

        public override bool MenuReturnToCaller(Menu menu)
        {
            if (NoBack) return false;

            return base.MenuReturnToCaller(menu);
        }

        public override void Init()
        {
 	        base.Init();

            CallDelay = ReturnToCallerDelay = 0;
            MyMenu.OnB = MenuReturnToCaller;

            BackBox = new QuadClass("Title_Strip");
            BackBox.Alpha = .9f;
            MyPile.Add(BackBox, "Back");

            //MyPile.FadeIn(.33f);

            SetPos();

            // Burn one
            //MyPhsxStep();
        }

        void MenuGo_Play(MenuItem item)
        {
#if PC || WINDOWS
			Call(new StartMenu_Clouds(Title), 0);
#else
			if (CloudberryKingdomGame.ProfilesAvailable())
			{
				Call(new StartMenu_Clouds(Title), 0);
			}
			else
			{
				CloudberryKingdomGame.ShowError_MustBeSignedIn(Localization.Words.Err_MustBeSignedInToPlay);
			}

			//if (CloudberryKingdomGame.OnlineFunctionalityAvailable())
			//{
			//    Call(new StartMenu_Clouds(Title), 0);
			//}
			//else
			//{
			//    CloudberryKingdomGame.ShowError_MustBeSignedIn(Localization.Words.Err_MustBeSignedInToPlay);
			//}
#endif
        }

        void MenuGo_Leaderboards(MenuItem item)
        {
            if (CloudberryKingdomGame.OnlineFunctionalityAvailable(MenuItem.ActivatingPlayerIndex()))
            {
#if DEBUG && WINDOWS && XBOX
				Call(new LeaderboardGUI(Title, null, MenuItem.ActivatingPlayer), 0);
#elif XBOX
                var gamer = CloudberryKingdomGame.IndexToSignedInGamer(MenuItem.ActivatingPlayerIndex());
                if (gamer != null)
                {
					Challenge.LeaderboardIndex = ArcadeMenu.LeaderboardIndex(null, null);
                    Call(new LeaderboardGUI(Title, gamer, MenuItem.ActivatingPlayer), 0);
                }
#else
				Call(new LeaderboardGUI(Title, MenuItem.ActivatingPlayer), 0);
#endif
            }
            else
            {
                CloudberryKingdomGame.ShowError_MustBeSignedInToLive(Localization.Words.Err_MustBeSignedInToLive);
            }
        }

        void MenuGo_Achievements(MenuItem item)
        {
            if (MenuItem.ActivatingPlayer < 0 || MenuItem.ActivatingPlayer > 3) return;

#if XBOX
            if (PlayerManager.Get(MenuItem.ActivatingPlayer).MyGamer != null)
			{
				CloudberryKingdomGame.ShowAchievements = true;
				CloudberryKingdomGame.ShowFor = (PlayerIndex)MenuItem.ActivatingPlayer;
			}
			else
			{
				CloudberryKingdomGame.ShowError_MustBeSignedIn(Localization.Words.Err_MustBeSignedIn);
			}
#endif
        }

        void MenuGo_BuyGame(MenuItem item)
        {
            CloudberryKingdomGame.ShowMarketplace = true;
#if PC
#else
            CloudberryKingdomGame.ShowFor = (PlayerIndex)MenuItem.ActivatingPlayer;
#endif
        }

        protected override void MakeMenu()
        {
			GameIsDemo = CloudberryKingdomGame.IsDemo;

            MenuItem item;

            // Play
            item = new MenuItem(new Text(Localization.Words.PlayGame, ItemFont, true));
            item.Name = "Play";
            item.Go = MenuGo_Play;
            AddItem(item);

            // Leaderboard
            item = new MenuItem(new Text(Localization.Words.Leaderboards, ItemFont, true));
            item.Name = "Leaderboard";
            item.Go = MenuGo_Leaderboards;
            AddItem(item);

            // Achievements
            item = new MenuItem(new Text(Localization.Words.Achievements, ItemFont, true));
            item.Name = "Achievements";
            item.Go = MenuGo_Achievements;
            AddItem(item);

            //// Options
            //item = new MenuItem(new Text(Localization.Words.Options, ItemFont, true));
            //item.Name = "Options";
            //item.Go = MenuGo_Options;
            //AddItem(item);

			if (GameIsDemo)
			{
				// Buy Game
				item = new MenuItem(new Text(Localization.Words.UnlockFullGame, ItemFont, true));
				item.Name = "Buy";
				item.Go = MenuGo_BuyGame;
				AddItem(item);
			}

			// Credits
			//item = new MenuItem(new Text(Localization.Words.Credits, ItemFont, true));
			//item.Name = "Buy";
			//item.Go = MenuGo_Credits;
			//AddItem(item);

            // Exit
            item = new MenuItem(new Text(Localization.Words.Exit, ItemFont, true));
            item.Name = "Exit";
            item.Go = MenuGo_Exit;
            AddItem(item);

            EnsureFancy();

            this.CallToLeft = true;
        }

        protected QuadClass BackBox;

        void SetPos()
        {
            BackBox.TextureName = "White";
            BackBox.Quad.SetColor(ColorHelper.Gray(.1f));
            BackBox.Alpha = .73f;

			if (GameIsDemo)
			{
				MenuItem _item;
				_item = MyMenu.FindItemByName("Play"); if (_item != null) { _item.SetPos = new Vector2(0f, 168.3334f); _item.MyText.Scale = 0.605f; _item.MySelectedText.Scale = 0.605f; _item.SelectIconOffset = new Vector2(0f, 0f); }
				_item = MyMenu.FindItemByName("Leaderboard"); if (_item != null) { _item.SetPos = new Vector2(0f, -29.22222f); _item.MyText.Scale = 0.605f; _item.MySelectedText.Scale = 0.605f; _item.SelectIconOffset = new Vector2(0f, 0f); }
				_item = MyMenu.FindItemByName("Achievements"); if (_item != null) { _item.SetPos = new Vector2(0f, -229.5555f); _item.MyText.Scale = 0.605f; _item.MySelectedText.Scale = 0.605f; _item.SelectIconOffset = new Vector2(0f, 0f); }
				_item = MyMenu.FindItemByName("Buy"); if (_item != null) { _item.SetPos = new Vector2(0f, -435.7777f); _item.MyText.Scale = 0.605f; _item.MySelectedText.Scale = 0.605f; _item.SelectIconOffset = new Vector2(0f, 0f); }
				_item = MyMenu.FindItemByName("Exit"); if (_item != null) { _item.SetPos = new Vector2(0f, -624.9999f); _item.MyText.Scale = 0.605f; _item.MySelectedText.Scale = 0.605f; _item.SelectIconOffset = new Vector2(0f, 0f); }

				MyMenu.Pos = new Vector2(-91.66698f, -19.4445f);

				QuadClass _q;
				_q = MyPile.FindQuad("Back"); if (_q != null) { _q.Pos = new Vector2(-61.11133f, -336.1111f); _q.Size = new Vector2(608.4988f, 520.8323f); }

				MyPile.Pos = new Vector2(-27.77734f, -33.33337f);
			}
			else
			{
				if (Localization.CurrentLanguage.MyLanguage == Localization.Language.Chinese)
				{
					MenuItem _item;
					_item = MyMenu.FindItemByName("Play"); if (_item != null) { _item.SetPos = new Vector2(0f, 168.3334f); _item.MyText.Scale = 0.7549162f; _item.MySelectedText.Scale = 0.7549162f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Leaderboard"); if (_item != null) { _item.SetPos = new Vector2(0f, -54.22218f); _item.MyText.Scale = 0.7549162f; _item.MySelectedText.Scale = 0.7549162f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Achievements"); if (_item != null) { _item.SetPos = new Vector2(0f, -276.7778f); _item.MyText.Scale = 0.7549162f; _item.MySelectedText.Scale = 0.7549162f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Exit"); if (_item != null) { _item.SetPos = new Vector2(0f, -499.3334f); _item.MyText.Scale = 0.7549162f; _item.MySelectedText.Scale = 0.7549162f; _item.SelectIconOffset = new Vector2(0f, 0f); }

					MyMenu.Pos = new Vector2(-80.55576f, -44.44453f);

					QuadClass _q;
					_q = MyPile.FindQuad("Back"); if (_q != null) { _q.Pos = new Vector2(-61.11133f, -336.1111f); _q.Size = new Vector2(541.4985f, 466.9143f); }

					MyPile.Pos = new Vector2(-27.77734f, -33.33337f);
				}
				else if (Localization.CurrentLanguage.MyLanguage == Localization.Language.Japanese)
				{
					MenuItem _item;
					_item = MyMenu.FindItemByName("Play"); if (_item != null) { _item.SetPos = new Vector2(0f, 168.3334f); _item.MyText.Scale = 0.7559164f; _item.MySelectedText.Scale = 0.7559164f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Leaderboard"); if (_item != null) { _item.SetPos = new Vector2(0f, -54.22218f); _item.MyText.Scale = 0.7559164f; _item.MySelectedText.Scale = 0.7559164f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Achievements"); if (_item != null) { _item.SetPos = new Vector2(0f, -276.7778f); _item.MyText.Scale = 0.7559164f; _item.MySelectedText.Scale = 0.7559164f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Exit"); if (_item != null) { _item.SetPos = new Vector2(0f, -499.3334f); _item.MyText.Scale = 0.7559164f; _item.MySelectedText.Scale = 0.7559164f; _item.SelectIconOffset = new Vector2(0f, 0f); }

					MyMenu.Pos = new Vector2(-77.77793f, -38.88896f);

					QuadClass _q;
					_q = MyPile.FindQuad("Back"); if (_q != null) { _q.Pos = new Vector2(-61.11133f, -336.1111f); _q.Size = new Vector2(541.4985f, 466.9143f); }

					MyPile.Pos = new Vector2(-27.77734f, -33.33337f);
				}
				else
				{
					MenuItem _item;
					_item = MyMenu.FindItemByName("Play"); if (_item != null) { _item.SetPos = new Vector2(0f, 168.3334f); _item.MyText.Scale = 0.6669163f; _item.MySelectedText.Scale = 0.6669163f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Leaderboard"); if (_item != null) { _item.SetPos = new Vector2(0f, -54.22218f); _item.MyText.Scale = 0.6669163f; _item.MySelectedText.Scale = 0.6669163f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Achievements"); if (_item != null) { _item.SetPos = new Vector2(0f, -276.7778f); _item.MyText.Scale = 0.6669163f; _item.MySelectedText.Scale = 0.6669163f; _item.SelectIconOffset = new Vector2(0f, 0f); }
					_item = MyMenu.FindItemByName("Exit"); if (_item != null) { _item.SetPos = new Vector2(0f, -499.3334f); _item.MyText.Scale = 0.6669163f; _item.MySelectedText.Scale = 0.6669163f; _item.SelectIconOffset = new Vector2(0f, 0f); }

					MyMenu.Pos = new Vector2(-80.55576f, -83.33342f);

					QuadClass _q;
					_q = MyPile.FindQuad("Back"); if (_q != null) { _q.Pos = new Vector2(-61.11133f, -336.1111f); _q.Size = new Vector2(541.4985f, 466.9143f); }

					MyPile.Pos = new Vector2(-27.77734f, -33.33337f);
				}
			}
        }
    }
}