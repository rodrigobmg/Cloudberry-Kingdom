using System;
using Microsoft.Xna.Framework;
using CloudberryKingdom.Levels;

namespace CloudberryKingdom
{
    public class GameOverPanel : CkBaseMenu
    {
        int GameId_Score, GameId_Level;

        /// <summary>
        /// A High Scores list for the game just played.
        /// </summary>
        ScoreList MyHighScoreList, MyHighLevelList;

        GUI_TextBox MyTextBox;
        Vector2 MenuPos;

        public ScoreEntry HighScoreEntry, HighLevelEntry;
        public int Score = -1, Levels, Attempts, Time, Date;

        public int DelayPhsx = 42;

        public override void Init()
        {
            base.Init();

            Core.DrawLayer = Level.AfterPostDrawLayer;

            Fast();
        }

        public override void OnAdd()
        {
            base.OnAdd();

            // Set the aftermath data to note the failure
            Tools.CurrentAftermath = new AftermathData();
            Tools.CurrentAftermath.Success = false;

            // Absorb stats into the game's total stats
            PlayerManager.AbsorbTempStats();
            PlayerManager.AbsorbLevelStats();            

            // Update HighScore list
            PlayerManager.CalcScore(StatGroup.Game);
            if (Score == -1) Score = PlayerManager.GetGameScore();
            Attempts = PlayerManager.Score_Attempts;
            Time = PlayerManager.Score_Time;
            Date = ScoreDatabase.CurrentDate();
            ScoreDatabase.MostRecentScoreDate = Date;

            string GamerTag = PlayerManager.GetGroupGamerTag(18);
            HighScoreEntry = new ScoreEntry(GamerTag, GameId_Score, Score,  Score, Levels, Attempts, Time, Date);
            HighLevelEntry = new ScoreEntry(GamerTag, GameId_Level, Levels, Score, Levels, Attempts, Time, Date);

			if (!CloudberryKingdomGame.SimpleLeaderboards)
			{
				AddScore();
			}

            Create();

            // Absorb game stats into life time stats
            PlayerManager.AbsorbGameStats();

            // Initially hide the score screen
            this.SlideOut(PresetPos.Top, 0);

            MyGame.WaitThenDo(Awardments.AwardDelay(), () =>
                {
                    // Slow Rise
                    SlideInFrom = PresetPos.Bottom;
                    SlideOut(PresetPos.Bottom, 0);
                    SlideIn(70);

                    // Prevent menu interactions for a second
                    MyMenu.Active = false;
                    MyGame.WaitThenDo(DelayPhsx, () => MyMenu.Active = true);

#if PC
                    if (MyTextBox != null)
                        MyGame.WaitThenDo(DelayPhsx, () => MyTextBox.Active = true);
                        //MyMenu.Show = MyMenu.Active = false;
#endif
                });
        }
        
        void Create()
        {
            MyPile = new DrawPile();

            // Make the backdrop
            QuadClass backdrop = new QuadClass("Score_Screen", 1440);
            backdrop.Quad.SetColor(new Color(220, 220, 220));
            MyPile.Add(backdrop);
            backdrop.Pos = new Vector2(22.2233f, 10.55567f);

            // 'Game Over' text
            Text Text = new Text(Localization.Words.GameOver, Resources.Font_Grobold42_2, 1450, false, true, .6f);
            Text.Scale = 1f;
            Text.MyFloatColor = new Color(255, 255, 255).ToVector4();
            Text.OutlineColor = new Color(0, 0, 0).ToVector4();
            Text.Pos = new Vector2(-675.6388f, 575.4443f);
            MyPile.Add(Text, "Header");
            //Text.Shadow = true;
            Text.ShadowColor = new Color(.36f, .36f, .36f, .86f);
            Text.ShadowOffset = new Vector2(-24, -20);


            // 'Levels' text
            MyPile.Add(new Text(Localization.Words.Level, ItemFont, "Level"));
            Text = new Text(string.Format("{0}", Levels), ItemFont);
            SetHeaderProperties(Text);
            Text.Pos = new Vector2(-893.4177f, 378.9999f);
            MyPile.Add(Text, "LevelVal");

            // 'Score' text
            MyPile.Add(new Text(Localization.Words.Score, ItemFont, "Score"));
            Text = new Text(string.Format("{0}", Score), ItemFont);
            SetHeaderProperties(Text);
            Text.Pos = new Vector2(-873.9723f, 147.8889f);
            MyPile.Add(Text, "ScoreVal");

            // 'Distance' text
            //Text = new Text("Distance: " + Distance.ToString() + " feet", ItemFont);
            //SetHeaderProperties(Text);
            //Text.Pos = new Vector2(-940.6393f, 145.6666f);
            //MyPile.Add(Text);

            MakeMenu();

            EnsureFancy();
            MyMenu.Pos = new Vector2(400, -240);

#if PC
			if (CloudberryKingdomGame.SimpleLeaderboards)
			{
				MakeTextBox();
			}
#endif

            SetPos();
        }

