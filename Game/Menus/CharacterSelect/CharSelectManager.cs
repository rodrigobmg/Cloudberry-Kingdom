﻿using System.Collections.Generic;
using System;
using System.Linq;

using Microsoft.Xna.Framework;

using CoreEngine;

using CloudberryKingdom.Bobs;

namespace CloudberryKingdom
{
    public class CharacterSelectManager : ViewReadWrite
    {
        public static Action OnBack, OnDone;

        public static GUI_Panel ParentPanel;

        public override string[] GetViewables()
        {
            return new string[] { };
        }

        static readonly CharacterSelectManager instance = new CharacterSelectManager();
        public static CharacterSelectManager Instance { get { return instance; } }

        public static List<CharacterSelect> CharSelect = new List<CharacterSelect>(new CharacterSelect[] { null, null, null, null });
        public static bool IsShowing = false;

        static FancyVector2 CamPos;
        public static Text ChooseYourHero_Text;
        static bool Show_ChooseYourHero = false;

        static int ChooseYourHero_LerpLength = 100;

        static Vector2
            HidePos = new Vector2(0, 2520),
            ShowPos = new Vector2(50.79541f, 900.1587f);

        public CharacterSelectManager()
        {
            CamPos = new FancyVector2();
        }

        public static Set<Hat> AvailableHats;
        public static void UpdateAvailableHats()
        {
            UpdateAvailableBeards();

            // Determine which hats are availabe
            AvailableHats = new Set<Hat>();
            foreach (Hat hat in ColorSchemeManager.HatInfo)
                AvailableHats += hat;
        }

        public static Set<Hat> AvailableBeards;
        static void UpdateAvailableBeards()
        {
            // Determine which Beards are availabe
            AvailableBeards = new Set<Hat>();
            foreach (Hat Beard in ColorSchemeManager.BeardInfo)
                AvailableBeards += Beard;
        }

        static CharSelectBackdrop Backdrop;
        static bool QuickJoin = false;
        static BungeeSwitch Switch;
        public static void Start(GUI_Panel Parent, bool QuickJoin)
        {
            if (!QuickJoin)
            {
                CloudberryKingdomGame.SetPresence(CloudberryKingdomGame.Presence.TitleScreen);
            }

            FakeHide = false;
            CharacterSelectManager.QuickJoin = QuickJoin;

            GameData game = null;
            if (Parent == null)
                game = Tools.CurGameData;
            else
                game = Parent.MyGame;

            ParentPanel = Parent;
            
            // Add the backdrop
            Backdrop = new CharSelectBackdrop();
            game.AddGameObject(Backdrop);

            // Start the selects for each player
            game.WaitThenDo(0, _StartAll, "StartCharSelect");

            // Add the bungee switch
            if (Switch != null)
            {
                Switch.Release();
            }
            Switch = new BungeeSwitch(-1);
            game.AddGameObject(Switch);
        }
        static void _StartAll()
        {
            for (int i = 0; i < 4; i++)
                Start(i, CharacterSelectManager.QuickJoin);
        }

        static void Start(int PlayerIndex, bool QuickJoin)
        {
            Active = true;
            IsShowing = true;
            UpdateAvailableHats();

            // If the character select is already up just return
            if (CharSelect[PlayerIndex] != null)
                return;

            CharSelect[PlayerIndex] = new CharacterSelect(PlayerIndex, QuickJoin);
        }
        
        public static void FinishAll()
        {
            for (int i = 0; i < 4; i++)
                if (CharSelect[i] != null)
                    Finish(i, QuickJoin);
        }

        public static void Finish(int PlayerIndex, bool Join)
        {
            if (Join && !CharSelect[PlayerIndex].Fake && CharSelect[PlayerIndex].MyState == CharacterSelect.SelectState.Waiting)
            {
                Tools.CurGameData.CreateBob(PlayerIndex, true);
            }

            CharSelect[PlayerIndex].Release();
            CharSelect[PlayerIndex] = null;
        }

        static void EndCharSelect(int DelayOnReturn, int DelayKillCharSelect)
        {
            Cleanup();

            if (CharacterSelectManager.ParentPanel != null)
                CharacterSelectManager.ParentPanel.Show();

            CharacterSelectManager.ParentPanel = null;
        }

        static void Cleanup()
        {
            var game = Tools.CurGameData;

            CharacterSelectManager.FinishAll();

            game.KillToDo("StartCharSelect");

            game.RemoveGameObjects(GameObject.Tag.CharSelect);
            if (Backdrop != null) Backdrop.Release();
        }

        public static void SuddenCleanup()
        {
            IsShowing = false;
            FakeHide = false;

            for (int i = 0; i < 4; i++)
            {
                if (CharSelect[i] != null)
                {
                    CharSelect[i].Release();
                    CharSelect[i] = null;
                }
            }

            var game = Tools.CurGameData;

            game.KillToDo("StartCharSelect");

            game.RemoveGameObjects(GameObject.Tag.CharSelect);
            if (Backdrop != null) Backdrop.Release();

            OnDone = null;
            CharacterSelectManager.ParentPanel = null;
        }

