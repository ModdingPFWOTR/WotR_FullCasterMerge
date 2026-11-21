using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;

namespace FullCasterMerge
{
    static class FullCasterProgress
    {
        // Spell levels 1-9. Prepared matches the wizard/cleric chart. Spontaneous matches sorcerer/oracle.
        static readonly int[][] Prepared =
        {
            Array.Empty<int>(),
            new[] { 1 },
            new[] { 2 },
            new[] { 2, 1 },
            new[] { 3, 2 },
            new[] { 3, 2, 1 },
            new[] { 3, 3, 2 },
            new[] { 4, 3, 2, 1 },
            new[] { 4, 3, 3, 2 },
            new[] { 4, 4, 3, 2, 1 },
            new[] { 4, 4, 3, 3, 2 },
            new[] { 4, 4, 4, 3, 2, 1 },
            new[] { 4, 4, 4, 3, 3, 2 },
            new[] { 4, 4, 4, 4, 3, 2, 1 },
            new[] { 4, 4, 4, 4, 3, 3, 2 },
            new[] { 4, 4, 4, 4, 4, 3, 2, 1 },
            new[] { 4, 4, 4, 4, 4, 3, 3, 2 },
            new[] { 4, 4, 4, 4, 4, 4, 3, 2, 1 },
            new[] { 4, 4, 4, 4, 4, 4, 3, 3, 2 },
            new[] { 4, 4, 4, 4, 4, 4, 4, 3, 3 },
            new[] { 4, 4, 4, 4, 4, 4, 4, 4, 4 }
        };

        static readonly int[][] Spontaneous =
        {
            Array.Empty<int>(),
            new[] { 3 },
            new[] { 4 },
            new[] { 5 },
            new[] { 6, 3 },
            new[] { 6, 4 },
            new[] { 6, 5, 3 },
            new[] { 6, 6, 4 },
            new[] { 6, 6, 5, 3 },
            new[] { 6, 6, 6, 4 },
            new[] { 6, 6, 6, 5, 3 },
            new[] { 6, 6, 6, 6, 4 },
            new[] { 6, 6, 6, 6, 5, 3 },
            new[] { 6, 6, 6, 6, 6, 4 },
            new[] { 6, 6, 6, 6, 6, 5, 3 },
            new[] { 6, 6, 6, 6, 6, 6, 4 },
            new[] { 6, 6, 6, 6, 6, 6, 5, 3 },
            new[] { 6, 6, 6, 6, 6, 6, 6, 4 },
            new[] { 6, 6, 6, 6, 6, 6, 6, 5, 3 },
            new[] { 6, 6, 6, 6, 6, 6, 6, 6, 4 },
            new[] { 6, 6, 6, 6, 6, 6, 6, 6, 6 }
        };

        static readonly Dictionary<Kingmaker.Blueprints.Classes.Spells.BlueprintSpellbook, int> NativeMaxByBook = new Dictionary<Kingmaker.Blueprints.Classes.Spells.BlueprintSpellbook, int>();
        static readonly FieldInfo TypeField = AccessTools.Field(typeof(Spellbook), "m_Type");
        static readonly FieldInfo SpontaneousSlotsField = AccessTools.Field(typeof(Spellbook), "m_SpontaneousSlots");

        public static bool IsMerged(Spellbook book)
        {
            return book != null && book.IsMythic && !book.IsStandaloneMythic;
        }

        public static void MarkMerged(Spellbook book)
        {
            TypeField.SetValue(book, SpellbookType.Mythic);
        }

        public static bool IsMergedPartial(Spellbook book)
        {
            if (!Main.Enabled || book?.Blueprint == null || !IsMerged(book))
            {
                return false;
            }

            if (book.Blueprint.SpellsPerDay == null)
            {
                return false;
            }

            return NativeMax(book.Blueprint) < 9;
        }

        public static int NativeMax(Kingmaker.Blueprints.Classes.Spells.BlueprintSpellbook blueprint)
        {
            if (blueprint == null)
            {
                return 0;
            }

            if (NativeMaxByBook.TryGetValue(blueprint, out int cached))
            {
                return cached;
            }

            int max = 0;
            try
            {
                max = blueprint.MaxSpellLevel;
            }
            catch (Exception ex)
            {
                Main.Log("Native max spell level failed for " + blueprint.name + ": " + ex.Message);
            }

            NativeMaxByBook[blueprint] = max;
            return max;
        }

