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
    [BepInPlugin("amped.mod.auto_map_pins", "AMPED - Auto Map Pins Enhanced", "1.3.0")]
    [BepInProcess("valheim.exe")]
    public class Mod : BaseUnityPlugin {
        //***CONFIG ENTRIES***//
        //***GENERAL***//
        public static ManualLogSource Log;
        public static ConfigEntry<int> nexusID;
        public static ConfigEntry<bool> modEnabled;
        public static ConfigEntry<float> pinOverlapDistance;
        public static ConfigEntry<int> pinRange;
        public static ConfigEntry<bool> hideAllLabels;
        public static ConfigEntry<string> hidePinLabels;
        public static ConfigEntry<string> savePinTypes;
        public static ConfigEntry<int> defaultPinSize;
        public static ConfigEntry<string> customPinSizes;
        public static ConfigEntry<bool> loggingEnabled;
        
        //***ORES***//
        public static ConfigEntry<bool> oresEnabled;
        public static ConfigEntry<bool> oresLoggingEnabled;
        public static ConfigEntry<string> oresPinTypes;
        public static ConfigEntry<string> oresPinItems;

        //***PICKABLES***//
        public static ConfigEntry<bool> pickablesEnabled;
        public static ConfigEntry<bool> pickablesLoggingEnabled;
        public static ConfigEntry<string> pickablesPinTypes;
        public static ConfigEntry<string> pickablesPinItems;

        //***LOCATIONS***//
        public static ConfigEntry<bool> locsEnabled;
        public static ConfigEntry<bool> locsLoggingEnabled;
        public static ConfigEntry<string> locsPinTypes;
        public static ConfigEntry<string> locsPinItems;

        //***SPAWNERS***//
        public static ConfigEntry<bool> spwnsEnabled;
        public static ConfigEntry<bool> spwnsLoggingEnabled;
        public static ConfigEntry<string> spwnsPinTypes;
        public static ConfigEntry<string> spwnsPinItems;

        //***CREATURES***//
        public static ConfigEntry<bool> creaturesEnabled;
        public static ConfigEntry<bool> creaturesLoggingEnabled;
        public static ConfigEntry<string> creaturesPinTypes;
        public static ConfigEntry<string> creaturesPinItems;

        //***PUBLIC VARIABLES***//
        public static List<Minimap.PinData> autoPins;
        public static List<Minimap.PinData> savedPins;
        public static List<Minimap.PinData> pinRemList;
        public static List<Vector3> addedPinLocs;
        public static List<Vector3> dupPinLocs;
        public static List<string> filteredPins;
        public static Vector3 position;
        public static Dictionary<Vector3, string> pinItems = new Dictionary<Vector3, string>();
        public static Dictionary<Vector3, Minimap.PinData> remPinDict = new Dictionary<Vector3, Minimap.PinData>();
        public static bool hasMoved = false;
        public static bool checkingPins = false;
        public static string currEnv = "";

        private void Awake() {
            Log = Logger;
            
            /** General Config **/
            nexusID = Config.Bind("General", "NexusID", 744, "Nexus mod ID for updates");
            modEnabled = Config.Bind("General", "Enabled", true, "Enable this mod");
            loggingEnabled = Config.Bind("General", "Enable Logging", true, "Enable Logging");
            pinOverlapDistance = Config.Bind<float>("General", "PinOverlapDistance", 10, "Distance around pins to prevent overlapping of similar pins");
            pinRange = Config.Bind("General", "Pin Range", 150, "Sets the range that pins will appear on the mini-map. Lower value means you need to be closer to set pin.\nMin 5\nMax 150\nRecommended 50-75");
            if (pinRange.Value < 5) pinRange.Value = 5;
            if (pinRange.Value > 150) pinRange.Value = 150;
            hideAllLabels = Config.Bind("General", "Hide All Labels", false, "Hide all pin labels.\n*THIS WILL OVERRIDE THE INDIVIDUAL SETTINGS*");
            hidePinLabels = Config.Bind("General", "Hide Pin Label", "", "Hide individual pin type labels.\nValue should be a comma seperated list of pin types.");
            
            string defaultSavePinTypes = "Crypt,TrollCave,SunkenCrypt,FrostCave,InfestedMine";
            string defaultPinSizes = "Carrot:25,Turnip:25,Crypt:25,Sunken Crypt:25,Troll Cave:25,Skeleton:25,Draugr:25,Surtling:25,Greydwarf:25,Serpent:25:Leviathan:25";
            savePinTypes = Config.Bind("General", "Save Pin Types", defaultSavePinTypes, "These Pin Types will persist on the map after the player as left the area.\nValue should be a comma seperated list of pin types.");
            defaultPinSize = Config.Bind("General", "Default Pin Size", 20, "Default pin size for all pins.");
            customPinSizes = Config.Bind("General", "Custom Pin Sizes", defaultPinSizes, "Customize the size of individual pins.\nValue should be a comma seperated list of pin types each with a colon ':' and a size.\nExample: Berries:20,Troll Cave:25");
            
            //***ORES***//
            string defaultOresPinTypes = "Tin:tinSprite,Copper:copperSprite,Obsidian:obsidianSprite,Silver:silverSprite,Iron:ironSprite,Flametal:flametalSprite";
            string defaultOresPinItems = "Tin:$piece_deposit_tin,Copper:$piece_deposit_copper,Obsidian:$piece_deposit_obsidian,Silver:$piece_deposit_silver|$piece_deposit_silvervein,Iron:$piece_mudpile,Flametal:MineRock_Meteorite";
            oresEnabled = Config.Bind("Resources", "Enabled", true, "Enable/Disable pins for\nOres, Trees, and other destructable resource nodes");
            oresLoggingEnabled = Config.Bind("Resources", "Enable Logging", true, "Log object id and position of each destructable resource node in range of the player.\nUsed to get object Ids to assign to pin types");
            oresPinTypes = Config.Bind("Resources", "Pin Types", defaultOresPinTypes, "Comma seperated list of pin types.\nFormat should be <PinType>:<PinIcon>. Example: Berries:raspberrySprite,Mushroom:mushroomSprite\nVisit nexus for a list of available icon sprite names.\nIf you do not include an icon pin will use a generic white circle.");
            oresPinItems = Config.Bind("Resources", "Pin Items", defaultOresPinItems, "Associate game object ids to pins using a command seperated list.\nFormat should be <PinType>:<ObjectId>.\nYou can include multiple object ids for one pin type by seperating them with a pipe '|'.\nExample: Crypt:Crypt1|Crypt2|Crypt3|Crypt4,Mushroom:Pickable_Mushroom\n**NOTE Pins will automatically attempt to match (Clone) objects so you can just use the base objectId.");

            //***PICKABLES***//
            string defaultPickablePinTypes = "Berries:raspberrySprite,Blueberries:blueberrySprite,Cloudberries:cloudberrySprite,Thistle:thistleSprite,Mushroom:mushroomSprite,Carrot:carrotSprite,Turnip:turnipSprite,Dragon Egg:eggSprite";
            string defaultPickablePinItems = "Berries:RaspberryBush,Blueberries:BlueberryBush,Cloudberries:CloudberryBush,Thistle:Pickable_Thistle,Mushroom:Pickable_Mushroom,Carrot:Pickable_SeedCarrot,Turnip:Pickable_SeedTurnip,Dragon Egg:Pickable_DragonEgg";
            pickablesEnabled = Config.Bind("Pickables", "Enabled", true, "Enable/Disable pins for\nOres, Trees, and other destructable resource nodes");
            pickablesLoggingEnabled = Config.Bind("Pickables", "Enable Logging", true, "Log object id and position of each destructable resource node in range of the player.\nUsed to get object Ids to assign to pin types");
            pickablesPinTypes = Config.Bind("Pickables", "Pin Types", defaultPickablePinTypes, "Comma seperated list of pin types.\nFormat should be <PinType>:<PinIcon>. Example: Berries:raspberrySprite,Mushroom:mushroomSprite\nVisit nexus for a list of available icon sprite names.\nIf you do not include an icon pin will use a generic white circle.");
            pickablesPinItems = Config.Bind("Pickables", "Pin Items", defaultPickablePinItems, "Associate game object ids to pins using a command seperated list.\nFormat should be <PinType>:<ObjectId>.\nYou can include multiple object ids for one pin type by seperating them with a pipe '|'.\nExample: Crypt:Crypt1|Crypt2|Crypt3|Crypt4,Mushroom:Pickable_Mushroom\n**NOTE Pins will automatically attempt to match (Clone) objects so you can just use the base objectId.");

            //***LOCATIONS***//
            string defaultLocationPinTypes = "Crypt:cryptSprite,Sunken Crypt:sunkenCryptSprite,Troll Cave:trollCaveSprite,Surtling:surtlingSprite";
            string defaultLocationPinItems = "Crypt:Crypt1|Crypt2|Crypt3|Crypt4,Sunken Crypt:SunkenCrypt1|SunkenCrypt2|SunkenCrypt3|SunkenCrypt4,Troll Cave:TrollCave01|TrollCave02,Surtling:FireHole";
            locsEnabled = Config.Bind("Locations", "Enabled", true, "Enable/Disable pins for\nOres, Trees, and other destructable resource nodes");
            locsLoggingEnabled = Config.Bind("Locations", "Enable Logging", true, "Log object id and position of each destructable resource node in range of the player.\nUsed to get object Ids to assign to pin types");
            locsPinTypes = Config.Bind("Locations", "Pin Types", defaultLocationPinTypes, "Comma seperated list of pin types.\nFormat should be <PinType>:<PinIcon>. Example: Berries:raspberrySprite,Mushroom:mushroomSprite\nVisit nexus for a list of available icon sprite names.\nIf you do not include an icon pin will use a generic white circle.");
            locsPinItems = Config.Bind("Locations", "Pin Items", defaultLocationPinItems, "Associate game object ids to pins using a command seperated list.\nFormat should be <PinType>:<ObjectId>.\nYou can include multiple object ids for one pin type by seperating them with a pipe '|'.\nExample: Crypt:Crypt1|Crypt2|Crypt3|Crypt4,Mushroom:Pickable_Mushroom\n**NOTE Pins will automatically attempt to match (Clone) objects so you can just use the base objectId.");

            //***SPAWNERS***//
            string defaultSpawnerPinTypes = "Skeleton:skeletonSprite,Draugr:draugrSprite,Greydwarf:greydwarfSprite";
            string defaultSpawnerPinItems = "Skeleton:Evil bone pile,Draugr:Body Pile,Greydwarf:Greydwarf nest"; /** @TODO Update the spawner object ids to be correct **/
            spwnsEnabled = Config.Bind("Spawners", "Enabled", true, "Enable/Disable pins for\nOres, Trees, and other destructable resource nodes");
            spwnsLoggingEnabled = Config.Bind("Spawners", "Enable Logging", true, "Log object id and position of each destructable resource node in range of the player.\nUsed to get object Ids to assign to pin types");
            spwnsPinTypes = Config.Bind("Spawners", "Pin Types", defaultSpawnerPinTypes, "Comma seperated list of pin types.\nFormat should be <PinType>:<PinIcon>. Example: Berries:raspberrySprite,Mushroom:mushroomSprite\nVisit nexus for a list of available icon sprite names.\nIf you do not include an icon pin will use a generic white circle.");
            spwnsPinItems = Config.Bind("Spawners", "Pin Items", defaultSpawnerPinItems, "Associate game object ids to pins using a command seperated list.\nFormat should be <PinType>:<ObjectId>.\nYou can include multiple object ids for one pin type by seperating them with a pipe '|'.\nExample: Crypt:Crypt1|Crypt2|Crypt3|Crypt4,Mushroom:Pickable_Mushroom\n**NOTE Pins will automatically attempt to match (Clone) objects so you can just use the base objectId.");

            //***CREATURES***//
            string defaultCreaturePinTypes = "Serpent:serpentSprite,Leviathan:leviathanSprite";
            string defaultCreaturePinItems = "Serpent:$enemy_serpent,Leviathan:Leviathan";
            creaturesEnabled = Config.Bind("Creatures", "Enabled", true, "Enable/Disable pins for\nOres, Trees, and other destructable resource nodes");
            creaturesLoggingEnabled = Config.Bind("Creatures", "Enable Logging", true, "Log object id and position of each destructable resource node in range of the player.\nUsed to get object Ids to assign to pin types");
            creaturesPinTypes = Config.Bind("Creatures", "Pin Types", defaultCreaturePinTypes, "Comma seperated list of pin types.\nFormat should be <PinType>:<PinIcon>. Example: Berries:raspberrySprite,Mushroom:mushroomSprite\nVisit nexus for a list of available icon sprite names.\nIf you do not include an icon pin will use a generic white circle.");
            creaturesPinItems = Config.Bind("Creatures", "Pin Items", defaultCreaturePinItems, "Associate game object ids to pins using a command seperated list.\nFormat should be <PinType>:<ObjectId>.\nYou can include multiple object ids for one pin type by seperating them with a pipe '|'.\nExample: Crypt:Crypt1|Crypt2|Crypt3|Crypt4,Mushroom:Pickable_Mushroom\n**NOTE Pins will automatically attempt to match (Clone) objects so you can just use the base objectId.");

            if (!modEnabled.Value)
                enabled = false;
            else
            {
                new Harmony("materousapps.mods.automappins_configurable").PatchAll();
                
                Harmony.CreateAndPatchAll(typeof(Minimap_Patch), "materousapps.mods.automappins_configurable");
                Harmony.CreateAndPatchAll(typeof(DestructiblePatchSpawn), "materousapps.mods.automappins_configurable");
                Harmony.CreateAndPatchAll(typeof(PickablePatchSpawn), "materousapps.mods.automappins_configurable");
                Harmony.CreateAndPatchAll(typeof(LocationPatchSpawn), "materousapps.mods.automappins_configurable");
                Harmony.CreateAndPatchAll(typeof(SpawnAreaPatchSpawn), "materousapps.mods.automappins_configurable");
                Harmony.CreateAndPatchAll(typeof(MineRockPatchSpawn), "materousapps.mods.automappins_configurable");
                Harmony.CreateAndPatchAll(typeof(Player_Patches), "materousapps.mods.automappins_configurable");

                addedPinLocs = new List<Vector3>();
                dupPinLocs = new List<Vector3>();
                autoPins = new List<Minimap.PinData>();
                pinRemList = new List<Minimap.PinData>();

                /** 
                 * @TODO Preserve these lists in physical conf files 
                 **/
                savedPins = new List<Minimap.PinData>();
                filteredPins = new List<string>();

                Assets.Init();          
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
                //Log.LogInfo(string.Format("[AMP] Checking Distance between Pins {0} & {1}: {2}", aName, pin.m_name, (double)Utils.DistanceXZ(pos, pin.m_pos)));
                if ((double)Utils.DistanceXZ(pos, pin.m_pos) < pinOverlapDistance.Value
                    && type == pin.m_type 
                    && (aName == pin.m_name || aIcon == pin.m_icon))
                {
                    //Log.LogInfo(string.Format("[AMP] Duplicate pins for {0} found", aName));
                    match = pin;
                    return true;
                }
            }
            match = null;
            return false;
        }

        public static void checkPins(Vector3 charPos)
        {
            //Log.LogInfo(string.Format("[AMP] checkingPins? {0}", checkingPins));
            if (checkingPins)
                return;

            foreach (KeyValuePair<Vector3, string> kvp in pinItems)
            {
                checkingPins = true;
                if (Vector3.Distance(charPos, kvp.Key) < pinRange.Value)
                {
                    if (!addedPinLocs.Contains(kvp.Key) && !dupPinLocs.Contains(kvp.Key))
                    {
                        //Log.LogInfo(string.Format("Checking Pin {0} at {1}", kvp.Value, kvp.Key));
                        PinnedObject.pinOb(kvp.Value, kvp.Key);
                    }
                }
                else if (Vector3.Distance(charPos, kvp.Key) > pinRange.Value)
                {
                    foreach (Minimap.PinData tempPin in autoPins)
                    {

                        if (!remPinDict.ContainsKey(kvp.Key) && !tempPin.m_save)
                        {
                            //Log.LogInfo(string.Format("Adding {0} at {1} to removal List", kvp.Value, kvp.Key));
                            remPinDict.Add(kvp.Key, tempPin);
                            pinRemList.Add(tempPin);
                        }
                    }
                }
            }
            checkingPins = false;

            if (pinRemList != null)
            {
                foreach (Minimap.PinData tempRemPin in pinRemList)
                {
                    if (Vector3.Distance(charPos, tempRemPin.m_pos) < pinRange.Value)
                        return;

                    Minimap.instance.RemovePin(tempRemPin);
                    autoPins.Remove(tempRemPin);
                    addedPinLocs.Remove(tempRemPin.m_pos);
                    remPinDict.Remove(tempRemPin.m_pos);
                    //Log.LogInfo(string.Format("Removed Pin {0} at {1}", tempRemPin.m_name, tempRemPin.m_pos));
                }
            }
            pinRemList.Clear();
            
        }

        public static Minimap.PinData GetNearestPin(Vector3 pos, float radius, List<Minimap.PinData> pins)
        {

            Minimap.PinData pinData = null;
            float num1 = 999999f;
            foreach (Minimap.PinData pin in pins)
            {
                float num2 = Utils.DistanceXZ(pos, pin.m_pos);
                if (num2 < radius && (num2 < num1 || pinData == null))
                {
                    pinData = pin;
                    num1 = num2;
                    //Log.LogInfo(string.Format("[AMP] Nearest Pin Type = {0} at {1}", pinData.m_type, pinData.m_pos));
                }
            }
            return pinData;
        }

        public static void FilterPins()
        {
            savedPins.ForEach(pin => pin.m_uiElement?.gameObject.SetActive(ShouldPinRender(pin)));
        }

        private static bool ShouldPinRender(Minimap.PinData pin)
        {
            if (filteredPins == null || filteredPins.Count == 0)
                return true;

            if (!filteredPins.Contains(pin.m_type.ToString()))
                return true;

            return false;
            //return (uint)((IEnumerable<string>)filteredPins).Count(filter => !pin.m_type.ToString().ToLower().Replace(' ', '_').Contains(filter)) > 0U;
        }

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

        public static void pinOb(string tempName, Vector3 aPos)
        {
            if (Mod.currEnv == "Crypt" || Mod.currEnv == "SunkenCrypt" || Mod.currEnv == "FrostCaves" || Mod.currEnv == "InfectedMine")
                return;

            if (tempName != null && tempName != "")
            {
                loadData(tempName);

                //don't show filtered pins.
                if (Mod.filteredPins.Contains(pType.ToString()))
                    return;

                if (Mod.autoPins.Count > 0)
                {
                    if (Mod.SimilarPinExists(aPos, (Minimap.PinType)pType, Mod.autoPins, aName, aIcon, out Minimap.PinData _))
                    {
                        Mod.dupPinLocs.Add(aPos);
                        return;
                    }
                }

                //Mod.Log.LogInfo(string.Format("[AMP] Checking Distance between Player position {0} & {1} position {2}: {3}", GameCamera.instance.transform.position, aName, aPos, Vector3.Distance(GameCamera.instance.transform.position, aPos)));
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
                //Mod.Log.LogInfo(string.Format("[AMP] Added Pin {0} at {1}", pin.m_name, pin.m_pos));

                if (aIcon)
                    pin.m_icon = aIcon;

                pin.m_worldSize = pinSize;
                pin.m_save = aSave;

                
                //Mod.Log.LogInfo(string.Format("[AMP] Tracking: {0} at {1} {2} {3}", aName, aPos.x, aPos.y, aPos.z));
                if(!Mod.autoPins.Contains(pin))
                    Mod.autoPins.Add(pin);
                if(!Mod.addedPinLocs.Contains(aPos))
                    Mod.addedPinLocs.Add(aPos);

                if (pin.m_save && !Mod.savedPins.Contains(pin))
                    Mod.savedPins.Add(pin);
            }
        }

        private void Update()
        {
            
        }

        private void OnDestroy()
        {
            if (pin == null || Minimap.instance == null)
                return;

            loadData(pin.m_type.ToString());
            //Mod.Log.LogInfo(string.Format("OnDestroy Called for Type {0} at {1}", pin.m_type, pin.m_pos));
            //Mod.Log.LogInfo(string.Format("[AMP] Save Pin {0}? = {1}.", pin.m_name, aSave));

            if (aSave || pin.m_save)
            {
                //Mod.Log.LogInfo(string.Format("[AMP] Leaving Pin {0} on map.", pin.m_name));
                return;
            }
            if (Vector3.Distance(pin.m_pos, Player_Patches.currPos) < Mod.pinRange.Value)
                return;

            //Minimap.instance.RemovePin(pin);
            //Mod.autoPins.Remove(pin);
            //Mod.addedPinLocs.Remove(pin.m_pos);
            //Mod.dupPinLocs.Remove(pin.m_pos);
            //Mod.pinItems.Remove(pin.m_pos);
        }

        public static void loadData(string pin)
        {
            switch (pin)
            {
                case "Tin":
                case "101":
                    aName = Mod.tinName.Value;
                    pType = 101;
                    aSave = Mod.saveTin.Value;
                    aIcon = Assets.tinSprite;
                    showName = Mod.showTinName.Value;
                    pinSize = Mod.pinTinSize.Value;
                    break;
                case "Copper":
                case "102":
                    aName = Mod.copperName.Value;
                    pType = 102;
                    aSave = Mod.saveCopper.Value;
                    aIcon = Assets.copperSprite;
                    showName = Mod.showCopperName.Value;
                    pinSize = Mod.pinCopperSize.Value;
                    break;
                case "Obsidian":
                case "103":
                    aName = Mod.obsidianName.Value;
                    pType = 103;
                    aSave = Mod.saveObsidian.Value;
                    aIcon = Assets.obsidianSprite;
                    showName = Mod.showObsidianName.Value;
                    pinSize = Mod.pinObsidianSize.Value;
                    break;
                case "Silver":
                case "104":
                    aName = Mod.silverName.Value;
                    pType = 104;
                    aSave = Mod.saveSilver.Value;
                    aIcon = Assets.silverSprite;
                    showName = Mod.showSilverName.Value;
                    pinSize = Mod.pinSilverSize.Value;
                    break;
                case "Berries":
                case "105":
                    aName = Mod.berriesName.Value;
                    pType = 105;
                    aSave = Mod.saveBerries.Value;
                    aIcon = Assets.raspberrySprite;
                    showName = Mod.showBerriesName.Value;
                    pinSize = Mod.pinBerriesSize.Value;
                    break;
                case "Blueberries":
                case "106":
                    aName = Mod.blueberriesName.Value;
                    pType = 106;
                    aSave = Mod.saveBlueberries.Value;
                    aIcon = Assets.blueberrySprite;
                    showName = Mod.showBlueberriesName.Value;
                    pinSize = Mod.pinBlueberriesSize.Value;
                    break;
                case "Cloudberries":
                case "107":
                    aName = Mod.cloudberriesName.Value;
                    pType = 107;
                    aSave = Mod.saveCloudberries.Value;
                    aIcon = Assets.cloudberrySprite;
                    showName = Mod.showCloudberriesName.Value;
                    pinSize = Mod.pinCloudberriesSize.Value;
                    break;
                case "Thistle":
                case "108":
                    aName = Mod.thistleName.Value;
                    pType = 108;
                    aSave = Mod.saveThistle.Value;
                    aIcon = Assets.thistleSprite;
                    showName = Mod.showThistleName.Value;
                    pinSize = Mod.pinThistleSize.Value;
                    break;
                case "DragonEgg":
                case "109":
                    aName = Mod.dragonEggName.Value;
                    pType = 109;
                    aSave = Mod.saveDragonEgg.Value;
                    aIcon = Assets.eggSprite;
                    showName = Mod.showDragonEggName.Value;
                    pinSize = Mod.pinDragonEggSize.Value;
                    break;
                case "Mushroom":
                case "110":
                    aName = Mod.mushroomName.Value;
                    pType = 110;
                    aSave = Mod.saveMushroom.Value;
                    aIcon = Assets.mushroomSprite;
                    showName = Mod.showMushroomName.Value;
                    pinSize = Mod.pinMushroomSize.Value;
                    break;
                case "Carrot":
                case "111":
                    aName = Mod.carrotName.Value;
                    pType = 111;
                    aSave = Mod.saveCarrot.Value;
                    aIcon = Assets.carrotSprite;
                    showName = Mod.showCarrotName.Value;
                    pinSize = Mod.pinCarrotSize.Value;
                    break;
                case "Turnip":
                case "112":
                    aName = Mod.turnipName.Value;
                    pType = 112;
                    aSave = Mod.saveTurnip.Value;
                    aIcon = Assets.turnipSprite;
                    showName = Mod.showTurnipName.Value;
                    pinSize = Mod.pinTurnipSize.Value;
                    break;
                case "Crypt":
                case "113":
                    aName = Mod.cryptName.Value;
                    pType = 113;
                    aSave = Mod.saveCrypt.Value;
                    aIcon = Assets.cryptSprite;
                    showName = Mod.showCryptName.Value;
                    pinSize = Mod.pinCryptSize.Value;
                    break;
                case "SunkenCrypt":
                case "114":
                    aName = Mod.sunkenCryptName.Value;
                    pType = 114;
                    aSave = Mod.saveSunkenCrypt.Value;
                    aIcon = Assets.sunkenCryptSprite;
                    showName = Mod.showSunkenCryptName.Value;
                    pinSize = Mod.pinSunkenCryptSize.Value;
                    break;
                case "TrollCave":
                case "115":
                    aName = Mod.trollCaveName.Value;
                    pType = 115;
                    aSave = Mod.saveTrollCave.Value;
                    aIcon = Assets.trollCaveSprite;
                    showName = Mod.showTrollCaveName.Value;
                    pinSize = Mod.pinTrollCaveSize.Value;
                    break;
                case "Skeleton":
                case "116":
                    aName = Mod.skeletonName.Value;
                    pType = 116;
                    aSave = Mod.saveSkeleton.Value;
                    aIcon = Assets.skeletonSprite;
                    showName = Mod.showSkeletonName.Value;
                    pinSize = Mod.pinSkeletonSize.Value;
                    break;
                case "Surtling":
                case "117":
                    aName = Mod.surtlingName.Value;
                    pType = 117;
                    aSave = Mod.saveSurtling.Value;
                    aIcon = Assets.surtlingSprite;
                    showName = Mod.showSurtlingName.Value;
                    pinSize = Mod.pinSurtlingSize.Value;
                    break;
                case "Draugr":
                case "118":
                    aName = Mod.draugrName.Value;
                    pType = 118;
                    aSave = Mod.saveDraugr.Value;
                    aIcon = Assets.draugrSprite;
                    showName = Mod.showDraugrName.Value;
                    pinSize = Mod.pinDraugrSize.Value;
                    break;
                case "Greydwarf":
                case "119":
                    aName = Mod.greydwarfName.Value;
                    pType = 119;
                    aSave = Mod.saveGreydwarf.Value;
                    aIcon = Assets.greydwarfSprite;
                    showName = Mod.showGreydwarfName.Value;
                    pinSize = Mod.pinGreydwarfSize.Value;
                    break;
                case "Serpent":
                case "120":
                    aName = Mod.serpentName.Value;
                    pType = 120;
                    aSave = Mod.saveSerpent.Value;
                    aIcon = Assets.serpentSprite;
                    showName = Mod.showSerpentName.Value;
                    pinSize = Mod.pinSerpentSize.Value;
                    break;
                case "Iron":
                case "121":
                    aName = Mod.ironName.Value;
                    pType = 121;
                    aSave = Mod.saveIron.Value;
                    aIcon = Assets.ironSprite;
                    showName = Mod.showIronName.Value;
                    pinSize = Mod.pinIronSize.Value;
                    break;
                case "Flametal":
                case "122":
                    aName = Mod.flametalName.Value;
                    pType = 122;
                    aSave = Mod.saveFlametal.Value;
                    aIcon = Assets.flametalSprite;
                    showName = Mod.showFlametalName.Value;
                    pinSize = Mod.pinFlametalSize.Value;
                    break;
                case "Leviathan":
                case "123":
                    aName = Mod.leviathanName.Value;
                    pType = 123;
                    aSave = Mod.saveLeviathan.Value;
                    aIcon = Assets.leviathanSprite;
                    showName = Mod.showLeviathanName.Value;
                    pinSize = Mod.pinLeviathanSize.Value;
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
        public static Sprite flametalSprite;
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
        public static Sprite leviathanSprite;

        public static Sprite genericTinSprite;
        public static Sprite genericCopperSprite;
        public static Sprite genericSilverSprite;
        public static Sprite genericObsidianSprite;
        public static Sprite genericIronSprite;
        public static Sprite genericRaspberrySprite;
        public static Sprite genericBlueberrySprite;
        public static Sprite genericCloudberrySprite;
        public static Sprite genericThistleSprite;
        public static Sprite genericEggSprite;

        public static void Init() {
            tinSprite = LoadSprite("AMP_Configurable.Resources.TinOre.png");
            copperSprite = LoadSprite("AMP_Configurable.Resources.copperore.png");
            silverSprite = LoadSprite("AMP_Configurable.Resources.silverore.png");
            obsidianSprite = LoadSprite("AMP_Configurable.Resources.obsidian.png");
            ironSprite = LoadSprite("AMP_Configurable.Resources.ironscrap.png");
            flametalSprite = LoadSprite("AMP_Configurable.Resources.flametalore.png");
            raspberrySprite = LoadSprite("AMP_Configurable.Resources.raspberry.png");
            blueberrySprite = LoadSprite("AMP_Configurable.Resources.blueberries.png");
            cloudberrySprite = LoadSprite("AMP_Configurable.Resources.cloudberry.png");
            thistleSprite = LoadSprite("AMP_Configurable.Resources.thistle.png");
            mushroomSprite = LoadSprite("AMP_Configurable.Resources.mushroom.png");
            carrotSprite = LoadSprite("AMP_Configurable.Resources.carrot.png");
            turnipSprite = LoadSprite("AMP_Configurable.Resources.turnip.png");
            eggSprite = LoadSprite("AMP_Configurable.Resources.dragonegg.png");
            cryptSprite = LoadSprite("AMP_Configurable.Resources.surtling_core.png");
            sunkenCryptSprite = LoadSprite("AMP_Configurable.Resources.witheredbone.png");
            trollCaveSprite = LoadSprite("AMP_Configurable.Resources.TrophyFrostTroll.png");
            skeletonSprite = LoadSprite("AMP_Configurable.Resources.TrophySkeleton.png");
            surtlingSprite = LoadSprite("AMP_Configurable.Resources.TrophySurtling.png");
            draugrSprite = LoadSprite("AMP_Configurable.Resources.TrophyDraugr.png");
            greydwarfSprite = LoadSprite("AMP_Configurable.Resources.TrophyGreydwarf.png");
            serpentSprite = LoadSprite("AMP_Configurable.Resources.TrophySerpent.png");
            leviathanSprite = LoadSprite("AMP_Configurable.Resources.chitin.png");

            genericTinSprite = LoadSprite("AMP_Configurable.Resources.mapicon_pin_tin.png");
            genericCopperSprite = LoadSprite("AMP_Configurable.Resources.mapicon_pin_copper.png");
            genericSilverSprite = LoadSprite("AMP_Configurable.Resources.mapicon_pin_silver.png");
            genericObsidianSprite = LoadSprite("AMP_Configurable.Resources.mapicon_pin_obsidian.png");
            genericIronSprite = LoadSprite("AMP_Configurable.Resources.mapicon_pin_iron.png");
            genericRaspberrySprite = LoadSprite("AMP_Configurable.Resources.mapicon_pin_raspberry.png");
            genericBlueberrySprite = LoadSprite("AMP_Configurable.Resources.mapicon_pin_blueberry.png");
            genericCloudberrySprite = LoadSprite("AMP_Configurable.Resources.mapicon_pin_cloudberry.png");
            genericThistleSprite = LoadSprite("AMP_Configurable.Resources.mapicon_pin_thistle.png");
            genericEggSprite = LoadSprite("AMP_Configurable.Resources.mapicon_egg.png");
        }

        internal static Texture2D LoadTexture(byte[] file)
        {
            if (((IEnumerable<byte>)file).Count<byte>() > 0)
            {
                Texture2D texture2D = new Texture2D(2, 2);
                if (ImageConversion.LoadImage(texture2D, file))
                {
                    return texture2D;
                }
            }
            return (Texture2D)null;
        }

        public static Sprite LoadSprite(string iconPath, float PixelsPerUnit = 50f) {
            Texture2D SpriteTexture = LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), iconPath));
            return SpriteTexture? Sprite.Create(SpriteTexture, new Rect(0.0f, 0.0f, SpriteTexture.width, SpriteTexture.height), new Vector2(0.0f, 0.0f), PixelsPerUnit) : (Sprite)null;
        }
    }
}
