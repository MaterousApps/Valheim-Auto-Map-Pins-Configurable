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
using AutoPins.Patches;

namespace AutoPins
{
    [BepInPlugin("materousapps.mods.automappins_configurable", "Auto Map Pins", "1.0.0")]
    [BepInProcess("valheim.exe")]
    public class Mod : BaseUnityPlugin
    {
        public static ManualLogSource Log;
        public static ConfigEntry<int> nexusID;
        public static ConfigEntry<bool> modEnabled;
        public static ConfigEntry<float> pinOverlapDistance;
        public static ConfigEntry<int> pinIcons;
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
        public static ConfigEntry<bool> pinDragonEgg;
        public static ConfigEntry<bool> saveDragonEgg;
        public static ConfigEntry<bool> showDragonEggName;
        public static ConfigEntry<float> pinDragonEggSize;

        public static List<Minimap.PinData> autoPins;



        private void Awake()
        {
            Mod.Log = this.Logger;

            AutoPins.Mod.modEnabled = Config.Bind("1. General", "Enabled", true, "Enable this mod");
            AutoPins.Mod.pinOverlapDistance = Config.Bind<float>("1. General", "PinOverlapDistance", 5, "Distance around pins to prevent overlapping of similar pins");
            AutoPins.Mod.pinIcons = Config.Bind<int>("1. General", "PinIcons", 2, "Use [1] Game sprites(copper for copper, silver for silver, etc.) or [2] Colored pins (must be 1 or 2)");
            //AutoPins.Mod.nexusID = Config.Bind<int>("General", "NexusID", 274, "Nexus mod ID for updates");
            AutoPins.Mod.pinCopper = Config.Bind("2. Copper", "PinCopper", true, "Show pins for Copper");
            AutoPins.Mod.saveCopper = Config.Bind("2. Copper", "SaveCopper", false, "Save pins for Copper");
            AutoPins.Mod.showCopperName = Config.Bind("2. Copper", "ShowCopperName", true, "Show name for Copper");
            AutoPins.Mod.pinCopperSize = Config.Bind<float>("2. Copper", "PinCopperSize", 15, "Size of Copper pin on minimap/main Map (10-20 is recommended)");
            AutoPins.Mod.pinTin = Config.Bind("3. Tin", "PinTin", true, "Show pins for Tin");
            AutoPins.Mod.saveTin = Config.Bind("3. Tin", "SaveTin", false, "Save pins for Tin");
            AutoPins.Mod.showTinName = Config.Bind("3. Tin", "ShowTinName", true, "Show name for Tin");
            AutoPins.Mod.pinTinSize = Config.Bind<float>("3. Tin", "PinTinSize", 15, "Size of pin Tin on minimap/main Map (10-20 is recommended)");
            AutoPins.Mod.pinObsidian = Config.Bind("4. Obsidian", "PinObsidian", true, "Show pins for Obsidian");
            AutoPins.Mod.saveObsidian = Config.Bind("4. Obsidian", "SaveObsidian", false, "Save pins for Obsidian");
            AutoPins.Mod.showObsidianName = Config.Bind("4. Obsidian", "ShowObsidianName", true, "Show name for Obsidian");
            AutoPins.Mod.pinObsidianSize = Config.Bind<float>("4. Obsidian", "PinObsidianSize", 15, "Size of Obsidian pin on minimap/main Map (10-20 is recommended)");
            AutoPins.Mod.pinSilver = Config.Bind("5. Silver", "PinSilver", true, "Show pins for Silver");
            AutoPins.Mod.saveSilver = Config.Bind("5. Silver", "SaveSilver", false, "Save pins for Silver");
            AutoPins.Mod.showSilverName = Config.Bind("5. Silver", "ShowSilverName", true, "Show name for Silver");
            AutoPins.Mod.pinSilverSize = Config.Bind<float>("5. Silver", "PinSilverSize", 15, "Size of Silver pin on minimap/main Map (10-20 is recommended)");
            AutoPins.Mod.pinBerries = Config.Bind("6. Berries", "PinBerries", true, "Show pins for Berries");
            AutoPins.Mod.saveBerries = Config.Bind("6. Berries", "SaveBerries", false, "Save pins for Berries");
            AutoPins.Mod.showBerriesName = Config.Bind("6. Berries", "ShowBerriesName", true, "Show name for Berries");
            AutoPins.Mod.pinBerriesSize = Config.Bind<float>("6. Berries", "PinBerriesSize", 15, "Size of Berries pin on minimap/main Map (10-20 is recommended)");
            AutoPins.Mod.pinBlueberries = Config.Bind("7. Blueberries", "PinBlueberries", true, "Show pins for Blueberries");
            AutoPins.Mod.saveBlueberries = Config.Bind("7. Blueberries", "SaveBlueberries", false, "Save pins for Blueberries");
            AutoPins.Mod.showBlueberriesName = Config.Bind("7. Blueberries", "ShowBlueberriesName", true, "Show name for Blueberries");
            AutoPins.Mod.pinBlueberriesSize = Config.Bind<float>("7. Blueberries", "PinBlueberriesSize", 15, "Size of Blueberries pin on minimap/main Map (10-20 is recommended)");
            AutoPins.Mod.pinCloudberries = Config.Bind("8. Cloudberries", "PinCloudberries", true, "Show pins for Cloudberries");
            AutoPins.Mod.saveCloudberries = Config.Bind("8. Cloudberries", "SaveCloudberries", false, "Save pins for Cloudberries");
            AutoPins.Mod.showCloudberriesName = Config.Bind("8. Cloudberries", "ShowCloudberriesName", true, "Show name for Cloudberries");
            AutoPins.Mod.pinCloudberriesSize = Config.Bind<float>("8. Cloudberries", "PinCloudberriesSize", 15, "Size of Cloudberries pin on minimap/main Map (10-20 is recommended)");
            AutoPins.Mod.pinThistle = Config.Bind("9. Thistle", "PinThistle", true, "Show pins for Thistle");
            AutoPins.Mod.saveThistle = Config.Bind("9. Thistle", "SaveThistle", false, "Save pins for Thistle");
            AutoPins.Mod.showThistleName = Config.Bind("9. Thistle", "ShowThistleName", true, "Show name for Thistle");
            AutoPins.Mod.pinThistleSize = Config.Bind<float>("9. Thistle", "PinThistleSize", 15, "Size of Thistle pin on minimap/main Map (10-20 is recommended)");
            AutoPins.Mod.pinDragonEgg = Config.Bind("10. Dragon Eggs", "PinDragonEgg", true, "Show pins for Dragon Eggs");
            AutoPins.Mod.saveDragonEgg = Config.Bind("10. Dragon Eggs", "SaveDragonEgg", false, "Save pins for Dragon Eggs");
            AutoPins.Mod.showDragonEggName = Config.Bind("10. Dragon Eggs", "ShowDragonEggName", true, "Show name for Dragon Eggs");
            AutoPins.Mod.pinDragonEggSize = Config.Bind<float>("10. Dragon Eggs", "PinDragonEggSize", 15, "Size of Dragon Eggs pin on minimap/main Map (10-20 is recommended)");

            if (!modEnabled.Value)
                enabled = false;
            else
            {
                new Harmony("materousapps.mods.automappins_configurable").PatchAll();
                Assets.Init(pinIcons.Value); 
                Harmony.CreateAndPatchAll(typeof(Minimap_Patch), "materousapps.mods.automappins_configurable");
                Harmony.CreateAndPatchAll(typeof(DestructiblePatchSpawn), "materousapps.mods.automappins_configurable");
                Harmony.CreateAndPatchAll(typeof(PickablePatchSpawn), "materousapps.mods.automappins_configurable");
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
                //Mod.Log.LogInfo(string.Format(" Checking Distance between Pins {0} & {1}: {2}", aName, pin.m_name, (double)Utils.DistanceXZ(pos, pin.m_pos)));
                if ((double)Utils.DistanceXZ(pos, pin.m_pos) < (double)pinOverlapDistance.Value && type == pin.m_type && (aName == pin.m_name || aIcon == pin.m_icon))
                {
                    //Mod.Log.LogInfo(string.Format("Duplicate pins for {0} found", aName));
                    match = pin;
                    return true;
                }
            }
            match = (Minimap.PinData)null;
            return false;
        }

