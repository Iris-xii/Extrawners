
using Quintessential;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using Quintessential.Serialization;
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

using static ExtrawnersMod;

#nullable enable
internal static class ExtrawnersExt {
  internal static class BondKinds {
    internal const enum_126 normal = enum_126.Standard;
    internal const enum_126 triplex_ogr = enum_126.Prisma0 | enum_126.Prisma1 | enum_126.Prisma2;
    internal const enum_126 triplex_o = enum_126.Prisma0;
    internal const enum_126 triplex_g = enum_126.Prisma1;
    internal const enum_126 triplex_r = enum_126.Prisma2;
    internal const enum_126 triplex_og = enum_126.Prisma0 | enum_126.Prisma1;
    internal const enum_126 triplex_or = enum_126.Prisma0 | enum_126.Prisma2;
    internal const enum_126 triplex_gr = enum_126.Prisma1 | enum_126.Prisma2;
  }

  /// <exception cref="ArgumentException"></exception>
  internal static Molecule Atom(this Molecule m, string atomName, HexIndex pos) {
    var atomType = MaybeAtomTypeByName(atomName) ?? throw new ArgumentException($"Atom name \"{atomName}\" wasn't found.");
    m.method_1105(new(atomType), pos);
    return m;
  }
  /// <exception cref="ArgumentException"></exception>
  internal static Molecule Atom(this Molecule m, string atomName, int a, int b) => m.Atom(atomName, new(a, b));
  internal static Molecule Atom(this Molecule m, AtomType atomType, int a, int b) {
    m.method_1105(new(atomType), new(a, b));
    return m;
  }

  internal static Molecule Bond(this Molecule m, enum_126 kind, HexIndex a, HexIndex b) {
    m.method_1112(kind, a, b, new());
    return m;
  }
  internal static Molecule Bond(this Molecule m, enum_126 kind, int a, int b, int c, int d) => m.Bond(kind, new(a, b), new(c, d));

  internal static PartSimState PSS(SolutionEditorBase seb, Part part) => seb.method_507().method_481(part);
  internal static HexRotation PartRotation(PartSimState pss) => pss.field_2726;
  internal static HexRotation PartRotation(SolutionEditorBase seb, Part part) => seb.method_507().method_481(part).field_2726;
  internal static Solution Solution(this SolutionEditorBase seb) => seb.method_502();
  internal static Puzzle Puzzle(this Solution sol) => sol.method_1934();
  internal static string PuzzleId(this Puzzle puzzle) => puzzle.field_2766;
  internal static PartType Type(this Part part) => part.method_1159();
  internal static float AnimTime(this SolutionEditorBase seb) => seb.method_504();
  internal static float AccumulatedTime(this SolutionEditorBase seb) => seb.method_509();
  internal static enum_128 IsRunning(this SolutionEditorBase seb) => seb.method_503();

  internal static SolutionEditorBase SEB(this Sim sim) => sim.field_3818;
  internal static void AddMolecule(this Sim sim, Molecule m) => sim.field_3823.Add(m);
  internal static bool RemoveMolecule(this Sim sim, Molecule m) => sim.field_3823.Remove(m);
  internal static Molecule ShiftedToGlobal(this Molecule m, Part part) => m.ShiftedBy(part.method_1161(), part.method_1163());
  internal static Molecule ShiftedBy(this Molecule m, HexIndex shift, HexRotation rot) => m.method_1115(rot).method_1117(shift);
  internal static Molecule SimCoordsToPart(this Molecule m, Part part) => m.method_1117(-part.method_1161()).method_1115(part.method_1163().Negative());
  internal static List<Part> PartList(this Solution solution) => solution.field_3919;
  internal static List<Part> PartList(this Sim sim) => sim.field_3818.method_502().field_3919;
  internal static int Cycle(this Sim sim) => sim.method_1818();

