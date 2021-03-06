﻿using System;
using Microsoft.Xna.Framework;

using CloudberryKingdom.Blocks;
using CloudberryKingdom.InGameObjects;

namespace CloudberryKingdom.Levels
{
    public class MakeFinalDoorVertical : MakeThing
    {
        protected Level MyLevel;

        /// <summary>
        /// The block on which the final door rests on.
        /// </summary>
        BlockBase FinalBlock;

        /// <summary>
        /// The position of the final door.
        /// </summary>
        Vector2 FinalPos;

        public MakeFinalDoorVertical(Level level)
        {
            MyLevel = level;
        }

        public override void Phase1()
        {
            base.Phase1();
        }

        Vector2 BobAvgPos(Level level)
        {
            Vector2 avg = Vector2.Zero;
            foreach (var bob in level.Bobs)
                avg += bob.Pos;

            return avg / level.Bobs.Count;
        }

        public override void Phase2()
        {
            base.Phase2();

            // Find a final block that was used by the computer.
            if (MyLevel.CurMakeData.PieceSeed.GeometryType == LevelGeometry.Down)
                FinalBlock = Tools.ArgMin(MyLevel.Blocks.FindAll(match => match.Core.GenData.Used), element => element.Core.Data.Position.Y);
            else
            {
                try
                {
                    var ValidBlocks = MyLevel.Blocks.FindAll(ValidFinalBlock);
                    FinalBlock = Tools.ArgMax(ValidBlocks, element => element.Pos.Y);

                    var avgpos = BobAvgPos(MyLevel);
                    ValidBlocks = ValidBlocks.FindAll(match => match.Pos.Y > FinalBlock.Pos.Y - 5);
                    FinalBlock = Tools.ArgMin(ValidBlocks, element => Math.Abs(element.Pos.X - avgpos.X));
                }
                catch (Exception e)
                {
                    Failed = true;
                    FinalBlock = null;
                    return;
                }

                // Test bad door placement: abort and report failure
                //if (Tools.GlobalRnd.RndFloat() > .8f)
                //{
                //    Failed = true;
                //    FinalBlock = null;
                //    return;
                //}
            }

            FinalPos = FinalBlock.Core.Data.Position;

            // Cut computer run short once the computer reaches the door.
            int Earliest = 100000;
            Vector2 pos = FinalPos;
            float Closest = -1;
            int NewLastStep = MyLevel.LastStep;
            for (int i = 0; i < MyLevel.CurPiece.NumBobs; i++)
                for (int j = MyLevel.LastStep - 1; j > 0; j--)
                {
                    Vector2 BobPos = MyLevel.CurPiece.Recording[i].AutoLocs[j];
                    float Dist = (BobPos - FinalPos).Length();

                    if (Closest == -1 || Dist < Closest)
                    {
                        Earliest = Math.Min(Earliest, j);
                        Closest = Dist;
                        pos = BobPos;
                        NewLastStep = j;
                    }
                }

            MyLevel.LastStep = Math.Min(Earliest, MyLevel.LastStep);
        }

        private static bool ValidFinalBlock(BlockBase block)
        {
            return block.Core.GenData.Used && block.Core.MyType == ObjectType.NormalBlock;
        }

        protected Door MadeDoor;
        public override void Phase3()
        {
            base.Phase3();

            // Add door
            MadeDoor = MyLevel.PlaceDoorOnBlock(FinalPos, FinalBlock, false);
            MadeDoor.Core.EditorCode1 = LevelConnector.EndOfLevelCode;

            // Attach an action to the door
            MakeFinalDoor.AttachDoorAction(MadeDoor);
        }

        public override void Cleanup()
        {
            MadeDoor = null;
            FinalBlock = null;
        }
    }
}
