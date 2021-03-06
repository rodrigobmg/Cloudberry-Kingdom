﻿
using System;

namespace CloudberryKingdom
{
    public class StringWorldTimed : StringWorldGameData
    {
        GUI_Timer MyGUI_Timer;
        TimerWarning Warning;
        public GUI_Score MyGUI_Score;
        public GUI_Level MyGUI_Level;
        CoinScoreMultiplierObject MyCoinScoreMultiplier;

        public StringWorldTimed(Func<int, LevelSeedData> GetSeed, GUI_Timer Timer)
            : base(GetSeed)
        {
            MyGUI_Timer = Timer;

            Warning = new TimerWarning();
            Warning.MyTimer = Timer;

            Timer.OnTimeExpired += (timer) => { if (MyGUI_Score != null && !MyGUI_Score.MyLevel.LevelReleased) MyGUI_Score.SlideOut(GUI_Panel.PresetPos.Top); };
            Timer.OnTimeExpired += (timer) => { if (MyGUI_Level != null && !MyGUI_Level.MyLevel.LevelReleased) MyGUI_Level.SlideOut(GUI_Panel.PresetPos.Top); };

            MyGUI_Score = new GUI_Score(true);
            MyGUI_Level = new GUI_Level(true);

            // Coin score multiplier
            MyCoinScoreMultiplier = new CoinScoreMultiplierObject();

            // Add 'Perfect' watcher
            OnSwapToFirstLevel += (data) =>
                {
                    data.MyGame.AddGameObject(MyGUI_Timer, Warning, MyGUI_Score, MyGUI_Level,
                        MyCoinScoreMultiplier, new PerfectScoreObject(false, true, false));
                };
        }

		protected override void AdditionalSetLevel()
		{
			base.AdditionalSetLevel();

			NextLevelSeed.MyGame.MyLevel.SetBack(3);
		}

        public override void Release()
        {
            base.Release();

            MyGUI_Timer.ForceRelease();
        }

        public override void AdditionalSwapToLevelProcessing(GameData game)
        {
            base.AdditionalSwapToLevelProcessing(game);

            MyGUI_Level.SetLevel(CurLevelIndex + 1);

            game.TakeOnce = true;
        }
    }
}