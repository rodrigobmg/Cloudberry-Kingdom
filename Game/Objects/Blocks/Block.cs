﻿using System;
using System.IO;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using CoreEngine.Random;

using CloudberryKingdom.Bobs;
using CloudberryKingdom.Levels;

namespace CloudberryKingdom.Blocks
{
    public class BlockData : ObjectData
    {
        public void Decide_RemoveIfUnused(float ChanceToKeep, Rand Rnd)
        {
            GenData.Decide_RemoveIfUnused(ChanceToKeep, Rnd);
            if (GenData.RemoveIfUnused || Rnd.RndFloat() < .75f)
                BlobsOnTop = true;
            else
                Virgin = true;
        }

        public bool Safe;

        public bool Finalized, NoExtend;

        /// <summary>
        /// If false then objects attached to the block use Box.Target.Center as their reference point,
        /// otherwise they use CustomCenterAsParent
        /// </summary>
        public bool UseCustomCenterAsParent;
        public Vector2 CustomCenterAsParent;
        public Vector2 OffsetMultAsParent;

        /// <summary>
        /// If true the block is drawn upside down.
        /// </summary>
        public PieceQuad.Orientation MyOrientation;

        /// <summary>
        /// If true, overlapping non-child objects are removed.
        /// </summary>
        public bool RemoveOverlappingObjects;

        /// <summary>
        /// True if any part of the block besides the top has been used.
        /// </summary>
        public bool NonTopUsed;

        /// <summary>
        /// If true the block can not have its height modified, even if the tile set allows it.
        /// </summary>
        public bool DisableFlexibleHeight;

        /// <summary>
        /// If true the block is deleted if ever attempted to be made TopOnly
        /// </summary>
        public bool DeleteIfTopOnly;

        public bool StoodOn, HitHead;

        /// <summary>
        /// When true the block will not override the imposition of another block.
        /// </summary>
        public bool DoNotPushHard;

        /// <summary>
        /// If true then a player that lands on this block will take on its Y-velocity.
        /// </summary>
        public bool GivesVelocity;

        /// <summary>
        /// The speed of the top surface of this block
        /// </summary>
        public float GroundSpeed;

        public float Layer;

        public bool UseTopOnlyTexture;

        public bool Ceiling, CeilingDraw, BlobsOnTop, Virgin;

        /// <summary>
        /// If true this block is an end piece of a level, typically with a door on it.
        /// </summary>
        public bool EndPiece;

        /// <summary>
        /// If true this block is an start piece of a level, typically with a door on it.
        /// </summary>
        public bool StartPiece;

        public bool OnlyCollidesWithLowerLayers;

        public BlockBase TopRightNeighbor, TopLeftNeighbor;

        public List<ObjectBase> Objects = new List<ObjectBase>();

        public override void Release()
        {
            base.Release();

            //Objects = null;
            Objects.Clear();

            TopRightNeighbor = null;
            TopLeftNeighbor = null;
        }

        public void Draw()
        {
            if (Objects == null) return;

            foreach (ObjectBase obj in Objects)
                if (!obj.Core.MarkedForDeletion)
                    obj.Draw();
        }

        public override void Init()
        {
            base.Init();

            //Objects = new List<IObject>();

            DoNotPushHard = false;

            UseCustomCenterAsParent = false;
            CustomCenterAsParent = Vector2.Zero;
            OffsetMultAsParent = Vector2.One;

            MyOrientation = PieceQuad.Orientation.Normal;

            RemoveOverlappingObjects = true;

            Finalized = NoExtend = false;

            NonTopUsed = false;

            DisableFlexibleHeight = false;
            DeleteIfTopOnly = false;

            StoodOn = HitHead = false;

            GivesVelocity = true;

            UseTopOnlyTexture = true;

            Layer = 0;

            CeilingDraw = Ceiling = BlobsOnTop = Virgin = false;

            TopLeftNeighbor = TopRightNeighbor = null;
        }

