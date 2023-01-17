using AMP_Configurable.Patches;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;

namespace AMP_Configurable
{
  [BepInPlugin("amped.mod.auto_map_pins", "AMPED - Auto Map Pins Enhanced", "1.3.4")]
  [BepInProcess("valheim.exe")]
  public class Mod : BaseUnityPlugin
  {
    //***CONFIG ENTRIES***//
    //***GENERAL***//
    public static ManualLogSource Log;
    public static ConfigEntry<int> nexusID;
    public static ConfigEntry<bool> modEnabled;
    public static ConfigEntry<float> pinOverlapDistance;
    public static ConfigEntry<int> pinRange;
    public static ConfigEntry<bool> hideAllLabels;
    public static ConfigEntry<string> hidePinLabels;
    public static ConfigEntry<string> hidePinTypes;
    public static ConfigEntry<string> savePinTypes;
    public static ConfigEntry<string> customPinSizes;
    public static ConfigEntry<bool> loggingEnabled;
    public static ConfigEntry<string> customConfigFiles;

    //***ORES***//
    public static ConfigEntry<bool> destructablesEnabled;
    public static ConfigEntry<bool> destructableLoggingEnabled;

    //***PICKABLES***//
    public static ConfigEntry<bool> pickablesEnabled;
    public static ConfigEntry<bool> pickablesLoggingEnabled;

    //***LOCATIONS***//
    public static ConfigEntry<bool> locsEnabled;
    public static ConfigEntry<bool> locsLoggingEnabled;

    //***SPAWNERS***//
    public static ConfigEntry<bool> spwnsEnabled;
    public static ConfigEntry<bool> spwnsLoggingEnabled;

    //***CREATURES***//
    public static ConfigEntry<bool> creaturesEnabled;
    public static ConfigEntry<bool> creaturesLoggingEnabled;

    //***PUBLIC VARIABLES***//
    public static IDictionary<int, PinConfig.PinType> mtypePins = new Dictionary<int, PinConfig.PinType>();
    public static IDictionary<string, int> objectPins = new Dictionary<string, int>();
    public static List<Minimap.PinData> autoPins;
    public static List<Minimap.PinData> savedPins;
    public static List<Minimap.PinData> pinRemList;
    public static List<Vector3> addedPinLocs;
    public static List<Vector3> dupPinLocs;
    public static List<string> filteredPins;
    public static Vector3 position;
    public static Dictionary<Vector3, PinConfig.PinType> pinItems = new Dictionary<Vector3, PinConfig.PinType>();
    public static Dictionary<Vector3, Minimap.PinData> remPinDict = new Dictionary<Vector3, Minimap.PinData>();
    public static bool hasMoved = false;
    public static bool checkingPins = false;
    public static string currEnv = "";

