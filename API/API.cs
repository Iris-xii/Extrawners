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

  internal readonly Quintessential.Pair<A,B> Quint() => new(this.Left,this.Right);
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
  public Bond(int bondTypeFlags,ApiHexIdx a,ApiHexIdx b) {
    this.bondTypeFlags = bondTypeFlags;
    this.a = a;
    this.b = b;
  }

  public const int StandardFlag = 1;
  public const int Prisma0Flag = 2;
  public const int Prisma1Flag = 4;
  public const int Prisma2Flag = 8;
  public bool HasStandardBond() => (bondTypeFlags & StandardFlag) > 0;
  public bool HasPrisma0() => (bondTypeFlags & Prisma0Flag) > 0;
  public bool HasPrisma1() => (bondTypeFlags & Prisma1Flag) > 0;
  public bool HasPrisma2() => (bondTypeFlags & Prisma2Flag) > 0;

  internal Bond(class_277 omBond) {
    bondTypeFlags = (int)omBond.field_2186;
    a = new(omBond.field_2187);
    b = new(omBond.field_2188);
  }
  internal readonly class_277 OM() => new((enum_126)bondTypeFlags, a.OM(), b.OM());
}

public sealed class Molec {
  /// <summary> `string` is the atom's quint ID. </summary>
  public Dictionary<ApiHexIdx, string> atoms = new();
  public List<Bond> bonds = new();
  public Molec() {}

  public bool MatchesExact(Molec other) { // TODO: reimplement this without conversions
    var om1 = this.OM();
    var om2 = other.OM();
    //DebugLog($"Comparison {DumpMol(om1)} vs {DumpMol(om2)}: {molecMatchesExact(om1,om2)}");
    return molecMatchesExact(om1,om2);
  }
  /// <summary> Returns true if <see cref="smaller"/> can 'fit through' this molecule,
  /// if this molecule were a hole. </summary> 
  public bool OtherCanFitThrough(Molec smaller) {
    var om1 = this.OM();
    var om2 = smaller.OM();
    return MolecMatchesSinkAny(om2,om1,null);
  }


//
  internal Molecule OM() {
    Molecule omMolec = new();
    foreach (var KV in atoms) {
      var atomId = KV.Value;
      var pos = KV.Key.OM();
      omMolec.method_1105(new(atomId.AsQuintAtomType()), pos);
    }
    foreach (var bond in bonds) {
      omMolec.method_1111((enum_126)bond.bondTypeFlags, bond.a.OM(), bond.b.OM());
    }
    return omMolec;
  }
  internal Molec(Molecule omm) { 
    foreach (var kv in omm.method_1100()) {
      var atom = kv.Value.field_2275.QuintAtomType;
      var pos = new ApiHexIdx(kv.Key);
      this.atoms.Add(pos,atom);
    }
    foreach (var bond in omm.method_1101()) {
      var myBond = new Bond(bond);
      this.bonds.Add(myBond);
    } 
  }
}