        public override void Clone(ObjectData A)
        {
            base.Clone(A);

            BlockData BlockDataA = A as BlockData;
            if (BlockDataA == null)
                throw(new Exception("Can't copy block data from object data"));

            Safe = BlockDataA.Safe;

            UseCustomCenterAsParent = BlockDataA.UseCustomCenterAsParent;
            CustomCenterAsParent = BlockDataA.CustomCenterAsParent;
            OffsetMultAsParent = BlockDataA.OffsetMultAsParent;

            MyOrientation = BlockDataA.MyOrientation;

            GivesVelocity = BlockDataA.GivesVelocity;

            EndPiece = BlockDataA.EndPiece;

            BlobsOnTop = BlockDataA.BlobsOnTop;
            Ceiling = BlockDataA.Ceiling;
            CeilingDraw = BlockDataA.CeilingDraw;
            Virgin = BlockDataA.Virgin;

            UseTopOnlyTexture = BlockDataA.UseTopOnlyTexture;

            OnlyCollidesWithLowerLayers = BlockDataA.OnlyCollidesWithLowerLayers;

            Layer = BlockDataA.Layer;

            Objects.Clear();
            Objects.AddRange(BlockDataA.Objects);
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);

            writer.Write(Safe);

            writer.Write(BlobsOnTop);
            writer.Write(Ceiling);
            writer.Write(Virgin);

            writer.Write(OnlyCollidesWithLowerLayers);

            writer.Write(Layer);
        }