    private void Awake()
    {
      Log = Logger;

      /** General Config **/
      nexusID = Config.Bind("_General_", "1. NexusID", 2199, "Nexus mod ID for updates");
      modEnabled = Config.Bind("_General_", "2. Enabled", true, "Enable this mod");

      pinOverlapDistance = Config.Bind<float>("1. Pins", "1. PinOverlapDistance", 10, "Distance around pins to prevent overlapping of similar pins");
      pinRange = Config.Bind("1. Pins", "2. Pin Range", 100, "Sets the range that pins will appear on the mini-map. Lower value means you need to be closer to set pin.\nMin 5\nMax 150\nRecommended 50-75");
      if (pinRange.Value < 5) pinRange.Value = 5;
      if (pinRange.Value > 150) pinRange.Value = 150;
      hideAllLabels = Config.Bind("1. Pins", "3. Hide All Labels", false, "Hide all pin labels.\n*THIS WILL OVERRIDE THE INDIVIDUAL SETTINGS*");
      hidePinLabels = Config.Bind("1. Pins", "4. Hide Pin Label", "", "Hide individual pin type labels.\nValue should be a comma seperated list of pin types.");
      hidePinTypes = Config.Bind("1. Pins", "5. Hide Pin Label", "", "Hide individual pin types.\nValue should be a comma seperated list of pin types.");
      string defaultSavePinTypes = "Crypt,Troll Cave,Sunken Crypt,Frost Cave,Infested Mine";
      savePinTypes = Config.Bind("1. Pins", "6. Save Pin Types", defaultSavePinTypes, "These Pin Types will persist on the map after the player as left the area.\nValue should be a comma seperated list of pin types.");

      destructablesEnabled = Config.Bind("2. Pins Enable/Disable", "1. Resources", true, "Enable/Disable pins for\nOres, Trees, and other destructable resource nodes");
      pickablesEnabled = Config.Bind("2. Pins Enable/Disable", "2. Pickables", true, "Enable/Disable pins for\nBerries, Mushrooms, and other pickable items");
      locsEnabled = Config.Bind("2. Pins Enable/Disable", "3. Locations", true, "Enable/Disable pins for\nCrypts, Troll Caves, and other discoverable locations");
      spwnsEnabled = Config.Bind("2. Pins Enable/Disable", "4. Spawners", true, "Enable/Disable pins for\nGreydwarf nests, Skeleton Bone Piles, and other creature spawners");
      creaturesEnabled = Config.Bind("2. Pins Enable/Disable", "5. Creatures", true, "Enable/Disable pins for\nSerpents, and other creatures when they spawn with in range of the player");

      loggingEnabled = Config.Bind("3. Logging", "1. Enable Logging", true, "Enable Logging");
      destructableLoggingEnabled = Config.Bind("3. Logging", "2. Resource Logging", false, "Log object id and position of each destructable resource node in range of the player.\nUsed to get object Ids to assign to pin types");
      pickablesLoggingEnabled = Config.Bind("3. Logging", "3. Pickables Logging", false, "Log object id and position of each pickable item in range of the player.\nUsed to get object Ids to assign to pin types");
      locsLoggingEnabled = Config.Bind("3. Logging", "4. Locations Logging", false, "Log object id and position of each location object in range of the player.\nUsed to get object Ids to assign to pin types");
      spwnsLoggingEnabled = Config.Bind("3. Logging", "5. Spawners Logging", false, "Log object id and position of each creature spawner node in range of the player.\nUsed to get object Ids to assign to pin types");
      creaturesLoggingEnabled = Config.Bind("3. Logging", "6. Creatures Logging", false, "Log object id and position of creatures that spawn in range of the player.\nUsed to get object Ids to assign to pin types");

      /** Load Pin Type Config from JSON files **/
      string[] hidePins = hidePinTypes.Value.Split(',');
      string[] configFiles = ResourceUtils.GetPinConfigFiles();
      PinConfig.PinConfig pinTypes = null;
      objectPins = new Dictionary<string, int>();
      mtypePins = new Dictionary<int, PinConfig.PinType>();

      if (configFiles != null)
        foreach (string confFile in configFiles)
        {
          Mod.Log.LogInfo($"Loading pin config file: {Path.GetFileName(confFile)}");
          pinTypes = ResourceUtils.LoadPinConfig(confFile);
          foreach (PinConfig.PinType pinType in pinTypes.pins)
          {
            // Load mtype reference dictionary
            if (!hidePins.Contains(pinType.label))
              mtypePins[pinType.type] = pinType;

            // Load objectId referance dictionary 
            foreach (string objectId in pinType.object_ids)
              if (!hidePins.Contains(pinType.label))
                objectPins[objectId] = pinType.type;
          }
        }

      if (!modEnabled.Value)
      {
        enabled = false;
      }
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
        if (pos == pin.m_pos)
        {
          match = pin;
          return true;
        }
        else

        if ((double)Utils.DistanceXZ(pos, pin.m_pos) < pinOverlapDistance.Value
            && type == pin.m_type
            && (aName == pin.m_name || aIcon == pin.m_icon))
        {
          match = pin;
          return true;
        }
      }
      match = null;
      return false;
    }

