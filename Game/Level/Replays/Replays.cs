﻿using CloudberryKingdom.Bobs;
using System;

namespace CloudberryKingdom.Levels
{
    public partial class Level
    {
        /// <summary>
        /// Whether a replay is available to be watched.
        /// </summary>
        public bool ReplayAvailable { get { return MySwarmBundle != null; } }

        public SwarmBundle MySwarmBundle;
        public Recording CurrentRecording;
        public bool Recording;
        ReplayGUI MyReplayGUI;

        public bool SingleOnly = false;

        public bool NoCameraChange = false;
        void SaveCamera()
        {
            if (NoCameraChange) return;

            HoldCamera = MainCamera;
            MainCamera = new Camera(MainCamera);
        }
        void RestoreCamera()
        {
            if (NoCameraChange) return;

            // Destroy the temporary replay camera and
            // start using the previous camera once again
            MainCamera.Release();
            MainCamera = HoldCamera;
        }

        public void SetReplay()
        {
            int NumBobs = Bobs.Count;
            Bobs.Clear();
            for (int i = 0; i < NumBobs; i++)
            {
                if (MySwarmBundle.CurrentSwarm.MainRecord.Recordings.Length <= i)
                    break;

                var hero = DefaultHeroType;
                if (DefaultHeroType2 != null && i == 1)
                    hero = DefaultHeroType2;

                Bob Comp = new Bob(hero, false);
                Comp.SetColorScheme(PlayerManager.Get(NumBobs - i - 1).ColorScheme);

                Comp.MyRecord = MySwarmBundle.CurrentSwarm.MainRecord.Recordings[i];
                Comp.CompControl = true;
                Comp.Immortal = true;
                AddBob(Comp);
            }
        }

        public void WatchReplay(bool SaveCurInfo)
        {
            Awardments.CheckForAward_Replay(PlayerManager.Score_Attempts);

            Tools.PhsxSpeed = 1;

            SuppressCheckpoints = true;
            GhostCheckpoints = true;

            MyReplayGUI = new ReplayGUI();
            MyReplayGUI.Type = ReplayGUIType.Replay;
            MyGame.AddGameObject(MyReplayGUI);

            MyReplayGUI.StartUp();

            if (Recording) StopRecording();

            if (!MySwarmBundle.Initialized)
                MySwarmBundle.Init(this);

            if (SaveCurInfo)
            {
                HoldPlayerBobs.Clear();
                HoldPlayerBobs.AddRange(Bobs);
                HoldCamPos = MainCamera.Data.Position;
                SaveCamera();
            }

            // Select the first swarm in the bundle to start with
            MySwarmBundle.SetSwarm(this, 0);

            PreventReset = false;
            FreezeCamera = false;
            Watching = true;
            Replay = true;
            ReplayPaused = false;
            //            MainReplayOnly = true;


            SetReplay();

            SetToReset = true;
        }

        public Action OnWatchComputer;

        public void WatchComputer() { WatchComputer(true); }
        public void WatchComputer(bool GUI)
        {
            if (Watching) return;

			if (MyGame != null) MyGame.WatchComputerEvent();

            Tools.PhsxSpeed = 1;

            // Consider the reset free if the players are close to the start
            FreeReset = CloseToStart();
            if (!FreeReset)
                CountReset();

            SaveCamera();

            if (GUI)
            {
                // Create the GUI
                MyReplayGUI = new ReplayGUI();
                MyReplayGUI.Type = ReplayGUIType.Computer;
                MyGame.AddGameObject(MyReplayGUI);

                MyReplayGUI.StartUp();
            }

            Watching = true;
            SuppressCheckpoints = true;
            GhostCheckpoints = true;

            // Swap the player Bobs for computer Bobs
            HoldPlayerBobs.Clear();
            HoldPlayerBobs.AddRange(Bobs);

            Bobs.Clear();
            for (int i = 0; i < CurPiece.NumBobs; i++)
            {
                var hero = DefaultHeroType;
                if (DefaultHeroType2 != null && i == 1)
                    hero = DefaultHeroType2;

                Bob Comp = new Bob(hero, false);

                Comp.MyPiece = CurPiece;
                Comp.MyPieceIndex = i;
                Comp.MyRecord = CurPiece.Recording[i];
                Comp.CompControl = true;
                Comp.Immortal = true;
                AddBob(Comp);
                
#if DEBUG
				Comp.SetColorScheme(PlayerManager.Player.ColorScheme);
#else
				Comp.SetColorScheme(ColorSchemeManager.ComputerColorSchemes[0]);
#endif			

                Comp.MoveData = CurPiece.MyMakeData.MoveData[i];
                int Copy = Comp.MoveData.Copy;
                if (Copy >= 0)
                    Comp.SetColorScheme(Bobs[Copy].MyColorScheme);
            }

            // Set special Bob parameters
            MySourceGame.SetAdditionalBobParameters(Bobs);

            // Additional actions
            if (OnWatchComputer != null) OnWatchComputer();

            SetToReset = true;
        }

        public bool EndOfReplay()
        {
            return CurPhsxStep >= CurPiece.PieceLength;
        }

        public Action OnEndReplay;
        public void EndReplay()
        {
            SuppressCheckpoints = false;
            GhostCheckpoints = false;

            RestoreCamera();

            Replay = Watching = false;
            Recording = false;
            ReplayPaused = false;

            MainCamera.Data.Position = HoldCamPos;
            //FreezeCamera = true;
            MainCamera.Update();

			foreach (Bob bob in Bobs)
			{
				bob.MyRecord = null;
				bob.Release();
			}
            Bobs.Clear();

            Bobs.AddRange(HoldPlayerBobs);
            foreach (Bob bob in Bobs)
            {
                bob.PlayerObject.AnimQueue.Clear();
                bob.PlayerObject.EnqueueAnimation(0, 0, true);
                bob.PlayerObject.DequeueTransfers();
            }

            if (OnEndReplay != null) OnEndReplay();
        }


        public void EndComputerWatch()
        {
            RestoreCamera();

            ReplayPaused = false;
            StartPlayerPlay();

            if (OnEndReplay != null) OnEndReplay();
        }
    }
}
