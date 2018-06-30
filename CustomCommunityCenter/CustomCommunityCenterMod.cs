using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
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
                Monitor.Log("Config is null...");
                config = new ModConfig();
                config.Enabled = true;
                config.Bundles = GetVanillaCommunityCenterInfo();

                helper.WriteConfig<ModConfig>(config);
            }
            else if(!config.Enabled)
            {
                Monitor.Log("Mod is disabled...");
                config = new ModConfig();
                config.Enabled = true;
                config.Bundles = GetVanillaCommunityCenterInfo();

                helper.WriteConfig<ModConfig>(config);
            }
            else
            {
                Monitor.Log("Resetting config file...");

                config = new ModConfig();
                config.Enabled = true;
                config.Bundles = GetVanillaCommunityCenterInfo();

                helper.WriteConfig<ModConfig>(config);
            }

            helper.ConsoleCommands.Add("show", "Shows Bundle Menu", (string command, string[] arguments) =>
            {
                bool fromGameMenu = false;
                int areaIndex = 0;
                if (arguments.Length == 1 && int.TryParse(arguments[0], out int index)) areaIndex = index;
                if (arguments.Length == 2 && bool.TryParse(arguments[1], out bool temp)) fromGameMenu = temp;

                Game1.player.mailbox.Add("canReadJunimoText");

                CustomJunimoNoteMenu menu = new CustomJunimoNoteMenu(config.Bundles, areaIndex, fromGameMenu, false);
                Game1.activeClickableMenu = menu;
            });

            helper.ConsoleCommands.Add("giveitems", "Gives items for bundle area", (string command, string[] arguments) =>
            {
                if (arguments.Length != 1) return;
                if (!int.TryParse(arguments[0], out int index)) return;

                List<Item> itemList = new List<Item>();
                foreach(var bundle in config.Bundles[index].Bundles)
                {
                    foreach(var ingredient in bundle.Ingredients)
                    {
                        Item item;

                        if (ingredient.ItemId == (int)Objects.Money)
                        {
                            Game1.player.Money += ingredient.RequiredStack;
                            continue;
                        }
                        else if (ingredient.ItemQuality != (int)Quality.Regular) item = new StardewValley.Object(ingredient.ItemId, ingredient.RequiredStack, false, -1, ingredient.ItemQuality);
                        else item = ObjectFactory.getItemFromDescription((byte)ingredient.ItemType, ingredient.ItemId, ingredient.RequiredStack);

                        itemList.Add(item);
                    }
                }

                Monitor.Log("Providing " + itemList.Count + " goodies.");
                if(itemList.Count > 0) Game1.activeClickableMenu = new ItemGrabMenu(itemList);
            });

            Monitor.Log("Initialized");
        }

        protected void removeAndReplaceLocation(GameLocation toRemove, GameLocation toReplace)
        {
            Game1.locations.Remove(toRemove);
            Game1.locations.Add(toReplace);
        }

        public static List<BundleAreaInfo> GetVanillaCommunityCenterInfo()
        {
            List<BundleAreaInfo> bundleAreas = new List<BundleAreaInfo>();

            var craftsRoom = new BundleAreaInfo() { Bundles = new List<BundleInfo>(), Name = "Bridge Repair" };
            var pantry = new BundleAreaInfo() { Bundles = new List<BundleInfo>(), Name = "Greenhouse" };
            var fishTank = new BundleAreaInfo() { Bundles = new List<BundleInfo>(), Name = "Glittering Boulder Removed" };
            var boilerRoom = new BundleAreaInfo() { Bundles = new List<BundleInfo>(), Name = "Minecarts Repaired" };
            var bulletinBoard = new BundleAreaInfo() { Bundles = new List<BundleInfo>(), Name = "Friendship ♡" };
            var vault = new BundleAreaInfo() { Bundles = new List<BundleInfo>(), Name = "Bus Repair" };

            bundleAreas.Add(pantry);
            bundleAreas.Add(craftsRoom);            
            bundleAreas.Add(fishTank);
            bundleAreas.Add(boilerRoom);
            bundleAreas.Add(vault);
            bundleAreas.Add(bulletinBoard);
            

            var springForaging = new BundleInfo();
            springForaging.Name = "Spring Foraging";
            springForaging.Ingredients = new List<BundleIngredientInfo>();
            springForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.WildHorseradish, (int)Quality.Regular, 1));
            springForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.Daffodil, (int)Quality.Regular, 1));
            springForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.Leek, (int)Quality.Regular, 1));
            springForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.Dandelion, (int)Quality.Regular, 1));
            springForaging.IngredientsRequired = 4;
            springForaging.RewardItemType = (int)ItemType.Object;
            springForaging.RewardItemId = (int)Objects.SpringSeeds;
            springForaging.RewardItemStack = 30;
            craftsRoom.Bundles.Add(springForaging);

            var summerForaging = new BundleInfo();
            summerForaging.Name = "Summer Foraging";
            summerForaging.Ingredients = new List<BundleIngredientInfo>();
            summerForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.Grape, (int)Quality.Regular, 1));
            summerForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.SpiceBerry, (int)Quality.Regular, 1));
            summerForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.SweetPea, (int)Quality.Regular, 1));
            summerForaging.IngredientsRequired = 3;
            summerForaging.RewardItemType = (int)ItemType.Object;
            summerForaging.RewardItemId = (int)Objects.SummerSeeds;
            summerForaging.RewardItemStack = 30;
            craftsRoom.Bundles.Add(summerForaging);

            var fallForaging = new BundleInfo();
            fallForaging.Name = "Fall Foraging";
            fallForaging.Ingredients = new List<BundleIngredientInfo>();
            fallForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.CommonMushroom, (int)Quality.Regular, 1));
            fallForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.WildPlum, (int)Quality.Regular, 1));
            fallForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.Hazelnut, (int)Quality.Regular, 1));
            fallForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.Blackberry, (int)Quality.Regular, 1));
            fallForaging.IngredientsRequired = 4;
            fallForaging.RewardItemType = (int)ItemType.Object;
            fallForaging.RewardItemId = (int)Objects.FallSeeds;
            fallForaging.RewardItemStack = 30;
            craftsRoom.Bundles.Add(fallForaging);

            var winterForaging = new BundleInfo();
            winterForaging.Name = "Winter Foraging";
            winterForaging.Ingredients = new List<BundleIngredientInfo>();
            winterForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.WinterRoot, (int)Quality.Regular, 1));
            winterForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.CrystalFruit, (int)Quality.Regular, 1));
            winterForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.SnowYam, (int)Quality.Regular, 1));
            winterForaging.Ingredients.Add(new BundleIngredientInfo((int)Objects.Crocus, (int)Quality.Regular, 1));
            winterForaging.IngredientsRequired = 4;
            winterForaging.RewardItemType = (int)ItemType.Object;
            winterForaging.RewardItemId = (int)Objects.WinterSeeds;
            winterForaging.RewardItemStack = 30;
            craftsRoom.Bundles.Add(winterForaging);

            var construction = new BundleInfo();
            construction.Name = "Construction";
            construction.Ingredients = new List<BundleIngredientInfo>();
            construction.Ingredients.Add(new BundleIngredientInfo((int)Objects.Wood, (int)Quality.Regular, 99));
            construction.Ingredients.Add(new BundleIngredientInfo((int)Objects.Wood, (int)Quality.Regular, 99));
            construction.Ingredients.Add(new BundleIngredientInfo((int)Objects.Stone, (int)Quality.Regular, 99));
            construction.Ingredients.Add(new BundleIngredientInfo((int)Objects.Hardwood, (int)Quality.Regular, 10));
            construction.IngredientsRequired = 4;
            construction.RewardItemType = (int)ItemType.BigCraftable;
            construction.RewardItemId = (int)BigCraftables.CharcoalKiln;
            construction.RewardItemStack = 1;
            craftsRoom.Bundles.Add(construction);

            var exoticForaging = new BundleInfo();
            exoticForaging.Name = "Exotic Foraging";
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
            exoticForaging.RewardItemId = (int)Objects.AutumnsBounty;
            exoticForaging.RewardItemStack = 5;
            craftsRoom.Bundles.Add(exoticForaging);

            var springCrops = new BundleInfo();
            springCrops.Name = "Spring Crops";
            springCrops.Ingredients = new List<BundleIngredientInfo>();
            springCrops.Ingredients.Add(new BundleIngredientInfo((int)Objects.Parsnip, (int)Quality.Regular, 1));
            springCrops.Ingredients.Add(new BundleIngredientInfo((int)Objects.GreenBean, (int)Quality.Regular, 1));
            springCrops.Ingredients.Add(new BundleIngredientInfo((int)Objects.Potato, (int)Quality.Regular, 1));
            springCrops.Ingredients.Add(new BundleIngredientInfo((int)Objects.Cauliflower, (int)Quality.Regular, 1));
            springCrops.IngredientsRequired = 4;
            springCrops.RewardItemType = (int)ItemType.Object;
            springCrops.RewardItemId = (int)Objects.SpeedGro;
            springCrops.RewardItemStack = 20;
            pantry.Bundles.Add(springCrops);

            var summerCrops = new BundleInfo();
            summerCrops.Name = "Summer Crops";
            summerCrops.Ingredients = new List<BundleIngredientInfo>();
            summerCrops.Ingredients.Add(new BundleIngredientInfo((int)Objects.Tomato, (int)Quality.Regular, 1));
            summerCrops.Ingredients.Add(new BundleIngredientInfo((int)Objects.HotPepper, (int)Quality.Regular, 1));
            summerCrops.Ingredients.Add(new BundleIngredientInfo((int)Objects.Blueberry, (int)Quality.Regular, 1));
            summerCrops.Ingredients.Add(new BundleIngredientInfo((int)Objects.Melon, (int)Quality.Regular, 1));
            summerCrops.IngredientsRequired = 4;
            summerCrops.RewardItemType = (int)ItemType.Object;
            summerCrops.RewardItemId = (int)Objects.QualitySprinkler;
            summerCrops.RewardItemStack = 1;
            pantry.Bundles.Add(summerCrops);

            var fallCrops = new BundleInfo();
            fallCrops.Name = "Fall Crops";
            fallCrops.Ingredients = new List<BundleIngredientInfo>();
            fallCrops.Ingredients.Add(new BundleIngredientInfo((int)Objects.Corn, (int)Quality.Regular, 1));
            fallCrops.Ingredients.Add(new BundleIngredientInfo((int)Objects.Eggplant, (int)Quality.Regular, 1));
            fallCrops.Ingredients.Add(new BundleIngredientInfo((int)Objects.Pumpkin, (int)Quality.Regular, 1));
            fallCrops.Ingredients.Add(new BundleIngredientInfo((int)Objects.Yam, (int)Quality.Regular, 1));
            fallCrops.IngredientsRequired = 4;
            fallCrops.RewardItemType = (int)ItemType.BigCraftable;
            fallCrops.RewardItemId = (int)BigCraftables.BeeHouse;
            fallCrops.RewardItemStack = 1;
            pantry.Bundles.Add(fallCrops);

            var qualityCrops = new BundleInfo();
            qualityCrops.Name = "Quality Crops";
            qualityCrops.Ingredients = new List<BundleIngredientInfo>();
            qualityCrops.Ingredients.Add(new BundleIngredientInfo((int)Objects.Parsnip, (int)Quality.Gold, 5));
            qualityCrops.Ingredients.Add(new BundleIngredientInfo((int)Objects.Melon, (int)Quality.Gold, 5));
            qualityCrops.Ingredients.Add(new BundleIngredientInfo((int)Objects.Pumpkin, (int)Quality.Gold, 5));
            qualityCrops.Ingredients.Add(new BundleIngredientInfo((int)Objects.Corn, (int)Quality.Gold, 5));
            qualityCrops.IngredientsRequired = 3;
            qualityCrops.RewardItemType = (int)ItemType.BigCraftable;
            qualityCrops.RewardItemId = (int)BigCraftables.PreservesJar;
            qualityCrops.RewardItemStack = 1;
            pantry.Bundles.Add(qualityCrops);

            var animalProducts = new BundleInfo();
            animalProducts.Name = "Animal Products";
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
            animalProducts.RewardItemStack = 1;
            pantry.Bundles.Add(animalProducts);

            var artisan = new BundleInfo();
            artisan.Name = "Artisan";
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
            artisan.RewardItemStack = 1;
            pantry.Bundles.Add(artisan);

            var riverFish = new BundleInfo();
            riverFish.Name = "River Fish";
            riverFish.Ingredients = new List<BundleIngredientInfo>();
            riverFish.Ingredients.Add(new BundleIngredientInfo((int)Objects.Sunfish, (int)Quality.Regular, 1));
            riverFish.Ingredients.Add(new BundleIngredientInfo((int)Objects.Catfish, (int)Quality.Regular, 1));
            riverFish.Ingredients.Add(new BundleIngredientInfo((int)Objects.Shad, (int)Quality.Regular, 1));
            riverFish.Ingredients.Add(new BundleIngredientInfo((int)Objects.TigerTrout, (int)Quality.Regular, 1));
            riverFish.IngredientsRequired = 4;
            riverFish.RewardItemType = (int)ItemType.Object;
            riverFish.RewardItemId = (int)Objects.Bait;
            riverFish.RewardItemStack = 30;
            fishTank.Bundles.Add(riverFish);

            var lakeFish = new BundleInfo();
            lakeFish.Name = "Lake Fish";
            lakeFish.Ingredients = new List<BundleIngredientInfo>();
            lakeFish.Ingredients.Add(new BundleIngredientInfo((int)Objects.LargemouthBass, (int)Quality.Regular, 1));
            lakeFish.Ingredients.Add(new BundleIngredientInfo((int)Objects.Carp, (int)Quality.Regular, 1));
            lakeFish.Ingredients.Add(new BundleIngredientInfo((int)Objects.Bullhead, (int)Quality.Regular, 1));
            lakeFish.Ingredients.Add(new BundleIngredientInfo((int)Objects.Sturgeon, (int)Quality.Regular, 1));
            lakeFish.IngredientsRequired = 4;
            lakeFish.RewardItemType = (int)ItemType.Object;
            lakeFish.RewardItemId = (int)Objects.DressedSpinner;
            lakeFish.RewardItemStack = 1;
            fishTank.Bundles.Add(lakeFish);

            var oceanFish = new BundleInfo();
            oceanFish.Name = "Ocean Fish";
            oceanFish.Ingredients = new List<BundleIngredientInfo>();
            oceanFish.Ingredients.Add(new BundleIngredientInfo((int)Objects.Sardine, (int)Quality.Regular, 1));
            oceanFish.Ingredients.Add(new BundleIngredientInfo((int)Objects.Tuna, (int)Quality.Regular, 1));
            oceanFish.Ingredients.Add(new BundleIngredientInfo((int)Objects.RedSnapper, (int)Quality.Regular, 1));
            oceanFish.Ingredients.Add(new BundleIngredientInfo((int)Objects.Tilapia, (int)Quality.Regular, 1));
            oceanFish.IngredientsRequired = 4;
            oceanFish.RewardItemType = (int)ItemType.Object;
            oceanFish.RewardItemId = (int)Objects.WarpTotemBeach;
            oceanFish.RewardItemStack = 5;
            fishTank.Bundles.Add(oceanFish);

            var nightFish = new BundleInfo();
            nightFish.Name = "Night Fishing";
            nightFish.Ingredients = new List<BundleIngredientInfo>();
            nightFish.Ingredients.Add(new BundleIngredientInfo((int)Objects.Walleye, (int)Quality.Regular, 1));
            nightFish.Ingredients.Add(new BundleIngredientInfo((int)Objects.Bream, (int)Quality.Regular, 1));
            nightFish.Ingredients.Add(new BundleIngredientInfo((int)Objects.Eel, (int)Quality.Regular, 1));
            nightFish.IngredientsRequired = 3;
            nightFish.RewardItemType = (int)ItemType.Object;
            nightFish.RewardItemId = (int)Objects.SmallGlowRing;
            nightFish.RewardItemStack = 1;
            fishTank.Bundles.Add(nightFish);

            var crabPot = new BundleInfo();
            crabPot.Name = "Crab Pot";
            crabPot.Ingredients = new List<BundleIngredientInfo>();
            crabPot.Ingredients.Add(new BundleIngredientInfo((int)Objects.Lobster, (int)Quality.Regular, 1));
            crabPot.Ingredients.Add(new BundleIngredientInfo((int)Objects.Crayfish, (int)Quality.Regular, 1));
            crabPot.Ingredients.Add(new BundleIngredientInfo((int)Objects.Crab, (int)Quality.Regular, 1));
            crabPot.Ingredients.Add(new BundleIngredientInfo((int)Objects.Cockle, (int)Quality.Regular, 1));
            crabPot.Ingredients.Add(new BundleIngredientInfo((int)Objects.Mussel, (int)Quality.Regular, 1));
            crabPot.Ingredients.Add(new BundleIngredientInfo((int)Objects.Shrimp, (int)Quality.Regular, 1));
            crabPot.Ingredients.Add(new BundleIngredientInfo((int)Objects.Snail, (int)Quality.Regular, 1));
            crabPot.Ingredients.Add(new BundleIngredientInfo((int)Objects.Periwinkle, (int)Quality.Regular, 1));
            crabPot.Ingredients.Add(new BundleIngredientInfo((int)Objects.Oyster, (int)Quality.Regular, 1));
            crabPot.Ingredients.Add(new BundleIngredientInfo((int)Objects.Clam, (int)Quality.Regular, 1));
            crabPot.IngredientsRequired = 5;
            crabPot.RewardItemType = (int)ItemType.Object;
            crabPot.RewardItemId = (int)Objects.CrabPot;
            crabPot.RewardItemStack = 3;
            fishTank.Bundles.Add(crabPot);

            var specialtyFish = new BundleInfo();
            specialtyFish.Name = "Specialty Fish";
            specialtyFish.Ingredients = new List<BundleIngredientInfo>();
            specialtyFish.Ingredients.Add(new BundleIngredientInfo((int)Objects.Pufferfish, (int)Quality.Regular, 1));
            specialtyFish.Ingredients.Add(new BundleIngredientInfo((int)Objects.Ghostfish, (int)Quality.Regular, 1));
            specialtyFish.Ingredients.Add(new BundleIngredientInfo((int)Objects.Sandfish, (int)Quality.Regular, 1));
            specialtyFish.Ingredients.Add(new BundleIngredientInfo((int)Objects.Woodskip, (int)Quality.Regular, 1));
            specialtyFish.IngredientsRequired = 4;
            specialtyFish.RewardItemType = (int)ItemType.Object;
            specialtyFish.RewardItemId = (int)Objects.DishOTheSea;
            specialtyFish.RewardItemStack = 5;
            fishTank.Bundles.Add(specialtyFish);

            var blacksmiths = new BundleInfo();
            blacksmiths.Name = "Blacksmith's";
            blacksmiths.Ingredients = new List<BundleIngredientInfo>();
            blacksmiths.Ingredients.Add(new BundleIngredientInfo((int)Objects.CopperBar, (int)Quality.Regular, 1));
            blacksmiths.Ingredients.Add(new BundleIngredientInfo((int)Objects.IronBar, (int)Quality.Regular, 1));
            blacksmiths.Ingredients.Add(new BundleIngredientInfo((int)Objects.GoldBar, (int)Quality.Regular, 1));
            blacksmiths.IngredientsRequired = 3;
            blacksmiths.RewardItemType = (int)ItemType.BigCraftable;
            blacksmiths.RewardItemId = (int)BigCraftables.Furnace;
            blacksmiths.RewardItemStack = 1;
            boilerRoom.Bundles.Add(blacksmiths);

            var geologists = new BundleInfo();
            geologists.Name = "Geologists's";
            geologists.Ingredients = new List<BundleIngredientInfo>();
            geologists.Ingredients.Add(new BundleIngredientInfo((int)Objects.Quartz, (int)Quality.Regular, 1));
            geologists.Ingredients.Add(new BundleIngredientInfo((int)Objects.EarthCrystal, (int)Quality.Regular, 1));
            geologists.Ingredients.Add(new BundleIngredientInfo((int)Objects.FrozenTear, (int)Quality.Regular, 1));
            geologists.Ingredients.Add(new BundleIngredientInfo((int)Objects.FireQuartz, (int)Quality.Regular, 1));
            geologists.IngredientsRequired = 4;
            geologists.RewardItemType = (int)ItemType.Object;
            geologists.RewardItemId = (int)Objects.OmniGeode;
            geologists.RewardItemStack = 5;
            boilerRoom.Bundles.Add(geologists);

            var adventurers = new BundleInfo();
            adventurers.Name = "Adventurers's";
            adventurers.Ingredients = new List<BundleIngredientInfo>();
            adventurers.Ingredients.Add(new BundleIngredientInfo((int)Objects.Slime, (int)Quality.Regular, 99));
            adventurers.Ingredients.Add(new BundleIngredientInfo((int)Objects.BatWing, (int)Quality.Regular, 10));
            adventurers.Ingredients.Add(new BundleIngredientInfo((int)Objects.SolarEssence, (int)Quality.Regular, 1));
            adventurers.Ingredients.Add(new BundleIngredientInfo((int)Objects.VoidEssence, (int)Quality.Regular, 1));
            adventurers.IngredientsRequired = 2;
            adventurers.RewardItemType = (int)ItemType.Object;
            adventurers.RewardItemId = (int)Objects.SmallMagnetRing;
            adventurers.RewardItemStack = 1;
            boilerRoom.Bundles.Add(adventurers);

            var chefs = new BundleInfo();
            chefs.Name = "Chef's";
            chefs.Ingredients = new List<BundleIngredientInfo>();
            chefs.Ingredients.Add(new BundleIngredientInfo((int)Objects.MapleSyrup, (int)Quality.Regular, 1));
            chefs.Ingredients.Add(new BundleIngredientInfo((int)Objects.FiddleheadFern, (int)Quality.Regular, 1));
            chefs.Ingredients.Add(new BundleIngredientInfo((int)Objects.Truffle, (int)Quality.Regular, 1));
            chefs.Ingredients.Add(new BundleIngredientInfo((int)Objects.Poppy, (int)Quality.Regular, 1));
            chefs.Ingredients.Add(new BundleIngredientInfo((int)Objects.MakiRoll, (int)Quality.Regular, 1));
            chefs.Ingredients.Add(new BundleIngredientInfo((int)Objects.FriedEgg, (int)Quality.Regular, 1));
            chefs.IngredientsRequired = 6;
            chefs.RewardItemType = (int)ItemType.Object;
            chefs.RewardItemId = (int)Objects.PinkCake;
            chefs.RewardItemStack = 3;
            bulletinBoard.Bundles.Add(chefs);

            var dye = new BundleInfo();
            dye.Name = "Dye";
            dye.Ingredients = new List<BundleIngredientInfo>();
            dye.Ingredients.Add(new BundleIngredientInfo((int)Objects.RedMushroom, (int)Quality.Regular, 1));
            dye.Ingredients.Add(new BundleIngredientInfo((int)Objects.SeaUrchin, (int)Quality.Regular, 1));
            dye.Ingredients.Add(new BundleIngredientInfo((int)Objects.Sunflower, (int)Quality.Regular, 1));
            dye.Ingredients.Add(new BundleIngredientInfo((int)Objects.DuckFeather, (int)Quality.Regular, 1));
            dye.Ingredients.Add(new BundleIngredientInfo((int)Objects.Aquamarine, (int)Quality.Regular, 1));
            dye.Ingredients.Add(new BundleIngredientInfo((int)Objects.RedCabbage, (int)Quality.Regular, 1));
            dye.IngredientsRequired = 6;
            dye.RewardItemType = (int)ItemType.BigCraftable;
            dye.RewardItemId = (int)BigCraftables.SeedMaker;
            dye.RewardItemStack = 1;
            bulletinBoard.Bundles.Add(dye);

            var fieldResearch = new BundleInfo();
            fieldResearch.Name = "Field Research";
            fieldResearch.Ingredients = new List<BundleIngredientInfo>();
            fieldResearch.Ingredients.Add(new BundleIngredientInfo((int)Objects.PurpleMushroom, (int)Quality.Regular, 1));
            fieldResearch.Ingredients.Add(new BundleIngredientInfo((int)Objects.NautilusShell, (int)Quality.Regular, 1));
            fieldResearch.Ingredients.Add(new BundleIngredientInfo((int)Objects.Chub, (int)Quality.Regular, 1));
            fieldResearch.Ingredients.Add(new BundleIngredientInfo((int)Objects.FrozenGeode, (int)Quality.Regular, 1));
            fieldResearch.IngredientsRequired = 4;
            fieldResearch.RewardItemType = (int)ItemType.BigCraftable;
            fieldResearch.RewardItemId = (int)BigCraftables.RecyclingMachine;
            fieldResearch.RewardItemStack = 1;
            bulletinBoard.Bundles.Add(fieldResearch);

            var fodder = new BundleInfo();
            fodder.Name = "Fodder";
            fodder.Ingredients = new List<BundleIngredientInfo>();
            fodder.Ingredients.Add(new BundleIngredientInfo((int)Objects.Wheat, (int)Quality.Regular, 10));
            fodder.Ingredients.Add(new BundleIngredientInfo((int)Objects.Hay, (int)Quality.Regular, 10));
            fodder.Ingredients.Add(new BundleIngredientInfo((int)Objects.Apple, (int)Quality.Regular, 3));
            fodder.IngredientsRequired = 3;
            fodder.RewardItemType = (int)ItemType.BigCraftable;
            fodder.RewardItemId = (int)BigCraftables.Heater;
            fodder.RewardItemStack = 1;
            bulletinBoard.Bundles.Add(fodder);

            var enchanters = new BundleInfo();
            enchanters.Name = "Enchanter's";
            enchanters.Ingredients = new List<BundleIngredientInfo>();
            enchanters.Ingredients.Add(new BundleIngredientInfo((int)Objects.OakResin, (int)Quality.Regular, 1));
            enchanters.Ingredients.Add(new BundleIngredientInfo((int)Objects.Wine, (int)Quality.Regular, 1));
            enchanters.Ingredients.Add(new BundleIngredientInfo((int)Objects.RabbitsFoot, (int)Quality.Regular, 1));
            enchanters.Ingredients.Add(new BundleIngredientInfo((int)Objects.Pomegranate, (int)Quality.Regular, 1));
            enchanters.IngredientsRequired = 4;
            enchanters.RewardItemType = (int)ItemType.Object;
            enchanters.RewardItemId = (int)Objects.GoldBar;
            enchanters.RewardItemStack = 5;
            bulletinBoard.Bundles.Add(enchanters);

            var vault2500 = new BundleInfo();
            vault2500.Name = "2,500";
            vault2500.IsPurchase = true;
            vault2500.Ingredients = new List<BundleIngredientInfo>();
            vault2500.Ingredients.Add(new BundleIngredientInfo((int)Objects.Money, (int)Quality.Regular, 2500));
            vault2500.IngredientsRequired = 1;
            vault2500.RewardItemType = (int)ItemType.Object;
            vault2500.RewardItemId = (int)Objects.ChocolateCake;
            vault2500.RewardItemStack = 3;
            vault.Bundles.Add(vault2500);

            var vault5000 = new BundleInfo();
            vault5000.Name = "5,000";
            vault5000.IsPurchase = true;
            vault5000.Ingredients = new List<BundleIngredientInfo>();
            vault5000.Ingredients.Add(new BundleIngredientInfo((int)Objects.Money, (int)Quality.Regular, 5000));
            vault5000.IngredientsRequired = 1;
            vault5000.RewardItemType = (int)ItemType.Object;
            vault5000.RewardItemId = (int)Objects.QualityFertilizer;
            vault5000.RewardItemStack = 30;
            vault.Bundles.Add(vault5000);

            var vault10000 = new BundleInfo();
            vault10000.Name = "10,000";
            vault10000.IsPurchase = true;
            vault10000.Ingredients = new List<BundleIngredientInfo>();
            vault10000.Ingredients.Add(new BundleIngredientInfo((int)Objects.Money, (int)Quality.Regular, 10000));
            vault10000.IngredientsRequired = 1;
            vault10000.RewardItemType = (int)ItemType.BigCraftable;
            vault10000.RewardItemId = (int)BigCraftables.LightningRod;
            vault10000.RewardItemStack = 1;
            vault.Bundles.Add(vault10000);

            var vault25000 = new BundleInfo();
            vault25000.Name = "25,000";
            vault25000.IsPurchase = true;
            vault25000.Ingredients = new List<BundleIngredientInfo>();
            vault25000.Ingredients.Add(new BundleIngredientInfo((int)Objects.Money, (int)Quality.Regular, 25000));
            vault25000.IngredientsRequired = 1;
            vault25000.RewardItemType = (int)ItemType.BigCraftable;
            vault25000.RewardItemId = (int)BigCraftables.Crystalarium;
            vault25000.RewardItemStack = 1;
            vault.Bundles.Add(vault25000);

            return bundleAreas;
        }
    }
}
