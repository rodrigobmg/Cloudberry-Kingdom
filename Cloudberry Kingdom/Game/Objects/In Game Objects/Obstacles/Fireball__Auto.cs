﻿using System;
using System.Linq;

using Microsoft.Xna.Framework;

using CloudberryKingdom.Blocks;
using CloudberryKingdom.Obstacles;

namespace CloudberryKingdom.Levels
{
    public class Fireball_Parameters : AutoGen_Parameters
    {
        public int NumDirections; // Evenly divide angle range into max(range/(N+4),pi/20) directions and randomly pick from those?
        public int Multi; // careful not to increase angle too much, start at greatest angle and add fireballs backwards
                            // use angle range as the spread angle 

        public Param FireballSpeed, FireballMaxAngle, FireballMinDist, MinFireballDensity, MaxFireballDensity, Period;
        public Param KeepUnused;

        /// <summary>
        /// Whether the fireballs arc.
        /// </summary>
        public bool Arc = false;

        public struct _Special
        {
            /// <summary>
            /// A special fill type, used for a survival level.
            /// </summary>
            public bool SurvivalFill;

            /// <summary>
            /// A special fill type, with emitters lining the borders of the level.
            /// </summary>
            public bool BorderFill;
        }
        public _Special Special;

        public TunnelFill Tunnel;
        public int TunnelTimeSpace = Unset.Int;
        public enum TunnelTypes { Chaos, _0, _45, _90, _45_45, _4way };
        public TunnelTypes TunnelType = TunnelTypes._0;

        public bool DoFill = false;

        public float BorderFillStep = Unset.Float;
        public bool BorderTop = true;

        public float SurvivalHallwaySpeed = Unset.Float;

        public override void SetParameters(PieceSeedData PieceSeed, Level level)
        {
            base.SetParameters(PieceSeed, level);

            NumAngles = 4;
            NumPeriods = 1;
            NumOffsets = 16;

            KeepUnused = new Param(PieceSeed);
            if (level.DefaultHeroType is BobPhsxSpaceship)
            {
                KeepUnused.SetVal(u => BobPhsxSpaceship.KeepUnused(u[Upgrade.Fireball]));
            }

            FillWeight = new Param(PieceSeed, u => u[Upgrade.Fireball]);

            if (PieceSeed.GeometryType == LevelGeometry.Up ||
                PieceSeed.GeometryType == LevelGeometry.Down)
                Special.BorderFill = true;

            Special.BorderFill = false;

            Tunnel = new TunnelFill();
            SetVal(ref TunnelTimeSpace, u => Math.Max(8, DifficultyHelper.Interp19(28, 8, u[Upgrade.Fireball])));

            SetVal(ref SurvivalHallwaySpeed, u =>
            {
                return DifficultyHelper.Interp19(20, 45, u[Upgrade.Speed]);
            });

            SetVal(ref BorderFillStep, u =>
            {
                if (u[Upgrade.Fireball] > 0)
                    DoFill = true;

                return DifficultyHelper.Interp159(800, 500, 200, u[Upgrade.Fireball]);
            });

            // General difficulty
            BobWidthLevel = new Param(PieceSeed);
            BobWidthLevel.SetVal(u => u[Upgrade.Fireball]);

            FireballSpeed = new Param(PieceSeed);
            FireballSpeed.SetVal(u => 8 + u[Upgrade.Speed] + 1f * u[Upgrade.Fireball]);

            FireballMaxAngle = new Param(PieceSeed);
            FireballMaxAngle.SetVal(u => Math.Min(750, 0 + 100 * u[Upgrade.Fireball]));

            FireballMinDist = new Param(PieceSeed);
            FireballMinDist.SetVal(u => DifficultyHelper.Interp159(700, 340, 120, u[Upgrade.Fireball]));

            MinFireballDensity = new Param(PieceSeed);
            MinFireballDensity.SetVal(u => 4 * u[Upgrade.Fireball]);

            MaxFireballDensity = new Param(PieceSeed);
            MaxFireballDensity.SetVal(u => 4 * u[Upgrade.Fireball]);

            Period = new Param(PieceSeed);
            Period.SetVal(u => Math.Max(20, 140 - 12 * u[Upgrade.Speed]));
        }
    }

    public sealed class Fireball_AutoGen : AutoGen
    {
        static readonly Fireball_AutoGen instance = new Fireball_AutoGen();
        public static Fireball_AutoGen Instance { get { return instance; } }

        static Fireball_AutoGen() { }
        Fireball_AutoGen()
        {
            Do_PreFill_2 = true;
        }

