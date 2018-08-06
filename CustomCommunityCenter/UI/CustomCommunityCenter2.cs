using StardewValley.Locations;
using CustomCommunityCenter.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;
using xTile;
using xTile.Dimensions;
using xTile.Tiles;

namespace CustomCommunityCenter.UI
{
    public class CustomCommunityCenter2 : CommunityCenter
    {
        public static string CommunityCenterName = "CommunityCenter";
        public static string CommunityCenterMapName = "Maps\\CommunityCenter_Ruins";

        [XmlElement("warehouse")]
        private readonly NetBool warehouse = new NetBool();

        [XmlIgnore]
        private readonly NetEvent0 newJunimoNoteCheckEvent = new NetEvent0();

        [XmlIgnore]
        private readonly NetEvent1Field<int, NetInt> restoreAreaCutsceneEvent = new NetEvent1Field<int, NetInt>();

        [XmlIgnore]
        private readonly NetEvent1Field<int, NetInt> areaCompleteRewardEvent = new NetEvent1Field<int, NetInt>();

        private float messageAlpha;

        private List<int> junimoNotesViewportTargets;

        private Dictionary<int, List<int>> areaToBundleDictionary;

        private Dictionary<int, int> bundleToAreaDictionary;

        private int restoreAreaTimer;

        private int restoreAreaPhase;

        private int restoreAreaIndex;

        private Cue buildUpSound;

        public CustomCommunityCenter2() : base()
        {

            //areasComplete = new NetArray<bool, NetBool>(CommunityCenterHelper.BundleAreas.Count);

            initNetFields();
            initAreaBundleConversions();
        }

        protected override void initNetFields()
        {
            // Read data from mod config
            //SetupNetFieldsFromModConfig();
            //SetupModConfigFromNetFields();

            // Continue net field initialization
            base.initNetFields();
            base.NetFields.AddFields(warehouse, areasComplete, numberOfStarsOnPlaque, newJunimoNoteCheckEvent, restoreAreaCutsceneEvent, areaCompleteRewardEvent);
            newJunimoNoteCheckEvent.onEvent += doCheckForNewJunimoNotes;
            restoreAreaCutsceneEvent.onEvent += doRestoreAreaCutscene;
            areaCompleteRewardEvent.onEvent += doAreaCompleteReward;
        }

        public virtual void SetupNetFieldsFromModConfig()
        {
            int bundleCount = 0;
            var bundleAreas = CommunityCenterHelper.BundleAreas;

            // Clear Net Fields for multiplayer sync
            bundles.Clear();
            bundleRewards.Clear();

            for (int i = 0; i < bundleAreas.Count; i++)
            {
                // Areas in community center that are restored
                areasComplete[i] = bundleAreas[i].Completed;

                bool[] bundleFlag = new bool[bundleAreas[i].Bundles.Count];
                NetArray<bool, NetBool> ingredients = new NetArray<bool, NetBool>();
                for (int j = 0; j < bundleAreas[i].Bundles.Count; j++)
                {
                    bundleFlag[j] = bundleAreas[i].Bundles[j].Completed;

                    bundleRewards.Add(bundleCount, bundleFlag[j]);
                    bundleCount++;
                    for (int k = 0; k < bundleAreas[i].Bundles[j].Ingredients.Count; k++)
                    {
                        ingredients.Add(bundleAreas[i].Bundles[j].Ingredients[k].Completed);
                    }
                }

                bundles.Add(i, ingredients);
            }

            Debug.WriteLine("Netfields updated!");
        }

