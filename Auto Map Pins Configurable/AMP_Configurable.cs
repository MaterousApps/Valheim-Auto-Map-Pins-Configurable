using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using Utilities;
using AMP_Configurable.Patches;

namespace AMP_Configurable
{
    [BepInPlugin("AMP_Configurable", "Auto Map Pins", "1.0.0")]
    [BepInProcess("valheim.exe")]
    public class Mod : BaseUnityPlugin
    {
        //***CONFIG ENTRIES***//
        //***GENERAL***//
        public static ManualLogSource Log;
        public static ConfigEntry<int> nexusID;
        public static ConfigEntry<bool> modEnabled;
        public static ConfigEntry<float> pinOverlapDistance;
        public static ConfigEntry<int> pinIcons;
        public static ConfigEntry<int> pinRange;
        public static ConfigEntry<bool> hideAllNames;
        //***ORES***//
        public static ConfigEntry<bool> pinCopper;
        public static ConfigEntry<bool> saveCopper;
        public static ConfigEntry<bool> showCopperName;
        public static ConfigEntry<float> pinCopperSize;
        public static ConfigEntry<bool> pinTin;
        public static ConfigEntry<bool> saveTin;
        public static ConfigEntry<bool> showTinName;
        public static ConfigEntry<float> pinTinSize;
        public static ConfigEntry<bool> pinObsidian;
        public static ConfigEntry<bool> saveObsidian;
        public static ConfigEntry<bool> showObsidianName;
        public static ConfigEntry<float> pinObsidianSize;
        public static ConfigEntry<bool> pinSilver;
        public static ConfigEntry<bool> saveSilver;
        public static ConfigEntry<bool> showSilverName;
        public static ConfigEntry<float> pinSilverSize;
        public static ConfigEntry<bool> pinIron;
        public static ConfigEntry<bool> saveIron;
        public static ConfigEntry<bool> showIronName;
        public static ConfigEntry<float> pinIronSize;
        //***PICKABLES***//
        public static ConfigEntry<bool> pinBerries;
        public static ConfigEntry<bool> saveBerries;
        public static ConfigEntry<bool> showBerriesName;
        public static ConfigEntry<float> pinBerriesSize;
        public static ConfigEntry<bool> pinBlueberries;
        public static ConfigEntry<bool> saveBlueberries;
        public static ConfigEntry<bool> showBlueberriesName;
        public static ConfigEntry<float> pinBlueberriesSize;
        public static ConfigEntry<bool> pinCloudberries;
        public static ConfigEntry<bool> saveCloudberries;
        public static ConfigEntry<bool> showCloudberriesName;
        public static ConfigEntry<float> pinCloudberriesSize;
        public static ConfigEntry<bool> pinThistle;
        public static ConfigEntry<bool> saveThistle;
        public static ConfigEntry<bool> showThistleName;
        public static ConfigEntry<float> pinThistleSize;
        public static ConfigEntry<bool> pinMushroom;
        public static ConfigEntry<bool> saveMushroom;
        public static ConfigEntry<bool> showMushroomName;
        public static ConfigEntry<float> pinMushroomSize;
        public static ConfigEntry<bool> pinCarrot;
        public static ConfigEntry<bool> saveCarrot;
        public static ConfigEntry<bool> showCarrotName;
        public static ConfigEntry<float> pinCarrotSize;
        public static ConfigEntry<bool> pinTurnip;
        public static ConfigEntry<bool> saveTurnip;
        public static ConfigEntry<bool> showTurnipName;
        public static ConfigEntry<float> pinTurnipSize;
        public static ConfigEntry<bool> pinDragonEgg;
        public static ConfigEntry<bool> saveDragonEgg;
        public static ConfigEntry<bool> showDragonEggName;
        public static ConfigEntry<float> pinDragonEggSize;
        //***LOCATIONS***//
        public static ConfigEntry<bool> pinCrypt;
        public static ConfigEntry<bool> saveCrypt;
        public static ConfigEntry<bool> showCryptName;
        public static ConfigEntry<float> pinCryptSize;
        public static ConfigEntry<bool> pinSunkenCrypt;
        public static ConfigEntry<bool> saveSunkenCrypt;
        public static ConfigEntry<bool> showSunkenCryptName;
        public static ConfigEntry<float> pinSunkenCryptSize;
        public static ConfigEntry<bool> pinTrollCave;
        public static ConfigEntry<bool> saveTrollCave;
        public static ConfigEntry<bool> showTrollCaveName;
        public static ConfigEntry<float> pinTrollCaveSize;
        //***SPAWNERS***//
        public static ConfigEntry<bool> pinSkeleton;
        public static ConfigEntry<bool> saveSkeleton;
        public static ConfigEntry<bool> showSkeletonName;
        public static ConfigEntry<float> pinSkeletonSize;
        public static ConfigEntry<bool> pinDraugr;
        public static ConfigEntry<bool> saveDraugr;
        public static ConfigEntry<bool> showDraugrName;
        public static ConfigEntry<float> pinDraugrSize;
        public static ConfigEntry<bool> pinSurtling;
        public static ConfigEntry<bool> saveSurtling;
        public static ConfigEntry<bool> showSurtlingName;
        public static ConfigEntry<float> pinSurtlingSize;
        public static ConfigEntry<bool> pinGreydwarf;
        public static ConfigEntry<bool> saveGreydwarf;
        public static ConfigEntry<bool> showGreydwarfName;
        public static ConfigEntry<float> pinGreydwarfSize;
        public static ConfigEntry<bool> pinSerpent;
        public static ConfigEntry<bool> saveSerpent;
        public static ConfigEntry<bool> showSerpentName;
        public static ConfigEntry<float> pinSerpentSize;

