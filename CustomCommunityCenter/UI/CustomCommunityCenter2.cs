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
using System.IO;
using StardewValley.Quests;
using xTile.ObjectModel;
using StardewValley.Menus;

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

        private readonly Func<Location, xTile.Dimensions.Rectangle, Farmer, bool> gameLocationCheckAction;

        private readonly Action<GameTime> gameLocationUpdateWhenCurrentLocation;

        private readonly NetEvent1Field<float, NetFloat> removeTemporarySpritesWithIDEvent = new NetEvent1Field<float, NetFloat>();

        private readonly NetEvent1Field<int, NetInt> rumbleAndFadeEvent = new NetEvent1Field<int, NetInt>();

        private readonly NetEvent1<DamagePlayersEventArg> damagePlayersEvent = new NetEvent1<DamagePlayersEventArg>();

        private struct DamagePlayersEventArg : NetEventArg
        {
            public Microsoft.Xna.Framework.Rectangle Area;

            public int Damage;

            public void Read(BinaryReader reader)
            {
                Area = reader.ReadRectangle();
                Damage = reader.ReadInt32();
            }

            public void Write(BinaryWriter writer)
            {
                writer.WriteRectangle(Area);
                writer.Write(Damage);
            }
        }


        public CustomCommunityCenter2(string name) : base(name)
        {
            var ptr = typeof(GameLocation).GetMethod("checkAction").MethodHandle.GetFunctionPointer();
            gameLocationCheckAction = (Func<Location, xTile.Dimensions.Rectangle, Farmer, bool>)Activator.CreateInstance(typeof(Func<Location, xTile.Dimensions.Rectangle, Farmer, bool>), this, ptr);

            ptr = typeof(GameLocation).GetMethod("UpdateWhenCurrentLocation").MethodHandle.GetFunctionPointer();
            gameLocationUpdateWhenCurrentLocation = (Action<GameTime>)Activator.CreateInstance(typeof(Action<GameTime>), this, ptr);

            initNetFields();
            initAreaBundleConversions();
        }

        private void gameLocationInitNetFields()
        {
            // null = interiorDoors
            NetFields.AddFields(mapPath, uniqueName, name, lightLevel, sharedLights, isFarm, isOutdoors, isStructure, ignoreDebrisWeather, ignoreOutdoorLighting, ignoreLights, treatAsOutdoors, warps, doors, waterColor, netObjects, projectiles, largeTerrainFeatures, terrainFeatures, characters, debris, netAudio.NetFields, removeTemporarySpritesWithIDEvent, rumbleAndFadeEvent, damagePlayersEvent, lightGlows, fishSplashPoint, orePanPoint);
            sharedLights.OnValueAdded += delegate (LightSource light)
            {
                if (Game1.currentLocation.Name == Name)
                {
                    Game1.currentLightSources.Add(light);
                }
            };
            sharedLights.OnValueRemoved += delegate (LightSource light)
            {
                if (Game1.currentLocation.Name == Name)
                {
                    Game1.currentLightSources.Remove(light);
                }
            };
            netObjects.OnConflictResolve += delegate (Vector2 pos, NetRef<StardewValley.Object> rejected, NetRef<StardewValley.Object> accepted)
            {
                if (Game1.IsMasterGame)
                {
                    StardewValley.Object value = rejected.Value;
                    if (value != null)
                    {
                        value.NetFields.Parent = null;
                        value.dropItem(this, pos * 64f, pos * 64f);
                    }
                }
            };
            removeTemporarySpritesWithIDEvent.onEvent += removeTemporarySpritesWithIDLocal;
            characters.OnValueRemoved += delegate (NPC npc)
            {
                npc.Removed();
            };
        }

        protected override void initNetFields()
        {
            // Read data from mod config
            //SetupNetFieldsFromModConfig();
            //SetupModConfigFromNetFields();

            // Continue net field initialization
            gameLocationInitNetFields();

            NetFields.AddFields(warehouse, areasComplete, numberOfStarsOnPlaque, newJunimoNoteCheckEvent, restoreAreaCutsceneEvent, areaCompleteRewardEvent);
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

        public new void restoreAreaCutscene(int whichArea)
        {
            restoreAreaCutsceneEvent.Fire(whichArea);
            doRestoreAreaCutscene(whichArea);
        }

        public new void areaCompleteReward(int whichArea)
        {
            areaCompleteRewardEvent.Fire(whichArea);
        }

        public new void checkForNewJunimoNotes()
        {
            newJunimoNoteCheckEvent.Fire();
        }

        public new Junimo getJunimoForArea(int whichArea)
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

        public new bool shouldNoteAppearInArea(int area)
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

        public new int numberOfCompleteBundles()
        {
            int number = 0;

            foreach (var bundleArea in CommunityCenterHelper.BundleAreas)
            {
                number += bundleArea.BundlesCompleted;
            }

            return number;
        }

        public new void addJunimoNote(int area)
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

            return gameLocationCheckAction(tileLocation, viewport, who);
        }

        public new void loadArea(int area, bool showEffects = true)
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

        public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
        {
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

        public override void UpdateWhenCurrentLocation(GameTime time)
        {
            gameLocationUpdateWhenCurrentLocation(time);

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
                        if ((Game1.screenGlowAlpha == Game1.screenGlowMax || Game1.currentLocation.Name != Name) && restoreAreaTimer > 5200)
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

        private void checkBundle(int area)
        {
            bundleMutexes[area].RequestLock(delegate
            {
                Game1.activeClickableMenu = new CustomJunimoNoteMenu(CommunityCenterHelper.BundleAreas, area, false, false);
            }, null);
        }

        private void doCheckForNewJunimoNotes()
        {
            if (Game1.currentLocation.Name == Name)
            {
                for (int i = 0; i < areasComplete.Count; i++)
                {
                    if (!isJunimoNoteAtArea(i) && shouldNoteAppearInArea(i) && (junimoNotesViewportTargets == null || !junimoNotesViewportTargets.Contains(i)))
                    {
                        // changed to use local
                        AddJunimoNoteViewportTarget(i);
                    }
                }
            }
        }

        private void AddJunimoNoteViewportTarget(int area)
        {
            if (junimoNotesViewportTargets == null)
            {
                junimoNotesViewportTargets = new List<int>();
            }
            junimoNotesViewportTargets.Add(area);
        }

        protected override void resetSharedState()
        {
            SetupNetFieldsFromModConfig();

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

            Utility.killAllStaticLoopingSoundCues();
            if (Game1.CurrentEvent == null && !Name.ToLower().Contains("bath"))
            {
                Game1.player.canOnlyWalk = false;
            }
            Game1.UpdateViewPort(false, new Point(Game1.player.getStandingX(), Game1.player.getStandingY()));
            Game1.previousViewportPosition = new Vector2((float)Game1.viewport.X, (float)Game1.viewport.Y);
            foreach (IClickableMenu onScreenMenu in Game1.onScreenMenus)
            {
                onScreenMenu.gameWindowSizeChanged(new Microsoft.Xna.Framework.Rectangle(Game1.viewport.X, Game1.viewport.Y, Game1.viewport.Width, Game1.viewport.Height), new Microsoft.Xna.Framework.Rectangle(Game1.viewport.X, Game1.viewport.Y, Game1.viewport.Width, Game1.viewport.Height));
            }
            ignoreWarps = false;
            if (Game1.player.rightRing.Value != null)
            {
                Game1.player.rightRing.Value.onNewLocation(Game1.player, this);
            }
            if (Game1.player.leftRing.Value != null)
            {
                Game1.player.leftRing.Value.onNewLocation(Game1.player, this);
            }
            forceViewportPlayerFollow = Map.Properties.ContainsKey("ViewportFollowPlayer");
            lastTouchActionLocation = Game1.player.getTileLocation();
            for (int n = Game1.player.questLog.Count - 1; n >= 0; n--)
            {
                ((NetList<Quest, NetRef<Quest>>)Game1.player.questLog)[n].adjustGameLocation(this);
            }
            if (!isOutdoors.Value)
            {
                Game1.player.FarmerSprite.currentStep = "thudStep";
            }
            if (!isOutdoors.Value || ignoreOutdoorLighting.Value)
            {
                map.Properties.TryGetValue("AmbientLight", out PropertyValue ambientLight);
                if (ambientLight != null)
                {
                    string[] colorSplit = ambientLight.ToString().Split(' ');
                    Game1.ambientLight = new Color(Convert.ToInt32(colorSplit[0]), Convert.ToInt32(colorSplit[1]), Convert.ToInt32(colorSplit[2]));
                }
                else if (Game1.isDarkOut() || lightLevel.Value > 0f)
                {
                    Game1.ambientLight = new Color(180, 180, 0);
                }
                else
                {
                    Game1.ambientLight = Color.White;
                }
                if (Game1.bloom != null)
                {
                    Game1.bloom.Visible = false;
                }
                if (Game1.currentSong != null && Game1.currentSong.Name.Contains("ambient"))
                {
                    Game1.changeMusicTrack("none");
                }
            }

            setUpLocationSpecificFlair();

            map.Properties.TryGetValue("Light", out PropertyValue lights);
            if (lights != null && !ignoreLights.Value)
            {
                string[] split5 = lights.ToString().Split(' ');
                for (int l = 0; l < split5.Length; l += 3)
                {
                    Game1.currentLightSources.Add(new LightSource(Convert.ToInt32(split5[l + 2]), new Vector2((float)(Convert.ToInt32(split5[l]) * 64 + 32), (float)(Convert.ToInt32(split5[l + 1]) * 64 + 32)), 1f));
                }
            }

            PropertyValue musicValue = null;
            map.Properties.TryGetValue("Music", out musicValue);
            if (musicValue != null)
            {
                string[] split3 = musicValue.ToString().Split(' ');
                if (split3.Length > 1)
                {
                    if (Game1.timeOfDay >= Convert.ToInt32(split3[0]) && Game1.timeOfDay < Convert.ToInt32(split3[1]) && !split3[2].Equals(Game1.currentSong.Name))
                    {
                        Game1.changeMusicTrack(split3[2]);
                    }
                }
                else if (Game1.currentSong == null || Game1.currentSong.IsStopped || !split3[0].Equals(Game1.currentSong.Name))
                {
                    Game1.changeMusicTrack(split3[0]);
                }
            }

            Game1.currentLightSources.UnionWith(sharedLights);


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

        public static new string getAreaNameFromNumber(int areaNumber)
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

        public static new string getAreaEnglishDisplayNameFromNumber(int areaNumber)
        {
            return CommunityCenterHelper.BundleAreas[areaNumber].Name;
            //return Game1.content.LoadBaseString("Strings\\Locations:CommunityCenter_AreaName_" + getAreaNameFromNumber(areaNumber).Replace(" ", ""));
        }

        public static new string getAreaDisplayNameFromNumber(int areaNumber)
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
