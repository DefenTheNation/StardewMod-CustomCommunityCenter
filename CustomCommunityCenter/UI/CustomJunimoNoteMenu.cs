using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomCommunityCenter
{
    public class CustomJunimoNoteMenu : IClickableMenu
    {
        public static int Width = 1280;
        public static int Height = 720;
        public static bool canClick = true;
        public static bool ShowCloseButton = true;
        public static string BundleTextureName = "LooseSprites\\JunimoNote";
        public static List<TemporaryAnimatedSprite> tempSprites = new List<TemporaryAnimatedSprite>();
        public static ScreenSwipe screenSwipe;
        public static string hoverText = "";

        private int areaIndex;
        private int bundleIndex;

        private bool ViewingSpecificBundle;
        private Texture2D noteTexture;
        private InventoryMenu inventory;
        private Item heldItem;
        private Item hoveredItem;
        private DisplayBundle currentPageBundle;
        private List<DisplayBundle> bundles;
        private readonly List<BundleAreaInfo> bundleGroupList;

        public ClickableTextureComponent BackButton { get; set; }
        public ClickableTextureComponent PurchaseButton { get; set; }
        public ClickableTextureComponent AreaNextButton { get; set; }
        public ClickableTextureComponent AreaBackButton { get; set; }
        public ClickableAnimatedComponent PresentButton { get; set; }

        public bool FromGameMenu { get; set; }
        public bool FromThisMenu { get; set; }
        public bool BundlesChanged { get; set; }
        public bool ScrambledText { get; set; }

        public List<ClickableTextureComponent> IngredientSlots { get; set; } = new List<ClickableTextureComponent>();
        public List<ClickableTextureComponent> IngredientList { get; set; } = new List<ClickableTextureComponent>();
        public List<ClickableTextureComponent> OtherClickableComponents { get; set; } = new List<ClickableTextureComponent>();

        public CustomJunimoNoteMenu(List<BundleAreaInfo> info, int index, bool _fromGameMenu, bool _fromThisMenu)
            : base(Game1.viewport.Width / 2 - 640, Game1.viewport.Height / 2 - 360, Width, Height, ShowCloseButton)
        {
            bundles = new List<DisplayBundle>();

            FromGameMenu = _fromGameMenu;
            FromThisMenu = _fromThisMenu;

            bundleGroupList = info;
            setupMenu(index);

            Game1.player.forceCanMove();
            AreaNextButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width - 128, base.yPositionOnScreen, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f, false)
            {
                visible = false,
                myID = 101,
                leftNeighborID = 102
            };
            AreaBackButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + 64, base.yPositionOnScreen, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f, false)
            {
                visible = false,
                myID = 102,
                rightNeighborID = 101
            };
            for (int j = 0; j < 6; j++)
            {
#warning Get whether note should appear from community center
                //if (cc.shouldNoteAppearInArea((area + j) % 6))
                if(true)
                {
                    AreaNextButton.visible = true;
                }
            }
            for (int i = 0; i < 6; i++)
            {
                int a = areaIndex - i;
                if (a == -1)
                {
                    a = 5;
                }
#warning Get whether note should appear from community center
                // if(cc.shouldNoteAppearInArea(area))
                if (true)
                {
                    AreaBackButton.visible = true;
                }
            }

            foreach (var bundle in bundles)
            {
#warning debug
                //bundle.DepositsAllowed = false;
                bundle.DepositsAllowed = true;
            }
            if (Game1.options.SnappyMenus)
            {
                base.populateClickableComponentList();
                snapToDefaultClickableComponent();
            }
        }

        private string getRewardNameForArea(int index)
        {
            return bundleGroupList[areaIndex].Name;
        }

        private void setupMenu(int area)
        {
            areaIndex = area;
            noteTexture = Game1.temporaryContent.Load<Texture2D>(BundleTextureName);
            inventory = new InventoryMenu(xPositionOnScreen + 128, yPositionOnScreen + 140, true, null, Utility.highlightSmallObjects, 36, 6, 8, 8, false)
            {
                capacity = 36
            };

            // Add bundles here
            bundles.Clear();

            int bundlesAdded = 0;
            var bundleInfo = bundleGroupList[areaIndex];
            foreach(var bundle in bundleInfo.Bundles)
            {
                bundles.Add(new DisplayBundle(bundlesAdded, BundleTextureName, bundle, getBundleLocationFromNumber(bundlesAdded))
                {
                    myID = bundlesAdded + 505,
                    rightNeighborID = -7777,
                    leftNeighborID = -7777,
                    upNeighborID = -7777,
                    downNeighborID = -7777,
                    fullyImmutable = true
                });
                bundlesAdded++;
            }

            // Add back button
            BackButton = new ClickableTextureComponent("Back", new Rectangle(xPositionOnScreen + borderWidth * 2 + 8, yPositionOnScreen + borderWidth * 2 + 4, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false)
            {
                myID = 103
            };

            // Add update to community center
            checkForRewards();
            canClick = true;
            Game1.playSound("shwip");
        }

        private void setupBundleSpecificPage(DisplayBundle bundle)
        {
            tempSprites.Clear();

            ViewingSpecificBundle = true;
            currentPageBundle = bundle;

            if(bundle.BundleInfo.IsPurchase)
            {
                setupPurchaseBundle(bundle);
            }
            else
            {
                setupRegularBundle(bundle);
            }

            
        }

        private void setupPurchaseBundle(DisplayBundle bundle)
        {
            if (FromGameMenu) return;

            PurchaseButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + 800, base.yPositionOnScreen + 504, 260, 72), noteTexture, new Rectangle(517, 286, 65, 20), 4f, false)
            {
                myID = 797,
                leftNeighborID = 103
            };
            if (Game1.options.SnappyMenus)
            {
                base.currentlySnappedComponent = PurchaseButton;
                snapCursorToCurrentSnappedComponent();
            }
        }

        private void setupRegularBundle(DisplayBundle bundle)
        {
            var ingredientList = bundle.BundleInfo.Ingredients;
            int numberOfIngredientSlots = bundle.BundleInfo.IngredientsRequired;
            int numberOfPossibleIngredients = ingredientList.Count;

            List<Rectangle> ingredientSlotRectangles = new List<Rectangle>();
            addRectangleRowsToList(ingredientSlotRectangles, numberOfIngredientSlots, 932, 540);
            for (int k = 0; k < ingredientSlotRectangles.Count; k++)
            {
                IngredientSlots.Add(new ClickableTextureComponent(ingredientSlotRectangles[k], noteTexture, new Rectangle(512, 244, 18, 18), 4f, false)
                {
                    myID = k + 250,
                    rightNeighborID = ((k < ingredientSlotRectangles.Count - 1) ? (k + 250 + 1) : (-1)),
                    leftNeighborID = ((k > 0) ? (k + 250 - 1) : (-1))
                });
            }
            List<Rectangle> ingredientListRectangles = new List<Rectangle>();
            addRectangleRowsToList(ingredientListRectangles, numberOfPossibleIngredients, 932, 364);
            for (int j = 0; j < ingredientListRectangles.Count; j++)
            {
                if (Game1.objectInformation.ContainsKey(ingredientList[j].ItemId))
                {
                    string displayName = Game1.objectInformation[ingredientList[j].ItemId].Split('/')[4];

                    IngredientList.Add(new ClickableTextureComponent("", ingredientListRectangles[j], "", displayName, Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, ingredientList[j].ItemId, 16, 16), 4f, false)
                    {
                        item = new StardewValley.Object(ingredientList[j].ItemId, ingredientList[j].RequiredStack, false, -1, ingredientList[j].ItemQuality)
                    });
                }
            }
            updateIngredientSlots(bundle);
            if (Game1.options.SnappyMenus)
            {
                base.populateClickableComponentList();
                if (inventory != null && inventory.inventory != null)
                {
                    for (int i = 0; i < inventory.inventory.Count; i++)
                    {
                        if (inventory.inventory[i] != null)
                        {
                            if (inventory.inventory[i].downNeighborID == 101)
                            {
                                inventory.inventory[i].downNeighborID = -1;
                            }
                            if (inventory.inventory[i].rightNeighborID == 106)
                            {
                                inventory.inventory[i].rightNeighborID = 250;
                            }
                            if (inventory.inventory[i].leftNeighborID == -1)
                            {
                                inventory.inventory[i].leftNeighborID = 103;
                            }
                            if (inventory.inventory[i].upNeighborID >= 1000)
                            {
                                inventory.inventory[i].upNeighborID = 103;
                            }
                        }
                    }
                }
                base.currentlySnappedComponent = base.getComponentWithID(0);
                snapCursorToCurrentSnappedComponent();
            }
        }

        private void updateIngredientSlots(DisplayBundle bundle)
        {
            int slotNumber = 0;
            var info = bundle.BundleInfo;

            for (int i = 0; i < info.Ingredients.Count; i++)
            {
                if (info.Ingredients[i].Completed && slotNumber < IngredientSlots.Count)
                {
                    IngredientSlots[slotNumber].item = new StardewValley.Object(info.Ingredients[i].ItemId, info.Ingredients[i].RequiredStack, false, -1, info.Ingredients[i].ItemQuality);
                    bundle.IngredientDepositAnimation(IngredientSlots[slotNumber], "LooseSprites\\JunimoNote", true);
                    slotNumber++;
                }
            }
        }

        private void addRectangleRowsToList(List<Rectangle> toAddTo, int numberOfItems, int centerX, int centerY)
        {
            switch (numberOfItems)
            {
                case 1:
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY, 1, 72, 72, 12));
                    break;
                case 2:
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY, 2, 72, 72, 12));
                    break;
                case 3:
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY, 3, 72, 72, 12));
                    break;
                case 4:
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY, 4, 72, 72, 12));
                    break;
                case 5:
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 3, 72, 72, 12));
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 2, 72, 72, 12));
                    break;
                case 6:
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 3, 72, 72, 12));
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 3, 72, 72, 12));
                    break;
                case 7:
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 4, 72, 72, 12));
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 3, 72, 72, 12));
                    break;
                case 8:
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 4, 72, 72, 12));
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 4, 72, 72, 12));
                    break;
                case 9:
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 5, 72, 72, 12));
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 4, 72, 72, 12));
                    break;
                case 10:
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 5, 72, 72, 12));
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 5, 72, 72, 12));
                    break;
                case 11:
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 6, 72, 72, 12));
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 5, 72, 72, 12));
                    break;
                case 12:
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 6, 72, 72, 12));
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 6, 72, 72, 12));
                    break;
            }
        }

        private List<Rectangle> createRowOfBoxesCenteredAt(int xStart, int yStart, int numBoxes, int boxWidth, int boxHeight, int horizontalGap)
        {
            List<Rectangle> rectangles = new List<Rectangle>();
            int actualXStart = xStart - numBoxes * (boxWidth + horizontalGap) / 2;
            int actualYStart = yStart - boxHeight / 2;
            for (int i = 0; i < numBoxes; i++)
            {
                rectangles.Add(new Rectangle(actualXStart + i * (boxWidth + horizontalGap), actualYStart, boxWidth, boxHeight));
            }
            return rectangles;
        }

        private void completeCommunityCenter(int areaIndex)
        {
#warning mark bundle group as completed
#warning provide reward for bundle group
            //((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).areasComplete[whichArea] = true;
            //((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).areaCompleteReward(whichArea);
            base.exitFunction = restoreAreaOnExit;
        }

        private void restoreAreaOnExit()
        {
            if(!FromGameMenu)
            {
                ((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).restoreAreaCutscene(areaIndex);
            }
        }

        private void checkIfBundleIsComplete()
        {
            if (!ViewingSpecificBundle || currentPageBundle == null || !currentPageBundle.BundleInfo.Completed) return;

            if (heldItem != null)
            {
                Game1.player.addItemToInventory(heldItem);
                heldItem = null;
            }

#warning update community center bundle status
            //for (int j = 0; j < ((NetDictionary<int, bool[], NetArray<bool, NetBool>, SerializableDictionary<int, bool[]>, NetBundles>)((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).bundles)[currentPageBundle.bundleIndex].Length; j++)
            //{
            //    ((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).bundles.FieldDict[currentPageBundle.bundleIndex][j] = true;
            //}
#warning notify community center to check for new junimo notes
            //((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).checkForNewJunimoNotes();
            screenSwipe = new ScreenSwipe(0, -1f, -1);
            currentPageBundle.completionAnimation(this, true, 400);
            canClick = false;
#warning sync bundles in multiplayer and send multiplayer chat message
            //((NetDictionary<int, bool, NetBool, SerializableDictionary<int, bool>, NetIntDictionary<bool, NetBool>>)((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).bundleRewards)[currentPageBundle.bundleIndex] = true;
            //helper.globalChatInfoMessage("Bundle");
            bool isOneIncomplete = false;
            foreach (var bundle in bundles)
            {
                if (!bundle.BundleInfo.Completed && bundle.BundleInfo.Name != currentPageBundle.BundleInfo.Name)
                {
                    isOneIncomplete = true;
                    break;
                }
            }
            if (!isOneIncomplete)
            {
#warning complete community center section and give reward
                //((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).areasComplete[whichArea] = true;
                base.exitFunction = restoreAreaOnExit;
                //((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).areaCompleteReward(whichArea);
            }
            else
            {
                Junimo i = ((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).getJunimoForArea(areaIndex);
                i?.bringBundleBackToHut(Bundle.getColorFromColorIndex(currentPageBundle.BundleColor), Game1.getLocationFromName("CommunityCenter"));
            }

            checkForRewards();
        }

        private void checkForRewards()
        {
            foreach (var bundle in bundles)
            {
                if (bundle.BundleInfo.Completed && !bundle.BundleInfo.Collected)
                {
                    PresentButton = new ClickableAnimatedComponent(new Rectangle(xPositionOnScreen + 592, yPositionOnScreen + 512, 72, 72), "", Game1.content.LoadString("Strings\\StringsFromCSFiles:JunimoNoteMenu.cs.10783"), new TemporaryAnimatedSprite("LooseSprites\\JunimoNote", new Rectangle(548, 262, 18, 20), 70f, 4, 99999, new Vector2(-64f, -64f), false, false, 0.5f, 0f, Color.White, 4f, 0f, 0f, 0f, true));
                }
            }

        }

        public void TakeDownBundleSpecificPage(DisplayBundle b)
        {
            if (!ViewingSpecificBundle) return;

            if (b == null)
            {
                b = currentPageBundle;
            }

            ViewingSpecificBundle = false;
            IngredientSlots.Clear();
            IngredientList.Clear();
            tempSprites.Clear();
            PurchaseButton = null;

            if (Game1.options.SnappyMenus)
            {
                snapToDefaultClickableComponent();
            }
        }

        private void openRewardsMenu()
        {
            Game1.playSound("smallSelect");
            List<Item> rewards = new List<Item>();

            var bundleList = bundleGroupList[areaIndex].Bundles;
            for(int i = 0; i < bundleList.Count; i++) // (var bundle in bundleGroupList[areaIndex].Bundles)
            {
                if(bundleList[i].Completed && !bundleList[i].Collected)
                {
                    var rewardItem = ObjectFactory.getItemFromDescription((byte)bundleList[i].RewardItemType, bundleList[i].RewardItemId, bundleList[i].RewardItemStack);
                    rewardItem.SpecialVariable = i;
                    rewards.Add(rewardItem);
                }
            }

            Game1.activeClickableMenu = new ItemGrabMenu(rewards, false, true, null, null, null, rewardGrabbed, false, true, true, true, false, 0, null, -1, null);
            Game1.activeClickableMenu.exitFunction = ((base.exitFunction != null) ? base.exitFunction : new onExit(reOpenThisMenu));
        }

        private void reOpenThisMenu()
        {
            Game1.activeClickableMenu = new CustomJunimoNoteMenu(bundleGroupList, areaIndex, FromGameMenu, true);
        }

        private void rewardGrabbed(Item i, Farmer who)
        {
            bundleGroupList[areaIndex].Bundles[i.SpecialVariable].Collected = true;
        }

        private void closeBundlePage()
        {
            if (!ViewingSpecificBundle) return;

            //hoveredItem = null;
            inventory.descriptionText = "";
            if (heldItem == null)
            {
                TakeDownBundleSpecificPage(currentPageBundle);
                Game1.playSound("shwip");
            }
            else
            {
                heldItem = inventory.tryToAddItem(heldItem, "coin");
            }
        }

        private Point getBundleLocationFromNumber(int bundlesAdded)
        {
            Point location = new Point(xPositionOnScreen, yPositionOnScreen);
            switch (bundlesAdded)
            {
                case 0:
                    location.X += 592;
                    location.Y += 136;
                    break;
                case 1:
                    location.X += 392;
                    location.Y += 384;
                    break;
                case 2:
                    location.X += 784;
                    location.Y += 388;
                    break;
                case 5:
                    location.X += 588;
                    location.Y += 276;
                    break;
                case 6:
                    location.X += 588;
                    location.Y += 380;
                    break;
                case 3:
                    location.X += 304;
                    location.Y += 252;
                    break;
                case 4:
                    location.X += 892;
                    location.Y += 252;
                    break;
                case 7:
                    location.X += 440;
                    location.Y += 164;
                    break;
                case 8:
                    location.X += 776;
                    location.Y += 164;
                    break;
            }

            return location;
        }

        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), Color.Black * 0.5f);

            if (!ViewingSpecificBundle)
            {
                b.Draw(noteTexture, new Vector2((float)base.xPositionOnScreen, (float)base.yPositionOnScreen), new Rectangle(0, 0, 320, 180), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
                SpriteText.drawStringHorizontallyCenteredAt(b, ScrambledText ? CommunityCenter.getAreaEnglishDisplayNameFromNumber(areaIndex) : CommunityCenter.getAreaDisplayNameFromNumber(areaIndex), base.xPositionOnScreen + base.width / 2 + 16, base.yPositionOnScreen + 12, 999999, -1, 99999, 0.88f, 0.88f, ScrambledText, -1);
                if (ScrambledText)
                {
                    SpriteText.drawString(b, LocalizedContentManager.CurrentLanguageLatin ? Game1.content.LoadString("Strings\\StringsFromCSFiles:JunimoNoteMenu.cs.10786") : Game1.content.LoadBaseString("Strings\\StringsFromCSFiles:JunimoNoteMenu.cs.10786"), base.xPositionOnScreen + 96, base.yPositionOnScreen + 96, 999999, base.width - 192, 99999, 0.88f, 0.88f, true, -1, "", -1);
                    base.draw(b);
                    if (canClick)
                    {
                        base.drawMouse(b);
                    }
                    return;
                }
                foreach (var bundle in bundles)
                {
                    bundle.Draw(b);
                }
                if (PresentButton != null)
                {
                    PresentButton.draw(b);
                }

                tempSprites.ForEach(x => x.draw(b, true, 0, 0, 1f));

                if(FromGameMenu)
                {
                    if (AreaNextButton.visible)
                    {
                        AreaNextButton.draw(b);
                    }
                    if (AreaBackButton.visible)
                    {
                        AreaBackButton.draw(b);
                    }
                }
            }
            else
            {
                b.Draw(noteTexture, new Vector2((float)base.xPositionOnScreen, (float)base.yPositionOnScreen), new Rectangle(320, 0, 320, 180), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
                if (currentPageBundle != null)
                {
                    b.Draw(noteTexture, new Vector2((float)(base.xPositionOnScreen + 872), (float)(base.yPositionOnScreen + 88)), new Rectangle(bundleIndex * 16 * 2 % noteTexture.Width, 180 + 32 * (bundleIndex * 16 * 2 / noteTexture.Width), 32, 32), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.15f);
                    float textX = Game1.dialogueFont.MeasureString((!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", currentPageBundle.label)).X;
                    b.Draw(noteTexture, new Vector2((float)(base.xPositionOnScreen + 936 - (int)textX / 2 - 16), (float)(base.yPositionOnScreen + 228)), new Rectangle(517, 266, 4, 17), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
                    b.Draw(noteTexture, new Rectangle(base.xPositionOnScreen + 936 - (int)textX / 2, base.yPositionOnScreen + 228, (int)textX, 68), new Rectangle(520, 266, 1, 17), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.1f);
                    b.Draw(noteTexture, new Vector2((float)(base.xPositionOnScreen + 936 + (int)textX / 2), (float)(base.yPositionOnScreen + 228)), new Rectangle(524, 266, 4, 17), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
                    b.DrawString(Game1.dialogueFont, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", currentPageBundle.label), new Vector2((float)(base.xPositionOnScreen + 936) - textX / 2f, (float)(base.yPositionOnScreen + 244)) + new Vector2(2f, 2f), Game1.textShadowColor);
                    b.DrawString(Game1.dialogueFont, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", currentPageBundle.label), new Vector2((float)(base.xPositionOnScreen + 936) - textX / 2f, (float)(base.yPositionOnScreen + 244)) + new Vector2(0f, 2f), Game1.textShadowColor);
                    b.DrawString(Game1.dialogueFont, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", currentPageBundle.label), new Vector2((float)(base.xPositionOnScreen + 936) - textX / 2f, (float)(base.yPositionOnScreen + 244)) + new Vector2(2f, 0f), Game1.textShadowColor);
                    b.DrawString(Game1.dialogueFont, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", currentPageBundle.label), new Vector2((float)(base.xPositionOnScreen + 936) - textX / 2f, (float)(base.yPositionOnScreen + 244)), Game1.textColor * 0.9f);
                }
                BackButton.draw(b);
                if (PurchaseButton != null)
                {
                    PurchaseButton.draw(b);
                    Game1.dayTimeMoneyBox.drawMoneyBox(b, -1, -1);
                }

                tempSprites.ForEach(x => x.draw(b, true, 0, 0, 1f));

                IngredientSlots.ForEach(x =>
                {
                    if (x.item == null) x.draw(b, FromGameMenu ? (Color.LightGray * 0.5f) : Color.White, 0.89f);
                    x.drawItem(b, 4, 4);
                });

                IngredientList.ForEach(x =>
                {
                    b.Draw(Game1.shadowTexture, new Vector2((float)(x.bounds.Center.X - Game1.shadowTexture.Bounds.Width * 4 / 2 - 4), (float)(x.bounds.Center.Y + 4)), Game1.shadowTexture.Bounds, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
                    x.drawItem(b, 0, 0);
                });

                inventory.draw(b);
            }

            SpriteText.drawStringWithScrollCenteredAt(b, getRewardNameForArea(areaIndex), base.xPositionOnScreen + base.width / 2, Math.Min(base.yPositionOnScreen + base.height + 20, Game1.viewport.Height - 64 - 8), "", 1f, -1, 0, 0.88f, false);
            base.draw(b);
            Game1.mouseCursorTransparency = 1f;

            if (canClick)
            {
                base.drawMouse(b);
            }
            if (heldItem != null)
            {
                heldItem.drawInMenu(b, new Vector2((float)(Game1.getOldMouseX() + 16), (float)(Game1.getOldMouseY() + 16)), 1f);
            }
            if (inventory.descriptionText.Length > 0)
            {
                if (hoveredItem != null)
                {
                    IClickableMenu.drawToolTip(b, hoveredItem.getDescription(), hoveredItem.DisplayName, hoveredItem, false, -1, 0, -1, -1, null, -1);
                }
            }
            else
            {
                IClickableMenu.drawHoverText(b, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText") && hoverText.Length > 0) ? "???" : hoverText, Game1.dialogueFont, 0, 0, -1, null, -1, null, null, 0, -1, -1, -1, -1, 1f, null);
            }
            if (screenSwipe != null)
            {
                screenSwipe.draw(b);
            }
        }

        public override void update(GameTime time)
        {
            foreach (var bundle in bundles)
            {
                bundle.update(time);
            }

            for (int i = tempSprites.Count - 1; i >= 0; i--)
            {
                if (tempSprites[i].update(time))
                {
                    tempSprites.RemoveAt(i);
                }
            }

            if (PresentButton != null)
            {
                PresentButton.update(time);
            }

            if (screenSwipe != null)
            {
                canClick = false;
                if (screenSwipe.update(time))
                {
                    screenSwipe = null;
                    canClick = true;
                }
            }

            if (BundlesChanged && FromGameMenu)
            {
                reOpenThisMenu();
            }
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (canClick)
            {
                base.receiveLeftClick(x, y, playSound);
                if (!ScrambledText)
                {
                    if (ViewingSpecificBundle)
                    {
                        heldItem = inventory.leftClick(x, y, heldItem, true);
                        if (BackButton.containsPoint(x, y) && heldItem == null)
                        {
                            TakeDownBundleSpecificPage(currentPageBundle);
                            Game1.playSound("shwip");
                        }
                        if (heldItem != null)
                        {
                            if (Game1.oldKBState.IsKeyDown(Keys.LeftShift))
                            {
                                for (int m = 0; m < IngredientSlots.Count; m++)
                                {
                                    if (IngredientSlots[m].item == null)
                                    {
                                        heldItem = currentPageBundle.tryToDepositThisItem(heldItem, IngredientSlots[m], "LooseSprites\\JunimoNote");
                                        checkIfBundleIsComplete();
                                        return;
                                    }
                                }
                            }
                            for (int l = 0; l < IngredientSlots.Count; l++)
                            {
                                if (IngredientSlots[l].containsPoint(x, y) && IngredientSlots[l].item == null)
                                {
                                    heldItem = currentPageBundle.tryToDepositThisItem(heldItem, IngredientSlots[l], "LooseSprites\\JunimoNote");
                                    checkIfBundleIsComplete();
                                }
                            }
                        }
                        if (PurchaseButton != null && PurchaseButton.containsPoint(x, y))
                        {
                            int moneyRequired = currentPageBundle.BundleInfo.PurchaseAmount;
                            //int moneyRequired = 0;
                            if (Game1.player.Money >= moneyRequired)
                            {
                                Game1.player.Money -= moneyRequired;
                                Game1.playSound("select");
                                currentPageBundle.completionAnimation(this, true, 0);
                                if (PurchaseButton != null)
                                {
                                    PurchaseButton.scale = PurchaseButton.baseScale * 0.75f;
                                }

                                currentPageBundle.BundleInfo.PurchaseCompleted();
                                checkForRewards();

                                if (bundleGroupList[areaIndex].Completed)
                                {
                                    completeCommunityCenter(areaIndex);
                                }
                                else
                                {
#warning set junimo to get star for plaque
                                    Junimo k = ((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).getJunimoForArea(areaIndex);
                                    k?.bringBundleBackToHut(Bundle.getColorFromColorIndex(currentPageBundle.BundleColor), Game1.getLocationFromName("CommunityCenter"));
                                }
                            }
                            else
                            {
                                Game1.dayTimeMoneyBox.moneyShakeTimer = 600;
                            }
                        }
                        if (base.upperRightCloseButton != null && !readyToClose() && base.upperRightCloseButton.containsPoint(x, y))
                        {
                            closeBundlePage();
                        }
                    }
                    else
                    {
                        for (int i = 0; i < bundles.Count; i++)
                        {
                            if (bundles[i].CanBeClicked() && bundles[i].containsPoint(x, y))
                            {
                                bundleIndex = i;
                                setupBundleSpecificPage(bundles[i]);
                                Game1.playSound("shwip");
                                return;
                            }
                        }

                        if (PresentButton != null && PresentButton.containsPoint(x, y) && !FromGameMenu && !FromThisMenu)
                        {
                            openRewardsMenu();
                        }
                        if (FromGameMenu)
                        {
                            if (AreaNextButton.containsPoint(x, y))
                            {
                                for (int j = 1; j < 7; j++)
                                {
                                    if (shouldNoteAppearInArea((areaIndex + j) % 6))
                                    {
                                        Game1.activeClickableMenu = new CustomJunimoNoteMenu(bundleGroupList, (areaIndex + j) % 6, FromGameMenu, true);
                                        return;
                                    }
                                }
                            }
                            else if (AreaBackButton.containsPoint(x, y))
                            {
                                int area = areaIndex;
                                for (int i = 1; i < 7; i++)
                                {
                                    area--;
                                    if (area == -1)
                                    {
                                        area = 5;
                                    }
                                    if (shouldNoteAppearInArea(area))
                                    {
                                        Game1.activeClickableMenu = new CustomJunimoNoteMenu(bundleGroupList, area, FromGameMenu, true);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                    if (heldItem != null && !isWithinBounds(x, y) && heldItem.canBeTrashed())
                    {
                        Game1.playSound("throwDownITem");
                        Game1.createItemDebris(heldItem, Game1.player.getStandingPosition(), Game1.player.FacingDirection, null, -1);
                        heldItem = null;
                    }
                }
            }
        }

        private bool shouldNoteAppearInArea(int area)
        {
#warning update shouldNoteAppearInArea to do checks in Community Center class
            return true;
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            if (canClick)
            {
                if (ViewingSpecificBundle)
                {
                    heldItem = inventory.rightClick(x, y, heldItem, true);
                }
                if (!ViewingSpecificBundle && readyToClose())
                {
                    base.exitThisMenu(true);
                }
            }
        }

        public override void receiveKeyPress(Keys key)
        {
            base.receiveKeyPress(key);
            if (key.Equals(Keys.Delete) && heldItem != null && heldItem.canBeTrashed())
            {
                if (heldItem is StardewValley.Object && Game1.player.specialItems.Contains((heldItem as StardewValley.Object).ParentSheetIndex))
                {
                    Game1.player.specialItems.Remove((heldItem as StardewValley.Object).ParentSheetIndex);
                }
                heldItem = null;
                Game1.playSound("trashcan");
            }
            if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && !readyToClose())
            {
                closeBundlePage();
            }
        }

        public override void receiveGamePadButton(Buttons b)
        {
            base.receiveGamePadButton(b);
            if (FromGameMenu)
            {
                CommunityCenter cc = Game1.getLocationFromName("CommunityCenter") as CommunityCenter;
                switch (b)
                {
                    case Buttons.RightTrigger:
                        {
                            int j = 1;
                            while (true)
                            {
                                if (j < 7)
                                {
                                    if (!cc.shouldNoteAppearInArea((areaIndex + j) % 6))
                                    {
                                        j++;
                                        continue;
                                    }
                                    break;
                                }
                                return;
                            }
                            Game1.activeClickableMenu = new CustomJunimoNoteMenu(bundleGroupList, (areaIndex + j) % 6, FromGameMenu, true);
                            break;
                        }
                    case Buttons.LeftTrigger:
                        {
                            int area = areaIndex;
                            int i = 1;
                            while (true)
                            {
                                if (i < 7)
                                {
                                    area--;
                                    if (area == -1)
                                    {
                                        area = 5;
                                    }
                                    if (!cc.shouldNoteAppearInArea(area))
                                    {
                                        i++;
                                        continue;
                                    }
                                    break;
                                }
                                return;
                            }
                            Game1.activeClickableMenu = new CustomJunimoNoteMenu(bundleGroupList, area, FromGameMenu, true);
                            break;
                        }
                }
            }
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            if (!ScrambledText)
            {
                hoverText = "";
                if (ViewingSpecificBundle)
                {
                    BackButton.tryHover(x, y, 0.1f);
                    hoveredItem = inventory.hover(x, y, heldItem);

                    foreach(var ingredient in IngredientList)
                    {
                        if (ingredient.bounds.Contains(x, y))
                        {
                            hoverText = ingredient.hoverText;
                            break;
                        }
                    }
              
                    if (heldItem != null)
                    {
                        foreach(var slot in IngredientSlots)
                        {
                            if (slot.bounds.Contains(x, y) && currentPageBundle.canAcceptThisItem(heldItem, slot))
                            {
                                slot.sourceRect.X = 530;
                                slot.sourceRect.Y = 262;
                            }
                            else
                            {
                                slot.sourceRect.X = 512;
                                slot.sourceRect.Y = 244;
                            }
                        }
                    }
                    if (PurchaseButton != null)
                    {
                        PurchaseButton.tryHover(x, y, 0.1f);
                    }
                }
                else
                {
                    if (PresentButton != null)
                    {
                        hoverText = PresentButton.tryHover(x, y);
                    }
                    foreach (var bundle in bundles)
                    {
                        bundle.tryHoverAction(x, y);
                    }
                    if (FromGameMenu)
                    {
                        AreaNextButton.tryHover(x, y, 0.1f);
                        AreaBackButton.tryHover(x, y, 0.1f);
                    }
                }
            }
        }

        public override void snapToDefaultClickableComponent()
        {
            base.currentlySnappedComponent = base.getComponentWithID(505);
            snapCursorToCurrentSnappedComponent();
        }

        protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
        {
            if (oldID - 505 >= 0 && oldID - 505 < 10 && base.currentlySnappedComponent != null)
            {
                int lowestScoreBundle = -1;
                int lowestScore = 999999;
                Point startingPosition = base.currentlySnappedComponent.bounds.Center;
                for (int i = 0; i < bundles.Count; i++)
                {
                    if (bundles[i].myID != oldID)
                    {
                        int score = 999999;
                        Point bundlePosition = bundles[i].bounds.Center;
                        switch (direction)
                        {
                            case 3:
                                if (bundlePosition.X < startingPosition.X)
                                {
                                    score = startingPosition.X - bundlePosition.X + Math.Abs(startingPosition.Y - bundlePosition.Y) * 3;
                                }
                                break;
                            case 0:
                                if (bundlePosition.Y < startingPosition.Y)
                                {
                                    score = startingPosition.Y - bundlePosition.Y + Math.Abs(startingPosition.X - bundlePosition.X) * 3;
                                }
                                break;
                            case 1:
                                if (bundlePosition.X > startingPosition.X)
                                {
                                    score = bundlePosition.X - startingPosition.X + Math.Abs(startingPosition.Y - bundlePosition.Y) * 3;
                                }
                                break;
                            case 2:
                                if (bundlePosition.Y > startingPosition.Y)
                                {
                                    score = bundlePosition.Y - startingPosition.Y + Math.Abs(startingPosition.X - bundlePosition.X) * 3;
                                }
                                break;
                        }
                        if (score < 10000 && score < lowestScore)
                        {
                            lowestScore = score;
                            lowestScoreBundle = i;
                        }
                    }
                }
                if (lowestScoreBundle != -1)
                {
                    base.currentlySnappedComponent = base.getComponentWithID(lowestScoreBundle + 505);
                    snapCursorToCurrentSnappedComponent();
                }
                else
                {
                    switch (direction)
                    {
                        case 2:
                            if (PresentButton != null)
                            {
                                base.currentlySnappedComponent = PresentButton;
                                snapCursorToCurrentSnappedComponent();
                                PresentButton.upNeighborID = oldID;
                            }
                            break;
                        case 3:
                            if (AreaBackButton != null)
                            {
                                base.currentlySnappedComponent = AreaBackButton;
                                snapCursorToCurrentSnappedComponent();
                                AreaBackButton.rightNeighborID = oldID;
                            }
                            break;
                        case 1:
                            if (AreaNextButton != null)
                            {
                                base.currentlySnappedComponent = AreaNextButton;
                                snapCursorToCurrentSnappedComponent();
                                AreaNextButton.leftNeighborID = oldID;
                            }
                            break;
                    }
                }
            }
        }

        public override bool readyToClose()
        {
            if (heldItem == null)
            {
                return !ViewingSpecificBundle;
            }
            return false;
        }
       
    }
}
