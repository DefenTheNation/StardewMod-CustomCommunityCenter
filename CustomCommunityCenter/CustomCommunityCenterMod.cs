using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCommunityCenter
{
    public class CustomCommunityCenterMod : Mod
    {
        protected string bundleTextureName = "LooseSprites\\JunimoNote";

        public override void Entry(IModHelper helper)
        {
            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
        }

        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            int bundleGroupIndex = 0;
            int bundleIndex = 1;
            string rawBundleInfo = "";
            bool[] completedIngredientList = { false, false, false, false };

            CustomJunimoNoteMenu noteMenu = new CustomJunimoNoteMenu(null, false);

            Point displayPoint = new Point(noteMenu.xPositionOnScreen, noteMenu.yPositionOnScreen);
            displayPoint.X += 392;
            displayPoint.Y += 384;

            Bundle b = new Bundle(bundleIndex, rawBundleInfo, completedIngredientList, displayPoint, bundleTextureName, null);
        }

        protected void removeAndReplaceLocation(GameLocation toRemove, GameLocation toReplace)
        {
            Game1.locations.Remove(toRemove);
            Game1.locations.Add(toReplace);
        }
    }
}
