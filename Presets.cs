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

  public static Preset RandomInputRule(List<Molecule> randomBag,
      int bagMult = 1) {
    var moleculesBag = new List<Molecule>();
    bagMult = bagMult < 1 ? 1 : bagMult;
    for (int i = 0; i < bagMult; i++) {
      moleculesBag.AddRange(randomBag);
    }
    HashSet<HexIndex> hexes = new();
    HashSet<Pair<HexIndex, HexIndex>> sortaBonds = new();
    float molCountF = (float)moleculesBag.Count;
    foreach (var mol in moleculesBag) {
      hexes.UnionWith(mol.method_1100().Select(a => a.Key));
      sortaBonds.UnionWith(mol.method_1101().Select(a => new Pair<HexIndex, HexIndex>(a.field_2187, a.field_2188)));
    }
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
            seb, pss, pos, part, maybeSimStarted: seb.method_503() != enum_128.Stopped);
        }
      };
      gd.logicFn += (Sim sim, LogicWhen when) => {
        var seb = sim.SEB();
        foreach (var thisPart in sim.PartList().Where(p => p.Type() == SpawnerGlyph.partTypes[nextGlyph])) {
          var pss = PSS(seb, thisPart); 
          if (when == LogicWhen.PRE_CYCLE && sim.Cycle() == 0) {
            pss.SetDynState<Random>("rng", new(seb.Solution().Puzzle().PuzzleId().GetHashCode()));
            pss.SetDynState<List<Molecule>>("curBag", randomBag.ToList());
          }
          var rng = pss.GetDynState<Random>("rng");
          var bag = pss.GetDynState<List<Molecule>>("curBag");
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
          if (when == LogicWhen.PRE_CYCLE) {
            if (DoesNotOverlap(sim, thisPart, molecShifted)) {
              if (sim.Cycle() == 0) {
                sim.AddMolecule(molecShifted); 
              }
            }
          }
          else if (when.FireGlyph()) {
            if (DoesNotOverlap(sim, thisPart, molecShifted)) {
              sim.AddMolecule(molecShifted); 
            }
          }
          else if (when == LogicWhen.WELL_AFTER_CYCLE) {
            if (DoesNotOverlap(sim, thisPart, molecShifted)) {
              SpawnerGlyph.QueueMolAnimation(sim, outMolecRaw, pss, thisPart);
              pss.SetDynState<Molecule?>("cur", null);
            }
          }
        }
      };
    };
  }

  public static int PushOrigin(GlyphData gd) {
    gd.origins.Add(new HexIndex(gd.origins.Count, gd.origins.Count + 1 * 2));
    return gd.origins.Count - 1;
  }
}