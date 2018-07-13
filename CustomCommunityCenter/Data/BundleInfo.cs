using StardewValley;
using StardewValley.Objects;
using System.Collections.Generic;
using System.Linq;

namespace CustomCommunityCenter.Data
{
    public class BundleInfo
    {
        public string Name { get; set; }
        public bool Collected { get; set; }
        public int RewardItemType { get; set; }
        public int RewardItemId { get; set; }
        public int RewardItemStack { get; set; }
        public int IngredientsRequired { get; set; }
        public bool IsPurchase { get; set; }
        public List<BundleIngredientInfo> Ingredients { get; set; }

        public int PurchaseAmount
        {
            get { return Ingredients.Count == 1 && Ingredients[0].ItemId == (int)Objects.Money ? Ingredients[0].RequiredStack : 0; }
        }

        public bool Completed
        {
            get { return IngredientsRequired == Ingredients.Count(x => x.Completed); }
        }

        public Item ClaimReward()
        {
            return ObjectFactory.getItemFromDescription((byte)RewardItemType, RewardItemId, RewardItemStack);
        }
        
        public void PurchaseCompleted()
        {
            if (IsPurchase && Ingredients.Count == 1 && Ingredients[0].ItemId == (int)Objects.Money)
                Ingredients[0].Completed = true;
        }
    }
}
