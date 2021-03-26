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

            modEnabled = Config.Bind("General", "Enabled", true, "Enable this mod");
            pinOverlapDistance = Config.Bind<float>("General", "PinOverlapDistance", 5, "Distance around pins to prevent overlapping of similar pins");
            pinIcons = Config.Bind<int>("General", "PinIcons", 1, "Use [1] Game sprites(copper for copper, silver for silver, etc.) or [2] Colored pins (must be 1 or 2)");
            //AutoPins.Mod.nexusID = Config.Bind<int>("General", "NexusID", 274, "Nexus mod ID for updates");
            pinCopper = Config.Bind("Ores - Copper", "PinCopper", true, "Show pins for Copper");
            saveCopper = Config.Bind("Ores - Copper", "SaveCopper", false, "Save pins for Copper");
            showCopperName = Config.Bind("Ores - Copper", "ShowCopperName", true, "Show name for Copper");
            pinCopperSize = Config.Bind<float>("Ores - Copper", "PinCopperSize", 15, "Size of Copper pin on minimap/main Map (10-20 is recommended)");
            pinTin = Config.Bind("Ores - Tin", "PinTin", true, "Show pins for Tin");
            saveTin = Config.Bind("Ores - Tin", "SaveTin", false, "Save pins for Tin");
            showTinName = Config.Bind("Ores - Tin", "ShowTinName", true, "Show name for Tin");
            pinTinSize = Config.Bind<float>("Ores - Tin", "PinTinSize", 15, "Size of pin Tin on minimap/main Map (10-20 is recommended)");
            pinObsidian = Config.Bind("Ores - Obsidian", "PinObsidian", true, "Show pins for Obsidian");
            saveObsidian = Config.Bind("Ores - Obsidian", "SaveObsidian", false, "Save pins for Obsidian");
            showObsidianName = Config.Bind("Ores - Obsidian", "ShowObsidianName", true, "Show name for Obsidian");
            pinObsidianSize = Config.Bind<float>("Ores - Obsidian", "PinObsidianSize", 15, "Size of Obsidian pin on minimap/main Map (10-20 is recommended)");
            pinSilver = Config.Bind("Ores - Silver", "PinSilver", true, "Show pins for Silver");
            saveSilver = Config.Bind("Ores - Silver", "SaveSilver", false, "Save pins for Silver");
            showSilverName = Config.Bind("Ores - Silver", "ShowSilverName", true, "Show name for Silver");
            pinSilverSize = Config.Bind<float>("Ores - Silver", "PinSilverSize", 15, "Size of Silver pin on minimap/main Map (10-20 is recommended)");
            pinBerries = Config.Bind("Pickables - Berries", "PinBerries", true, "Show pins for Berries");
            saveBerries = Config.Bind("Pickables - Berries", "SaveBerries", false, "Save pins for Berries");
            showBerriesName = Config.Bind("Pickables - Berries", "ShowBerriesName", true, "Show name for Berries");
            pinBerriesSize = Config.Bind<float>("Pickables - Berries", "PinBerriesSize", 15, "Size of Berries pin on minimap/main Map (10-20 is recommended)");
            pinBlueberries = Config.Bind("Pickables - Blueberries", "PinBlueberries", true, "Show pins for Blueberries");
            saveBlueberries = Config.Bind("Pickables - Blueberries", "SaveBlueberries", false, "Save pins for Blueberries");
            showBlueberriesName = Config.Bind("Pickables - Blueberries", "ShowBlueberriesName", true, "Show name for Blueberries");
            pinBlueberriesSize = Config.Bind<float>("Pickables - Blueberries", "PinBlueberriesSize", 15, "Size of Blueberries pin on minimap/main Map (10-20 is recommended)");
            pinCloudberries = Config.Bind("Pickables - Cloudberries", "PinCloudberries", true, "Show pins for Cloudberries");
            saveCloudberries = Config.Bind("Pickables - Cloudberries", "SaveCloudberries", false, "Save pins for Cloudberries");
            showCloudberriesName = Config.Bind("Pickables - Cloudberries", "ShowCloudberriesName", true, "Show name for Cloudberries");
            pinCloudberriesSize = Config.Bind<float>("Pickables - Cloudberries", "PinCloudberriesSize", 15, "Size of Cloudberries pin on minimap/main Map (10-20 is recommended)");
            pinThistle = Config.Bind("Pickables - Thistle", "PinThistle", true, "Show pins for Thistle");
            saveThistle = Config.Bind("Pickables - Thistle", "SaveThistle", false, "Save pins for Thistle");
            showThistleName = Config.Bind("Pickables - Thistle", "ShowThistleName", true, "Show name for Thistle");
            pinThistleSize = Config.Bind<float>("Pickables - Thistle", "PinThistleSize", 15, "Size of Thistle pin on minimap/main Map (10-20 is recommended)");
            pinDragonEgg = Config.Bind("Misc - Dragon Eggs", "PinDragonEgg", true, "Show pins for Dragon Eggs");
            saveDragonEgg = Config.Bind("Misc - Dragon Eggs", "SaveDragonEgg", false, "Save pins for Dragon Eggs");
            showDragonEggName = Config.Bind("Misc - Dragon Eggs", "ShowDragonEggName", true, "Show name for Dragon Eggs");
            pinDragonEggSize = Config.Bind<float>("Misc - Dragon Eggs", "PinDragonEggSize", 15, "Size of Dragon Eggs pin on minimap/main Map (10-20 is recommended)");

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
                if ((double)Utils.DistanceXZ(pos, pin.m_pos) < pinOverlapDistance.Value && type == pin.m_type && (aName == pin.m_name || aIcon == pin.m_icon))
                {
                    //Mod.Log.LogInfo(string.Format("Duplicate pins for {0} found", aName));
                    match = pin;
                    return true;
                }
            }
            match = null;
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
                    tinSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.TinOre.png")));
                    copperSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.copperore.png")));
                    silverSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.silverore.png")));
                    obsidianSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.obsidian.png")));
                    //ironSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.ironscrap.png")));
                    raspberrySprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.raspberry.png")));
                    blueberrySprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.blueberries.png")));
                    cloudberrySprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.cloudberry.png")));
                    thistleSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.thistle.png")));
                    eggSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.dragonegg.png")));
                    break;
                default:
                    tinSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.mapicon_pin_tin.png")));
                    copperSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.mapicon_pin_copper.png")));
                    silverSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.mapicon_pin_silver.png")));
                    obsidianSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.mapicon_pin_obsidian.png")));
                    //ironSprite = LoadSprite(LoadTexture(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "AMP_Configurable.Resources.mapicon_pin_iron.png")));
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
