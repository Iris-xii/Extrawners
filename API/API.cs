namespace Extrawners.API;

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


public record struct ApiPair<A, B> {
  public ApiPair(A left, B right) { Left = left; Right = right; }
  public A Left { get; set; }
  public B Right { get; set; }

  internal readonly Quintessential.Pair<A, B> Quint() => new(this.Left, this.Right);
}

public readonly record struct ApiHexIdx {
  public readonly int Q;
  public readonly int R;
  public ApiHexIdx(int q, int r) { Q = q; R = r; }

  internal readonly HexIndex OM() => new(this.Q, this.R);
  internal ApiHexIdx(HexIndex om) {
    Q = om.Q;
    R = om.R;
  }
}

public readonly record struct Bond {
  public readonly int bondTypeFlags;
  public readonly ApiHexIdx a;
  public readonly ApiHexIdx b;
  public Bond(int bondTypeFlags, ApiHexIdx a, ApiHexIdx b) {
    this.bondTypeFlags = bondTypeFlags;
    this.a = a;
    this.b = b;
  }
  public const int StandardFlag = 1;
  public const int Prisma0Flag = 2;
  public const int Prisma1Flag = 4;
  public const int Prisma2Flag = 8;
  internal const int AllPrismaFlagsCombined = Prisma0Flag | Prisma1Flag | Prisma2Flag;
  public bool HasStandardBond() => (bondTypeFlags & StandardFlag) > 0;
  public bool HasTriplexO() => (bondTypeFlags & Prisma0Flag) > 0;
  public bool HasTriplexG() => (bondTypeFlags & Prisma1Flag) > 0;
  public bool HasTriplexR() => (bondTypeFlags & Prisma2Flag) > 0;
  public bool IsTriplexOGR() => (bondTypeFlags & AllPrismaFlagsCombined) == (Prisma0Flag | Prisma1Flag | Prisma2Flag);
  public bool IsTriplexO() => (bondTypeFlags & AllPrismaFlagsCombined) == Prisma0Flag;
  public bool IsTriplexG() => (bondTypeFlags & AllPrismaFlagsCombined) == Prisma1Flag;
  public bool IsTriplexR() => (bondTypeFlags & AllPrismaFlagsCombined) == Prisma2Flag;
  public bool IsTriplexOG() => (bondTypeFlags & AllPrismaFlagsCombined) == (Prisma0Flag | Prisma1Flag);
  public bool IsTriplexOR() => (bondTypeFlags & AllPrismaFlagsCombined) == (Prisma0Flag | Prisma2Flag);
  public bool IsTriplexGR() => (bondTypeFlags & AllPrismaFlagsCombined) == (Prisma1Flag | Prisma2Flag);

  internal Bond(class_277 omBond) {
    bondTypeFlags = (int)omBond.field_2186;
    a = new(omBond.field_2187);
    b = new(omBond.field_2188);
  }
  internal readonly class_277 OM() => new((enum_126)bondTypeFlags, a.OM(), b.OM());
}

public sealed class Molec {
  /// <summary> `string` is the atom's quint ID. </summary>
  private Dictionary<ApiHexIdx, string> _atoms = new();
  private List<Bond> _bonds = new();

  public IList<Bond> Bonds { get => _bonds; }
  public IDictionary<ApiHexIdx, string> Atoms { get => _atoms; }

  public Molec() { }
  public Molec Atom(string atomType, int q, int r) { Atoms.Add(new(q, r), atomType); return this; }
  public Molec Bond(int bondType, int qA, int rA, int qB, int rB) {
    Bonds.Add(new(bondType, new(qA, rA), new(qB, rB))); return this;
  }
  public Molec ShiftBy(ApiHexIdx amount) => ShiftBy(amount.Q, amount.R);
  public Molec ShiftBy(int Q, int R) {
    var shifted = new Molec {
      _bonds = this.Bonds
      .Select(bnd => new Bond(bnd.bondTypeFlags, new(bnd.a.Q + Q, bnd.a.R + R), new(bnd.b.Q + Q, bnd.b.R + R))).ToList(),
      _atoms = new Dictionary<ApiHexIdx, string>()
    };
    foreach (var atom in this.Atoms) {shifted._atoms.Add(new(atom.Key.Q + Q,atom.Key.R + R),atom.Value);}
    return shifted;
  }
  public Molec Merged(Molec mergeOnTop) {
    Dictionary<ApiHexIdx, string> mAtoms = new(this._atoms);
    HashSet<Bond> mBonds = new(this._bonds);
    foreach(var oBond in mergeOnTop.Bonds) {
      mBonds.Add(oBond);
    }
    foreach(var oAtom in mergeOnTop.Atoms) {
      mAtoms[oAtom.Key] = oAtom.Value;
    }
    return new Molec {
      _atoms = mAtoms,
      _bonds = new(mBonds),
    };
  }

  public bool MatchesExact(Molec other) { // TODO: reimplement this without conversions
    var om1 = this.OM();
    var om2 = other.OM();
    //DebugLog($"Comparison {DumpMol(om1)} vs {DumpMol(om2)}: {molecMatchesExact(om1,om2)}");
    return molecMatchesExact(om1, om2);
  }
  /// <summary> Returns true if <see cref="smaller"/> can 'fit through' this molecule,
  /// if this molecule were a hole. </summary> 
  public bool OtherCanFitThrough(Molec smaller) {
    var om1 = this.OM();
    var om2 = smaller.OM();
    return MolecMatchesSinkAny(om2, om1, null);
  }

  //
  internal Molecule OM() {
    Molecule omMolec = new();
    foreach (var KV in Atoms) {
      var atomId = KV.Value;
      var pos = KV.Key.OM();
      omMolec.method_1105(new(atomId.AsQuintAtomType()), pos);
    }
    foreach (var bond in Bonds) {
      omMolec.method_1111((enum_126)bond.bondTypeFlags, bond.a.OM(), bond.b.OM());
    }
    return omMolec;
  }
  internal Molec(Molecule omm) {
    foreach (var kv in omm.method_1100()) {
      var atom = kv.Value.field_2275.QuintAtomType;
      var pos = new ApiHexIdx(kv.Key);
      this.Atoms.Add(pos, atom);
    }
    foreach (var bond in omm.method_1101()) {
      var myBond = new Bond(bond);
      this.Bonds.Add(myBond);
    }
  }
}