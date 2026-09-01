
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
internal sealed partial record class SpawnerGlyph { 
  internal HexIndex origin;
  internal int partTypesIndex = -1;
  internal HashSet<HexIndex> holeHexes = new();
  internal HashSet<Pair<HexIndex, HexIndex>> holeBonds = new();
  internal string? customName = null;
  internal string? customDesc = null;
  internal Resources.HoleGlyph holeTextures = Resources.normal;
  /// <summary> Preview molecule as if it were an input (produces molecs) </summary>
  internal List<Molecule> drawInputRawMolecules = new();
  /// <summary> Preview molecule as if it were an output (sink) </summary>
  internal List<Molecule> drawOutputRawMolecules = new();
  internal bool fixDisjointMolecules = false; 
  /// <summary> If != 0, need this many products to complete the puzzle </summary>
  internal ushort requiredProducts = 0;
  /// <summary> Controls 'sinking' (accepting) of molecules </summary>
  internal SinkData sinkData = new();
  /// <summary> Controls input/production of molecules </summary>
  internal ProduceData produceData = new();
  internal IEnumerable<Molecule> HexesAndBondsFromMolec {
    set => HexesAndBondsRef(value, ref this.holeHexes, ref this.holeBonds);
  }  
  internal IEnumerable<HexIndex> CollisionHexes() => holeHexes;
  internal SpawnerGlyph(int partTypesIndex) { //I miss `required`
    this.partTypesIndex = partTypesIndex;
    origin = new HexIndex(partTypesIndex - (partTypesIndex % 2 == 0 ? 0 : 4), partTypesIndex + 1 * 5);
  }


}