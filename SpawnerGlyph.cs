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

  public static readonly Texture base_empty_fuzz = class_235.method_615("textures/glyph_base_parts/base_empty_fuzz");
  public static readonly Texture base_bl = class_235.method_615("textures/glyph_base_parts/base_bl");
  public static readonly Texture base_br = class_235.method_615("textures/glyph_base_parts/base_br");
  public static readonly Texture base_l = class_235.method_615("textures/glyph_base_parts/base_l");
  public static readonly Texture base_r = class_235.method_615("textures/glyph_base_parts/base_r");
  public static readonly Texture base_tl = class_235.method_615("textures/glyph_base_parts/base_tl");
  public static readonly Texture base_tr = class_235.method_615("textures/glyph_base_parts/base_tr");
  public static readonly Texture pipe_ring = class_235.method_615("textures/parts/pipe_ring");
  public static readonly Texture pipe_base = class_235.method_615("textures/parts/pipe_base");
  public static readonly Texture pipe_bond = class_235.method_615("textures/parts/pipe_bond");
  public const string PART_ID = "extrawners-glyph";
  internal static Texture genericBase = Brimstone.API.GetTexture();

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
    field_1547 = genericBase, // Panel icon
    field_1548 = genericBase, // Hovered panel icon
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
    pt.field_1547 = genericBase; // Panel icon
    pt.field_1548 = genericBase; // Hovered panel icon
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


  public static void DrawFullBaseFromMol(Part part,
      Vector2 pos,
      SolutionEditorBase editor,
      class_195 renderer,
      Molecule mol) {
    var hexes = mol.method_1100().Select(a => a.Key);
    foreach (var hex in hexes) {
      DrawBase(part, pos, editor, renderer, hex);
    }
    foreach (var c277 in mol.method_1101()) {
      var from = c277.field_2187;
      var to = c277.field_2188;
      DrawBaseBond(part, pos, editor, renderer, from: from, to: to);
    }
  }

  public static void DrawBaseBond(Part part,
      Vector2 pos,
      SolutionEditorBase seb,
      class_195 renderer,
      HexIndex from,
      HexIndex to,
      float offset_x = -23f,
      float offset_y = 24f) {
    float angle = class_187.field_1742.method_492(to - from).Angle();
    var OFFSET = new Vector2(offset_x, offset_y); //new Vector2(-23f, 20f);
    renderer.method_526(pipe_bond,
      from,
      Vector2.Zero,
      OFFSET,
      angle);
  }
  public static void DrawBase(Part part,
      Vector2 pos,
      SolutionEditorBase seb,
      class_195 renderer,
      HexIndex hexPos) {
    //renderer.method_530(base_empty_fuzz, hexPos, 0f);
    renderer.method_530(pipe_base, hexPos, 0f);
    renderer.method_528(pipe_ring, hexPos, Vector2.Zero);
    var rotation = PartRotation(seb, part);
    var rotationF = rotation.ToRadians();

  }
  public static void DrawMolAsIfInput(Molecule rawM,
    SolutionEditorBase seb,
    PartSimState pss,
    Vector2 rendererPos,
    Part part,
    bool? maybeSimStarted = null,
    Molecule? maybeAnimateMolecule = null) {
    var state = pss.GetDefaultDynState();
    Molecule? animateMolecule = maybeAnimateMolecule ?? state.animatingMolecule;
    bool simStarted = maybeSimStarted ?? state.simStarted;
    if (!simStarted) { DrawMol(rawM, pss, rendererPos, part); }
    if (animateMolecule is not null) {
      DrawMol(rawM, pss, rendererPos, part, fractionOnBoard: seb.AnimTime());
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
      DrawMol(rawM, pss, rendererPos, part, fractionOnBoard: class_162.method_416(seb.AnimTime(), 0f, 1f, 1f, 0f), alpha: alpha);
      //DrawMol(rawM, pss, rendererPos, part, fractionOnBoard: class_162.method_416(seb.AnimTime(), 0f, 1f, 1f, 0f), alpha: 1f);
      //DrawMol(rawM, pss, rendererPos, part, alpha: 0.05f, fractionOnBoard: 1f, light:true );
    }
    if (seb is SolutionEditorScreen ses && state.simStarted) {
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
      Func<Molecule, Molecule, bool>? molecMatchesFn = null) {
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
  public const int MAX_SPAWNERS = 8;
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