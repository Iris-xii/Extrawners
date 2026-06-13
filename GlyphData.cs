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
using System.Runtime.CompilerServices;
using Quintessential;

#nullable enable
public sealed record class GlyphData {
  /// <summary>
  /// List of origins for every glyph. An entry here implies the existence of said glyph. (Otherwise
  /// it won't spawn)
  /// </summary>
  public List<HexIndex> origins = new(); 
  public delegate void PartTypeModify(PartType[] partTypes);
  public PartTypeModify partTypeModify = (_t) => {};
  public delegate void RenderFn(int glyphIndex,
      Part part,
      Vector2 pos,
      SolutionEditorBase seb,
      class_195 renderer);
  public RenderFn partRenderer = (_,_,_,_,_) => {};
  public delegate void PreCycleFn(Sim sim);
  public PreCycleFn glyphPreCycle = (_) => {};
  public delegate void AfterCycle(Sim sim,bool firstHalf);
  public AfterCycle glyphAfterCycle = (_,_) => {};
  public delegate void WellAfterCycleFn(Sim sim);
  public WellAfterCycleFn glyphWellAfterCycle = (_) => {};

  /*
  private Molecule LastMol() {
    if (molecules.Count == 0) {
      var m = new Molecule();
      molecules.Add(m);
      return m;
    }
    else {
      return molecules[molecules.Count - 1];
    }
  }
  private void DoInstruction(string instrFull) {
    if (instrFull.Length == 0) { return; }
    string[] split = instrFull.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
    if (split.Length == 0) { return; }
    string instr = split[0];
    if (instr == "origin" && split.Length >= 3) {
      bool parseOk = int.TryParse(split[1], out int a);
      parseOk = int.TryParse(split[2], out int b) && parseOk;
      if (parseOk) {
        this.origin = new(a, b);
        return;
      }
    }
    else if (instr == "atom" && split.Length >= 4) {
      var atomType = MaybeAtomTypeByName(split[1]);
      bool parseOk = int.TryParse(split[2], out int a);
      parseOk = int.TryParse(split[3], out int b) && parseOk;
      if (atomType is null) {
        Log($"AtomType {split[1]} not found.");
      }
      if (parseOk && atomType is not null) {
        var mol = LastMol();
        mol.method_1105(new(atomType), new HexIndex(a, b));
        if (!hexes.Contains(new HexIndex(a, b))) {
          hexes.Add(new HexIndex(a, b));
        }
        return;
      }
    }
    else if (instr == "bond" && split.Length >= 6) {
      string bondTypeS = split[1];
      bool parseOk = int.TryParse(split[2], out int a);
      parseOk = int.TryParse(split[3], out int b) && parseOk;
      parseOk = int.TryParse(split[4], out int c) && parseOk;
      parseOk = int.TryParse(split[5], out int d) && parseOk;
      enum_126 bondKind = enum_126.None;
      if (int.TryParse(bondTypeS, out int bondNumber)) {
        bondKind = (enum_126)bondNumber;
      }
      else {
        if (bondTypeS == "normal") { bondKind = enum_126.Standard; }
        else if (bondTypeS == "triplex_ogr") { bondKind = enum_126.Prisma0 | enum_126.Prisma1 | enum_126.Prisma2; }
        else if (bondTypeS == "triplex_o") { bondKind = enum_126.Prisma0; }
        else if (bondTypeS == "triplex_g") { bondKind = enum_126.Prisma1; }
        else if (bondTypeS == "triplex_r") { bondKind = enum_126.Prisma2; }
        else if (bondTypeS == "triplex_og") { bondKind = enum_126.Prisma0 | enum_126.Prisma1; }
        else if (bondTypeS == "triplex_or") { bondKind = enum_126.Prisma0 | enum_126.Prisma2; }
        else if (bondTypeS == "triplex_gr") { bondKind = enum_126.Prisma1 | enum_126.Prisma2; }
        else { Log($"Invalid bond type {bondTypeS}"); parseOk = false; }
      }
      if (parseOk) {
        var mol = LastMol();
        mol.method_1112(bondKind, new(a, b), new(c, d), new());
        return;
      }
    }
    else if (instr.StartsWith("~~")) { return; } // byproduct of OMMMB export, maybe. Ignore
    Log($"Instruction {instrFull} may be incorrect. [instr:{instr}, len:{split.Length}]");
    throw new ArgumentException($"Instruction {instrFull} may be incorrect.");
  } 

  internal const string PREFIX = "extrawners:glyph:";
  private static bool FromPermissionMaybe(string perm, out GlyphData? glyphData) {
    if (!perm.StartsWith(PREFIX)) {
      glyphData = null;
      return false;
    }
    perm = perm.Substring(PREFIX.Length);
    //Log($"Reading extrawners:glyph: perm: {perm}");
    glyphData = FromString(perm);
    return true;
  } 
  public static GlyphData FromString(string s) {
    s = new(s.Where(c => !(char.IsWhiteSpace(c) && c != ' ')).ToArray());
    string[] instructions = s.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
    GlyphData glyphData = new();
    foreach (var instr in instructions) {
      glyphData.DoInstruction(instr);
    }
    return glyphData;
  } 

  internal static void LoadPuzzleContent() {
    Log("Adding PayloadHandler");
    QApi.AddSolutionPayloadHandler("extrawners:glyph", (solution, data) => {
      Log($"Reading & applying: extrawners:glyph");
      var glyphData = FromString(data);
      int ptypeIndex = SpawnerGlyph.ApplyData(solution,glyphData);

      HexIndex position = glyphData.origin;
      HexRotation rotation = new();


      Part part = new(SpawnerGlyph.partTypes[ptypeIndex], false);
      solution.method_1939(part, position);
      part.method_1197(solution, rotation);
    });
  }
  */
}