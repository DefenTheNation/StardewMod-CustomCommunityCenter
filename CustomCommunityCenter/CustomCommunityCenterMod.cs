using CustomCommunityCenter.API;
using CustomCommunityCenter.Data;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System.Collections.Generic;

namespace CustomCommunityCenter
{
    public class CustomCommunityCenterMod : Mod
    {
        protected string bundleTextureName = "LooseSprites\\JunimoNote";

        public CommunityCenterHelper ModAPI { get; set; }

        public override void Entry(IModHelper helper)
        {
            var config = helper.ReadConfig<ModConfig>();
            if(config == null || config.BundleRooms == null)
            {
                Monitor.Log("Warning: No config found. Generating vanilla config...");
                config = new ModConfig
                {
                    Enabled = true,
                    BundleRooms = GetVanillaCommunityCenterInfo()
                };

                helper.WriteConfig(config);
            }
            else if(!config.Enabled)
            {
                Monitor.Log("Mod is disabled");
                return;
            }

            ModAPI = new CommunityCenterHelper(helper, config);
            CommunityCenterHelper.Helper = ModAPI;

            helper.ConsoleCommands.Add("show", "Shows Bundle Menu", (string command, string[] arguments) =>
            {
                bool fromGameMenu = false;
                int areaIndex = 0;
                if (arguments.Length == 1 && int.TryParse(arguments[0], out int index)) areaIndex = index;
                if (arguments.Length == 2 && bool.TryParse(arguments[1], out bool temp)) fromGameMenu = temp;

                Game1.player.mailbox.Add("canReadJunimoText");

                CustomJunimoNoteMenu menu = new CustomJunimoNoteMenu(config.BundleRooms, areaIndex, fromGameMenu, false);
                Game1.activeClickableMenu = menu;
            });

            helper.ConsoleCommands.Add("giveitems", "Gives items for bundle area", (string command, string[] arguments) =>
            {
                if (arguments.Length != 1)
                {
                    Monitor.Log("Usage: giveitems intBundleIndex", LogLevel.Warn);
                    return;
                }
                if (!int.TryParse(arguments[0], out int index))
                {
                    Monitor.Log("Usage: giveitems intBundleIndex\nex: giveitems 0 ", LogLevel.Warn);
                    return;
                }
                if (index < 0 || config.BundleRooms.Count <= index)
                {
                    Monitor.Log("Warning: Bundle Index outside bounds of bundle array. Use index between 0 and " + (config.BundleRooms.Count - 1) + " (inclusive)");
                    return;
                }

                bool itemsAdded = false;
                int cashAdded = 0;
                List<Item> itemList = new List<Item>();
                foreach(var bundle in config.BundleRooms[index].Bundles)
                {
                    foreach(var ingredient in bundle.Ingredients)
                    {
                        Item item;
                        itemsAdded = true;

                        if (ingredient.ItemId == (int)Objects.Money)
                        {
                            cashAdded += ingredient.RequiredStack;
                            Game1.player.Money += ingredient.RequiredStack;
                            continue;
                        }
                        else if (ingredient.ItemQuality != (int)Quality.Regular) item = new StardewValley.Object(ingredient.ItemId, ingredient.RequiredStack, false, -1, ingredient.ItemQuality);
                        else item = ObjectFactory.getItemFromDescription((byte)ingredient.ItemType, ingredient.ItemId, ingredient.RequiredStack);

                        itemList.Add(item);
                    }
                }

                
                if (itemList.Count > 0)
                {
                    Monitor.Log("Providing " + itemList.Count + " goodies.", LogLevel.Info);
                    Game1.activeClickableMenu = new ItemGrabMenu(itemList);
                }
                else if(itemsAdded && cashAdded > 0)
                {
                    Monitor.Log("Providing " + cashAdded + " gold", LogLevel.Info);
                }
                else
                {
                    Monitor.Log("Warning: bundle was empty, no rewards provided", LogLevel.Warn);
                }
            });

            helper.ConsoleCommands.Add("shownet", "Shows Bundle Menu", (string command, string[] arguments) =>
            {
                int count = 0;
                string output = "";
                foreach(var item in CommunityCenterHelper.WorldState.Value.Bundles.Values)
                {
                    output = "Bundle id " + count + ":\n";
                    for(int i = 0; i < item.Length; i++)
                    {
                        output += i + ": " + item[i] + "\n";
                    }
                    output += "\n";
                    count++;

                    Monitor.Log(output);
                }

                count = 0;
                foreach(var item in CommunityCenterHelper.WorldState.Value.BundleRewards.Values)
                {
                    output = "Reward " + count + ": " + item;
                    count++;
                    Monitor.Log(output);
                }
            });

            Monitor.Log("Initialized");
        }