        public virtual void SetupModConfigFromNetFields()
        {
            int totalBundleCount = 0;
            int ingredientCount = 0;
            var bundleAreas = CommunityCenterHelper.BundleAreas;

            for (int i = 0; i < bundleAreas.Count; i++)
            {
                ingredientCount = 0;
                for (int j = 0; j < bundleAreas[i].Bundles.Count; j++)
                {
                    bundleAreas[i].Bundles[j].Collected = bundleRewards[totalBundleCount];

                    for (int k = 0; k < bundleAreas[i].Bundles[j].Ingredients.Count; k++)
                    {
                        bundleAreas[i].Bundles[j].Ingredients[k].Completed = bundles[i][ingredientCount];
                        ingredientCount++;
                    }

                    totalBundleCount++;
                }
            }
        }

        private void initAreaBundleConversions()
        {
            areaToBundleDictionary = new Dictionary<int, List<int>>();
            bundleToAreaDictionary = new Dictionary<int, int>();
            for (int j = 0; j < CommunityCenterHelper.BundleAreas.Count; j++)
            {
                areaToBundleDictionary.Add(j, new List<int>());
                NetMutex i = new NetMutex();
                bundleMutexes.Add(i);
                base.NetFields.AddField(i.NetFields);
            }
        }

        private int getAreaNumberFromName(string name)
        {
            for (int i = 0; i < CommunityCenterHelper.BundleAreas.Count; i++)
            {
                if (CommunityCenterHelper.BundleAreas[i].Name == name) return i;
            }

            return -1;

            //switch (name)
            //{
            //    case "Pantry":
            //        return 0;
            //    case "Crafts Room":
            //    case "CraftsRoom":
            //        return 1;
            //    case "Fish Tank":
            //    case "FishTank":
            //        return 2;
            //    case "Boiler Room":
            //    case "BoilerRoom":
            //        return 3;
            //    case "Vault":
            //        return 4;
            //    case "BulletinBoard":
            //    case "Bulletin Board":
            //    case "Bulletin":
            //        return 5;
            //    default:
            //        return -1;
            //}
        }

        private Point getNotePosition(int area)
        {
            switch (area)
            {
                case 0:
                    return new Point(14, 5);
                case 2:
                    return new Point(40, 10);
                case 1:
                    return new Point(14, 23);
                case 3:
                    return new Point(63, 14);
                case 4:
                    return new Point(55, 6);
                case 5:
                    return new Point(46, 11);
                default:
                    return Point.Zero;
            }
        }

        private string getMessageForAreaCompletion()
        {
            int areasComplete = getNumberOfAreasComplete();
            if (areasComplete >= 1 && areasComplete <= 6)
            {
                return Game1.content.LoadString("Strings\\Locations:CommunityCenter_AreaCompletion" + areasComplete, Game1.player.Name);
            }
            return "";
        }

        private int getNumberOfAreasComplete()
        {
            int complete = 0;
            for (int i = 0; i < areasComplete.Count; i++)
            {
                if (areasComplete[i])
                {
                    complete++;
                }
            }
            return complete;
        }

        public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
        {
            switch ((base.map.GetLayer("Buildings").Tiles[tileLocation] != null) ? base.map.GetLayer("Buildings").Tiles[tileLocation].TileIndex : (-1))
            {
                case 1799:
                    if (numberOfCompleteBundles() > 2)
                    {
                        checkBundle(5);
                    }
                    break;
                case 1824:
                case 1825:
                case 1826:
                case 1827:
                case 1828:
                case 1829:
                case 1830:
                case 1831:
                case 1832:
                case 1833:
                    checkBundle(getAreaNumberFromLocation(who.getTileLocation()));
                    break;
            }
            return base.checkAction(tileLocation, viewport, who);
        }

        private void checkBundle(int area)
        {
            bundleMutexes[area].RequestLock(delegate
            {
                Game1.activeClickableMenu = new CustomJunimoNoteMenu(CommunityCenterHelper.BundleAreas, area, false, false);
            }, null);
        }

        private void doCheckForNewJunimoNotes()
        {
            if (Game1.currentLocation == this)
            {
                for (int i = 0; i < areasComplete.Count; i++)
                {
                    if (!isJunimoNoteAtArea(i) && shouldNoteAppearInArea(i) && (junimoNotesViewportTargets == null || !junimoNotesViewportTargets.Contains(i)))
                    {
                        addJunimoNoteViewportTarget(i);
                    }
                }
            }
        }

