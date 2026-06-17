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
using System.Runtime.CompilerServices;
using Quintessential;
using System;

using static ExtrawnersExt;

using static LogicWhen;

#nullable enable
public static class SpawnerGlyph {

  public const string PART_ID = "extrawners-glyph";

  public static PartType[] partTypes = new PartType[0];
  internal static GlyphData.RenderFn glyphRenderer = (_, _, _, _, _) => { };
  internal static GlyphData.LogicFn logicFn = (_, _) => { };

  internal static PartType SpawnerPartTypeNew(int number) => new() {
    field_1528 = PART_ID + $"-{number}", // ID
    field_1529 = class_134.method_253($"Input/Output [{number}]", string.Empty), // Name
    field_1530 = class_134.method_253("", string.Empty), // Description
    field_1531 = 0, // Cost
    field_1539 = true, // Is a glyph 
    field_1549 = null, // Shadow/glow
    field_1550 = null, // Stroke/outline
    field_1547 = Resources.genericBase[number], // Panel icon
    field_1548 = Resources.genericBase[number], // Hovered panel icon
    field_1552 = true, // only one?
    field_1540 = new HexIndex[]{
                new(0, 0),
            }, // Spaces used
    field_1536 = true,
    field_1551 = Permissions.None,
    IsForced = true,
    CustomPermissionCheck = perms => false,
  };

  internal static void SpawnerPartTypeReset(int number, PartType pt) {
    pt.field_1528 = PART_ID + $"-{number}"; // ID
    pt.field_1529 = class_134.method_253($"Input/Output [{number}]", string.Empty); // Name
    pt.field_1530 = class_134.method_253("", string.Empty); // Description
    pt.field_1531 = 0; // Cost
    pt.field_1539 = true; // Is a glyph 
    pt.field_1549 = null; // Shadow/glow
    pt.field_1550 = null; // Stroke/outline
    pt.field_1547 = Resources.genericBase[number]; // Panel icon
    pt.field_1548 = Resources.genericBase[number]; // Hovered panel icon
    pt.field_1552 = true; // only one?
    pt.field_1540 = new HexIndex[]{
                new(0, 0),
            }; // Spaces used
    pt.field_1536 = true;
    pt.field_1551 = Permissions.None;
    pt.IsForced = true;
    pt.CustomPermissionCheck = perms => false;
  }

  internal static int ExtractIndex(string field_1528) {
    var lastPart = field_1528.Substring(PART_ID.Length + 1);
    return int.Parse(lastPart);
  }


  public static void DrawFullBaseFromMol(
      class_195 renderer,
      Molecule mol,
      float offset_x = -23f,
      float offset_y = 24f,
      Texture? tbase = null,
      Texture? ring = null,
      Texture? bond = null) {
    var hexes = mol.method_1100().Select(a => a.Key);
    foreach (var hex in hexes) {
      DrawBase(renderer, hex, tbase, ring);
    }
    foreach (var c277 in mol.method_1101()) {
      var from = c277.field_2187;
      var to = c277.field_2188;
      DrawBaseBond(renderer, from: from, to: to, offset_x, offset_y, bond);
    }
  }
  public static void DrawFullBaseFromHexesAndBonds<H, B>(
    class_195 renderer,
    H hexes,
    B bonds,
    float offset_x = -23f,
    float offset_y = 24f,
    Texture? tbase = null,
    Texture? ring = null,
    Texture? bond = null)
     where H : IEnumerable<HexIndex> where B : IEnumerable<Pair<HexIndex, HexIndex>> {
    foreach (var hex in hexes) {
      DrawBase(renderer, hex, tbase, ring);
    }
    foreach (var c277 in bonds) {
      var from = c277.Left;
      var to = c277.Right;
      DrawBaseBond(renderer, from: from, to: to, offset_x, offset_y, bond);
    }
  }


