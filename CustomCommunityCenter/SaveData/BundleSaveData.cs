using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCommunityCenter.SaveData
{
    public class BundleSaveData
    {
        public string Name { get; set; }
        public bool Collected { get; set; }
        public List<IngredientSaveData> Ingredients { get; set; }
    }
}
