using Microsoft.Xna.Framework;

namespace CloudberryKingdom
{
    public class VerifyQuitLevelMenu : VerifyBaseMenu
    {
        public VerifyQuitLevelMenu(bool CallBaseConstructor) : base(CallBaseConstructor)
        {
            EnableBounce();
        }
        public VerifyQuitLevelMenu(int Control) : base(Control, true)
        {
            //EnableBounce();
        }

        public override void Init()
        {
            base.Init();

            // Make the menu
            MenuItem item;

            // Header
            EzText HeaderText = new EzText(Localization.Words.ExitLevelQuestion, ItemFont, true);
            SetHeaderProperties(HeaderText);
            HeaderText.Name = "Header";
            MyPile.Add(HeaderText);

            // Ok
            item = new MenuItem(new EzText(Localization.Words.Yes, ItemFont, true));
            item.Go = _item =>
                {
                    Tools.CurrentAftermath = new AftermathData();
                    Tools.CurrentAftermath.Success = false;
                    Tools.CurrentAftermath.EarlyExit = true;

                    Tools.CurGameData.EndGame(false);
                };
            item.Name = "Yes";
            AddItem(item);

            // No
            item = new MenuItem(new EzText(Localization.Words.No, ItemFont, true));
            item.Go = Cast.ToItem(ReturnToCaller);
            item.Name = "No";
            AddItem(item);

            MyMenu.OnX = MyMenu.OnB = MenuReturnToCaller;

            // Select the first item in the menu to start
            MyMenu.SelectItem(0);

            SetPos();
        }

        void SetPos()
        {
            MenuItem _item;
            _item = MyMenu.FindItemByName("Yes"); if (_item != null) { _item.SetPos = new Vector2(0f, 380.4445f); _item.MyText.Scale = 0.8f; _item.MySelectedText.Scale = 0.8f; _item.SelectIconOffset = new Vector2(0f, 0f); }
            _item = MyMenu.FindItemByName("No"); if (_item != null) { _item.SetPos = new Vector2(0f, 94.33334f); _item.MyText.Scale = 0.8f; _item.MySelectedText.Scale = 0.8f; _item.SelectIconOffset = new Vector2(0f, 0f); }

            MyMenu.Pos = new Vector2(0f, -302.7777f);

            EzText _t;
            _t = MyPile.FindEzText("Header"); if (_t != null) { _t.Pos = new Vector2(0f, 777.4443f); _t.Scale = 0.96f; }

            QuadClass _q;
            _q = MyPile.FindQuad("Backdrop"); if (_q != null) { _q.Pos = new Vector2(0f, 316.6668f); _q.Size = new Vector2(1281.083f, 729.4391f); }

            MyPile.Pos = new Vector2(0f, -319.4444f);
        }
    }
}