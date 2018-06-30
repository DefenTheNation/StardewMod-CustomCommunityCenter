﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCommunityCenter
{

    public enum ItemType
    {
        Object = 0,
        BigCraftable = 1,
        Weapon = 2,
        SpecialItem = 3,
        RegularObjectRecipe = 4,
        BigCraftableRecipe = 5,
    }

    public enum Quality
    {
        Regular = 0,
        Silver = 1,
        Gold = 2,
        Iridium = 4
    }

    public enum Boots
    {
        Sneakers = 504,
        RubberBoots = 505,
        LeatherBoots = 506,
        WorkBoots = 507,
        CombatBoots = 508,
        TundraBoots = 509,
        ThermalBoots = 510,
        DarkBoots = 511,
        FirewalkerBoots = 512,
        GenieShoes = 513,
        SpaceBoots = 514,
        CowboyBoots = 515,
    }

    public enum Hats
    {
        CowboyHat = 0,
        BowlerHat = 1,
        TopHat = 2,
        Sombrero = 3,
        StrawHat = 4,
        OfficialCap = 5,
        BlueBonnet = 6,
        PlumChapeau = 7,
        SkeletonMask = 8,
        GoblinMask = 9,
        ChickenMask = 10,
        Earmuffs = 11,
        DelicateBow = 12,
        Tropiclip = 13,
        ButterflyBow = 14,
        HuntersCap = 15,
        TruckerHat = 16,
        SailorsCap = 17,
        GoodOlCap = 18,
        Fedora = 19,
        CoolCap = 20,
        LuckyBow = 21,
        PolkaBow = 22,
        GnomesCap = 23,
        EyePatch = 24,
        SantaHat = 25,
        Tiara = 26,
        HardHat = 27,
        Souwester = 28,
        Daisy = 29,
        WatermelonBand = 30,
        MouseEars = 31,
        CatEars = 32,
        CowgalHat = 33,
        CowpokeHat = 34,
        ArchersCap = 35,
        PandaHat = 36,
        BlueCowboyHat = 37,
        RedCowboyHat = 38,
        ConeHat = 39,
    }

    public enum Weapons
    {
        RustySword = 0,
        SilverSaber = 1,
        DarkSword = 2,
        HolyBlade = 3,
        GalaxySword = 4,
        BoneSword = 5,
        IronEdge = 6,
        TemplarsBlade = 7,
        ObsidianEdge = 8,
        LavaKatana = 9,
        Claymore = 10,
        SteelSmallsword = 11,
        WoodenBlade = 12,
        InsectHead = 13,
        NeptunesGlaive = 14,
        ForestSword = 15,
        CarvingKnife = 16,
        IronDirk = 17,
        BurglarsShank = 18,
        ShadowDagger = 19,
        ElfBlade = 20,
        CrystalDagger = 21,
        WindSpire = 22,
        GalaxyDagger = 23,
        WoodClub = 24,
        AlexsBat = 25,
        LeadRod = 26,
        WoodMallet = 27,
        TheSlammer = 28,
        GalaxyHammer = 29,
        SamsOldGuitar = 30,
        Femur = 31,
        Slingshot = 32,
        MasterSlingshot = 33,
        GalaxySlingshot = 34,
        ElliottsPencil = 35,
        MarusWrench = 36,
        HarveysMallet = 37,
        PennysFryer = 38,
        LeahsWhittler = 39,
        AbbysPlanchette = 40,
        SebsLostMace = 41,
        HaleysIron = 42,
        PiratesSword = 43,
        Cutlass = 44,
        WickedKris = 45,
        Kudgel = 46,
        Scythe = 47,
        YetiTooth = 48,
        Rapier = 49,
        SteelFalchion = 50,
        BrokenTrident = 51,
        TemperedBroadsword = 52,
    }

    public enum BigCraftables
    {
        HousePlant = 0,
        HousePlant2 = 1,
        HousePlant3 = 2,
        HousePlant4 = 3,
        HousePlant5 = 4,
        HousePlant6 = 5,
        HousePlant7 = 6,
        HousePlant8 = 7,
        Scarecrow = 8,
        LightningRod = 9,
        BeeHouse = 10,
        Keg = 12,
        Furnace = 13,
        PreservesJar = 15,
        CheesePress = 16,
        Loom = 17,
        OilMaker = 19,
        RecyclingMachine = 20,
        Crystalarium = 21,
        TablePieceL = 22,
        TablePieceR = 23,
        MayonnaiseMachine = 24,
        SeedMaker = 25,
        WoodChair = 26,
        WoodChair2 = 27,
        SkeletonModel = 28,
        Obelisk = 29,
        ChickenStatue = 31,
        StoneCairn = 32,
        SuitOfArmor = 33,
        SignOfTheVessel = 34,
        BasicLog = 35,
        LawnFlamingo = 36,
        WoodSign = 37,
        StoneSign = 38,
        BigGreenCane = 40,
        GreenCanes = 41,
        MixedCane = 42,
        RedCanes = 43,
        BigRedCane = 44,
        OrnamentalHayBale = 45,
        LogSection = 46,
        GraveStone = 47,
        SeasonalDecor = 48,
        StoneFrog = 52,
        StoneParrot = 53,
        StoneOwl = 54,
        StoneJunimo = 55,
        SlimeBall = 56,
        GardenPot = 62,
        Bookcase = 64,
        FancyTable = 65,
        AncientTable = 66,
        AncientStool = 67,
        GrandfatherClock = 68,
        TeddyTimer = 69,
        DeadTree = 70,
        Staircase = 71,
        TallTorch = 72,
        RitualMask = 73,
        Bonfire = 74,
        Bongo = 75,
        DecorativeSpears = 76,
        Boulder = 78,
        Door = 79,
        Door2 = 80,
        LockedDoor = 81,
        LockedDoor2 = 82,
        WickedStatue = 83,
        WickedStatue2 = 84,
        SlothSkeletonL = 85,
        SlothSkeletonM = 86,
        SlothSkeletonR = 87,
        StandingGeode = 88,
        ObsidianVase = 89,
        CrystalChair = 90,
        SingingStone = 94,
        StoneOwl2 = 95,
        StrangeCapsule = 96,
        EmptyCapsule = 98,
        FeedHopper = 99,
        Incubator = 101,
        Heater = 104,
        Tapper = 105,
        Camera = 106,
        PlushBunny = 107,
        TuboFlowers = 108,
        TuboFlowers2 = 109,
        Rarecrow = 110,
        DecorativePitcher = 111,
        DriedSunflowers = 112,
        Rarecrow2 = 113,
        CharcoalKiln = 114,
        StardewHeroTrophy = 116,
        SodaMachine = 117,
        Barrel = 118,
        Crate = 119,
        Barrel2 = 120,
        Crate2 = 121,
        Barrel3 = 122,
        Crate3 = 123,
        Barrel4 = 124,
        Crate4 = 125,
        Rarecrow3 = 126,
        StatueOfEndlessFortune = 127,
        MushroomBox = 128,
        Chest = 130,
        Rarecrow4 = 136,
        Rarecrow5 = 137,
        Rarecrow6 = 138,
        Rarecrow7 = 139,
        Rarecrow8 = 140,
        PrairieKingArcadeSystem = 141,
        WoodenBrazier = 143,
        StoneBrazier = 144,
        GoldBrazier = 145,
        Campfire = 146,
        StumpBrazier = 147,
        CarvedBrazier = 148,
        SkullBrazier = 149,
        BarrelBrazier = 150,
        MarbleBrazier = 151,
        WoodLamppost = 152,
        IronLamppost = 153,
        WormBin = 154,
        HMTGF = 155,
        SlimeIncubator = 156,
        SlimeEggPress = 158,
        JunimoKartArcadeSystem = 159,
        StatueOfPerfection = 160,
        PinkyLemon = 161,
        Foroguemon = 162,
        Cask = 163,
        SolidGoldLewis = 164,
        AutoGrabber = 165,
        SeasonalPlant = 184,
        SeasonalPlant2 = 188,
        SeasonalPlant3 = 192,
        SeasonalPlant4 = 196,
        SeasonalPlant5 = 200,
        SeasonalPlant6 = 204,
    }

    public enum Objects
    {
        Weeds = 0,
        DiamondStone = 2,
        RubyStone = 4,
        WildHorseradish = 16,
        Daffodil = 18,
        Leek = 20,
        Dandelion = 22,
        Parsnip = 24,
        Lumber = 30,
        Emerald = 60,
        Aquamarine = 62,
        Ruby = 64,
        Amethyst = 66,
        Topaz = 68,
        Jade = 70,
        Diamond = 72,
        PrismaticShard = 74,
        GeodeStone = 75,
        FrozenGeodeStone = 76,
        MagmaGeodeStone = 77,
        CaveCarrot = 78,
        SecretNote = 79,
        Quartz = 80,
        FireQuartz = 82,
        FrozenTear = 84,
        EarthCrystal = 86,
        Coconut = 88,
        CactusFruit = 90,
        Sap = 92,
        Torch = 93,
        SpiritTorch = 94,
        DwarfScrollI = 96,
        DwarfScrollII = 97,
        DwarfScrollIII = 98,
        DwarfScrollIV = 99,
        ChippedAmphora = 100,
        Arrowhead = 101,
        LostBook = 102,
        AncientDoll = 103,
        ElvishJewelry = 104,
        ChewingStick = 105,
        OrnamentalFan = 106,
        DinosaurEgg = 107,
        RareDisc = 108,
        AncientSword = 109,
        RustySpoon = 110,
        RustySpur = 111,
        RustyCog = 112,
        ChickenStatue = 113,
        AncientSeed = 114,
        PrehistoricTool = 115,
        DriedStarfish = 116,
        Anchor = 117,
        GlassShards = 118,
        BoneFlute = 119,
        PrehistoricHandaxe = 120,
        DwarvishHelm = 121,
        DwarfGadget = 122,
        AncientDrum = 123,
        GoldenMask = 124,
        GoldenRelic = 125,
        StrangeDoll = 126,
        StrangeDoll2 = 127,
        Pufferfish = 128,
        Anchovy = 129,
        Tuna = 130,
        Sardine = 131,
        Bream = 132,
        LargemouthBass = 136,
        SmallmouthBass = 137,
        RainbowTrout = 138,
        Salmon = 139,
        Walleye = 140,
        Perch = 141,
        Carp = 142,
        Catfish = 143,
        Pike = 144,
        Sunfish = 145,
        RedMullet = 146,
        Herring = 147,
        Eel = 148,
        Octopus = 149,
        RedSnapper = 150,
        Squid = 151,
        Seaweed = 152,
        GreenAlgae = 153,
        SeaCucumber = 154,
        SuperCucumber = 155,
        Ghostfish = 156,
        WhiteAlgae = 157,
        Stonefish = 158,
        Crimsonfish = 159,
        Angler = 160,
        IcePip = 161,
        LavaEel = 162,
        Legend = 163,
        Sandfish = 164,
        ScorpionCarp = 165,
        TreasureChest = 166,
        JojaCola = 167,
        Trash = 168,
        Driftwood = 169,
        BrokenGlasses = 170,
        BrokenCD = 171,
        SoggyNewspaper = 172,
        LargeWhiteEgg = 174,
        WhiteEgg = 176,
        Hay = 178,
        BrownEgg = 180,
        LargeBrownEgg = 182,
        Milk = 184,
        LargeMilk = 186,
        GreenBean = 188,
        Cauliflower = 190,
        Potato = 192,
        FriedEgg = 194,
        Omelet = 195,
        Salad = 196,
        CheeseCauliflower = 197,
        BakedFish = 198,
        ParsnipSoup = 199,
        VegetableMedley = 200,
        CompleteBreakfast = 201,
        FriedCalamari = 202,
        StrangeBun = 203,
        LuckyLunch = 204,
        FriedMushroom = 205,
        Pizza = 206,
        BeanHotpot = 207,
        GlazedYams = 208,
        CarpSurprise = 209,
        Hashbrowns = 210,
        Pancakes = 211,
        SalmonDinner = 212,
        FishTaco = 213,
        CrispyBass = 214,
        PepperPoppers = 215,
        Bread = 216,
        TomKhaSoup = 218,
        TroutSoup = 219,
        ChocolateCake = 220,
        PinkCake = 221,
        RhubarbPie = 222,
        Cookie = 223,
        Spaghetti = 224,
        FriedEel = 225,
        SpicyEel = 226,
        Sashimi = 227,
        MakiRoll = 228,
        Tortilla = 229,
        RedPlate = 230,
        EggplantParmesan = 231,
        RicePudding = 232,
        IceCream = 233,
        BlueberryTart = 234,
        AutumnsBounty = 235,
        PumpkinSoup = 236,
        SuperMeal = 237,
        CranberrySauce = 238,
        Stuffing = 239,
        FarmersLunch = 240,
        SurvivalBurger = 241,
        DishOTheSea = 242,
        MinersTreat = 243,
        RootsPlatter = 244,
        Sugar = 245,
        WheatFlour = 246,
        Oil = 247,
        Garlic = 248,
        Kale = 250,
        Rhubarb = 252,
        Melon = 254,
        Tomato = 256,
        Morel = 257,
        Blueberry = 258,
        FiddleheadFern = 259,
        HotPepper = 260,
        Wheat = 262,
        Radish = 264,
        RedCabbage = 266,
        Starfruit = 268,
        Corn = 270,
        Eggplant = 272,
        Artichoke = 274,
        Pumpkin = 276,
        BokChoy = 278,
        Yam = 280,
        Chanterelle = 281,
        Cranberries = 282,
        Holly = 283,
        Beet = 284,
        CherryBomb = 286,
        Bomb = 287,
        MegaBomb = 288,
        IronOreStone = 290,
        Twig = 294,
        Twig2 = 295,
        Salmonberry = 296,
        GrassStarter = 297,
        HardwoodFence = 298,
        AmaranthSeeds = 299,
        Amaranth = 300,
        GrapeStarter = 301,
        HopsStarter = 302,
        PaleAle = 303,
        Hops = 304,
        VoidEgg = 305,
        Mayonnaise = 306,
        DuckMayonnaise = 307,
        VoidMayonnaise = 308,
        Acorn = 309,
        MapleSeed = 310,
        PineCone = 311,
        Weeds2 = 313,
        Weeds3 = 314,
        Weeds4 = 315,
        Weeds5 = 316,
        Weeds6 = 317,
        Weeds7 = 318,
        Weeds8 = 319,
        Weeds9 = 320,
        Weeds10 = 321,
        WoodFence = 322,
        StoneFence = 323,
        IronFence = 324,
        Gate = 325,
        DwarvishTranslationGuide = 326,
        WoodFloor = 328,
        StoneFloor = 329,
        Clay = 330,
        WeatheredFloor = 331,
        CrystalFloor = 333,
        CopperBar = 334,
        IronBar = 335,
        GoldBar = 336,
        IridiumBar = 337,
        RefinedQuartz = 338,
        Honey = 340,
        TeaSet = 341,
        Pickles = 342,
        WeirdStone = 343,
        Jelly = 344,
        Beer = 346,
        RareSeed = 347,
        Wine = 348,
        EnergyTonic = 349,
        Juice = 350,
        MuscleRemedy = 351,
        BasicFertilizer = 368,
        QualityFertilizer = 369,
        BasicRetainingSoil = 370,
        QualityRetainingSoil = 371,
        Clam = 372,
        GoldenPumpkin = 373,
        Poppy = 376,
        CopperOre = 378,
        IronOre = 380,
        Coal = 382,
        GoldOre = 384,
        IridiumOre = 386,
        Wood = 388,
        Stone = 390,
        NautilusShell = 392,
        Coral = 393,
        RainbowShell = 394,
        Coffee = 395,
        SpiceBerry = 396,
        SeaUrchin = 397,
        Grape = 398,
        SpringOnion = 399,
        Strawberry = 400,
        StrawFloor = 401,
        SweetPea = 402,
        FieldSnack = 403,
        CommonMushroom = 404,
        WoodPath = 405,
        WildPlum = 406,
        GravelPath = 407,
        Hazelnut = 408,
        CrystalPath = 409,
        Blackberry = 410,
        CobblestonePath = 411,
        WinterRoot = 412,
        BlueSlimeEgg = 413,
        CrystalFruit = 414,
        SteppingStonePath = 415,
        SnowYam = 416,
        SweetGemBerry = 417,
        Crocus = 418,
        Vinegar = 419,
        RedMushroom = 420,
        Sunflower = 421,
        PurpleMushroom = 422,
        Rice = 423,
        Cheese = 424,
        FairySeeds = 425,
        GoatCheese = 426,
        TulipBulb = 427,
        Cloth = 428,
        JazzSeeds = 429,
        Truffle = 430,
        SunflowerSeeds = 431,
        TruffleOil = 432,
        CoffeeBean = 433,
        Stardrop = 434,
        GoatMilk = 436,
        RedSlimeEgg = 437,
        LGoatMilk = 438,
        PurpleSlimeEgg = 439,
        Wool = 440,
        ExplosiveAmmo = 441,
        DuckEgg = 442,
        DuckFeather = 444,
        RabbitsFoot = 446,
        StoneBase = 449,
        StoneMines = 450,
        Weeds11 = 452,
        PoppySeeds = 453,
        AncientFruit = 454,
        SpangleSeeds = 455,
        AlgaeSoup = 456,
        PaleBroth = 457,
        Bouquet = 458,
        Mead = 459,
        MermaidsPendant = 460,
        DecorativePot = 461,
        DrumBlock = 463,
        FluteBlock = 464,
        SpeedGro = 465,
        DeluxeSpeedGro = 466,
        ParsnipSeeds = 472,
        BeanStarter = 473,
        CauliflowerSeeds = 474,
        PotatoSeeds = 475,
        GarlicSeeds = 476,
        KaleSeeds = 477,
        RhubarbSeeds = 478,
        MelonSeeds = 479,
        TomatoSeeds = 480,
        BlueberrySeeds = 481,
        PepperSeeds = 482,
        WheatSeeds = 483,
        RadishSeeds = 484,
        RedCabbageSeeds = 485,
        StarfruitSeeds = 486,
        CornSeeds = 487,
        EggplantSeeds = 488,
        ArtichokeSeeds = 489,
        PumpkinSeeds = 490,
        BokChoySeeds = 491,
        YamSeeds = 492,
        CranberrySeeds = 493,
        BeetSeeds = 494,
        SpringSeeds = 495,
        SummerSeeds = 496,
        FallSeeds = 497,
        WinterSeeds = 498,
        AncientSeeds = 499,
        SmallGlowRing = 516,
        GlowRing = 517,
        SmallMagnetRing = 518,
        MagnetRing = 519,
        SlimeCharmerRing = 520,
        WarriorRing = 521,
        VampireRing = 522,
        SavageRing = 523,
        RingofYoba = 524,
        SturdyRing = 525,
        BurglarsRing = 526,
        IridiumBand = 527,
        JukeboxRing = 528,
        AmethystRing = 529,
        TopazRing = 530,
        AquamarineRing = 531,
        JadeRing = 532,
        EmeraldRing = 533,
        RubyRing = 534,
        Geode = 535,
        FrozenGeode = 536,
        MagmaGeode = 537,
        Alamite = 538,
        Bixite = 539,
        Baryte = 540,
        Aerinite = 541,
        Calcite = 542,
        Dolomite = 543,
        Esperite = 544,
        Fluorapatite = 545,
        Geminite = 546,
        Helvite = 547,
        Jamborite = 548,
        Jagoite = 549,
        Kyanite = 550,
        Lunarite = 551,
        Malachite = 552,
        Neptunite = 553,
        LemonStone = 554,
        Nekoite = 555,
        Orpiment = 556,
        PetrifiedSlime = 557,
        ThunderEgg = 558,
        Pyrite = 559,
        OceanStone = 560,
        GhostCrystal = 561,
        Tigerseye = 562,
        Jasper = 563,
        Opal = 564,
        FireOpal = 565,
        Celestine = 566,
        Marble = 567,
        Sandstone = 568,
        Granite = 569,
        Basalt = 570,
        Limestone = 571,
        Soapstone = 572,
        Hematite = 573,
        Mudstone = 574,
        Obsidian = 575,
        Slate = 576,
        FairyStone = 577,
        StarShards = 578,
        PrehistoricScapula = 579,
        PrehistoricTibia = 580,
        PrehistoricSkull = 581,
        SkeletalHand = 582,
        PrehistoricRib = 583,
        PrehistoricVertebra = 584,
        SkeletalTail = 585,
        NautilusFossil = 586,
        AmphibianFossil = 587,
        PalmFossil = 588,
        Trilobite = 589,
        ArtifactSpot = 590,
        Tulip = 591,
        SummerSpangle = 593,
        FairyRose = 595,
        BlueJazz = 597,
        Sprinkler = 599,
        PlumPudding = 604,
        ArtichokeDip = 605,
        StirFry = 606,
        RoastedHazelnuts = 607,
        PumpkinPie = 608,
        RadishSalad = 609,
        FruitSalad = 610,
        BlackberryCobbler = 611,
        CranberryCandy = 612,
        Apple = 613,
        Bruschetta = 618,
        QualitySprinkler = 621,
        CherrySapling = 628,
        ApricotSapling = 629,
        OrangeSapling = 630,
        PeachSapling = 631,
        PomegranateSapling = 632,
        AppleSapling = 633,
        Apricot = 634,
        Orange = 635,
        Peach = 636,
        Pomegranate = 637,
        Cherry = 638,
        IridiumSprinkler = 645,
        Coleslaw = 648,
        FiddleheadRisotto = 649,
        PoppyseedMuffin = 651,
        StoneOre1 = 668,
        StoneOre2 = 670,
        Weeds12 = 674,
        Weeds13 = 675,
        Weeds14 = 676,
        Weeds15 = 677,
        Weeds16 = 678,
        Weeds17 = 679,
        GreenSlimeEgg = 680,
        RainTotem = 681,
        MutantCarp = 682,
        BugMeat = 684,
        Bait = 685,
        Spinner = 686,
        DressedSpinner = 687,
        WarpTotemFarm = 688,
        WarpTotemMountains = 689,
        WarpTotemBeach = 690,
        BarbedHook = 691,
        LeadBobber = 692,
        TreasureHunter = 693,
        TrapBobber = 694,
        CorkBobber = 695,
        Sturgeon = 698,
        TigerTrout = 699,
        Bullhead = 700,
        Tilapia = 701,
        Chub = 702,
        Magnet = 703,
        Dorado = 704,
        Albacore = 705,
        Shad = 706,
        Lingcod = 707,
        Halibut = 708,
        Hardwood = 709,
        CrabPot = 710,
        Lobster = 715,
        Crayfish = 716,
        Crab = 717,
        Cockle = 718,
        Mussel = 719,
        Shrimp = 720,
        Snail = 721,
        Periwinkle = 722,
        Oyster = 723,
        MapleSyrup = 724,
        OakResin = 725,
        PineTar = 726,
        Chowder = 727,
        FishStew = 728,
        Escargot = 729,
        LobsterBisque = 730,
        MapleBar = 731,
        CrabCakes = 732,
        Woodskip = 734,
        StrawberrySeeds = 745,
        JackOLantern = 746,
        RottenPlant = 747,
        RottenPlant2 = 748,
        OmniGeode = 749,
        Weeds18 = 750,
        CopperOreStone = 751,
        StoneMinesDark1 = 760,
        StoneMinesDark2 = 762,
        GoldOreStone = 764,
        IridiumOreStone = 765,
        Slime = 766,
        BatWing = 767,
        SolarEssence = 768,
        VoidEssence = 769,
        MixedSeeds = 770,
        Fiber = 771,
        OilofGarlic = 772,
        LifeElixir = 773,
        WildBait = 774,
        Glacierfish = 775,
        Weeds19 = 784,
        Weeds20 = 785,
        Weeds21 = 786,
        BatteryPack = 787,
        LostAxe = 788,
        LuckyPurpleShorts = 789,
        BerryBasket = 790,
        Weeds22 = 792,
        Weeds23 = 793,
        Weeds24 = 794,
        VoidSalmon = 795,
        Slimejack = 796,
        Pearl = 797,
        MidnightSquid = 798,
        SpookFish = 799,
        Blobfish = 800,
        WeddingRing = 801,
        CactusSeeds = 802,
        IridiumMilk = 803,
    }
}
