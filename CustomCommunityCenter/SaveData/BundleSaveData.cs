using System.Collections.Generic;

namespace CustomCommunityCenter.SaveData
{
    public class BundleSaveData
    {
        public string Name { get; set; }
        public bool Collected { get; set; }
        public List<IngredientSaveData> Ingredients { get; set; }
    }
}
