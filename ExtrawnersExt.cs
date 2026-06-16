
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
public static class ExtrawnersExt {
  public static class BondKinds {
    public const enum_126 normal = enum_126.Standard;
    public const enum_126 triplex_ogr = enum_126.Prisma0 | enum_126.Prisma1 | enum_126.Prisma2;
    public const enum_126 triplex_o = enum_126.Prisma0;
    public const enum_126 triplex_g = enum_126.Prisma1;
    public const enum_126 triplex_r = enum_126.Prisma2;
    public const enum_126 triplex_og = enum_126.Prisma0 | enum_126.Prisma1;
    public const enum_126 triplex_or = enum_126.Prisma0 | enum_126.Prisma2;
    public const enum_126 triplex_gr = enum_126.Prisma1 | enum_126.Prisma2;
  }

  /// <exception cref="ArgumentException"></exception>
  public static Molecule Atom(this Molecule m, string atomName, HexIndex pos) {
    var atomType = MaybeAtomTypeByName(atomName) ?? throw new ArgumentException($"Atom name \"{atomName}\" wasn't found.");
    m.method_1105(new(atomType), pos);
    return m;
  }
  /// <exception cref="ArgumentException"></exception>
  public static Molecule Atom(this Molecule m, string atomName, int a, int b) => m.Atom(atomName, new(a, b));
  public static Molecule Atom(this Molecule m, AtomType atomType, int a, int b) {
    m.method_1105(new(atomType), new(a, b));
    return m;
  }

  public static Molecule Bond(this Molecule m, enum_126 kind, HexIndex a, HexIndex b) {
    m.method_1112(kind, a, b, new());
    return m;
  }
  public static Molecule Bond(this Molecule m, enum_126 kind, int a, int b, int c, int d) => m.Bond(kind, new(a, b), new(c, d));

  public static PartSimState PSS(SolutionEditorBase seb, Part part) => seb.method_507().method_481(part);
  public static HexRotation PartRotation(PartSimState pss) => pss.field_2726;
  public static HexRotation PartRotation(SolutionEditorBase seb, Part part) => seb.method_507().method_481(part).field_2726;
  public static Solution Solution(this SolutionEditorBase seb) => seb.method_502();
  public static Puzzle Puzzle(this Solution sol) => sol.method_1934();
  public static string PuzzleId(this Puzzle puzzle) => puzzle.field_2766;
  public static PartType Type(this Part part) => part.method_1159();
  public static float AnimTime(this SolutionEditorBase seb) => seb.method_504();
  public static float AccumulatedTime(this SolutionEditorBase seb) => seb.method_509();
  public static enum_128 IsRunning(this SolutionEditorBase seb) => seb.method_503();

  public static SolutionEditorBase SEB(this Sim sim) => sim.field_3818;
  public static void AddMolecule(this Sim sim, Molecule m) => sim.field_3823.Add(m);
  public static bool RemoveMolecule(this Sim sim, Molecule m) => sim.field_3823.Remove(m);
  public static Molecule ShiftedBy(this Molecule m, Part part) => m.ShiftedBy(part.method_1161(), part.method_1163());
  public static Molecule ShiftedBy(this Molecule m, HexIndex shift, HexRotation rot) => m.method_1115(rot).method_1117(shift);
  public static Molecule SimCoordsToPart(this Molecule m, Part part) => m.method_1117(-part.method_1161()).method_1115(part.method_1163().Negative());
  public static List<Part> PartList(this Solution solution) => solution.field_3919;
  public static List<Part> PartList(this Sim sim) => sim.field_3818.method_502().field_3919;
  public static int Cycle(this Sim sim) => sim.method_1818();

  public static void SetHexesToMol(this PartType t, Molecule m) =>
    t.field_1540 = m.method_1100().Select(a => a.Key).ToArray();
  public static void SetHexesToAllMols(this PartType t, IEnumerable<Molecule> mls) =>
    t.field_1540 = mls.SelectMany(m => m.method_1100().Keys).Distinct().ToArray();
  public static void SetName(this PartType t, string name) =>
    t.field_1529 = class_134.method_253(name, string.Empty);
  public static void SetDescription(this PartType t, string desc) =>
    t.field_1530 = class_134.method_253(desc, string.Empty);


  public static void SetRequiredOutputs(this Part part, int required) => part_method_1170(part, required);
  public static int GetRequiredOutputs(this Part part) => part.method_1169();
  public static void AddToCurrentOutputs(this PartSimState pss, int add, int limit) {
    if ((pss.field_2730 + 1) <= limit) { pss.field_2730 += add; }
  }
  public static void SetCurrentOutputs(this PartSimState pss, int current) => pss.field_2730 = current;
  public static int CurrentOutputs(this PartSimState pss) => pss.field_2730;

