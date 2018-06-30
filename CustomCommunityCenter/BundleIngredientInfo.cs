namespace CustomCommunityCenter
{
    public class BundleIngredientInfo
    {
        public BundleIngredientInfo()
        {

        }

        public BundleIngredientInfo(int itemId, int itemQuality, int itemStack)
        {
            ItemType = 0;
            ItemId = itemId;
            ItemQuality = itemQuality;
            RequiredStack = itemStack;
        }

        public int ItemType { get; set; }
        public int ItemId { get; set; }
        public int ItemQuality { get; set; }
        public int RequiredStack { get; set; }
        public bool Completed { get; set; }

        public bool BundleCompleted(StardewValley.Object o)
        {
            if(!Completed && ItemId == o.ParentSheetIndex && ItemQuality <= o.Quality && RequiredStack <= o.Stack)
            {
                Completed = true;
                return true;
            }

            return false;
        }
    }
}
