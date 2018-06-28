using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace CustomCommunityCenter
{
    public class DisplayBundle : ClickableComponent
    {
        private const int SquareWidth = 64;

        public const int Color_Green = 0;

        public const int Color_Purple = 1;

        public const int Color_Orange = 2;

        public const int Color_Yellow = 3;

        public const int Color_Red = 4;

        public const int Color_Blue = 5;

        public const int Color_Teal = 6;

        private int bundleColor;
        private int completionTimer;
        private bool isCompleted;
        private bool depositsAllowed = true;
        private int ingredientSlots;

        protected TemporaryAnimatedSprite sprite;
        protected BundleInfo bundleInfo;

        public DisplayBundle(BundleInfo info, Point position) : base(new Rectangle(position.X, position.Y, SquareWidth, SquareWidth), "")
        {
            bundleInfo = info;
            isCompleted = bundleInfo.Completed;
            ingredientSlots = bundleInfo.Requirements.Count;

            bundleColor = Color_Yellow;
            string textureName = "";

            sprite = new TemporaryAnimatedSprite(textureName, new Rectangle(bundleColor * 256 % 512, 244 + bundleColor * 256 / 512 * 16, 16, 16), 70f, 3, 99999, new Vector2((float)base.bounds.X, (float)base.bounds.Y), false, false, 0.8f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
            {
                pingPong = true
            };
            sprite.paused = false;
            sprite.sourceRect.X += sprite.sourceRect.Width;
            if (name.ToLower().Contains(Game1.currentSeason) && !isCompleted)
            {
                //shake(0.07363108f);
            }
            if (isCompleted)
            {
                //completionAnimation(false, 0);
            }
        }

        public Item tryToDepositThisItem(Item item, ClickableTextureComponent slot, string noteTextureName)
        {
            if (!depositsAllowed)
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:JunimoNote_MustBeAtCC"));
                return item;
            }
            else if (item is StardewValley.Object o)
            {
                foreach(var bundleItem in bundleInfo.Requirements)
                {
                    if(bundleItem.BundleCompleted(o))
                    {
                        item.Stack -= bundleItem.RequiredStack;
                        slot.item = ObjectFactory.getItemFromDescription(0, bundleItem.ItemId, bundleItem.RequiredStack);
                        Game1.playSound("newArtifact");

                        if (item.Stack <= 0) return null;
                        else return item;                              
                    }
                }
            }

            return item;
        }

        public bool CanBeClicked()
        {
            return !isCompleted;
        }

        //public void completionAnimation(bool playSound = true, int delay = 0)
        //{
        //    if (delay <= 0)
        //    {
        //        completionAnimation(playSound);
        //    }
        //    else
        //    {
        //        completionTimer = delay;
        //    }
        //}

        public static Color getColorFromColorIndex(int color)
        {
            switch (color)
            {
                case 5:
                    return Color.LightBlue;
                case 0:
                    return Color.Lime;
                case 2:
                    return Color.Orange;
                case 1:
                    return Color.DeepPink;
                case 4:
                    return Color.Red;
                case 6:
                    return Color.Cyan;
                case 3:
                    return Color.Orange;
                default:
                    return Color.Lime;
            }
        }
    }
}
