using System;
using Microsoft.Xna.Framework;

using CoreEngine.Random;

using CloudberryKingdom.Levels;

namespace CloudberryKingdom
{
    public delegate void ModifyPieceSeedData(PieceSeedData Piece);
    public delegate void ModifyMakeData(ref Level.MakeData makeData);

    public enum LevelGeometry { Right, Up, OneScreen, Down, Big }
    public enum LevelZoom { Normal, Big }

    public enum MetaGameType { None, Escalation, TimeCrisis };

    public class PieceSeedData
    {
        public MetaGameType MyMetaGameType = MetaGameType.None;

        public void ApplyMetaGameStyling()
        {
            switch (MyMetaGameType)
            {
                case MetaGameType.Escalation:
                    // Shorten the initial computer delay
                    Style.ComputerWaitLengthRange = new Vector2(8, 35);

                    Style.MyModParams = (level, p) =>
                    {
                        Coin_Parameters Params = (Coin_Parameters)p.Style.FindParams(Coin_AutoGen.Instance);
                        Params.StartFrame = 90;
                        Params.FillType = Coin_Parameters.FillTypes.Regular;
                    };

                    break;

                case MetaGameType.TimeCrisis:
                    // Shorten the initial computer delay
                    Style.ComputerWaitLengthRange = new Vector2(4, 23);

                    // Only one path
                    //Paths = 1; LockNumOfPaths = true;

                    Style.MyModParams = (level, p) =>
                    {
                        Coin_Parameters Params = (Coin_Parameters)p.Style.FindParams(Coin_AutoGen.Instance);
                        Params.FillType = Coin_Parameters.FillTypes.Rush;
                    };

                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Uses the upgrade data in MyUpgrades1 to calculate the level gen data.
        /// </summary>
        public void CalculateSimple()
        {
            MyUpgrades1.CalcGenData(MyGenData.gen1, Style);

            RndDifficulty.ZeroUpgrades(MyUpgrades2);
            MyUpgrades1.UpgradeLevels.CopyTo(MyUpgrades2.UpgradeLevels, 0);
            MyUpgrades2.CalcGenData(MyGenData.gen2, Style);
        }

        public Action<Level> PreStage1, PreStage2;

        public AutoGen_Parameters this[AutoGen gen]
        {
            get { return Style.FindParams(gen); }
        }

        /// <summary>
        /// Type of level to be made, relating to shape and direction. Different from the GameType.
        /// </summary>
        public LevelGeometry GeometryType = LevelGeometry.Right;

        /// <summary>
        /// Type of level to be made, relating to the camera zoom.
        /// </summary>
        public LevelZoom ZoomType = LevelZoom.Normal;

        public float ExtraBlockLength = 0;

        public StyleData Style;

        public RichLevelGenData MyGenData;
        public Upgrades MyUpgrades1, MyUpgrades2;

        public Upgrades u { get { return MyUpgrades1; } }

        public Vector2 Start, End;
        public Vector2 CamZoneStartAdd, CamZoneEndAdd;

        int _Paths = -1;
        public int Paths
        {
            get
            {
                return _Paths;
            }
            set
            {
                _Paths = value;
            }
        }

        public bool LockNumOfPaths = false;

        public Level.LadderType Ladder;
        public BlockEmitter_Parameters.BoxStyle ElevatorBoxStyle = BlockEmitter_Parameters.BoxStyle.TopOnly;

        public PieceSeedData PieceSeed; // Used if this is a platform used for making new platforms

        public bool CheckpointsAtStart, InitialCheckpointsHere;

        public int MyPieceIndex = -1;

        public void Release()
        {
            if (Style != null) Style.Release(); Style = null;
            PieceSeed = null;
            MyGenData = null;
            MyLevelSeed = null;
        }

        public void CopyFrom(PieceSeedData piece)
        {
            //Style = piece.Style.Clone();
            MyUpgrades1.CopyFrom(piece.MyUpgrades1);
            MyUpgrades1.CalcGenData(MyGenData.gen1, Style);
            MyUpgrades2.CopyFrom(piece.MyUpgrades2);
            MyUpgrades2.CalcGenData(MyGenData.gen2, Style);
        }

        public void CopyUpgrades(PieceSeedData piece)
        {
            MyUpgrades1.CopyFrom(piece.MyUpgrades1);
            MyUpgrades2.CopyFrom(piece.MyUpgrades2);
        }

        public void CalcBounds()
        {
        }

        public void StandardClose()
        {
            int TestNumber;
                
            TestNumber = Rnd.RndInt(0, 1000);
            Tools.Write(string.Format("Test close start: {0}", TestNumber));

            MyUpgrades1.CalcGenData(MyGenData.gen1, Style);

            RndDifficulty.ZeroUpgrades(MyUpgrades2);
            MyUpgrades1.UpgradeLevels.CopyTo(MyUpgrades2.UpgradeLevels, 0);
            MyUpgrades2.CalcGenData(MyGenData.gen2, Style);

            TestNumber = Rnd.RndInt(0, 1000);
            Tools.Write(string.Format("Test close end: {0}", TestNumber));

            Style.MyInitialPlatsType = StyleData.InitialPlatsType.Door;
            Style.MyFinalPlatsType = StyleData.FinalPlatsType.Door;

            
            //Paths = 1; LockNumOfPaths = true;
        }

        LevelSeedData MyLevelSeed;
        public Rand Rnd { get { return MyLevelSeed.Rnd; } }

        public PieceSeedData(LevelSeedData LevelSeed)
        {
            MyLevelSeed = LevelSeed;
            Init(LevelGeometry.Right);
        }

        public PieceSeedData(int Index, LevelGeometry Type, LevelSeedData LevelSeed)
        {
            MyLevelSeed = LevelSeed;
            MyPieceIndex = Index;
            Init(Type);
        }

        void Init(LevelGeometry Type)
        {
            GeometryType = Type;

            if (MyLevelSeed != null)
            switch (GeometryType)
            {
                case LevelGeometry.Right: Style = new SingleData(Rnd); break;
                case LevelGeometry.Down: Style = new DownData(Rnd); break;
                case LevelGeometry.Up: Style = new UpData(Rnd); break;
                case LevelGeometry.OneScreen: Style = new OneScreenData(Rnd); break;
                case LevelGeometry.Big: Style = new BigData(Rnd); break;
            }
            
            MyGenData = new RichLevelGenData();
            MyGenData.gen1 = new LevelGenData();
            MyGenData.gen2 = new LevelGenData();

            MyUpgrades1 = new Upgrades();
            MyUpgrades2 = new Upgrades();

            //MyGenData.gen3 = new LevelGenData();
            //MyUpgrades1.CalcGenData(MyGenData.gen3);

            Ladder = Level.LadderType.None;
        }


        public void NoBlobs()
        {
            MyUpgrades1[Upgrade.FallingBlock] =
                Math.Max(MyUpgrades1[Upgrade.FallingBlock],
                         MyUpgrades1[Upgrade.FlyBlob]);
            MyUpgrades1[Upgrade.FlyBlob] = 0;

            MyUpgrades2[Upgrade.MovingBlock] =
                Math.Max(MyUpgrades2[Upgrade.MovingBlock],
                         MyUpgrades2[Upgrade.FlyBlob]);
            MyUpgrades2[Upgrade.FlyBlob] = 0;
        }
    }
}