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
  internal SpawningLists spawningList = new();

  internal List<Molecule> currentlySpawningRaw = new();
  internal List<Molecule> currentlySinkingRaw = new();

  /// <summary> When not empty, this totally takes over the normal sink behavior, requiring you fed it the sequence in order. </summary>
  internal List<Molecule> takeoverSinkSequence = new();
  internal bool forceTakeOverAlways = false; //hack

  internal object? userData = null;

  internal SinkEffect TrySink(
      Molecule candidateSim,
      Sim sim,
      Part part) {
    return glyph.sinkData.TrySinkInner(candidateSim,
    glyph.holeHexes,
    sim,
    part,
    moleculesInSequenceSank,
    ref takeoverSinkSequence,
    forceTakeOverAlways);
  }
  internal void RealizeSpawningQueue(Part part, Sim sim, out List<Molecule> didSpawnRaw) {
    didSpawnRaw = new();
    List<Molecule> toRemove = new();
    foreach (var rawM in currentlySpawningRaw) {
      if (DoesNotOverlap(sim, part, rawM.ShiftedToGlobal(part))) {
        var shifted = rawM.ShiftedToGlobal(part);
        sim.AddMolecule(shifted);
        didSpawnRaw.Add(rawM);
        if (glyph.fixDisjointMolecules) { Brimstone.API.ForceRecomputeBonds(rawM); }
        toRemove.Add(rawM);
      }
    }
    foreach (var rem in toRemove) { currentlySpawningRaw.Remove(rem); }
  }

  internal void BeginSpawning(Random rng, Part part, Sim sim) {
    foreach (var KV in spawningList.Enumerate()) {
      var spawnList = KV.Value;
      ProduceDataPerSpawnList prodData;
      if (glyph.produceData.perSpawnListProduceData.ContainsKey(KV.Key)) {
        prodData = glyph.produceData.perSpawnListProduceData[KV.Key];
      }
      else {
        prodData = new();
      }
      AttemptBeginSpawningPerList(spawnList, prodData, rng, part, sim);
    }
  }

  private void AttemptBeginSpawningPerList(List<Molecule> spawnList, ProduceDataPerSpawnList prodData,
  Random rng, Part part, Sim sim) {
    while (true) { //attempt to spawn as many molecules as we're allowed, no longer just one
      if (spawnList.Count <= 0) { //attempt a refill
        spawnList.AddRange(prodData.repeatingRefillQueue);
      }
      if (spawnList.Count > 0) {
        var choose = prodData.queueChooseMethod;
        if (choose.PeekChooseFrom(spawnList, rng, out var chosenIdx) is Molecule rawM) {
          var shifted = rawM.ShiftedToGlobal(part);
          HashSet<HexIndex> occupiedInQueue = new(currentlySpawningRaw
            .SelectMany(m => m.ShiftedToGlobal(part).method_1100().Keys));
          if (DoesNotOverlap(sim, part, shifted, occupiedInQueue)) {
            spawnList.RemoveAt(chosenIdx);
            currentlySpawningRaw.Add(rawM);
            if (glyph.fixDisjointMolecules) { Brimstone.API.ForceRecomputeBonds(shifted); }
            continue;
          }
        }
      }
      break;
    }
  }
  internal IEnumerable<Molecule> SinkPreview() {
    if (takeoverSinkSequence.Count > 0) {
      yield return takeoverSinkSequence[0];
      yield break;
    }
    else if (forceTakeOverAlways) { yield break; }
    foreach (var m in glyph.sinkData.progressMolecules) yield return m;
    var len = glyph.sinkData.sequencedProgressMolecules.Count;
    if (len > 0) {
      yield return glyph.sinkData.sequencedProgressMolecules[moleculesInSequenceSank % len];
    }
  }

  internal bool IsSatisfied() =>
    validOutputsSank >= glyph.requiredProducts;

  internal SpawnerState(SpawnerGlyph data) {
    this.glyph = data with { };
    this.PartTypesIndex = data.partTypesIndex;
    this.forceTakeOverAlways = data.forceTakeoverSequence;
    foreach (var KVprodData in data.produceData.perSpawnListProduceData) {
      var queue = this.spawningList.Get(KVprodData.Key);
      queue.AddRange(KVprodData.Value.initialSpawnQueue);
    }
  }
}