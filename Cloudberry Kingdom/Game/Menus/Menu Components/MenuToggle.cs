﻿using System;

namespace CloudberryKingdom
{
    public class MenuToggle : MenuItem
    {
        public MenuToggle(EzFont Font)
        {
            EzText text = new EzText("xxxxx", Font);
            base.Init(text, text.Clone());
        }

        public string PrefixText = "";

        bool MyState = false;
        public void Toggle(bool state)
        {
            MyState = state;

            if (state)
            {
                MyText.SubstituteText(PrefixText + "On");
                MySelectedText.SubstituteText(PrefixText + "On");
            }
            else
            {
                MyText.SubstituteText(PrefixText + "Off");
                MySelectedText.SubstituteText(PrefixText + "Off");
            }
        }

        public Action<bool> OnToggle;
        public override void PhsxStep(bool Selected)
        {
            base.PhsxStep(Selected);

            if (!Selected) return;

            if (ButtonCheck.State(ControllerButtons.A, Control).Pressed)
            {
                Toggle(!MyState);

                if (OnToggle != null)
                    OnToggle(MyState);
            }
        }
    }
}