        void SetPos()
        {
			if (Localization.CurrentLanguage.MyLanguage == Localization.Language.Italian)
			{
				MenuItem _item;
				_item = MyMenu.FindItemByName("PlayAgain"); if (_item != null) { _item.SetPos = new Vector2(-808f, 110f); _item.MyText.Scale = 0.5419669f; _item.MySelectedText.Scale = 0.5419669f; _item.SelectIconOffset = new Vector2(0f, 0f); }
				_item = MyMenu.FindItemByName("Leaderboard"); if (_item != null) { _item.SetPos = new Vector2(-808f, -48.88657f); _item.MyText.Scale = 0.5419669f; _item.MySelectedText.Scale = 0.5419669f; _item.SelectIconOffset = new Vector2(0f, 0f); }
				_item = MyMenu.FindItemByName("Done"); if (_item != null) { _item.SetPos = new Vector2(-808f, -207.7731f); _item.MyText.Scale = 0.5419669f; _item.MySelectedText.Scale = 0.5419669f; _item.SelectIconOffset = new Vector2(0f, 0f); }

				MyMenu.Pos = new Vector2(552.7777f, -259.4444f);

				Text _t;
				_t = MyPile.FindText("Header"); if (_t != null) { _t.Pos = new Vector2(-675.6388f, 575.4443f); _t.Scale = 1f; }
				_t = MyPile.FindText("Level"); if (_t != null) { _t.Pos = new Vector2(-882.3064f, 406.7777f); _t.Scale = 0.891417f; }
				_t = MyPile.FindText("LevelVal"); if (_t != null) { _t.Pos = new Vector2(189.915f, 401.2222f); _t.Scale = 0.891417f; }
				_t = MyPile.FindText("Score"); if (_t != null) { _t.Pos = new Vector2(-885.0834f, 161.7779f); _t.Scale = 0.891417f; }
				_t = MyPile.FindText("ScoreVal"); if (_t != null) { _t.Pos = new Vector2(201.0272f, 153.4444f); _t.Scale = 0.891417f; }

				QuadClass _q;
				_q = MyPile.FindQuad(""); if (_q != null) { _q.Pos = new Vector2(22.2233f, 10.55567f); _q.Size = new Vector2(1440f, 900f); }

				MyPile.Pos = new Vector2(0f, 0f);
			}
			else
			{
				MyMenu.Pos = new Vector2(400f, -240f);

				Text _t;
				_t = MyPile.FindText("Header"); if (_t != null) { _t.Pos = new Vector2(-675.6388f, 575.4443f); _t.Scale = 1f; }
				_t = MyPile.FindText("Level"); if (_t != null) { _t.Pos = new Vector2(-857.3064f, 384.5554f); _t.Scale = 0.999f; }
				_t = MyPile.FindText("LevelVal"); if (_t != null) { _t.Pos = new Vector2(-26.75146f, 376.2222f); _t.Scale = 0.999f; }
				_t = MyPile.FindText("Score"); if (_t != null) { _t.Pos = new Vector2(-871.1947f, 159f); _t.Scale = 0.999f; }
				_t = MyPile.FindText("ScoreVal"); if (_t != null) { _t.Pos = new Vector2(-12.86145f, 150.6666f); _t.Scale = 0.999f; }

				MyPile.Pos = new Vector2(0f, 0f);
			}
        }

#if PC
        protected override void ReleaseBody()
        {
            base.ReleaseBody();

            if (MyTextBox != null) MyTextBox.Release(); MyTextBox = null;
        }

