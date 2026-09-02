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

using static Extrawners.ExtrawnersMod;
using static ExtrawnersExt;
using Quintessential;

using static LogicWhen;
#nullable enable

internal sealed record class ExPuzzleData {
  internal List<SpawnerGlyph> glyphs = new();
  internal CounterData counterData = new();

  internal SpawnerGlyph NewGlyph() {
    int next = glyphs.Count;
    var added = new SpawnerGlyph(next);
    glyphs.Add(added);
    return added;
  }

  internal void PreparePartTypes() {
    foreach (var glyph in glyphs) {
      var pType = SpawnerGlyph.partTypes[glyph.partTypesIndex];
      pType.field_1540 = glyph.CollisionHexes().ToArray();
      pType.field_1529 = class_134.method_253(glyph.customName ?? $"Input/Output [{glyph.partTypesIndex}]", string.Empty); // Name
      pType.field_1530 = class_134.method_253(glyph.customDesc ?? "", string.Empty); // Description
    }
  }
}
 

internal sealed record class SimState {
  internal readonly ExPuzzleData pData;
  internal Dictionary<Part, SpawnerState> spawnerStates;
  internal CounterSystem counterSystem;
  internal Random rng;

  internal SimState(ExPuzzleData pData, Solution sol) {
    pData.PreparePartTypes();
    this.pData = pData;
    this.counterSystem = new(pData.counterData);
    this.spawnerStates = new();
    this.rng = new(sol.Puzzle().field_2766.GetHashCode());
    foreach (var anyP in sol.PartList()) {
      var partType = anyP.Type();
      var matchingThisPart = pData.glyphs
        .Where(pdg => SpawnerGlyph.partTypes[pdg.partTypesIndex].field_1528 == partType.field_1528);
      foreach (var matchingSpawnerGlyph in matchingThisPart) {
        spawnerStates[anyP] = new(matchingSpawnerGlyph);
      }
    }
  }

  internal void RenderFn(int glyphIndex, Part part, Vector2 pos, SolutionEditorBase seb, class_195 renderer) {
    foreach (var spawnerStateKV in spawnerStates.Where(ss => ss.Value.PartTypesIndex == glyphIndex)) {
      var spawnerState = spawnerStateKV.Value;
      var data = spawnerState.glyph;
      var pss = PSS(seb, part);
      if (data.holeHexes.Count > 0) {
        SpawnerGlyph.DrawFullBaseFromHexesAndBonds(renderer,
          data.holeHexes,
          data.holeBonds,
          tbase: data.holeTextures.bg,
          ring: data.holeTextures.ring,
          bond: data.holeTextures.bond);
      }
      if (data.drawInputRawMolecules.Count > 0) {
        SpawnerGlyph.DrawMolAsIfInput(
          data.drawInputRawMolecules[(int)Math.Floor(seb.AccumulatedTime() % data.drawInputRawMolecules.Count)],
          seb, pss, pos, part,
          animateMoleculesRaw: spawnerState.currentlySpawningRaw);
      }
      if (data.drawOutputRawMolecules.Count > 0) {
        List<Molecule> preview;
        if (seb.method_503() == enum_128.Stopped) {
          preview = data.drawOutputRawMolecules;
        }
        else {
          preview = spawnerState.SinkPreview().ToList();
        }
        SpawnerGlyph.DrawMolAsIfOutput(
          preview[(int)Math.Floor(seb.AccumulatedTime() % preview.Count)],
          seb, pss, renderer, pos, part,
          animateMoleculesRaw: spawnerState.currentlySinkingRaw,
          doOutputText: data.requiredProducts > 0,
          requiredOutputs: data.requiredProducts,
          currentOutputs: spawnerState.validOutputsSank <= data.requiredProducts ?
            spawnerState.validOutputsSank : data.requiredProducts
        );
      }
    }
  }
  internal void LogicFn(Sim sim, LogicWhen when) {
    var seb = sim.SEB();

    // Ichor safe spots -- Hackish!
    if (when == PRE_CYCLE && sim.Cycle() == 0) {
      ExtransmutationsCompat.perDynGlyphSimSafeSpots = new();
      HashSet<HexIndex> ichorSafe = new();
      foreach (var KV in spawnerStates) {
        var state = KV.Value;
        var part = KV.Key;
        var data = state.glyph;
        var safeIn = data.drawInputRawMolecules
          .SelectMany(m => m.ShiftedBy(part).method_1100())
          .Where(kv => kv.Value.field_2275.QuintAtomType == "Extransmutations:ichor")
          .Select(kv => kv.Key);
        var safeOut = data.drawOutputRawMolecules
          .SelectMany(m => m.ShiftedBy(part).method_1100())
          .Where(kv => kv.Value.field_2275.QuintAtomType == "Extransmutations:ichor")
          .Select(kv => kv.Key);
        foreach (var item in safeIn.Concat(safeOut)) {
          ichorSafe.Add(item);
        }
      }
      ExtransmutationsCompat.perDynGlyphSimSafeSpots[0] = ichorSafe.ToList();
    }
    // Sink
    foreach (var KV in spawnerStates) {
      SpawnerState state = KV.Value;
      var part = KV.Key;
      var pss = PSS(seb, part);
      counterSystem.WithdrawToSink(state); //not affected by Extranscompat!
      if (when == PRE_CYCLE) {
        state.currentlySinkingRaw = new();
      }
      else if (when == FIRST_HALF && !ExtransmutationsCompat.isIchorSuppressionActive) {
        List<SinkEffect> effects = new();
        foreach (var simMolec in sim.field_3823) {
          SinkEffect effect = state.TrySink(simMolec, sim, part);
          effects.Add(effect);
        }
        foreach (var effect in effects) { effect.UpdateState(state, sim, part); }
        foreach (var effect in effects) { counterSystem.AddCountersSank(effect, part, state.glyph); }
      }
    }
    // Produce 
    foreach (var KV in spawnerStates) {
      var state = KV.Value;
      var part = KV.Key;
      var pss = PSS(seb, part);
      if (when == PRE_CYCLE && sim.Cycle() == 0) { //spawn starting molec
        state.BeginSpawning(rng, part, sim );
        state.RealizeSpawningQueue(part, sim, out var didSpawnQueue);
        counterSystem.AddCountersProducing(state.glyph, didSpawnQueue);
      }
      else if (when.FireGlyph()) {
        counterSystem.WithdrawToProduce(state);
        state.RealizeSpawningQueue(part, sim, out var didSpawnQueue);
        counterSystem.AddCountersProducing(state.glyph, didSpawnQueue);
      }
      else if (when == MID_CYCLE_B4_ANIM) {
        state.BeginSpawning(rng, part, sim);
      }
    }
  }

  internal bool IsExtrawnersSatisfied() =>
    spawnerStates.All(KV => KV.Value.IsSatisfied());
}