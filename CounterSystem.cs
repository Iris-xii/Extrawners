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

internal sealed record CounterData {
  internal List<CounterOnSink> onExtrawnersSink = new();
  internal List<CounterOnProduce> onExtrawnersProduce = new();
  internal List<CounterWithdrawal> withdrawals = new();
}

/// <summary>
/// For 'dependent outputs' and related, mostlys
/// </summary>
internal sealed record CounterSystem {
  internal Dictionary<string, int> counters = new();
  internal CounterData data;

  internal CounterSystem(CounterData data) {
    this.data = data;
  }
  internal void WithdrawToProduce(SpawnerState state) {
    foreach (var withdrawal in data.withdrawals) {
      withdrawal.ProcessProducing(this, state);
    }
  }
  internal void WithdrawToSink(SpawnerState state) {
    foreach (var withdrawal in data.withdrawals) {
      withdrawal.ProcessSinkTakeover(this, state);
    }
  }
  internal void AddCountersProducing(SpawnerGlyph glyphData, IEnumerable<Molecule> rawSpawnedMols) {
    foreach (var counterOnProduce in data.onExtrawnersProduce) {
      foreach (var rawMolec in rawSpawnedMols) {
        counterOnProduce.TryUpdateCounters(this, glyphData, rawMolec);
      }
    }
  }
  internal void AddCountersSank(SinkEffect eff, Part part, SpawnerGlyph glyphData) {
    foreach (var counterOnSink in data.onExtrawnersSink) {
      counterOnSink.TryUpdateCounters(this, eff, part, glyphData);
    }
  }
}
internal sealed record CounterWithdrawal {
  private CounterWithdrawal(SpawnerGlyph target) { this.__target = target; }
  private CounterWithdrawal(int glyphIndex) { this.__glyphIdx = glyphIndex; }
  private enum K { PRODUCE, SINK_TAKEOVER }
  private K k = K.PRODUCE;
  internal Dictionary<string, int> withdrawal = new();
  internal List<Molecule>? outputOnceIfProduceRaw = null;
  internal List<Molecule>? nowAcceptsTheseIfSinkTakover = null;
  private bool sinkTakeoverForce = false;

  private SpawnerGlyph? __target;
  private int __glyphIdx = -1;
  private int __queueIdx = 0;
  private bool IsTarget(SpawnerState state) {
    if (__target is not null) {
      return state.glyph == __target;
    }
    if (__glyphIdx >= 0) {
      return state.glyph.partTypesIndex == __glyphIdx;
    }
    return false;
  }

  internal static CounterWithdrawal Producing(List<Molecule> toProduceOnce,
  Dictionary<string, int> withdrawals,
  int onQueueIndex,
  SpawnerGlyph spawnTarget) => new(spawnTarget) {
    withdrawal = withdrawals,
    k = K.PRODUCE,
    __queueIdx = onQueueIndex,
    outputOnceIfProduceRaw = toProduceOnce,
  };
  internal static CounterWithdrawal Producing(List<Molecule> toProduceOnce,
  Dictionary<string, int> withdrawals,
  int onQueueIndex,
  int glyphIndex) => new(glyphIndex) {
    withdrawal = withdrawals,
    k = K.PRODUCE,
    __queueIdx = onQueueIndex,
    outputOnceIfProduceRaw = toProduceOnce,
  };
  internal static CounterWithdrawal SinkTakeover(List<Molecule> nowAcceptsThese,
  Dictionary<string, int> withdrawals,
  SpawnerGlyph sinkTarget,
  bool forceTakeover = false) => new(sinkTarget) {
    withdrawal = withdrawals,
    k = K.SINK_TAKEOVER,
    nowAcceptsTheseIfSinkTakover = nowAcceptsThese,
    sinkTakeoverForce = forceTakeover,
  };
  internal static CounterWithdrawal SinkTakeover(List<Molecule> nowAcceptsThese,
  Dictionary<string, int> withdrawals,
  int glyphIndex,
  bool forceTakeover = false) => new(glyphIndex) {
    withdrawal = withdrawals,
    k = K.SINK_TAKEOVER,
    nowAcceptsTheseIfSinkTakover = nowAcceptsThese,
    sinkTakeoverForce = forceTakeover,
  };
  internal void ProcessSinkTakeover(CounterSystem sys, SpawnerState state) {
    if (k != K.SINK_TAKEOVER) return;
    if (nowAcceptsTheseIfSinkTakover is null) return;
    if (!IsTarget(state)) return;
    if (sinkTakeoverForce) state.forceTakeOverAlways = true;
    var couldWithdraw = TryWithdraw(sys);
    if (!couldWithdraw) return;
    state.takeoverSinkSequence.AddRange(nowAcceptsTheseIfSinkTakover);
  }
  internal void ProcessProducing(CounterSystem sys, SpawnerState state) {
    if (k != K.PRODUCE) return;
    if (!IsTarget(state)) return;
    var couldWithdraw = TryWithdraw(sys);
    if (!couldWithdraw) return;
    var molQueue = state.spawningList.Get(__queueIdx);
    state.spawningList.Set(molQueue.Concat(outputOnceIfProduceRaw).ToList(), __queueIdx);
  }
  private bool TryWithdraw(CounterSystem sys) {
    foreach (var KV in withdrawal) {
      if (!sys.counters.ContainsKey(KV.Key)) sys.counters[KV.Key] = 0;
      if (sys.counters[KV.Key] < KV.Value) { return false; }
    }
    foreach (var KV in withdrawal) { sys.counters[KV.Key] -= KV.Value; }
    return true;
  }
}

