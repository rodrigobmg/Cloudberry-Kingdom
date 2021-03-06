﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;

using CoreEngine;

using CloudberryKingdom.Levels;

namespace CloudberryKingdom
{
    public class ObjectIcon : ViewReadWrite
    {
        public override string[] GetViewables()
        {
            return new string[] { };
        }

        public bool Flipped = false;

        public static ObjectIcon RobotIcon, PathIcon, SlowMoIcon;
        public static ObjectIcon CheckpointIcon, RandomIcon, CustomIcon, CustomHoverIcon;

        public static Dictionary<Upgrade, ObjectIcon> UpgradeIcons;
        public static Dictionary<ObjectType, ObjectIcon> ObjIcons;

        public static ObjectIcon CreateIcon(Upgrade upgrade)
        {
            return CreateIcon(upgrade, false);
        }
        public static ObjectIcon CreateIcon(Upgrade upgrade, bool big)
        {
            if (UpgradeIcons.ContainsKey(upgrade))
                return UpgradeIcons[upgrade].Clone();
            else
            {
                var info = TileSet.UpgradeToInfo(upgrade, "castle");
                
                PictureIcon icon;
                if (big && info.Icon_Big != null)
                    icon = new PictureIcon(info.Icon_Big);
                else
                    icon = new PictureIcon(info.Icon);
                
                icon.DisplayText = UpgradeName(upgrade);
                return icon;
            }
        }

        public static Localization.Words UpgradeName(Upgrade upgrade)
        {
            switch (upgrade)
            {
                case Upgrade.BouncyBlock: return Localization.Words.BouncyBlocks;
                case Upgrade.Cloud: return Localization.Words.Clouds;
                case Upgrade.Elevator: return Localization.Words.Elevators;
                case Upgrade.FallingBlock: return Localization.Words.FallingBlocks;
                case Upgrade.FireSpinner: return Localization.Words.Firespinners;
                case Upgrade.SpikeyGuy: return Localization.Words.Boulders;
                case Upgrade.Pinky: return Localization.Words.SpikeyGuys;
                case Upgrade.FlyBlob: return Localization.Words.FlyingBlobs;
                case Upgrade.GhostBlock: return Localization.Words.GhostBlocks;
                case Upgrade.Laser: return Localization.Words.Lasers;
                case Upgrade.MovingBlock: return Localization.Words.MovingBlocks;
                case Upgrade.Spike: return Localization.Words.Spikes;
                case Upgrade.Fireball: return Localization.Words.Fireballs;
                case Upgrade.Firesnake: return Localization.Words.None;
                case Upgrade.SpikeyLine: return Localization.Words.SpikeyLines;
                case Upgrade.Serpent: return Localization.Words.Serpent;
                case Upgrade.LavaDrip: return Localization.Words.Sludge;
                case Upgrade.Pendulum: return Localization.Words.Pendulums;

                default: return Localization.Words.None;
            }
        }

        public static ObjectIcon CreateIcon(ObjectType obj)
        {
            switch (obj)
            {
                case ObjectType.FallingBlock: return CreateIcon(Upgrade.FallingBlock);
                case ObjectType.MovingBlock: return CreateIcon(Upgrade.MovingBlock);
                case ObjectType.GhostBlock: return CreateIcon(Upgrade.GhostBlock);
                case ObjectType.FlyingBlob: return CreateIcon(Upgrade.FlyBlob);
                case ObjectType.BouncyBlock: return CreateIcon(Upgrade.BouncyBlock);
            }

            return null;
            //return ObjIcons[obj].Clone();
        }

