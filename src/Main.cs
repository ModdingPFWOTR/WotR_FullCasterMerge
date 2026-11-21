using System;
using HarmonyLib;
using UnityModManagerNet;

namespace FullCasterMerge
{
    public static class Main
    {
        public static UnityModManager.ModEntry ModEntry;
        public static Harmony HarmonyInstance;
        public static bool Enabled = true;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            ModEntry = modEntry;
            HarmonyInstance = new Harmony(modEntry.Info.Id);
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;

            try
            {
                HarmonyInstance.PatchAll(typeof(Main).Assembly);
                Log("Full Caster Merge loaded.");
            }
            catch (Exception ex)
            {
                Log("Harmony patch failed: " + ex);
                return false;
            }

            return true;
        }

        public static void Log(string message)
        {
            ModEntry?.Logger.Log(message);
        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Enabled = value;
            if (value)
            {
                SpellbookMerger.SuppressDuplicateMythicBooksInParty();
            }

            Log(value ? "Mod enabled." : "Mod disabled. Merged books keep the merge, but slot progression falls back to the class table until you turn this on again.");
            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            UI.UmmMenu.Draw();
        }
    }
}