        //***PUBLIC VARIABLES***//
        public static List<Minimap.PinData> autoPins;
        public static List<Vector3> addedPinLocs;
        public static List<Minimap.PinData> pinRemList;
        public static Vector3 position;
        public static Dictionary<Vector3, string> pinItems = new Dictionary<Vector3, string>();
        public static bool hasMoved = false;



        private void Awake()
        {
            Log = Logger;

            modEnabled = Config.Bind("General", "Enabled", true, "Enable this mod");
            pinOverlapDistance = Config.Bind<float>("General", "PinOverlapDistance", 5, "Distance around pins to prevent overlapping of similar pins");
            pinIcons = Config.Bind<int>("General", "PinIcons", 1, "Use [1] Game sprites(copper for copper, silver for silver, etc.) or [2] Colored pins (must be 1 or 2)");
            pinRange = Config.Bind<int>("General", "PinRange", 450, "Sets the range that pins will appear on the mini-map. Lower value means you need to be closer to set pin.\nMin 10\nMax 450\nRecommended 50-75");
            if (pinRange.Value < 10)
                pinRange.Value = 10;
            if (pinRange.Value > 450)
                pinRange.Value = 450;
            hideAllNames = Config.Bind("General", "HideAllNames", false, "This option will hide all names for all pins.\n*THIS WILL OVERRIDE THE INDIVIDUAL SETTINGS*");
            //nexusID = Config.Bind<int>("General", "NexusID", 774, "Nexus mod ID for updates");
            //***ORES***//
            pinCopper = Config.Bind("Ores - Copper", "PinCopper", true, "Show pins for Copper");
            saveCopper = Config.Bind("Ores - Copper", "SaveCopper", false, "Save pins for Copper");
            showCopperName = Config.Bind("Ores - Copper", "ShowCopperName", true, "Show name for Copper");
            pinCopperSize = Config.Bind<float>("Ores - Copper", "PinCopperSize", 20, "Size of Copper pin on minimap/main Map (20 is recommended)");
            pinTin = Config.Bind("Ores - Tin", "PinTin", true, "Show pins for Tin");
            saveTin = Config.Bind("Ores - Tin", "SaveTin", false, "Save pins for Tin");
            showTinName = Config.Bind("Ores - Tin", "ShowTinName", true, "Show name for Tin");
            pinTinSize = Config.Bind<float>("Ores - Tin", "PinTinSize", 20, "Size of pin Tin on minimap/main Map (20 is recommended)");
            pinObsidian = Config.Bind("Ores - Obsidian", "PinObsidian", true, "Show pins for Obsidian");
            saveObsidian = Config.Bind("Ores - Obsidian", "SaveObsidian", false, "Save pins for Obsidian");
            showObsidianName = Config.Bind("Ores - Obsidian", "ShowObsidianName", true, "Show name for Obsidian");
            pinObsidianSize = Config.Bind<float>("Ores - Obsidian", "PinObsidianSize", 20, "Size of Obsidian pin on minimap/main Map (20 is recommended)");
            pinSilver = Config.Bind("Ores - Silver", "PinSilver", true, "Show pins for Silver");
            saveSilver = Config.Bind("Ores - Silver", "SaveSilver", false, "Save pins for Silver");
            showSilverName = Config.Bind("Ores - Silver", "ShowSilverName", true, "Show name for Silver");
            pinSilverSize = Config.Bind<float>("Ores - Silver", "PinSilverSize", 20, "Size of Silver pin on minimap/main Map (20 is recommended)");
            pinIron = Config.Bind("Ores - Iron", "PinIron", true, "Show pins for Iron");
            saveIron = Config.Bind("Ores - Iron", "SaveIron", false, "Save pins for Iron");
            showIronName = Config.Bind("Ores - Iron", "ShowIronName", true, "Show name for Iron");
            pinIronSize = Config.Bind<float>("Ores - Iron", "PinIronSize", 20, "Size of Iron pin on minimap/main Map (20 is recommended)");
            //***PICKABLES***//
            pinBerries = Config.Bind("Pickables - Berries", "PinBerries", true, "Show pins for Berries");
            saveBerries = Config.Bind("Pickables - Berries", "SaveBerries", false, "Save pins for Berries");
            showBerriesName = Config.Bind("Pickables - Berries", "ShowBerriesName", true, "Show name for Berries");
            pinBerriesSize = Config.Bind<float>("Pickables - Berries", "PinBerriesSize", 20, "Size of Berries pin on minimap/main Map (20 is recommended)");
            pinBlueberries = Config.Bind("Pickables - Blueberries", "PinBlueberries", true, "Show pins for Blueberries");
            saveBlueberries = Config.Bind("Pickables - Blueberries", "SaveBlueberries", false, "Save pins for Blueberries");
            showBlueberriesName = Config.Bind("Pickables - Blueberries", "ShowBlueberriesName", true, "Show name for Blueberries");
            pinBlueberriesSize = Config.Bind<float>("Pickables - Blueberries", "PinBlueberriesSize", 20, "Size of Blueberries pin on minimap/main Map (20 is recommended)");
            pinCloudberries = Config.Bind("Pickables - Cloudberries", "PinCloudberries", true, "Show pins for Cloudberries");
            saveCloudberries = Config.Bind("Pickables - Cloudberries", "SaveCloudberries", false, "Save pins for Cloudberries");
            showCloudberriesName = Config.Bind("Pickables - Cloudberries", "ShowCloudberriesName", true, "Show name for Cloudberries");
            pinCloudberriesSize = Config.Bind<float>("Pickables - Cloudberries", "PinCloudberriesSize", 20, "Size of Cloudberries pin on minimap/main Map (20 is recommended)");
            pinThistle = Config.Bind("Pickables - Thistle", "PinThistle", true, "Show pins for Thistle");
            saveThistle = Config.Bind("Pickables - Thistle", "SaveThistle", false, "Save pins for Thistle");
            showThistleName = Config.Bind("Pickables - Thistle", "ShowThistleName", true, "Show name for Thistle");
            pinThistleSize = Config.Bind<float>("Pickables - Thistle", "PinThistleSize", 20, "Size of Thistle pin on minimap/main Map (20 is recommended)");
            pinMushroom = Config.Bind("Pickables - Mushroom", "PinMushroom", true, "Show pins for Mushroom");
            saveMushroom = Config.Bind("Pickables - Mushroom", "SaveMushroom", false, "Save pins for Mushroom");
            showMushroomName = Config.Bind("Pickables - Mushroom", "ShowMushroomName", true, "Show name for Mushroom");
            pinMushroomSize = Config.Bind<float>("Pickables - Mushroom", "PinMushroomSize", 20, "Size of Mushroom pin on minimap/main Map (20 is recommended)");
            pinCarrot = Config.Bind("Pickables - Carrot", "PinCarrot", true, "Show pins for Carrot");
            saveCarrot = Config.Bind("Pickables - Carrot", "SaveCarrot", false, "Save pins for Carrot");
            showCarrotName = Config.Bind("Pickables - Carrot", "ShowCarrotName", true, "Show name for Carrot");
            pinCarrotSize = Config.Bind<float>("Pickables - Carrot", "PinCarrotSize", 25, "Size of Carrot pin on minimap/main Map (25 is recommended)");
            pinTurnip = Config.Bind("Pickables - Turnip", "PinTurnip", true, "Show pins for Turnip");
            saveTurnip = Config.Bind("Pickables - Turnip", "SaveTurnip", false, "Save pins for Turnip");
            showTurnipName = Config.Bind("Pickables - Turnip", "ShowTurnipName", true, "Show name for Turnip");
            pinTurnipSize = Config.Bind<float>("Pickables - Turnip", "PinTurnipSize", 25, "Size of Turnip pin on minimap/main Map (25 is recommended)");
            pinDragonEgg = Config.Bind("Misc - Dragon Eggs", "PinDragonEgg", true, "Show pins for Dragon Eggs");
            saveDragonEgg = Config.Bind("Misc - Dragon Eggs", "SaveDragonEgg", false, "Save pins for Dragon Eggs");
            showDragonEggName = Config.Bind("Misc - Dragon Eggs", "ShowDragonEggName", true, "Show name for Dragon Eggs");
            pinDragonEggSize = Config.Bind<float>("Misc - Dragon Eggs", "PinDragonEggSize", 20, "Size of Dragon Eggs pin on minimap/main Map (20 is recommended)");
            //***LOCATIONS***//
            pinCrypt = Config.Bind("Locations - Crypt", "PinCrypt", true, "Show pins for Crypts");
            saveCrypt = Config.Bind("Locations - Crypt", "SaveCrypt", false, "Save pins for Crypts");
            showCryptName = Config.Bind("Locations - Crypt", "ShowCryptName", true, "Show name for Crypts");
            pinCryptSize = Config.Bind<float>("Locations - Crypt", "PinCryptSize", 25, "Size of Crypts pin on minimap/main Map (25 is recommended)");
            pinSunkenCrypt = Config.Bind("Locations - SunkenCrypt", "PinSunkenCrypt", true, "Show pins for SunkenCrypts");
            saveSunkenCrypt = Config.Bind("Locations - SunkenCrypt", "SaveSunkenCrypt", false, "Save pins for SunkenCrypts");
            showSunkenCryptName = Config.Bind("Locations - SunkenCrypt", "ShowSunkenCryptName", true, "Show name for SunkenCrypts");
            pinSunkenCryptSize = Config.Bind<float>("Locations - SunkenCrypt", "PinSunkenCryptSize", 25, "Size of SunkenCrypts pin on minimap/main Map (25 is recommended)");
            pinTrollCave = Config.Bind("Locations - TrollCave", "PinTrollCave", true, "Show pins for TrollCaves");
            saveTrollCave = Config.Bind("Locations - TrollCave", "SaveTrollCave", false, "Save pins for TrollCaves");
            showTrollCaveName = Config.Bind("Locations - TrollCave", "ShowTrollCaveName", true, "Show name for TrollCaves");
            pinTrollCaveSize = Config.Bind<float>("Locations - TrollCave", "PinTrollCaveSize", 25, "Size of TrollCaves pin on minimap/main Map (25 is recommended)");
            //***SPAWNERS***//
            pinSkeleton = Config.Bind("Spawners - Skeleton", "PinSkeleton", true, "Show pins for Skeleton");
            saveSkeleton = Config.Bind("Spawners - Skeleton", "SaveSkeleton", false, "Save pins for Skeleton");
            showSkeletonName = Config.Bind("Spawners - Skeleton", "ShowSkeletonName", true, "Show name for Skeleton");
            pinSkeletonSize = Config.Bind<float>("Spawners - Skeleton", "PinSkeletonSize", 25, "Size of Skeleton pin on minimap/main Map (25 is recommended)");
            pinDraugr = Config.Bind("Spawners - Draugr", "PinDraugr", true, "Show pins for Draugr");
            saveDraugr = Config.Bind("Spawners - Draugr", "SaveDraugr", false, "Save pins for Draugr");
            showDraugrName = Config.Bind("Spawners - Draugr", "ShowDraugrName", true, "Show name for Draugr");
            pinDraugrSize = Config.Bind<float>("Spawners - Draugr", "PinDraugrSize", 25, "Size of Draugr pin on minimap/main Map (25 is recommended)");
            pinSurtling = Config.Bind("Spawners - Surtling", "PinSurtling", true, "Show pins for Surtling");
            saveSurtling = Config.Bind("Spawners - Surtling", "SaveSurtling", false, "Save pins for Surtling");
            showSurtlingName = Config.Bind("Spawners - Surtling", "ShowSurtlingName", true, "Show name for Surtling");
            pinSurtlingSize = Config.Bind<float>("Spawners - Surtling", "PinSurtlingSize", 25, "Size of Surtling pin on minimap/main Map (25 is recommended)");
            pinGreydwarf = Config.Bind("Spawners - Greydwarf", "PinGreydwarf", true, "Show pins for Greydwarf");
            saveGreydwarf = Config.Bind("Spawners - Greydwarf", "SaveGreydwarf", false, "Save pins for Greydwarf");
            showGreydwarfName = Config.Bind("Spawners - Greydwarf", "ShowGreydwarfName", true, "Show name for Greydwarf");
            pinGreydwarfSize = Config.Bind<float>("Spawners - Greydwarf", "PinGreydwarfSize", 25, "Size of Greydwarf pin on minimap/main Map (25 is recommended)");
            pinSerpent = Config.Bind("Spawners - Serpent", "PinSerpent", true, "Show pins for Serpent");
            saveSerpent = Config.Bind("Spawners - Serpent", "SaveSerpent", false, "Save pins for Serpent");
            showSerpentName = Config.Bind("Spawners - Serpent", "ShowSerpentName", true, "Show name for Serpent");
            pinSerpentSize = Config.Bind<float>("Spawners - Serpent", "PinSerpentSize", 25, "Size of Serpent pin on minimap/main Map (25 is recommended)");

            if (!modEnabled.Value)
                enabled = false;
            else
            {
                new Harmony("materousapps.mods.automappins_configurable").PatchAll();
                Assets.Init(pinIcons.Value); 
                Harmony.CreateAndPatchAll(typeof(Minimap_Patch), "materousapps.mods.automappins_configurable");
                Harmony.CreateAndPatchAll(typeof(DestructiblePatchSpawn), "materousapps.mods.automappins_configurable");
                Harmony.CreateAndPatchAll(typeof(PickablePatchSpawn), "materousapps.mods.automappins_configurable");
                Harmony.CreateAndPatchAll(typeof(LocationPatchSpawn), "materousapps.mods.automappins_configurable");
                Harmony.CreateAndPatchAll(typeof(SpawnAreaPatchSpawn), "materousapps.mods.automappins_configurable");
                Harmony.CreateAndPatchAll(typeof(MineRockPatchSpawn), "materousapps.mods.automappins_configurable");
                //Harmony.CreateAndPatchAll(typeof(Player_Patches), "materousapps.mods.automappins_configurable");

            }
        }