        public override object GetApi()
        {
            return ModAPI;
        }

        public static List<BundleAreaInfo> GetVanillaCommunityCenterInfo()
        {
            List<BundleAreaInfo> bundleAreas = new List<BundleAreaInfo>();

            var pantry = new BundleAreaInfo() { Bundles = new List<BundleInfo>(), Name = "Pantry", RewardName = "Greenhouse" };
            var craftsRoom = new BundleAreaInfo() { Bundles = new List<BundleInfo>(), Name = "Crafts Room", RewardName = "Bridge Repair" };           
            var fishTank = new BundleAreaInfo() { Bundles = new List<BundleInfo>(), RewardName = "Glittering Boulder Removed" };
            var boilerRoom = new BundleAreaInfo() { Bundles = new List<BundleInfo>(), RewardName = "Minecarts Repaired" };
            var bulletinBoard = new BundleAreaInfo() { Bundles = new List<BundleInfo>(), RewardName = "Friendship ♡" };
            var vault = new BundleAreaInfo() { Bundles = new List<BundleInfo>(), RewardName = "Bus Repair" };

            bundleAreas.Add(pantry);
            bundleAreas.Add(craftsRoom);            
            bundleAreas.Add(fishTank);
            bundleAreas.Add(boilerRoom);
            bundleAreas.Add(vault);
            bundleAreas.Add(bulletinBoard);

            var springForaging = new BundleInfo
            {
                Name = "Spring Foraging",
                IngredientsRequired = 4,
                RewardItemType = (int)ItemType.Object,
                RewardItemId = (int)Objects.SpringSeeds,
                RewardItemStack = 30,
                Ingredients = new List<BundleIngredientInfo>
                {
                    new BundleIngredientInfo((int)Objects.WildHorseradish, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Daffodil, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Leek, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Dandelion, (int)Quality.Regular, 1)
                }
            };
            craftsRoom.Bundles.Add(springForaging);

            var summerForaging = new BundleInfo
            {
                Name = "Summer Foraging",
                IngredientsRequired = 3,
                RewardItemType = (int)ItemType.Object,
                RewardItemId = (int)Objects.SummerSeeds,
                RewardItemStack = 30,
                Ingredients = new List<BundleIngredientInfo>
                {
                    new BundleIngredientInfo((int)Objects.Grape, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.SpiceBerry, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.SweetPea, (int)Quality.Regular, 1)
                }
            };
            craftsRoom.Bundles.Add(summerForaging);

            var fallForaging = new BundleInfo
            {
                Name = "Fall Foraging",
                IngredientsRequired = 4,
                RewardItemType = (int)ItemType.Object,
                RewardItemId = (int)Objects.FallSeeds,
                RewardItemStack = 30,
                Ingredients = new List<BundleIngredientInfo>
                {
                    new BundleIngredientInfo((int)Objects.CommonMushroom, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.WildPlum, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Hazelnut, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Blackberry, (int)Quality.Regular, 1)
                }
                
            };
            craftsRoom.Bundles.Add(fallForaging);

            var winterForaging = new BundleInfo
            {
                Name = "Winter Foraging",
                IngredientsRequired = 4,
                RewardItemType = (int)ItemType.Object,
                RewardItemId = (int)Objects.WinterSeeds,
                RewardItemStack = 30,
                Ingredients = new List<BundleIngredientInfo>
                {
                    new BundleIngredientInfo((int)Objects.WinterRoot, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.CrystalFruit, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.SnowYam, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Crocus, (int)Quality.Regular, 1)
                }                
            };
            craftsRoom.Bundles.Add(winterForaging);

            var construction = new BundleInfo
            {
                Name = "Construction",
                IngredientsRequired = 4,
                RewardItemType = (int)ItemType.BigCraftable,
                RewardItemId = (int)BigCraftables.CharcoalKiln,
                RewardItemStack = 1,
                Ingredients = new List<BundleIngredientInfo>
                {
                    new BundleIngredientInfo((int)Objects.Wood, (int)Quality.Regular, 99),
                    new BundleIngredientInfo((int)Objects.Wood, (int)Quality.Regular, 99),
                    new BundleIngredientInfo((int)Objects.Stone, (int)Quality.Regular, 99),
                    new BundleIngredientInfo((int)Objects.Hardwood, (int)Quality.Regular, 10)
                }
                
            };
            craftsRoom.Bundles.Add(construction);

            var exoticForaging = new BundleInfo
            {
                Name = "Exotic Foraging",
                IngredientsRequired = 5,
                RewardItemType = (int)ItemType.Object,
                RewardItemId = (int)Objects.AutumnsBounty,
                RewardItemStack = 5,
                Ingredients = new List<BundleIngredientInfo>
                {
                    new BundleIngredientInfo((int)Objects.Coconut, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.CactusFruit, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.CaveCarrot, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.RedMushroom, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.PurpleMushroom, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.MapleSyrup, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.OakResin, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.PineTar, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Morel, (int)Quality.Regular, 1)
                }
            };
            craftsRoom.Bundles.Add(exoticForaging);

            var springCrops = new BundleInfo
            {
                Name = "Spring Crops",
                IngredientsRequired = 4,
                RewardItemType = (int)ItemType.Object,
                RewardItemId = (int)Objects.SpeedGro,
                RewardItemStack = 20,
                Ingredients = new List<BundleIngredientInfo>
                {
                    new BundleIngredientInfo((int)Objects.Parsnip, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.GreenBean, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Potato, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Cauliflower, (int)Quality.Regular, 1)
                }               
            };
            pantry.Bundles.Add(springCrops);

            var summerCrops = new BundleInfo
            {
                Name = "Summer Crops",
                IngredientsRequired = 4,
                RewardItemType = (int)ItemType.Object,
                RewardItemId = (int)Objects.QualitySprinkler,
                RewardItemStack = 1,
                Ingredients = new List<BundleIngredientInfo>
                {
                    new BundleIngredientInfo((int)Objects.Tomato, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.HotPepper, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Blueberry, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Melon, (int)Quality.Regular, 1)
                }               
            };
            pantry.Bundles.Add(summerCrops);

            var fallCrops = new BundleInfo
            {
                Name = "Fall Crops",
                IngredientsRequired = 4,
                RewardItemType = (int)ItemType.BigCraftable,
                RewardItemId = (int)BigCraftables.BeeHouse,
                RewardItemStack = 1,
                Ingredients = new List<BundleIngredientInfo>
                {
                    new BundleIngredientInfo((int)Objects.Corn, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Eggplant, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Pumpkin, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Yam, (int)Quality.Regular, 1)
                }               
            };
            pantry.Bundles.Add(fallCrops);

            var qualityCrops = new BundleInfo
            {
                Name = "Quality Crops",
                IngredientsRequired = 3,
                RewardItemType = (int)ItemType.BigCraftable,
                RewardItemId = (int)BigCraftables.PreservesJar,
                RewardItemStack = 1,
                Ingredients = new List<BundleIngredientInfo>
                {
                    new BundleIngredientInfo((int)Objects.Parsnip, (int)Quality.Gold, 5),
                    new BundleIngredientInfo((int)Objects.Melon, (int)Quality.Gold, 5),
                    new BundleIngredientInfo((int)Objects.Pumpkin, (int)Quality.Gold, 5),
                    new BundleIngredientInfo((int)Objects.Corn, (int)Quality.Gold, 5)
                }               
            };
            pantry.Bundles.Add(qualityCrops);

            var animalProducts = new BundleInfo
            {
                Name = "Animal Products",
                IngredientsRequired = 5,
                RewardItemType = (int)ItemType.BigCraftable,
                RewardItemId = (int)BigCraftables.CheesePress,
                RewardItemStack = 1,
                Ingredients = new List<BundleIngredientInfo>
                {
                    new BundleIngredientInfo((int)Objects.LargeMilk, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.LargeWhiteEgg, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.LargeBrownEgg, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.LGoatMilk, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Wool, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.DuckEgg, (int)Quality.Regular, 1)
                }                
            };
            pantry.Bundles.Add(animalProducts);

            var artisan = new BundleInfo
            {
                Name = "Artisan",
                IngredientsRequired = 6,
                RewardItemType = (int)ItemType.BigCraftable,
                RewardItemId = (int)BigCraftables.Keg,
                RewardItemStack = 1,
                Ingredients = new List<BundleIngredientInfo>
                {
                    new BundleIngredientInfo((int)Objects.TruffleOil, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Cloth, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.GoatCheese, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Cheese, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Honey, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Jelly, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Apple, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Apricot, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Orange, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Peach, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Pomegranate, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Cherry, (int)Quality.Regular, 1)
                }               
            };
            pantry.Bundles.Add(artisan);

            var riverFish = new BundleInfo
            {
                Name = "River Fish",
                IngredientsRequired = 4,
                RewardItemType = (int)ItemType.Object,
                RewardItemId = (int)Objects.Bait,
                RewardItemStack = 30,
                Ingredients = new List<BundleIngredientInfo>
                {
                    new BundleIngredientInfo((int)Objects.Sunfish, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Catfish, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Shad, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.TigerTrout, (int)Quality.Regular, 1)
                }               
            };
            fishTank.Bundles.Add(riverFish);

            var lakeFish = new BundleInfo
            {
                Name = "Lake Fish",
                IngredientsRequired = 4,
                RewardItemType = (int)ItemType.Object,
                RewardItemId = (int)Objects.DressedSpinner,
                RewardItemStack = 1,
                Ingredients = new List<BundleIngredientInfo>
                {
                    new BundleIngredientInfo((int)Objects.LargemouthBass, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Carp, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Bullhead, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Sturgeon, (int)Quality.Regular, 1)
                }               
            };
            fishTank.Bundles.Add(lakeFish);

            var oceanFish = new BundleInfo
            {
                Name = "Ocean Fish",
                IngredientsRequired = 4,
                RewardItemType = (int)ItemType.Object,
                RewardItemId = (int)Objects.WarpTotemBeach,
                RewardItemStack = 5,
                Ingredients = new List<BundleIngredientInfo>
                {
                    new BundleIngredientInfo((int)Objects.Sardine, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Tuna, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.RedSnapper, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Tilapia, (int)Quality.Regular, 1)
                }               
            };
            fishTank.Bundles.Add(oceanFish);

            var nightFish = new BundleInfo
            {
                Name = "Night Fishing",
                IngredientsRequired = 3,
                RewardItemType = (int)ItemType.Object,
                RewardItemId = (int)Objects.SmallGlowRing,
                RewardItemStack = 1,
                Ingredients = new List<BundleIngredientInfo>
                {
                    new BundleIngredientInfo((int)Objects.Walleye, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Bream, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Eel, (int)Quality.Regular, 1)
                }              
            };
            fishTank.Bundles.Add(nightFish);

            var crabPot = new BundleInfo
            {
                Name = "Crab Pot",
                IngredientsRequired = 5,
                RewardItemType = (int)ItemType.Object,
                RewardItemId = (int)Objects.CrabPot,
                RewardItemStack = 3,
                Ingredients = new List<BundleIngredientInfo>
                {
                    new BundleIngredientInfo((int)Objects.Lobster, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Crayfish, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Crab, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Cockle, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Mussel, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Shrimp, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Snail, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Periwinkle, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Oyster, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Clam, (int)Quality.Regular, 1)
                }                
            };
            fishTank.Bundles.Add(crabPot);

            var specialtyFish = new BundleInfo
            {
                Name = "Specialty Fish",
                IngredientsRequired = 4,
                RewardItemType = (int)ItemType.Object,
                RewardItemId = (int)Objects.DishOTheSea,
                RewardItemStack = 5,
                Ingredients = new List<BundleIngredientInfo>
                {
                    new BundleIngredientInfo((int)Objects.Pufferfish, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Ghostfish, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Sandfish, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Woodskip, (int)Quality.Regular, 1)
                }               
            };
            fishTank.Bundles.Add(specialtyFish);

            var blacksmiths = new BundleInfo
            {
                Name = "Blacksmith's",
                IngredientsRequired = 3,
                RewardItemType = (int)ItemType.BigCraftable,
                RewardItemId = (int)BigCraftables.Furnace,
                RewardItemStack = 1,
                Ingredients = new List<BundleIngredientInfo>
                {
                    new BundleIngredientInfo((int)Objects.CopperBar, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.IronBar, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.GoldBar, (int)Quality.Regular, 1)
                }             
            };
            boilerRoom.Bundles.Add(blacksmiths);

            var geologists = new BundleInfo
            {
                Name = "Geologists's",
                IngredientsRequired = 4,
                RewardItemType = (int)ItemType.Object,
                RewardItemId = (int)Objects.OmniGeode,
                RewardItemStack = 5,
                Ingredients = new List<BundleIngredientInfo>
                {
                    new BundleIngredientInfo((int)Objects.Quartz, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.EarthCrystal, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.FrozenTear, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.FireQuartz, (int)Quality.Regular, 1)
                }                
            };
            boilerRoom.Bundles.Add(geologists);

            var adventurers = new BundleInfo
            {
                Name = "Adventurers's",
                IngredientsRequired = 2,
                RewardItemType = (int)ItemType.Object,
                RewardItemId = (int)Objects.SmallMagnetRing,
                RewardItemStack = 1,
                Ingredients = new List<BundleIngredientInfo>
                {
                    new BundleIngredientInfo((int)Objects.Slime, (int)Quality.Regular, 99),
                    new BundleIngredientInfo((int)Objects.BatWing, (int)Quality.Regular, 10),
                    new BundleIngredientInfo((int)Objects.SolarEssence, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.VoidEssence, (int)Quality.Regular, 1)
                }
            };
            boilerRoom.Bundles.Add(adventurers);

            var chefs = new BundleInfo
            {
                Name = "Chef's",
                IngredientsRequired = 6,
                RewardItemType = (int)ItemType.Object,
                RewardItemId = (int)Objects.PinkCake,
                RewardItemStack = 3,
                Ingredients = new List<BundleIngredientInfo>
                {
                    new BundleIngredientInfo((int)Objects.MapleSyrup, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.FiddleheadFern, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Truffle, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Poppy, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.MakiRoll, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.FriedEgg, (int)Quality.Regular, 1)
                }               
            };
            bulletinBoard.Bundles.Add(chefs);

            var dye = new BundleInfo
            {
                Name = "Dye",
                IngredientsRequired = 6,
                RewardItemType = (int)ItemType.BigCraftable,
                RewardItemId = (int)BigCraftables.SeedMaker,
                RewardItemStack = 1,
                Ingredients = new List<BundleIngredientInfo>
                {
                    new BundleIngredientInfo((int)Objects.RedMushroom, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.SeaUrchin, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Sunflower, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.DuckFeather, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Aquamarine, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.RedCabbage, (int)Quality.Regular, 1)
                }
            };
            bulletinBoard.Bundles.Add(dye);

            var fieldResearch = new BundleInfo
            {
                Name = "Field Research",
                IngredientsRequired = 4,
                RewardItemType = (int)ItemType.BigCraftable,
                RewardItemId = (int)BigCraftables.RecyclingMachine,
                RewardItemStack = 1,
                Ingredients = new List<BundleIngredientInfo>
                {
                    new BundleIngredientInfo((int)Objects.PurpleMushroom, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.NautilusShell, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Chub, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.FrozenGeode, (int)Quality.Regular, 1)
                }                
            };
            bulletinBoard.Bundles.Add(fieldResearch);

            var fodder = new BundleInfo
            {
                Name = "Fodder",
                IngredientsRequired = 3,
                RewardItemType = (int)ItemType.BigCraftable,
                RewardItemId = (int)BigCraftables.Heater,
                RewardItemStack = 1,
                Ingredients = new List<BundleIngredientInfo>
                {
                    new BundleIngredientInfo((int)Objects.Wheat, (int)Quality.Regular, 10),
                    new BundleIngredientInfo((int)Objects.Hay, (int)Quality.Regular, 10),
                    new BundleIngredientInfo((int)Objects.Apple, (int)Quality.Regular, 3)
                }              
            };
            bulletinBoard.Bundles.Add(fodder);

            var enchanters = new BundleInfo
            {
                Name = "Enchanter's",
                IngredientsRequired = 4,
                RewardItemType = (int)ItemType.Object,
                RewardItemId = (int)Objects.GoldBar,
                RewardItemStack = 5,
                Ingredients = new List<BundleIngredientInfo>
                {
                    new BundleIngredientInfo((int)Objects.OakResin, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Wine, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.RabbitsFoot, (int)Quality.Regular, 1),
                    new BundleIngredientInfo((int)Objects.Pomegranate, (int)Quality.Regular, 1)
                }
            };
            bulletinBoard.Bundles.Add(enchanters);

            var vault2500 = new BundleInfo
            {
                Name = "2,500",
                IsPurchase = true,
                IngredientsRequired = 1,
                RewardItemType = (int)ItemType.Object,
                RewardItemId = (int)Objects.ChocolateCake,
                RewardItemStack = 3,
                Ingredients = new List<BundleIngredientInfo>
                {
                    new BundleIngredientInfo((int)Objects.Money, (int)Quality.Regular, 2500)
                }               
            };
            vault.Bundles.Add(vault2500);

            var vault5000 = new BundleInfo
            {
                Name = "5,000",
                IsPurchase = true,
                IngredientsRequired = 1,
                RewardItemType = (int)ItemType.Object,
                RewardItemId = (int)Objects.QualityFertilizer,
                RewardItemStack = 30,
                Ingredients = new List<BundleIngredientInfo>
                {
                    new BundleIngredientInfo((int)Objects.Money, (int)Quality.Regular, 5000)
                }                
            };
            vault.Bundles.Add(vault5000);

            var vault10000 = new BundleInfo
            {
                Name = "10,000",
                IsPurchase = true,
                IngredientsRequired = 1,
                RewardItemType = (int)ItemType.BigCraftable,
                RewardItemId = (int)BigCraftables.LightningRod,
                RewardItemStack = 1,
                Ingredients = new List<BundleIngredientInfo>
                {
                    new BundleIngredientInfo((int)Objects.Money, (int)Quality.Regular, 10000)
                },               
            };
            vault.Bundles.Add(vault10000);

            var vault25000 = new BundleInfo
            {
                Name = "25,000",
                IsPurchase = true,
                IngredientsRequired = 1,
                RewardItemType = (int)ItemType.BigCraftable,
                RewardItemId = (int)BigCraftables.Crystalarium,
                RewardItemStack = 1,
                Ingredients = new List<BundleIngredientInfo>
                {
                    new BundleIngredientInfo((int)Objects.Money, (int)Quality.Regular, 25000)
                }               
            };
            vault.Bundles.Add(vault25000);

            return bundleAreas;
        }
    }
}