        protected override void resetSharedState()
        {
            SetupNetFieldsFromModConfig();
            base.resetSharedState();
        }

        protected override void resetLocalState()
        {
            SetupModConfigFromNetFields();
            base.resetLocalState();
        }

        private int getAreaNumberFromLocation(Vector2 tileLocation)
        {
            for (int i = 0; i < areasComplete.Count; i++)
            {
                if (getAreaBounds(i).Contains((int)tileLocation.X, (int)tileLocation.Y))
                {
                    return i;
                }
            }
            return -1;
        }

        private Microsoft.Xna.Framework.Rectangle getAreaBounds(int area)
        {
            switch (area)
            {
                case 1:
                    return new Microsoft.Xna.Framework.Rectangle(0, 12, 21, 17);
                case 0:
                    return new Microsoft.Xna.Framework.Rectangle(0, 0, 22, 11);
                case 2:
                    return new Microsoft.Xna.Framework.Rectangle(35, 4, 9, 9);
                case 3:
                    return new Microsoft.Xna.Framework.Rectangle(52, 9, 16, 12);
                case 5:
                    return new Microsoft.Xna.Framework.Rectangle(22, 13, 28, 9);
                case 4:
                    return new Microsoft.Xna.Framework.Rectangle(45, 0, 15, 9);
                case 6:
                    return new Microsoft.Xna.Framework.Rectangle(44, 10, 6, 3);
                case 7:
                    return new Microsoft.Xna.Framework.Rectangle(22, 4, 13, 9);
                default:
                    return Microsoft.Xna.Framework.Rectangle.Empty;
            }
        }

        private void doAreaCompleteReward(int whichArea)
        {
            string mailReceivedID = "";
            switch (whichArea)
            {
                case 3:
                    mailReceivedID = "ccBoilerRoom";
                    break;
                case 0:
                    mailReceivedID = "ccPantry";
                    break;
                case 2:
                    mailReceivedID = "ccFishTank";
                    break;
                case 4:
                    mailReceivedID = "ccVault";
                    break;
                case 5:
                    mailReceivedID = "ccBulletin";
                    Game1.addMailForTomorrow("ccBulletinThankYou", false, false);
                    foreach (NPC allCharacter in Utility.getAllCharacters())
                    {
                        if (!allCharacter.datable.Value)
                        {
                            Game1.player.changeFriendship(500, allCharacter);
                        }
                    }
                    break;
                case 1:
                    mailReceivedID = "ccCraftsRoom";
                    break;
            }
            if (mailReceivedID.Length > 0 && !Game1.player.mailReceived.Contains(mailReceivedID))
            {
                Game1.player.mailForTomorrow.Add(mailReceivedID + "%&NL&%");
            }
        }

        private void doRestoreAreaCutscene(int whichArea)
        {
            areasComplete[whichArea] = true;
            restoreAreaIndex = whichArea;
            restoreAreaPhase = 0;
            restoreAreaTimer = 1000;
            if (Game1.player.currentLocation.Name == Name)
            {
                Game1.freezeControls = true;
                Game1.changeMusicTrack("none");
            }
        }

        private void setViewportToNextJunimoNoteTarget()
        {
            if (junimoNotesViewportTargets.Count > 0)
            {
                Game1.freezeControls = true;
                int area = junimoNotesViewportTargets[0];
                Point p = getNotePosition(area);
                Game1.moveViewportTo(new Vector2(p.X, p.Y) * 64f, 5f, 2000, afterViewportGetsToJunimoNotePosition, setViewportToNextJunimoNoteTarget);
            }
            else
            {
                Game1.viewportFreeze = true;
                Game1.viewportHold = 10000;
                Game1.globalFadeToBlack(Game1.afterFadeReturnViewportToPlayer, 0.02f);
                Game1.freezeControls = false;
                Game1.afterViewport = null;
            }
        }