  internal static void SetHexesToMol(this PartType t, Molecule m) =>
    t.field_1540 = m.method_1100().Select(a => a.Key).ToArray();
  internal static void SetHexesToAllMols(this PartType t, IEnumerable<Molecule> mls) =>
    t.field_1540 = mls.SelectMany(m => m.method_1100().Keys).Distinct().ToArray();
  internal static void SetName(this PartType t, string name) =>
    t.field_1529 = class_134.method_253(name, string.Empty);
  internal static void SetDescription(this PartType t, string desc) =>
    t.field_1530 = class_134.method_253(desc, string.Empty);

  internal static AtomType AsQuintAtomType(this string s) { 
      AtomType atomType;
      try {
        atomType = ExtrawnersMod.VanillaAtomTypes.Concat(QApi.ModAtomTypes)
       .Where(a => a.QuintAtomType == s)
       .First();
      }
      catch (Exception) { atomType = VanillaAtoms.salt; }
      return atomType;
  }
 
  internal static void DrawMol(Molecule rawM,
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
  [Obsolete]
  internal static void SetAsOutput(this PartType t) => t.SetDynState<bool>("output", true);


  internal static void SetRequiredOutputs(this Part part, int required) => part_method_1170(part, required);
  internal static int GetRequiredOutputs(this Part part) => part.method_1169();
  internal static void AddToCurrentOutputs(this PartSimState pss, int add, int limit) {
    if ((pss.field_2730 + 1) <= limit) { pss.field_2730 += add; }
  }
  internal static void SetCurrentOutputs(this PartSimState pss, int current) => pss.field_2730 = current;
  internal static int CurrentOutputs(this PartSimState pss) => pss.field_2730;

  internal static Solution m1817(this Sim sim) => sim.field_3818.method_502();
  internal static Action<Part, int> part_method_1170 =
    typeof(Part).GetMethod("method_1170", BF.NonPublic | BF.Instance).CreateDelegate<Action<Part, int>>();
  internal static Func<Sim, Molecule, HashSet<HexIndex>, bool> m1837 =
    typeof(Sim).GetMethod("method_1837", BF.NonPublic | BF.Instance).CreateDelegate<Func<Sim, Molecule, HashSet<HexIndex>, bool>>();
  internal static Func<Sim.class_403, HexIndex, bool> c_method_1860 =
    typeof(Sim.class_403).GetMethod("method_1860", BF.NonPublic | BF.Instance).CreateDelegate<Func<Sim.class_403, HexIndex, bool>>();

  internal static Func<Molecule, Molecule, bool> molecMatchesExact =
      typeof(Sim).GetMethod("method_1844", BF.NonPublic | BF.Static).CreateDelegate<Func<Molecule, Molecule, bool>>();
  internal static bool MolecMatchesSinkAny(Molecule simMolecShifted, Molecule templateShifted,Sim sim) {
    // Serializing them and de-serializing them is a bit jank but
    // I didn't feel like writing a clone function by hand and otherwise
    // there is weird action at a distance from the molecules referencing
    // the same objects 
    Molecule simTemplateMutated = new PuzzleModel.MoleculeM(templateShifted).FromModel();
    Molecule simMutated = new PuzzleModel.MoleculeM(simMolecShifted).FromModel();
    List<HexIndex> inBaseButNotGrabbed = new();
    foreach (var kv in simTemplateMutated.method_1100()) { // BASE
      kv.Value.field_2275 = VanillaAtoms.salt;
      inBaseButNotGrabbed.Add(kv.Key);
    }
    foreach (var kv in simMutated.method_1100()) { // INPUT ATTEMPT
      kv.Value.field_2275 = VanillaAtoms.salt;
      inBaseButNotGrabbed.Remove(kv.Key);
    }
    foreach (var hi in inBaseButNotGrabbed) {
      simTemplateMutated.method_1107(hi);
    }
    // bond removal method_1114 
    List<Pair<HexIndex, HexIndex>> removeTemplate = new();
    foreach (var bnd in simTemplateMutated.method_1101()) {
      removeTemplate.Add(new(bnd.field_2187, bnd.field_2188));
    } 
    foreach (var bnd in removeTemplate) {
      Brimstone.API.RemoveBonds(sim,simTemplateMutated,bnd.Left,bnd.Right,false,false); 
    }
    List<Pair<HexIndex, HexIndex>> removeSim = new();
    foreach (var bnd in simMutated.method_1101()) {
      removeSim.Add(new(bnd.field_2187, bnd.field_2188));
    }
    foreach (var bnd in removeSim) {
      Brimstone.API.RemoveBonds(sim,simMutated,bnd.Left,bnd.Right,false,false); 
    }  
    return molecMatchesExact(simTemplateMutated, simMutated);
    //TODO: bond adjusting and improve this, it's just yoinked from Extransmissions.
  }

  internal static bool MoleculeHeld(this Sim sim, Molecule molec) {
    return sim.HeldGrippers.Any((gripper) => {
      var pss = PSS(sim.SEB(), gripper);
      var maybeHolding = pss.field_2729;
      return maybeHolding.method_1085() && (maybeHolding.method_1087() == molec);
    });
  }
  internal static bool DoesNotOverlap(Sim sim, Part? unused, Molecule shifted,
  HashSet<HexIndex>? alreadyOccupiedAbsolute = null) {
    HashSet<HexIndex> hashSet = alreadyOccupiedAbsolute ?? new();
    foreach (Molecule item in sim.field_3823) {
      hashSet.UnionWith(item.method_1100().Keys);
    }
    //HexIndex param_ = item2.method_1161();
    //HexRotation param_2 = item2.method_1163();
    //Molecule molecule = item2.method_1185(sim.m1817()).method_1115(param_2).method_1117(param_);
    if (!m1837(sim, shifted, hashSet)) {
      return true;
    }
    return false;
  }

  internal static void method_1854_crash(this Sim s, string param_5403, HexIndex param_5404, HexIndex param_5405) {
    Vector2 vector = class_187.field_1742.method_492(param_5404);
    Vector2 vector2 = class_187.field_1742.method_492(param_5405);
    s.field_3818.method_518(0f, param_5403, new Vector2[2] { vector, vector2 });
  }


  internal static T GetDynStateOrDef<T>(this PartSimState pss, string entry) where T : new() {
    DynamicData dyn_pss = new(pss);
    object? maybeState = dyn_pss.Get(entry);
    T state;
    if (maybeState is not null) {
      state = (T)maybeState;
    }
    else {
      state = new();
      dyn_pss.Set(entry, state);
    }
    return state;
  }
  [Obsolete("DynState with PartType is too easy to get wrong")]
  internal static T GetDynStateOrDef<T>(this PartType pt, string entry) where T : new() {
    DynamicData dyn_pss = new(pt);
    object? maybeState = dyn_pss.Get(entry);
    T state;
    if (maybeState is not null) {
      state = (T)maybeState;
    }
    else {
      state = new();
      dyn_pss.Set(entry, state);
    }
    return state;
  }
  [Obsolete("DynState with PartType is too easy to get wrong")]
  internal static T? GetDynStateOrNull<T>(this PartType pt, string entry) where T : class? {
    DynamicData dyn_pss = new(pt);
    object? maybeState = dyn_pss.Get(entry);
    T state;
    if (maybeState is not null) {
      state = (T)maybeState;
    }
    else {
      return null;
    }
    return state;
  }
  internal static T? GetDynStateOrNull<T>(this PartSimState pss, string entry) where T : class? {
    DynamicData dyn_pss = new(pss);
    object? maybeState = dyn_pss.Get(entry);
    T state;
    if (maybeState is not null) {
      state = (T)maybeState;
    }
    else {
      return null;
    }
    return state;
  }
  internal static void SetDynState<T>(this PartSimState pss, string entry, T to) {
    DynamicData dyn_pss = new(pss);
    dyn_pss.Set(entry, to);
  }
  [Obsolete("DynState with PartType is too easy to get wrong")]
  internal static void SetDynState<T>(this PartType pt, string entry, T to) {
    DynamicData dyn_pss = new(pt);
    dyn_pss.Set(entry, to);
  }
  internal static ExtrawnersDynState GetDefaultDynState(this PartSimState pss) => pss.GetDynStateOrDef<ExtrawnersDynState>("defaultState");

  /// <summary> A handful of things utilize a few 'dynamic' states by default if nothing else
  /// is specified. <br></br><br></br>
  /// Call this on every Extrawners part that utilizes these to reset them. </summary>
  internal static void AutoStatesReset(Sim sim, Part part, bool isOutput) {
    var pss = PSS(sim.SEB(), part);
    if (sim.Cycle() == 0) {
      pss.SetDynState("defaultState", new ExtrawnersDynState() {
        simStarted = true,
        animatingMolecule = null,
        isOutput = isOutput,
      });
    }
    else {
      var state = pss.GetDefaultDynState();
      state.animatingMolecule = null;
    }
  }
  internal record class ExtrawnersDynState {
    internal bool simStarted = false;
    internal Molecule? animatingMolecule = null;
    internal bool isOutput = false;
  }

  internal static void Play(this Sound sound, SolutionEditorBase seb) {
    sound.field_4062 = false;
    sound.method_28(seb.method_506());
  }

  // class_187: Hex -> Vector tools?

  internal static AtomType? MaybeAtomTypeByName(string name) {
    var all_atoms = QApi.ModAtomTypes.ToList();
    all_atoms.AddRange(VanillaAtomTypes);
    return all_atoms.Where(a => a.QuintAtomType.ToLowerInvariant() == name.ToLowerInvariant()).FirstOrDefault();
  }

  internal static void HexesAndBondsOut(IEnumerable<Molecule> molecules,
      out HashSet<HexIndex> hexes,
      out HashSet<Pair<HexIndex, HexIndex>> sortaBonds) {
    hexes = new();
    sortaBonds = new();
    foreach (var mol in molecules) {
      hexes.UnionWith(mol.method_1100().Select(a => a.Key));
      sortaBonds.UnionWith(mol.method_1101().Select(a => new Pair<HexIndex, HexIndex>(a.field_2187, a.field_2188)));
    }
  }
  internal static void HexesAndBondsRef(IEnumerable<Molecule> molecules,
    ref HashSet<HexIndex> hexes,
    ref HashSet<Pair<HexIndex, HexIndex>> sortaBonds) {
    foreach (var mol in molecules) {
      hexes.UnionWith(mol.method_1100().Select(a => a.Key));
      sortaBonds.UnionWith(mol.method_1101().Select(a => new Pair<HexIndex, HexIndex>(a.field_2187, a.field_2188)));
    }
  }
  
  internal static string? TryGetPuzzleFile(string nameWithExt) {
    var customPath = Path.Combine(class_269.field_2102, "custom");
    string targetFile = nameWithExt;
    string? foundFilePathFull = null;
    foreach (var filepath in Directory.EnumerateFiles(customPath)) {
      var filename = Path.GetFileName(filepath);
      if (filename == targetFile) {
        foundFilePathFull = filepath;
        break;
      }
    }
    foreach (var puzzleDir in QuintessentialLoader.ModPuzzleDirectories) {
      foreach (var filepath in Directory.EnumerateFiles(puzzleDir)) {
        var filename = Path.GetFileName(filepath);
        if (filename == targetFile) {
          foundFilePathFull = filepath;
          break;
        }
      }
    }
    return foundFilePathFull;
  }
  
  internal struct FuckingComparer : IEqualityComparer<Molecule> {//I can't get Distinct to just take a lambda >:(
    public readonly bool Equals(Molecule x, Molecule y) => molecMatchesExact(x, y);
    public readonly int GetHashCode(Molecule obj) => obj.GetHashCode();
  }
}