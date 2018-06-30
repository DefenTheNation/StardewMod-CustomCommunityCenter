using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

        private int completionTimer;     
        private int ingredientSlots;

        private float maxShake;
        private bool shakeLeft;

        public int BundleColor { get; set; }
        public bool DepositsAllowed { get; set; } = true;

        protected TemporaryAnimatedSprite sprite;
        public BundleInfo BundleInfo { get; set; }

        public DisplayBundle(int bundleIndex, string textureName, BundleInfo info, Point position) : base(new Rectangle(position.X, position.Y, SquareWidth, SquareWidth), "")
        {
            BundleInfo = info;
            ingredientSlots = BundleInfo.IngredientsRequired;

            BundleColor = bundleIndex;

            sprite = new TemporaryAnimatedSprite(textureName, new Rectangle(BundleColor * 256 % 512, 244 + BundleColor * 256 / 512 * 16, 16, 16), 70f, 3, 99999, new Vector2((float)base.bounds.X, (float)base.bounds.Y), false, false, 0.8f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
            {
                pingPong = true
            };
            sprite.paused = false;
            sprite.sourceRect.X += sprite.sourceRect.Width;
            if (name.ToLower().Contains(Game1.currentSeason) && !BundleInfo.Completed)
            {
                shake(0.07363108f);
            }
            if (BundleInfo.Completed)
            {
                completionAnimation(false, 0);
            }
        }

        public void tryHoverAction(int x, int y)
        {
            if (base.bounds.Contains(x, y) && !BundleInfo.Completed)
            {
                sprite.paused = false;
                label = BundleInfo.Name;
                CustomJunimoNoteMenu.hoverText = BundleInfo.Name;
            }
            else if (!BundleInfo.Completed)
            {
                sprite.reset();
                sprite.sourceRect.X += sprite.sourceRect.Width;
                sprite.paused = true;
            }
        }

        public Item tryToDepositThisItem(Item item, ClickableTextureComponent slot, string noteTextureName)
        {
            if (!DepositsAllowed)
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:JunimoNote_MustBeAtCC"));
                return item;
            }
            else if (item is StardewValley.Object o)
            {
                foreach(var bundleItem in BundleInfo.Ingredients)
                {
                    if(bundleItem.TryCompleteIngredient(o))
                    {
                        IngredientDepositAnimation(slot, noteTextureName, false);
                        slot.item = ObjectFactory.getItemFromDescription(0, bundleItem.ItemId, bundleItem.RequiredStack);
                        
                        Game1.playSound("newArtifact");

                        slot.sourceRect.X = 512;
                        slot.sourceRect.Y = 244;
                        break;
                    }
                }
            }

            if (item.Stack <= 0) return null;
            else return item;
        }

        public bool CanBeClicked()
        {
            return !BundleInfo.Completed;
        }

        public void IngredientDepositAnimation(ClickableComponent slot, string noteTextureName, bool skipAnimation = false)
        {
            TemporaryAnimatedSprite t = new TemporaryAnimatedSprite(noteTextureName, new Rectangle(530, 244, 18, 18), 50f, 6, 1, new Vector2((float)slot.bounds.X, (float)slot.bounds.Y), false, false, 0.88f, 0f, Color.White, 4f, 0f, 0f, 0f, true)
            {
                holdLastFrame = true,
                endSound = "cowboy_monsterhit"
            };

            if (skipAnimation)
            {
                t.sourceRect.Offset(t.sourceRect.Width * 5, 0);
                t.sourceRectStartingPos = new Vector2((float)t.sourceRect.X, (float)t.sourceRect.Y);
                t.animationLength = 1;
            }

            CustomJunimoNoteMenu.tempSprites.Add(t);
        }

        public void Draw(SpriteBatch b)
        {
            sprite.draw(b, true, 0, 0, 1f);
        }

        public void completionAnimation(bool playSound = true, int delay = 0)
        {
            if (delay <= 0)
            {
                completionAnimation(playSound);
            }
            else
            {
                completionTimer = delay;
            }
        }

        public void shakeAndAllowClicking(int extraInfo)
        {
            maxShake = 0.07363108f;
            CustomJunimoNoteMenu.canClick = true;
        }

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

        internal void completionAnimation(CustomJunimoNoteMenu customJunimoNoteMenu, bool playSound = true, int delay = 0)
        {
            if (delay <= 0)
            {
                completionAnimation(playSound);
            }
            else
            {
                completionTimer = delay;
            }
        }

        private void completionAnimation(bool playSound = true)
        {
            if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is CustomJunimoNoteMenu menu)
            {
                menu.TakeDownBundleSpecificPage(null);
            }
            sprite.pingPong = false;
            sprite.paused = false;
            sprite.sourceRect.X = (int)sprite.sourceRectStartingPos.X;
            sprite.sourceRect.X += sprite.sourceRect.Width;
            sprite.animationLength = 15;
            sprite.interval = 50f;
            sprite.totalNumberOfLoops = 0;
            sprite.holdLastFrame = true;
            sprite.endFunction = shake;
            sprite.extraInfoForEndBehavior = 1;

            bool complete = BundleInfo.Completed;
            if (complete)
            {
                sprite.sourceRect.X += sprite.sourceRect.Width * 14;
                sprite.sourceRectStartingPos = new Vector2((float)sprite.sourceRect.X, (float)sprite.sourceRect.Y);
                sprite.currentParentTileIndex = 14;
                sprite.interval = 0f;
                sprite.animationLength = 1;
                sprite.extraInfoForEndBehavior = 0;
            }
            else
            {
                if (playSound)
                {
                    Game1.playSound("dwop");
                }
                base.bounds.Inflate(64, 64);
                CustomJunimoNoteMenu.tempSprites.AddRange(Utility.sparkleWithinArea(base.bounds, 8, getColorFromColorIndex(BundleColor) * 0.5f, 100, 0, ""));
                base.bounds.Inflate(-64, -64);
            }
        }

        public void shake(float force = 0.07363108f)
        {
            if (sprite.paused)
            {
                maxShake = force;
            }
        }

        public void shake(int extraInfo)
        {
            maxShake = 0.07363108f;
            if (extraInfo == 1)
            {
                Game1.playSound("leafrustle");
                CustomJunimoNoteMenu.tempSprites.Add(new TemporaryAnimatedSprite(50, sprite.position, getColorFromColorIndex(BundleColor), 8, false, 100f, 0, -1, -1f, -1, 0)
                {
                    motion = new Vector2(-1f, 0.5f),
                    acceleration = new Vector2(0f, 0.02f)
                });
                CustomJunimoNoteMenu.tempSprites.Add(new TemporaryAnimatedSprite(50, sprite.position, getColorFromColorIndex(BundleColor), 8, false, 100f, 0, -1, -1f, -1, 0)
                {
                    motion = new Vector2(1f, 0.5f),
                    acceleration = new Vector2(0f, 0.02f),
                    flipped = true,
                    delayBeforeAnimationStart = 50
                });
            }
        }

        public void update(GameTime time)
        {
            sprite.update(time);
            if (completionTimer > 0 && CustomJunimoNoteMenu.screenSwipe == null)
            {
                completionTimer -= time.ElapsedGameTime.Milliseconds;
                if (completionTimer <= 0)
                {
                    completionAnimation(true);
                }
            }
            if (Game1.random.NextDouble() < 0.005 && (BundleInfo.Completed || base.name.ToLower().Contains(Game1.currentSeason)))
            {
                shake(0.07363108f);
            }
            if (maxShake > 0f)
            {
                if (shakeLeft)
                {
                    sprite.rotation -= 0.0157079641f;
                    if (sprite.rotation <= 0f - maxShake)
                    {
                        shakeLeft = false;
                    }
                }
                else
                {
                    sprite.rotation += 0.0157079641f;
                    if (sprite.rotation >= maxShake)
                    {
                        shakeLeft = true;
                    }
                }
            }

            if (maxShake > 0f)
            {
                maxShake = Math.Max(0f, maxShake - 0.0007669904f);
            }
        }

        internal bool canAcceptThisItem(Item heldItem, ClickableTextureComponent slot)
        {
            if (slot.item != null) return false;
            else if(!(heldItem is StardewValley.Object)) return false;

            var o = heldItem as StardewValley.Object;
            foreach(var item in BundleInfo.Ingredients)
            {
                if (item.WillCompleteIngredient(o)) return true;
            }

            return false;
        }
    }
}