        public override AutoGen_Parameters SetParameters(PieceSeedData data, Level level)
        {
            Fireball_Parameters Params = new Fireball_Parameters();
            Params.SetParameters(data, level);

            return (AutoGen_Parameters)Params;
        }

        public override ObjectBase CreateAt(Level level, Vector2 pos, Vector2 BL, Vector2 TR)
        {
            base.CreateAt(level, pos, BL, TR);

            StyleData Style = level.Style;
            RichLevelGenData GenData = level.CurMakeData.GenData;
            PieceSeedData piece = level.CurMakeData.PieceSeed;

            // Get emitter parameters
            Fireball_Parameters Params = (Fireball_Parameters)level.Style.FindParams(Fireball_AutoGen.Instance);

            int Period = (int)Params.Period.GetVal(pos);
            int Offset = Params.ChooseOffset(Period, level.Rnd);

            return null;
        }

        void inner(Fireball_Parameters Params, Level level, Vector2 pos, int i, LevelGeometry Geometry)
        {
            Fireball emitter = (Fireball)CreateAt(level, pos);

            float Speed = Params.FireballSpeed.GetVal(pos) * (i == 0 ? 1 : -1);

            if (Params.Arc)
            {
                emitter.Core.StartData.Acceleration.Y = -Math.Abs(Speed) / 100f;
            }

            float MaxAngle = .001f * Params.FireballMaxAngle.GetVal(pos);
            double Angle = Fireball_AutoGen.GetAngle(MaxAngle, Params.NumAngles, level.Rnd);
            if (Geometry == LevelGeometry.Right) Angle += Math.PI / 2;
            emitter.Core.StartData.Velocity = CoreMath.AngleToDir(Angle) * Speed;

            level.AddObject(emitter);

            emitter.Core.GenData.EnforceBounds = false;
        }

        void BorderFill(Level level, Vector2 BL, Vector2 TR)
        {
            Fireball_Parameters Params = (Fireball_Parameters)level.Style.FindParams(Fireball_AutoGen.Instance);

            LevelGeometry Geometry = level.CurMakeData.PieceSeed.GeometryType;

            int HoldNumPeriods = Params.NumPeriods;
            Params.NumPeriods = 1;

            Vector2 pos = BL;
            
            float shift = -50;
            
            for (int i = 0; i <= 1; i++)
            {
                if (!Params.BorderTop && i == 1) continue;

                if (Geometry == LevelGeometry.Right)
                {
                    shift = -250;
                    pos.Y = (i == 0 ? level.MainCamera.BL.Y + shift : level.MainCamera.TR.Y - shift);
                    
                    pos.X = BL.X + Params.BorderFillStep/2;
                    while (pos.X < TR.X + 400)
                    {
                        int n = level.Rnd.RndInt(1, 3);
                        for (int q = 0; q < n; q++)
                            inner(Params, level, pos, i, Geometry);
                        pos.X += n * Params.BorderFillStep;
                    }
                }
                else
                {
                    pos.X = (i == 0 ? level.MainCamera.BL.X + shift : level.MainCamera.TR.X - shift);
                    for (pos.Y = BL.Y; pos.Y < TR.Y; pos.Y += Params.BorderFillStep)
                        inner(Params, level, pos, i, Geometry);
                }
            }

            Params.NumPeriods = HoldNumPeriods;
        }
        
        /*
        void Circle(Level level, Vector2 Center, float Radius, int Num, int Dir)
        {
            for (int j = 0; j < Num; j++)
            {
                Fireball emitter = (Fireball)CreateAt(level, Center);

                emitter.Period = 3 * emitter.Period / 2;
                emitter.Offset = (int)(j * ((float)emitter.Period / Num));

                emitter.Core.StartData.Velocity = Dir;

                emitter.Core.GenData.KeepIfUnused = true;

                level.AddObject(emitter);
            }
        }*/

