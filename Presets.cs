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

  internal static GlyphData? LoadPresets(Puzzle puzzle, Solution sol) {
    var puzzleId = puzzle.field_2766;
    var tableOk = presetsTable.TryGetValue(puzzleId, out var maybePresetsFromTable);
    if (tableOk) {
      var output = new GlyphData();
      foreach (var preset in maybePresetsFromTable) {
        preset(output, puzzle, sol);
      }
      return output;
    }
    return null;
  }

  public static Preset MultiOutput(List<Molecule> okOutputs,
      bool sinkAny = false,
      bool wrongMolCrashesSim = false,
      int requiredProducts = 6) {
    if (requiredProducts <= 0) { requiredProducts = 6; }
    HexesAndBonds(okOutputs, out var hexes, out var sortaBonds);
    float molCountF = (float)okOutputs.Count;
    return (gd, puzzle, sol) => {
      var nextGlyph = PushOrigin(gd);
      gd.partTypeModify += (partTypes, sol) => {
        partTypes[nextGlyph].SetHexesToAllMols(okOutputs);
        string name = okOutputs.Count > 1 ? "Multi-Output" : "Output";
        string descPartOne = okOutputs.Count > 1 ? "This output accepts multiple potential products." : "A product for the alchemical engine.";
        string descPartTwo = "";
        if (sinkAny && wrongMolCrashesSim) {
          descPartTwo = " It also accepts any molecule that may fit, but inserting an incorrect molecule will halt the alchemical engine.";
        }
        else if (sinkAny && !wrongMolCrashesSim) {
          descPartTwo = " It also accepts any molecule that may fit, but it will not count as progress towards the solution.";
        }
        partTypes[nextGlyph].SetName(name);
        partTypes[nextGlyph].SetDescription($"{descPartOne}{descPartTwo}");
      };
      gd.partRenderer += (glyphIndex, part, pos, seb, renderer) => {
        var pss = PSS(seb, part);
        if (glyphIndex == nextGlyph) {
          SpawnerGlyph.DrawFullBaseFromHexesAndBonds(renderer, hexes, sortaBonds);
          SpawnerGlyph.DrawMolAsIfOutput(okOutputs[(int)Math.Floor(seb.AccumulatedTime() % molCountF)],
            seb, pss, renderer, pos, part);
        }
      };
      gd.logicFn += (Sim sim, LogicWhen when) => {
        var seb = sim.SEB();
        foreach (Part thisPart in sim.PartList().Where(p => p.Type() == SpawnerGlyph.partTypes[nextGlyph])) {
          var pss = PSS(seb, thisPart);
          if (when == LogicWhen.PRE_CYCLE && sim.Cycle() == 0) {
            AutoStatesReset(sim, thisPart, isOutput: true);
            thisPart.SetRequiredOutputs(requiredProducts);
          }
          else if (when == LogicWhen.PRE_CYCLE) {
            AutoStatesReset(sim, thisPart, isOutput: true);
          }
          else if (when.FireGlyph()) {
            pss.GetDefaultDynState().isOutput = true;
            foreach (var rawM in okOutputs) {
              if (SpawnerGlyph.ShouldAcceptMol(sim, rawM, pss, thisPart,
                  out var accepted, molecMatchesFn: null)) {
                SpawnerGlyph.QueueMolAnimation(sim, rawM, pss, thisPart);
                sim.RemoveMolecule(accepted);
                class_238.field_1991.field_1868.Play(seb);
                pss.AddToCurrentOutputs(1, requiredProducts);
                break;
              }
            }
            foreach (var rawM in okOutputs) {
              if (sinkAny && SpawnerGlyph.ShouldAcceptMol(sim, rawM, pss, thisPart,
                  out var acceptedWrong, molecMatchesFn: MolecMatchesSinkAny)) {
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
      int bagMult = 1) {
    var moleculesBag = new List<Molecule>();
    bagMult = bagMult < 1 ? 1 : bagMult;
    for (int i = 0; i < bagMult; i++) {
      moleculesBag.AddRange(randomBag);
    }
    float molCountF = (float)moleculesBag.Count;
    HexesAndBonds(randomBag, out var hexes, out var sortaBonds);
    return (gd, puzzle, sol) => {
      var nextGlyph = PushOrigin(gd);
      gd.partTypeModify += (partTypes, sol) => {
        partTypes[nextGlyph].SetHexesToAllMols(moleculesBag);
        partTypes[nextGlyph].SetName("Random Input");
        partTypes[nextGlyph].SetDescription("This reagent may be one of several randomly chosen molecules.");
      };
      gd.partRenderer += (glyphIndex, part, pos, seb, renderer) => {
        var pss = PSS(seb, part);
        if (glyphIndex == nextGlyph) {
          SpawnerGlyph.DrawFullBaseFromHexesAndBonds(renderer, hexes, sortaBonds);
          SpawnerGlyph.DrawMolAsIfInput(moleculesBag[(int)Math.Floor(seb.AccumulatedTime() % molCountF)],
            seb, pss, pos, part);
        }
      };
      gd.logicFn += (Sim sim, LogicWhen when) => {
        var seb = sim.SEB();
        foreach (var thisPart in sim.PartList().Where(p => p.Type() == SpawnerGlyph.partTypes[nextGlyph])) {
          var pss = PSS(seb, thisPart);
          if (when == LogicWhen.PRE_CYCLE && sim.Cycle() == 0) {
            AutoStatesReset(sim, thisPart, false);
            if (sim.Cycle() == 0) {
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

  internal static int PushOrigin(GlyphData gd) {
    gd.origins.Add(new HexIndex(gd.origins.Count, gd.origins.Count + 1 * 4));
    return gd.origins.Count - 1;
  }
}