        public static void InitIcons()
        {
            UpgradeIcons = new Dictionary<Upgrade, ObjectIcon>();

            float StandardWidth = 161 * 1.31f;
            //UpgradeIcons.Add(Upgrade.BouncyBlock, new PictureIcon("Bouncy blocks", "Icon_BouncyBlock1", Color.Lime, StandardWidth*.555f));
            //UpgradeIcons.Add(Upgrade.Cloud, new PictureIcon("Clouds", "Icon_Cloud3", Color.LightGray, StandardWidth*1.135f));
            //UpgradeIcons.Add(Upgrade.Elevator, new PictureIcon("Elevators", "Icon_Palette", Color.LightBlue, StandardWidth*.810f));
            //UpgradeIcons.Add(Upgrade.FallingBlock, new PictureIcon("Falling blocks", "Icon_FallingBlock1", Color.Red, StandardWidth*.555f));
            //UpgradeIcons.Add(Upgrade.FireSpinner, new PictureIcon("Fire spinners", "Icon_FireSpinner2", Color.Orange, StandardWidth*1.022f));
            //UpgradeIcons.Add(Upgrade.SpikeyGuy, new PictureIcon("Spikey guys", "Icon_Spikey", Color.LightGray, StandardWidth*.835f));
            //UpgradeIcons.Add(Upgrade.Pinky, new PictureIcon("Pinkies", "Pinky", Color.LightGray, StandardWidth*.835f));
            //UpgradeIcons.Add(Upgrade.FlyBlob, new PictureIcon("Flying blobs", "Icon_Blob", Color.Lime, StandardWidth*1.056f, new Vector2(0, -45)));
            //UpgradeIcons.Add(Upgrade.GhostBlock, new PictureIcon("Ghost blocks", "Icon_Ghost", Color.Lime, StandardWidth*1.148f, new Vector2(0, -80)));
            //UpgradeIcons.Add(Upgrade.Laser, new PictureIcon("Lasers", "Icon_Laser2", Color.Red, StandardWidth*.72f));
            //UpgradeIcons.Add(Upgrade.MovingBlock, new PictureIcon("Moving blocks", "Blue_Small", Color.LightBlue, StandardWidth*.62f));
            //UpgradeIcons.Add(Upgrade.Spike, new PictureIcon("Spikes", "Icon_Spike2", Color.LightGray, StandardWidth*.99f));
            //UpgradeIcons.Add(Upgrade.Fireball, new PictureIcon("Fireballs", "Icon_Fireball", Color.Orange, StandardWidth*.905f));
            //UpgradeIcons.Add(Upgrade.Firesnake, new PictureIcon("Firesnake", "Icon_Firesnake", Color.Orange, StandardWidth * .905f));
            //UpgradeIcons.Add(Upgrade.SpikeyLine, new PictureIcon("Spikey line", "Icon_SpikeyLine", Color.Orange, StandardWidth * .905f));

            UpgradeIcons.Add(Upgrade.Jump, new PictureIcon(Localization.Words.JumpDifficulty, "Jump", Color.Orange, StandardWidth * 1.07f));
            UpgradeIcons.Add(Upgrade.Speed, new PictureIcon(Localization.Words.LevelSpeed, "SpeedIcon", Color.Orange, StandardWidth * 1.036f));
            UpgradeIcons.Add(Upgrade.Ceiling, new PictureIcon(Localization.Words.Ceilings, "CeilingIcon", Color.Orange, StandardWidth * .9f));

            ObjIcons = new Dictionary<ObjectType,ObjectIcon>();
            //ObjIcons.Add(ObjectType.FallingBlock, UpgradeIcons[Upgrade.FallingBlock]);
            //ObjIcons.Add(ObjectType.MovingBlock, UpgradeIcons[Upgrade.MovingBlock]);
            //ObjIcons.Add(ObjectType.GhostBlock, UpgradeIcons[Upgrade.GhostBlock]);
            //ObjIcons.Add(ObjectType.FlyingBlob, UpgradeIcons[Upgrade.FlyBlob]);
            //ObjIcons.Add(ObjectType.BouncyBlock, UpgradeIcons[Upgrade.BouncyBlock]);

            //CheckIcon = new PictureIcon("Check", Color.Lime, StandardWidth * .85f);
            //UncheckedIcon = new PictureIcon("Uncheck", Color.Lime, StandardWidth * .85f);
            
            //CheckpointIcon = new PictureIcon("Icon_Checkpoint", Color.Lime, StandardWidth * .85f);
            CheckpointIcon = new PictureIcon("Icon_Checkpoint_v2", Color.Lime, StandardWidth * .85f);
            //RandomIcon = new PictureIcon("Unknown", Color.Lime, StandardWidth * 1.2f);
            RandomIcon = new PictureIcon("HeroIcon_Random", Color.Lime, StandardWidth * 1.08f);
            CustomIcon = new PictureIcon("HeroIcon_Custom", Color.Lime, StandardWidth * 1.45f);

            //RobotIcon = new PictureIcon("Robot", Color.Lime, StandardWidth * .75f);
            //PathIcon = new PictureIcon("Path", Color.Lime, StandardWidth * .75f);
            //SlowMoIcon = new PictureIcon("SlowMo", Color.Lime, StandardWidth * .75f);
            RobotIcon = new PictureIcon("Powerup_Computer", Color.Lime, StandardWidth * .75f);
            PathIcon = new PictureIcon("Powerup_Path", Color.Lime, StandardWidth * .75f);
            SlowMoIcon = new PictureIcon("Powerup_SlowMo", Color.Lime, StandardWidth * .75f);
        }

