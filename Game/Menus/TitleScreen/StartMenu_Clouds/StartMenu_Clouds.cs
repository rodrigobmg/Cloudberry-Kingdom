using Microsoft.Xna.Framework;

namespace CloudberryKingdom
{
    public class StartMenu_Clouds : StartMenu
    {
        bool CallingOptionsMenu;

		protected override void MenuGo_Campaign(MenuItem item)
		{
			// Upsell
			if (CloudberryKingdomGame.IsDemo)
			{
				Title.BackPanel.SetState(TitleBackgroundState.Scene_Blur_Dark);
				Call(new UpSellMenu(Localization.Words.UpSell_Campaign, MenuItem.ActivatingPlayer));

				return;
			}

			base.MenuGo_Campaign(item);
		}

        protected override void MenuGo_Options(MenuItem item)
        {
#if XBOX
            // Player needs a storage device
            if (MenuItem.ActivatingPlayer >= 0 && CoreStorage.Device[MenuItem.ActivatingPlayer] != null)
            {
                CoreStorage.Device[MenuItem.ActivatingPlayer].NeedsConnection = true;
            }
#endif
			Title.BackPanel.SetState(TitleBackgroundState.Scene_Blur_Dark);
            Call(new StartMenu_Clouds_Options(Control, true), 0);
            CallingOptionsMenu = true;
        }

        protected override void Exit()
        {
            SelectSound = null;
            Title.BackPanel.SetState(TitleBackgroundState.Scene_Blur_Dark);
            Call(new StartMenu_Clouds_Exit(Control), 0);
        }

        protected override void BringNextMenu()
        {
            base.BringNextMenu();

            Hide();
        }

        public TitleGameData_Clouds Title;
        public StartMenu_Clouds(TitleGameData_Clouds Title) : base()
        {
			EnableBounce();
			ReturnToCallerDelay = 0;


            this.Title = Title;
            CallingOptionsMenu = false;
        }

        public override void SlideIn(int Frames)
        {
            Title.BackPanel.SetState(TitleBackgroundState.Scene_Title);
            base.SlideIn(0);
        }

        public override void SlideOut(PresetPos Preset, int Frames)
        {
			base.SlideOut(Preset, 0);
			
			base.SlideOut(Preset, Frames);
        }

        protected override void BringCampaign()
        {
            base.BringCampaign();

            Call(new StartMenu_Clouds_Campaign(Title));
        }

        protected override void BringArcade()
        {
            base.BringArcade();

            Call(new StartMenu_Clouds_Arcade(Title));
        }

        protected override void BringFreeplay()
        {
            base.BringFreeplay();

            SkipCallSound = true;
            Call(new StartMenu_Clouds_CustomLevel(Title));
        }

        protected override void SetItemProperties(MenuItem item)
        {
            SetItemProperties_White(item);
            item.MySelectedText.Shadow = item.MyText.Shadow = false;
        }

        public override void OnAdd()
        {
			CloudberryKingdomGame.SetPresence(CloudberryKingdomGame.Presence.TitleScreen);

            base.OnAdd();
        }

		public override void Call(GUI_Panel child, int Delay)
		{
			UseBounce = false;
			ReturnToCallerDelay = 0;

			base.Call(child, Delay);
		}

        public override void OnReturnTo()
        {
			UseBounce = false;
			ReturnToCallerDelay = 0;

			CloudberryKingdomGame.SetPresence(CloudberryKingdomGame.Presence.TitleScreen);

            if (CallingOptionsMenu)
            {
				MyMenu.ReadyToPlaySound = false;
				if (CloudberryKingdomGame.MainMenuType == MainMenuTypes.WiiU)
				{
					MyMenu.SelectItem(3);
				}
				else if (CloudberryKingdomGame.MainMenuType == MainMenuTypes.Xbox)
				{
					MyMenu.SelectItem(3);
				}
				else
				{
					MyMenu.SelectItem(2);
				}
				MyMenu.ReadyToPlaySound = true;

                CallingOptionsMenu = false;
            }

            base.OnReturnTo();
        }


		public override void ReturnToCaller()
		{
            if (NoBack) return;

			UseBounce = false;
			ReturnToCallerDelay = 0;

            base.ReturnToCaller();
        }

        public override void Init()
        {
 	        base.Init();

            CallDelay = ReturnToCallerDelay = 0;
            MyMenu.OnB = MenuReturnToCaller;

            SetPos();
        }

        protected override void MakeMenu()
        {
            MenuItem item;

            // Arcade
            item = new MenuItem(new Text(Localization.Words.TheArcade, ItemFont, true));
            item.Name = "Arcade";
            item.Go = MenuGo_Arcade;
            AddItem(item);

            // Campaign
            item = new MenuItem(new Text(Localization.Words.StoryMode, ItemFont, true));
            item.Name = "Campaign";
            AddItem(item);
            item.Go = MenuGo_Campaign;

            // Free Play
            item = new MenuItem(new Text(Localization.Words.FreePlay, ItemFont, true));
            item.Name = "Freeplay";
            item.Go = MenuGo_Freeplay;
            AddItem(item);

            // Options
            item = new MenuItem(new Text(
#if XBOX
				Localization.Words.Options_Xbox
#else		
				Localization.Words.Options
#endif
				, ItemFont, true));
            item.Name = "Options";
            item.Go = MenuGo_Options;
            AddItem(item);

			// Exit
            item = new MenuItem(new Text(Localization.Words.Back, ItemFont, true));
            item.Name = "Exit";
            item.Go = ItemReturnToCaller;
            AddItem(item);

            EnsureFancy();

            this.CallToLeft = true;
        }

        protected virtual void SetPos()
        {
			MenuItem _item;
			_item = MyMenu.FindItemByName("Arcade"); if (_item != null) { _item.SetPos = new Vector2(0f, 365.5279f); _item.MyText.Scale = 0.6517504f; _item.MySelectedText.Scale = 0.6517504f; _item.SelectIconOffset = new Vector2(0f, 0f); }
			_item = MyMenu.FindItemByName("Campaign"); if (_item != null) { _item.SetPos = new Vector2(0f, 174.1946f); _item.MyText.Scale = 0.6517504f; _item.MySelectedText.Scale = 0.6517504f; _item.SelectIconOffset = new Vector2(0f, 0f); }
			_item = MyMenu.FindItemByName("Freeplay"); if (_item != null) { _item.SetPos = new Vector2(0f, -17.1387f); _item.MyText.Scale = 0.6517504f; _item.MySelectedText.Scale = 0.6517504f; _item.SelectIconOffset = new Vector2(0f, 0f); }
			_item = MyMenu.FindItemByName("Options"); if (_item != null) { _item.SetPos = new Vector2(0f, -208.472f); _item.MyText.Scale = 0.6517504f; _item.MySelectedText.Scale = 0.6517504f; _item.SelectIconOffset = new Vector2(0f, 0f); }
			_item = MyMenu.FindItemByName("Exit"); if (_item != null) { _item.SetPos = new Vector2(0f, -399.8053f); _item.MyText.Scale = 0.6517504f; _item.MySelectedText.Scale = 0.6517504f; _item.SelectIconOffset = new Vector2(0f, 0f); }

			MyMenu.Pos = new Vector2(-80.55566f, -219.4445f);

			MyPile.Pos = new Vector2(-27.77734f, -33.33337f);
        }
    }
}