        public static bool SimilarPinExists(
          Vector3 pos,
          Minimap.PinType type,
          List<Minimap.PinData> pins,
          string aName,
          Sprite aIcon,
          out Minimap.PinData match)
        {
            foreach (Minimap.PinData pin in pins)
            {
                if(pos == pin.m_pos)
                {
                    match = pin;
                    return true;
                }
                else
                //Mod.Log.LogInfo(string.Format("[AMP] Checking Distance between Pins {0} & {1}: {2}", aName, pin.m_name, (double)Utils.DistanceXZ(pos, pin.m_pos)));
                if (((double)Utils.DistanceXZ(pos, pin.m_pos) < pinOverlapDistance.Value && (int)Utils.DistanceXZ(pos, pin.m_pos) < pinRange.Value)
                    && type == pin.m_type 
                    && (aName == pin.m_name || aIcon == pin.m_icon))
                {
                    //Log.LogInfo(string.Format("[AMP] old pin Type = {0}, new pin type = {1}", pin.m_type, type));
                    //Log.LogInfo(string.Format("[AMP] old pin Name = {0}, new pin Name = {1}", pin.m_name, aName));
                    //Log.LogInfo(string.Format("[AMP] old pin Icon = {0}, new pin Icon = {1}", pin.m_icon.ToString(), aIcon.ToString()));
                    //Mod.Log.LogInfo(string.Format("[AMP] Duplicate pins for {0} found", aName));
                    match = pin;
                    return true;
                }
            }
            match = null;
            return false;
        }