internal record CounterOnProduce {
  internal List<CounterOp> ops = new();
  // conditions:
  internal List<int> needGlyphIndexIfNotEmpty = new();
  internal List<Molecule> mustHaveBeenOneOfIfNotEmptyRaw = new();
  internal void TryUpdateCounters(CounterSystem sys, SpawnerGlyph glyphData, Molecule producedLocalCoords) {
    if (needGlyphIndexIfNotEmpty.Count > 0
      && !needGlyphIndexIfNotEmpty.Any(i => i == glyphData.partTypesIndex)) {
      return;
    }
    if (mustHaveBeenOneOfIfNotEmptyRaw.Count > 0
     && !mustHaveBeenOneOfIfNotEmptyRaw.Any(chck => molecMatchesExact(chck, producedLocalCoords))) {
      return;
    }
    ops.ApplyOps(sys);
  }
}

internal record CounterOnSink {
  internal List<CounterOp> ops = new();
  // conditions:
  internal List<int> needGlyphIndexIfNotEmpty = new();
  /// <summary> Meaning: it had to have increased the current Outputs and made progress towards sim solution </summary>
  internal bool mustHaveProgressedOnSink = false;
  internal List<Molecule> mustHaveBeenOneOfIfNotEmptyRaw = new();
  internal void TryUpdateCounters(CounterSystem sys, SinkEffect eff, Part part, SpawnerGlyph glyphData) {
    if (needGlyphIndexIfNotEmpty.Count > 0
      && !needGlyphIndexIfNotEmpty.Any(i => i == glyphData.partTypesIndex)) {
      return;
    }
    if (!eff.ShouldSink()) return;
    if (mustHaveProgressedOnSink && !eff.ShouldProgress()) return;
    if (eff.MaybeSankMolecule() is not Molecule m) return;
    var raw = m.SimCoordsToPart(part);
    if (mustHaveBeenOneOfIfNotEmptyRaw.Count > 0
     && !mustHaveBeenOneOfIfNotEmptyRaw.Any(chck => molecMatchesExact(chck, raw))) {
      return;
    }
    ops.ApplyOps(sys);
  }
}

internal static class CounterExt {
  internal static void ApplyOps<T>(this T ops, CounterSystem sys)
  where T : IEnumerable<CounterOp> {
    foreach (var op in ops) { op.Apply(sys); }
  }
}

internal record struct CounterOp {
  private enum K { SUM, SUM_CLAMPED, SET_TO }
  private string target;
  private int arg1;
  private K k;
  internal static CounterOp Sum(string target, int a) => new() { target = target, arg1 = a, k = K.SUM };
  internal static CounterOp SumClamped(string target, int a) => new() { target = target, arg1 = a, k = K.SUM_CLAMPED };
  internal static CounterOp SetTo(string target, int to) => new() { target = target, arg1 = to, k = K.SET_TO };
  internal readonly void Apply(CounterSystem sys) {
    if (!sys.counters.ContainsKey(target)) sys.counters[target] = 0;
    switch (k) {
      case K.SUM:
        sys.counters[target] += arg1;
        break;
      case K.SUM_CLAMPED:
        var sum = sys.counters[target] + arg1;
        if (sum < 0) sum = 0;
        sys.counters[target] = sum;
        break;
      case K.SET_TO:
        sys.counters[target] = arg1;
        break;
    }
  }
}