  public static void DrawBaseBond(
      class_195 renderer,
      HexIndex from,
      HexIndex to,
      float offset_x = -23f,
      float offset_y = 24f,
      Texture? bond = null) {
    float angle = class_187.field_1742.method_492(to - from).Angle();
    var OFFSET = new Vector2(offset_x, offset_y); //new Vector2(-23f, 20f);
    renderer.method_526(bond ?? Resources.pipe_bond,
      from,
      Vector2.Zero,
      OFFSET,
      angle);
  }
  public static void DrawBase(
      class_195 renderer,
      HexIndex hexPos,
      Texture? tbase = null,
      Texture? ring = null) {
    //renderer.method_530(base_empty_fuzz, hexPos, 0f);
    renderer.method_530(tbase ?? Resources.pipe_base, hexPos, 0f);
    renderer.method_528(ring ?? Resources.pipe_ring, hexPos, Vector2.Zero);

  }
  public static void DrawMolAsIfInput(Molecule rawM,
    SolutionEditorBase seb,
    PartSimState pss,
    Vector2 rendererPos,
    Part part,
    Molecule? maybeAnimateMolecule = null) {
    var state = pss.GetDefaultDynState();
    Molecule? animateMolecule = maybeAnimateMolecule ?? state.animatingMolecule;
    if (seb.method_503() == enum_128.Stopped) { DrawMol(rawM, pss, rendererPos, part); }
    if (animateMolecule is not null) {
      DrawMol(animateMolecule, pss, rendererPos, part, fractionOnBoard: seb.AnimTime());
    }
  }
  public static void DrawMolAsIfOutput(Molecule rawM,
      SolutionEditorBase seb,
      PartSimState pss,
      class_195 renderer,
      Vector2 rendererPos,
      Part part,
      Molecule? maybeAnimateMolecule = null) {
    var state = pss.GetDefaultDynState();
    Molecule? animateMolecule = maybeAnimateMolecule ?? state.animatingMolecule;
    DrawMol(rawM, pss, rendererPos, part, shadowStrength: 0f, alpha: 0.4f);
    if (animateMolecule is not null) {
      var alpha = seb.AnimTime() < 0.5f ? 1f : class_162.method_416(seb.AnimTime(), 0.5f, 1f, 1f, 0f);
      DrawMol(animateMolecule, pss, rendererPos, part, fractionOnBoard: class_162.method_416(seb.AnimTime(), 0f, 1f, 1f, 0f), alpha: alpha);
    }
    if (seb is SolutionEditorScreen ses && seb.method_503() != enum_128.Stopped) {
      string currentCount = $"{pss.CurrentOutputs()}/{part.GetRequiredOutputs()}";
      Vector2 off = renderer.field_1797;
      class_135.method_272(class_238.field_1989.field_101.field_783, off);
      class_135.method_290(currentCount, off + new Vector2(28f, 7f), class_238.field_1990.field_2143, class_181.field_1718, (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, default(Color), null, int.MaxValue, param_3473: false, param_3474: true);
    }
  }
  public static void DrawMol(Molecule rawM,
      PartSimState pss,
      Vector2 rendererPos,
      Part part,
      float rotation = 0f,
      float alpha = 1f,
      float fractionOnBoard = 1f,
      float shadowStrength = 1f,
      bool light = false) {
    Editor.method_925(rawM.method_1115(PartRotation(pss)),
      rendererPos,
      -part.method_1161(), //hexindex
      rotation /*rotation*/,
      alpha /*alpha*/,
      fractionOnBoard /* 0 = gone*/,
      shadowStrength /*shadow str*/, light /*light*/, null);
  }

  public static void QueueMolAnimation(Sim sim,
      Molecule rawM,
      PartSimState pss,
      Part part) {
    var state = pss.GetDefaultDynState();
    state.animatingMolecule = rawM;
  }
  public static void SpawnMol(Sim sim,
    Molecule rawM,
    PartSimState pss,
    Part part) {
    var shifted = rawM.ShiftedBy(part);
    sim.AddMolecule(shifted);
  }
  public static void AsIfInput(Sim sim,
    Molecule rawM,
    PartSimState pss,
    Part part,
    LogicWhen when,
    bool doAutoStatesReset = true) {
    var shifted = rawM.ShiftedBy(part);
    if (when == PRE_CYCLE) {
      if (doAutoStatesReset) { AutoStatesReset(sim, part, isOutput: false); }
      if (DoesNotOverlap(sim, part, shifted)) {
        if (sim.Cycle() == 0) {
          sim.AddMolecule(shifted);
        }
      }
    }
    else if (when.FireGlyph()) {
      if (DoesNotOverlap(sim, part, shifted)) {
        sim.AddMolecule(shifted);
      }
    }
    else if (when == WELL_AFTER_CYCLE) {
      if (DoesNotOverlap(sim, part, shifted)) {
        QueueMolAnimation(sim, rawM, pss, part);
      }
    }
  }
  public static void AsIfOutput(Sim sim,
    Molecule rawM,
    PartSimState pss,
    Part part,
    LogicWhen when,
    bool doAutoStatesReset = true,
    Func<Molecule, Molecule, bool>? molecMatchesFn = null,
    int outputsAmount = 6) {
    var seb = sim.SEB();
    if (when == PRE_CYCLE) {
      if (doAutoStatesReset) { AutoStatesReset(sim, part, isOutput: true); }
      part.SetRequiredOutputs(outputsAmount);
    }
    else if (when.FireGlyph()) {
      if (ShouldAcceptMol(sim, rawM, pss, part, out var accepted, molecMatchesFn)) {
        QueueMolAnimation(sim, rawM, pss, part);
        sim.RemoveMolecule(accepted);
        class_238.field_1991.field_1868.Play(seb);
        pss.AddToCurrentOutputs(1, outputsAmount);
      }
    }
  }
  public static bool ShouldAcceptMol(Sim sim,
      Molecule rawTemplateM,
      PartSimState pss,
      Part part,
      out Molecule accepted,
      Func<Molecule, Molecule, bool>? molecMatchesFn = null,
      bool doIchor = true) {
    if (doIchor && ExtransmutationsCompat.isIchorSuppressionActive) { accepted = null!; return false; }
    molecMatchesFn ??= molecMatchesExact;
    var seb = sim.SEB();
    var templateShifted = rawTemplateM.ShiftedBy(part);
    foreach (var m in sim.field_3823) {
      if (molecMatchesFn(m, templateShifted) && !sim.MoleculeHeld(m)) {
        accepted = m;
        return true;
      }
    }
    accepted = null!;
    return false;
  }

  private static void RendererOld(Part part,
      Vector2 pos,
      SolutionEditorBase seb,
      class_195 renderer) {
    //PartType pType = part.Type(); 
    //Vector2 centre = genericBase.method_691();
    //var time = seb.method_504();
    //class_236 uco = seb.method_1989(part, pos);
    //PartSimState pss = seb.method_507().method_481(part);
    //var solution = seb.method_502();

    //renderer.method_523(genericBase, new Vector2(-1, -1), centre, 0f);

    //foreach (var m in data.molecules) {
    //  //Vector2 SCREEN_OFFSET = ses.field_4009;
    //  Editor.method_925(m.method_1115(pss.field_2726), pos, -part.method_1161(), 0f /*rotation*/, 1f /*alpha*/, 1f /* 0 = gone*/, 1f /*shadow str*/, false /*light*/, null);
    //}
  }
  public const int MAX_SPAWNERS = 16;
  internal static void LoadPuzzleContent() {
    List<PartType> partTypesList = new();
    for (int i = 0; i < MAX_SPAWNERS; i++) {
      var pType = SpawnerPartTypeNew(i);
      partTypesList.Add(pType);
      QApi.AddPartType(pType, (part, pos, seb, renderer) => {
        PartType pType = part.Type();
        for (int glyphIdx = 0; glyphIdx < partTypes.Length; glyphIdx++) {
          if (partTypes[glyphIdx].field_1528 == pType.field_1528) {
            SpawnerGlyph.glyphRenderer(glyphIdx, part, pos, seb, renderer);
            break;
          }
        }
      });
      QApi.AddPartTypeToPanel(pType, false);
    }
    QApi.RunAfterCycle((sim, firstHalf) => {
      ExtransmutationsCompat.isIchorSuppressionActive = false;
      ExtransmutationsCompat.updateIsIchorSuppressionActive(sim);
      if (firstHalf) { logicFn(sim, LogicWhen.FIRST_HALF); }
      if (!firstHalf) { logicFn(sim, LogicWhen.SECOND_HALF); }
    });
    partTypes = partTypesList.ToArray();
  }

  internal static void Cleanup() {
    for (int i = 0; i < MAX_SPAWNERS; i++) {
      SpawnerPartTypeReset(i, partTypes[i]);
    }
  }

}