        private void afterViewportGetsToJunimoNotePosition()
        {
            int area = junimoNotesViewportTargets[0];
            junimoNotesViewportTargets.RemoveAt(0);
            addJunimoNote(area);
            base.localSound("reward");
        }

        private void startGoodbyeDance()
        {
            Game1.freezeControls = true;
            getJunimoForArea(0).Position = new Vector2(23f, 11f) * 64f;
            getJunimoForArea(1).Position = new Vector2(27f, 11f) * 64f;
            getJunimoForArea(2).Position = new Vector2(24f, 12f) * 64f;
            getJunimoForArea(4).Position = new Vector2(26f, 12f) * 64f;
            getJunimoForArea(3).Position = new Vector2(28f, 12f) * 64f;
            getJunimoForArea(5).Position = new Vector2(25f, 11f) * 64f;
            for (int i = 0; i < areasComplete.Count; i++)
            {
                getJunimoForArea(i).stayStill();
                getJunimoForArea(i).faceDirection(1);
                getJunimoForArea(i).fadeBack();
                getJunimoForArea(i).IsInvisible = false;
                getJunimoForArea(i).setAlpha(1f);
                getJunimoForArea(i).sayGoodbye();
            }
        }

        private void endGoodbyeDance()
        {
            for (int i = 0; i < areasComplete.Count; i++)
            {
                getJunimoForArea(i).fadeAway();
            }
            Game1.pauseThenDoFunction(3600, loadJunimoHut);
            Game1.freezeControls = true;
        }

        private void loadJunimoHut()
        {
            loadArea(7, true);
            Game1.flashAlpha = 1f;
            base.localSound("wand");
            Game1.freezeControls = false;
            Game1.showGlobalMessage(Game1.content.LoadString("Strings\\Locations:CommunityCenter_JunimosReturned"));
        }

        //public static string getAreaNameFromNumber(int areaNumber)
        //{
        //    return CommunityCenterHelper.BundleAreas[areaNumber].Name;
        //    //switch (areaNumber)
        //    //{
        //    //    case 3:
        //    //        return "Boiler Room";
        //    //    case 5:
        //    //        return "Bulletin Board";
        //    //    case 1:
        //    //        return "Crafts Room";
        //    //    case 2:
        //    //        return "Fish Tank";
        //    //    case 0:
        //    //        return "Pantry";
        //    //    case 4:
        //    //        return "Vault";
        //    //    default:
        //    //        return "";
        //    //}
        //}

        //public static string getAreaEnglishDisplayNameFromNumber(int areaNumber)
        //{
        //    return CommunityCenterHelper.BundleAreas[areaNumber].Name;
        //    //return Game1.content.LoadBaseString("Strings\\Locations:CommunityCenter_AreaName_" + getAreaNameFromNumber(areaNumber).Replace(" ", ""));
        //}

        //public static string getAreaDisplayNameFromNumber(int areaNumber)
        //{
        //    return CommunityCenterHelper.BundleAreas[areaNumber].Name;
        //    //return Game1.content.LoadString("Strings\\Locations:CommunityCenter_AreaName_" + getAreaNameFromNumber(areaNumber).Replace(" ", ""));
        //}