        public static int TableSlots(Spellbook book, int spellLevel)
        {
            if (spellLevel <= 0 || spellLevel > 9)
            {
                return 0;
            }

            int casterLevel = book.CasterLevel;
            if (casterLevel > 20)
            {
                casterLevel = 20;
            }

            if (casterLevel < 1)
            {
                return 0;
            }

            int[] row = book.Blueprint.Spontaneous ? Spontaneous[casterLevel] : Prepared[casterLevel];
            if (spellLevel > row.Length)
            {
                return 0;
            }

            return row[spellLevel - 1];
        }

        public static int HighestCircle(Spellbook book)
        {
            int highest = 0;
            for (int level = 9; level >= 1; level--)
            {
                if (TableSlots(book, level) > 0)
                {
                    highest = level;
                    break;
                }
            }

            if (highest >= 9 && HasTenthCircle(book) && book.CasterLevel >= 28)
            {
                return 10;
            }

            return highest;
        }

        public static int TenthCircleSlots(Spellbook book)
        {
            if (!HasTenthCircle(book))
            {
                return 0;
            }

            int casterLevel = book.CasterLevel;
            if (casterLevel >= 30)
            {
                return 4;
            }

            if (casterLevel >= 28)
            {
                return 2;
            }

            return 0;
        }

        public static bool HasTenthCircle(Spellbook book)
        {
            try
            {
                return book?.Owner?.State?.Features != null && (bool)book.Owner.State.Features.EnableSpellLevel10;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static int SlotsPerDay(Spellbook book, int spellLevel)
        {
            if (book?.Owner == null)
            {
                return 0;
            }

            int table = spellLevel == 10 ? TenthCircleSlots(book) : TableSlots(book, spellLevel);
            if (table <= 0)
            {
                return 0;
            }

            ModifiableValueAttributeStat stat = book.Owner.Stats.GetStat(book.Blueprint.CastingAttribute) as ModifiableValueAttributeStat;
            if (stat == null)
            {
                return 0;
            }

            int permanent = stat.CalculatePermanentValueWithoutTempBuffs();
            if (Math.Min(stat.ModifiedValue, permanent) < 10 + spellLevel)
            {
                return 0;
            }

            int total = table;
            int score = book.Owner.IsPlayerFaction ? permanent : stat.BaseValue;
            int abilityBonus = (score - 10) / 2 - spellLevel;
            if (abilityBonus >= 0)
            {
                total += abilityBonus / 4 + 1;
            }

            UnitPartExtraSpellsPerDay extra = book.Owner.Get<UnitPartExtraSpellsPerDay>();
            if (extra?.BonusSpells != null && spellLevel >= 0 && spellLevel < extra.BonusSpells.Length)
            {
                total += extra.BonusSpells[spellLevel];
            }

            return total;
        }

        public static void RestoreMythicSpells(Spellbook book)
        {
            if (book == null)
            {
                return;
            }

            try
            {
                AccessTools.Method(typeof(Spellbook), "TryRestoreKnownSpells")?.Invoke(book, null);
            }
            catch (Exception ex)
            {
                Main.Log("Restore mythic spells failed: " + ex.Message);
            }
        }

        public static void RefreshSlots(Spellbook book)
        {
            if (!IsMergedPartial(book))
            {
                return;
            }

            try
            {
                book.UpdateAllSlotsSize(false);
                int[] slots = SpontaneousSlotsField?.GetValue(book) as int[];
                if (!book.Blueprint.Spontaneous || slots == null)
                {
                    return;
                }

                int native = NativeMax(book.Blueprint);
                int max = book.MaxSpellLevel;
                for (int level = native + 1; level <= max && level < slots.Length; level++)
                {
                    if (slots[level] == 0)
                    {
                        slots[level] = book.GetSpellsPerDay(level);
                    }
                }
            }
            catch (Exception ex)
            {
                Main.Log("Refresh slots failed: " + ex.Message);
            }
        }
    }
}
