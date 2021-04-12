using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AMP_Configurable.Commands;

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
            return ((type != Minimap.PinType.Death ? 0 : (Mod.SimilarPinExists(pos, type, ___m_pins, name, PinnedObject.aIcon, out Minimap.PinData _) ? 1 : 0)) & (save ? 1 : 0)) == 0;
        }

        [HarmonyPatch(typeof(Minimap), "UpdateProfilePins")]
        [HarmonyPrefix]
        private static void Minimap_UpdateProfilePins(
          ref List<Minimap.PinData> ___m_pins)
        {
            
            //while (count < ___m_pins.Count())
            if(!checkedSavedPins)
            {
                //Debug.Log("[AMP] Checking Saved Pins");
                //Debug.Log(string.Format("[AMP] m_pins Count {0}", ___m_pins.Count()));
                foreach (Minimap.PinData pins in ___m_pins)
                {
                    if ((int)pins.m_type >= 100)
                    {
                        if(!Mod.savedPins.Contains(pins))
                            Mod.savedPins.Add(pins);

                        //Debug.Log(string.Format("[AMP] Pin {0} has type of {1}", pins.m_name, pins.m_type));
                        PinnedObject.loadData(pins.m_type.ToString());

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
                    //count++;
                }
                checkedSavedPins = true;
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

            foreach(Minimap.PinData p in ___m_pins)
            {
                if(Mod.filteredPins.Contains(p.m_type.ToString()))
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

            if (closestPin == null)
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

            //Mod.Log.LogInfo(string.Format("WorldPoint = {0}", worldPoint));
            Minimap.PinData closestPin = Mod.GetNearestPin(worldPoint, 5, ___m_pins);

            if (closestPin == null)
            {
                //Mod.Log.LogInfo("[AMP] Closest pin is null");
                return true;
            }

            closestPin.m_checked = !closestPin.m_checked;
            return false;
        }

    }

    [HarmonyPatch(typeof(Destructible), "Start")]
    internal class DestructiblePatchSpawn
    {
        private static void Postfix(ref Destructible __instance)
        {
            HoverText hoverTextComp = __instance.GetComponent<HoverText>();

            if (!hoverTextComp)
            {
                return;
            }

            
            string hoverText = hoverTextComp.m_text;
            string aName = "";

            //Mod.Log.LogInfo(string.Format("[AMP - Dest] Found {0} at {1} {2} {3}", hoverText, hoverTextComp.transform.position.x, hoverTextComp.transform.position.y, hoverTextComp.transform.position.z));
            switch (hoverText)
            {
                case "$piece_deposit_tin":
                    if (Mod.pinTin.Value)
                    {
                        aName = "Tin";
                    }
                    break;
                case "$piece_deposit_copper":
                    if (Mod.pinCopper.Value)
                    {
                        aName = "Copper";
                    }
                    break;
                case "$piece_deposit_obsidian":
                    if (Mod.pinObsidian.Value)
                    {
                        aName = "Obsidian";
                    }
                    break;
                case "$piece_deposit_silver":
                case "$piece_deposit_silvervein":
                    if (Mod.pinSilver.Value)
                    {
                        aName = "Silver";
                    }
                    break;
                case "$piece_mudpile":
                    if (Mod.pinIron.Value)
                    {
                        aName = "Iron";
                    }
                    break;
                default:
                    aName = "";
                    break;
            }

            if (aName != "")
            {
                //__instance.gameObject.AddComponent<PinnedObject>().Init(aName, hoverTextComp.transform.position);

                if (!Mod.pinItems.ContainsKey(hoverTextComp.transform.position))
                {
                    Mod.pinItems.Add(hoverTextComp.transform.position, aName);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pickable), "Awake")]
    internal class PickablePatchSpawn
    {
        private static void Postfix(ref Pickable __instance)
        {
            Pickable pickableComp = __instance.GetComponent<Pickable>();

            if (!pickableComp)
            {
                return;
            }

            string pickableText = pickableComp.name;
            string aName = "";

            //Mod.Log.LogInfo(string.Format("Found {0} at {1} {2} {3}", pickableText, pickableComp.transform.position.x, pickableComp.transform.position.y, pickableComp.transform.position.z));

            switch (pickableText)
            {
                case "RaspberryBush":
                case "RaspberryBush(Clone)":
                    if (Mod.pinBerries.Value)
                    {
                        aName = "Berries";
                    }
                    break;
                case "BlueberryBush":
                case "BlueberryBush(Clone)":
                    if (Mod.pinBlueberries.Value)
                    {
                        aName = "Blueberries";
                    }
                    break;
                case "CloudberryBush":
                case "CloudberryBush(Clone)":
                    if (Mod.pinCloudberries.Value)
                    {
                        aName = "Cloudberries";
                    }
                    break;
                case "Pickable_Thistle":
                case "Pickable_Thistle(Clone)":
                    if (Mod.pinThistle.Value)
                    {
                        aName = "Thistle";
                    }
                    break;
                case "Pickable_DragonEgg":
                case "Pickable_DragonEgg(Clone)":
                    if (Mod.pinDragonEgg.Value)
                    {
                        aName = "DragonEgg";
                    }
                    break;
                case "Pickable_Mushroom":
                case "Pickable_Mushroom(Clone)":
                    if (Mod.pinMushroom.Value)
                    {
                        aName = "Mushroom";
                    }
                    break;
                case "Pickable_SeedCarrot":
                case "Pickable_SeedCarrot(Clone)":
                    if (Mod.pinCarrot.Value)
                    {
                        aName = "Carrot";
                    }
                    break;
                case "Pickable_SeedTurnip":
                case "Pickable_SeedTurnip(Clone)":
                    if (Mod.pinTurnip.Value)
                    {
                        aName = "Turnip";
                    }
                    break;
                default:
                    aName = "";
                    break;
            }

            if (aName != "")
            {
                //__instance.gameObject.AddComponent<PinnedObject>().Init(aName, pickableComp.transform.position);

                if (!Mod.pinItems.ContainsKey(pickableComp.transform.position))
                {
                    Mod.pinItems.Add(pickableComp.transform.position, aName);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Location), "Awake")]
    internal class LocationPatchSpawn
    {
        private static void Postfix(ref Location __instance)
        {
            Location locComp = __instance.GetComponent<Location>();
            if (!locComp)
            {
                return;
            }

            string locText = locComp.name;
            string aName = "";

            //Mod.Log.LogInfo(string.Format("[AMP - Loc] Found {0} at {1} {2} {3}", locText, locComp.transform.position.x, locComp.transform.position.y, locComp.transform.position.z));
            switch (locText)
            {
                case "Crypt1":
                case "Crypt2":
                case "Crypt3":
                case "Crypt4":
                case "Crypt1(Clone)":
                case "Crypt2(Clone)":
                case "Crypt3(Clone)":
                case "Crypt4(Clone)":
                    if (Mod.pinCrypt.Value)
                    {
                        aName = "Crypt";
                    }
                    break;
                case "SunkenCrypt1":
                case "SunkenCrypt2":
                case "SunkenCrypt3":
                case "SunkenCrypt4":
                case "SunkenCrypt1(Clone)":
                case "SunkenCrypt2(Clone)":
                case "SunkenCrypt3(Clone)":
                case "SunkenCrypt4(Clone)":
                    if (Mod.pinSunkenCrypt.Value)
                    {
                        aName = "SunkenCrypt";
                    }
                    break;
                case "TrollCave01":
                case "TrollCave02":
                case "TrollCave01(Clone)":
                case "TrollCave02(Clone)":
                    if (Mod.pinTrollCave.Value)
                    {
                        aName = "TrollCave";
                    }
                    break;
                case "FireHole":
                case "FireHole(Clone)":
                    if (Mod.pinSurtling.Value)
                    {
                        aName = "Surtling";
                    }
                    break;
                default:
                    aName = "";
                    break;
            }

            if (aName != "")
            {
                //__instance.gameObject.AddComponent<PinnedObject>().Init(aName, locComp.transform.position);

                if (!Mod.pinItems.ContainsKey(locComp.transform.position))
                {
                    Mod.pinItems.Add(locComp.transform.position, aName);
                }
            }
        }
    }

    [HarmonyPatch(typeof(SpawnArea), "Awake")]
    internal class SpawnAreaPatchSpawn
    {
        private static void Postfix(ref SpawnArea __instance)
        {
            HoverText spawnComp = __instance.GetComponent<HoverText>();

            if (!spawnComp)
            {
                return;
            }

            string spawnText = spawnComp.m_text;
            string aName = "";

            //Mod.Log.LogInfo(string.Format("[AMP - Spawn] Found {0} at {1} {2} {3}", spawnText, spawnComp.transform.position.x, spawnComp.transform.position.y, spawnComp.transform.position.z));
            switch (spawnText)
            {
                case "Evil bone pile":
                    if (Mod.pinSkeleton.Value)
                    {
                        aName = "Skeleton";
                    }
                    break;
                case "Body Pile":
                case "Body pile":
                    if (Mod.pinDraugr.Value)
                    {
                        aName = "Draugr";
                    }
                    break;
                case "Greydwarf nest":
                    if (Mod.pinGreydwarf.Value)
                    {
                        aName = "Greydwarf";
                    }
                    break;
                default:
                    aName = "";
                    break;
            }

            if (aName != "")
            {
                //__instance.gameObject.AddComponent<PinnedObject>().Init(aName, spawnComp.transform.position);

                if (!Mod.pinItems.ContainsKey(spawnComp.transform.position))
                {
                    Mod.pinItems.Add(spawnComp.transform.position, aName);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Character), "Awake")]
    internal class CharacterPatchSpawn
    {
        private static void Postfix(ref CreatureSpawner __instance)
        {
            Character creatureComp = __instance.GetComponent<Character>();

            if (!creatureComp)
            {
                return;
            }

            string creatureText = creatureComp.m_name;
            string aName = "";

            //Mod.Log.LogInfo(string.Format("[AMP - Characters] Found {0} at {1} {2} {3}", creatureText, creatureComp.transform.position.x, creatureComp.transform.position.y, creatureComp.transform.position.z));
            switch (creatureText)
            {
                case "$enemy_serpent":
                    if (Mod.pinSerpent.Value)
                    {
                        aName = "Serpent";
                    }
                    break;
                default:
                    aName = "";
                    break;
            }

            if (aName != "")
            {
                //__instance.gameObject.AddComponent<PinnedObject>().Init(aName, spawnComp.transform.position);

                if (!Mod.pinItems.ContainsKey(creatureComp.transform.position))
                {
                    Mod.pinItems.Add(creatureComp.transform.position, aName);
                }
            }
        }
    }

    [HarmonyPatch(typeof(MineRock), "Start")]
    internal class MineRockPatchSpawn
    {
        private static void Postfix(ref MineRock __instance)
        {
            MineRock mineComp = __instance.GetComponent<MineRock>();

            if (!mineComp)
            {
                return;
            }

            string mineText = mineComp.name;
            string aName = "";

            //Mod.Log.LogInfo(string.Format("[AMP - MineRock] Found {0} at {1} {2} {3}", mineText, mineComp.transform.position.x, mineComp.transform.position.y, mineComp.transform.position.z));
            switch (mineText)
            {
                case "MineRock_Meteorite":
                case "MineRock_Meteorite(Clone)":
                    if (Mod.pinFlametal.Value)
                    {
                        aName = "Flametal";
                    }
                    break;
                default:
                    aName = "";
                    break;
            }

            if (aName != "")
            {
                //__instance.gameObject.AddComponent<PinnedObject>().Init(aName, spawnComp.transform.position);

                if (!Mod.pinItems.ContainsKey(mineComp.transform.position))
                {
                    Mod.pinItems.Add(mineComp.transform.position, aName);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Leviathan), "Awake")]
    internal class LeviathanPatchSpawn
    {
        private static void Postfix(ref Leviathan __instance)
        {
            Leviathan levComp = __instance.GetComponent<Leviathan>();

            if (!levComp)
            {
                return;
            }

            string levText = levComp.name;
            string aName = "";

            //Mod.Log.LogInfo(string.Format("[AMP - Leviathan] Found {0} at {1} {2} {3}", levText, levComp.transform.position.x, levComp.transform.position.y, levComp.transform.position.z));
            switch (levText)
            {
                case "Leviathan":
                case "Leviathan(Clone)":
                    if (Mod.pinLeviathan.Value)
                    {
                        aName = "Leviathan";
                    }
                    break;
                default:
                    aName = "";
                    break;
            }

            if (aName != "")
            {
                //__instance.gameObject.AddComponent<PinnedObject>().Init(aName, spawnComp.transform.position);

                if (!Mod.pinItems.ContainsKey(levComp.transform.position))
                {
                    Mod.pinItems.Add(levComp.transform.position, aName);
                }
            }
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