        public QuadClass Backdrop;
        public Color BarColor;

        public Localization.Words DisplayText;

        public FancyVector2 FancyPos = new FancyVector2();

        public Vector2 Pos { get { return FancyPos.RelVal; } set { FancyPos.RelVal = value; } }

        public OscillateParams MyOscillateParams;
        public ObjectIcon()
        {
            MyOscillateParams.Set(2f, 1.02f, .215f);

            Backdrop = new QuadClass(null, true);
            Backdrop.SetToDefault();
            Backdrop.TextureName = "Icon_Backdrop";
            Backdrop.ScaleYToMatchRatio(210);
        }

        public virtual void SetShadow(Color color)
        {
        }

        public virtual void SetShadow(bool Shadow)
        {
        }

        public virtual void Fade(bool fade)
        {
        }

        public enum IconScale { Widget, Full, NearlyFull };
        public virtual ObjectIcon Clone(IconScale ScaleType)
        {
            ObjectIcon icon = new ObjectIcon();

            icon.DisplayText = DisplayText;

            return icon;
        }

        public virtual ObjectIcon Clone()
        {
            return Clone(IconScale.Full);
        }

        public float PrevSetRatio = 1;
        public virtual void SetScale(float Ratio)
        {
            PrevSetRatio = Ratio;
        }

        public virtual void Draw(bool Selected)
        {
            FancyPos.Update();
            Backdrop.Pos = FancyPos.AbsVal;
            //Backdrop.Draw();
        }

#if WINDOWS
        public virtual bool HitTest(Vector2 pos)
        {
            return false;
        }
#endif
    }

    public class PictureIcon : ObjectIcon
    {
        public override string[] GetViewables()
        {
            return new string[] { };
        }

        public QuadClass IconQuad;

        public CoreTexture IconTexture;
        public float NormalWidth;

        public PictureIcon(SpriteInfo info)
        {
            IconQuad = new QuadClass(FancyPos, true);
            IconQuad.Set(info);

            IconQuad.Quad.Playing = false;

            if (IconQuad.Quad.TextureAnim == null)
                IconTexture = IconQuad.Quad.MyTexture;
            else
                IconTexture = IconQuad.Quad.TextureAnim.Anims[0].Data[0];

            this.DisplayText = Localization.Words.None;
            this.NormalWidth = 161 * 1.31f * info.Size.X / 62f;
        }

        public PictureIcon(Localization.Words DisplayText, string IconTextureString, Color BarColor, float Width)
        {
            this.DisplayText = DisplayText;
            Init(Tools.TextureWad.FindByName(IconTextureString), BarColor, Width);
        }
        public PictureIcon(Localization.Words DisplayText, string IconTextureString, Color BarColor, float Width, Vector2 HitPadding)
        {
            this.DisplayText = DisplayText;
            this.HitPadding = HitPadding;
            Init(Tools.TextureWad.FindByName(IconTextureString), BarColor, Width);
        }
        public PictureIcon(string IconTextureString, Color BarColor, float Width)
        {
            Init(Tools.TextureWad.FindByName(IconTextureString), BarColor, Width);
        }
        public PictureIcon(CoreTexture IconTexture, Color BarColor, float Width)
        {
            Init(IconTexture, BarColor, Width);
        }

