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
public static class Presets {
  public delegate void Preset(GlyphData gdToModify, Puzzle puzzle, Solution sol);
  public static Dictionary<string, List<Preset>> presetsTable = new();
  internal static Dictionary<string, Pair<List<int>, List<int>>> removeTable = new();

  public static void Add(Puzzle puzzle, List<Preset> presets) => presetsTable.Add(puzzle.PuzzleId(), presets);
  public static void Add(string puzzleID, List<Preset> presets) => presetsTable.Add(puzzleID, presets);

  internal static GlyphData? LoadPresets(Puzzle puzzle, Solution sol, bool actualSolLoad) {
    List<int> inputsToRemove = new();
    List<int> outputsToRemove = new();
    GlyphData? toReturn = null;
    var puzzleId = puzzle.field_2766;
    if (presetsTable.TryGetValue(puzzleId, out var maybePresetsFromTable)) {
      var output = new GlyphData();
      foreach (var preset in maybePresetsFromTable) {
        preset(output, puzzle, sol);
      }
      toReturn = output;
    }
    else if (ExtransmissionsFormat.TryRead(puzzle, sol, out var extransmissionsGD, ref inputsToRemove, ref outputsToRemove, actualSolLoad)) {
      toReturn = extransmissionsGD;
    }
    else if (YamlFormat.TryFindYaml(puzzle, out var glyphData, puzzle, sol)) {
      toReturn = glyphData;
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

  public static void RemoveInputsAndOutputsOnlyDuringSolve(Puzzle puzzle, List<int> inputsToRemove, List<int> outputsToRemove) {
    removeTable.Add(puzzle.PuzzleId(), new Pair<List<int>, List<int>>(inputsToRemove, outputsToRemove));
  }
  public static void RemoveInputsAndOutputsOnlyDuringSolve(string puzzleId, List<int> inputsToRemove, List<int> outputsToRemove) {
    removeTable.Add(puzzleId, new Pair<List<int>, List<int>>(inputsToRemove, outputsToRemove));
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
  //                      *--- Allow normal outputs this time too?
  //                      v
  // n inputs spawned per k outputs......
  //  (with an additional c inputs at the beginning)
  // Can be the same as the Catalyst preset too, but for a normal catalyst you just have the first c inputs and the rest is just n = 0 inputs per k outputs
  public static Preset Spawner(
      List<Molecule>? spawnAtBeginning = null,
      MultiOutputDependency[]? spawnOnOutput = null,
      string customName = "",
      string customDesc = "",
      HexIndex? forcedOrigin = null,
      bool fixDisjointMolecules = false) {
    spawnAtBeginning ??= new();
    spawnOnOutput ??= new MultiOutputDependency[0];
    var combined = spawnAtBeginning.Concat(spawnOnOutput.SelectMany(m => m.molecules)).ToList();
    float molCountF = (float)combined.Count;
    HexesAndBonds(combined, out var hexes, out var sortaBonds);
    return (gd, puzzle, sol) => {
      var nextGlyph = PushOrigin(gd, forcedOrigin);
      gd.partTypeModify += (partTypes, sol) => {
        string name = spawnOnOutput.Length == 0 ? "Catalyst" : "External Process";
        string howMany = spawnAtBeginning.Count > 1 ? $"producing {spawnAtBeginning.Count} molecules to be used as a catalyst." : "producing a single molecule to be used as a catalyst.";
        if (spawnAtBeginning.Count <= 0) { howMany = "allegedly, though of dubious utility."; }
        string desc = spawnOnOutput.Length == 0 ? $"A catalyst for the transmutation engine, {howMany}"
         : "An external process/synthesis connected to this transmutation engine, producing extra molecules on output.";
        partTypes[nextGlyph].SetName(customName == "" ? name : customName);
        partTypes[nextGlyph].SetDescription(customDesc == "" ? desc : customDesc);
      };
      gd.partRenderer += (glyphIndex, part, pos, seb, renderer) => {
        var pss = PSS(seb, part);
        if (glyphIndex == nextGlyph) {
          SpawnerGlyph.DrawFullBaseFromHexesAndBonds(renderer, hexes, sortaBonds,
            tbase: Resources.spawner_pipe_base,
            ring: Resources.spawner_pipe_ring,
            bond: Resources.spawner_pipe_bond);
          if (combined.Count > 1) {
            SpawnerGlyph.DrawMolAsIfInput(combined[(int)Math.Floor(seb.AccumulatedTime() % molCountF)],
              seb, pss, pos, part);
          }
          else if (combined.Count == 1) {
            SpawnerGlyph.DrawMolAsIfInput(combined[0],
              seb, pss, pos, part);
          }
        }
      };
      gd.multiOutputSuccessfulOutputCallbacks += (int outputIdx, int moleculeIdx, Sim sim) => {
        var seb = sim.SEB();
        foreach (var thisPart in sim.PartList().Where(p => p.Type() == SpawnerGlyph.partTypes[nextGlyph])) {
          var pss = PSS(seb, thisPart);
          var queue = pss.GetDynStateOrDef<Queue<Molecule>>("molQueue");
          bool queueChanged = false;
          foreach (var soo in spawnOnOutput) {
            if (soo.outputGlyphIndex != outputIdx) { continue; }
            if (soo.outputMoleculeIndex != moleculeIdx) { continue; }
            foreach (var mol in soo.molecules) {
              queue.Enqueue(mol);
              queueChanged = true;
            }
          }
          if (queueChanged) {
            pss.SetDynState<bool>("animatingOutPush", true);
            SpawnerGlyph.QueueMolAnimation(sim, queue.Peek(), pss, thisPart);
          }
        }
      };
      gd.logicFn += (Sim sim, LogicWhen when) => {
        var seb = sim.SEB();
        foreach (var thisPart in sim.PartList().Where(p => p.Type() == SpawnerGlyph.partTypes[nextGlyph])) {
          var pss = PSS(seb, thisPart);
          ExtransmutationsCompat.InputDoIchor(nextGlyph, thisPart, combined);
          if (when == LogicWhen.PRE_CYCLE) {
            AutoStatesReset(sim, thisPart, false);
            if (sim.Cycle() == 0) { pss.SetDynState<Queue<Molecule>>("molQueue", new(spawnAtBeginning)); }
            if (sim.Cycle() == 0
                && pss.GetDynStateOrDef<Queue<Molecule>>("molQueue") is Queue<Molecule> molQueue
                && molQueue.Count > 0) {
              var outMolecRaw = molQueue.Peek();
              var molecShifted = outMolecRaw.ShiftedBy(thisPart);
              if (DoesNotOverlap(sim, thisPart, molecShifted)) {
                molQueue.Dequeue();
                sim.AddMolecule(molecShifted);
                if (fixDisjointMolecules) { Brimstone.API.ForceRecomputeBonds(molecShifted); }
              }
            }
          }
          else if (when.FireGlyph()) {
            pss.GetDefaultDynState().animatingMolecule = null;
            if (pss.GetDynStateOrDef<Queue<Molecule>>("molQueue") is Queue<Molecule> molQueue
               && molQueue.Count > 0
               && !pss.GetDynStateOrDef<bool>("animatingOutPush")) {
              var outMolecRaw = molQueue.Peek();
              var molecShifted = outMolecRaw.ShiftedBy(thisPart);
              if (DoesNotOverlap(sim, thisPart, molecShifted)) {
                molQueue.Dequeue();
                sim.AddMolecule(molecShifted);
                if (fixDisjointMolecules) { Brimstone.API.ForceRecomputeBonds(molecShifted); }
              }
            }
            pss.SetDynState<bool>("animatingOutPush", false);
          }
          else if (when == LogicWhen.WELL_AFTER_CYCLE) {
            if (pss.GetDynStateOrDef<Queue<Molecule>>("molQueue") is Queue<Molecule> molQueue
                && molQueue.Count > 0) {
              var outMolecRaw = molQueue.Peek();
              var molecShifted = outMolecRaw.ShiftedBy(thisPart);
              if (DoesNotOverlap(sim, thisPart, molecShifted)) {
                SpawnerGlyph.QueueMolAnimation(sim, outMolecRaw, pss, thisPart);
              }
            }
          }
        }
      };
    };
  }


  public static Preset MultiOutput(List<Molecule> okOutputs,
      bool sinkAny = false,
      bool wrongMolCrashesSim = false,
      int? mRequiredProducts = null,
      string customName = "",
      string customDesc = "",
      HexIndex? forcedOrigin = null) {
    int requiredProducts = (int)(mRequiredProducts is null ? 6 : mRequiredProducts);
    if (requiredProducts < 0) { requiredProducts = 6; }
    HexesAndBonds(okOutputs, out var hexes, out var sortaBonds);
    float molCountF = (float)okOutputs.Count;
    return (gd, puzzle, sol) => {
      var nextGlyph = PushOrigin(gd, forcedOrigin);
      SpawnerGlyph.partTypes[nextGlyph].SetDynState<Queue<Molecule>?>("dep", null); //<- anything using partTyes.setState needs to be reset per puzzle
      gd.partTypeModify += (partTypes, sol) => {
        partTypes[nextGlyph].SetHexesToAllMols(okOutputs);
        string name = customName != "" ? customName
          : okOutputs.Count > 1 ? "Multi-Output" : "Output";
        string descPartOne = okOutputs.Count > 1 ? "This output accepts multiple potential products." : "A product for the alchemical engine.";
        string descPartTwo = "";
        string descPartDependant = partTypes[nextGlyph].GetDynStateOrNull<Queue<Molecule>>("dep") == null ? "" : "\nThis output additionally depends on one of the inputs, requiring that you output a certain molecule after pulling a matching molecule from the input.";
        if (sinkAny && wrongMolCrashesSim) {
          descPartTwo = " It also accepts any molecule that may fit, but inserting an incorrect molecule will halt the alchemical engine.";
        }
        else if (sinkAny && !wrongMolCrashesSim) {
          descPartTwo = " It also accepts any molecule that may fit, but it will not count as progress towards the solution.";
        }
        partTypes[nextGlyph].SetName(name);
        partTypes[nextGlyph].SetDescription(customDesc == "" ? $"{descPartOne}{descPartTwo}{descPartDependant}" : customDesc);
        partTypes[nextGlyph].SetAsOutput();
      };
      gd.partRenderer += (glyphIndex, part, pos, seb, renderer) => {
        var pss = PSS(seb, part);
        if (glyphIndex == nextGlyph) {
          if (sinkAny && !wrongMolCrashesSim) {
            SpawnerGlyph.DrawFullBaseFromHexesAndBonds(renderer, hexes, sortaBonds,
            tbase: Resources.blue_pipe_base,
            ring: Resources.blue_pipe_ring,
            bond: Resources.blue_pipe_bond);
          }
          else if (sinkAny && wrongMolCrashesSim) {
            SpawnerGlyph.DrawFullBaseFromHexesAndBonds(renderer, hexes, sortaBonds,
            tbase: Resources.crimson_pipe_base,
            ring: Resources.crimson_pipe_ring,
            bond: Resources.crimson_pipe_bond);
          }
          else {
            SpawnerGlyph.DrawFullBaseFromHexesAndBonds(renderer, hexes, sortaBonds);
          }
          var maybeQ = SpawnerGlyph.partTypes[glyphIndex].GetDynStateOrNull<Queue<Molecule>>("dep");
          if (maybeQ is not null && seb.IsRunning() != enum_128.Stopped) {
            if (maybeQ.Count > 0) {
              SpawnerGlyph.DrawMolAsIfOutput(maybeQ.Peek(),
                seb, pss, renderer, pos, part);
            }
          }
          else {
            SpawnerGlyph.DrawMolAsIfOutput(okOutputs[(int)Math.Floor(seb.AccumulatedTime() % molCountF)],
              seb, pss, renderer, pos, part);
          }
        }
      };
      gd.logicFn += (Sim sim, LogicWhen when) => {
        var seb = sim.SEB();
        foreach (Part thisPart in sim.PartList().Where(p => p.Type() == SpawnerGlyph.partTypes[nextGlyph])) {
          var pss = PSS(seb, thisPart);
          if (when == LogicWhen.PRE_CYCLE && sim.Cycle() == 0) {
            thisPart.SetRequiredOutputs(requiredProducts);
          }
          else if (when == LogicWhen.PRE_CYCLE) {
            AutoStatesReset(sim, thisPart, isOutput: true);
          }
          else if (when.FireGlyph()) {
            ExtransmutationsCompat.OutputDoIchor(nextGlyph, thisPart, okOutputs, SpawnerGlyph.partTypes[nextGlyph].GetDynStateOrNull<Queue<Molecule>>("dep"));
            pss.GetDefaultDynState().isOutput = true;
            foreach (var rawM in okOutputs) {
              if (SpawnerGlyph.partTypes[nextGlyph].GetDynStateOrNull<Queue<Molecule>>("dep") is Queue<Molecule> q) {
                if (!molecMatchesExact(q.Peek(), rawM)) {
                  continue;
                }
              }
              if (SpawnerGlyph.ShouldAcceptMol(sim, rawM, pss, thisPart,
                  out var accepted, molecMatchesFn: null)) {
                SpawnerGlyph.QueueMolAnimation(sim, rawM, pss, thisPart);
                sim.RemoveMolecule(accepted);
                int molIndex = -1; // <- for spawner
                for (int i = 0; i < okOutputs.Count; i++) {
                  var item = okOutputs[i];
                  if (molecMatchesExact(item, rawM)) {
                    molIndex = i;
                    break;
                  }
                }
                gd.multiOutputSuccessfulOutputCallbacks(nextGlyph, molIndex, sim); // <- for spawner
                if (SpawnerGlyph.partTypes[nextGlyph].GetDynStateOrNull<Queue<Molecule>>("dep") is Queue<Molecule> q2) {
                  q2.Dequeue();
                }
                class_238.field_1991.field_1868.Play(seb);
                pss.AddToCurrentOutputs(1, requiredProducts);
                break;
              }
            }
            foreach (var rawM in okOutputs) {
              if (sinkAny && SpawnerGlyph.ShouldAcceptMol(sim, rawM, pss, thisPart,
                  out var acceptedWrong, molecMatchesFn: MolecMatchesSinkAny) && rawM.method_1100().Count > 0
                  && acceptedWrong.method_1100().Count > 0) {
                SpawnerGlyph.QueueMolAnimation(sim, acceptedWrong.SimCoordsToPart(thisPart), pss, thisPart);
                sim.RemoveMolecule(acceptedWrong);
                class_238.field_1991.field_1868.Play(seb);
                if (wrongMolCrashesSim) {
                  sim.method_1854_crash("Invalid outputs are not allowed in this puzzle.", thisPart.method_1161(), thisPart.method_1161());
                }
              }
            }
          }
          else if (when == LogicWhen.WELL_AFTER_CYCLE) { }
        }
      };
    };
  }

  public static Preset RandomInputRule(List<Molecule> randomBag,
      MultiOutputDependency[]? dependentOutputs = null,
      string customName = "",
      string customDesc = "",
      HexIndex? forcedOrigin = null,
      bool fixDisjointMolecules = false) {
    void WhenAddMolRaw(Molecule rawM) {
      int molIdx = -1;
      for (int i = 0; i < randomBag.Count; i++) {
        if (molecMatchesExact(rawM, randomBag[i])) {
          molIdx = i;
          break;
        }
      }
      if (molIdx == -1) { throw new InvalidDataException("molIdx is -1"); }
      if (dependentOutputs is not null) {
        foreach (var depOutput in dependentOutputs) {
          if (depOutput.outputMoleculeIndex != molIdx) { continue; }
          var q = SpawnerGlyph.partTypes[depOutput.outputGlyphIndex].GetDynStateOrNull<Queue<Molecule>>("dep");
          q ??= new();
          foreach (var m in depOutput.molecules) {
            q.Enqueue(m);
          }
          SpawnerGlyph.partTypes[depOutput.outputGlyphIndex].SetDynState("dep", q);
        }
      }
    }

    float molCountF = (float)randomBag.Count;
    HexesAndBonds(randomBag, out var hexes, out var sortaBonds);
    return (gd, puzzle, sol) => {
      var nextGlyph = PushOrigin(gd, forcedOrigin);
      gd.partTypeModify += (partTypes, sol) => {
        partTypes[nextGlyph].SetHexesToAllMols(randomBag);
        string maybeRandomInput = randomBag.Count > 1 ? "Random Input" : "Reagent";
        string maybeRandomDesc = randomBag.Count > 1 ? "This reagent may be one of several randomly chosen molecules." : "A reagent for the alchemical engine.";
        partTypes[nextGlyph].SetName(customName == "" ? maybeRandomInput : customName);
        partTypes[nextGlyph].SetDescription(customDesc == "" ? maybeRandomDesc : customDesc);
      };
      gd.partRenderer += (glyphIndex, part, pos, seb, renderer) => {
        var pss = PSS(seb, part);
        if (glyphIndex == nextGlyph) {
          SpawnerGlyph.DrawFullBaseFromHexesAndBonds(renderer, hexes, sortaBonds,
            tbase: randomBag.Count > 1 ? Resources.blue_pipe_base : Resources.pipe_base,
            ring: randomBag.Count > 1 ? Resources.blue_pipe_ring : Resources.pipe_ring,
            bond: randomBag.Count > 1 ? Resources.blue_pipe_bond : Resources.pipe_bond);
          SpawnerGlyph.DrawMolAsIfInput(randomBag[(int)Math.Floor(seb.AccumulatedTime() % molCountF)],
            seb, pss, pos, part);
        }
      };
      gd.logicFn += (Sim sim, LogicWhen when) => {
        var seb = sim.SEB();
        foreach (var thisPart in sim.PartList().Where(p => p.Type() == SpawnerGlyph.partTypes[nextGlyph])) {
          var pss = PSS(seb, thisPart);
          ExtransmutationsCompat.InputDoIchor(nextGlyph, thisPart, randomBag);
          if (when == LogicWhen.PRE_CYCLE) {
            AutoStatesReset(sim, thisPart, false);
            if (sim.Cycle() == 0) {
              if (dependentOutputs is not null) {
                foreach (var depOutput in dependentOutputs) {
                  SpawnerGlyph.partTypes[depOutput.outputGlyphIndex].SetDynState("dep", new Queue<Molecule>());
                }
              }
              pss.SetDynState<Random>("rng", new(seb.Solution().Puzzle().PuzzleId().GetHashCode()));
              pss.SetDynState<List<Molecule>>("curBag", randomBag.ToList());
              {
                var rng = pss.GetDynStateOrDef<Random>("rng");
                var bag = pss.GetDynStateOrDef<List<Molecule>>("curBag");
                Molecule? cur = pss.GetDynStateOrNull<Molecule?>("cur");
                if (bag.Count == 0) {
                  bag = randomBag.ToList();
                  pss.SetDynState("curBag", bag);
                }
                if (cur is null) {
                  var i = rng.Next(0, bag.Count);
                  cur = bag[i];
                  bag.RemoveAt(i);
                  pss.SetDynState("cur", cur);
                }
                var outMolecRaw = cur;
                var molecShifted = outMolecRaw.ShiftedBy(thisPart);
                if (DoesNotOverlap(sim, thisPart, molecShifted)) {
                  sim.AddMolecule(molecShifted);
                  if (fixDisjointMolecules) { Brimstone.API.ForceRecomputeBonds(molecShifted); }
                  WhenAddMolRaw(outMolecRaw);
                  pss.SetDynState<Molecule?>("cur", null);
                }
              }
            }
          }
          else if (when.FireGlyph()) {
            pss.GetDefaultDynState().animatingMolecule = null;
            var rng = pss.GetDynStateOrDef<Random>("rng");
            var bag = pss.GetDynStateOrDef<List<Molecule>>("curBag");
            Molecule? cur = pss.GetDynStateOrNull<Molecule?>("cur");
            if (bag.Count == 0) {
              bag = randomBag.ToList();
              pss.SetDynState("curBag", bag);
            }
            if (cur is null) {
              var i = rng.Next(0, bag.Count);
              cur = bag[i];
              bag.RemoveAt(i);
              pss.SetDynState("cur", cur);
            }
            var outMolecRaw = cur;
            var molecShifted = outMolecRaw.ShiftedBy(thisPart);
            if (DoesNotOverlap(sim, thisPart, molecShifted)) {
              sim.AddMolecule(molecShifted);
              if (fixDisjointMolecules) { Brimstone.API.ForceRecomputeBonds(molecShifted); }
              WhenAddMolRaw(outMolecRaw);
              pss.SetDynState<Molecule?>("cur", null);
            }
          }
          else if (when == LogicWhen.WELL_AFTER_CYCLE) {
            var rng = pss.GetDynStateOrDef<Random>("rng");
            var bag = pss.GetDynStateOrDef<List<Molecule>>("curBag");
            Molecule? cur = pss.GetDynStateOrNull<Molecule?>("cur");
            if (bag.Count == 0) {
              bag = randomBag.ToList();
              pss.SetDynState("curBag", bag);
            }
            if (cur is null) {
              var i = rng.Next(0, bag.Count);
              cur = bag[i];
              bag.RemoveAt(i);
              pss.SetDynState("cur", cur);
            }
            var outMolecRaw = cur;
            var molecShifted = outMolecRaw.ShiftedBy(thisPart);
            if (DoesNotOverlap(sim, thisPart, molecShifted)) {
              SpawnerGlyph.QueueMolAnimation(sim, outMolecRaw, pss, thisPart);
            }
          }
        }
      };
    };
  }
  public struct MultiOutputDependency {
    public int outputGlyphIndex = 0;
    public int outputMoleculeIndex = 0;
    public Molecule[] molecules = new Molecule[0];

    public MultiOutputDependency(int glyphIndex, int molIndex) { outputGlyphIndex = glyphIndex; outputMoleculeIndex = molIndex; }
  }

  private static int PushOrigin(GlyphData gd) {
    gd.origins.Add(new HexIndex(gd.origins.Count - (gd.origins.Count % 2 == 0 ? 0 : 4), gd.origins.Count + 1 * 5));
    return gd.origins.Count - 1;
  }
  private static int PushOrigin(GlyphData gd, HexIndex? optLocation) {
    if (optLocation is not null) {
      gd.origins.Add((HexIndex)optLocation);
      return gd.origins.Count - 1;
    }
    else {
      return PushOrigin(gd);
    }
  }
}