        static float[] AngleSnaps = { 0, 90, 180, 270 };
        void SetSurvivalParams(Level level, Fireball emitter, Vector2 EmitPos, Vector2 AimPos)
        {
            Fireball_Parameters Params = (Fireball_Parameters)level.Style.FindParams(Fireball_AutoGen.Instance);

            float Speed = Params.SurvivalHallwaySpeed;

            emitter.Period = 500;
            emitter.Offset = level.Rnd.Rnd.Next(emitter.Period);

            emitter.Core.StartData.Velocity = Speed * Vector2.Normalize(AimPos - EmitPos);

            // Snap angle to 0/90/180/270 if close
            Vector2 polar = CoreMath.CartesianToPolar(emitter.Core.StartData.Velocity);

            foreach (float AngleSnap in AngleSnaps)
                if (CoreMath.AngleDist(CoreMath.c * polar.X, AngleSnap) < 7) polar.X = AngleSnap / CoreMath.c;

            emitter.Core.StartData.Velocity = CoreMath.PolarToCartesian(polar);

            emitter.Core.GenData.KeepIfUnused = true;

            emitter.Core.GenData.LimitDensity = false;
            emitter.Core.GenData.EnforceBounds = false;

            level.AddObject(emitter);
        }
        void SurvivalFill(Level level, Vector2 BL, Vector2 TR)
        {
            Vector2 Center = (TR + BL) / 2;

            int Num = 3000;

            int Period = 850;
            for (int j = 0; j * Period < Num; j++)
            for (int i = 0; i < Period; i++)
            {
                Vector2 EmitPosition, AimPosition;
                
                
                EmitPosition = level.Rnd.RndDir() * 3000 + Center;
                if (EmitPosition.X > TR.X + 500) EmitPosition.X = TR.X + 500;
                if (EmitPosition.X < BL.X - 500) EmitPosition.X = BL.X - 500;
                if (EmitPosition.Y > TR.Y + 500) EmitPosition.Y = TR.Y + 500;
                if (EmitPosition.Y < BL.Y - 500) EmitPosition.Y = BL.Y - 500;

                AimPosition = Center + new Vector2(level.Rnd.RndFloat(-900, 900), 150);
                

                //switch(3)
                switch(level.Rnd.RndInt(0, 3))
                {
                    case 0:
                        EmitPosition = new Vector2(level.Rnd.RndFloat(Center.X + 1000, Center.X - 1000, 75), TR.Y + 300);
                        AimPosition = EmitPosition + new Vector2(0, -1000);
                        break;

                    case 1:
                        EmitPosition = new Vector2(level.Rnd.RndFloat(Center.X + 1000, Center.X - 1000, 75), BL.Y - 300);
                        AimPosition = EmitPosition + new Vector2(0, 1000);
                        break;

                    case 2:
                        EmitPosition = new Vector2(TR.X + 300, level.Rnd.RndFloat(Center.Y - 350, Center.Y + 350, 75));
                        AimPosition = EmitPosition + new Vector2(-1000, 0);
                        break;

                    default:
                        EmitPosition = new Vector2(BL.X - 300, level.Rnd.RndFloat(Center.Y - 350, Center.Y + 350, 75));
                        AimPosition = EmitPosition + new Vector2(1000, 0);
                        break;
                }

                Fireball emitter = (Fireball)CreateAt(level, EmitPosition);
                SetSurvivalParams(level, emitter, EmitPosition, AimPosition);

                emitter.Offset = i;
                emitter.Period = Period;
            }
        }

        void SurvivalRotatingTunnel(Level level, Vector2 BL, Vector2 TR, int Length)
        {
            Fireball_Parameters Params = (Fireball_Parameters)level.Style.FindParams(Fireball_AutoGen.Instance);

            Length -= 30;

            int TimeSpace = Params.TunnelTimeSpace;
            int Width = 2250; int WidthNum = 10;


            Params.Tunnel.Init(Length / TimeSpace + 1, WidthNum);


            Vector2 Center = (TR + BL) / 2;

            float Dist = (TR - Center).Length() + 200;


            float Angle = 45;
            int I = 0;
            for (int i = 0; i < Length; i += TimeSpace)
            {
                switch (Params.TunnelType)
                {
                    case Fireball_Parameters.TunnelTypes.Chaos:
                        Angle += 77; break;

                    case Fireball_Parameters.TunnelTypes._0:
                        Angle = 0; break;

                    case Fireball_Parameters.TunnelTypes._45:
                        Angle = 45; break;

                    case Fireball_Parameters.TunnelTypes._90:
                        Angle = 90; break;

                    case Fireball_Parameters.TunnelTypes._4way:
                        Angle = 90 * I; break;

                    case Fireball_Parameters.TunnelTypes._45_45:
                        Angle += I % 2 == 0 ? 90 : -90; break;
                }
                
                
                //Angle = 150 * orientation;

                //if (i % 60 == 0) Angle += 90;

                //Angle += 180;

                

                Vector2 Dir = CoreMath.DegreesToDir(Angle);
                Vector2 Tangent = new Vector2(-Dir.Y, Dir.X);

                int J = 0;
                for (int j = -WidthNum / 2; j < WidthNum / 2; j++)
                {
                    Vector2 EmitPosition, AimPosition;

                    float offset = j;

                    // Stagger every other row
                    if (I % 2 == 0) offset += .5f;

                    // Calculate start position and aim
                    EmitPosition = Center + Dist * Dir + Tangent * offset * Width / WidthNum;
                    AimPosition = Center + Tangent * j * Width / WidthNum;

                    /*
                    EmitPosition = level.Rnd.RndDir() * 3000 + Center;
                    if (EmitPosition.X > TR.X) EmitPosition.X = TR.X + 200;
                    if (EmitPosition.X < BL.X) EmitPosition.X = BL.X - 200;
                    if (EmitPosition.Y > TR.Y) EmitPosition.Y = TR.Y + 200;
                    if (EmitPosition.Y < BL.Y) EmitPosition.Y = BL.Y - 200;

                    AimPosition = Center + new Vector2(level.Rnd.RndFloat(-900, 900), 150);
                    */

                    Fireball emitter = (Fireball)CreateAt(level, EmitPosition);
                    SetSurvivalParams(level, emitter, EmitPosition, AimPosition);
                    emitter.Offset = i;
                    emitter.Period = 100000;// Length;

                    Params.Tunnel.TunnelGUIDs[I, J] = emitter.Core.MyGuid;

                    J++;
                }

                I++;
            }
        }

