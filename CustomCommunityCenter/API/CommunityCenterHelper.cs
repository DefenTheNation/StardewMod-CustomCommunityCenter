using CustomCommunityCenter.Data;
using CustomCommunityCenter.SaveData;
using CustomCommunityCenter.UI;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CustomCommunityCenter.API
{
    public class CommunityCenterHelper : ICommunityCenterHelper
    {
        public static CommunityCenterHelper Helper { get; set; }
        public static NetRoot<IWorldState> WorldState { get; set; }
        public static Multiplayer MultiplayerHelper { get; set; }
        public static IList<BundleAreaInfo> BundleAreas { get; set; }

        protected IModHelper ModHelper { get; set; }
        protected ModConfig Config { get; set; }

        internal static CustomCommunityCenterLocation CustomCC;
        internal static CommunityCenter CommunityCenter;

        public CommunityCenterHelper(IModHelper helper, ModConfig config)
        {
            ModHelper = helper;
            Config = config;
            BundleAreas = Config.BundleRooms;

            SaveEvents.BeforeSave += PresaveData;
            SaveEvents.AfterSave += InjectCommunityCenter;
            SaveEvents.AfterLoad += InjectCommunityCenter;
            SaveEvents.AfterCreate += InjectCommunityCenter;
        }

        public static int GetBundleRewardIndex(int areaIndex, int bundleIndex)
        {
            int bundleRewardIndex = 0;
            for (int i = 0; i < BundleAreas.Count; i++)
            {
                for (int j = 0; j < BundleAreas[i].Bundles.Count; j++)
                {
                    if (i == areaIndex && j == bundleIndex) return bundleRewardIndex;
                    bundleRewardIndex++;
                }
            }

            return -1;
        }

        public static void IngredientComplete(BundleInfo bundle, BundleIngredientInfo ingredient)
        {
            int ingredientCount = 0;
            var bundleAreas = Helper.Config.BundleRooms;
            BundleInfo matchedBundle;

            for(int i = 0; i < bundleAreas.Count; i++)
            {
                ingredientCount = 0;
                matchedBundle = null;
                for(int j = 0; j < bundleAreas[i].Bundles.Count; j++)
                {
                    if (bundleAreas[i].Bundles[j].Name == bundle.Name)
                    {
                        matchedBundle = bundleAreas[i].Bundles[j];
                        break;
                    }
                    else ingredientCount += bundleAreas[i].Bundles[j].Ingredients.Count;
                }

                if (matchedBundle == null) continue;
                for(int j = 0; j < matchedBundle.Ingredients.Count; j++)
                {
                    if(!matchedBundle.Ingredients[j].Completed && matchedBundle.Ingredients[j].ItemId == ingredient.ItemId && matchedBundle.Ingredients[j].RequiredStack == ingredient.RequiredStack)
                    {
                        matchedBundle.Ingredients[j].Completed = true;
                        WorldState.Value.Bundles.FieldDict[i][ingredientCount] = true;

                        WriteToNetFieldsFromConfig();
                        return;
                    }
                }
            }
        }

        public static void BundleComplete()
        {
            WriteToNetFieldsFromConfig();
        }

        public static void BundleAreaComplete()
        {
            WriteToNetFieldsFromConfig();
        }

        public static void WriteToNetFieldsFromConfig()
        {
            CustomCC.SetupNetFieldsFromModConfig();
            WorldState.MarkDirty();
            MultiplayerHelper.UpdateLate(true);            
        }

        public static void ReadFromNetFieldsToConfig()
        {
            CustomCC.SetupModConfigFromNetFields();
        }

        public void SetBundleAreas(IList<BundleAreaInfo> bundleAreas)
        {
            Config.BundleRooms = bundleAreas.ToList();
            BundleAreas = Config.BundleRooms;

            ModHelper.WriteConfig(Config);
        }

        public void AddBundle(string bundleAreaName, BundleInfo bundle)
        {
            foreach(var bundleArea in BundleAreas)
            {
                if(bundleArea.RewardName == bundleAreaName)
                {
                    bundleArea.Bundles.Add(bundle);
                    break;
                }
            }
        }

        public void RemoveBundle(string bundleAreaName, string bundleName)
        {
            foreach(var bundleArea in BundleAreas)
            {
                if(bundleArea.RewardName == bundleAreaName)
                {
                    for (int i = 0; i < bundleArea.Bundles.Count; i++)
                    {
                        if (bundleArea.Bundles[i].Name == bundleName)
                        {
                            bundleArea.Bundles.RemoveAt(i);
                            break;
                        }
                    }

                    break;
                }
            }
        }

        protected virtual void SaveFarmProgress()
        {
            if (Game1.IsClient) return;

            string saveDataPath = GetSaveDataPath();
            var saveData = new FarmSaveData()
            {
                BundleRooms = new List<BundleAreaSaveData>()
            };           

            foreach (var bundleArea in Config.BundleRooms)
            {
                BundleAreaSaveData bundleAreaSaveData = new BundleAreaSaveData()
                {
                    Name = bundleArea.RewardName,
                    Bundles = new List<BundleSaveData>()
                };
                saveData.BundleRooms.Add(bundleAreaSaveData);

                foreach (var bundle in bundleArea.Bundles)
                {
                    BundleSaveData bundleSaveData = new BundleSaveData()
                    {
                        Name = bundle.Name,
                        Collected = bundle.Collected,
                        Ingredients = new List<IngredientSaveData>()
                    };
                    bundleAreaSaveData.Bundles.Add(bundleSaveData);

                    foreach (var ingredient in bundle.Ingredients)
                    {
                        IngredientSaveData ingredientSaveData = new IngredientSaveData()
                        {
                            ItemId = ingredient.ItemId,
                            ItemQuality = ingredient.ItemQuality,
                            RequiredStack = ingredient.RequiredStack,
                            Completed = ingredient.Completed
                        };

                        bundleSaveData.Ingredients.Add(ingredientSaveData);
                    }
                }
            }

            ModHelper.WriteJsonFile(saveDataPath, saveData);
        }

        protected virtual void LoadFarmProgress()
        {
            string saveDataPath = GetSaveDataPath();
            var saveData = ModHelper.ReadJsonFile<FarmSaveData>(saveDataPath);

            if (saveData == null || saveData.BundleRooms == null) return;

            // Get community center location
            for (int i = 0; i < Game1.locations.Count; i++)
            {
                if (Game1.locations[i].Name == CustomCC.Name)
                {
                    CommunityCenter = Game1.locations[i] as CommunityCenter;
                    break;
                }
            }

            BundleAreaSaveData bundleAreaSaveData;
            BundleSaveData bundleSaveData;
            IngredientSaveData ingredientSaveData;

            foreach (var bundleArea in Config.BundleRooms)
            {
                bundleAreaSaveData = null;
                foreach (var saveBundleArea in saveData.BundleRooms)
                {
                    if (saveBundleArea.Name == bundleArea.RewardName)
                    {
                        bundleAreaSaveData = saveBundleArea;
                        break;
                    }
                }

                if (bundleAreaSaveData == null) continue;

                foreach (var bundle in bundleArea.Bundles)
                {
                    bundleSaveData = null;
                    foreach (var bundleSave in bundleAreaSaveData.Bundles)
                    {
                        if (bundleSave.Name == bundle.Name)
                        {
                            bundleSaveData = bundleSave;
                            break;
                        }
                    }

                    if (bundleSaveData == null) continue;

                    bundle.Collected = bundleSaveData.Collected;
                    foreach (var ingredient in bundle.Ingredients)
                    {
                        ingredientSaveData = null;
                        foreach (var ingredientSave in bundleSaveData.Ingredients)
                        {
                            if (ingredientSave.ItemId == ingredient.ItemId && ingredientSave.ItemQuality == ingredient.ItemQuality
                                && ingredientSave.RequiredStack == ingredient.RequiredStack)
                            {
                                ingredientSaveData = ingredientSave;
                                break;
                            }
                        }

                        if (ingredientSaveData == null) continue;

                        ingredient.Completed = ingredientSaveData.Completed;
                    }
                }
            }
        }

        protected virtual void PresaveData(object sender, EventArgs e)
        {
            CustomCC.SetupModConfigFromNetFields();
            SaveFarmProgress();
            RemoveAndReplaceCC(CommunityCenter);     
        }

        protected virtual void InjectCommunityCenter(object sender, EventArgs e)
        {
            CustomCC = new CustomCommunityCenterLocation(true);

            MultiplayerHelper = ModHelper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
            WorldState = Game1.netWorldState;

            LoadFarmProgress();
            // Now that config is loaded, update the net fields
            if (Game1.IsClient) CustomCC.SetupModConfigFromNetFields();
            else CustomCC.SetupNetFieldsFromModConfig();

            RemoveAndReplaceCC(CustomCC);
        }

        protected virtual string GetConfigDataPath()
        {
            return Path.Combine("saveData", Constants.SaveFolderName + "_config.json");
        }

        protected virtual string GetSaveDataPath()
        {
            return Path.Combine("saveData", Constants.SaveFolderName + "_save.json");
        }

        protected virtual void RemoveAndReplaceCC(GameLocation toReplace)
        {
            for(int i = 0; i < Game1.locations.Count; i++)
            {
                if(Game1.locations[i].Name == CustomCC.Name)
                {
                    Game1.locations.RemoveAt(i);
                    break;
                }
            }

            Game1.locations.Add(toReplace);
        }
    }
}