        //public static void checkPins(Vector3 cPos)
        //{
        //    if (pinRemList == null)
        //    {
        //        pinRemList = new List<Minimap.PinData>();
        //    }

        //    foreach (KeyValuePair<Vector3, string> kvp in pinItems)
        //    {
        //        if (!addedPinLocs.Contains(kvp.Key))
        //        {
        //            if (Vector3.Distance(cPos, kvp.Key) < pinRange.Value)
        //            {
        //                PinnedObject.Init(kvp.Value, kvp.Key);
        //            }
        //            else
        //            {
        //                if (autoPins != null)
        //                {
        //                    foreach (Minimap.PinData tempPin in autoPins)
        //                    {
        //                        if (kvp.Key == tempPin.m_pos && !tempPin.m_save)
        //                        {
        //                            pinRemList.Add(tempPin);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    if (pinRemList != null)
        //    {
        //        foreach (Minimap.PinData tempRemPin in pinRemList)
        //        {
        //            Minimap.instance.RemovePin(tempRemPin);
        //            autoPins.Remove(tempRemPin);
        //        }
        //    }
        //    pinRemList.Clear();
        //}

        public Mod()
        {
            return;
        }
    }

    internal class PinnedObject : MonoBehaviour
    {
        public static Minimap.PinData pin;
        public static bool aSave = false;
        public static bool showName = false;
        public static float pinSize = 20;
        public static Sprite aIcon;
        public static string aName = "";
        public static int pType;