        public Fireball_Parameters GetParams(Level level)
        {
            return (Fireball_Parameters)level.Style.FindParams(Fireball_AutoGen.Instance);
        }
        
        public override ObjectBase CreateAt(Level level, Vector2 pos)
        {
            Fireball_Parameters Params = GetParams(level);

            Fireball emitter = (Fireball)level.Recycle.GetObject(ObjectType.Fireball, true);

            emitter.Init(new PhsxData(), level);

            emitter.Core.Data.Position = pos;
            emitter.Core.StartData.Position = pos;

            emitter.Period = (level.Rnd.Rnd.Next(Params.NumPeriods - 1) + 1) * (int)Params.Period.GetVal(pos);
            
            if (Params.NumOffsets < 0)
                emitter.Offset = level.Rnd.Rnd.Next(emitter.Period);
            else
                emitter.Offset = level.Rnd.Rnd.Next(0, Params.NumOffsets) * emitter.Period / Params.NumOffsets;

            return emitter;
        }

        public static double GetAngle(float MaxAngle, int NumAngles, Rand Rnd)
        {
            if (NumAngles < 0)
                return Rnd.Rnd.NextDouble() * 2 * MaxAngle - MaxAngle;
            else if (NumAngles == 1)
                return 0;
            else
                return Rnd.Rnd.Next(0, NumAngles) * 2f * MaxAngle / (NumAngles - 1) - MaxAngle;
        }


        public override void PreFill_2(Level level, Vector2 BL, Vector2 TR)
        {
            base.PreFill_2(level, BL, TR);

            // Get Fireball parameters
            Fireball_Parameters Params = (Fireball_Parameters)level.Style.FindParams(Fireball_AutoGen.Instance);

            int Length = level.CurPiece.PieceLength;

            if (Params.Special.SurvivalFill)
                //SurvivalFill(level, BL, TR);
                SurvivalRotatingTunnel(level, BL, TR, Length);

            //if (Params.Special.BorderFill)
                //BorderFill(level, BL, TR);

            if (Params.DoStage2Fill && Params.DoFill)
            ////    AutoFireballs_OnIceBlocks(level);
                BorderFill(level, BL, TR);
                //level.AutoFireballs();
        }

        public override void Cleanup_2(Level level, Vector2 BL, Vector2 TR)
        {
            base.Cleanup_2(level, BL, TR);
            //level.CleanupFireballs(BL, TR);

            // Get Fireball parameters
            Fireball_Parameters Params = (Fireball_Parameters)level.Style.FindParams(Fireball_AutoGen.Instance);

            if (Params.Special.SurvivalFill)
                Params.Tunnel.CleanupTunnel(level);
        }
    }

    public partial class Level
    {
        public void CleanupFireballs(Vector2 BL, Vector2 TR)
        {
            // Get Fireball parameters
            Fireball_Parameters Params = (Fireball_Parameters)Style.FindParams(Fireball_AutoGen.Instance);

            Cleanup(ObjectType.Fireball, pos =>
            {
                float dist = Params.FireballMinDist.GetVal(pos);
                return new Vector2(dist, dist);
            }, BL, TR);
        }

