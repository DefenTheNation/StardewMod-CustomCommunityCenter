using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCommunityCenter
{
    public class CustomJunimoNoteMenu : IClickableMenu
    {
        public static int Width = 1280;
        public static int Height = 720;
        public static bool ShowCloseButton = true;
        public static string BundleTextureName = "LooseSprites\\JunimoNote";

        private Texture2D noteTexture;
        private InventoryMenu inventory;

        public CustomJunimoNoteMenu(BundleInfo info, bool fromGameMenu)
            : base(Game1.viewport.Width / 2 - 640, Game1.viewport.Height / 2 - 360, Width, Height, ShowCloseButton)
        {
            setupMenu();
        }

        private void setupMenu()
        {
            noteTexture = Game1.temporaryContent.Load<Texture2D>(BundleTextureName);
            inventory = new InventoryMenu(xPositionOnScreen + 128, yPositionOnScreen + 140, true, null, Utility.highlightSmallObjects, 36, 6, 8, 8, false)
            {
                capacity = 36
            };


        }
    }
}
