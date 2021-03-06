namespace CloudberryKingdom
{
    public class GUI_Timer : GUI_Timer_Base
    {
        public override void OnAdd()
        {
            base.OnAdd();
            
            MyGame.OnCoinGrab += OnCoinGrab;
            MyGame.OnCompleteLevel += OnCompleteLevel;
        }

        protected override void ReleaseBody()
        {
            MyGame.OnCoinGrab -= OnCoinGrab; 
            
            base.ReleaseBody();            
        }

        public int CoinTimeValue = 60, MinLevelStartTimeValue = 62 + 31;
        public int MaxTime = 60;
        public void OnCoinGrab(ObjectBase obj)
        {
            Time += CoinTimeValue;

            if (Time > MaxTime)
                Time = MaxTime;
        }

        public void OnCompleteLevel(Levels.Level level)
        {
            MinLevelStartTimeValue = 124;
            Time = CoreMath.Restrict(MinLevelStartTimeValue, MaxTime, Time);
        }
    }
}