        public void AutoFireballs()
        {
            // Get Fireball parameters
            Fireball_Parameters Params = (Fireball_Parameters)Style.FindParams(Fireball_AutoGen.Instance);
            
            int density;

            foreach (BlockBase block in Blocks)
            {
                Vector2 pos = block.Core.Data.Position;

                if (block.Core.Placed) continue;

                //if (!(block is NormalBlock)) continue;
                if (block.BlockCore.MyType == ObjectType.LavaBlock) continue;
                if (block.BlockCore.Virgin) continue;
                if (block.BlockCore.Finalized) continue;



                density = (int)Rnd.RndFloat(Params.MinFireballDensity.GetVal(pos),
                                         Params.MaxFireballDensity.GetVal(pos));

                // Add emitter
                float xmin = block.Box.Current.BL.X + 80;
                float xmax = block.Box.Current.TR.X - 80;
                float xdif = xmax - xmin;// block.Box.Current.TR.X - block.Box.Current.BL.X - 150;
                float ymax = Math.Min(block.Box.Current.TR.Y - 80, MainCamera.TR.Y - 80);
                float ymin = Math.Max(block.Box.Current.BL.Y + 80, MainCamera.BL.Y + 80);
                float ydif = ymax - ymin;//block.Box.Current.TR.Y - block.Box.Current.BL.Y - 150;

                if (block.BlockCore.MyType == ObjectType.LavaBlock && (xdif < 20)) continue;
                if (block.BlockCore.MyType != ObjectType.LavaBlock && (xdif <= 80 || ydif <= 80)) continue;
                
                float average = (int)(Math.Max(xdif, ydif) * (float)density / (700f ));
                int n = (int)average;
                if (average < 1) if (Rnd.Rnd.NextDouble() < average) n = 1;

                float xspace = xdif / (float)Math.Max(1,(int)(xdif / 125));
                float yspace = ydif / (float)Math.Max(1,(int)(ydif / 125));

                for (int i = 0; i < n; i++)
                {
                    float x = (float)Rnd.Rnd.NextDouble() * xdif + xmin;
                    float y = (float)Rnd.Rnd.NextDouble() * ydif + ymin;

                    x = (int)((x - xmin) / xspace) * xspace + xmin;
                    y = (int)((y - ymin) / yspace) * yspace + ymin;

                    // Make sure chosen location doesn't intersect another block
                    if (!Blocks.All(delegate(BlockBase block2)
                    {
                        return block2 == block || block2.BlockCore.Layer < block.BlockCore.Layer ||
                            x > block2.Box.Current.TR.X + 100 || x < block2.Box.Current.BL.X - 100 ||
                            y > block2.Box.Current.TR.Y + 100 || y < block2.Box.Current.BL.Y - 100;
                    })) continue;

                    Fireball emitter = (Fireball)Fireball_AutoGen.Instance.CreateAt(this, new Vector2(x, y));
                        
                    emitter.SetParentBlock(block);

                    int Dir = 3;
                    int VerticalOdds = 4;
                    int HorizontalOdds = 1;
                    if (block.BlockCore.Ceiling) Dir = Rnd.Choose(HorizontalOdds, VerticalOdds, HorizontalOdds, 0);
                    else Dir = Rnd.Choose(HorizontalOdds, 0, HorizontalOdds, VerticalOdds);

                    if (block.BlockCore.Ceiling) Dir = 1; else Dir = 3;

                    // Diagonal paths
                    float MaxAngle = .001f * Params.FireballMaxAngle.GetVal(pos);
                    double Angle = Fireball_AutoGen.GetAngle(MaxAngle, Params.NumAngles, Rnd);

                    // Lava emitters are always vertical and invisible
                    float Speed = Params.FireballSpeed.GetVal(block.Core.Data.Position);
                    if (block.BlockCore.MyType == ObjectType.LavaBlock)
                    {
                        emitter.Core.StartData.Acceleration = new Vector2(0, -.7f);
                        emitter.Core.StartData.Velocity = new Vector2(0, 50);
                    }
                    else
                    switch (Dir)
                    {
                        case 3:
                            Angle += Math.PI / 2;
                            //emitter.Core.StartData.Velocity = new Vector2(0, Speed);
                            emitter.Core.StartData.Velocity = CoreMath.AngleToDir(Angle) * Speed;

                            break;

                        case 1:
                            Angle -= Math.PI / 2;
                            //emitter.Core.StartData.Velocity = new Vector2(0, -Speed);
                            emitter.Core.StartData.Velocity = CoreMath.AngleToDir(Angle) * Speed;

                            break;

                        case 0:
                            emitter.Core.StartData.Velocity = new Vector2(Speed, 0);

                            break;

                        case 2:
                            emitter.Core.StartData.Velocity = new Vector2(-Speed, 0);

                            break;
                    }

                    AddObject(emitter);
                }
            }
        }
    }
}