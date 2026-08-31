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

/// <summary> Spawns `molecules` when defined output swallows a molecule. </summary>
internal struct MultiOutputDependency {
  internal int outputGlyphIndex = 0;
  internal int outputMoleculeIndex = 0; // <- somewhat dubious
  internal Molecule[] molecules = new Molecule[0];
  internal MultiOutputDependency(int glyphIndex, int molIndex) { outputGlyphIndex = glyphIndex; outputMoleculeIndex = molIndex; }
}

internal sealed record class ExPuzzleData {
  internal List<SpawnerGlyph> glyphs = new();
  /// <summary> Key is spawner idx. </summary>
  internal Dictionary<int, List<MultiOutputDependency>> multiOutputDependencyTemp = new(); // TODO: Find a way to represent this that doesn't suck as much

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
  internal List<SpawnerState> spawnerStates;

  internal SimState(ExPuzzleData pData) {
    pData.PreparePartTypes();
    this.pData = pData;
    this.spawnerStates = pData.glyphs.Select(d => new SpawnerState(d)).ToList();
  }

  internal void RenderFn(int glyphIndex, Part part, Vector2 pos, SolutionEditorBase seb, class_195 renderer) {
    var spawnerState = spawnerStates[glyphIndex];
    var data = spawnerState.glyph;
    var pss = PSS(seb,part);
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
        seb, pss, pos, part);
    }
    if(data.drawOutputRawMolecules.Count > 0) {
      SpawnerGlyph.DrawMolAsIfOutput(
        data.drawOutputRawMolecules[(int)Math.Floor(seb.AccumulatedTime() % data.drawOutputRawMolecules.Count)],
        seb,pss,renderer,pos,part,
        doOutputText: data.requiredProducts > 0,
        requiredOutputs: data.requiredProducts,
        currentOutputs: spawnerState.validOutputsSunk <= data.requiredProducts?
          spawnerState.validOutputsSunk : data.requiredProducts
      );
    }

  }

  internal void LogicFn(Sim sim, LogicWhen when) {

  }
}