        void MakeTextBox()
        {
            // Do nothing if the score doesn't qualify for the high score list
            if (!MyHighScoreList.Qualifies(HighScoreEntry) &&
                (MyHighLevelList == null || !MyHighLevelList.Qualifies(HighLevelEntry)))
                return;

            // Make the text box to allow the player to enter their name
            MyTextBox = new GUI_EnterName();
            MyTextBox.Active = false; // Keep inactive until parent GUI_Panel says it's OK to take input.
            MyTextBox.AutoDraw = false;
            MyGame.AddGameObject(MyTextBox);

            MyTextBox.Pos.SetCenter(Pos);
            MyTextBox.Pos.RelVal = new Vector2(95.23779f, -506);
            MyTextBox.Pos.code = 23;
            
            // Hide the menu
            MenuPos = MyMenu.Pos;
            MyMenu.Show = MyMenu.Active = false;

            // Show the menu when the user is done entering their name
            MyTextBox.OnEnter += () =>
                {
                    // Use the entered text as the GamerTag
                    HighScoreEntry.GamerTag = HighLevelEntry.GamerTag = MyTextBox.Text;

                    // Add the high score
                    AddScore();

                    MyGame.WaitThenDo(35, () =>
                        {
                            float width = MyGame.Cam.GetWidth();

                            MyMenu.Show = MyMenu.Active = true;
                            MyMenu.FancyPos.LerpTo(MenuPos + new Vector2(width, 0), MenuPos, 20);
                            MyTextBox.Pos.LerpTo(new Vector2(-width, 0), 20);
                        });
                };
        }
#else
        protected override void ReleaseBody()
        {
            base.ReleaseBody();
        }
#endif

        private void AddScore()
        {
            MyHighScoreList.Add(HighScoreEntry);
            MyHighLevelList.Add(HighLevelEntry);
            ScoreDatabase.Add(HighScoreEntry);
            ScoreDatabase.Add(HighLevelEntry);

#if PC
			var score = HighScoreEntry;

			int highscore = Math.Max(score.Score, PlayerManager.MaxPlayerHighScore(score.GameId));

			ScoreEntry copy = new ScoreEntry(score.GamerTag, score.GameId, highscore, highscore, score.Level, score.Attempts, score.Time, score.Date);
			copy.GameId += Challenge.LevelMask;

			// Write to Leaderboard if not in trial mode
			if (!CloudberryKingdomGame.IsDemo)
			{
				Leaderboard.WriteToLeaderboard(copy);
			}
#endif

#if XBOX
            var score = HighScoreEntry;

            ScoreEntry[] scores = { null, null, null, null };
            for (int i = 0; i < 4; i++)
            {
                if (PlayerManager.Players[i] != null && PlayerManager.Players[i].Exists)
                {
                    int highscore = Math.Max(score.Score, PlayerManager.MaxPlayerHighScore(score.GameId));
                    ScoreEntry copy = new ScoreEntry(score.GamerTag, score.GameId, highscore, highscore, score.Level, score.Attempts, score.Time, score.Date);
                    copy.GameId += Challenge.LevelMask;

                    scores[i] = copy;                    
                }
            }

			// Write to Leaderboard if not in trial mode
			if (!CloudberryKingdomGame.IsDemo)
			{
				Leaderboard.WriteToLeaderboard(scores);
			}
#endif

            ArcadeMenu.CheckForArcadeUnlocks(HighScoreEntry);

            if (!CloudberryKingdomGame.OnlineFunctionalityAvailable())
            {
                CloudberryKingdomGame.ShowError_MustBeSignedInToLiveForLeaderboard();
            }
        }

