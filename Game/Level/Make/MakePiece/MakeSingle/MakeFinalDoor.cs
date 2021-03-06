﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

using CloudberryKingdom.Blocks;
using CloudberryKingdom.InGameObjects;

namespace CloudberryKingdom.Levels
{
    public class MakeFinalDoor : MakeThing
    {
        Level MyLevel;

        /// <summary>
        /// The block on which the final door rests on.
        /// </summary>
        public BlockBase FinalBlock;

        /// <summary>
        /// The position of the final door.
        /// </summary>
        public Vector2 FinalPos;

        List<BlockBase> FinalBlocks = new List<BlockBase>();

        public MakeFinalDoor(Level level)
        {
            MyLevel = level;
        }

        public override void Phase1()
        {
            base.Phase1();

            MyLevel.EndBuffer = 1500;

            Vector2 BL;

            // New style end blocks
            if (MyLevel.MyTileSet.FixedWidths)
                BL = new Vector2(MyLevel.MaxRight + 450, MyLevel.MainCamera.BL.Y + Level.SafetyNetHeight);
            // Old style end blocks
            else
                BL = new Vector2(MyLevel.MaxRight + 450, MyLevel.MainCamera.BL.Y + Level.SafetyNetHeight + 65);
            
            float Spacing = 200;
            Vector2 TR = new Vector2(MyLevel.MaxRight + 1000, MyLevel.MainCamera.TR.Y - 850);

            float NewRight = MyLevel.VanillaFill(BL, TR, 400, Spacing,
            block => block.BlockCore.EndPiece = true, ModBlock);

            // Make lowest block a safety (we'll place the door here if no other block is used)
            FinalBlocks[0].Core.GenData.KeepIfUnused = true;
            FinalBlocks[0].BlockCore.NonTopUsed = true;

            // New style end blocks
            if (MyLevel.MyTileSet.FixedWidths)
            {
                MyLevel.LastSafetyBlock.Extend(Side.Right, FinalBlocks[0].Box.BL.X - 50);
            }
            // Old style end blocks
            else
            {
                // Extend lowest block to match up with safety net (or extend safety net instead)
                if (MyLevel.LastSafetyBlock != null &&
                    MyLevel.LastSafetyBlock.Box.TR.X + 50 < FinalBlocks[0].Box.BL.X)
                    FinalBlocks[0].Extend(Side.Left, MyLevel.LastSafetyBlock.Box.TR.X + 50);
                else
                    MyLevel.LastSafetyBlock.Extend(Side.Right, FinalBlocks[0].Box.BL.X - 50);
            }
        }

        void ModBlock(BlockBase block)
        {
            block.BlockCore.DisableFlexibleHeight = true;
            block.BlockCore.DeleteIfTopOnly = true;
            block.BlockCore.GenData.RemoveIfUnused = true;
            block.BlockCore.GenData.AlwaysUse = false;
            block.Core.EditorCode1 = "FinalBlock";
            FinalBlocks.Add(block);

            // New style end blocks
            if (MyLevel.MyTileSet.FixedWidths)
            {
                block.BlockCore.EndPiece = true;
                block.Core.DrawLayer = 0;
            }
        }

        public override void Phase2()
        {
            base.Phase2();

            // Find the highest final block that was used by the computer.
            FinalBlock = MyLevel.Blocks
                .FindAll(match => match.Core.GenData.Used && match.Core == "FinalBlock")
                .ArgMax(block => block.Box.TR.Y);

            // If none exist use the lowest block
            if (FinalBlock == null)
            {
                FinalBlock = FinalBlocks[0];
                FinalBlock.StampAsUsed(0);
            }
            else
            {
                if (FinalBlocks[0].Core.GenData.KeepIfUnused && !FinalBlocks[0].Core.GenData.Used)
                    FinalBlocks[0].CollectSelf();
            }

            FinalPos = new Vector2(FinalBlock.Box.BL.X + 200, FinalBlock.Box.TR.Y);

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

        public override void Phase3()
        {
            base.Phase3();

            // New style end blocks
            if (MyLevel.MyTileSet.FixedWidths)
            {
                FinalPos.X += 230;
            }

            // Add door
            Door door = MyLevel.PlaceDoorOnBlock(FinalPos, FinalBlock, MyLevel.MyTileSet.CustomStartEnd ? false : true);

            // New style end blocks
            if (MyLevel.MyTileSet.FixedWidths)
            {
                door.Mirror = true;
            }

            SetFinalDoor(door, MyLevel, FinalPos);

            // Push lava down
            MyLevel.PushLava(FinalBlock.Box.Target.TR.Y - 60);

            // Add exit sign
            if (MyLevel.Info.Doors.ShowSign)
            {
                Sign sign = new Sign(false, MyLevel);
                sign.PlaceAt(door.GetTop());
                MyLevel.AddObject(sign);
            }

            // Cleanup
            FinalBlocks.Clear();
            FinalBlock = null;
        }

        public static void AttachDoorAction(ILevelConnector door)
        {
            if (Tools.WorldMap != null)
            {
                StringWorldGameData stringworld = Tools.WorldMap as StringWorldGameData;
                if (stringworld != null)
                {
                    door.OnOpen = stringworld.EOL_StringWorldDoorAction;
                    door.OnEnter = stringworld.EOL_StringWorldDoorEndAction;
                }
            }
        }

        public static void SetFinalDoor(Door door, Level level, Vector2 FinalPos)
        {
            door.Core.EditorCode1 = LevelConnector.EndOfLevelCode;

            // Attach an action to the door
            AttachDoorAction(door);

            // Mod CameraZone
            CameraZone camzone = (CameraZone)level.Objects.Find(obj =>
                obj.Core.MyType == ObjectType.CameraZone);

            camzone.End.X = FinalPos.X - level.MainCamera.GetWidth() / 2 + 500;

            if (level.PieceSeed.ZoomType == LevelZoom.Big)
                camzone.End.X -= 150;
        }
    }
}
