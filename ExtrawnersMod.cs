
using Quintessential;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using Quintessential.Serialization;
using Quintessential.Settings;
using MonoMod.Utils;

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


#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
//dotnet build;rm Extrawners.dll;cp bin/Debug/net4.5.2/Extrawners.dll ./
public sealed partial class ExtrawnersMod : QuintessentialMod {


  [SettingsLabel("Print Molecules to log on level load?")]
  internal static bool printMoleculesOnLoad = true;

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
    hook_sim_method_1825 = new Hook(
      typeof(Sim).GetMethod("method_1825", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public),
      OnSimMethod1825);
    hook_Sim_method_1828 = new Hook(typeof(Sim).GetMethod("method_1828", BF.NonPublic | BF.Instance), OnSimMethod1828_SpawnScaffolds);
    hook_Sim_method_1836 = new Hook(typeof(Sim).GetMethod("method_1836", BF.NonPublic | BF.Instance), OnSimMethod_1836_WellAfterCycle);

    hook_method_947 = new Hook(typeof(GameLogic).GetMethod("method_947", BF.Public | BF.Instance), OnScreenTransitionAway);
    DoExamplePuzzles();
  }

  public override void Unload() {
    puzzleinfoscreen_method_1275.Dispose();
    puzzleinfoscreen_method_1275 = null;
    hook_GameLogic_method_946.Dispose();
    hook_GameLogic_method_946 = null;
    hookApplyChanges.Dispose();
    hookApplyChanges = null;
    hook_sim_method_1825.Dispose();
    hook_sim_method_1825 = null;
    hook_method_947.Dispose();
    hook_method_947 = null;
  }

  public Hook hookApplyChanges = null;
  internal static void HookApplyChanges(
    Action<Puzzle, Solution> orig,
    Puzzle puzzle,
    Solution solution) {

    orig(puzzle, solution);

    var puzzleId = puzzle.field_2766;
    GlyphData? maybeGlyphData = puzzleGlyphData
      .Where(a => a.Key == puzzleId)
      .Select(a => a.Value)
      .FirstOrDefault();
    maybeGlyphData ??= Presets.LoadPresets(puzzle, solution,actualSolLoad: false);
    if (maybeGlyphData is GlyphData glyphData) {
      if (glyphData.origins.Count > SpawnerGlyph.MAX_SPAWNERS) {
        throw new ArgumentOutOfRangeException($"Only {SpawnerGlyph.MAX_SPAWNERS} max spawner glyphs are allowed at a time. Bug me (Iris) to increase this if you need more.");
      }
      GlyphDataSetupShared(glyphData);
      for (int i = 0; i < glyphData.origins.Count; i++) {
        var origin = glyphData.origins[i];
        glyphData.partTypeModify(SpawnerGlyph.partTypes, solution);

        HexIndex position = origin;
        HexRotation rotation = new();

        Part part = new(SpawnerGlyph.partTypes[i], false);
        solution.method_1939(part, position);
        part.method_1197(solution, rotation);
      }
    }
  }

  internal static void GlyphDataSetupShared(GlyphData glyphData) {
    SpawnerGlyph.Cleanup();
    SpawnerGlyph.glyphRenderer = glyphData.partRenderer;
    SpawnerGlyph.logicFn = glyphData.logicFn;
  }

  public Hook puzzleinfoscreen_method_1275;
  internal static void OnSolLoad(
      Action<PuzzleInfoScreen, Solution> orig,
      PuzzleInfoScreen self,
      Solution solution) {
    Puzzle puzzle = solution.method_1934();
    var puzzleId = puzzle.field_2766;
    if (printMoleculesOnLoad) { PrintMoleculesOnLoad(puzzle); }
    resetPuzzleIODeleteHack = () => { };

    GlyphData? maybeGlyphData = puzzleGlyphData
      .Where(a => a.Key == puzzleId)
      .Select(a => a.Value)
      .FirstOrDefault();
    maybeGlyphData ??= Presets.LoadPresets(puzzle, solution, actualSolLoad: true);
    if (maybeGlyphData is GlyphData glyphData) {
      if (glyphData.origins.Count > SpawnerGlyph.MAX_SPAWNERS) {
        throw new ArgumentOutOfRangeException($"Only {SpawnerGlyph.MAX_SPAWNERS} max spawner glyphs are allowed at a time. Bug me (Iris) to increase this if you need more.");
      }
      GlyphDataSetupShared(glyphData);
      glyphData.partTypeModify(SpawnerGlyph.partTypes, solution);
    }
    //Puzzle puzzle = solution.method_1934();
    //var perms = puzzle.CustomPermissions ?? new();

    //var partList = solution.method_1941();
    //if(partList.All(p => p.method_1159().field_1528 != SpawnerGlyph.PART_ID)) {} 
    orig(self, solution);

  }

  public Hook hook_method_947;
  private static void OnScreenTransitionAway(Action<GameLogic, Maybe<class_124>, Maybe<class_124>> orig,
    GameLogic gl, Maybe<class_124> param_4618, Maybe<class_124> param_4619) {
    resetPuzzleIODeleteHack();
    orig(gl, param_4618, param_4619);
  }


  private static void PrintMoleculesOnLoad(Puzzle puzzle) {
    static string Dump(Molecule m) {
      var stringEnumerator = m.method_1100()
      .Select(a => $".Atom(\"{a.Value.field_2275.QuintAtomType}\",{a.Key.Q},{a.Key.R})")
      .Concat(
        m.method_1101().Select(a =>
        $".Bond((enum_126){(int)a.field_2186},{a.field_2187.Q},{a.field_2187.R},{a.field_2188.Q},{a.field_2188.R})")
      );
      return String.Join(String.Empty, stringEnumerator);
    }
    PuzzleInputOutput[] pInput = puzzle.field_2770;
    PuzzleInputOutput[] pOutput = puzzle.field_2771;
    for (int i = 0; i < pInput.Length; i++) {
      Log($"input@{puzzle.PuzzleId()} #{i}:\n" +
      $"var input{i} = new Molecule(){Dump(pInput[i].field_2813)};");
    }
    for (int i = 0; i < pOutput.Length; i++) {
      Log($"output@{puzzle.PuzzleId()} #{i}:\n" +
      $"var output{i} = new Molecule(){Dump(pOutput[i].field_2813)};");
    }
  }

  internal static Action resetPuzzleIODeleteHack = () => { };
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

  public static Hook hook_Sim_method_1828;
  public delegate void orig_Sim_method_1828(Sim sim); //code that runs every cycle but before parts are processed
  private static void OnSimMethod1828_SpawnScaffolds(orig_Sim_method_1828 orig, Sim sim) {
    orig(sim);
    SpawnerGlyph.logicFn(sim, LogicWhen.PRE_CYCLE);
  }

  public static Hook hook_Sim_method_1836;
  public delegate void orig_Sim_method_1836(Sim sim); //code that runs every cycle but before parts are processed
  private static void OnSimMethod_1836_WellAfterCycle(orig_Sim_method_1836 orig, Sim sim) {
    orig(sim);
    SpawnerGlyph.logicFn(sim, LogicWhen.WELL_AFTER_CYCLE);
  }

  public Hook hook_sim_method_1825;
  private static bool OnSimMethod1825(On.Sim.orig_method_1825 orig, Sim s) {
    foreach (var part in s.PartList().Where(p => SpawnerGlyph.partTypes.Contains(p.Type()))) {
      var pss = PSS(s.SEB(), part);
      var state = pss.GetDefaultDynState();
      var partType = part.Type();
      if ((state.isOutput || partType.GetDynStateOrDef<bool>("output")) && (pss.CurrentOutputs() < part.GetRequiredOutputs())) {
        return false;
      }
    }
    return orig(s);
  }





  public static void DebugLog(string s) => Logger.Log($"[extrawners-debug] {s}");
  public static void Log(string s) => Logger.Log($"[extrawners] {s}");
}