using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using UnityEngine;

namespace FullCasterMerge.UI
{
    static class UmmMenu
    {
        static Vector2 _scroll;
        static string _status = string.Empty;

        public static void Draw()
        {
            GUILayout.Label("Funde o spellbook mítico no livro da classe. O livro fundido passa a usar a progressão de um conjurador completo, até o 9º círculo. A lista da classe não muda; entram as magias do livro mítico.");
            GUILayout.Label("O 10º círculo continua dependendo da feature do jogo que libera magia de nível 10. Salve antes de fundir. O merge fica gravado no save.");
            if (!Main.Enabled)
            {
                GUILayout.Label("Mod desligado. A progressão de full caster fica pausada.");
            }

            GUILayout.Space(6);
            var characters = Characters();
            if (characters == null)
            {
                GUILayout.Label("Carregue um save para fundir.");
                return;
            }

            if (!string.IsNullOrEmpty(_status))
            {
                GUILayout.Label(_status);
            }

            _scroll = GUILayout.BeginScrollView(_scroll);
            foreach (UnitEntityData unit in characters)
            {
                DrawUnit(unit);
            }

            GUILayout.EndScrollView();
        }

        static void DrawUnit(UnitEntityData unit)
        {
            if (unit?.Descriptor?.Spellbooks == null)
            {
                return;
            }

            List<Spellbook> books = new List<Spellbook>();
            foreach (Spellbook book in unit.Descriptor.Spellbooks)
            {
                if (book != null)
                {
                    books.Add(book);
                }
            }

            if (books.Count == 0)
            {
                return;
            }

            GUILayout.Space(8);
            GUILayout.Label(NameOf(unit), GUI.skin.box);

            List<ClassData> mythicClasses = new List<ClassData>(SpellbookMerger.MythicClasses(unit));
            foreach (Spellbook book in SpellbookMerger.ClassBooks(unit))
            {
                string kind = book.Blueprint.Spontaneous ? "spontaneous" : "prepared";
                bool merged = FullCasterProgress.IsMerged(book);
                string state = merged
                    ? "fundido, círculo " + book.MaxSpellLevel + ", CL " + book.CasterLevel
                    : "círculo da classe " + FullCasterProgress.NativeMax(book.Blueprint) + ", CL " + book.CasterLevel;
                GUILayout.Label(book.Blueprint.name + " (" + kind + ", " + state + ")");

                if (merged)
                {
                    if (GUILayout.Button("Atualizar magias míticas de " + book.Blueprint.name, GUILayout.Width(360)))
                    {
                        FullCasterProgress.RestoreMythicSpells(book);
                        FullCasterProgress.RefreshSlots(book);
                        _status = book.Blueprint.name + " atualizado. Círculo " + book.MaxSpellLevel + ".";
                    }

                    continue;
                }

                foreach (ClassData mythic in mythicClasses)
                {
                    string mythicName = mythic.Spellbook != null ? mythic.Spellbook.name : "Mythic";
                    if (GUILayout.Button("Fundir " + mythicName + " em " + book.Blueprint.name, GUILayout.Width(360)))
                    {
                        string error = SpellbookMerger.Merge(book, mythic);
                        _status = error ?? ("Fundiu " + mythicName + " em " + book.Blueprint.name + ". Círculo " + book.MaxSpellLevel + ", CL " + book.CasterLevel + ".");
                    }
                }
            }

            bool hasClassBook = false;
            foreach (Spellbook book in SpellbookMerger.ClassBooks(unit))
            {
                hasClassBook = true;
                break;
            }

            if (mythicClasses.Count == 0 && hasClassBook)
            {
                GUILayout.Label("Nenhum spellbook mítico separado. Se já fundiu, o livro da classe acima é o fundido.");
            }
            else if (mythicClasses.Count > 0 && !hasClassBook)
            {
                GUILayout.Label("Esse personagem não tem spellbook de classe para receber o merge.");
            }
        }

        static List<UnitEntityData> Characters()
        {
            try
            {
                var characters = Kingmaker.Game.Instance?.Player?.AllCharacters;
                if (characters == null)
                {
                    return null;
                }

                List<UnitEntityData> list = new List<UnitEntityData>();
                foreach (UnitEntityData unit in characters)
                {
                    if (unit != null)
                    {
                        list.Add(unit);
                    }
                }

                return list;
            }
            catch (System.Exception ex)
            {
                Main.Log("Character list failed: " + ex.Message);
                return null;
            }
        }

        static string NameOf(UnitEntityData unit)
        {
            try
            {
                string name = unit.CharacterName;
                if (!string.IsNullOrEmpty(name))
                {
                    return name;
                }
            }
            catch (System.Exception)
            {
            }

            return unit.Blueprint != null ? unit.Blueprint.name : "Character";
        }
    }
}
