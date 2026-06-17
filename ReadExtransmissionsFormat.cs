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

/// <summary>
/// Handling extransmissions-style format, to more easily switch over old puzzles
/// </summary>
public static class ExtransmissionsFormat {
  private const string PREFIX = "extrawners::rule::";


  [Serializable]
  public class MultiOutput {
    public int OutputMol = -1; //this is now unused
    public List<PuzzleModel.PuzzleIoM>? Accepts = null;
    public bool SinkAny = false;
    public bool WrongMolCrashesSim = false;
    public int RequiredProducts = 6; //brand new piece of data, because Extrawners requires it
  }
  [Serializable]
  public class RandomInputRule {
    public int InputMol = -1;  //this is now unused
    public List<PuzzleModel.PuzzleIoM>? RandomBag = null;
    public int BagMult = 1;
  }
  // Actually, this one rule probably doesn't translate to Extrawners
  // nearly well enough to include. :(
  // [Serializable]
  // public class IOPair {
  //   public int InputMol = -1;  //this is now unused
  //   public int OutputMol = -1; //this is now unused
  //   public List<PuzzleModel.PuzzleIoM>? RandomInputs = null;
  //   public List<PuzzleModel.PuzzleIoM>? RandomOutputs = null;
  // }
  public static bool TryRead(Puzzle puzzle, Solution sol, out GlyphData glyphData, ref List<int> inputsToRemove, ref List<int> outputsToRemove, bool actualSolLoad) {
    glyphData = new();
    bool any = false;
    foreach (var customPerm in puzzle.CustomPermissions) {
      ReadCustomPermissionString(customPerm, glyphData, puzzle, sol, setTrueIfAlteredGd: ref any, inputsToRemove, outputsToRemove);
    }
    return any;
  }

  // yoinked from Extransmissions
  private static void ReadCustomPermissionString(string customPermissionString, GlyphData gd, Puzzle p, Solution sol,
   ref bool setTrueIfAlteredGd,
   List<int> inputsToRemove,
   List<int> outputsToRemove) {
    if (customPermissionString.StartsWith(PREFIX)) {
      var withoutEtPrefix = customPermissionString.Substring(PREFIX.Length);
      int sepLocation = withoutEtPrefix.IndexOf("::");
      var type = sepLocation >= 0 ? withoutEtPrefix.Substring(0, sepLocation) : "";
      var withoutPrefix = sepLocation >= 0 ? withoutEtPrefix.Substring(sepLocation + "::".Length) : "";
      Log($"Loading type: {type}");
      // I'm hardcoding these, it's just support to quickly turn Extranmissions puzzles into Extrawners
      // not meant to be a long term thing
      if (type == "RandomInputRule" || type == "RandomInput") {
        var data = YamlHelper.Deserializer.Deserialize<RandomInputRule>(withoutPrefix);
        if (data is null || data.RandomBag is null) { throw new InvalidDataException($"Couldn't parse {withoutPrefix}"); }
        List<Molecule> rawRandomBag = data.RandomBag.Select(q => q.Molecule.FromModel()).ToList();
        List<Molecule> multipliedRandomBag = new();
        foreach (var item in rawRandomBag) {
          var limit = data.BagMult;
          limit = limit < 1 ? 1 : limit;
          for (int i = 0; i < limit; i++) {
            multipliedRandomBag.Add(item);
          }
        }
        Presets.RandomInputRule(multipliedRandomBag)(gd, p, sol);
        inputsToRemove.Add(data.InputMol);
        setTrueIfAlteredGd = true;
      }
      else if (type == "MultiOutput") {
        var data = YamlHelper.Deserializer.Deserialize<MultiOutput>(withoutPrefix);
        if (data is null || data.Accepts is null) { throw new InvalidDataException($"Couldn't parse {withoutPrefix}"); }
        List<Molecule> actualAccepts = data.Accepts.Select(q => q.Molecule.FromModel()).ToList();
        Presets.MultiOutput(actualAccepts, data.SinkAny, data.WrongMolCrashesSim, data.RequiredProducts)(gd, p, sol);
        outputsToRemove.Add(data.OutputMol);
        setTrueIfAlteredGd = true;
      }
      else if (type == "IOPair") {
        throw new NotSupportedException($"Reading IOPair rules into Extrawners isn't supported because the way Extrawners works in this regard is too different (the InputMol and OutputMol are usually ignored by Extrawners as it does not override/hook default OM i/o, but this rule requires them to work at all)");
      }
      else {
        Log($"No rules matched input: {withoutPrefix}");
        throw new InvalidOperationException($"No rules matched input");
      }
    }
  }
}