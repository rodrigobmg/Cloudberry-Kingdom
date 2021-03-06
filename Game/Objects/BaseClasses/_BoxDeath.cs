using Microsoft.Xna.Framework;
using CloudberryKingdom.Bobs;

namespace CloudberryKingdom
{
    public abstract class _BoxDeath : _Death
    {
        protected AABox Box;
        public Vector2 BoxSize;

        public override void MakeNew()
        {
            base.MakeNew();

            Box.Initialize(Vector2.Zero, Vector2.One);
        }

        public _BoxDeath() { }
        public _BoxDeath(bool BoxesOnly) { Construct(BoxesOnly); }

        public override void Construct(bool BoxesOnly)
        {
            Core.BoxesOnly = BoxesOnly;

            Box = new AABox();

            MakeNew();
        }

        public virtual Vector2 GetBoxPos()
        {
            return Pos;
        }

        public override void Init(Vector2 pos, Levels.Level level)
        {
            base.Init(pos, level);

            Box.Initialize(GetBoxPos(), BoxSize);

            Box.SetTarget(GetBoxPos(), BoxSize);
            Box.SwapToCurrent();
        }

        public virtual void Scale(float scale)
        {
            Box.Scale(scale);
        }

        protected void TargetPos()
        {
        }

        protected override void ActivePhsxStep()
        {
            Box.SetTarget(Pos, BoxSize);

            if (Core.WakeUpRequirements)
            {
                Box.SwapToCurrent();
                Core.WakeUpRequirements = false;
            }
        }

        public override void PhsxStep2()
        {
            if (Core.SkippedPhsx) return;

            Box.SwapToCurrent();
        }

        public override void OnMarkedForDeletion()
        {
            base.OnMarkedForDeletion();
        }

        protected override void DrawBoxes()
        {
            Box.Draw(new Color(50, 50, 255, 120), 5);
        }

        public override void Move(Vector2 shift)
        {
            base.Move(shift);

            Box.Move(shift);
        }

        public override void Interact(Bob bob)
        {
            if (!Core.SkippedPhsx)
            {
                bool Col = Phsx.BoxBoxOverlap(bob.Box2, Box);

                if (Col)
                {
                    if (Core.MyLevel.PlayMode == 0)
                        bob.Die(DeathType, this);

                    if (Core.MyLevel.PlayMode != 0)
                    {
                        bool col = Phsx.BoxBoxOverlap_Tiered(Box, Core, bob, AutoGenSingleton);

                        if (col)
                        {
                            //if ((Pos - bob.Pos).Length() > 2000) Tools.Write(0);
                            Core.Recycle.CollectObject(this);
                        }
                    }
                }
            }
        }

        public override void Clone(ObjectBase A)
        {
            Core.Clone(A.Core);

            Core.WakeUpRequirements = true;
        }
    }
}
