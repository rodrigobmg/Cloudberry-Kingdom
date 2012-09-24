using Microsoft.Xna.Framework;
using CloudberryKingdom.Stats;

namespace CloudberryKingdom
{
    public class StartMenu_MW_PressStart : StartMenuBase
    {
        public TitleGameData_MW Title;
        public StartMenu_MW_PressStart(TitleGameData_MW Title)
            : base()
        {
            this.Title = Title;
        }

        public override void SlideIn(int Frames)
        {
            Title.BackPanel.SetState(StartMenu_MW_Backpanel.State.Scene_Title);
            base.SlideIn(0);
        }

        public override void SlideOut(PresetPos Preset, int Frames)
        {
            base.SlideOut(Preset, 0);
        }

        public override void OnAdd()
        {
            base.OnAdd();
        }

        EzText Text;
        int DelayToAllowInput;
        public override void Init()
        {
 	        base.Init();

            DelayToAllowInput = 80;
            CallDelay = ReturnToCallerDelay = 0;

            MyPile = new DrawPile();

            EnsureFancy();

            Text = new EzText("Press any key to start", Tools.Font_Grobold42, true);
            Text.MyFloatColor = new Color(226, 10, 83).ToVector4();
            Text.OutlineColor = Color.Black.ToVector4();
            MyPile.Add(Text);

            SetPos();
        }
        
        void SetPos()
        {
            EzText _t;
            _t = MyPile.FindEzText(""); if (_t != null) { _t.Pos = new Vector2(8.332428f, -672.2224f); _t.Scale = 0.8212293f; }
            MyPile.Pos = new Vector2(0f, 0f);
        }

        protected override void MyPhsxStep()
        {
            base.MyPhsxStep(); if (!Active) return;

            if (DelayToAllowInput > 0)
            {
                DelayToAllowInput--;
                return;
            }

            if (ButtonCheck.AnyKey())
            {
                DelayToAllowInput = 10;

                //Tools.Nothing();
                Call(new StartMenu_MW(Title));
                Hide();
            }
        }

        float t = 0;
        protected override void MyDraw()
        {
            if (!Active) return;

            float s = Tools.Periodic(.85f, 1f, 3, Tools.t);
            Text.MyFloatColor = new Color((int)(226 * s), 10, 83).ToVector4();
            //Text.Pos = new Vector2(-922.2231f, -619.4446f + Tools.Periodic(0, 8, 3, Tools.t));
            Text.Scale = Tools.Periodic(.818668f, .838668f, 3, Tools.t);

            t += .01f;
            Text.Alpha = t;

            base.MyDraw();
        }

        public override void OnReturnTo()
        {
            base.OnReturnTo();
        }
    }
}