        protected override void SetHeaderProperties(Text text)
        {
            base.SetHeaderProperties(text);

            text.MyFloatColor = new Color(255, 254, 252).ToVector4();
            //text.MyFloatColor = new Color(255, 232, 77).ToVector4();
            text.Scale *= 1.48f;

            text.Shadow = false;
        }

        void MakeMenu()
        {
            MyMenu = new Menu(false);

            MyMenu.Control = -1;

            MyMenu.OnB = null;


            MenuItem item;
            FontScale *= .89f * 1.16f;

            item = new MenuItem(new Text(Localization.Words.PlayAgain, ItemFont));
			item.Name = "PlayAgain";
            item.Go = Cast.ToItem(Action_PlayAgain);
            AddItem(item);

            item = new MenuItem(new Text(Localization.Words.Leaderboard, ItemFont));
			item.Name = "Leaderboard";
            item.Go = Cast.ToItem(Action_ShowHighScores);
            AddItem(item);

            item = new MenuItem(new Text(Localization.Words.Done, ItemFont));
			item.Name = "Done";
            item.Go = Cast.ToItem(Action_Done);
            AddItem(item);
        }

        protected override void SetItemProperties(MenuItem item)
        {
            base.SetItemProperties(item);

            item.MyText.MyFloatColor = new Color(255, 255, 255).ToVector4();
            item.MySelectedText.MyFloatColor = new Color(50, 220, 50).ToVector4();
        }

        void Action_Done()
        {
            SlideOut(PresetPos.Top, 13);
            Active = false;

            Tools.SongWad.FadeOut();
            MyGame.WaitThenDo(36, () => MyGame.EndGame(false));

            return;
        }

        void Action_PlayAgain()
        {
            SlideOut(PresetPos.Top, 13);
            Active = false;

            Tools.SongWad.FadeOut();

            MyGame.WaitThenDo(36, () => MyGame.EndGame(true));
            return;
        }

        void Action_ShowHighScores()
        {
			if (CloudberryKingdomGame.SimpleLeaderboards)
			{
				Hide(PresetPos.Bottom);
                Call(new HighScorePanel(MyHighScoreList, MyHighLevelList));
			}
			else
			{
                if (CloudberryKingdomGame.OnlineFunctionalityAvailable(MenuItem.ActivatingPlayerIndex()))
                {
#if XBOX
                    var gamer = CloudberryKingdomGame.IndexToSignedInGamer(MenuItem.ActivatingPlayerIndex());
                    if (gamer != null)
                    {
						Hide(PresetPos.Bottom);
                        Call(new LeaderboardGUI(null, gamer, MenuItem.ActivatingPlayer), 0);
                        Hide();
                    }
#else
					Hide(PresetPos.Bottom);
					Call(new LeaderboardGUI(null, MenuItem.ActivatingPlayer), 0);
					Hide();
#endif
				}
                else
                {
                    CloudberryKingdomGame.ShowError_MustBeSignedInToLive(Localization.Words.Err_MustBeSignedInToLive);
                }
			}
        }

        public GameOverPanel() { }
        public GameOverPanel(int GameId_Score, int GameId_Level)
        {
            this.GameId_Score = GameId_Score;
            this.GameId_Level = GameId_Level;

            MyHighScoreList = ScoreDatabase.GetList(GameId_Score);
            MyHighScoreList.MyFormat = ScoreEntry.Format.Score;
            
            MyHighLevelList = ScoreDatabase.GetList(GameId_Level);
            MyHighLevelList.MyFormat = ScoreEntry.Format.Level;
        }

        protected override void MyDraw()
        {
 	         base.MyDraw();

#if PC
            if (MyTextBox != null)
                MyTextBox.ManualDraw();
#endif
        }
    }
}