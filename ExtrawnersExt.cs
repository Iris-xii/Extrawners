
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
  public static PartType Type(this Part part) => part.method_1159();

  // class_187: Hex -> Vector tools?

  internal static AtomType? MaybeAtomTypeByName(string name) {
    var all_atoms = QApi.ModAtomTypes.ToList();
    all_atoms.AddRange(VanillaAtomTypes);
    return all_atoms.Where(a => a.QuintAtomType.ToLowerInvariant() == name.ToLowerInvariant()).FirstOrDefault();
  }
}