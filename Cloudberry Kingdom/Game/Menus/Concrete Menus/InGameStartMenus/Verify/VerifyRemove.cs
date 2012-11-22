using Microsoft.Xna.Framework;

namespace CloudberryKingdom
{
    public class VerifyRemoveMenu : VerifyBaseMenu
    {
        public VerifyRemoveMenu(int Control) : base(Control) { }

        public override void Init()
        {
            base.Init();

            // Make the menu
            MenuItem item;

            // Header
            EzText HeaderText = new EzText(Localization.WordString(Localization.Words.RemovePlayerQuestion) + "?", ItemFont, false);
            SetHeaderProperties(HeaderText);
            MyPile.Add(HeaderText);
            HeaderText.Name = "Header";
            HeaderText.Pos = HeaderPos;

            string PlayerName = PlayerManager.Get(Control).GetName();
            var PlayerText = new EzText(PlayerName, ItemFont, false);
            SetHeaderProperties(PlayerText);
            PlayerText.Name = "PlayerText";
            PlayerManager.Get(Control).SetNameText(PlayerText);
            MyPile.Add(PlayerText);

            // Yes
            item = new MenuItem(new EzText(Localization.Words.Yes, ItemFont));
            item.Go = _Yes;
            item.Name = "Yes";
            AddItem(item);
            item.SelectSound = null;

            // No
            item = new MenuItem(new EzText(Localization.Words.No, ItemFont));
            item.Go = Cast.ToItem(ReturnToCaller);
            AddItem(item);
            item.Name = "No";
            item.SelectSound = null;

            MyMenu.OnX = MyMenu.OnB = MenuReturnToCaller;

            // Select the first item in the menu to start
            MyMenu.SelectItem(0);

            SetPos();
        }

        void SetPos()
        {
            // Set Positions
            MenuItem _item;
            _item = MyMenu.FindItemByName("Yes"); if (_item != null) { _item.SetPos = new Vector2(800f, 361f); _item.MyText.Scale = 0.8f; _item.MySelectedText.Scale = 0.8f; _item.SelectIconOffset = new Vector2(0f, 0f); }
            _item = MyMenu.FindItemByName("No"); if (_item != null) { _item.SetPos = new Vector2(800f, 61f); _item.MyText.Scale = 0.8f; _item.MySelectedText.Scale = 0.8f; _item.SelectIconOffset = new Vector2(0f, 0f); }

            MyMenu.Pos = new Vector2(-1027.779f, -408.3333f);

            EzText _t;
            _t = MyPile.FindEzText("Header"); if (_t != null) { _t.Pos = new Vector2(280.5555f, 560.778f); _t.Scale = 0.96f; }
            _t = MyPile.FindEzText("PlayerText"); if (_t != null) { _t.Pos = new Vector2(283.333f, 872.2222f); _t.Scale = 0.96f; }

            QuadClass _q;
            _q = MyPile.FindQuad("Backdrop"); if (_q != null) { _q.Pos = new Vector2(1181.251f, 241.6668f); _q.Size = new Vector2(1287.75f, 890.8389f); }

            MyPile.Pos = new Vector2(-1141.667f, -258.3334f);
        }

        public static bool YesChosen = false;

        private void _Yes(MenuItem _item)
        {
            if (PlayerManager.GetNumPlayers() > 1)
            {
                if (Control >= 0)
                    Tools.CurGameData.RemovePlayer(Control);
            }

            YesChosen = true;

            ReturnToCaller();
        }
    }
}