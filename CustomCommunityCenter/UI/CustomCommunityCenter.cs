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
    public class CustomCommunityCenterLocation : GameLocation
    {
        public static string CommunityCenterName = "CommunityCenter";
        public static string CommunityCenterMapName = "Maps\\CommunityCenter_Ruins";        

        public const int AREA_Pantry = 0;

        public const int AREA_FishTank = 2;

        public const int AREA_CraftsRoom = 1;

        public const int AREA_BoilerRoom = 3;

        public const int AREA_Vault = 4;

        public const int AREA_Bulletin = 5;

        public const int AREA_Bulletin2 = 6;

        public const int AREA_JunimoHut = 7;

        [XmlElement("warehouse")]
        private readonly NetBool warehouse = new NetBool();

        [XmlIgnore]
        public List<NetMutex> bundleMutexes = new List<NetMutex>();

        public readonly NetArray<bool, NetBool> areasComplete = new NetArray<bool, NetBool>(6);

        [XmlElement("numberOfStarsOnPlaque")]
        public readonly NetInt numberOfStarsOnPlaque = new NetInt();

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

        public const int PHASE_firstPause = 0;

        public const int PHASE_junimoAppear = 1;

        public const int PHASE_junimoDance = 2;

        public const int PHASE_restore = 3;

        private int restoreAreaTimer;

        private int restoreAreaPhase;

        private int restoreAreaIndex;

        private Cue buildUpSound;

        [XmlElement("bundles")]
        public NetBundles bundles
        {
            get { return CommunityCenterHelper.WorldState.Value.Bundles; }
        }

        [XmlElement("bundleRewards")]
        public NetIntDictionary<bool, NetBool> bundleRewards
        {
            get { return CommunityCenterHelper.WorldState.Value.BundleRewards; }
        }

        public CustomCommunityCenterLocation() : base()
        {
            areasComplete = new NetArray<bool, NetBool>(CommunityCenterHelper.BundleAreas.Count);

            initAreaBundleConversions();
        }

        public CustomCommunityCenterLocation(bool dummy) : base(CommunityCenterMapName, CommunityCenterName)
        {
            areasComplete = new NetArray<bool, NetBool>(CommunityCenterHelper.BundleAreas.Count);

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
                for(int j = 0; j < bundleAreas[i].Bundles.Count; j++)
                {
                    bundleFlag[j] = bundleAreas[i].Bundles[j].Completed;

                    bundleRewards.Add(bundleCount, bundleFlag[j]);
                    bundleCount++;
                    for(int k = 0; k < bundleAreas[i].Bundles[j].Ingredients.Count; k++)
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
                for(int j = 0; j < bundleAreas[i].Bundles.Count; j++)
                {
                    bundleAreas[i].Bundles[j].Collected = bundleRewards[totalBundleCount];

                    for(int k = 0; k < bundleAreas[i].Bundles[j].Ingredients.Count; k++)
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
            for(int i = 0; i < CommunityCenterHelper.BundleAreas.Count; i++)
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

        public void addJunimoNote(int area)
        {
            Point position = getNotePosition(area);
            if (!position.Equals(Vector2.Zero))
            {
                StaticTile[] tileFrames = getJunimoNoteTileFrames(area);
                string layer = (area == 5) ? "Front" : "Buildings";
                base.map.GetLayer(layer).Tiles[position.X, position.Y] = new AnimatedTile(base.map.GetLayer(layer), tileFrames, 70L);
                Game1.currentLightSources.Add(new LightSource(4, new Vector2(position.X * 64, position.Y * 64), 1f));
                base.temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2(position.X * 64, position.Y * 64), Color.White, 8, false, 100f, 0, -1, -1f, -1, 0)
                {
                    layerDepth = 1f,
                    interval = 50f,
                    motion = new Vector2(1f, 0f),
                    acceleration = new Vector2(-0.005f, 0f)
                });
                base.temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2(position.X * 64 - 12, position.Y * 64 - 12), Color.White, 8, false, 100f, 0, -1, -1f, -1, 0)
                {
                    scale = 0.75f,
                    layerDepth = 1f,
                    interval = 50f,
                    motion = new Vector2(1f, 0f),
                    acceleration = new Vector2(-0.005f, 0f),
                    delayBeforeAnimationStart = 50
                });
                base.temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2(position.X * 64 - 12, position.Y * 64 + 12), Color.White, 8, false, 100f, 0, -1, -1f, -1, 0)
                {
                    layerDepth = 1f,
                    interval = 50f,
                    motion = new Vector2(1f, 0f),
                    acceleration = new Vector2(-0.005f, 0f),
                    delayBeforeAnimationStart = 100
                });
                base.temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2(position.X * 64, position.Y * 64), Color.White, 8, false, 100f, 0, -1, -1f, -1, 0)
                {
                    layerDepth = 1f,
                    scale = 0.75f,
                    interval = 50f,
                    motion = new Vector2(1f, 0f),
                    acceleration = new Vector2(-0.005f, 0f),
                    delayBeforeAnimationStart = 150
                });
            }
        }

        public int numberOfCompleteBundles()
        {
            int number = 0;

            foreach(var bundleArea in CommunityCenterHelper.BundleAreas)
            {
                number += bundleArea.BundlesCompleted;
            }

            return number;
        }

        public void addStarToPlaque()
        {
            numberOfStarsOnPlaque.Value++;
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

        public Dictionary<int, bool[]> bundlesDict()
        {
            return (from kvp in bundles.Pairs
                    select new KeyValuePair<int, bool[]>(kvp.Key, kvp.Value.ToArray())).ToDictionary((KeyValuePair<int, bool[]> x) => x.Key, (KeyValuePair<int, bool[]> y) => y.Value);
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

        public void addJunimoNoteViewportTarget(int area)
        {
            if (junimoNotesViewportTargets == null)
            {
                junimoNotesViewportTargets = new List<int>();
            }
            junimoNotesViewportTargets.Add(area);
        }

        public void checkForNewJunimoNotes()
        {
            newJunimoNoteCheckEvent.Fire();
        }

        private void doCheckForNewJunimoNotes()
        {
            if (Game1.currentLocation.Name == Name)
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

        public void removeJunimoNote(int area)
        {
            Point p = getNotePosition(area);
            if (area == 5)
            {
                base.map.GetLayer("Front").Tiles[p.X, p.Y] = null;
            }
            else
            {
                base.map.GetLayer("Buildings").Tiles[p.X, p.Y] = null;
            }
        }

        public bool isJunimoNoteAtArea(int area)
        {
            Point p = getNotePosition(area);
            if (area == 5)
            {
                return base.map.GetLayer("Front").Tiles[p.X, p.Y] != null;
            }
            return base.map.GetLayer("Buildings").Tiles[p.X, p.Y] != null;
        }

        public bool shouldNoteAppearInArea(int area)
        {
            if (area >= 0 && areasComplete.Count > area && !areasComplete[area])
            {
                int completedBundles = numberOfCompleteBundles();

                switch (area)
                {
                    case 1:
                        return true;
                    case 0:
                    case 2:
                        if (completedBundles <= 0)
                        {
                            break;
                        }
                        return true;
                    case 3:
                        if (completedBundles <= 1)
                        {
                            break;
                        }
                        return true;
                    case 5:
                        if (completedBundles <= 2)
                        {
                            break;
                        }
                        return true;
                    case 4:
                        if (completedBundles <= 3)
                        {
                            break;
                        }
                        return true;
                }
            }
            return false;
        }

        public override void updateMap()
        {
            if (Game1.MasterPlayer.mailReceived.Contains("JojaMember"))
            {
                warehouse.Value = true;
                base.mapPath.Value = "Maps\\CommunityCenter_Joja";
            }
            else if (areAllAreasComplete())
            {
                base.mapPath.Value = "Maps\\CommunityCenter_Refurbished";
            }
            base.updateMap();
        }

        protected override void resetSharedState()
        {
            SetupNetFieldsFromModConfig();

            Debug.WriteLine("CC shared state reset!");

            base.resetSharedState();
            if (!Game1.MasterPlayer.mailReceived.Contains("JojaMember") && !areAllAreasComplete())
            {
                for (int i = 0; i < areasComplete.Count; i++)
                {
                    if (shouldNoteAppearInArea(i))
                    {
                        base.characters.Add(new Junimo(new Vector2(getNotePosition(i).X, getNotePosition(i).Y + 2) * 64f, i, false));
                    }
                }
            }
            numberOfStarsOnPlaque.Value = 0;
            for (int j = 0; j < areasComplete.Count; j++)
            {
                if (areasComplete[j])
                {
                    numberOfStarsOnPlaque.Value++;
                }
            }
        }

        protected override void resetLocalState()
        {
            SetupModConfigFromNetFields();

            Debug.WriteLine("CC local state reset!");

            base.resetLocalState();
            if (!Game1.MasterPlayer.mailReceived.Contains("JojaMember") && !areAllAreasComplete())
            {
                for (int i = 0; i < areasComplete.Count; i++)
                {
                    if (shouldNoteAppearInArea(i))
                    {
                        addJunimoNote(i);
                    }
                    else if (areasComplete[i])
                    {
                        loadArea(i, false);
                    }
                }
            }
            if (!Game1.eventUp && !areAllAreasComplete())
            {
                Game1.changeMusicTrack("communityCenter");
            }
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

        protected void removeJunimo()
        {
            for (int i = base.characters.Count - 1; i >= 0; i--)
            {
                if (base.characters[i] is Junimo)
                {
                    base.characters.RemoveAt(i);
                }
            }
        }

        public override void cleanupBeforeSave()
        {
            removeJunimo();
        }

        public override void cleanupBeforePlayerExit()
        {
            base.cleanupBeforePlayerExit();
            if (base.farmers.Count() <= 1)
            {
                removeJunimo();
            }
            Game1.changeMusicTrack("none");
        }

        public bool isBundleComplete(int bundleIndex)
        {
            for (int i = 0; i < bundles[bundleIndex].Length; i++)
            {
                if (!bundles[bundleIndex][i])
                {
                    return false;
                }
            }
            return true;
        }

        public void areaCompleteReward(int whichArea)
        {
            areaCompleteRewardEvent.Fire(whichArea);
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

        public void loadArea(int area, bool showEffects = true)
        {
            SetupModConfigFromNetFields();

            Microsoft.Xna.Framework.Rectangle areaToRefurbish = getAreaBounds(area);
            Map refurbishedMap = Game1.game1.xTileContent.Load<Map>("Maps\\CommunityCenter_Refurbished");
            for (int x = areaToRefurbish.X; x < areaToRefurbish.Right; x++)
            {
                for (int y = areaToRefurbish.Y; y < areaToRefurbish.Bottom; y++)
                {
                    if (refurbishedMap.GetLayer("Back").Tiles[x, y] != null)
                    {
                        base.map.GetLayer("Back").Tiles[x, y].TileIndex = refurbishedMap.GetLayer("Back").Tiles[x, y].TileIndex;
                    }
                    if (refurbishedMap.GetLayer("Buildings").Tiles[x, y] != null)
                    {
                        base.map.GetLayer("Buildings").Tiles[x, y] = new StaticTile(base.map.GetLayer("Buildings"), base.map.TileSheets[0], BlendMode.Alpha, refurbishedMap.GetLayer("Buildings").Tiles[x, y].TileIndex);
                        base.adjustMapLightPropertiesForLamp(refurbishedMap.GetLayer("Buildings").Tiles[x, y].TileIndex, x, y, "Buildings");
                    }
                    else
                    {
                        base.map.GetLayer("Buildings").Tiles[x, y] = null;
                    }
                    if (refurbishedMap.GetLayer("Front").Tiles[x, y] != null)
                    {
                        base.map.GetLayer("Front").Tiles[x, y] = new StaticTile(base.map.GetLayer("Front"), base.map.TileSheets[0], BlendMode.Alpha, refurbishedMap.GetLayer("Front").Tiles[x, y].TileIndex);
                        base.adjustMapLightPropertiesForLamp(refurbishedMap.GetLayer("Front").Tiles[x, y].TileIndex, x, y, "Front");
                    }
                    else
                    {
                        base.map.GetLayer("Front").Tiles[x, y] = null;
                    }
                    if (refurbishedMap.GetLayer("Paths").Tiles[x, y] != null && refurbishedMap.GetLayer("Paths").Tiles[x, y].TileIndex == 8)
                    {
                        Game1.currentLightSources.Add(new LightSource(4, new Vector2(x * 64, y * 64), 2f));
                    }
                    if (showEffects && Game1.random.NextDouble() < 0.58 && refurbishedMap.GetLayer("Buildings").Tiles[x, y] == null)
                    {
                        base.temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2(x * 64, y * 64), Color.White, 8, false, 100f, 0, -1, -1f, -1, 0)
                        {
                            layerDepth = 1f,
                            interval = 50f,
                            motion = new Vector2(Game1.random.Next(17) / 10f, 0f),
                            acceleration = new Vector2(-0.005f, 0f),
                            delayBeforeAnimationStart = Game1.random.Next(500)
                        });
                    }
                }
            }
            if (area == 5)
            {
                loadArea(6, true);
            }
            base.addLightGlows();
        }

        public void restoreAreaCutscene(int whichArea)
        {
            restoreAreaCutsceneEvent.Fire(whichArea);
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

        public override void updateEvenIfFarmerIsntHere(GameTime time, bool ignoreWasUpdatedFlush = false)
        {
            base.updateEvenIfFarmerIsntHere(time, ignoreWasUpdatedFlush);
            restoreAreaCutsceneEvent.Poll();
            newJunimoNoteCheckEvent.Poll();
            areaCompleteRewardEvent.Poll();
            foreach (NetMutex bundleMutex in bundleMutexes)
            {
                bundleMutex.Update(this);
                if (bundleMutex.IsLockHeld() && Game1.activeClickableMenu == null)
                {
                    bundleMutex.ReleaseLock();
                }
            }
        }

        public override void UpdateWhenCurrentLocation(GameTime time)
        {
            base.UpdateWhenCurrentLocation(time);
            if (restoreAreaTimer > 0)
            {
                int old = restoreAreaTimer;
                restoreAreaTimer -= time.ElapsedGameTime.Milliseconds;
                switch (restoreAreaPhase)
                {
                    case 0:
                        if (restoreAreaTimer <= 0)
                        {
                            restoreAreaTimer = 3000;
                            restoreAreaPhase = 1;
                            if (Game1.player.currentLocation.Name == Name)
                            {
                                Game1.player.faceDirection(2);
                                Game1.player.jump();
                                Game1.player.jitterStrength = 1f;
                                Game1.player.showFrame(94, false);
                            }
                        }
                        break;
                    case 1:
                        if (Game1.IsMasterGame && Game1.random.NextDouble() < 0.4)
                        {
                            Vector2 v = Utility.getRandomPositionInThisRectangle(getAreaBounds(restoreAreaIndex), Game1.random);
                            Junimo i = new Junimo(v * 64f, restoreAreaIndex, true);
                            if (!base.isCollidingPosition(i.GetBoundingBox(), Game1.viewport, i))
                            {
                                base.characters.Add(i);

                                CommunityCenterHelper.MultiplayerHelper.broadcastSprites(this, new TemporaryAnimatedSprite((Game1.random.NextDouble() < 0.5) ? 5 : 46, v * 64f + new Vector2(16f, 16f), Color.White, 8, false, 100f, 0, -1, -1f, -1, 0)
                                {
                                    layerDepth = 1f
                                });
                                //Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite((Game1.random.NextDouble() < 0.5) ? 5 : 46, v * 64f + new Vector2(16f, 16f), Color.White, 8, false, 100f, 0, -1, -1f, -1, 0)
                                //{
                                //    layerDepth = 1f
                                //});
                                base.localSound("tinyWhip");
                            }
                        }
                        if (restoreAreaTimer <= 0)
                        {
                            restoreAreaTimer = 999999;
                            restoreAreaPhase = 2;
                            if (Game1.player.currentLocation.Name != Name)
                            {
                                break;
                            }
                            Game1.screenGlowOnce(Color.White, true, 0.005f, 1f);
                            if (Game1.soundBank != null)
                            {
                                buildUpSound = Game1.soundBank.GetCue("wind");
                                buildUpSound.SetVariable("Volume", 0f);
                                buildUpSound.SetVariable("Frequency", 0f);
                                buildUpSound.Play();
                            }
                            Game1.player.jitterStrength = 2f;
                            Game1.player.stopShowingFrame();
                        }
                        Game1.drawLighting = false;
                        break;
                    case 2:
                        if (buildUpSound != null)
                        {
                            buildUpSound.SetVariable("Volume", Game1.screenGlowAlpha * 150f);
                            buildUpSound.SetVariable("Frequency", Game1.screenGlowAlpha * 150f);
                        }
                        if (Game1.screenGlowAlpha >= Game1.screenGlowMax)
                        {
                            messageAlpha += 0.008f;
                            messageAlpha = Math.Min(messageAlpha, 1f);
                        }
                        if ((Game1.screenGlowAlpha == Game1.screenGlowMax || Game1.currentLocation != this) && restoreAreaTimer > 5200)
                        {
                            restoreAreaTimer = 5200;
                        }
                        if (restoreAreaTimer < 5200 && Game1.random.NextDouble() < (double)((float)(5200 - restoreAreaTimer) / 10000f))
                        {
                            base.localSound((Game1.random.NextDouble() < 0.5) ? "dustMeep" : "junimoMeep1");
                        }
                        if (restoreAreaTimer <= 0)
                        {
                            restoreAreaTimer = 2000;
                            messageAlpha = 0f;
                            restoreAreaPhase = 3;
                            if (Game1.IsMasterGame)
                            {
                                for (int j = base.characters.Count - 1; j >= 0; j--)
                                {
                                    if (base.characters[j] is Junimo && (base.characters[j] as Junimo).temporaryJunimo.Value)
                                    {
                                        base.characters.RemoveAt(j);
                                    }
                                }
                            }
                            if (Game1.player.currentLocation.Name == Name)
                            {
                                Game1.screenGlowHold = false;
                                loadArea(restoreAreaIndex, true);
                                if (buildUpSound != null)
                                {
                                    buildUpSound.Stop(AudioStopOptions.Immediate);
                                }
                                base.localSound("wand");
                                Game1.changeMusicTrack("junimoStarSong");
                                base.localSound("woodyHit");
                                Game1.flashAlpha = 1f;
                                Game1.player.stopJittering();
                                Game1.drawLighting = true;
                            }
                        }
                        break;
                    case 3:
                        if (old > 1000 && restoreAreaTimer <= 1000)
                        {
                            Junimo k = getJunimoForArea(restoreAreaIndex);
                            if (k != null && Game1.IsMasterGame)
                            {
                                k.Position = Utility.getRandomAdjacentOpenTile(Utility.PointToVector2(getNotePosition(restoreAreaIndex)), this) * 64f;
                                int iter = 0;
                                while (base.isCollidingPosition(k.GetBoundingBox(), Game1.viewport, k) && iter < 20)
                                {
                                    k.Position = Utility.getRandomPositionInThisRectangle(getAreaBounds(restoreAreaIndex), Game1.random);
                                    iter++;
                                }
                                if (iter < 20)
                                {
                                    k.fadeBack();
                                    k.returnToJunimoHutToFetchStar(this);
                                }
                            }
                        }
                        if (restoreAreaTimer <= 0)
                        {
                            Game1.freezeControls = false;
                        }
                        break;
                }
            }
            else if (Game1.activeClickableMenu == null && junimoNotesViewportTargets != null && junimoNotesViewportTargets.Count > 0 && !Game1.isViewportOnCustomPath())
            {
                setViewportToNextJunimoNoteTarget();
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

        public Junimo getJunimoForArea(int whichArea)
        {
            foreach (NPC character in base.characters)
            {
                if (character is Junimo && (character as Junimo).whichArea.Value == whichArea)
                {
                    return character as Junimo;
                }
            }
            Junimo i = new Junimo(Vector2.Zero, whichArea, false);
            base.addCharacter(i);
            return i;
        }

        public bool areAllAreasComplete()
        {
            foreach (bool item in areasComplete)
            {
                if (!item)
                {
                    return false;
                }
            }
            return true;
        }

        public void junimoGoodbyeDance()
        {
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
            }
            Game1.moveViewportTo(new Vector2(Game1.player.getStandingX(), Game1.player.getStandingY()), 2f, 5000, startGoodbyeDance, endGoodbyeDance);
            Game1.viewportFreeze = false;
            Game1.freezeControls = true;
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

        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            for (int i = 0; i < numberOfStarsOnPlaque.Value; i++)
            {
                switch (i)
                {
                    case 0:
                        b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(2136f, 324f)), new Microsoft.Xna.Framework.Rectangle(354, 401, 7, 7), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
                        break;
                    case 1:
                        b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(2136f, 364f)), new Microsoft.Xna.Framework.Rectangle(354, 401, 7, 7), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
                        break;
                    case 2:
                        b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(2096f, 384f)), new Microsoft.Xna.Framework.Rectangle(354, 401, 7, 7), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
                        break;
                    case 3:
                        b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(2056f, 364f)), new Microsoft.Xna.Framework.Rectangle(354, 401, 7, 7), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
                        break;
                    case 4:
                        b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(2056f, 324f)), new Microsoft.Xna.Framework.Rectangle(354, 401, 7, 7), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
                        break;
                    case 5:
                        b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(2096f, 308f)), new Microsoft.Xna.Framework.Rectangle(354, 401, 7, 7), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
                        break;
                }
            }
        }

        public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
        {
            base.drawAboveAlwaysFrontLayer(b);
            if (messageAlpha > 0f)
            {
                Junimo i = getJunimoForArea(0);
                if (i != null)
                {
                    b.Draw(i.Sprite.Texture, new Vector2(Game1.viewport.Width / 2 - 32, Game1.viewport.Height * 2 / 3f - 64f), new Microsoft.Xna.Framework.Rectangle((int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 800.0) / 100 * 16, 0, 16, 16), Color.Lime * messageAlpha, 0f, new Vector2(i.Sprite.SpriteWidth * 4 / 2, (i.Sprite.SpriteHeight * 4) * 3f / 4f) / 4f, Math.Max(0.2f, 1f) * 4f, i.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1f);
                }
                b.DrawString(Game1.dialogueFont, "\"" + Game1.parseText(getMessageForAreaCompletion() + "\"", Game1.dialogueFont, 640), new Vector2(Game1.viewport.Width / 2 - 320, Game1.viewport.Height * 2 / 3f), Game1.textColor * messageAlpha * 0.6f);
            }
        }

        public static string getAreaNameFromNumber(int areaNumber)
        {
            return CommunityCenterHelper.BundleAreas[areaNumber].Name;
            //switch (areaNumber)
            //{
            //    case 3:
            //        return "Boiler Room";
            //    case 5:
            //        return "Bulletin Board";
            //    case 1:
            //        return "Crafts Room";
            //    case 2:
            //        return "Fish Tank";
            //    case 0:
            //        return "Pantry";
            //    case 4:
            //        return "Vault";
            //    default:
            //        return "";
            //}
        }

        public static string getAreaEnglishDisplayNameFromNumber(int areaNumber)
        {
            return CommunityCenterHelper.BundleAreas[areaNumber].Name;
            //return Game1.content.LoadBaseString("Strings\\Locations:CommunityCenter_AreaName_" + getAreaNameFromNumber(areaNumber).Replace(" ", ""));
        }

        public static string getAreaDisplayNameFromNumber(int areaNumber)
        {
            return CommunityCenterHelper.BundleAreas[areaNumber].Name;
            //return Game1.content.LoadString("Strings\\Locations:CommunityCenter_AreaName_" + getAreaNameFromNumber(areaNumber).Replace(" ", ""));
        }

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
