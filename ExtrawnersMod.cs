
using Quintessential;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using Quintessential.Serialization;

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
using static ExtrawnersExt;

//dotnet build;rm Extrawners.dll;cp bin/Debug/net4.5.2/Extrawners.dll ./
public sealed partial class ExtrawnersMod : QuintessentialMod {
  public static Dictionary<string, GlyphData> puzzleGlyphData = new();
  public static AtomType[] VanillaAtomTypes {
    get => new AtomType[]{
    VanillaAtoms.air,
    VanillaAtoms.copper,
    VanillaAtoms.earth,
    VanillaAtoms.fire,
    VanillaAtoms.gold,
    VanillaAtoms.iron,
    VanillaAtoms.lead,
    VanillaAtoms.mors,
    VanillaAtoms.quicksilver,
    VanillaAtoms.quintessence,
    VanillaAtoms.salt,
    VanillaAtoms.silver,
    VanillaAtoms.tin,
    VanillaAtoms.vitae,
    VanillaAtoms.water,
  };
  }

  public override void LoadPuzzleContent() {
    SpawnerGlyph.LoadPuzzleContent();
    //GlyphData.LoadPuzzleContent();
  }

  public override void Load() {

  }

  public override void PostLoad() {
    puzzleinfoscreen_method_1275 = new Hook(
     typeof(PuzzleInfoScreen).GetMethod("method_1275", BF.NonPublic | BF.Instance),
     OnSolLoad
    );
    hook_GameLogic_method_946 = new Hook(
      typeof(GameLogic).GetMethod("method_946", BF.Public | BF.Instance),
      OnGameLogicMethod945
    );
    hookApplyChanges = new Hook(
      typeof(Solution).GetMethod("ApplyChanges", BF.Public | BF.Static),
      HookApplyChanges
    );
    {
      var m0 = new Molecule()
      .Atom("extransmutations:ichor", 0, 0)
      .Atom("extransmutations:ichor", 1, 0)
      .Atom("extransmutations:ichor", 2, 0)
      .Bond(BondKinds.normal, 1, 0, 2, 0);
      var m1 = new Molecule()
      .Atom("salt", 0, 0)
      .Atom("salt", 1, 0)
      .Bond(BondKinds.triplex_ogr, 0, 0, 1, 0);
      puzzleGlyphData.Add("c248888215006990", new() {
        origins = new() { new(0, 0), new(3, 3) },
        partTypeModify = (index, partTypes) => {
          partTypes[0].field_1540 = m0.method_1100().Select(a => a.Key).ToArray();
          partTypes[1].field_1540 = m1.method_1100().Select(a => a.Key).ToArray();
        },
        partRenderer = (glyphIndex, part, pos, seb, renderer) => {
          if (glyphIndex == 0) {
            SpawnerGlyph.DrawHexes(part, pos, seb, renderer, m0.method_1100().Select(a => a.Key));
            //SpawnerGlyph.DrawMol(m0, PSS(seb, part), pos, part);
          }
          else if (glyphIndex == 1) {
            SpawnerGlyph.DrawHexes(part, pos, seb, renderer, m1.method_1100().Select(a => a.Key));
            //SpawnerGlyph.DrawMol(m1, PSS(seb, part), pos, part);
          }
        },
      });
    }
  }

  public override void Unload() {
    puzzleinfoscreen_method_1275.Dispose();
    puzzleinfoscreen_method_1275 = null;
    hook_GameLogic_method_946.Dispose();
    hook_GameLogic_method_946 = null;
    hookApplyChanges.Dispose();
    hookApplyChanges = null;
  }

  public Hook hookApplyChanges = null;
  internal static void HookApplyChanges(
    Action<Puzzle, Solution> orig,
    Puzzle puzzle,
    Solution solution) {

    orig(puzzle, solution);

    var puzzleId = puzzle.field_2766;
    foreach (var glyphData in puzzleGlyphData.Where(a => a.Key == puzzleId).Select(a => a.Value)) {
      if (glyphData.origins.Count > SpawnerGlyph.MAX_SPAWNERS) {
        throw new ArgumentOutOfRangeException($"Only {SpawnerGlyph.MAX_SPAWNERS} max spawner glyphs are allowed at a time. Bug me (Iris) to increase this if you need more.");
      }
      SpawnerGlyph.Cleanup();
      SpawnerGlyph.glyphRenderer = glyphData.partRenderer;
      for (int i = 0; i < glyphData.origins.Count; i++) {
        var origin = glyphData.origins[i];
        glyphData.partTypeModify(glyphIndex: i, SpawnerGlyph.partTypes);

        HexIndex position = origin;
        HexRotation rotation = new();

        Part part = new(SpawnerGlyph.partTypes[i], false);
        solution.method_1939(part, position);
        part.method_1197(solution, rotation);
      }
    }
  }

  public Hook puzzleinfoscreen_method_1275;
  internal static void OnSolLoad(
      Action<PuzzleInfoScreen, Solution> orig,
      PuzzleInfoScreen self,
      Solution solution) {
    Puzzle puzzle = solution.method_1934();
    var puzzleId = puzzle.field_2766;

    foreach (var glyphData in puzzleGlyphData.Where(a => a.Key == puzzleId).Select(a => a.Value)) {
      if (glyphData.origins.Count > SpawnerGlyph.MAX_SPAWNERS) {
        throw new ArgumentOutOfRangeException($"Only {SpawnerGlyph.MAX_SPAWNERS} max spawner glyphs are allowed at a time. Bug me (Iris) to increase this if you need more.");
      }
      SpawnerGlyph.Cleanup();
      SpawnerGlyph.glyphRenderer = glyphData.partRenderer;
      for (int i = 0; i < glyphData.origins.Count; i++) {
        var origin = glyphData.origins[i];
        glyphData.partTypeModify(glyphIndex: i, SpawnerGlyph.partTypes);
      }
    }
    //Puzzle puzzle = solution.method_1934();
    //var perms = puzzle.CustomPermissions ?? new();

    //var partList = solution.method_1941();
    //if(partList.All(p => p.method_1159().field_1528 != SpawnerGlyph.PART_ID)) {}
    orig(self, solution);
  }
  public Hook hook_GameLogic_method_946;
  internal delegate void origOnGameLogicMethod945(GameLogic self, IScreen param_4617);
  internal static void OnGameLogicMethod945(
      origOnGameLogicMethod945 orig,
      GameLogic self,
      IScreen param_4617) {
    if (param_4617 is PuzzleInfoScreen) {// presumably switching puzzles, let's clean up
      SpawnerGlyph.Cleanup();
    }
    orig(self, param_4617);
  }

  public static void DebugLog(string s) => Logger.Log($"[extrawners-debug] {s}");
  public static void Log(string s) => Logger.Log($"[extrawners] {s}");
}