using AMP_Configurable.Commands;
using AMP_Configurable.PinConfig;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AMP_Configurable.Patches
{
  internal class Minimap_Patch : MonoBehaviour
  {
    public static int count = 0;
    public static bool checkedSavedPins = false;

    [HarmonyReversePatch]
    [HarmonyPatch(typeof(Minimap), "ScreenToWorldPoint", new System.Type[] { typeof(Vector3) })]
    public static Vector3 ScreenToWorldPoint(object instance, Vector3 screenPos) => throw new NotImplementedException();

    [HarmonyPatch(typeof(Minimap), "Awake")]
    [HarmonyPostfix]
    private static void Minimap_Awake()
    {
      checkedSavedPins = false;
      Mod.addedPinLocs.Clear();
      Mod.dupPinLocs.Clear();
      Mod.autoPins.Clear();
      Mod.pinRemList.Clear();
      Mod.savedPins.Clear();
    }

    [HarmonyPatch(typeof(Minimap), "Start")]
    [HarmonyPostfix]
    private static void Minimap_Start(
        Minimap __instance,
        ref bool[] ___m_visibleIconTypes)
    {
      ___m_visibleIconTypes = new bool[150];
      for (int index = 0; index < ___m_visibleIconTypes.Length; index++)
      {
        ___m_visibleIconTypes[index] = true;
      }
    }

    [HarmonyPatch(typeof(Minimap), "AddPin")]
    [HarmonyPrefix]
    private static bool Minimap_AddPin(
      ref Minimap __instance,
      List<Minimap.PinData> ___m_pins,
      Vector3 pos,
      Minimap.PinType type,
      string name,
      bool save,
      bool isChecked)
    {
      bool shouldAddPin = ((type != Minimap.PinType.Death ? 0 : (Mod.SimilarPinExists(pos, type, ___m_pins, name, PinnedObject.aIcon, out Minimap.PinData _) ? 1 : 0)) & (save ? 1 : 0)) == 0;
      if (Mod.loggingEnabled.Value && shouldAddPin) Mod.Log.LogInfo($"[AMP] Trying to add pin {type}");
      return shouldAddPin;
    }

    [HarmonyPatch(typeof(Minimap), "UpdateProfilePins")]
    [HarmonyPrefix]
    private static void Minimap_UpdateProfilePins(
      ref List<Minimap.PinData> ___m_pins)
    {
      if (!checkedSavedPins)
      {
        foreach (Minimap.PinData pins in ___m_pins)
        {
          if ((int)pins.m_type >= 100)
          {
            if (!Mod.savedPins.Contains(pins))
              Mod.savedPins.Add(pins);

            PinnedObject.loadData(null, pins.m_type.ToString());

            if (pins.m_type == (Minimap.PinType)PinnedObject.pType && !Mod.filteredPins.Contains(PinnedObject.aName))
            {
              pins.m_icon = PinnedObject.aIcon;
              pins.m_worldSize = PinnedObject.pinSize;
              if (!PinnedObject.showName)
              {
                pins.m_name = string.Empty;
              }
            }
          }
        }
        checkedSavedPins = true;

        Mod.checkPins(Player_Patches.currPos);
      }

    }

    [HarmonyPatch(typeof(Minimap), "UpdatePins")]
    [HarmonyPrefix]
    private static void Minimap_UpdatePins(
      Minimap __instance,
      ref List<Minimap.PinData> ___m_pins)
    {
      if (Mod.filteredPins.Count() == 0)
        return;

      foreach (Minimap.PinData p in ___m_pins)
      {
        if (Mod.filteredPins.Contains(p.m_type.ToString()))
        {
          Mod.FilterPins();
        }
      }
    }

    [HarmonyPatch(typeof(Minimap), "OnMapRightClick")]
    [HarmonyPrefix]
    private static bool Minimap_OMRC(
        Minimap __instance,
        ref List<Minimap.PinData> ___m_pins)
    {
      ZLog.Log("[AMP] Right click");

      Vector3 worldPoint = ScreenToWorldPoint(__instance, Input.mousePosition);

      //Mod.Log.LogInfo(string.Format("WorldPoint = {0}", worldPoint));
      Minimap.PinData closestPin = Mod.GetNearestPin(worldPoint, 5, ___m_pins);

      //Mod.Log.LogInfo(string.Format("Pin Name - {0}, Pin Icon - {1}, Pin Type {2}", closestPin.m_name, closestPin.m_icon.name, closestPin.m_type));

      if (closestPin == null || closestPin.m_icon.name == "mapicon_start" || closestPin.m_type == Minimap.PinType.Death)
        return true;

      __instance.RemovePin(closestPin);
      Mod.addedPinLocs.Remove(closestPin.m_pos);
      Mod.autoPins.Remove(closestPin);
      Mod.remPinDict.Remove(closestPin.m_pos);
      return false;
    }

    [HarmonyPatch(typeof(Minimap), "OnMapLeftClick")]
    [HarmonyPrefix]
    private static bool Minimap_OMLC(
        Minimap __instance,
        ref List<Minimap.PinData> ___m_pins)
    {
      ZLog.Log("[AMP] Left click");
      Vector3 worldPoint = ScreenToWorldPoint(__instance, Input.mousePosition);

      Minimap.PinData closestPin = Mod.GetNearestPin(worldPoint, 5, ___m_pins);

      if (closestPin == null) return true;

      closestPin.m_checked = !closestPin.m_checked;
      return false;
    }

  }

  [HarmonyPatch(typeof(Destructible), "Start")]
  internal class DestructiblePatchSpawn
  {
    private static void Postfix(ref Destructible __instance)
    {
      if (!Mod.destructablesEnabled.Value) return;
      HoverText hoverTextComp = __instance.GetComponent<HoverText>();

      if (!hoverTextComp) return;

      string hoverText = hoverTextComp.m_text;
      hoverText = hoverText.Replace("(Clone)", "");

      PinType type = null;
      Mod.Log.LogDestructible(hoverText, hoverTextComp.transform.position);

      if (Mod.objectPins.ContainsKey(hoverText))
        type = Mod.mtypePins[Mod.objectPins[hoverText]];

      if (type != null && !Mod.pinItems.ContainsKey(hoverTextComp.transform.position))
        Mod.pinItems.Add(hoverTextComp.transform.position, type);
    }
  }

  [HarmonyPatch(typeof(Pickable), "Awake")]
  internal class PickablePatchSpawn
  {
    private static void Postfix(ref Pickable __instance)
    {
      if (!Mod.pickablesEnabled.Value) return;
      Pickable pickableComp = __instance.GetComponent<Pickable>();

      if (!pickableComp) return;

      string pickableText = pickableComp.name;
      pickableText = pickableText.Replace("(Clone)", "");
      PinType type = null;
      Mod.Log.LogPickable(pickableText, pickableComp.transform.position);

      if (Mod.objectPins.ContainsKey(pickableText))
        type = Mod.mtypePins[Mod.objectPins[pickableText]];

      if (type != null && !Mod.pinItems.ContainsKey(pickableComp.transform.position))
        Mod.pinItems.Add(pickableComp.transform.position, type);
    }
  }

  [HarmonyPatch(typeof(Location), "Awake")]
  internal class LocationPatchSpawn
  {
    private static void Postfix(ref Location __instance)
    {
      if (!Mod.locsEnabled.Value) return;
      Location locComp = __instance.GetComponent<Location>();

      if (!locComp) return;

      string locText = locComp.name;
      locText = locText.Replace("(Clone)", "");
      PinType type = null;
      Mod.Log.LogLocation(locText, locComp.transform.position);

      if (Mod.objectPins.ContainsKey(locText))
        type = Mod.mtypePins[Mod.objectPins[locText]];

      if (type != null && !Mod.pinItems.ContainsKey(locComp.transform.position))
        Mod.pinItems.Add(locComp.transform.position, type);
    }
  }

  [HarmonyPatch(typeof(SpawnArea), "Awake")]
  internal class SpawnAreaPatchSpawn
  {
    private static void Postfix(ref SpawnArea __instance)
    {
      if (!Mod.spwnsEnabled.Value) return;
      HoverText spawnComp = __instance.GetComponent<HoverText>();

      if (!spawnComp) return;

      string spawnText = spawnComp.m_text;
      spawnText = spawnText.Replace("(Clone)", "");
      PinType type = null;
      Mod.Log.LogSpawn(spawnText, spawnComp.transform.position);

      if (Mod.objectPins.ContainsKey(spawnText))
        type = Mod.mtypePins[Mod.objectPins[spawnText]];

      if (type != null && !Mod.pinItems.ContainsKey(spawnComp.transform.position))
        Mod.pinItems.Add(spawnComp.transform.position, type);
    }
  }

  [HarmonyPatch(typeof(Character), "Awake")]
  internal class CharacterPatchSpawn
  {
    private static void Postfix(ref CreatureSpawner __instance)
    {
      if (!Mod.creaturesEnabled.Value) return;
      Character creatureComp = __instance.GetComponent<Character>();

      if (!creatureComp) return;

      string creatureText = creatureComp.m_name;
      creatureText = creatureText.Replace("(Clone)", "");
      PinType type = null;
      Mod.Log.LogCreature(creatureText, creatureComp.transform.position);

      if (Mod.objectPins.ContainsKey(creatureText))
        type = Mod.mtypePins[Mod.objectPins[creatureText]];

      if (type != null && !Mod.pinItems.ContainsKey(creatureComp.transform.position))
        Mod.pinItems.Add(creatureComp.transform.position, type);
    }
  }

  [HarmonyPatch(typeof(MineRock), "Start")]
  internal class MineRockPatchSpawn
  {
    private static void Postfix(ref MineRock __instance)
    {
      if (!Mod.destructablesEnabled.Value) return;
      MineRock mineComp = __instance.GetComponent<MineRock>();

      if (!mineComp) return;

      string mineText = mineComp.name;
      PinType type = null;
      Mod.Log.LogDestructible(mineText, mineComp.transform.position);

      if (Mod.objectPins.ContainsKey(mineText))
        type = Mod.mtypePins[Mod.objectPins[mineText]];

      if (type != null && !Mod.pinItems.ContainsKey(mineComp.transform.position))
        Mod.pinItems.Add(mineComp.transform.position, type);
    }
  }

  [HarmonyPatch(typeof(Leviathan), "Awake")]
  internal class LeviathanPatchSpawn
  {
    private static void Postfix(ref Leviathan __instance)
    {
      if (!Mod.creaturesEnabled.Value) return;
      Leviathan levComp = __instance.GetComponent<Leviathan>();

      if (!levComp) return;

      string levText = levComp.name;
      levText = levText.Replace("(Clone)", "");
      PinType type = null;
      Mod.Log.LogCreature(levText, levComp.transform.position);

      if (Mod.objectPins.ContainsKey(levText))
        type = Mod.mtypePins[Mod.objectPins[levText]];

      if (type != null && !Mod.pinItems.ContainsKey(levComp.transform.position))
        Mod.pinItems.Add(levComp.transform.position, type);
    }
  }
  internal class Player_Patches
  {
    public static Vector3 currPos;
    public static Vector3 prevPos;
    private const int interval = 30;

    [HarmonyPatch(typeof(Player), "Awake")]
    internal class PlayerAwakePatch
    {
      private static void Postfix(ref Player __instance)
      {
        if (!Player.m_localPlayer)
          return;

        currPos = __instance.transform.position;
        prevPos = __instance.transform.position;

      }
    }

    [HarmonyPatch(typeof(Player), "Update")]
    internal class PlayerUpdatePatch
    {
      private static void Postfix(ref Player __instance)
      {
        if (!Player.m_localPlayer)
          return;

        Mod.currEnv = EnvMan.instance.GetCurrentEnvironment().m_name;

        if (Time.frameCount % interval == 0)
        {
          currPos = __instance.transform.position;

          if (Vector3.Distance(currPos, prevPos) > 5)
          {
            Mod.hasMoved = true;
            prevPos = currPos;
          }
          else
          {
            Mod.hasMoved = false;
          }
        }

        if (Mod.hasMoved)
        {
          Mod.checkPins(currPos);
        }
      }
    }
  }

  internal static class AMPCommandPatcher
  {
    private static Harmony harmony;
    private static bool initComplete;

    public static Harmony Harmony
    {
      get => harmony;
      set => harmony = value;
    }

    public static bool InitComplete
    {
      get => initComplete;
      set => initComplete = value;
    }

    public static void InitPatch()
    {
      if (InitComplete)
        return;
      try
      {
        harmony = Harmony.CreateAndPatchAll(typeof(AMPCommandPatcher).Assembly, null);
      }
      catch (Exception ex)
      {
        AMP_Commands.PrintOut("Something failed, there is a strong possibility another mod blocked this operation.");
      }
      finally
      {
        InitComplete = true;
      }
    }
  }
}
