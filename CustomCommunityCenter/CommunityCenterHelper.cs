using CustomCommunityCenter.Data;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.IO;

namespace CustomCommunityCenter
{
    public class CommunityCenterHelper
    {
        public static IList<BundleAreaInfo> BundleAreas { get; set; }

        protected IModHelper ModHelper { get; set; }
        protected ModConfig Config { get; set; }

        internal readonly CustomCommunityCenter customCommunityCenter;
        internal readonly CommunityCenter communityCenter;

        public CommunityCenterHelper(IModHelper helper, ModConfig config)
        {
            ModHelper = helper;
            Config = config;

            customCommunityCenter = new CustomCommunityCenter();
            communityCenter = new CommunityCenter(CustomCommunityCenter.CommunityCenterName);

            BundleAreas = Config.BundleRooms;

            SaveEvents.BeforeSave += PresaveData;
            SaveEvents.AfterSave += InjectCommunityCenter;
            SaveEvents.AfterLoad += InjectCommunityCenter;
            SaveEvents.AfterCreate += InjectCommunityCenter;
        }

        public virtual void PresaveData(object sender, EventArgs e)
        {
            RemoveAndReplaceLocation(customCommunityCenter, communityCenter);

            string saveDataPath = GetSaveDataPath();
            var saveData = new SaveData()
            {
                BundleRooms = new List<BundleAreaSaveData>()
            };

            foreach(var bundleArea in Config.BundleRooms)
            {
                BundleAreaSaveData bundleAreaSaveData = new BundleAreaSaveData()
                {
                    Name = bundleArea.Name,
                    Bundles = new List<BundleSaveData>()
                };
                saveData.BundleRooms.Add(bundleAreaSaveData);

                foreach(var bundle in bundleArea.Bundles)
                {
                    BundleSaveData bundleSaveData = new BundleSaveData()
                    {
                        Name = bundle.Name,
                        Collected = bundle.Collected,
                        Ingredients = new List<IngredientSaveData>()
                    };
                    bundleAreaSaveData.Bundles.Add(bundleSaveData);

                    foreach(var ingredient in bundle.Ingredients)
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

        public virtual void InjectCommunityCenter(object sender, EventArgs e)
        {
            RemoveAndReplaceLocation(communityCenter, customCommunityCenter);

            string saveDataPath = GetSaveDataPath();
            var saveData = ModHelper.ReadJsonFile<SaveData>(saveDataPath);

            if (saveData == null || saveData.BundleRooms == null) return;

            BundleAreaSaveData bundleAreaSaveData;
            BundleSaveData bundleSaveData;
            IngredientSaveData ingredientSaveData;

            foreach(var bundleArea in Config.BundleRooms)
            {
                bundleAreaSaveData = null;
                foreach(var saveBundleArea in saveData.BundleRooms)
                {
                    if(saveBundleArea.Name == bundleArea.Name)
                    {
                        bundleAreaSaveData = saveBundleArea;
                        break;
                    }
                }

                if (bundleAreaSaveData == null) continue;
                
                foreach(var bundle in bundleArea.Bundles)
                {
                    bundleSaveData = null;
                    foreach(var bundleSave in bundleAreaSaveData.Bundles)
                    {
                        if(bundleSave.Name == bundle.Name)
                        {
                            bundleSaveData = bundleSave;
                            break;
                        }
                    }

                    if (bundleSaveData == null) continue;

                    bundle.Collected = bundleSaveData.Collected;
                    foreach(var ingredient in bundle.Ingredients)
                    {
                        ingredientSaveData = null;
                        foreach(var ingredientSave in bundleSaveData.Ingredients)
                        {
                            if(ingredientSave.ItemId == ingredient.ItemId && ingredientSave.ItemQuality == ingredient.ItemQuality
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

        public virtual string GetSaveDataPath()
        {
            return Path.Combine("saveData", Constants.SaveFolderName + ".json");
        }

        protected void RemoveAndReplaceLocation(GameLocation toRemove, GameLocation toReplace)
        {
            Game1.locations.Remove(toRemove);
            Game1.locations.Add(toReplace);
        }
    }
}
