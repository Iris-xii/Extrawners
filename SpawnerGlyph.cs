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
  public static Texture genericBase = Brimstone.API.GetTexture();

  public static PartType[] partTypes = new PartType[0];
  public static GlyphData.Renderer glyphRenderer = (_, _, _, _, _) => { };

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


  public static void DrawHexes<H>(Part part,
      Vector2 pos,
      SolutionEditorBase editor,
      class_195 renderer,
      H hexes)
      where H : IEnumerable<HexIndex> {
    foreach (var hex in hexes) {
      DrawHexBase(part, pos, editor, renderer, hexes, hex);
    }
    foreach (var hex in hexes) {
      DrawHexBonds(part, pos, editor, renderer, hexes, hex);
    }
  }

  public static void DrawHexBonds<H>(Part part,
      Vector2 pos,
      SolutionEditorBase seb,
      class_195 renderer,
      H allHexes,
      HexIndex hexPos)
      where H : IEnumerable<HexIndex> {
    float angleFwd = class_187.field_1742.method_492((hexPos + new HexIndex(1, 0)) - hexPos).Angle();
    float angleBack = class_187.field_1742.method_492((hexPos + new HexIndex(-1, 0)) - hexPos).Angle();
    if (allHexes.Contains(hexPos + new HexIndex(1, 0))) {
      renderer.method_526(pipe_bond,
        hexPos,
        Vector2.Zero,
        new Vector2(-23f, 20f),
        angleFwd);
    }
    if (allHexes.Contains(hexPos + new HexIndex(-1, 0))) {
      //renderer.method_526(pipe_bond,
      //  hexPos,
      //  Vector2.Zero,
      //  new Vector2(-23f, 20f),
      //  angleBack);
    }
  }
  public static void DrawHexBase<H>(Part part,
      Vector2 pos,
      SolutionEditorBase seb,
      class_195 renderer,
      H allHexes,
      HexIndex hexPos)
      where H : IEnumerable<HexIndex> {
    //renderer.method_530(base_empty_fuzz, hexPos, 0f);
    renderer.method_530(pipe_base, hexPos, 0f);
    renderer.method_528(pipe_ring, hexPos, Vector2.Zero);
    var rotation = PartRotation(seb, part);
    var rotationF = rotation.ToRadians();

  }
  public static void DrawMol(Molecule m,
      PartSimState pss,
      Vector2 rendererPos,
      Part part,
      float rotation = 0f,
      float alpha = 1f,
      float fractionOnBoard = 1f,
      float shadowStrength = 1f) {
    Editor.method_925(m.method_1115(PartRotation(pss)),
      rendererPos,
      -part.method_1161(),
      rotation /*rotation*/,
      alpha /*alpha*/,
      fractionOnBoard /* 0 = gone*/,
      shadowStrength /*shadow str*/, false /*light*/, null);
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
  private static class_236 method_1989(SolutionEditorBase seb, Part param_5518, Vector2 param_5543) {
    return seb.method_1990(param_5518, param_5543, seb.method_504());
  }
  private static Vector2 method_1999(class_236 param_5571, HexIndex param_5572) {
    return param_5571.field_1984 + class_187.field_1742.method_492(param_5572).Rotated(param_5571.field_1985);
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
    partTypes = partTypesList.ToArray();
  }

  internal static void Cleanup() {
    for (int i = 0; i < MAX_SPAWNERS; i++) {
      SpawnerPartTypeReset(i, partTypes[i]);
    }
  }

}