        void Init(CoreTexture IconTexture, Color BarColor, float Width)
        {
            this.IconTexture = IconTexture;
            this.BarColor = BarColor;
            this.NormalWidth = Width;

            IconQuad = new QuadClass(FancyPos, true);
            IconQuad.SetToDefault();
            IconQuad.Quad.MyTexture = IconTexture;
            IconQuad.ScaleYToMatchRatio(Width);

            IconQuad.Shadow = true;
            IconQuad.ShadowColor = new Color(.2f, .2f, .2f, 1f);
            IconQuad.ShadowOffset = new Vector2(12, 12);
        }

        public override void SetShadow(Color color)
        {
            base.SetShadow(color);

            IconQuad.ShadowColor = color;
        }

        public override void SetShadow(bool Shadow)
        {
            base.SetShadow(Shadow);

            IconQuad.Shadow = Shadow;
        }

        public override void Fade(bool fade)
        {
            base.Fade(fade);

            if (fade)
                IconQuad.Quad.SetColor(new Color(100, 100, 100));
            else
                IconQuad.Quad.SetColor(Color.White);
        }

        public override ObjectIcon Clone(IconScale ScaleType)
        {
            float width = NormalWidth;
            if (ScaleType == IconScale.Widget)
                width *= .3f;
            if (ScaleType == IconScale.NearlyFull)
                width *= .9f;

            PictureIcon icon = new PictureIcon(IconTexture, BarColor, width);
            icon.DisplayText = DisplayText;
            icon.IconQuad.Quad.v0 = IconQuad.Quad.v0;
            icon.IconQuad.Quad.v1 = IconQuad.Quad.v1;
            icon.IconQuad.Quad.v2 = IconQuad.Quad.v2;
            icon.IconQuad.Quad.v3 = IconQuad.Quad.v3;

            icon.HitPadding = HitPadding;

            return (ObjectIcon)icon;
        }

        public override void SetScale(float Ratio)
        {
            base.SetScale(Ratio);

            IconQuad.Scale(Ratio * NormalWidth / IconQuad.Size.X);
        }

        public override void Draw(bool Selected)
        {
            base.Draw(Selected);

            if (Selected)
            {
                Vector2 HoldSize = IconQuad.Size;
                IconQuad.Scale(MyOscillateParams.GetScale());
                //IconQuad.Scale(Oscillate.GetScale(SelectCount, 2f, 1.02f, .215f));
                IconQuad.Draw();
                IconQuad.Size = HoldSize;
            }
            else
            {
                // Flip if level is flipped
                if (Flipped)
                {
                    if (IconQuad.Base.e1.X > 0)
                        IconQuad.SizeX *= -1;
                }

                IconQuad.Draw();
                MyOscillateParams.Reset();
            }
        }

        public Vector2 HitPadding;
#if WINDOWS
        public override bool HitTest(Vector2 pos)
        {
            return IconQuad.HitTest(pos, HitPadding) ||
                base.HitTest(pos);
        }
#endif
    }

    public class CustomHoverIcon : ObjectIcon
    {
        public override string[] GetViewables()
        {
            return new string[] { };
        }

        public QuadClass GearQuad, YQuad;

        public CustomHoverIcon()
        {
            YQuad = new QuadClass(FancyPos, true);
            YQuad.SetToDefault();
            YQuad.TextureName = "Xbox_Y";
            YQuad.ScaleYToMatchRatio(60);
            YQuad.Pos = new Vector2(60f, 0f);

            GearQuad = new QuadClass(FancyPos, true);
            GearQuad.SetToDefault();
            GearQuad.TextureName = "Gears";
            GearQuad.ScaleYToMatchRatio(82);
            GearQuad.Pos = new Vector2(-60.55469f, -16.66663f);
        }

        public override void Draw(bool Selected)
        {
            base.Draw(Selected);

            YQuad.Draw();
            GearQuad.Draw();            
        }
    }
}