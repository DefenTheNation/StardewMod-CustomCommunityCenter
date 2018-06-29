using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
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

        public ClickableTextureComponent backButton;
        public ClickableTextureComponent purchaseButton;
        public ClickableTextureComponent areaNextButton;
        public ClickableTextureComponent areaBackButton;
        public ClickableAnimatedComponent presentButton;

        private bool ViewingSpecificBundle;
        private bool fromGameMenu;
        private bool fromThisMenu;

        private int whichArea;

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

            // Add bundles here

            // Add back button

            // Add update to community center
        }

        private void setupBundleSpecificPage(DisplayBundle bundle)
        {
            //tempSprites.Clear();

            ViewingSpecificBundle = true;

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
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY, 1, 72, 72, 12));
                    break;
                case 2:
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY, 2, 72, 72, 12));
                    break;
                case 3:
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY, 3, 72, 72, 12));
                    break;
                case 4:
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY, 4, 72, 72, 12));
                    break;
                case 5:
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY - 36, 3, 72, 72, 12));
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY + 40, 2, 72, 72, 12));
                    break;
                case 6:
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY - 36, 3, 72, 72, 12));
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY + 40, 3, 72, 72, 12));
                    break;
                case 7:
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY - 36, 4, 72, 72, 12));
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY + 40, 3, 72, 72, 12));
                    break;
                case 8:
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY - 36, 4, 72, 72, 12));
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY + 40, 4, 72, 72, 12));
                    break;
                case 9:
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY - 36, 5, 72, 72, 12));
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY + 40, 4, 72, 72, 12));
                    break;
                case 10:
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY - 36, 5, 72, 72, 12));
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY + 40, 5, 72, 72, 12));
                    break;
                case 11:
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY - 36, 6, 72, 72, 12));
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY + 40, 5, 72, 72, 12));
                    break;
                case 12:
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY - 36, 6, 72, 72, 12));
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY + 40, 6, 72, 72, 12));
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
                                //((NetDictionary<int, bool, NetBool, SerializableDictionary<int, bool>, NetIntDictionary<bool, NetBool>>)((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).bundleRewards)[currentPageBundle.bundleIndex] = true;
                                //(Game1.getLocationFromName("CommunityCenter") as CommunityCenter).bundles.FieldDict[currentPageBundle.bundleIndex][0] = true;
                                checkForRewards();
                                bool isOneIncomplete = false;

                                foreach(var bundle in bundles)
                                {
                                    if(!bundle.BundleInfo.Completed && bundle.BundleInfo.Name != currentPageBundle.BundleInfo.Name)
                                    {
                                        isOneIncomplete = true;
                                        break;
                                    }
                                }

                                if (!isOneIncomplete)
                                {
#warning mark bundle group as completed
#warning provide reward
                                    //((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).areasComplete[whichArea] = true;
                                    //((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).areaCompleteReward(whichArea);
                                    base.exitFunction = restoreAreaOnExit;
                                }
                                else
                                {
#warning set junimo to get star for plaque
                                    Junimo k = ((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).getJunimoForArea(whichArea);
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
                        foreach(var bundle in bundles)
                        {
                            if(bundle.CanBeClicked() && bundle.containsPoint(x,y))
                            {
                                setupBundleSpecificPage(bundle);
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
                                    if (cc.shouldNoteAppearInArea((whichArea + j) % 6))
                                    {
                                        Game1.activeClickableMenu = new JunimoNoteMenu(true, (whichArea + j) % 6, true);
                                        return;
                                    }
                                }
                            }
                            else if (areaBackButton.containsPoint(x, y))
                            {
                                int area = whichArea;
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

        private void restoreAreaOnExit()
        {
            throw new NotImplementedException();
        }

        private void checkForRewards()
        {
            throw new NotImplementedException();
        }

        private void checkIfBundleIsComplete()
        {
            throw new NotImplementedException();
        }

        private void takeDownBundleSpecificPage(DisplayBundle currentPageBundle)
        {
            throw new NotImplementedException();
        }

        private void openRewardsMenu()
        {
            throw new NotImplementedException();
        }

        private void closeBundlePage()
        {
            throw new NotImplementedException();
        }
    }
}