        public void Init(string aName, Vector3 aPos)
        {
            

            if (aName != null && aName != "")
            {
                loadData(aName);

                if (Mod.autoPins == null)
                {
                    Mod.autoPins = new List<Minimap.PinData>();
                }

                if (Mod.addedPinLocs == null)
                {
                    Mod.addedPinLocs = new List<Vector3>();
                }

                if (Mod.autoPins.Count > 0)
                {
                    if (Mod.SimilarPinExists(aPos, (Minimap.PinType)pType, Mod.autoPins, aName, aIcon, out Minimap.PinData _))
                    {
                        return;
                    }
                }

                //Mod.Log.LogInfo(string.Format("[AMP] Checking Distance between Player position {0} & {1} position {2}: {3}", GameCamera.instance.transform.position, aName, transform.position, Vector3.Distance(GameCamera.instance.transform.position, transform.position)));
                if (Mod.hideAllNames.Value)
                    showName = false;

                if (showName)
                {
                    pin = Minimap.instance.AddPin(aPos, (Minimap.PinType)pType, aName, aSave, false);
                }
                else
                {
                    pin = Minimap.instance.AddPin(aPos, (Minimap.PinType)pType, string.Empty, aSave, false);
                }

                //Mod.Log.LogInfo(string.Format("[AMP] Pin Type = {0}", pin.m_type));

                if (aIcon)
                    pin.m_icon = aIcon;

                pin.m_worldSize = pinSize;
                pin.m_save = aSave;

                //Mod.Log.LogInfo(string.Format("[AMP] Tracking: {0} at {1} {2} {3}", aName, transform.position.x, transform.position.y, transform.position.z));
                Mod.autoPins.Add(pin);
                Mod.addedPinLocs.Add(aPos);

            }
        }
        private void Update()
        {

        }

