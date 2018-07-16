using CustomCommunityCenter.Data;
using CustomCommunityCenter.SaveData;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CustomCommunityCenter.API
{
    public class CommunityCenterHelper : ICommunityCenterHelper
    {
        public static IList<BundleAreaInfo> BundleAreas { get; set; }

        protected IModHelper ModHelper { get; set; }
        protected ModConfig Config { get; set; }

        internal static readonly CustomCommunityCenter CustomCommunityCenter = new CustomCommunityCenter();
        internal static readonly CommunityCenter CommunityCenter = new CommunityCenter(CustomCommunityCenter.CommunityCenterName);

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
            SaveFarmProgress();
            RemoveAndReplaceLocation(CustomCommunityCenter, CommunityCenter);     
        }

        protected virtual void InjectCommunityCenter(object sender, EventArgs e)
        {
            LoadFarmProgress();
            RemoveAndReplaceLocation(CommunityCenter, CustomCommunityCenter);     
        }

        protected virtual string GetSaveDataPath()
        {
            return Path.Combine("saveData", Constants.SaveFolderName + ".json");
        }

        protected virtual void RemoveAndReplaceLocation(GameLocation toRemove, GameLocation toReplace)
        {
            Game1.locations.Remove(toRemove);
            Game1.locations.Add(toReplace);
        }
    }
}