        public override void Read(BinaryReader reader)
        {
            base.Read(reader);

            Safe = reader.ReadBoolean();

            BlobsOnTop = reader.ReadBoolean();
            Ceiling = reader.ReadBoolean();
            Virgin = reader.ReadBoolean();

            OnlyCollidesWithLowerLayers = reader.ReadBoolean();

            Layer = reader.ReadInt32();
        }
    }

    public class BlockBase : ObjectBase
    {
        public AABox MyBox;
        public AABox Box { get { return MyBox; } }

        public bool Active;
        public bool IsActive { get { return Active; } set { Active = value; } }

        protected BlockData BlockCoreData;
        public BlockData BlockCore { get { return BlockCoreData; } }

        public BlockBase()
        {
            BlockCoreData = new BlockData();
            CoreData = BlockCore as ObjectData;
        }

        public override void Release()
        {
            base.Release();
            
            MyBox = null;

            if (MyDraw != null)
            {
                MyDraw.Release();
                MyDraw = null;
            }
        }

        public override void Reset(bool BoxesOnly)
        {
            base.Reset(BoxesOnly);

            BlockCore.BoxesOnly = BoxesOnly;

            if (!Core.BoxesOnly)
                ResetPieces();
        }

        public void Init(ref Vector2 center, ref Vector2 size, Level level, BlockGroup group)
        {
            size *= level.Info.ScaleAll * level.Info.ScaleAllBlocks;

            if (level.MyTileSet.FixedWidths)
                group.SnapWidthUp(ref size);
            MyBox.Initialize(center, size);
            MyDraw.MyTemplate = Core.MyTileSet.GetPieceTemplate(this, level.Rnd, group);

            bool UseLowerBlockBounds = false;
            if (level != null && level.CurMakeData != null)
                UseLowerBlockBounds = level.Style.UseLowerBlockBounds;
            
            AdditionalInit(ref center, ref size, level, UseLowerBlockBounds);
        }

        public void AdditionalInit(ref Vector2 center, ref Vector2 size, Level level, bool UseLowerBlockBounds)
        {
            float newsizey = MyDraw.MyTemplate.ModLowerBlockBound;
            if (UseLowerBlockBounds && newsizey != 0)
            {
                float shift;

                if (MyDraw.MyTemplate.RelativeLowerLip)
                {
                    shift = newsizey;
                    center.Y -= shift / 2;
                    size.Y += shift;
                }
                else
                {
                    shift = (newsizey - size.Y);
                    center.Y -= shift / 2;
                    size.Y += shift;
                }
                MyBox.Initialize(center, size);
            }

            Core.Data.Position = BlockCore.Data.Position = BlockCore.StartData.Position = center;

            if (!Core.BoxesOnly) Reset(false);
        }

        public NormalBlockDraw MyDraw;
        public virtual void ResetPieces()
        {
            if (MyDraw == null) return;

            MyDraw.Init(this);

            MyDraw.MyPieces.Center.Playing = false;
        }

        public override void PhsxStep2()
        {
            if (!Active) return;

            MyBox.SwapToCurrent();
        }


        public virtual void Extend(Side side, float pos) { }

        public virtual void LandedOn(Bob bob) { }
        public virtual void HitHeadOn(Bob bob) { }
        public virtual void SideHit(Bob bob) { }
        public virtual void Hit(Bob bob) { }

        public virtual bool PostCollidePreDecision(Bob bob)
        {
            return false;
        }

		public virtual bool PostCollideDecision_Bottom_FourWay(Bob bob, ref ColType Col, ref bool Overlap)
		{
			BobPhsxFourWay fourway = bob.MyPhsx as BobPhsxFourWay;
			if (fourway == null) return PostCollideDecision_Bottom_Normal(bob, ref Col, ref Overlap);

			if (Col == ColType.Bottom)
			{
				if (fourway.AirTime > 40)
					return false;
				else
					return true;
			}
			return false;
		}
		
        public virtual bool PostCollideDecision_Bottom_Meat(Bob bob, ref ColType Col, ref bool Overlap)
        {
            if (Col == ColType.Bottom) return true;
            return false;
        }
        public virtual bool PostCollideDecision_Bottom_Normal(Bob bob, ref ColType Col, ref bool Overlap)
        {
            if (bob.MyPhsx.Gravity > 0)
            {
                if (Col == ColType.Bottom) return true;
                return false;
            }
            else
            {
                if (Col == ColType.Top) return true;
                return false;
            }
        }
        public virtual bool PostCollideDecision_Bottom(Bob bob, ref ColType Col, ref bool Overlap)
        {
            if (bob.MyPhsx is BobPhsxMeat)
                return PostCollideDecision_Bottom_Meat(bob, ref Col, ref Overlap);
			//else if (bob.MyPhsx is BobPhsxFourWay)
				//return PostCollideDecision_Bottom_FourWay(bob, ref Col, ref Overlap);
            else
                return PostCollideDecision_Bottom_Normal(bob, ref Col, ref Overlap);
        }

        public virtual bool PostCollideDecision_Side_Meat(Bob bob, ref ColType Col, ref bool Overlap)
        {
            if (Col == ColType.Left || Col == ColType.Right)
            {
                if (Core.GetPhsxStep() < bob.MyPhsx.LastUsedStamp + 7) return true;

                if (Box.BL.Y > bob.Box.TR.Y - 40) return true;
                if (Box.TR.Y < bob.Box.BL.Y + 40) return true;

                if (this is BouncyBlock) return true;

                float Safety = 860;
                if (bob.Pos.X > Cam.Pos.X + Safety && Col == ColType.Left) return false;
                if (bob.Pos.X < Cam.Pos.X - Safety && Col == ColType.Right) return false;
                if (bob.Pos.X > Cam.Pos.X + Safety && Col == ColType.Right) return true;
                if (bob.Pos.X < Cam.Pos.X - Safety && Col == ColType.Left) return true;

                if (bob.WantsToLand)
                    return false;
                else
                    return true;
            }

            return false;
        }
        public virtual bool PostCollideDecision_Side_Normal(Bob bob, ref ColType Col, ref bool Overlap)
        {
            if (Col == ColType.Left || Col == ColType.Right) return true;

            if (bob.MyPhsx.Gravity > 0)
            {
                if (Col != ColType.Top && Overlap) return true;
            }
            else
            {
                if (Col != ColType.Bottom && Overlap) return true;
            }
            
            return false;
        }
        public virtual bool PostCollideDecision_Side(Bob bob, ref ColType Col, ref bool Overlap)
        {
            if (bob.MyPhsx is BobPhsxMeat)
                return PostCollideDecision_Side_Meat(bob, ref Col, ref Overlap);
            else
                return PostCollideDecision_Side_Normal(bob, ref Col, ref Overlap);
        }

        public virtual bool PostCollideDecision_Land_Meat(Bob bob, ref ColType Col, ref bool Overlap)
        {
            BobPhsxMeat meat = (BobPhsxMeat)bob.MyPhsx;

            if (meat.WantToLandOnTop && Col == ColType.Top) return false;

            if (this is BouncyBlock) return false;

            if (Col == ColType.Top) return true;

            return false;
        }
        public virtual bool PostCollideDecision_Land_Normal(Bob bob, ref ColType Col, ref bool Overlap)
        {
            if (bob.MyPhsx.Gravity > 0)
            {
                if (Col == ColType.Top && bob.WantsToLand == false) return true;
            }
            else
            {
                if (Col == ColType.Bottom && bob.WantsToLand == false) return true;
            }

            return false;
        }
		public virtual bool PostCollideDecision_Land_FourWay(Bob bob, ref ColType Col, ref bool Overlap)
		{
			if (Col == ColType.Top && bob.WantsToLand == false) return true;
			return false;
		}
        public virtual bool PostCollideDecision_Land(Bob bob, ref ColType Col, ref bool Overlap)
        {
            if (bob.MyPhsx is BobPhsxMeat)
                return PostCollideDecision_Land_Meat(bob, ref Col, ref Overlap);
			//else if (bob.MyPhsx is BobPhsxFourWay)
				//return PostCollideDecision_Land_FourWay(bob, ref Col, ref Overlap);
            else
                return PostCollideDecision_Land_Normal(bob, ref Col, ref Overlap);
        }

        public virtual void PostCollideDecision(Bob bob, ref ColType Col, ref bool Overlap, ref bool Delete)
        {
            // Decide if we should delete or keep the block
            //bool Delete = false;

            // MAKE SAVENOBLOCK a countdown
            if (bob.SaveNoBlock) Delete = true;
            if (bob.BottomCol && Col == ColType.Top) Delete = true;
            if (bob.TopCol && Col == ColType.Bottom) Delete = true;
            
            //if (Col == ColType.Top && bob.WantsToLand == false) Delete = true;

            Delete |= PostCollideDecision_Land(bob, ref Col, ref Overlap);
            Delete |= PostCollideDecision_Bottom(bob, ref Col, ref Overlap);
            Delete |= PostCollideDecision_Side(bob, ref Col, ref Overlap);
            
            // ???
            if (Overlap && Col == ColType.NoCol && !Box.TopOnly && !(this is NormalBlock && !BlockCore.NonTopUsed)) Delete = true;

            // Don't land on the very edge of the block
            EdgeSafety(bob, ref Delete);

            // Don't land on a block that says not to
            bool DesiresDeletion = false;
            if (Core.GenData.TemporaryNoLandZone ||
                !Core.GenData.Used && !((BlockBase)this).PermissionToUse())
                DesiresDeletion = Delete = true;

            if (Core.GenData.Used) Delete = false;
            if (!DesiresDeletion && Core.GenData.AlwaysLandOn && !Core.MarkedForDeletion && Col == ColType.Top) Delete = false;
            if (!DesiresDeletion && Core.GenData.AlwaysLandOn_Reluctantly && bob.WantsToLand_Reluctant && !Core.MarkedForDeletion && Col == ColType.Top) Delete = false;

            if (Overlap && Core.GenData.RemoveIfOverlap) Delete = true;
            if (!DesiresDeletion && Core.GenData.AlwaysUse && !Core.MarkedForDeletion) Delete = false;
        }

        private void EdgeSafety(Bob bob, ref bool Delete)
        {
            if (bob.MyPhsx is BobPhsxMeat) return;

            if (!Delete && !bob.MyPhsx.OnGround)
            {
                float Safety = BlockCore.GenData.EdgeSafety;
                if (bob.Box.BL.X > Box.TR.X - Safety ||
                    bob.Box.TR.X < Box.BL.X + Safety)
                {
                    Delete = true;
                }
            }
        }
        
        public virtual void PostKeep(Bob bob, ref ColType Col, ref bool Overlap) { }
        public virtual void PostInteractWith(Bob bob, ref ColType Col, ref bool Overlap) { }
    }

    /*
    public interface Block : ObjectBase
    {
        AABox Box { get; }
        bool IsActive { get; set; }
        BlockData BlockCore { get; }

        void Extend(Side side, float pos);

        void LandedOn(Bob bob);
        void HitHeadOn(Bob bob);
        void Smash(Bob bob);
        void SideHit(Bob bob);
        void Hit(Bob bob);

        bool PreDecision(Bob bob);

        bool PostCollidePreDecision(Bob bob);
        void PostCollideDecision(Bob bob, ref ColType Col, ref bool Overlap, ref bool Delete);
        void PostKeep(Bob bob, ref ColType Col, ref bool Overlap);
        void PostInteractWith(Bob bob);
    }*/
}