        public static bool AllExited()
        {
            bool All = true;
            for (int i = 0; i < 4; i++)
                if (CharSelect[i] != null && !CharSelect[i].Fake && CharSelect[i].MyState != CharacterSelect.SelectState.Beginning)
                    All = false;
            return All;
        }

        static bool AllFinished()
        {
            // False if no one has joined
            bool SomeOneIsHere = false;
            for (int i = 0; i < 4; i++)
                if (CharSelect[i] != null &&
                    !(CharSelect[i].QuickJoin && CharSelect[i].Fake) &&
                    CharSelect[i].MyState != CharacterSelect.SelectState.Beginning)
                    SomeOneIsHere = true;
            
            if (!SomeOneIsHere)
                return false;

            // Otherwise true if no one is still selecting
            bool All = true;
            for (int i = 0; i < 4; i++)
                if (CharSelect[i] != null &&
                    CharSelect[i].MyState == CharacterSelect.SelectState.Selecting)
                    All = false;
            return All;
        }

        static bool AllNull()
        {
            return CharSelect.All<CharacterSelect>(select => select == null);
        }

        public static int DrawLayer = 10;
        public static float BobZoom = 2.6f;
        public static float ZoomMod = 1.1f;
        public static void Draw()
        {
            if (FakeHide)
                return;

            if (!IsShowing)
                return;

            Camera cam = Tools.CurLevel.MainCamera;
            Vector2 HoldZoom = cam.Zoom;
            cam.Zoom = new Vector2(.001f, .001f) / ZoomMod;
            cam.SetVertexCamera();

            CamPos.AbsVal = CamPos.RelVal = cam.Data.Position;
            
            float setzoom = 0;

            // Draw the bungees
            if (CloudberryKingdomGame.BungeeSwitch)
            {
                Doll LastDrawnDoll = null;

                for (int i = 0; i < 4; i++)
                    if (CharSelect[i] != null)
                    {
                        float _zoom = BobZoom;

                        if (setzoom != _zoom)
                        {
                            Tools.QDrawer.Flush();
                            cam.SetVertexZoom(new Vector2(_zoom));
                            setzoom = BobZoom;
                        }

                        var doll = CharSelect[i].MyDoll;
                        if (doll.ShowBob)
                        {
                            // Draw a bungee from this doll to the last doll drawn.
                            if (LastDrawnDoll != null)
                            {
                                BobLink.Draw(LastDrawnDoll.MyDoll.Pos, doll.MyDoll.Pos);
                            }

                            LastDrawnDoll = doll;
                        }
                    }
            }

            // Draw the stickmen
            for (int i = 0; i < 4; i++)
                if (CharSelect[i] != null)
                {
                    float _zoom = BobZoom;

                    if (setzoom != _zoom)
                    {
                        Tools.QDrawer.Flush();
                        cam.SetVertexZoom(new Vector2(_zoom));
                        setzoom = BobZoom;
                    }

                    var doll = CharSelect[i].MyDoll;
                    doll.DrawBob();
                }
            Tools.QDrawer.Flush();
            cam.SetVertexCamera();

            cam.Zoom = HoldZoom;
            cam.SetVertexCamera();

            Switch.ManualDraw();
        }

        public static bool FakeHide = false;
        public static void AfterFinished()
        {
            IsShowing = false;
            FakeHide = false;
            
            Cleanup();
            if (OnDone != null) OnDone(); OnDone = null;
            CharacterSelectManager.ParentPanel = null;
        }

#if PC
		public static bool NonGamepadJoined = false;
#endif

        public static bool Active = false;
        public static void PhsxStep()
        {
            if (!Active) return;

            if (!IsShowing)
                return;

            if (AllNull())
                IsShowing = false;

#if PC
			// Check if a player has joined that isn't using a gamepad (via mouse or keyboard)
			NonGamepadJoined = false;
			for (int i = 0; i < 4; i++)
			{
				// If a player is past the PressToJoin screen and DOESN'T have a controller connected, assume they are using the mouse or keyboard.
				if (CharSelect[i].MyState != CharacterSelect.SelectState.Beginning && !CoreGamepad.IsConnected(i))
				{
					NonGamepadJoined = true;
					break;
				}
			}
#endif

            // Check for finishing from character selection
            if (AllFinished() && IsShowing)
            {
                Active = false;
                
                if (QuickJoin)
                    Tools.CurGameData.SlideOut_FadeIn(0, QuickJoinFinish);
                    //Tools.CurGameData.WaitThenDo(0, AfterFinished);
                else
                    Tools.CurGameData.SlideOut_FadeIn(0, AfterFinished);
            }

            // Check for ready to exit from character selection
            if (AllExited() && IsShowing)
            {
				
                if (ButtonCheck.State(ControllerButtons.B, -2).Pressed
#if PC
					|| Tools.RightMouseReleased()
					|| (Backdrop != null && Backdrop.BackClicked)
#endif
					)
                {
                    Active = false;
                    IsShowing = false;
                    EndCharSelect(0, 0);
                }
            }
        }

        static void QuickJoinFinish()
        {
            FakeHide = true;
            Tools.CurGameData.WaitThenDo(12, AfterFinished);
        }
    }
}