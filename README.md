# Full Caster Merge

Unity Mod Manager mod for Pathfinder: Wrath of the Righteous.

It merges a mythic path spellbook into any class spellbook. The merged book stops using the class spell ceiling and follows full-caster progression instead.

## What it changes

- With a save loaded, the UMM menu lists each character's class book and any separate mythic book. A button merges the two.
- Caster level stays the class caster level plus mythic rank, the same way the game and ToyBox count a merge.
- If the class book stopped before 9th circle, circles 1 through 9 follow a full-caster table. Spontaneous books use the sorcerer/oracle table. Prepared books use the wizard/cleric table.
- The class spell list is left alone. An Inquisitor does not learn the Cleric list. Spells that fill the new circles come from the mythic book.
- A book that was already a full caster, such as Cleric or Wizard, keeps its own table.
- 10th circle opens only if the character has the game feature that unlocks 10th-level spells. Without it, the ceiling is 9th.
- A merge already done in ToyBox picks up this progression when the save loads. You do not need to merge again.

## Install

1. Install Unity Mod Manager and apply it to Wrath of the Righteous.
2. Build with `dotnet build`, or copy `FullCasterMerge.dll` and `Info.json` into `<game>\Mods\FullCasterMerge`.
3. Start the game, confirm the mod in UMM, and load a save.
4. Save before merging. The merge is stored on the character, and this menu does not undo it.

A successful build copies the mod into the game's Mods folder.

## Limits

Turning the mod off does not undo the merge. Extra circles disappear while the mod is off, because the class table is still the original one, and they come back when the mod is turned on again.

New circles on a spontaneous book start full when that circle's pool is zero. Resting in game also refills the pool. A prepared book gains empty slots to memorize.
