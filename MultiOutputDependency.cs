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

/// <summary> Spawns `molecules` when defined output swallows a molecule. </summary>
internal struct MultiOutputDependency {
  internal int outputGlyphIndex = 0;
  internal int outputMoleculeIndex = 0; // <- somewhat dubious
  internal Molecule[] molecules = new Molecule[0];
  internal MultiOutputDependency(int glyphIndex, int molIndex) { outputGlyphIndex = glyphIndex; outputMoleculeIndex = molIndex; }
}
internal static class MultiOutputDependencyExt {
  internal static CounterData ToCounterData(this List<MultiOutputDependency> list) {
    int nextAvailableCounterName = 0;
    List<CounterOnSink> cos = new();
    List<CounterWithdrawal> withdrawals = new();
    foreach (var mod in list) {
      cos.Add(new CounterOnSink() {
        needGlyphIndexIfNotEmpty = new() { mod.outputGlyphIndex },
        mustHaveProgressedOnSink = true,
        ops = new() {
          CounterOp.Sum($"{nextAvailableCounterName}",1)
        }
      });
      withdrawals.Add(CounterWithdrawal.Producing(mod.molecules.ToList(),
        new() { { $"{nextAvailableCounterName}", 1 } }));

      nextAvailableCounterName += 1;
    }
    return new CounterData() {
      onExtrawnersSink = cos,
      withdrawals = withdrawals
    };
  }
}