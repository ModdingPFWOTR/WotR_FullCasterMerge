using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace FullCasterMerge
{
    static class SpellbookMerger
    {
        public static IEnumerable<ClassData> MythicClasses(UnitEntityData unit)
        {
            if (unit?.Descriptor?.Spellbooks == null || unit.Descriptor.Progression?.Classes == null)
            {
                yield break;
            }

            HashSet<Kingmaker.Blueprints.Classes.Spells.BlueprintSpellbook> standalone = new HashSet<Kingmaker.Blueprints.Classes.Spells.BlueprintSpellbook>();
            foreach (Spellbook book in unit.Descriptor.Spellbooks)
            {
                if (book != null && book.IsStandaloneMythic && book.Blueprint != null)
                {
                    standalone.Add(book.Blueprint);
                }
            }

            foreach (ClassData data in unit.Descriptor.Progression.Classes)
            {
                if (data?.Spellbook != null && standalone.Contains(data.Spellbook))
                {
                    yield return data;
                }
            }
        }

        public static IEnumerable<Spellbook> ClassBooks(UnitEntityData unit)
        {
            if (unit?.Descriptor?.Spellbooks == null)
            {
                yield break;
            }

            foreach (Spellbook book in unit.Descriptor.Spellbooks)
            {
                if (book?.Blueprint == null || book.Blueprint.IsMythic || book.IsStandaloneMythic)
                {
                    continue;
                }

                yield return book;
            }
        }

        public static string Merge(Spellbook target, ClassData mythicClass)
        {
            if (target?.Owner == null || target.Blueprint == null)
            {
                return "Livro de destino inválido.";
            }

            if (target.IsStandaloneMythic || target.Blueprint.IsMythic)
            {
                return "Escolha o livro da classe.";
            }

            Kingmaker.Blueprints.Classes.Spells.BlueprintSpellbook mythicBook = mythicClass?.Spellbook;
            if (mythicBook == null || !mythicBook.IsMythic)
            {
                return "Esse livro não é um spellbook mítico.";
            }

            if (mythicBook.MythicSpellList == null)
            {
                return "O spellbook mítico não tem lista de magias.";
            }

            if (target.SpecialLists != null && target.SpecialLists.Contains(mythicBook.MythicSpellList))
            {
                return "Esse livro já recebeu essa lista mítica.";
            }

            try
            {
                mythicClass.Spellbook = target.Blueprint;
                FullCasterProgress.MarkMerged(target);
                target.AddSpecialList(mythicBook.MythicSpellList);

                int mythicLevel = target.Owner.Progression.MythicLevel;
                for (int level = target.MythicLevel; level < mythicLevel; level++)
                {
                    target.AddMythicLevel();
                }

                target.Owner.DeleteSpellbook(mythicBook);
                FullCasterProgress.RestoreMythicSpells(target);
                FullCasterProgress.RefreshSlots(target);
                SuppressDuplicateMythicBooks(target.Owner);
                Main.Log("Merged " + mythicBook.name + " into " + target.Blueprint.name + ". Max circle " + target.MaxSpellLevel + " at caster level " + target.CasterLevel + ".");
                return null;
            }
            catch (Exception ex)
            {
                Main.Log("Merge failed: " + ex);
                return ex.Message;
            }
        }

        public static void SuppressDuplicateMythicBooksInParty()
        {
            try
            {
                var characters = Kingmaker.Game.Instance?.Player?.AllCharacters;
                if (characters == null)
                {
                    return;
                }

                foreach (UnitEntityData unit in characters)
                {
                    SuppressDuplicateMythicBooks(unit?.Descriptor);
                }
            }
            catch (Exception ex)
            {
                Main.Log("Duplicate mythic book cleanup failed: " + ex.Message);
            }
        }

        public static Spellbook MergedBookFor(UnitDescriptor owner, BlueprintSpellbook mythicBlueprint)
        {
            if (owner == null || mythicBlueprint?.MythicSpellList == null)
            {
                return null;
            }

            foreach (Spellbook book in owner.Spellbooks)
            {
                if (book?.Blueprint == null || book.Blueprint == mythicBlueprint || !FullCasterProgress.IsMerged(book))
                {
                    continue;
                }

                if (book.SpecialLists != null && book.SpecialLists.Contains(mythicBlueprint.MythicSpellList))
                {
                    return book;
                }
            }

            return null;
        }

        public static void SuppressDuplicateMythicBooks(UnitDescriptor owner)
        {
            if (!Main.Enabled || owner?.Spellbooks == null || owner.Progression?.Classes == null)
            {
                return;
            }

            List<Spellbook> books = new List<Spellbook>();
            foreach (Spellbook book in owner.Spellbooks)
            {
                if (book != null)
                {
                    books.Add(book);
                }
            }

            foreach (Spellbook standalone in books)
            {
                if (!standalone.IsStandaloneMythic || standalone.Blueprint == null)
                {
                    continue;
                }

                Spellbook merged = MergedBookFor(owner, standalone.Blueprint);
                if (merged == null)
                {
                    continue;
                }

                Retarget(owner, standalone.Blueprint, merged.Blueprint);
                Absorb(standalone, merged);
                owner.DeleteSpellbook(standalone.Blueprint);
                FullCasterProgress.RefreshSlots(merged);
                Main.Log("Removed duplicate " + standalone.Blueprint.name + " already merged into " + merged.Blueprint.name + ".");
            }
        }

        static void Retarget(UnitDescriptor owner, BlueprintSpellbook from, BlueprintSpellbook to)
        {
            foreach (ClassData data in owner.Progression.Classes)
            {
                if (data != null && data.Spellbook == from)
                {
                    data.Spellbook = to;
                }
            }
        }

        static void Absorb(Spellbook from, Spellbook into)
        {
            try
            {
                foreach (var known in from.GetAllKnownSpells())
                {
                    BlueprintAbility spell = known?.Blueprint;
                    if (spell == null || into.IsKnown(spell))
                    {
                        continue;
                    }

                    into.AddKnown(known.SpellLevel, spell, false);
                }
            }
            catch (Exception ex)
            {
                Main.Log("Copying spells from duplicate book failed: " + ex.Message);
            }
        }
    }
}
