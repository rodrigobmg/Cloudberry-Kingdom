namespace CloudberryKingdom
{
    class CampaignChapterItem : MenuItem
    {
        public int Chapter = 0;
        public bool Locked = false;

        public CampaignChapterItem(Text Text, int Chapter)
            : base(Text)
        {
            this.Chapter = Chapter;

            UpdateLock();
        }

        public void UpdateLock()
        {
            Locked = false;
            if (!CloudberryKingdomGame.Unlock_Levels)
            {
				int level = PlayerManager.MinPlayerTotalCampaignLevel();
				if (CampaignSequence.Instance.ChapterEnd.ContainsKey(Chapter - 1))
					Locked = level < CampaignSequence.Instance.ChapterEnd[Chapter - 1];
            }
        }
    }
}