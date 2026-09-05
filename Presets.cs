using Quintessential;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using Quintessential.Serialization;
using System.Collections.Generic;
using System.Reflection;
using Extrawners.API;

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
internal static class Presets {
  internal static Dictionary<string, Pair<List<int>, List<int>>> removeTable = new();

  internal static ExPuzzleData? LoadPresets(Puzzle puzzle, Solution sol, bool actualSolLoad) {
    List<int> inputsToRemove = new();
    List<int> outputsToRemove = new();
    ExPuzzleData? toReturn = null;
    var puzzleId = puzzle.field_2766;
    if (TryGetPuzzleFile($"{puzzleId}.extrawners.dll") is string fileNameFull) { 
      var ass = Assembly.LoadFrom(fileNameFull); 
      var maybeIExtPuzzles = ass.GetTypes().Where(type => typeof(IExtrawnersPuzzle).IsAssignableFrom(type) &&
      !type.IsInterface && !type.IsAbstract && type.IsPublic).ToList();
      if (maybeIExtPuzzles.Count > 1) {
        throw new InvalidDataException("Multiple IExtrawnersPuzzle implementations present. This is not allowed");
      }
      else if (maybeIExtPuzzles.Count == 0) {
        throw new InvalidDataException("No IExtrawnersPuzzle impl found. (Is the type public?)");
      }
      var iExtPuzzle = maybeIExtPuzzles.First();
      var activated = (IExtrawnersPuzzle)Activator.CreateInstance(iExtPuzzle);
      toReturn = new() {
        extrawnersPuzzleStatic = activated
      };
      var curIdx = 0;
      foreach (var potentialGlyph in activated.MakeExtrawnersGlyphs(new() {
        puzzleId = puzzleId
      })) {
        toReturn.glyphs.Add(potentialGlyph.ToExtrawners(curIdx));
        curIdx += 1;
      }
      inputsToRemove = activated.InputsToRemove();
      outputsToRemove = activated.OutputsToRemove();
    }
    else if (ExtransmissionsFormat.TryRead(puzzle, sol, out var extransmissionsPD, ref inputsToRemove, ref outputsToRemove, actualSolLoad)) {
      toReturn = extransmissionsPD;
    }
    else if (YamlFormat.TryFindYaml(puzzle, out var pData, puzzle, sol)) {
      toReturn = pData;
    }
    //removal
    if (actualSolLoad && (inputsToRemove.Count != 0 || outputsToRemove.Count != 0)) {
      RemoveInputsAndOutputsInternal(puzzle, inputsToRemove: inputsToRemove, outputsToRemove: outputsToRemove);
    }
    else if (actualSolLoad && removeTable.TryGetValue(puzzleId, out var data)) {
      RemoveInputsAndOutputsInternal(puzzle, data.Left, data.Right);
    }
    //
    return toReturn;
  }

  internal static void RemoveInputsAndOutputsOnlyDuringSolve(Puzzle puzzle, List<int> inputsToRemove, List<int> outputsToRemove) {
    removeTable[puzzle.PuzzleId()] = new Pair<List<int>, List<int>>(inputsToRemove, outputsToRemove);
  }
  internal static void RemoveInputsAndOutputsOnlyDuringSolve(string puzzleId, List<int> inputsToRemove, List<int> outputsToRemove) {
    removeTable[puzzleId] = new Pair<List<int>, List<int>>(inputsToRemove, outputsToRemove);
  }

  private static void RemoveInputsAndOutputsInternal(Puzzle puzzle, List<int> inputsToRemove, List<int> outputsToRemove) {
    PuzzleInputOutput[] inputs = puzzle.field_2770;
    PuzzleInputOutput[] outputs = puzzle.field_2771;
    List<PuzzleInputOutput> newInputs = new();
    List<PuzzleInputOutput> newOutputs = new();
    for (int i = 0; i < inputs.Length; i++) {
      if (inputsToRemove.Contains(i)) {
        Log($"Input #{i} will be removed.");
        continue;
      }
      newInputs.Add(inputs[i]);
    }
    for (int i = 0; i < outputs.Length; i++) {
      if (outputsToRemove.Contains(i)) {
        Log($"Output #{i} will be removed.");
        continue;
      }
      newOutputs.Add(outputs[i]);
    }
    puzzle.field_2770 = newInputs.ToArray();
    puzzle.field_2771 = newOutputs.ToArray();
    resetPuzzleIODeleteHack += () => {
      puzzle.field_2770 = inputs;
      puzzle.field_2771 = outputs;
    };
  }


