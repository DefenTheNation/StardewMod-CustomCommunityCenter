using StardewValley;
using StardewValley.Objects;
using System.Collections.Generic;
using System.Linq;

namespace CustomCommunityCenter
{
    public class BundleInfo
    {
        public string Name { get; set; }
        public bool Collected { get; set; }
        public int RewardItemType { get; set; }
        public int RewardItemId { get; set; }
        public int RewardItemStack { get; set; }
        public int IngredientsRequired { get; set; }
        public List<BundleIngredientInfo> Ingredients { get; set; }

        public bool Completed
        {
            get { return IngredientsRequired == Ingredients.Count(x => x.Completed); }
        }

        public Item ClaimReward()
        {
            return ObjectFactory.getItemFromDescription((byte)RewardItemType, RewardItemId, RewardItemStack);
        }        
    }
}
