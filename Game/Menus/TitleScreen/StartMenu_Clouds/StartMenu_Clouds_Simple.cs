using Microsoft.Xna.Framework;

namespace CloudberryKingdom
{
    public class StartMenu_Clouds_Simple : StartMenu_Clouds
    {
        public StartMenu_Clouds_Simple(TitleGameData_Clouds Title)
            : base(Title)
        {
        }

        protected override void BringCharacterSelect()
        {
            UseBounce = false;
            ReturnToCallerDelay = 0;

            NoBack = true;

            Title.BackPanel.SetState(TitleBackgroundState.CharSelect);
            CharacterSelect();
        }

		void MenuGo_Leaderboards(MenuItem item)
		{
			if (CloudberryKingdomGame.OnlineFunctionalityAvailable(MenuItem.ActivatingPlayerIndex()))
			{
#if XBOX
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

            // Options
            item = new MenuItem(new Text(Localization.Words.Options, ItemFont, true));
            item.Name = "Options";
            item.Go = MenuGo_Options;
            AddItem(item);

            if (CloudberryKingdomGame.MainMenuType == MainMenuTypes.PS3)
            {
			    // Leaderboard
                item = new MenuItem(new Text(Localization.Words.Leaderboards, ItemFont, true));
                item.Name = "Leaderboards";
			    item.Go = MenuGo_Leaderboards;
                AddItem(item);
            }

            if (CloudberryKingdomGame.MainMenuType == MainMenuTypes.PC)
            {
                // Exit
                item = new MenuItem(new Text(Localization.Words.Exit, ItemFont, true));
                item.Name = "Exit";
                item.Go = MenuGo_Exit;
                AddItem(item);
            }

            EnsureFancy();

            this.CallToLeft = true;
        }

        protected override void SetPos()
        {
if (CloudberryKingdomGame.MainMenuType == MainMenuTypes.PS3)
{
			MenuItem _item;
			_item = MyMenu.FindItemByName("Arcade"); if (_item != null) { _item.SetPos = new Vector2(0f, 365.5279f); _item.MyText.Scale = 0.66f; _item.MySelectedText.Scale = 0.66f; _item.SelectIconOffset = new Vector2(0f, 0f); }
			_item = MyMenu.FindItemByName("Campaign"); if (_item != null) { _item.SetPos = new Vector2(-8.333496f, 165.8613f); _item.MyText.Scale = 0.66f; _item.MySelectedText.Scale = 0.66f; _item.SelectIconOffset = new Vector2(0f, 0f); }
			_item = MyMenu.FindItemByName("Freeplay"); if (_item != null) { _item.SetPos = new Vector2(0f, -26.47217f); _item.MyText.Scale = 0.66f; _item.MySelectedText.Scale = 0.66f; _item.SelectIconOffset = new Vector2(0f, 0f); }
			_item = MyMenu.FindItemByName("Options"); if (_item != null) { _item.SetPos = new Vector2(0f, -405.25f); _item.MyText.Scale = 0.66f; _item.MySelectedText.Scale = 0.66f; _item.SelectIconOffset = new Vector2(0f, 0f); }
			_item = MyMenu.FindItemByName("Leaderboards"); if (_item != null) { _item.SetPos = new Vector2(0f, -216.0278f); _item.MyText.Scale = 0.66f; _item.MySelectedText.Scale = 0.66f; _item.SelectIconOffset = new Vector2(0f, 0f); }

			MyMenu.Pos = new Vector2(-80.55566f, -216.6667f);

			QuadClass _q;
			_q = MyPile.FindQuad("Back"); if (_q != null) { _q.Pos = new Vector2(-61.11133f, -336.1111f); _q.Size = new Vector2(524.4158f, 524.4158f); }

			MyPile.Pos = new Vector2(-27.77734f, -33.33337f);
}
else if (CloudberryKingdomGame.MainMenuType == MainMenuTypes.PC)
{
            MenuItem _item;
            _item = MyMenu.FindItemByName("Arcade"); if (_item != null) { _item.SetPos = new Vector2(5.555908f, 379.4168f); _item.MyText.Scale = 0.8137504f; _item.MySelectedText.Scale = 0.8137504f; _item.SelectIconOffset = new Vector2(0f, 0f); }
            _item = MyMenu.FindItemByName("Campaign"); if (_item != null) { _item.SetPos = new Vector2(5.555908f, 168.639f); _item.MyText.Scale = 0.8137504f; _item.MySelectedText.Scale = 0.8137504f; _item.SelectIconOffset = new Vector2(0f, 0f); }
            _item = MyMenu.FindItemByName("Options"); if (_item != null) { _item.SetPos = new Vector2(5.555908f, -42.13876f); _item.MyText.Scale = 0.8137504f; _item.MySelectedText.Scale = 0.8137504f; _item.SelectIconOffset = new Vector2(0f, 0f); }
            _item = MyMenu.FindItemByName("Exit"); if (_item != null) { _item.SetPos = new Vector2(5.555908f, -252.9165f); _item.MyText.Scale = 0.8137504f; _item.MySelectedText.Scale = 0.8137504f; _item.SelectIconOffset = new Vector2(0f, 0f); }

            MyMenu.Pos = new Vector2(20.88898f, -255.5556f);

            MyPile.Pos = new Vector2(-27.77734f, -33.33337f);
}
else if (CloudberryKingdomGame.MainMenuType == MainMenuTypes.WiiU)
{
	MenuItem _item;
	_item = MyMenu.FindItemByName("Arcade"); if (_item != null) { _item.SetPos = new Vector2(2.777832f, 298.8612f); _item.MyText.Scale = 0.66f; _item.MySelectedText.Scale = 0.66f; _item.SelectIconOffset = new Vector2(0f, 0f); }
	_item = MyMenu.FindItemByName("Campaign"); if (_item != null) { _item.SetPos = new Vector2(2.777832f, 104.7502f); _item.MyText.Scale = 0.66f; _item.MySelectedText.Scale = 0.66f; _item.SelectIconOffset = new Vector2(0f, 0f); }
	_item = MyMenu.FindItemByName("Freeplay"); if (_item != null) { _item.SetPos = new Vector2(2.777832f, -90.36105f); _item.MyText.Scale = 0.66f; _item.MySelectedText.Scale = 0.66f; _item.SelectIconOffset = new Vector2(0f, 0f); }
	_item = MyMenu.FindItemByName("Options"); if (_item != null) { _item.SetPos = new Vector2(2.777832f, -280.4723f); _item.MyText.Scale = 0.66f; _item.MySelectedText.Scale = 0.66f; _item.SelectIconOffset = new Vector2(0f, 0f); }

	MyMenu.Pos = new Vector2(-80.55566f, -219.4445f);

	QuadClass _q;
	_q = MyPile.FindQuad("Back"); if (_q != null) { _q.Pos = new Vector2(-50.00031f, -311.1111f); _q.Size = new Vector2(539.9984f, 460.6643f); }

	MyPile.Pos = new Vector2(-27.77734f, -33.33337f);
}

			MyMenu.SortByHeight();
        }
    }
}