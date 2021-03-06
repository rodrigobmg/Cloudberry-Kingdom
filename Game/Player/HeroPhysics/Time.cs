﻿using Microsoft.Xna.Framework;
using CloudberryKingdom.Levels;

namespace CloudberryKingdom
{
    public class BobPhsxTime : BobPhsxNormal
    {
        // Singleton
        protected override void InitSingleton()
        {
            base.InitSingleton();

            Specification = new HeroSpec(0, 0, 0, 1);
            Name = Localization.Words.TimeMaster;
            Adjective = "Chrono";
            Icon = new PictureIcon(Tools.TextureWad.FindByName("HeroIcon_Classic"), Color.White, 1.2f * DefaultIconWidth);
        }
        static readonly BobPhsxTime instance = new BobPhsxTime();
        public static new BobPhsxTime Instance { get { return instance; } }

        // Instancable class
        public BobPhsxTime()
        {
            Set(this);
        }

        public override void Set(BobPhsx phsx)
        {
            Set(phsx, Vector2.One);
        }
        public void Set(BobPhsx phsx, Vector2 modsize)
        {
            phsx.UseGroundSpeed = false;

            BobPhsxNormal normal = phsx as BobPhsxNormal;
            if (null != normal)
            {
            }
        }

        public override void Init(Bobs.Bob bob)
        {
            base.Init(bob);
        }

        public override void PhsxStep()
        {
            base.PhsxStep();
        }

        public override void LandOnSomething(bool MakeReadyToJump, ObjectBase ThingLandedOn)
        {
            base.LandOnSomething(MakeReadyToJump, ThingLandedOn);
        }

        enum Behavior { Pause, Regular };
        Behavior CurBehavior = Behavior.Pause;
        int BehaviorLength;
        public override void GenerateInput(int CurPhsxStep)
        {
            base.GenerateInput(CurPhsxStep);

            // Change behavior
            if (CurPhsxStep < 10)
            {
                CurBehavior = Behavior.Pause;
                BehaviorLength = 0;
            }
            else
            {
                if (BehaviorLength == 0)
                {
                    if (MyLevel.Rnd.RndFloat() > .7f)
                    {
                        CurBehavior = Behavior.Pause;
                        BehaviorLength = MyLevel.Rnd.RndInt(5, 10);
                        //BehaviorLength = MyLevel.Rnd.RndInt(5, 40);
                    }
                    else
                    {
                        CurBehavior = Behavior.Regular;
                        BehaviorLength = MyLevel.Rnd.RndInt(25, 60);
                    }
                }
                else
                    BehaviorLength--;
            }

            // Act according to behavior
            switch (CurBehavior)
            {
                case Behavior.Pause:
                    MyBob.CurInput.xVec.X = 0;
                    break;
                case Behavior.Regular:
                    break;
            }
        }

        public override void ModData(ref Level.MakeData makeData, StyleData Style)
        {
            base.ModData(ref makeData, Style);

            Style.TimeType = Level.TimeTypes.xSync;
        }
    }
}