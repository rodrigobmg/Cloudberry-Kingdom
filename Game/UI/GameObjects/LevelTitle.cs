using Microsoft.Xna.Framework;

namespace CloudberryKingdom
{
    public class LevelTitle : GUI_Panel
    {
        public override void OnAdd()
        {
            base.OnAdd();

            //Vector2 shift = new Vector2(0, -.5f * Core.MyLevel.MainCamera.GetHeight() + 380);
            Vector2 shift = new Vector2(0, -.5f * 2000 + 380);

            // Add the text
            text.Pos = shift;
            MyPile.Add(text);

            // Scale to fit
            Vector2 size = text.GetWorldSize();
            float max = MyGame.Cam.GetWidth() - 400;
            if (size.X > max)
                text.Scale *= max / size.X;

            // Slide out
            this.SlideOut(PresetPos.Bottom, 0);

            if (Perma)
                this.SlideIn(0);
        }

        public Text text;
        public LevelTitle(string str) { Init(str, Vector2.Zero, 1f, false); }
        public LevelTitle(string str, Vector2 shift, float scale, bool perma) { Init(str, shift, scale, perma); }

        public static LevelTitle HeroTitle(string str)
        {
            var title = new LevelTitle(str, new Vector2(150, -130), 1f, false);
            title.SlideInLength = 55;

            return title;
        }

        bool Perma;
        void Init(string str, Vector2 shift, float scale, bool perma)
        {
            SlideInLength = 84;

            this.Perma = perma;

            //Core.DrawLayer++; // Draw above cheering berries
            PauseOnPause = true;

            MyPile = new DrawPile();
            EnsureFancy();
            MyPile.Pos += shift;

            Tools.Warning(); // May be text, rather than Localization.Words
            text = new Text(str, Resources.Font_Grobold42, true, true);
            text.Scale *= scale;

            text.MyFloatColor = new Color(26, 188, 241).ToVector4();
            text.OutlineColor = new Color(255, 255, 255).ToVector4();

            text.Shadow = true;
            text.ShadowOffset = new Vector2(10.5f, 10.5f);
            text.ShadowColor = new Color(30, 30, 30);
        }

        int Count = 0;
        protected override void MyPhsxStep()
        {
            base.MyPhsxStep();

            Count++;

            // Make sure we're on top
            if (!Core.Released && Core.MyLevel != null)
                Core.MyLevel.MoveToTopOfDrawLayer(this);

            // Do nothing if this is permanent
            if (Perma) return;

            // Otherwise show and hide
            if (Count == 4)
                SlideIn();

            if (Count == 180)
            {
                SlideOut(PresetPos.Bottom, 160);
                ReleaseWhenDone = true;
            }
        }
    }
}