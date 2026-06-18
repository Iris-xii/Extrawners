using Quintessential;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using Quintessential.Serialization;

namespace Extrawners;

using PartType = class_139;
using PartTypes = class_191;
using Permissions = enum_149;
using AtomTypes = class_175;
using Font = class_1;
using Texture = class_256;
using Song = class_186;
using VanillaAtoms = Brimstone.API.VanillaAtoms;
using BF = System.Reflection.BindingFlags;
using static ExtrawnersExt;
using static ExtrawnersMod;

#nullable enable
public static class YamlFormat {

  /// <summary>
  /// An entry in the .extrawners.yaml file, with all the fields aggregated together.
  /// (So if ANY preset uses a certain field, it's probably present here) <br></br>
  /// The only one that's really required is Type, the others will be used by some Presets and not others.
  /// </summary>
  internal record class PresetEntry {
    public string Type = "";
    public List<PuzzleModel.MoleculeM>? RandomBag = null;
    public MODependency[]? DependentOutputs = null;
    public List<PuzzleModel.MoleculeM>? OkOutputs = null;
    public bool? SinkAny = null;
    public bool? WrongMolCrashesSim = null;
    public int? RequiredProducts = null;
    public bool? FixDisjointMolecules = null;
    public string CustomName = "";
    public string CustomDesc = "";
    public HexIndex? ForcedOrigin = null;
    public List<PuzzleModel.MoleculeM>? SpawnAtBeginning = null;
    public MODependency[]? SpawnOnOutput = null;

    public record class MODependency {
      public int OutputGlyphIndex = -1;
      public int OutputMoleculeIndex = -1;
      public PuzzleModel.MoleculeM[]? Molecules = null;
      public Presets.MultiOutputDependency ToPresetForm() => new(OutputGlyphIndex,OutputMoleculeIndex) {
        molecules = Molecules.Select(mm => mm.FromModel()).ToArray()
      };
    }
  }

  internal static void YamlStringToGlyphData(string yamlString, ref GlyphData glyphData, Puzzle puzzle, Solution sol) {
    var presetEntry = YamlHelper.Deserializer.Deserialize<List<PresetEntry>>(yamlString);
    foreach (var e in presetEntry) {
      Log($"Reading {e} from yaml...");
      if (e.Type == "RandomInputRule" || e.Type == "RandomInput") {
        Presets.MultiOutputDependency[]? maybeDepOutput = null;
        if (e.DependentOutputs is PresetEntry.MODependency[] depOuts) {
          maybeDepOutput = depOuts.Select(dO => dO.ToPresetForm()).ToArray();
        }
        Presets.RandomInputRule(e.RandomBag.Select(mm => mm.FromModel()).ToList(),
          dependentOutputs: maybeDepOutput,
          customName: e.CustomName,
          customDesc: e.CustomDesc,
          forcedOrigin: e.ForcedOrigin,
          fixDisjointMolecules: e.FixDisjointMolecules is bool b? b: false)
        (glyphData, puzzle, sol); // <- Don't forget this
      }
      else if (e.Type == "MultiOutput") {
        Presets.MultiOutput(e.OkOutputs.Select(e => e.FromModel()).ToList(),
          sinkAny: e.SinkAny is bool sinkB ? sinkB : false,
          wrongMolCrashesSim: e.WrongMolCrashesSim is bool wrongB ? wrongB : false,
          mRequiredProducts: e.RequiredProducts,
          customName: e.CustomName,
          customDesc: e.CustomDesc,
          forcedOrigin: e.ForcedOrigin)
        (glyphData, puzzle, sol);
      }
      else if(e.Type == "Spawner") {
        Presets.Spawner(
          spawnAtBeginning: e.SpawnAtBeginning?.Select(e=>e.FromModel()).ToList(),
          spawnOnOutput: e.SpawnOnOutput?.Select(m => m.ToPresetForm()).ToArray(),
          customName: e.CustomName,
          customDesc: e.CustomDesc,
          forcedOrigin: e.ForcedOrigin,
          fixDisjointMolecules: e.FixDisjointMolecules is bool b? b: false
        )
        (glyphData,puzzle,sol);
      }
      else {
        throw new InvalidDataException($"{e.Type} matches no known preset.");
      }
    }
  }

  internal static bool TryFindYaml(Puzzle currentlyLoading, out GlyphData maybeGlyphData, Puzzle puzzle, Solution sol) {
    maybeGlyphData = new();
    var customPath = Path.Combine(class_269.field_2102, "custom");
    string targetFile = $"{currentlyLoading.PuzzleId()}.extrawners.yaml";
    string? foundFilePathFull = null;
    foreach (var filepath in Directory.EnumerateFiles(customPath)) {
      var filename = Path.GetFileName(filepath);
      if (filename == targetFile) {
        foundFilePathFull = filepath;
        break;
      }
    }
    foreach (var puzzleDir in QuintessentialLoader.ModPuzzleDirectories) {
      foreach (var filepath in Directory.EnumerateFiles(puzzleDir)) {
        var filename = Path.GetFileName(filepath);
        if (filename == targetFile) {
          foundFilePathFull = filepath;
          break;
        }
      }
    }
    if (foundFilePathFull is not null) {
      Log($"Found extrawners file: {Path.GetFileName(foundFilePathFull)}");
      string fileContents = File.ReadAllText(foundFilePathFull);
      YamlStringToGlyphData(fileContents, ref maybeGlyphData, puzzle, sol);
      return true;
    }
    return false;
  }
}