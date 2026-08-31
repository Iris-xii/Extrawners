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
  internal int moleculesInSequenceSunk = 0;

  /// <summary> For progress, when an output. See data's required molecules to see if it is an output </summary>
  internal int validOutputsSunk = 0;

  internal SpawnerState(SpawnerGlyph glyph) {
    this.glyph = glyph with { };
    this.PartTypesIndex = glyph.partTypesIndex;
  }
}