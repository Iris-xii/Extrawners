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

public enum HoleTexturesKind {
  NORMAL, BLUE, RANDOMIZED_GLYPH, CRIMSON, SPAWNER, GREEN, SINK_ANY_GLYPH, UNKNOWN
}

public record ExtrawnersGlyphBrief {
  public string? customName = null;
  public string? customDesc = null;
  public string internalName = "";
  public ushort requiredProducts = 0; 
  /// <summary> Glyph co-ords </summary>
  public List<Molec> preSimDrawSink = new();
  /// <summary> Glyph co-ords </summary>
  public List<Molec> preSimDrawInput = new();

  internal ExtrawnersGlyphBrief(SpawnerGlyph glyph) {
    customName = glyph.customName;
    customDesc = glyph.customDesc;
    internalName = glyph.internalName; 
    requiredProducts = glyph.requiredProducts;
    preSimDrawInput = glyph.drawInputRawMolecules.Select(m => new Molec(m)).ToList();
    preSimDrawSink = glyph.drawOutputRawMolecules.Select(m => new Molec(m)).ToList();
  }
}

public record ExtrawnersGlyphData {
  public HashSet<ApiHexIdx> holeHexes = new();
  public HashSet<ApiPair<ApiHexIdx, ApiHexIdx>> holeBonds = new();
  public string? customName = null;
  public string? customDesc = null;
  public HoleTexturesKind holeTextures = HoleTexturesKind.NORMAL; 
  /// <summary> Glyph co-ords </summary>
  public List<Molec> preSimDrawSink = new();
  /// <summary> Glyph co-ords </summary>
  public List<Molec> preSimDrawInput = new();
  /// <summary> If != 0, is sink and need this many products to complete the puzzle </summary>
  public ushort requiredProducts = 0;
  /// <summary> Use this to tell apart different glyphs. or don't. your call </summary>
  public string internalName = "";

  //
  internal static Resources.HoleGlyph ActualHoleTex(HoleTexturesKind k) {
    return k switch {
      HoleTexturesKind.NORMAL => Resources.normal,
      HoleTexturesKind.BLUE => Resources.blue,
      HoleTexturesKind.RANDOMIZED_GLYPH => Resources.blue,
      HoleTexturesKind.CRIMSON => Resources.crimson,
      HoleTexturesKind.SINK_ANY_GLYPH => Resources.crimson,
      HoleTexturesKind.SPAWNER => Resources.spawner,
      HoleTexturesKind.GREEN => Resources.spawner,
      HoleTexturesKind.UNKNOWN => Resources.normal,
      _ => Resources.normal,
    };
  }
  internal SpawnerGlyph ToExtrawners(int givenIdx) => new(givenIdx) {
    holeTextures = ActualHoleTex(this.holeTextures),
    holeHexes = new(holeHexes.Select(i => i.OM())),
    holeBonds = new(holeBonds.Select(i => new Quintessential.Pair<HexIndex, HexIndex>(i.Left.OM(), i.Right.OM()))),
    customName = customName,
    customDesc = customDesc, 
    drawOutputRawMolecules = preSimDrawSink.Select(m => m.OM()).ToList(),
    drawInputRawMolecules = preSimDrawInput.Select(m => m.OM()).ToList(),
    requiredProducts = requiredProducts,
    internalName = internalName
  };
}
public record ExtrawnersGlyphState {
  public ExtrawnersGlyphBrief data;
  /// <summary> The object you made in <see cref="InitializePerGlyphUserData"/>, if any</summary>
  public object? userData;

  public Molec ToRelative(Molec absolute) {
    Molecule omm = absolute.OM().ShiftedToGlobal(part);
    return new(omm);
  }
  public Molec RelativeToAbsolute(Molec relative) {
    Molecule omm = relative.OM().SimCoordsToPart(part);
    return new(omm);
  }
  //
  private readonly Part part;
  private readonly SpawnerState state;
  internal ExtrawnersGlyphState(ExtrawnersGlyphBrief data, Part part,SpawnerState state) {
    this.data = data;
    this.part = part;
    this.state = state;
  }
}