        public Mod()
        {
            return;
        }
    }

    internal class PinnedObject : MonoBehaviour
    {
        public Minimap.PinData pin;
        public static bool aSave = false;
        public static bool showName = false;
        public static float pinSize = 15;
        public static Sprite aIcon;

        public void Init(string aName)
        {
            if (aName != null && aName != "")
            {
                loadData(aName);

                if (Mod.autoPins == null)
                {
                    Mod.autoPins = new List<Minimap.PinData>();
                }

                if (Mod.autoPins.Count > 0)
                {
                    if (Mod.SimilarPinExists(transform.position, (Minimap.PinType)3, Mod.autoPins, aName, aIcon, out Minimap.PinData _))
                    {
                        return;
                    }
                }

                if(showName)
                {
                    pin = Minimap.instance.AddPin(transform.position, (Minimap.PinType)3, aName, false, false);
                }
                else
                {
                    pin = Minimap.instance.AddPin(transform.position, (Minimap.PinType)3, string.Empty, false, false);
                }
                
                if (aIcon)
                {
                    pin.m_icon = aIcon;
                    pin.m_worldSize = pinSize;
                }
                //Mod.Log.LogInfo(string.Format("Tracking: {0} at {1} {2} {3}", aName, transform.position.x, transform.position.y, transform.position.z));
                Mod.autoPins.Add(pin);
            }
        }

