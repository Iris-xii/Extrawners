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
using Quintessential;
using System;

using static Extrawners.ExtrawnersMod;
using static ExtrawnersExt;
using static LogicWhen;


#nullable enable
internal sealed class SpawnerState {

  private int _partTypesIndex = -1;
  internal int PartTypesIndex { get => _partTypesIndex; set => _partTypesIndex = value; }
  internal int PuzzleDataGlyphsIndex { get => PartTypesIndex; } // currently, they're the same
  internal readonly SpawnerGlyph glyph;

  /// <summary> For <see cref="SinkData.sequencedProgressMolecules"/> </summary>
  internal int moleculesInSequenceSank = 0;

  /// <summary> For progress, when an output. See data's required molecules to see if it is an output </summary>
  internal int validOutputsSank = 0;

  /// <summary> Queue of molecules this glyph wishes to spawn </summary>
  private List<Molecule> spawnQueueRaw = new();

  internal List<Molecule> currentlySpawningRaw = new();
  internal List<Molecule> currentlySinkingRaw = new();


  internal void RealizeSpawningQueue(Part part, Sim sim) {
    List<Molecule> toRemove = new();
    foreach (var rawM in currentlySpawningRaw) {
      if (DoesNotOverlap(sim, part, rawM)) {
        var shifted = rawM.ShiftedBy(part);
        sim.AddMolecule(shifted);
        if (glyph.fixDisjointMolecules) { Brimstone.API.ForceRecomputeBonds(rawM); }
      }
      toRemove.Add(rawM);
    }
    foreach (var rem in toRemove) { currentlySpawningRaw.Remove(rem); }
  }

  internal void BeginSpawning(Random rng, Part part, Sim sim, bool ignoreCooldown = false) {
    while (true) { //attempt to spawn as many molecules as we're allowed, no longer just one
      if (spawnQueueRaw.Count <= 0) { //attempt a refill
        spawnQueueRaw.AddRange(glyph.produceData.repeatingRefillQueue);
      }
      if (spawnQueueRaw.Count > 0) {
        var choose = glyph.produceData.queueChooseMethod;
        if (choose.PeekChooseFrom(spawnQueueRaw, rng, out var chosenIdx) is Molecule rawM) {
          var shifted = rawM.ShiftedBy(part);
          HashSet<HexIndex> occupiedInQueue = new(currentlySpawningRaw
            .SelectMany(m => m.ShiftedBy(part).method_1100().Keys));
          if (DoesNotOverlap(sim, part, shifted, occupiedInQueue)) {
            spawnQueueRaw.RemoveAt(chosenIdx);
            currentlySpawningRaw.Add(rawM);
            if (glyph.fixDisjointMolecules) { Brimstone.API.ForceRecomputeBonds(shifted); }
            continue;
          }
        }
      }
      break;
    }
  }

  internal bool IsSatisfied() =>
    validOutputsSank >= glyph.requiredProducts;

  internal SpawnerState(SpawnerGlyph data) {
    this.glyph = data with { };
    this.PartTypesIndex = data.partTypesIndex;
    spawnQueueRaw.AddRange(data.produceData.initialSpawnQueue);
  }
}