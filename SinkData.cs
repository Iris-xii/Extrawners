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
internal record struct SinkEffect {
  internal enum K : byte { IGNORE, ACCEPT, SINK_NO_PROGRESS, SINK_CRASH }
  private K k;
  private Molecule? targetSim;
  private bool advanceSeq;
  internal static SinkEffect FromEffect(K e, Molecule simM) =>
    new() { k = e, targetSim = simM };
  internal static SinkEffect Ignore() => new() { k = K.IGNORE };
  internal static SinkEffect Accept(Molecule simM) => new() { k = K.ACCEPT, targetSim = simM };
  internal static SinkEffect AcceptAdvanceSeq(Molecule simM) => 
    new() { k = K.ACCEPT, targetSim = simM, advanceSeq = true };
  internal static SinkEffect SinkNoProgress(Molecule simM) => new() { k = K.SINK_NO_PROGRESS, targetSim = simM };
  internal static SinkEffect SinkCrash(Molecule simM) => new() { k = K.SINK_CRASH, targetSim = simM };
  private bool ShouldSink() => k != K.IGNORE;
  private bool ShouldProgress() => k == K.ACCEPT;
  internal void UpdateState(SpawnerState state,Sim sim,Part part) {
    var seb = sim.SEB();
    if(ShouldSink() && targetSim is not null) {
      sim.RemoveMolecule(targetSim);
      class_238.field_1991.field_1868.Play(seb);
      state.currentlySinkingRaw.Add(targetSim.SimCoordsToPart(part));
      if(ShouldProgress()) state.validOutputsSank += 1; 
      if(advanceSeq) state.moleculesInSequenceSank += 1;
      if(k == K.SINK_CRASH) {
        sim.method_1854_crash("Invalid outputs are not allowed in this puzzle.", part.method_1161(), part.method_1161());
      }
    }
  }
}
internal sealed record SinkData() {
  internal List<Molecule> progressMolecules = new();
  internal List<Molecule> noProgressMolecules = new();
  internal List<Molecule> crashMolecules = new();
  internal List<Molecule> sequencedProgressMolecules = new(); // <- suspiciously specific
  internal SinkEffect.K resultWhenFitButNoMatch = SinkEffect.K.IGNORE;

  internal SinkEffect TrySink(Molecule candidateSim,
      IEnumerable<HexIndex> holeHexes,
      Sim sim,
      Part part,
      int moleculesSunkFromSequence = -1) {
    if (moleculesSunkFromSequence >= 0 && sequencedProgressMolecules.Count > 0) {
      var len = sequencedProgressMolecules.Count;
      var templateShifted = sequencedProgressMolecules[moleculesSunkFromSequence % len].ShiftedBy(part);
      if (molecMatchesExact(candidateSim, templateShifted) && !sim.MoleculeHeld(candidateSim)) {
        return SinkEffect.AcceptAdvanceSeq(candidateSim);
      }
    }
    foreach (var rawM in progressMolecules) {
      var templateShifted = rawM.ShiftedBy(part);
      if (molecMatchesExact(candidateSim, templateShifted) && !sim.MoleculeHeld(candidateSim)) {
        return SinkEffect.Accept(candidateSim);
      }
    }
    foreach (var rawM in noProgressMolecules) {
      var templateShifted = rawM.ShiftedBy(part);
      if (molecMatchesExact(candidateSim, templateShifted) && !sim.MoleculeHeld(candidateSim)) {
        return SinkEffect.SinkNoProgress(candidateSim);
      }
    }
    foreach (var rawM in crashMolecules) {
      var templateShifted = rawM.ShiftedBy(part);
      if (molecMatchesExact(candidateSim, templateShifted) && !sim.MoleculeHeld(candidateSim)) {
        return SinkEffect.SinkNoProgress(candidateSim);
      }
    }
    var sinkAnyTemplateRaw = new Molecule();
    foreach (var hex in holeHexes) {
      sinkAnyTemplateRaw.method_1106(VanillaAtoms.salt, hex);
    }
    var sinkAnyTemplateShifted = sinkAnyTemplateRaw.ShiftedBy(part);
    if (MolecMatchesSinkAny(candidateSim, sinkAnyTemplateShifted)) {
      return SinkEffect.FromEffect(resultWhenFitButNoMatch, candidateSim);
    }
    return SinkEffect.Ignore();
  }
}