using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
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

        public bool scrambledText = false;
        public List<ClickableTextureComponent> IngredientSlots = new List<ClickableTextureComponent>();
        public List<ClickableTextureComponent> IngredientList = new List<ClickableTextureComponent>();
        public List<ClickableTextureComponent> OtherClickableComponents = new List<ClickableTextureComponent>();

        private Texture2D noteTexture;
        private InventoryMenu inventory;
        private Item heldItem;
        private DisplayBundle currentPageBundle;
        private List<DisplayBundle> bundles;
        private readonly List<BundleAreaInfo> bundleGroupList;

        public ClickableTextureComponent backButton;
        public ClickableTextureComponent purchaseButton;
        public ClickableTextureComponent areaNextButton;
        public ClickableTextureComponent areaBackButton;
        public ClickableAnimatedComponent presentButton;

        private bool ViewingSpecificBundle;
        private bool fromGameMenu;
        private bool fromThisMenu;

        private int areaIndex;
        private int bundleIndex;

        public CustomJunimoNoteMenu(List<BundleAreaInfo> info, int index, bool _fromGameMenu)
            : base(Game1.viewport.Width / 2 - 640, Game1.viewport.Height / 2 - 360, Width, Height, ShowCloseButton)
        {
            bundles = new List<DisplayBundle>();

            fromGameMenu = _fromGameMenu;
            fromThisMenu = !_fromGameMenu;

            bundleGroupList = info;
            setupMenu(index);
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
                bundles.Add(new DisplayBundle(bundle, getBundleLocationFromNumber(bundlesAdded)));
                bundlesAdded++;
            }

            // Add back button
            backButton = new ClickableTextureComponent("Back", new Rectangle(xPositionOnScreen + borderWidth * 2 + 8, yPositionOnScreen + borderWidth * 2 + 4, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false)
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
            //tempSprites.Clear();

            ViewingSpecificBundle = true;
            currentPageBundle = bundle;

            var ingredientList = bundle.BundleInfo.Ingredients;
            int numberOfIngredientSlots = ingredientList.Count;
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
            addRectangleRowsToList(ingredientListRectangles, numberOfIngredientSlots, 932, 364);
            for (int j = 0; j < ingredientListRectangles.Count; j++)
            {
                if (Game1.objectInformation.ContainsKey(ingredientList[j].ItemId))
                {
#warning Update display name to use custom config
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

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (canClick)
            {
                base.receiveLeftClick(x, y, playSound);
                if (!scrambledText)
                {
                    if (ViewingSpecificBundle)
                    {
                        heldItem = inventory.leftClick(x, y, heldItem, true);
                        if (backButton.containsPoint(x, y) && heldItem == null)
                        {
                            takeDownBundleSpecificPage(currentPageBundle);
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
                                if (IngredientSlots[l].containsPoint(x, y))
                                {
                                    heldItem = currentPageBundle.tryToDepositThisItem(heldItem, IngredientSlots[l], "LooseSprites\\JunimoNote");
                                    checkIfBundleIsComplete();
                                }
                            }
                        }
                        if (purchaseButton != null && purchaseButton.containsPoint(x, y))
                        {
                            int moneyRequired = currentPageBundle.BundleInfo.Ingredients.Last().RequiredStack;
                            if (Game1.player.Money >= moneyRequired)
                            {
                                Game1.player.Money -= moneyRequired;
                                Game1.playSound("select");
                                currentPageBundle.completionAnimation(this, true, 0);
                                if (purchaseButton != null)
                                {
                                    purchaseButton.scale = purchaseButton.baseScale * 0.75f;
                                }

#warning mark bundle as collected
                                bundleGroupList[areaIndex].Collected = true;
                                //((NetDictionary<int, bool, NetBool, SerializableDictionary<int, bool>, NetIntDictionary<bool, NetBool>>)((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).bundleRewards)[currentPageBundle.bundleIndex] = true;
                                //(Game1.getLocationFromName("CommunityCenter") as CommunityCenter).bundles.FieldDict[currentPageBundle.bundleIndex][0] = true;
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
                        for(int i = 0; i < bundles.Count; i++)
                        {
                            if(bundles[i].CanBeClicked() && bundles[i].containsPoint(x,y))
                            {
                                bundleIndex = i;
                                setupBundleSpecificPage(bundles[i]);
                                Game1.playSound("shwip");
                                return;
                            }
                        }

                        if (presentButton != null && presentButton.containsPoint(x, y) && !fromGameMenu && !fromThisMenu)
                        {
                            openRewardsMenu();
                        }
                        if (fromGameMenu)
                        {
                            CommunityCenter cc = Game1.getLocationFromName("CommunityCenter") as CommunityCenter;
                            if (areaNextButton.containsPoint(x, y))
                            {
                                for (int j = 1; j < 7; j++)
                                {
                                    if (cc.shouldNoteAppearInArea((areaIndex + j) % 6))
                                    {
                                        Game1.activeClickableMenu = new JunimoNoteMenu(true, (areaIndex + j) % 6, true);
                                        return;
                                    }
                                }
                            }
                            else if (areaBackButton.containsPoint(x, y))
                            {
                                int area = areaIndex;
                                for (int i = 1; i < 7; i++)
                                {
                                    area--;
                                    if (area == -1)
                                    {
                                        area = 5;
                                    }
                                    if (cc.shouldNoteAppearInArea(area))
                                    {
                                        Game1.activeClickableMenu = new JunimoNoteMenu(true, area, true);
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

        private void completeCommunityCenter(int areaIndex)
        {
#warning mark bundle group as completed
#warning provide reward for bundle 
            //((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).areasComplete[whichArea] = true;
            //((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).areaCompleteReward(whichArea);
            base.exitFunction = restoreAreaOnExit;
        }

        private void restoreAreaOnExit()
        {
            if(!fromGameMenu)
            {
                ((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).restoreAreaCutscene(areaIndex);
            }
        }

        private void checkIfBundleIsComplete()
        {
            if (!ViewingSpecificBundle || currentPageBundle == null) return;

            foreach (ClickableTextureComponent ingredientSlot in IngredientSlots)
            {
                if (ingredientSlot.item == null)
                {
                    return;
                }
            }

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
            //screenSwipe = new ScreenSwipe(0, -1f, -1);
            currentPageBundle.completionAnimation(this, true, 400);
            canClick = false;
#warning sync bundles in multiplayer and send multiplayer chat message
            //((NetDictionary<int, bool, NetBool, SerializableDictionary<int, bool>, NetIntDictionary<bool, NetBool>>)((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).bundleRewards)[currentPageBundle.bundleIndex] = true;
            //Game1.multiplayer.globalChatInfoMessage("Bundle");
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
                    presentButton = new ClickableAnimatedComponent(new Rectangle(xPositionOnScreen + 592, yPositionOnScreen + 512, 72, 72), "", Game1.content.LoadString("Strings\\StringsFromCSFiles:JunimoNoteMenu.cs.10783"), new TemporaryAnimatedSprite("LooseSprites\\JunimoNote", new Rectangle(548, 262, 18, 20), 70f, 4, 99999, new Vector2(-64f, -64f), false, false, 0.5f, 0f, Color.White, 4f, 0f, 0f, 0f, true));
                }
            }

        }

        private void takeDownBundleSpecificPage(DisplayBundle b)
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
            purchaseButton = null;

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
                    var rewardItem = ObjectFactory.getItemFromDescription((byte)bundleList[i].RewardItemType, bundleList[i].RewardItemId, bundleList[i].RewardStack);
                    rewardItem.SpecialVariable = i;
                    rewards.Add(rewardItem);
                }
            }

            Game1.activeClickableMenu = new ItemGrabMenu(rewards, false, true, null, null, null, rewardGrabbed, false, true, true, true, false, 0, null, -1, null);
            Game1.activeClickableMenu.exitFunction = ((base.exitFunction != null) ? base.exitFunction : new onExit(reOpenThisMenu));
        }

        private void reOpenThisMenu()
        {
            Game1.activeClickableMenu = new CustomJunimoNoteMenu(bundleGroupList, areaIndex, false);
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
                takeDownBundleSpecificPage(currentPageBundle);
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
    }
}