        private void OnDestroy()
        {
            if (pin == null || Minimap.instance == null)
                return;

            loadData(pin.m_name);
            Mod.Log.LogInfo("OnDestroy Called");
            //Mod.Log.LogInfo(string.Format("[AMP] Save Pin {0}? = {1}.", pin.m_name, aSave));

            if (aSave || pin.m_save)
            {
                //Mod.Log.LogInfo(string.Format("[AMP] Leaving Pin {0} on map.", pin.m_name));
                return;
            }

            Minimap.instance.RemovePin(pin);
            Mod.autoPins.Clear();
            Mod.addedPinLocs.Remove(pin.m_pos);
        }

        public static void loadData(string pin)
        {
            switch (pin)
            {
                case "Tin":
                case "101":
                    aName = "Tin";
                    pType = 101;
                    aSave = Mod.saveTin.Value;
                    aIcon = Assets.tinSprite;
                    showName = Mod.showTinName.Value;
                    pinSize = Mod.pinTinSize.Value;
                    break;
                case "Copper":
                case "102":
                    aName = "Copper";
                    pType = 102;
                    aSave = Mod.saveCopper.Value;
                    aIcon = Assets.copperSprite;
                    showName = Mod.showCopperName.Value;
                    pinSize = Mod.pinCopperSize.Value;
                    break;
                case "Obsidian":
                case "103":
                    aName = "Obsidian";
                    pType = 103;
                    aSave = Mod.saveObsidian.Value;
                    aIcon = Assets.obsidianSprite;
                    showName = Mod.showObsidianName.Value;
                    pinSize = Mod.pinObsidianSize.Value;
                    break;
                case "Silver":
                case "104":
                    aName = "Silver";
                    pType = 104;
                    aSave = Mod.saveSilver.Value;
                    aIcon = Assets.silverSprite;
                    showName = Mod.showSilverName.Value;
                    pinSize = Mod.pinSilverSize.Value;
                    break;
                case "Berries":
                case "105":
                    aName = "Berries";
                    pType = 105;
                    aSave = Mod.saveBerries.Value;
                    aIcon = Assets.raspberrySprite;
                    showName = Mod.showBerriesName.Value;
                    pinSize = Mod.pinBerriesSize.Value;
                    break;
                case "Blueberries":
                case "106":
                    aName = "Blueberries";
                    pType = 106;
                    aSave = Mod.saveBlueberries.Value;
                    aIcon = Assets.blueberrySprite;
                    showName = Mod.showBlueberriesName.Value;
                    pinSize = Mod.pinBlueberriesSize.Value;
                    break;
                case "Cloudberries":
                case "107":
                    aName = "Cloudberries";
                    pType = 107;
                    aSave = Mod.saveCloudberries.Value;
                    aIcon = Assets.cloudberrySprite;
                    showName = Mod.showCloudberriesName.Value;
                    pinSize = Mod.pinCloudberriesSize.Value;
                    break;
                case "Thistle":
                case "108":
                    aName = "Thistle";
                    pType = 108;
                    aSave = Mod.saveThistle.Value;
                    aIcon = Assets.thistleSprite;
                    showName = Mod.showThistleName.Value;
                    pinSize = Mod.pinThistleSize.Value;
                    break;
                case "DragonEgg":
                case "109":
                    aName = "DragonEgg";
                    pType = 109;
                    aSave = Mod.saveDragonEgg.Value;
                    aIcon = Assets.eggSprite;
                    showName = Mod.showDragonEggName.Value;
                    pinSize = Mod.pinDragonEggSize.Value;
                    break;
                case "Mushroom":
                case "110":
                    aName = "Mushroom";
                    pType = 110;
                    aSave = Mod.saveMushroom.Value;
                    aIcon = Assets.mushroomSprite;
                    showName = Mod.showMushroomName.Value;
                    pinSize = Mod.pinMushroomSize.Value;
                    break;
                case "Carrot":
                case "111":
                    aName = "Carrot";
                    pType = 111;
                    aSave = Mod.saveCarrot.Value;
                    aIcon = Assets.carrotSprite;
                    showName = Mod.showCarrotName.Value;
                    pinSize = Mod.pinCarrotSize.Value;
                    break;
                case "Turnip":
                case "112":
                    aName = "Turnip";
                    pType = 112;
                    aSave = Mod.saveTurnip.Value;
                    aIcon = Assets.turnipSprite;
                    showName = Mod.showTurnipName.Value;
                    pinSize = Mod.pinTurnipSize.Value;
                    break;
                case "Crypt":
                case "113":
                    aName = "Crypt";
                    pType = 113;
                    aSave = Mod.saveCrypt.Value;
                    aIcon = Assets.cryptSprite;
                    showName = Mod.showCryptName.Value;
                    pinSize = Mod.pinCryptSize.Value;
                    break;
                case "SunkenCrypt":
                case "114":
                    aName = "SunkenCrypt";
                    pType = 114;
                    aSave = Mod.saveSunkenCrypt.Value;
                    aIcon = Assets.sunkenCryptSprite;
                    showName = Mod.showSunkenCryptName.Value;
                    pinSize = Mod.pinSunkenCryptSize.Value;
                    break;
                case "TrollCave":
                case "115":
                    aName = "TrollCave";
                    pType = 115;
                    aSave = Mod.saveTrollCave.Value;
                    aIcon = Assets.trollCaveSprite;
                    showName = Mod.showTrollCaveName.Value;
                    pinSize = Mod.pinTrollCaveSize.Value;
                    break;
                case "Skeleton":
                case "116":
                    aName = "Skeleton";
                    pType = 116;
                    aSave = Mod.saveSkeleton.Value;
                    aIcon = Assets.skeletonSprite;
                    showName = Mod.showSkeletonName.Value;
                    pinSize = Mod.pinSkeletonSize.Value;
                    break;
                case "Surtling":
                case "117":
                    aName = "Surtling";
                    pType = 117;
                    aSave = Mod.saveSurtling.Value;
                    aIcon = Assets.surtlingSprite;
                    showName = Mod.showSurtlingName.Value;
                    pinSize = Mod.pinSurtlingSize.Value;
                    break;
                case "Draugr":
                case "118":
                    aName = "Draugr";
                    pType = 118;
                    aSave = Mod.saveDraugr.Value;
                    aIcon = Assets.draugrSprite;
                    showName = Mod.showDraugrName.Value;
                    pinSize = Mod.pinDraugrSize.Value;
                    break;
                case "Greydwarf":
                case "119":
                    aName = "Greydwarf";
                    pType = 119;
                    aSave = Mod.saveGreydwarf.Value;
                    aIcon = Assets.greydwarfSprite;
                    showName = Mod.showGreydwarfName.Value;
                    pinSize = Mod.pinGreydwarfSize.Value;
                    break;
                case "Serpent":
                case "120":
                    aName = "Serpent";
                    pType = 119;
                    aSave = Mod.saveSerpent.Value;
                    aIcon = Assets.serpentSprite;
                    showName = Mod.showSerpentName.Value;
                    pinSize = Mod.pinSerpentSize.Value;
                    break;
            }
        }