  internal static void Spawner(
      ref ExPuzzleData puzzleData,
      List<Molecule>? spawnAtBeginning = null,
      List<MultiOutputDependency>? spawnOnOutput = null,
      string argName = "",
      string argDesc = "",
      HexIndex? forcedOrigin = null,
      bool fixDisjointMolecules = false) {
    spawnAtBeginning ??= new();
    spawnOnOutput ??= new();
    var combined = spawnAtBeginning.Concat(spawnOnOutput.SelectMany(m => m.molecules));
    var combinedDeduplicated = combined.Distinct(new FuckingComparer()).ToList();

    var glyph = puzzleData.NewGlyph();
    {
      glyph.customName = argName == "" ? (spawnOnOutput.Count == 0 ? "Catalyst" : "External Process") : argName;
      string howMany = spawnAtBeginning.Count > 1 ? $"producing {spawnAtBeginning.Count} molecules to be used as a catalyst." : "producing a single molecule to be used as a catalyst.";
      if (spawnAtBeginning.Count <= 0) { howMany = "allegedly, though of dubious utility."; }
      string desc = spawnOnOutput.Count == 0 ? $"A catalyst for the transmutation engine, {howMany}"
       : "An external process/synthesis connected to this transmutation engine, producing extra molecules on output.";
      glyph.customDesc = argDesc == "" ? desc : argDesc;
      glyph.HexesAndBondsFromMolec = combined;
      glyph.holeTextures = Resources.spawner;
      glyph.drawInputRawMolecules = combinedDeduplicated;
      if (forcedOrigin is HexIndex fo) glyph.origin = fo;
      glyph.fixDisjointMolecules = fixDisjointMolecules;
      glyph.produceData = new() {
        perSpawnListProduceData = new() {
          {0,new() {initialSpawnQueue = spawnAtBeginning is null ? new() : spawnAtBeginning}}
        }
      };
    }
    if (spawnOnOutput is not null) puzzleData.counterData = spawnOnOutput.ToCounterDataSpawner(glyph);
  }


  internal static void MultiOutput(
      ref ExPuzzleData puzzleData,
      List<Molecule> okOutputs,
      bool sinkAny = false,
      bool wrongMolCrashesSim = false,
      int? mRequiredProducts = null,
      string argName = "",
      string argDesc = "",
      HexIndex? forcedOrigin = null,
      bool okOutputsIsSequence = false) {
    int requiredProducts = (int)(mRequiredProducts is null ? 6 : mRequiredProducts);
    if (requiredProducts < 0) { requiredProducts = 6; }

    var glyph = puzzleData.NewGlyph();
    {
      glyph.HexesAndBondsFromMolec = okOutputs;
      if (forcedOrigin is HexIndex fo) glyph.origin = fo;
      checked { glyph.requiredProducts = (ushort)requiredProducts; }

      glyph.customName = argName != "" ? argName
        : okOutputs.Count > 1 ? "Multi-Output" : "Output";
      string descPartOne = okOutputs.Count > 1 ? "This output accepts multiple potential products." : "A product for the alchemical engine.";
      string descPartTwo = "";
      if (sinkAny && wrongMolCrashesSim) {
        descPartTwo = " It also accepts any molecule that may fit, but inserting an incorrect molecule will halt the alchemical engine.";
      }
      else if (sinkAny && !wrongMolCrashesSim) {
        descPartTwo = " It also accepts any molecule that may fit, but it will not count as progress towards the solution.";
      }
      glyph.customDesc = argDesc == "" ? $"{descPartOne}{descPartTwo}" : argDesc;
      glyph.drawOutputRawMolecules = okOutputs;
      Resources.HoleGlyph color;
      if (sinkAny && !wrongMolCrashesSim) color = Resources.blue;
      else if (sinkAny && wrongMolCrashesSim) color = Resources.crimson;
      else color = Resources.normal;
      glyph.holeTextures = color;

      var fallback = SinkEffect.K.IGNORE;
      if (sinkAny && !wrongMolCrashesSim) fallback = SinkEffect.K.SINK_NO_PROGRESS;
      else if (sinkAny && wrongMolCrashesSim) fallback = SinkEffect.K.SINK_CRASH;
      glyph.sinkData = new() {
        progressMolecules = okOutputsIsSequence ? new() : okOutputs,
        sequencedProgressMolecules = okOutputsIsSequence ? okOutputs : new(),
        resultWhenFitButNoMatch = fallback
      };
    }
  }
  internal static void RandomInputRule(
    ref ExPuzzleData puzzleData,
    List<Molecule> randomBag,
    List<MultiOutputDependency>? dependentOutputs = null,
    string argName = "",
    string argDesc = "",
    HexIndex? forcedOrigin = null,
    bool fixDisjointMolecules = false,
    bool disableRng = false) {

    var glyph = puzzleData.NewGlyph();
    {
      if (forcedOrigin is HexIndex fo) glyph.origin = fo;
      glyph.fixDisjointMolecules = fixDisjointMolecules;
      glyph.HexesAndBondsFromMolec = randomBag;
      glyph.drawInputRawMolecules = randomBag;
      glyph.holeTextures = randomBag.Count > 1 ? Resources.blue : Resources.normal; //<- think about this harder
      string maybeRandomInput = randomBag.Count > 1 && !disableRng ? "Random Input" : "Reagent";
      string maybeRandomDesc = randomBag.Count > 1 && !disableRng ? "This reagent may be one of several randomly chosen molecules." : "A reagent for the alchemical engine.";
      glyph.customName = argName == "" ? maybeRandomInput : argName;
      glyph.customDesc = argDesc == "" ? maybeRandomDesc : argDesc;
      glyph.produceData = new() {
        perSpawnListProduceData = new() {
          {0,new() {
            repeatingRefillQueue = randomBag,
            queueChooseMethod = disableRng? SpawnChooseMethod.RepeatingSeq() : SpawnChooseMethod.Random(),
            }}
        },
      };
    }
    if (dependentOutputs is not null) puzzleData.counterData = dependentOutputs.ToCounterDataRandomInput(glyph, randomBag);
  }
}