    public static void checkPins(Vector3 charPos)
    {
      if (checkingPins) return;

      foreach (KeyValuePair<Vector3, PinConfig.PinType> kvp in pinItems)
      {
        checkingPins = true;
        if (Vector3.Distance(charPos, kvp.Key) < pinRange.Value)
        {
          if (!addedPinLocs.Contains(kvp.Key) && !dupPinLocs.Contains(kvp.Key))
          {
            PinnedObject.pinOb(kvp.Value, kvp.Key);
          }
        }
        else if (Vector3.Distance(charPos, kvp.Key) > pinRange.Value)
        {
          foreach (Minimap.PinData tempPin in autoPins)
          {

            if (!remPinDict.ContainsKey(kvp.Key) && !tempPin.m_save)
            {
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

    public static void pinOb(PinConfig.PinType tempPin, Vector3 aPos)
    {
      if (Mod.currEnv == "Crypt" || Mod.currEnv == "SunkenCrypt" || Mod.currEnv == "FrostCaves" || Mod.currEnv == "InfectedMine")
        return;

      if (tempPin != null)
      {
        loadData(tempPin);

        //don't show filtered pins.
        if (Mod.filteredPins.Contains(pType.ToString()) || aName == "")
          return;

        if (Mod.autoPins.Count > 0)
        {
          if (Mod.SimilarPinExists(aPos, (Minimap.PinType)pType, Mod.autoPins, aName, aIcon, out Minimap.PinData _))
          {
            Mod.dupPinLocs.Add(aPos);
            return;
          }
        }

        if (Mod.hideAllLabels.Value)
          showName = false;

        if (showName)
        {
          pin = Minimap.instance.AddPin(aPos, (Minimap.PinType)pType, aName, aSave, false);
        }
        else
        {
          pin = Minimap.instance.AddPin(aPos, (Minimap.PinType)pType, string.Empty, aSave, false);
        }

        if (aIcon)
          pin.m_icon = aIcon;

        pin.m_worldSize = pinSize;
        pin.m_save = aSave;

        if (!Mod.autoPins.Contains(pin))
          Mod.autoPins.Add(pin);
        if (!Mod.addedPinLocs.Contains(aPos))
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

      loadData(null, pin.m_type.ToString());

      if (aSave || pin.m_save)
      {
        return;
      }
      if (Vector3.Distance(pin.m_pos, Player_Patches.currPos) < Mod.pinRange.Value)
        return;
    }

    public static void loadData(PinConfig.PinType pin = null, string m_type = null)
    {
      /** loadData called with minimap pin type data **/
      if (pin == null && m_type != null)
      {
        pType = Int32.Parse(m_type);
        if (!Mod.mtypePins.ContainsKey(pType))
        {
          if (Mod.loggingEnabled.Value) Mod.Log.LogInfo($"[AMP] Failed to load pin data from minimap pin type {pType}");
          aName = "";
          return;
        }

        pin = Mod.mtypePins[pType];
        if (Mod.loggingEnabled.Value) Mod.Log.LogInfo($"[AMP] Loading pin {pin.label} from minimap type {pType}");
      }

      aName = pin.label;
      pType = pin.type;
      aSave = Mod.savePinTypes.Value.Split(',').Contains(pin.label);
      aIcon = pin.sprite;
      showName = !Mod.hidePinLabels.Value.Split(',').Contains(pin.label);
      pinSize = pin.size;
    }

    public PinnedObject()
    {
      return;
    }
  }

  public class Assets
  {
    public static void Init()
    {
      foreach (KeyValuePair<int, PinConfig.PinType> mtype in Mod.mtypePins)
        mtype.Value.sprite = LoadSprite(mtype.Value.icon);
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

    public static Sprite LoadSprite(string iconPath, float PixelsPerUnit = 50f)
    {
      Texture2D SpriteTexture = LoadTexture(ResourceUtils.GetResource(iconPath));
      return SpriteTexture ? Sprite.Create(SpriteTexture, new Rect(0.0f, 0.0f, SpriteTexture.width, SpriteTexture.height), new Vector2(0.0f, 0.0f), PixelsPerUnit) : (Sprite)null;
    }
  }
}
