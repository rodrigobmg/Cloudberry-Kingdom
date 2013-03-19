using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace CloudberryKingdom
{
    public class SoundMenu : VerifyBaseMenu
    {
        public bool LanguageOption;
        public SoundMenu(int Control, bool LanguageOption)
            : base(false)
        {
			Fade = false;

            EnableBounce();

            this.LanguageOption = LanguageOption;
            this.Control = Control;
            FixedToCamera = true;

            Constructor();
        }

        Localization.Language ChosenLanguage;

        EzText HeaderText;
        public override void Init()
        {
            ChosenLanguage = Localization.CurrentLanguage.MyLanguage;

            base.Init();
            
            if (UseBounce)
                CallToLeft = false;
            else
                CallToLeft = true;

            this.FontScale *= .9f;

            bool CenterItems = false;

            // Header
            HeaderText = new EzText(Localization.Words.Options, ItemFont);
            HeaderText.Name = "Header";
            SetHeaderProperties(HeaderText);
            MyPile.Add(HeaderText);

            MenuSlider FxSlider = new MenuSlider(new EzText(Localization.Words.SoundVolume, ItemFont, CenterItems));
            FxSlider.MyFloat = Tools.SoundVolume;
            FxSlider.Name = "Sound";
            AddItem(FxSlider);

            MenuSlider MusicSlider = new MenuSlider(new EzText(Localization.Words.MusicVolume, ItemFont, CenterItems));
            MusicSlider.MyFloat = Tools.MusicVolume;
            MusicSlider.Name = "Music";
            AddItem(MusicSlider);

            MenuItem item = new MenuItem(new EzText(Localization.Words.Controls, ItemFont, CenterItems));
            item.Go = Cast.ToItem(Go_Controls);
            item.Name = "Controls";
            AddItem(item);

            if (LanguageOption)
            {
                // Language
                var LanguageText = new EzText(Localization.Words.Language, ItemFont, CenterItems);
                SetHeaderProperties(LanguageText);
                LanguageText.Name = "Language";
                MyPile.Add(LanguageText);

                MenuList LanguageList = new MenuList();
                LanguageList.Go = ItemReturnToCaller;
                LanguageList.Name = "LanguageList";
                LanguageList.Center = false;
                LanguageList.MyExpandPos = new Vector2(-498.1506f, 713.873f);

                // Add languages to the language list
                for (int j = 0; j < Localization.NumLanguages; j++)
                {
                    Localization.Language l = (Localization.Language)j;

                    string str = Localization.LanguageName(l);
                    item = new MenuItem(new EzText(str, ItemFont, false, true));
                    SetItemProperties(item);
                    LanguageList.AddItem(item, l);
                }
                AddItem(LanguageList);
				LanguageList.SelectSound = null; // Don't play a sound (since pushing (A) will cause the back action anyway).
                LanguageList.SetIndex((int)Localization.CurrentLanguage.MyLanguage);
                LanguageList.OnConfirmedIndexSelect = () =>
                {
                    ChosenLanguage = (Localization.Language)LanguageList.ListIndex;
                    //PlayerManager.SavePlayerData.ResolutionPreferenceSet = true;
                    //ResolutionGroup.Use(LanguageList.CurObj as DisplayMode);
                    //SaveGroup.SaveAll();
                    //PlayerManager.SaveRezAndKeys();
                };
            }

#if PC_VERSION
            // Custom controls
            var mitem = new MenuItem(new EzText(Localization.Words.EditControls, ItemFont, CenterItems));
            mitem.Go = Cast.ToItem(Go_CustomControls);
            mitem.Name = "Custom";
            AddItem(mitem);

            // Full screen resolutions
            var RezText = new EzText(Localization.Words.Resolution, ItemFont, CenterItems);
            SetHeaderProperties(RezText);
            RezText.Name = "RezText";
            MyPile.Add(RezText);

            MenuList FsRezList = new MenuList();
            FsRezList.Name = "RezList";
            FsRezList.Center = false;
            FsRezList.MyExpandPos = new Vector2(-498.1506f, 713.873f);
            int i = 0;
            int CurRez = 0;

            // Get viable resolutions
            List<DisplayMode> modes = new List<DisplayMode>();
            foreach (DisplayMode mode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
            {
                if (modes.Any(existing => existing.Width == mode.Width && existing.Height == mode.Height))
                    continue;
                else
                    modes.Add(mode);
            }

            // Add resolutions to the current list
            bool found = false;
            foreach (var mode in modes)
            {
                string str = mode.Width + " x " + mode.Height;
                Tools.Write(str);
                item = new MenuItem(new EzText(str, ItemFont, false, true));
                SetItemProperties(item);
                FsRezList.AddItem(item, mode);

                if (mode.Width == Tools.TheGame.MyGraphicsDeviceManager.PreferredBackBufferWidth &&
                    mode.Height == Tools.TheGame.MyGraphicsDeviceManager.PreferredBackBufferHeight)
                {
                    CurRez = i;
                    found = true;
                }
                else if (!found && mode.Width == Tools.TheGame.MyGraphicsDeviceManager.PreferredBackBufferWidth)
                    CurRez = i;

                i++;
            }
            AddItem(FsRezList);
            FsRezList.SetIndex(CurRez);
            FsRezList.OnConfirmedIndexSelect = () =>
            {
                PlayerManager.SavePlayerData.ResolutionPreferenceSet = true;
                ResolutionGroup.Use(FsRezList.CurObj as DisplayMode);
                SaveGroup.SaveAll();
                PlayerManager.SaveRezAndKeys();
            };

            // Full screen toggle
            var FullScreenText = new EzText(Localization.Words.FullScreen, ItemFont, CenterItems);
            SetHeaderProperties(FullScreenText);
            FullScreenText.Name = "Fullscreen";
            MyPile.Add(FullScreenText);

            var toggle = new MenuToggle(ItemFont);
            toggle.OnToggle = (state) =>
            {
                PlayerManager.SavePlayerData.ResolutionPreferenceSet = true;
                Tools.Fullscreen = state;
                SaveGroup.SaveAll();
                PlayerManager.SaveRezAndKeys();
            };
            toggle.Name = "FullscreenToggle";
            toggle.Toggle(Tools.Fullscreen);

            AddItem(toggle);

            //AddToggle_FixedTimestep();
            //AddToggle_Borderless();
#endif

			// Credits
#if XBOX
			if (true)
#else
			if (LanguageOption && !CloudberryKingdomGame.HideLogos)
#endif
			{
				item = new MenuItem(new EzText(Localization.Words.Credits, ItemFont, CenterItems));
				item.Go = Cast.ToItem(Go_Credits);
				item.Name = "Credits";
				AddItem(item);

				// Black
				Black = new QuadClass();
				Black.FullScreen(Tools.CurCamera);
				Black.Quad.SetColor(Color.Black);
				Black.Alpha = 0;
			}



#if PC_VERSION
            MakeBackButton();
#else
            MakeBackButton();
            //MakeStaticBackButton();
#endif

#if PC_VERSION
			SetPosition_PC();
#else
			SetPosition_Console();
#endif
                

            MyMenu.SortByHeight();

            MyMenu.OnX = MyMenu.OnB = MenuReturnToCaller;

            // Select the first item in the menu to start
            MyMenu.SelectItem(0);
        }

#if PC_VERSION
        void Go_CustomControls()
        {
            if (UseBounce)
            {
                PauseGame = false;
                Hid = true;
                RegularSlideOut(PresetPos.Right, 0);
            }
            else
            {
                Hide();
            }

            Call(new CustomControlsMenu(), 0);
        }
#endif

        void Go_Controls()
        {
            if (UseBounce)
            {
                PauseGame = false;
                Hid = true;
                RegularSlideOut(PresetPos.Right, 0);
            }
            else
            {
                Hide();
            }

            Call(new ControlScreen(Control), 0);
        }

		void Go_Credits()
		{
			Tools.SongWad.FadeOut();
			MyMenu.Active = false;

			MyGame.WaitThenDo(4, StartFade);
			MyGame.WaitThenDo(87, StartCredits);
		}

		bool Fade;
		QuadClass Black;

		void StartFade()
		{
			Fade = true;
			Black.Alpha = 0;
		}

		void StartCredits()
		{
			MyGame.WaitThenDo(20, AfterCredits);
			MainVideo.StartVideo("Credits", false, 1.0f);
		}

		void AfterCredits()
		{
			Fade = false;
			Black.Alpha = 3.45f;
			MyMenu.Active = true;
			Tools.SongWad.Stop();
			//Tools.PlayHappyMusic();
			//Tools.SongWad.Start(true);
			Tools.SongWad.Restart(true, false);
		}

        public override void Release()
        {
			if (Core.Released) return;

			if (Black != null) Black.Release();

            if (ChosenLanguage != Localization.CurrentLanguage.MyLanguage)
            {
                Localization.SetLanguage(ChosenLanguage);
                MyGame.ReInitGameObjects();

                foreach (GameObject obj in MyGame.MyGameObjects)
                {
                    GUI_Panel panel = obj as GUI_Panel;
                    if (null != panel && (panel is StartMenu_MW_Pre || panel is StartMenu_MW_PressStart || panel is StartMenu_MW_Simple))
                        panel.SlideOut(PresetPos.Left, 0);
                }

                MyGame.PhsxStepsToDo += 20;

                ButtonCheck.PreventInput();
                ButtonCheck.PreventTimeStamp += 20;
            }

            SaveGroup.SaveAll();

            base.Release();
        }

        public override void ReturnToCaller()
        {
            //SaveGroup.SaveAll();
            base.ReturnToCaller();
        }

#if PC_VERSION
        //private void AddToggle_FixedTimestep()
        //{
        //    // Header
        //    var Text = new EzText(Localization.Words.FixedTimeStep, ItemFont);
        //    SetHeaderProperties(Text);
        //    MyPile.Add(Text);
        //    Text.Pos = new Vector2(-1232.142f, -499.9359f);
        //    Text.Scale *= .9f;

        //    // Menu item
        //    var Toggle = new MenuToggle(ItemFont);
        //    Toggle.OnToggle = Toggle_FixedTimestep;

        //    Toggle.Toggle(Tools.FixedTimeStep);
        //    Toggle.PrefixText = "";
        //    AddItem(Toggle);
        //    Toggle.SetPos = new Vector2(1315.078f, -451.4125f);
        //}

        //private void Toggle_FixedTimestep(bool state)
        //{
        //    PlayerManager.SavePlayerData.ResolutionPreferenceSet = true;
        //    Tools.FixedTimeStep = state;
        //    SaveGroup.SaveAll();
        //    PlayerManager.SaveRezAndKeys();
        //}

        private void AddToggle_Borderless()
        {
            // Text
            var Text = new EzText(Localization.Words.WindowBorder, ItemFont);
            SetHeaderProperties(Text);
            Text.Name = "WindowBorder";
            MyPile.Add(Text);
            Text.Pos = new Vector2(-1232.142f, -499.9359f);
            Text.Scale *= .9f;

            // Toggle
            var Toggle = new MenuToggle(ItemFont);
            Toggle.OnToggle = Toggle_Borderless;
            Toggle.Toggle(Tools.WindowBorder);
            Toggle.Name = "WindowBorderToggle";
            AddItem(Toggle);
            Toggle.SetPos = new Vector2(1315.078f, -451.4125f);
        }

        private void Toggle_Borderless(bool state)
        {
            PlayerManager.SavePlayerData.ResolutionPreferenceSet = true;
            Tools.WindowBorder = state;
            SaveGroup.SaveAll();
            PlayerManager.SaveRezAndKeys();

            Tools.GameClass.SetBorder(Tools.WindowBorder);
        }
#endif

        public override bool MenuReturnToCaller(Menu menu)
        {
#if PC_VERSION
            PlayerManager.SaveRezAndKeys();
#endif

            return base.MenuReturnToCaller(menu);
        }

        private void SetPosition_Console()
        {
            if (LanguageOption)
            {
				MenuItem _item;
				_item = MyMenu.FindItemByName("Sound"); if (_item != null) { _item.SetPos = new Vector2(-27.38177f, 745.9205f); _item.MyText.Scale = 0.6704169f; _item.MySelectedText.Scale = 0.6704169f; _item.SelectIconOffset = new Vector2(0f, 0f); ((MenuSlider)_item).SliderShift = new Vector2(1686.11f, -152.7778f); }
				_item = MyMenu.FindItemByName("Music"); if (_item != null) { _item.SetPos = new Vector2(-27.38177f, 531.5082f); _item.MyText.Scale = 0.6704169f; _item.MySelectedText.Scale = 0.6704169f; _item.SelectIconOffset = new Vector2(0f, 0f); ((MenuSlider)_item).SliderShift = new Vector2(1686.11f, -150.0001f); }
				_item = MyMenu.FindItemByName("LanguageList"); if (_item != null) { _item.SetPos = new Vector2(1325f, 136f); _item.MyText.Scale = 0.6704169f; _item.MySelectedText.Scale = 0.6704169f; _item.SelectIconOffset = new Vector2(0f, 0f); }
				_item = MyMenu.FindItemByName("Controls"); if (_item != null) { _item.SetPos = new Vector2(-27.38177f, 42.49181f); _item.MyText.Scale = 0.6704169f; _item.MySelectedText.Scale = 0.6704169f; _item.SelectIconOffset = new Vector2(0f, 0f); }
				_item = MyMenu.FindItemByName("Back"); if (_item != null) { _item.SetPos = new Vector2(1347.618f, -329.4443f); _item.MyText.Scale = 0.6704169f; _item.MySelectedText.Scale = 0.6704169f; _item.SelectIconOffset = new Vector2(0f, 0f); }
				_item = MyMenu.FindItemByName("Credits"); if (_item != null) { _item.SetPos = new Vector2(-27.38177f, -144.5556f); _item.MyText.Scale = 0.6704169f; _item.MySelectedText.Scale = 0.6704169f; _item.SelectIconOffset = new Vector2(0f, 0f); }

				MyMenu.Pos = new Vector2(-991.2675f, -246.4287f);

				EzText _t;
				_t = MyPile.FindEzText("Header"); if (_t != null) { _t.Pos = new Vector2(-1072.619f, 812.7616f); _t.Scale = 0.864f; }
				_t = MyPile.FindEzText("Language"); if (_t != null) { _t.Pos = new Vector2(-1033.333f, 13.88895f); _t.Scale = 0.6704169f; }

				QuadClass _q;
				_q = MyPile.FindQuad("Backdrop"); if (_q != null) { _q.Pos = new Vector2(-18.6521f, -7.539473f); _q.Size = new Vector2(1223.651f, 922.9517f); }

				MyPile.Pos = new Vector2(29.76172f, 21.82541f);
			}
            else
            {
                MenuItem _item;
                _item = MyMenu.FindItemByName("Sound"); if (_item != null) { _item.SetPos = new Vector2(-27.38177f, 745.9205f); _item.MyText.Scale = 0.6704169f; _item.MySelectedText.Scale = 0.6704169f; _item.SelectIconOffset = new Vector2(0f, 0f); ((MenuSlider)_item).SliderShift = new Vector2(1686.11f, -152.7778f); }
                _item = MyMenu.FindItemByName("Music"); if (_item != null) { _item.SetPos = new Vector2(-27.38177f, 531.5082f); _item.MyText.Scale = 0.6704169f; _item.MySelectedText.Scale = 0.6704169f; _item.SelectIconOffset = new Vector2(0f, 0f); ((MenuSlider)_item).SliderShift = new Vector2(1686.11f, -150.0001f); }
                _item = MyMenu.FindItemByName("Controls"); if (_item != null) { _item.SetPos = new Vector2(-27.38177f, 306.3806f); _item.MyText.Scale = 0.6704169f; _item.MySelectedText.Scale = 0.6704169f; _item.SelectIconOffset = new Vector2(0f, 0f); }
				_item = MyMenu.FindItemByName("Credits"); if (_item != null) { _item.SetPos = new Vector2(-27.38177f, 85.99994f); _item.MyText.Scale = 0.6704169f; _item.MySelectedText.Scale = 0.6704169f; _item.SelectIconOffset = new Vector2(0f, 0f); }
				_item = MyMenu.FindItemByName("Back"); if (_item != null) { _item.SetPos = new Vector2(1344.84f, -129.4444f); _item.MyText.Scale = 0.6704169f; _item.MySelectedText.Scale = 0.6704169f; _item.SelectIconOffset = new Vector2(0f, 0f); }

                MyMenu.Pos = new Vector2(-980.1562f, -338.0954f);

                EzText _t;
                _t = MyPile.FindEzText("Header"); if (_t != null) { _t.Pos = new Vector2(-1072.619f, 812.7616f); _t.Scale = 0.864f; }

                QuadClass _q;
                _q = MyPile.FindQuad("Backdrop"); if (_q != null) { _q.Pos = new Vector2(-18.6521f, -7.539473f); _q.Size = new Vector2(1223.651f, 922.9517f); }

                MyPile.Pos = new Vector2(29.76172f, 21.82541f);
            }
        }

        private void SetPosition_PC()
        {
            if (LanguageOption)
            {
if (Localization.CurrentLanguage.MyLanguage == Localization.Language.Italian)
{
				MenuItem _item;
				_item = MyMenu.FindItemByName("Sound"); if (_item != null) { _item.SetPos = new Vector2(-169.048f, 729.2538f); _item.MyText.Scale = 0.6606668f; _item.MySelectedText.Scale = 0.6606668f; _item.SelectIconOffset = new Vector2(0f, 0f); ((MenuSlider)_item).SliderShift = new Vector2(1869.443f, -152.7778f); }
				_item = MyMenu.FindItemByName("Music"); if (_item != null) { _item.SetPos = new Vector2(-169.048f, 537.0638f); _item.MyText.Scale = 0.6916667f; _item.MySelectedText.Scale = 0.6606668f; _item.SelectIconOffset = new Vector2(0f, 0f); ((MenuSlider)_item).SliderShift = new Vector2(1869.443f, -136.1112f); }
				_item = MyMenu.FindItemByName("RezList"); if (_item != null) { _item.SetPos = new Vector2(1077.38f, 151.8088f); _item.MyText.Scale = 0.6119168f; _item.MySelectedText.Scale = 0.6119168f; _item.SelectIconOffset = new Vector2(0f, 0f);  }
				_item = MyMenu.FindItemByName("FullscreenToggle"); if (_item != null) { _item.SetPos = new Vector2(1090.078f, 101.3653f); _item.MyText.Scale = 0.6095002f; _item.MySelectedText.Scale = 0.6095002f; _item.SelectIconOffset = new Vector2(0f, 0f);  }
				_item = MyMenu.FindItemByName("LanguageList"); if (_item != null) { _item.SetPos = new Vector2(1088.888f, -200.1113f); _item.MyText.Scale = 0.6166669f; _item.MySelectedText.Scale = 0.6166669f; _item.SelectIconOffset = new Vector2(0f, 0f);  }
				_item = MyMenu.FindItemByName("Controls"); if (_item != null) { _item.SetPos = new Vector2(538.491f, -254.7306f); _item.MyText.Scale = 0.6705834f; _item.MySelectedText.Scale = 0.6705834f; _item.SelectIconOffset = new Vector2(0f, 0f);  }
				_item = MyMenu.FindItemByName("Custom"); if (_item != null) { _item.SetPos = new Vector2(402.7765f, -441.3655f); _item.MyText.Scale = 0.6581671f; _item.MySelectedText.Scale = 0.6581671f; _item.SelectIconOffset = new Vector2(0f, 0f);  }
				_item = MyMenu.FindItemByName("Credits"); if (_item != null) { _item.SetPos = new Vector2(580.5563f, -616.7772f); _item.MyText.Scale = 0.6445836f; _item.MySelectedText.Scale = 0.6445836f; _item.SelectIconOffset = new Vector2(0f, 0f);  }
				_item = MyMenu.FindItemByName("Back"); if (_item != null) { _item.SetPos = new Vector2(1603.173f, -621.111f); _item.MyText.Scale = 0.72f; _item.MySelectedText.Scale = 0.72f; _item.SelectIconOffset = new Vector2(0f, 0f);  }

				MyMenu.Pos = new Vector2(-1007.934f, -43.651f);

				EzText _t;
				_t = MyPile.FindEzText("Header"); if (_t != null) { _t.Pos = new Vector2(-1072.62f, 968.3172f); _t.Scale = 0.864f; }
				_t = MyPile.FindEzText("Language"); if (_t != null) { _t.Pos = new Vector2(-1161.11f, -133.3337f); _t.Scale = 0.6308331f; }
				_t = MyPile.FindEzText("RezText"); if (_t != null) { _t.Pos = new Vector2(-1157.143f, 213.9516f); _t.Scale = 0.6066011f; }
				_t = MyPile.FindEzText("Fullscreen"); if (_t != null) { _t.Pos = new Vector2(-1151.586f, 33.39727f); _t.Scale = 0.5670165f; }

				QuadClass _q;
				_q = MyPile.FindQuad("Backdrop"); if (_q != null) { _q.Pos = new Vector2(-18.6521f, -10.31725f); _q.Size = new Vector2(1376.984f, 1077.035f); }

				MyPile.Pos = new Vector2(29.76172f, 21.82541f);
}
else if (Localization.CurrentLanguage.MyLanguage == Localization.Language.Spanish)
{
				MenuItem _item;
				_item = MyMenu.FindItemByName("Sound"); if (_item != null) { _item.SetPos = new Vector2(-169.048f, 729.2538f); _item.MyText.Scale = 0.6606668f; _item.MySelectedText.Scale = 0.6606668f; _item.SelectIconOffset = new Vector2(0f, 0f); ((MenuSlider)_item).SliderShift = new Vector2(1869.443f, -152.7778f); }
				_item = MyMenu.FindItemByName("Music"); if (_item != null) { _item.SetPos = new Vector2(-169.048f, 537.0638f); _item.MyText.Scale = 0.6916667f; _item.MySelectedText.Scale = 0.6606668f; _item.SelectIconOffset = new Vector2(0f, 0f); ((MenuSlider)_item).SliderShift = new Vector2(1869.443f, -136.1112f); }
				_item = MyMenu.FindItemByName("RezList"); if (_item != null) { _item.SetPos = new Vector2(1077.38f, 151.8088f); _item.MyText.Scale = 0.6119168f; _item.MySelectedText.Scale = 0.6119168f; _item.SelectIconOffset = new Vector2(0f, 0f);  }
				_item = MyMenu.FindItemByName("FullscreenToggle"); if (_item != null) { _item.SetPos = new Vector2(1090.078f, 101.3653f); _item.MyText.Scale = 0.6095002f; _item.MySelectedText.Scale = 0.6095002f; _item.SelectIconOffset = new Vector2(0f, 0f);  }
				_item = MyMenu.FindItemByName("LanguageList"); if (_item != null) { _item.SetPos = new Vector2(1088.888f, -200.1113f); _item.MyText.Scale = 0.6166669f; _item.MySelectedText.Scale = 0.6166669f; _item.SelectIconOffset = new Vector2(0f, 0f);  }
				_item = MyMenu.FindItemByName("Controls"); if (_item != null) { _item.SetPos = new Vector2(538.491f, -254.7306f); _item.MyText.Scale = 0.6705834f; _item.MySelectedText.Scale = 0.6705834f; _item.SelectIconOffset = new Vector2(0f, 0f);  }
				_item = MyMenu.FindItemByName("Custom"); if (_item != null) { _item.SetPos = new Vector2(402.7765f, -441.3655f); _item.MyText.Scale = 0.6581671f; _item.MySelectedText.Scale = 0.6581671f; _item.SelectIconOffset = new Vector2(0f, 0f);  }
				_item = MyMenu.FindItemByName("Credits"); if (_item != null) { _item.SetPos = new Vector2(580.5563f, -616.7772f); _item.MyText.Scale = 0.6445836f; _item.MySelectedText.Scale = 0.6445836f; _item.SelectIconOffset = new Vector2(0f, 0f);  }
				_item = MyMenu.FindItemByName("Back"); if (_item != null) { _item.SetPos = new Vector2(1603.173f, -621.111f); _item.MyText.Scale = 0.72f; _item.MySelectedText.Scale = 0.72f; _item.SelectIconOffset = new Vector2(0f, 0f);  }

				MyMenu.Pos = new Vector2(-1007.934f, -43.651f);

				EzText _t;
				_t = MyPile.FindEzText("Header"); if (_t != null) { _t.Pos = new Vector2(-1072.62f, 968.3172f); _t.Scale = 0.864f; }
				_t = MyPile.FindEzText("Language"); if (_t != null) { _t.Pos = new Vector2(-1161.11f, -133.3337f); _t.Scale = 0.6308331f; }
				_t = MyPile.FindEzText("RezText"); if (_t != null) { _t.Pos = new Vector2(-1157.143f, 213.9516f); _t.Scale = 0.6066011f; }
				_t = MyPile.FindEzText("Fullscreen"); if (_t != null) { _t.Pos = new Vector2(-1151.586f, 33.39727f); _t.Scale = 0.5670165f; }

				QuadClass _q;
				_q = MyPile.FindQuad("Backdrop"); if (_q != null) { _q.Pos = new Vector2(-18.6521f, -10.31725f); _q.Size = new Vector2(1376.984f, 1077.035f); }

				MyPile.Pos = new Vector2(29.76172f, 21.82541f);
}
else
{
				MenuItem _item;
				_item = MyMenu.FindItemByName("Sound"); if (_item != null) { _item.SetPos = new Vector2(-169.048f, 729.2538f); _item.MyText.Scale = 0.6606668f; _item.MySelectedText.Scale = 0.6606668f; _item.SelectIconOffset = new Vector2(0f, 0f); ((MenuSlider)_item).SliderShift = new Vector2(1869.443f, -152.7778f); }
				_item = MyMenu.FindItemByName("Music"); if (_item != null) { _item.SetPos = new Vector2(-169.048f, 537.0638f); _item.MyText.Scale = 0.6916667f; _item.MySelectedText.Scale = 0.6606668f; _item.SelectIconOffset = new Vector2(0f, 0f); ((MenuSlider)_item).SliderShift = new Vector2(1869.443f, -136.1112f); }
				_item = MyMenu.FindItemByName("RezList"); if (_item != null) { _item.SetPos = new Vector2(1077.38f, 151.8088f); _item.MyText.Scale = 0.6119168f; _item.MySelectedText.Scale = 0.6119168f; _item.SelectIconOffset = new Vector2(0f, 0f); }
				_item = MyMenu.FindItemByName("FullscreenToggle"); if (_item != null) { _item.SetPos = new Vector2(1090.078f, 101.3653f); _item.MyText.Scale = 0.6095002f; _item.MySelectedText.Scale = 0.6095002f; _item.SelectIconOffset = new Vector2(0f, 0f); }
				_item = MyMenu.FindItemByName("LanguageList"); if (_item != null) { _item.SetPos = new Vector2(1088.888f, -200.1113f); _item.MyText.Scale = 0.6166669f; _item.MySelectedText.Scale = 0.6166669f; _item.SelectIconOffset = new Vector2(0f, 0f); }
				_item = MyMenu.FindItemByName("Controls"); if (_item != null) { _item.SetPos = new Vector2(538.491f, -254.7306f); _item.MyText.Scale = 0.6705834f; _item.MySelectedText.Scale = 0.6705834f; _item.SelectIconOffset = new Vector2(0f, 0f); }
				_item = MyMenu.FindItemByName("Custom"); if (_item != null) { _item.SetPos = new Vector2(402.7765f, -441.3655f); _item.MyText.Scale = 0.6581671f; _item.MySelectedText.Scale = 0.6581671f; _item.SelectIconOffset = new Vector2(0f, 0f); }
				_item = MyMenu.FindItemByName("Credits"); if (_item != null) { _item.SetPos = new Vector2(580.5563f, -616.7772f); _item.MyText.Scale = 0.6445836f; _item.MySelectedText.Scale = 0.6445836f; _item.SelectIconOffset = new Vector2(0f, 0f); }
				_item = MyMenu.FindItemByName("Back"); if (_item != null) { _item.SetPos = new Vector2(1603.173f, -621.111f); _item.MyText.Scale = 0.72f; _item.MySelectedText.Scale = 0.72f; _item.SelectIconOffset = new Vector2(0f, 0f); }

				MyMenu.Pos = new Vector2(-1007.934f, -43.651f);

				EzText _t;
				_t = MyPile.FindEzText("Header"); if (_t != null) { _t.Pos = new Vector2(-1072.62f, 968.3172f); _t.Scale = 0.864f; }
				_t = MyPile.FindEzText("Language"); if (_t != null) { _t.Pos = new Vector2(-1036.11f, -119.4448f); _t.Scale = 0.6748332f; }
				_t = MyPile.FindEzText("RezText"); if (_t != null) { _t.Pos = new Vector2(-1040.476f, 238.9516f); _t.Scale = 0.6868508f; }
				_t = MyPile.FindEzText("Fullscreen"); if (_t != null) { _t.Pos = new Vector2(-1040.475f, 63.95281f); _t.Scale = 0.7051834f; }

				QuadClass _q;
				_q = MyPile.FindQuad("Backdrop"); if (_q != null) { _q.Pos = new Vector2(-18.6521f, -10.31725f); _q.Size = new Vector2(1376.984f, 1077.035f); }

				MyPile.Pos = new Vector2(29.76172f, 21.82541f);
}
			}
            else
            {
                MenuItem _item;
                _item = MyMenu.FindItemByName("Sound"); if (_item != null) { _item.SetPos = new Vector2(3.173767f, 751.4761f); _item.MyText.Scale = 0.72f; _item.MySelectedText.Scale = 0.72f; _item.SelectIconOffset = new Vector2(0f, 0f); ((MenuSlider)_item).SliderShift = new Vector2(1611.11f, -152.7778f); }
                _item = MyMenu.FindItemByName("Music"); if (_item != null) { _item.SetPos = new Vector2(64.28528f, 534.286f); _item.MyText.Scale = 0.72f; _item.MySelectedText.Scale = 0.72f; _item.SelectIconOffset = new Vector2(0f, 0f); ((MenuSlider)_item).SliderShift = new Vector2(1552.777f, -150.0001f); }
                _item = MyMenu.FindItemByName("Controls"); if (_item != null) { _item.SetPos = new Vector2(596.8245f, 325.825f); _item.MyText.Scale = 0.72f; _item.MySelectedText.Scale = 0.72f; _item.SelectIconOffset = new Vector2(0f, 0f); }
                _item = MyMenu.FindItemByName("Custom"); if (_item != null) { _item.SetPos = new Vector2(591.6658f, 133.6347f); _item.MyText.Scale = 0.72f; _item.MySelectedText.Scale = 0.72f; _item.SelectIconOffset = new Vector2(0f, 0f); }
                _item = MyMenu.FindItemByName("RezList"); if (_item != null) { _item.SetPos = new Vector2(1019.047f, -256.5245f); _item.MyText.Scale = 0.72f; _item.MySelectedText.Scale = 0.72f; _item.SelectIconOffset = new Vector2(0f, 0f); }
                _item = MyMenu.FindItemByName("FullscreenToggle"); if (_item != null) { _item.SetPos = new Vector2(1245.634f, -281.9681f); _item.MyText.Scale = 0.72f; _item.MySelectedText.Scale = 0.72f; _item.SelectIconOffset = new Vector2(0f, 0f); }
                _item = MyMenu.FindItemByName("WindowBorderToggle"); if (_item != null) { _item.SetPos = new Vector2(1315.078f, -451.4125f); _item.MyText.Scale = 0.72f; _item.MySelectedText.Scale = 0.72f; _item.SelectIconOffset = new Vector2(0f, 0f); }
                _item = MyMenu.FindItemByName("Back"); if (_item != null) { _item.SetPos = new Vector2(1603.173f, -621.111f); _item.MyText.Scale = 0.72f; _item.MySelectedText.Scale = 0.72f; _item.SelectIconOffset = new Vector2(0f, 0f); }

                MyMenu.Pos = new Vector2(-1007.934f, -43.651f);

                EzText _t;
                _t = MyPile.FindEzText("Header"); if (_t != null) { _t.Pos = new Vector2(-967.064f, 951.6506f); _t.Scale = 0.864f; }
                _t = MyPile.FindEzText("RezText"); if (_t != null) { _t.Pos = new Vector2(-1173.81f, -174.9373f); _t.Scale = 0.7776f; }
                _t = MyPile.FindEzText("Fullscreen"); if (_t != null) { _t.Pos = new Vector2(-1190.475f, -338.825f); _t.Scale = 0.7776f; }
                _t = MyPile.FindEzText("WindowBorder"); if (_t != null) { _t.Pos = new Vector2(-1232.142f, -499.9359f); _t.Scale = 0.7776f; }

                QuadClass _q;
                _q = MyPile.FindQuad("Backdrop"); if (_q != null) { _q.Pos = new Vector2(-18.6521f, -10.31725f); _q.Size = new Vector2(1376.984f, 1077.035f); }

                MyPile.Pos = new Vector2(29.76172f, 21.82541f);
            }
        }

        public override void OnAdd()
        {
 	        base.OnAdd();
        }

		protected override void MyDraw()
		{
			base.MyDraw();

			if (Black != null && Black.Alpha >= 0)
			{
				Black.Draw();
				if (Fade)
					Black.Alpha += .02f;
				else
					Black.Alpha -= .02f;
			}
		}
    }
}