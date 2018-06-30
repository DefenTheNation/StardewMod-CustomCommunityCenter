using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace CustomCommunityCenter
{
    public class CustomCommunityCenterMod : Mod
    {
        protected string bundleTextureName = "LooseSprites\\JunimoNote";

        public override void Entry(IModHelper helper)
        {
            var config = helper.ReadConfig<ModConfig>();
            if(config == null)
            {
                config = new ModConfig();
                config.Bundles = GetVanillaCommunityCenterInfo();

                helper.WriteConfig<ModConfig>(config);
            }

            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
        }
    

        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            int bundleIndex = 1;
            string rawBundleInfo = "";
            bool[] completedIngredientList = { false, false, false, false };

            CustomJunimoNoteMenu noteMenu = new CustomJunimoNoteMenu(null,0, false);

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

        public static List<BundleAreaInfo> GetVanillaCommunityCenterInfo()
        {
            List<BundleAreaInfo> bundleAreas = new List<BundleAreaInfo>();

            var craftsRoom = new BundleAreaInfo() { Bundles = new List<BundleInfo>() };
            var pantry = new BundleAreaInfo() { Bundles = new List<BundleInfo>() };
            var fishTank = new BundleAreaInfo() { Bundles = new List<BundleInfo>() };
            var boilerRoom = new BundleAreaInfo() { Bundles = new List<BundleInfo>() };
            var bulletinBoard = new BundleAreaInfo() { Bundles = new List<BundleInfo>() };
            var vault = new BundleAreaInfo() { Bundles = new List<BundleInfo>() };

            bundleAreas.Add(craftsRoom);
            bundleAreas.Add(pantry);
            bundleAreas.Add(fishTank);
            bundleAreas.Add(boilerRoom);
            bundleAreas.Add(bulletinBoard);
            bundleAreas.Add(vault);

            var springForaging = new BundleInfo();
            springForaging.Ingredients = new List<BundleIngredientInfo>();
            springForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.WildHorseradish, (int)Quality.Regular, 1));
            springForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.Daffodil, (int)Quality.Regular, 1));
            springForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.Leek, (int)Quality.Regular, 1));
            springForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.Dandelion, (int)Quality.Regular, 1));
            springForaging.IngredientsRequired = 4;
            springForaging.RewardItemType = (int)ItemType.Object;
            springForaging.RewardItemId = (int)Objects.SpringSeeds;
            springForaging.RewardStack = 30;
            craftsRoom.Bundles.Add(springForaging);

            var summerForaging = new BundleInfo();
            summerForaging.Ingredients = new List<BundleIngredientInfo>();
            summerForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.Grape, (int)Quality.Regular, 1));
            summerForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.SpiceBerry, (int)Quality.Regular, 1));
            summerForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.SweetPea, (int)Quality.Regular, 1));
            summerForaging.IngredientsRequired = 3;
            summerForaging.RewardItemType = (int)ItemType.Object;
            summerForaging.RewardItemId = (int)Objects.SummerSeeds;
            summerForaging.RewardStack = 30;
            craftsRoom.Bundles.Add(summerForaging);

            var fallForaging = new BundleInfo();
            fallForaging.Ingredients = new List<BundleIngredientInfo>();
            fallForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.CommonMushroom, (int)Quality.Regular, 1));
            fallForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.WildPlum, (int)Quality.Regular, 1));
            fallForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.Hazelnut, (int)Quality.Regular, 1));
            fallForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.Blackberry, (int)Quality.Regular, 1));
            fallForaging.IngredientsRequired = 4;
            fallForaging.RewardItemType = (int)ItemType.Object;
            fallForaging.RewardItemId = (int)Objects.FallSeeds;
            fallForaging.RewardStack = 30;
            craftsRoom.Bundles.Add(fallForaging);

            var winterForaging = new BundleInfo();
            winterForaging.Ingredients = new List<BundleIngredientInfo>();
            winterForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.WinterRoot, (int)Quality.Regular, 1));
            winterForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.CrystalFruit, (int)Quality.Regular, 1));
            winterForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.SnowYam, (int)Quality.Regular, 1));
            winterForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.Crocus, (int)Quality.Regular, 1));
            winterForaging.IngredientsRequired = 4;
            winterForaging.RewardItemType = (int)ItemType.Object;
            winterForaging.RewardItemId = (int)Objects.WinterSeeds;
            winterForaging.RewardStack = 30;
            craftsRoom.Bundles.Add(winterForaging);

            var construction = new BundleInfo();
            construction.Ingredients = new List<BundleIngredientInfo>();
            construction.Ingredients.Add(new BundleIngredientInfo((int)Objects.Wood, (int)Quality.Regular, 99));
            construction.Ingredients.Add(new BundleIngredientInfo((int)Objects.Wood, (int)Quality.Regular, 99));
            construction.Ingredients.Add(new BundleIngredientInfo((int)Objects.Stone, (int)Quality.Regular, 99));
            construction.Ingredients.Add(new BundleIngredientInfo((int)Objects.Hardwood, (int)Quality.Regular, 10));
            construction.IngredientsRequired = 4;
            construction.RewardItemType = (int)ItemType.BigCraftable;
            construction.RewardItemId = (int)BigCraftables.CharcoalKiln;
            construction.RewardStack = 1;
            craftsRoom.Bundles.Add(construction);

            var exoticForaging = new BundleInfo();
            exoticForaging.Ingredients = new List<BundleIngredientInfo>();
            exoticForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.Coconut, (int)Quality.Regular, 1));
            exoticForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.CactusFruit, (int)Quality.Regular, 1));
            exoticForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.CaveCarrot, (int)Quality.Regular, 1));
            exoticForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.RedMushroom, (int)Quality.Regular, 1));
            exoticForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.PurpleMushroom, (int)Quality.Regular, 1));
            exoticForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.MapleSyrup, (int)Quality.Regular, 1));
            exoticForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.OakResin, (int)Quality.Regular, 1));
            exoticForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.PineTar, (int)Quality.Regular, 1));
            exoticForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.Morel, (int)Quality.Regular, 1));
            exoticForaging.IngredientsRequired = 5;
            exoticForaging.RewardItemType = (int)ItemType.Object;
            exoticForaging.RewardItemId = (int)Objects.SpringSeeds;
            exoticForaging.RewardStack = 30;
            craftsRoom.Bundles.Add(exoticForaging);

            var springCrops = new BundleInfo();
            springCrops.Ingredients = new List<BundleIngredientInfo>();
            springCrops.Ingredients.Add(new BundleIngredientInfo((int)Objects.Parsnip, (int)Quality.Regular, 1));
            springCrops.Ingredients.Add(new BundleIngredientInfo((int)Objects.GreenBean, (int)Quality.Regular, 1));
            springCrops.Ingredients.Add(new BundleIngredientInfo((int)Objects.Potato, (int)Quality.Regular, 1));
            springCrops.Ingredients.Add(new BundleIngredientInfo((int)Objects.Cauliflower, (int)Quality.Regular, 1));
            springCrops.IngredientsRequired = 4;
            springCrops.RewardItemType = (int)ItemType.Object;
            springCrops.RewardItemId = (int)Objects.SpeedGro;
            springCrops.RewardStack = 20;
            pantry.Bundles.Add(springCrops);

            var summerCrops = new BundleInfo();
            summerCrops.Ingredients = new List<BundleIngredientInfo>();
            summerCrops.Ingredients.Add(new BundleIngredientInfo((int)Objects.Tomato, (int)Quality.Regular, 1));
            summerCrops.Ingredients.Add(new BundleIngredientInfo((int)Objects.HotPepper, (int)Quality.Regular, 1));
            summerCrops.Ingredients.Add(new BundleIngredientInfo((int)Objects.Blueberry, (int)Quality.Regular, 1));
            summerCrops.Ingredients.Add(new BundleIngredientInfo((int)Objects.Melon, (int)Quality.Regular, 1));
            summerCrops.IngredientsRequired = 4;
            summerCrops.RewardItemType = (int)ItemType.Object;
            summerCrops.RewardItemId = (int)Objects.QualitySprinkler;
            summerCrops.RewardStack = 1;
            pantry.Bundles.Add(summerCrops);

            var fallCrops = new BundleInfo();
            fallCrops.Ingredients = new List<BundleIngredientInfo>();
            fallCrops.Ingredients.Add(new BundleIngredientInfo((int)Objects.Corn, (int)Quality.Regular, 1));
            fallCrops.Ingredients.Add(new BundleIngredientInfo((int)Objects.Eggplant, (int)Quality.Regular, 1));
            fallCrops.Ingredients.Add(new BundleIngredientInfo((int)Objects.Pumpkin, (int)Quality.Regular, 1));
            fallCrops.Ingredients.Add(new BundleIngredientInfo((int)Objects.Yam, (int)Quality.Regular, 1));
            fallCrops.IngredientsRequired = 4;
            fallCrops.RewardItemType = (int)ItemType.BigCraftable;
            fallCrops.RewardItemId = (int)BigCraftables.BeeHouse;
            fallCrops.RewardStack = 1;
            pantry.Bundles.Add(fallCrops);

            var qualityCrops = new BundleInfo();
            qualityCrops.Ingredients = new List<BundleIngredientInfo>();
            qualityCrops.Ingredients.Add(new BundleIngredientInfo((int)Objects.Parsnip, (int)Quality.Gold, 5));
            qualityCrops.Ingredients.Add(new BundleIngredientInfo((int)Objects.Melon, (int)Quality.Gold, 5));
            qualityCrops.Ingredients.Add(new BundleIngredientInfo((int)Objects.Pumpkin, (int)Quality.Gold, 5));
            qualityCrops.Ingredients.Add(new BundleIngredientInfo((int)Objects.Corn, (int)Quality.Gold, 5));
            qualityCrops.IngredientsRequired = 3;
            qualityCrops.RewardItemType = (int)ItemType.BigCraftable;
            qualityCrops.RewardItemId = (int)BigCraftables.PreservesJar;
            qualityCrops.RewardStack = 1;
            pantry.Bundles.Add(qualityCrops);

            var animalProducts = new BundleInfo();
            animalProducts.Ingredients = new List<BundleIngredientInfo>();
            animalProducts.Ingredients.Add(new BundleIngredientInfo((int)Objects.LargeMilk, (int)Quality.Regular, 1));
            animalProducts.Ingredients.Add(new BundleIngredientInfo((int)Objects.LargeWhiteEgg, (int)Quality.Regular, 1));
            animalProducts.Ingredients.Add(new BundleIngredientInfo((int)Objects.LargeBrownEgg, (int)Quality.Regular, 1));
            animalProducts.Ingredients.Add(new BundleIngredientInfo((int)Objects.LGoatMilk, (int)Quality.Regular, 1));
            animalProducts.Ingredients.Add(new BundleIngredientInfo((int)Objects.Wool, (int)Quality.Regular, 1));
            animalProducts.Ingredients.Add(new BundleIngredientInfo((int)Objects.DuckEgg, (int)Quality.Regular, 1));
            animalProducts.IngredientsRequired = 5;
            animalProducts.RewardItemType = (int)ItemType.BigCraftable;
            animalProducts.RewardItemId = (int)BigCraftables.CheesePress;
            animalProducts.RewardStack = 1;
            pantry.Bundles.Add(animalProducts);

            var artisan = new BundleInfo();
            artisan.Ingredients = new List<BundleIngredientInfo>();
            artisan.Ingredients.Add(new BundleIngredientInfo((int)Objects.TruffleOil, (int)Quality.Regular, 1));
            artisan.Ingredients.Add(new BundleIngredientInfo((int)Objects.Cloth, (int)Quality.Regular, 1));
            artisan.Ingredients.Add(new BundleIngredientInfo((int)Objects.GoatCheese, (int)Quality.Regular, 1));
            artisan.Ingredients.Add(new BundleIngredientInfo((int)Objects.Cheese, (int)Quality.Regular, 1));
            artisan.Ingredients.Add(new BundleIngredientInfo((int)Objects.Honey, (int)Quality.Regular, 1));
            artisan.Ingredients.Add(new BundleIngredientInfo((int)Objects.Jelly, (int)Quality.Regular, 1));
            artisan.Ingredients.Add(new BundleIngredientInfo((int)Objects.Apple, (int)Quality.Regular, 1));
            artisan.Ingredients.Add(new BundleIngredientInfo((int)Objects.Apricot, (int)Quality.Regular, 1));
            artisan.Ingredients.Add(new BundleIngredientInfo((int)Objects.Orange, (int)Quality.Regular, 1));
            artisan.Ingredients.Add(new BundleIngredientInfo((int)Objects.Peach, (int)Quality.Regular, 1));
            artisan.Ingredients.Add(new BundleIngredientInfo((int)Objects.Pomegranate, (int)Quality.Regular, 1));
            artisan.Ingredients.Add(new BundleIngredientInfo((int)Objects.Cherry, (int)Quality.Regular, 1));
            artisan.IngredientsRequired = 6;
            artisan.RewardItemType = (int)ItemType.BigCraftable;
            artisan.RewardItemId = (int)BigCraftables.Keg;
            artisan.RewardStack = 1;
            pantry.Bundles.Add(artisan);

            var riverFish = new BundleInfo();
            riverFish.Ingredients = new List<BundleIngredientInfo>();

            return bundleAreas;
        }
    }
}
