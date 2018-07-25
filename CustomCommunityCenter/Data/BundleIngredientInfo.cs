namespace CustomCommunityCenter.Data
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

        public bool WillCompleteIngredient(StardewValley.Object o)
        {
            if(!Completed && ItemId == o.ParentSheetIndex && ItemQuality <= o.Quality && RequiredStack <= o.Stack)
            {
                return true;
            }

            return false;
        }

        public bool TryCompleteIngredient(StardewValley.Object o)
        {
            if (WillCompleteIngredient(o))
            {
                o.Stack -= RequiredStack;

                return true;
            }
            else return false;
        }
    }
}