        public PinnedObject()
        {
            return;
        }
    }

    public class Assets
    {
        public static Sprite tinSprite;
        public static Sprite copperSprite;
        public static Sprite silverSprite;
        public static Sprite obsidianSprite;
        public static Sprite ironSprite;
        public static Sprite raspberrySprite;
        public static Sprite blueberrySprite;
        public static Sprite cloudberrySprite;
        public static Sprite thistleSprite;
        public static Sprite mushroomSprite;
        public static Sprite carrotSprite;
        public static Sprite turnipSprite;
        public static Sprite eggSprite;
        public static Sprite cryptSprite;
        public static Sprite sunkenCryptSprite;
        public static Sprite trollCaveSprite;
        public static Sprite skeletonSprite;
        public static Sprite surtlingSprite;
        public static Sprite draugrSprite;
        public static Sprite greydwarfSprite;
        public static Sprite serpentSprite;

        public static void Init(int pinIcons)
        {
            //Mod.Log.LogInfo(string.Format("pinIcons = {0}", pinIcons));
            switch (pinIcons)
            {
                case 1:
                    tinSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.TinOre.png")));
                    copperSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.copperore.png")));
                    silverSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.silverore.png")));
                    obsidianSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.obsidian.png")));
                    ironSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.ironscrap.png")));
                    raspberrySprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.raspberry.png")));
                    blueberrySprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.blueberries.png")));
                    cloudberrySprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.cloudberry.png")));
                    thistleSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.thistle.png")));
                    mushroomSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.mushroom.png")));
                    carrotSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.carrot.png")));
                    turnipSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.turnip.png")));
                    eggSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.dragonegg.png")));
                    cryptSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.surtling_core.png")));
                    sunkenCryptSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.witheredbone.png")));
                    trollCaveSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.TrophyFrostTroll.png")));
                    skeletonSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.TrophySkeleton.png")));
                    surtlingSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.TrophySurtling.png")));
                    draugrSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.TrophyDraugr.png")));
                    greydwarfSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.TrophyGreydwarf.png")));
                    serpentSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.TrophySerpent.png")));
                    break;
                default:
                    tinSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.mapicon_pin_tin.png")));
                    copperSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.mapicon_pin_copper.png")));
                    silverSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.mapicon_pin_silver.png")));
                    obsidianSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.mapicon_pin_obsidian.png")));
                    ironSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.mapicon_pin_iron.png")));
                    raspberrySprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.mapicon_pin_raspberry.png")));
                    blueberrySprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.mapicon_pin_blueberry.png")));
                    cloudberrySprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.mapicon_pin_cloudberry.png")));
                    thistleSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.mapicon_pin_thistle.png")));
                    eggSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.mapicon_egg.png")));
                    break;
            }
        }

        internal static Texture2D LoadTexture(byte[] file)
        {
            if (((IEnumerable<byte>)file).Count<byte>() > 0)
            {
                Texture2D texture2D = new Texture2D(2, 2);
                if (ImageConversion.LoadImage(texture2D, file))
                {
                    //Mod.Log.LogInfo(string.Format("Texture Found"));
                    return texture2D;
                }
            }
            //Mod.Log.LogInfo(string.Format("Texture not found"));
            return (Texture2D)null;
        }

        public static Sprite LoadSprite(Texture2D SpriteTexture, float PixelsPerUnit = 50f)
        { 
            return SpriteTexture? Sprite.Create(SpriteTexture, new Rect(0.0f, 0.0f, SpriteTexture.width, SpriteTexture.height), new Vector2(0.0f, 0.0f), PixelsPerUnit) : (Sprite)null;
        }
    }
}