        private StaticTile[] getJunimoNoteTileFrames(int area)
        {
            if (area == 5)
            {
                return new StaticTile[13]
                {
                new StaticTile(base.map.GetLayer("Front"), base.map.TileSheets[0], BlendMode.Alpha, 1741),
                new StaticTile(base.map.GetLayer("Front"), base.map.TileSheets[0], BlendMode.Alpha, 1741),
                new StaticTile(base.map.GetLayer("Front"), base.map.TileSheets[0], BlendMode.Alpha, 1741),
                new StaticTile(base.map.GetLayer("Front"), base.map.TileSheets[0], BlendMode.Alpha, 1741),
                new StaticTile(base.map.GetLayer("Front"), base.map.TileSheets[0], BlendMode.Alpha, 1741),
                new StaticTile(base.map.GetLayer("Front"), base.map.TileSheets[0], BlendMode.Alpha, 1741),
                new StaticTile(base.map.GetLayer("Front"), base.map.TileSheets[0], BlendMode.Alpha, 1741),
                new StaticTile(base.map.GetLayer("Front"), base.map.TileSheets[0], BlendMode.Alpha, 1741),
                new StaticTile(base.map.GetLayer("Front"), base.map.TileSheets[0], BlendMode.Alpha, 1741),
                new StaticTile(base.map.GetLayer("Front"), base.map.TileSheets[0], BlendMode.Alpha, 1773),
                new StaticTile(base.map.GetLayer("Front"), base.map.TileSheets[0], BlendMode.Alpha, 1805),
                new StaticTile(base.map.GetLayer("Front"), base.map.TileSheets[0], BlendMode.Alpha, 1805),
                new StaticTile(base.map.GetLayer("Front"), base.map.TileSheets[0], BlendMode.Alpha, 1773)
                };
            }
            return new StaticTile[20]
            {
            new StaticTile(base.map.GetLayer("Buildings"), base.map.TileSheets[0], BlendMode.Alpha, 1833),
            new StaticTile(base.map.GetLayer("Buildings"), base.map.TileSheets[0], BlendMode.Alpha, 1833),
            new StaticTile(base.map.GetLayer("Buildings"), base.map.TileSheets[0], BlendMode.Alpha, 1833),
            new StaticTile(base.map.GetLayer("Buildings"), base.map.TileSheets[0], BlendMode.Alpha, 1833),
            new StaticTile(base.map.GetLayer("Buildings"), base.map.TileSheets[0], BlendMode.Alpha, 1833),
            new StaticTile(base.map.GetLayer("Buildings"), base.map.TileSheets[0], BlendMode.Alpha, 1833),
            new StaticTile(base.map.GetLayer("Buildings"), base.map.TileSheets[0], BlendMode.Alpha, 1833),
            new StaticTile(base.map.GetLayer("Buildings"), base.map.TileSheets[0], BlendMode.Alpha, 1833),
            new StaticTile(base.map.GetLayer("Buildings"), base.map.TileSheets[0], BlendMode.Alpha, 1833),
            new StaticTile(base.map.GetLayer("Buildings"), base.map.TileSheets[0], BlendMode.Alpha, 1832),
            new StaticTile(base.map.GetLayer("Buildings"), base.map.TileSheets[0], BlendMode.Alpha, 1824),
            new StaticTile(base.map.GetLayer("Buildings"), base.map.TileSheets[0], BlendMode.Alpha, 1825),
            new StaticTile(base.map.GetLayer("Buildings"), base.map.TileSheets[0], BlendMode.Alpha, 1826),
            new StaticTile(base.map.GetLayer("Buildings"), base.map.TileSheets[0], BlendMode.Alpha, 1827),
            new StaticTile(base.map.GetLayer("Buildings"), base.map.TileSheets[0], BlendMode.Alpha, 1828),
            new StaticTile(base.map.GetLayer("Buildings"), base.map.TileSheets[0], BlendMode.Alpha, 1829),
            new StaticTile(base.map.GetLayer("Buildings"), base.map.TileSheets[0], BlendMode.Alpha, 1830),
            new StaticTile(base.map.GetLayer("Buildings"), base.map.TileSheets[0], BlendMode.Alpha, 1831),
            new StaticTile(base.map.GetLayer("Buildings"), base.map.TileSheets[0], BlendMode.Alpha, 1832),
            new StaticTile(base.map.GetLayer("Buildings"), base.map.TileSheets[0], BlendMode.Alpha, 1833)
            };
        }
    }
}