        private void OnDestroy()
        {
            if (pin == null || Minimap.instance == null)
                return;

            loadData(pin.m_name);

            if (aSave)
            {
                return;
            }

            Minimap.instance.RemovePin(pin);
            Mod.autoPins.Clear();
            //Mod.Log.LogInfo(string.Format("Should we save: {0} - {1}", pin.m_name, aSave));
            //Mod.Log.LogInfo(string.Format("Removing: {0} at {1} {2} {3}", pin.m_name, transform.position.x, transform.position.y, transform.position.z));
        }
        public void loadData(string aName)
        {
            switch (aName)
            {
                case "Tin":
                    aSave = Mod.saveTin.Value;
                    aIcon = Assets.tinSprite;
                    showName = Mod.showTinName.Value;
                    pinSize = Mod.pinTinSize.Value;
                    break;
                case "Copper":
                    aSave = Mod.saveCopper.Value;
                    aIcon = Assets.copperSprite;
                    showName = Mod.showCopperName.Value;
                    pinSize = Mod.pinCopperSize.Value;
                    break;
                case "Obsidian":
                    aSave = Mod.saveObsidian.Value;
                    aIcon = Assets.obsidianSprite;
                    showName = Mod.showObsidianName.Value;
                    pinSize = Mod.pinObsidianSize.Value;
                    break;
                case "Silver":
                    aSave = Mod.saveSilver.Value;
                    aIcon = Assets.silverSprite;
                    showName = Mod.showSilverName.Value;
                    pinSize = Mod.pinSilverSize.Value;
                    break;
                case "Berries":
                    aSave = Mod.saveBerries.Value;
                    aIcon = Assets.raspberrySprite;
                    showName = Mod.showBerriesName.Value;
                    pinSize = Mod.pinBerriesSize.Value;
                    break;
                case "Blueberries":
                    aSave = Mod.saveBlueberries.Value;
                    aIcon = Assets.blueberrySprite;
                    showName = Mod.showBlueberriesName.Value;
                    pinSize = Mod.pinBlueberriesSize.Value;
                    break;
                case "Cloudberries":
                    aSave = Mod.saveCloudberries.Value;
                    aIcon = Assets.cloudberrySprite;
                    showName = Mod.showCloudberriesName.Value;
                    pinSize = Mod.pinCloudberriesSize.Value;
                    break;
                case "Thistle":
                    aSave = Mod.saveThistle.Value;
                    aIcon = Assets.thistleSprite;
                    showName = Mod.showThistleName.Value;
                    pinSize = Mod.pinThistleSize.Value;
                    break;
                case "DragonEgg":
                    aSave = Mod.saveDragonEgg.Value;
                    aIcon = Assets.eggSprite;
                    showName = Mod.showDragonEggName.Value;
                    pinSize = Mod.pinDragonEggSize.Value;
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
        public static Sprite eggSprite;

        public static void Init(int pinIcons)
        {
            Mod.Log.LogInfo(string.Format("pinIcons = {0}", pinIcons));
            switch (pinIcons)
            {
                case 1:
                    tinSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AutoPins.Resources.TinOre.png")));
                    copperSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AutoPins.Resources.copperore.png")));
                    silverSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AutoPins.Resources.silverore.png")));
                    obsidianSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AutoPins.Resources.obsidian.png")));
                    //ironSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AutoPins.Resources.ironscrap.png")));
                    raspberrySprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AutoPins.Resources.raspberry.png")));
                    blueberrySprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AutoPins.Resources.blueberries.png")));
                    cloudberrySprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AutoPins.Resources.cloudberry.png")));
                    thistleSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AutoPins.Resources.thistle.png")));
                    eggSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AutoPins.Resources.dragonegg.png")));
                    break;
                /*case 3:

                    break;*/
                default:
                    //Mod.Log.LogInfo(string.Format("using colored icons"));
                    tinSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AutoPins.Resources.mapicon_pin_tin.png")));
                    copperSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AutoPins.Resources.mapicon_pin_copper.png")));
                    silverSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AutoPins.Resources.mapicon_pin_silver.png")));
                    obsidianSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AutoPins.Resources.mapicon_pin_obsidian.png")));
                    //ironSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AutoPins.Resources.mapicon_pin_iron.png")));
                    raspberrySprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AutoPins.Resources.mapicon_pin_raspberry.png")));
                    blueberrySprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AutoPins.Resources.mapicon_pin_blueberry.png")));
                    cloudberrySprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AutoPins.Resources.mapicon_pin_cloudberry.png")));
                    thistleSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AutoPins.Resources.mapicon_pin_thistle.png")));
                    eggSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AutoPins.Resources.mapicon_egg.png")));
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
