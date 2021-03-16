using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AutoPins.Patches
{
    internal class Minimap_Patch
    {
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
            return ((type != Minimap.PinType.Death ? 0 : (Mod.SimilarPinExists(pos, type, ___m_pins, name, AutoPins.PinnedObject.aIcon, out Minimap.PinData _) ? 1 : 0)) & (save ? 1 : 0)) == 0;
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
                default:
                    aName = "";
                    break;
            }

            (__instance.gameObject.AddComponent<PinnedObject>()).Init(aName);
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
                    if (Mod.pinBlueberries.Value)
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
                default:
                    aName = "";
                    break;
            }

            (__instance.gameObject.AddComponent<PinnedObject>()).Init(aName);
        }
    }
}