  internal static Solution m1817(this Sim sim) => sim.field_3818.method_502();
  internal static Action<Part, int> part_method_1170 =
    typeof(Part).GetMethod("method_1170", BF.NonPublic | BF.Instance).CreateDelegate<Action<Part, int>>();
  internal static Func<Sim, Molecule, HashSet<HexIndex>, bool> m1837 =
    typeof(Sim).GetMethod("method_1837", BF.NonPublic | BF.Instance).CreateDelegate<Func<Sim, Molecule, HashSet<HexIndex>, bool>>();
  internal static Func<Sim.class_403, HexIndex, bool> c_method_1860 =
    typeof(Sim.class_403).GetMethod("method_1860", BF.NonPublic | BF.Instance).CreateDelegate<Func<Sim.class_403, HexIndex, bool>>();

  public static Func<Molecule, Molecule, bool> molecMatchesExact =
      typeof(Sim).GetMethod("method_1844", BF.NonPublic | BF.Static).CreateDelegate<Func<Molecule, Molecule, bool>>();
  public static bool MolecMatchesSinkAny(Molecule simMolecShifted, Molecule templateShifted) {
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
    return molecMatchesExact(simTemplateMutated, simMutated);
    //TODO: bond adjusting and improve this, it's just yoinked from Extransmissions.
  }

  public static bool MoleculeHeld(this Sim sim, Molecule molec) {
    return sim.HeldGrippers.Any((gripper) => {
      var pss = PSS(sim.SEB(), gripper);
      var maybeHolding = pss.field_2729;
      return maybeHolding.method_1085() && (maybeHolding.method_1087() == molec);
    });
  }
  public static bool DoesNotOverlap(Sim sim, Part item2, Molecule m) {
    HashSet<HexIndex> hashSet = new();
    foreach (Molecule item in sim.field_3823) {
      hashSet.UnionWith(item.method_1100().Keys);
    }
    //HexIndex param_ = item2.method_1161();
    //HexRotation param_2 = item2.method_1163();
    //Molecule molecule = item2.method_1185(sim.m1817()).method_1115(param_2).method_1117(param_);
    if (!m1837(sim, m, hashSet)) {
      return true;
    }
    return false;
  }

  public static void method_1854_crash(this Sim s,string param_5403, HexIndex param_5404, HexIndex param_5405) {
    Vector2 vector = class_187.field_1742.method_492(param_5404);
    Vector2 vector2 = class_187.field_1742.method_492(param_5405);
    s.field_3818.method_518(0f, param_5403, new Vector2[2] { vector, vector2 });
  }


  public static T GetDynStateOrDef<T>(this PartSimState pss, string entry) where T : new() {
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
  public static T? GetDynStateOrNull<T>(this PartSimState pss, string entry) where T : class? {
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
  public static void SetDynState<T>(this PartSimState pss, string entry, T to) {
    DynamicData dyn_pss = new(pss);
    dyn_pss.Set(entry, to);
  }
  public static ExtrawnersDynState GetDefaultDynState(this PartSimState pss) => pss.GetDynStateOrDef<ExtrawnersDynState>("defaultState");

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
  public record class ExtrawnersDynState {
    public bool simStarted = false;
    public Molecule? animatingMolecule = null;
    public bool isOutput = false;
  }

  public static void Play(this Sound sound, SolutionEditorBase seb) {
    sound.field_4062 = false;
    sound.method_28(seb.method_506());
  }

  // class_187: Hex -> Vector tools?

  internal static AtomType? MaybeAtomTypeByName(string name) {
    var all_atoms = QApi.ModAtomTypes.ToList();
    all_atoms.AddRange(VanillaAtomTypes);
    return all_atoms.Where(a => a.QuintAtomType.ToLowerInvariant() == name.ToLowerInvariant()).FirstOrDefault();
  }

  internal static void HexesAndBonds(IEnumerable<Molecule> molecules,
      out HashSet<HexIndex> hexes,
      out HashSet<Pair<HexIndex, HexIndex>> sortaBonds) {
    hexes = new();
    sortaBonds = new();
    foreach (var mol in molecules) {
      hexes.UnionWith(mol.method_1100().Select(a => a.Key));
      sortaBonds.UnionWith(mol.method_1101().Select(a => new Pair<HexIndex, HexIndex>(a.field_2187, a.field_2188)));
    }
  }
}