using System;
using HarmonyLib;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic;

namespace FullCasterMerge.Patches
{
    [HarmonyPatch(typeof(Spellbook), "GetMaxSpellLevel")]
    static class SpellbookMaxLevelPatch
    {
        static bool Prefix(Spellbook __instance, ref int __result)
        {
            try
            {
                if (!FullCasterProgress.IsMergedPartial(__instance))
                {
                    return true;
                }

                __result = FullCasterProgress.HighestCircle(__instance);
                return false;
            }
            catch (Exception ex)
            {
                Main.Log("GetMaxSpellLevel patch failed: " + ex.Message);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Spellbook), nameof(Spellbook.GetLastSpellbookLevel))]
    static class SpellbookLastLevelPatch
    {
        static void Postfix(Spellbook __instance, ref int __result)
        {
            try
            {
                if (!FullCasterProgress.IsMergedPartial(__instance))
                {
                    return;
                }

                int max = __instance.MaxSpellLevel;
                if (max > __result)
                {
                    __result = max;
                }
            }
            catch (Exception ex)
            {
                Main.Log("GetLastSpellbookLevel patch failed: " + ex.Message);
            }
        }
    }

    [HarmonyPatch(typeof(Spellbook), nameof(Spellbook.GetSpellsPerDay))]
    static class SpellbookSlotsPatch
    {
        static bool Prefix(Spellbook __instance, int spellLevel, ref int __result)
        {
            try
            {
                if (!FullCasterProgress.IsMergedPartial(__instance) || spellLevel <= 0 || spellLevel > 10)
                {
                    return true;
                }

                __result = FullCasterProgress.SlotsPerDay(__instance, spellLevel);
                return false;
            }
            catch (Exception ex)
            {
                Main.Log("GetSpellsPerDay patch failed: " + ex.Message);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Spellbook), nameof(Spellbook.PostLoad))]
    static class SpellbookPostLoadPatch
    {
        static void Postfix(Spellbook __instance)
        {
            FullCasterProgress.RefreshSlots(__instance);
        }
    }

    // ToyBox deletes the mythic book but leaves the mythic class pointing at it.
    // The next load then creates that book again, beside the one that was merged.
    [HarmonyPatch(typeof(UnitDescriptor), nameof(UnitDescriptor.DemandSpellbook), typeof(BlueprintSpellbook))]
    static class DemandMergedSpellbookPatch
    {
        static bool Prefix(UnitDescriptor __instance, BlueprintSpellbook blueprint, ref Spellbook __result)
        {
            try
            {
                if (!Main.Enabled || blueprint == null || !blueprint.IsMythic)
                {
                    return true;
                }

                SpellbookMerger.SuppressDuplicateMythicBooks(__instance);
                Spellbook merged = SpellbookMerger.MergedBookFor(__instance, blueprint);
                if (merged == null)
                {
                    return true;
                }

                __result = merged;
                return false;
            }
            catch (Exception ex)
            {
                Main.Log("DemandSpellbook patch failed: " + ex.Message);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(UnitDescriptor), nameof(UnitDescriptor.ApplyPostLoadFixes))]
    static class DuplicateBookPostLoadPatch
    {
        static void Postfix(UnitDescriptor __instance)
        {
            SpellbookMerger.SuppressDuplicateMythicBooks(__instance);
        }
    }

    [HarmonyPatch(typeof(UnitDescriptor), nameof(UnitDescriptor.TurnOn))]
    static class DuplicateBookTurnOnPatch
    {
        static void Postfix(UnitDescriptor __instance)
        {
            SpellbookMerger.SuppressDuplicateMythicBooks(__instance);
        }
    }
}
