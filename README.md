# Extrawners
A mod that adds spawners that create atoms, or Extrawners, if you will.
(It also adds computation like random inputs and multi-outputs, of course)

## Extransmissions Puzzles
_Most_ extransmissions puzzles can be made into Extrawners puzzles just by replacing `extransmissions::rule` with `extrawners::rule`, with the exception of IORule, as it doesn't play nice with how Extrawners works.

## C# Puzzles
Journals/Campaigns and other packs of puzzles with a mod attached can use C# to make Extrawners puzzles using `Presets.Add` and one of the Presets present in the Presets class.

## Yaml Puzzles
To use the yaml format, place a `[puzzle id].extrawners.yaml` file in either your mod's Puzzles folder, or your custom puzzles folder.

You can see `PresetEntry` in `YamlFormat.cs` for a class that contains every possible value for the yaml file, though not all of them are valid for every preset type.

Check out `yaml-test.extrawners.yaml` and `yaml-test.puzzle.yaml` for a slightly overwhelming example containing basically every kind of preset included.