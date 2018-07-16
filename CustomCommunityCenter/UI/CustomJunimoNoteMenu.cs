﻿using CustomCommunityCenter.API;
using CustomCommunityCenter.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
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

        public int AreaIndex { get; protected set; }
        public int BundleIndex { get; protected set; }

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

        public CustomJunimoNoteMenu(IList<BundleAreaInfo> info, int index, bool _fromGameMenu, bool _fromThisMenu)
            : base(Game1.viewport.Width / 2 - 640, Game1.viewport.Height / 2 - 360, Width, Height, ShowCloseButton)
        {
            bundles = new List<DisplayBundle>();

            FromGameMenu = _fromGameMenu;
            FromThisMenu = _fromThisMenu;

            bundleGroupList = info.ToList();
            SetupMenu(index);

            CustomCommunityCenter cc = CommunityCenterHelper.CustomCommunityCenter;

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
            for (int j = 0; j < info.Count; j++)
            {
                if (cc.shouldNoteAppearInArea((AreaIndex + j) % info.Count))
                {
                    AreaNextButton.visible = true;
                }
            }
            for (int i = 0; i < info.Count; i++)
            {
                int a = AreaIndex - i;
                if (a == -1)
                {
                    a = info.Count - 1;
                }

                if(cc.shouldNoteAppearInArea(AreaIndex))
                {
                    AreaBackButton.visible = true;
                }
            }

            foreach (var bundle in bundles)
            {
                bundle.DepositsAllowed = !_fromGameMenu;

                //#warning debug statement
                //bundle.DepositsAllowed = true;

            }

            if (Game1.options.SnappyMenus)
            {
                base.populateClickableComponentList();
                snapToDefaultClickableComponent();
            }
        }

        private string GetRewardNameForArea(int index)
        {
            return bundleGroupList[AreaIndex].RewardName;
        }

        private void SetupMenu(int area)
        {
            AreaIndex = area;
            noteTexture = Game1.temporaryContent.Load<Texture2D>(BundleTextureName);
            inventory = new InventoryMenu(xPositionOnScreen + 128, yPositionOnScreen + 140, true, null, Utility.highlightSmallObjects, 36, 6, 8, 8, false)
            {
                capacity = 36
            };

            // Add bundles here
            bundles.Clear();

            int bundlesAdded = 0;
            var bundleInfo = bundleGroupList[AreaIndex];
            foreach(var bundle in bundleInfo.Bundles)
            {
                bundles.Add(new DisplayBundle(bundlesAdded, BundleTextureName, bundle, GetBundleLocationFromNumber(bundlesAdded))
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
            CheckForRewards();
            canClick = true;
            Game1.playSound("shwip");
        }

        private void SetupBundleSpecificPage(DisplayBundle bundle)
        {
            tempSprites.Clear();

            ViewingSpecificBundle = true;
            currentPageBundle = bundle;

            if(bundle.BundleInfo.IsPurchase)
            {
                SetupPurchaseBundle(bundle);
            }
            else
            {
                SetupRegularBundle(bundle);
            }

            
        }

        private void SetupPurchaseBundle(DisplayBundle bundle)
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

        private void SetupRegularBundle(DisplayBundle bundle)
        {
            var ingredientList = bundle.BundleInfo.Ingredients;
            int numberOfIngredientSlots = bundle.BundleInfo.IngredientsRequired;
            int numberOfPossibleIngredients = ingredientList.Count;

            List<Rectangle> ingredientSlotRectangles = new List<Rectangle>();
            AddRectangleRowsToList(ingredientSlotRectangles, numberOfIngredientSlots, 932, 540);
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
            AddRectangleRowsToList(ingredientListRectangles, numberOfPossibleIngredients, 932, 364);
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
            UpdateIngredientSlots(bundle);
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

        private void UpdateIngredientSlots(DisplayBundle bundle)
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

        private void AddRectangleRowsToList(List<Rectangle> toAddTo, int numberOfItems, int centerX, int centerY)
        {
            switch (numberOfItems)
            {
                case 1:
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY, 1, 72, 72, 12));
                    break;
                case 2:
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY, 2, 72, 72, 12));
                    break;
                case 3:
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY, 3, 72, 72, 12));
                    break;
                case 4:
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY, 4, 72, 72, 12));
                    break;
                case 5:
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 3, 72, 72, 12));
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 2, 72, 72, 12));
                    break;
                case 6:
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 3, 72, 72, 12));
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 3, 72, 72, 12));
                    break;
                case 7:
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 4, 72, 72, 12));
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 3, 72, 72, 12));
                    break;
                case 8:
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 4, 72, 72, 12));
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 4, 72, 72, 12));
                    break;
                case 9:
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 5, 72, 72, 12));
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 4, 72, 72, 12));
                    break;
                case 10:
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 5, 72, 72, 12));
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 5, 72, 72, 12));
                    break;
                case 11:
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 6, 72, 72, 12));
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 5, 72, 72, 12));
                    break;
                case 12:
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 6, 72, 72, 12));
                    toAddTo.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 6, 72, 72, 12));
                    break;
            }
        }

        private List<Rectangle> CreateRowOfBoxesCenteredAt(int xStart, int yStart, int numBoxes, int boxWidth, int boxHeight, int horizontalGap)
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

        private void CompleteCommunityCenter(int areaIndex)
        {
            CommunityCenterHelper.CustomCommunityCenter.areasComplete[areaIndex] = true;
            CommunityCenterHelper.CustomCommunityCenter.areaCompleteReward(areaIndex);

            exitFunction = RestoreAreaOnExit;
        }

        private void RestoreAreaOnExit()
        {
            if(!FromGameMenu)
            {
                (CommunityCenterHelper.CustomCommunityCenter).restoreAreaCutscene(AreaIndex);
            }
        }

        private void CheckIfBundleIsComplete()
        {
            if (!ViewingSpecificBundle || currentPageBundle == null || !currentPageBundle.BundleInfo.Completed) return;

            if (heldItem != null)
            {
                Game1.player.addItemToInventory(heldItem);
                heldItem = null;
            }

            for (int j = 0; j < CommunityCenterHelper.CustomCommunityCenter.bundles[BundleIndex].Length; j++)
            {
                CommunityCenterHelper.CustomCommunityCenter.bundles.FieldDict[BundleIndex][j] = true;
            }

            CommunityCenterHelper.CustomCommunityCenter.checkForNewJunimoNotes();
            screenSwipe = new ScreenSwipe(0, -1f, -1);
            currentPageBundle.CompletionAnimation(this, true, 400);
            canClick = false;

            (CommunityCenterHelper.CustomCommunityCenter).bundleRewards[AreaIndex] = true;
#warning send multiplayer chat message
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
                CommunityCenterHelper.CustomCommunityCenter.areasComplete[AreaIndex] = true;
                CommunityCenterHelper.CustomCommunityCenter.areaCompleteReward(AreaIndex);
                base.exitFunction = RestoreAreaOnExit;
            }
            else
            {
                Junimo i = (CommunityCenterHelper.CustomCommunityCenter).getJunimoForArea(AreaIndex);
                i?.bringBundleBackToHut(Bundle.getColorFromColorIndex(currentPageBundle.BundleColor), CommunityCenterHelper.CustomCommunityCenter);
            }

            CheckForRewards();
        }

        private void CheckForRewards()
        {
            foreach (var bundle in bundles)
            {
                if (bundle.BundleInfo.Completed && !bundle.BundleInfo.Collected)
                {
                    PresentButton = new ClickableAnimatedComponent(new Rectangle(xPositionOnScreen + 592, yPositionOnScreen + 512, 72, 72), "", Game1.content.LoadString("Strings\\StringsFromCSFiles:JunimoNoteMenu.cs.10783"), new TemporaryAnimatedSprite("LooseSprites\\JunimoNote", new Rectangle(548, 262, 18, 20), 70f, 4, 99999, new Vector2(-64f, -64f), false, false, 0.5f, 0f, Color.White, 4f, 0f, 0f, 0f, true));
                    return;
                }
            }

            PresentButton = null;
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

        private void OpenRewardsMenu()
        {
            Game1.playSound("smallSelect");
            List<Item> rewards = new List<Item>();

            var bundleList = bundleGroupList[AreaIndex].Bundles;
            for(int i = 0; i < bundleList.Count; i++)
            {
                if(bundleList[i].Completed && !bundleList[i].Collected)
                {
                    var rewardItem = ObjectFactory.getItemFromDescription((byte)bundleList[i].RewardItemType, bundleList[i].RewardItemId, bundleList[i].RewardItemStack);
                    rewardItem.SpecialVariable = i;
                    rewards.Add(rewardItem);
                }
            }

            Game1.activeClickableMenu = new ItemGrabMenu(rewards, false, true, null, null, null, RewardGrabbed, false, true, true, true, false, 0, null, -1, null)
            {
                exitFunction = (exitFunction ?? new onExit(ReOpenThisMenu))
            };
        }

        private void ReOpenThisMenu()
        {
            Game1.activeClickableMenu = new CustomJunimoNoteMenu(bundleGroupList, AreaIndex, FromGameMenu, true);
        }

        private void RewardGrabbed(Item i, Farmer who)
        {
            bundleGroupList[AreaIndex].Bundles[i.SpecialVariable].Collected = true;
        }

        private void CloseBundlePage()
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

        private Point GetBundleLocationFromNumber(int bundlesAdded)
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
                b.Draw(noteTexture, new Vector2(base.xPositionOnScreen, base.yPositionOnScreen), new Rectangle(0, 0, 320, 180), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
                SpriteText.drawStringHorizontallyCenteredAt(b, ScrambledText ? CustomCommunityCenter.getAreaEnglishDisplayNameFromNumber(AreaIndex) : CustomCommunityCenter.getAreaDisplayNameFromNumber(AreaIndex), base.xPositionOnScreen + base.width / 2 + 16, base.yPositionOnScreen + 12, 999999, -1, 99999, 0.88f, 0.88f, ScrambledText, -1);
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
                b.Draw(noteTexture, new Vector2(base.xPositionOnScreen, base.yPositionOnScreen), new Rectangle(320, 0, 320, 180), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
                if (currentPageBundle != null)
                {
                    b.Draw(noteTexture, new Vector2(base.xPositionOnScreen + 872, base.yPositionOnScreen + 88), new Rectangle(BundleIndex * 16 * 2 % noteTexture.Width, 180 + 32 * (BundleIndex * 16 * 2 / noteTexture.Width), 32, 32), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.15f);
                    float textX = Game1.dialogueFont.MeasureString((!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", currentPageBundle.label)).X;
                    b.Draw(noteTexture, new Vector2(base.xPositionOnScreen + 936 - (int)textX / 2 - 16, base.yPositionOnScreen + 228), new Rectangle(517, 266, 4, 17), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
                    b.Draw(noteTexture, new Rectangle(base.xPositionOnScreen + 936 - (int)textX / 2, base.yPositionOnScreen + 228, (int)textX, 68), new Rectangle(520, 266, 1, 17), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.1f);
                    b.Draw(noteTexture, new Vector2(base.xPositionOnScreen + 936 + (int)textX / 2, base.yPositionOnScreen + 228), new Rectangle(524, 266, 4, 17), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
                    b.DrawString(Game1.dialogueFont, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", currentPageBundle.label), new Vector2(base.xPositionOnScreen + 936 - textX / 2f, base.yPositionOnScreen + 244) + new Vector2(2f, 2f), Game1.textShadowColor);
                    b.DrawString(Game1.dialogueFont, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", currentPageBundle.label), new Vector2(base.xPositionOnScreen + 936 - textX / 2f, base.yPositionOnScreen + 244) + new Vector2(0f, 2f), Game1.textShadowColor);
                    b.DrawString(Game1.dialogueFont, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", currentPageBundle.label), new Vector2(base.xPositionOnScreen + 936 - textX / 2f, base.yPositionOnScreen + 244) + new Vector2(2f, 0f), Game1.textShadowColor);
                    b.DrawString(Game1.dialogueFont, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", currentPageBundle.label), new Vector2(base.xPositionOnScreen + 936 - textX / 2f, base.yPositionOnScreen + 244), Game1.textColor * 0.9f);
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
                    b.Draw(Game1.shadowTexture, new Vector2(x.bounds.Center.X - Game1.shadowTexture.Bounds.Width * 4 / 2 - 4, x.bounds.Center.Y + 4), Game1.shadowTexture.Bounds, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
                    x.drawItem(b, 0, 0);
                });

                inventory.draw(b);
            }

            SpriteText.drawStringWithScrollCenteredAt(b, GetRewardNameForArea(AreaIndex), base.xPositionOnScreen + base.width / 2, Math.Min(base.yPositionOnScreen + base.height + 20, Game1.viewport.Height - 64 - 8), "", 1f, -1, 0, 0.88f, false);
            base.draw(b);
            Game1.mouseCursorTransparency = 1f;

            if (canClick)
            {
                base.drawMouse(b);
            }
            if (heldItem != null)
            {
                heldItem.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 16, Game1.getOldMouseY() + 16), 1f);
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
                bundle.Update(time);
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
                ReOpenThisMenu();
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
                                        heldItem = currentPageBundle.TryToDepositThisItem(heldItem, IngredientSlots[m], "LooseSprites\\JunimoNote");
                                        CheckIfBundleIsComplete();
                                        return;
                                    }
                                }
                            }
                            for (int l = 0; l < IngredientSlots.Count; l++)
                            {
                                if (IngredientSlots[l].containsPoint(x, y) && IngredientSlots[l].item == null)
                                {
                                    heldItem = currentPageBundle.TryToDepositThisItem(heldItem, IngredientSlots[l], "LooseSprites\\JunimoNote");
                                    CheckIfBundleIsComplete();
                                }
                            }
                        }
                        if (PurchaseButton != null && PurchaseButton.containsPoint(x, y))
                        {
                            int moneyRequired = currentPageBundle.BundleInfo.PurchaseAmount;

                            if (Game1.player.Money >= moneyRequired)
                            {
                                Game1.player.Money -= moneyRequired;
                                Game1.playSound("select");
                                currentPageBundle.CompletionAnimation(this, true, 0);
                                if (PurchaseButton != null)
                                {
                                    PurchaseButton.scale = PurchaseButton.baseScale * 0.75f;
                                }

                                currentPageBundle.BundleInfo.PurchaseCompleted();
                                CheckForRewards();

                                if (bundleGroupList[AreaIndex].Completed)
                                {
                                    CompleteCommunityCenter(AreaIndex);
                                }
                                else
                                {
                                    CustomCommunityCenter cc = CommunityCenterHelper.CustomCommunityCenter;
                                    Junimo k = cc.getJunimoForArea(AreaIndex);
                                    k?.bringBundleBackToHut(Bundle.getColorFromColorIndex(currentPageBundle.BundleColor), cc);
                                }
                            }
                            else
                            {
                                Game1.dayTimeMoneyBox.moneyShakeTimer = 600;
                            }
                        }
                        if (base.upperRightCloseButton != null && !readyToClose() && base.upperRightCloseButton.containsPoint(x, y))
                        {
                            CloseBundlePage();
                        }
                    }
                    else
                    {
                        for (int i = 0; i < bundles.Count; i++)
                        {
                            if (bundles[i].CanBeClicked() && bundles[i].containsPoint(x, y))
                            {
                                BundleIndex = i;
                                SetupBundleSpecificPage(bundles[i]);
                                Game1.playSound("shwip");
                                return;
                            }
                        }

                        if (PresentButton != null && PresentButton.containsPoint(x, y) && !FromGameMenu && !FromThisMenu)
                        {
                            OpenRewardsMenu();
                        }
                        if (FromGameMenu)
                        {
                            if (AreaNextButton.containsPoint(x, y))
                            {
                                for (int j = 1; j < 7; j++)
                                {
                                    if (ShouldNoteAppearInArea((AreaIndex + j) % 6))
                                    {
                                        Game1.activeClickableMenu = new CustomJunimoNoteMenu(bundleGroupList, (AreaIndex + j) % 6, FromGameMenu, true);
                                        return;
                                    }
                                }
                            }
                            else if (AreaBackButton.containsPoint(x, y))
                            {
                                int area = AreaIndex;
                                for (int i = 1; i < 7; i++)
                                {
                                    area--;
                                    if (area == -1)
                                    {
                                        area = 5;
                                    }
                                    if (ShouldNoteAppearInArea(area))
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

        private bool ShouldNoteAppearInArea(int area)
        {
            return CommunityCenterHelper.CustomCommunityCenter.shouldNoteAppearInArea(area);
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
                CloseBundlePage();
            }
        }

        public override void receiveGamePadButton(Buttons b)
        {
            base.receiveGamePadButton(b);
            if (FromGameMenu)
            {
                var cc = CommunityCenterHelper.CustomCommunityCenter;
                switch (b)
                {
                    case Buttons.RightTrigger:
                        {
                            int j = 1;
                            while (true)
                            {
                                if (j < 7)
                                {
                                    if (!cc.shouldNoteAppearInArea((AreaIndex + j) % 6))
                                    {
                                        j++;
                                        continue;
                                    }
                                    break;
                                }
                                return;
                            }
                            Game1.activeClickableMenu = new CustomJunimoNoteMenu(bundleGroupList, (AreaIndex + j) % 6, FromGameMenu, true);
                            break;
                        }
                    case Buttons.LeftTrigger:
                        {
                            int area = AreaIndex;
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
                            if (slot.bounds.Contains(x, y) && currentPageBundle.CanAcceptThisItem(heldItem, slot))
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
                        bundle.TryHoverAction(x, y);
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
