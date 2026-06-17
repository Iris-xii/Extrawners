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

#nullable enable
public static class ExtransmutationsCompat {
  /// <summary> To be set from Extransmutations </summary> 
  public static bool isIchorSuppressionActive = false;
  /// <summary> To be read from Extransmutations </summary> 
  public static IEnumerable<HexIndex> IchorSafeSpots { get => perDynGlyphSimSafeSpots.SelectMany(a => a.Value);}
  internal static Dictionary<int, List<HexIndex>> perDynGlyphSimSafeSpots = new();

  public static Action<Sim> updateIsIchorSuppressionActive = (_) => {};

  internal static void OnNewSolve() {
    isIchorSuppressionActive = false;
    perDynGlyphSimSafeSpots = new();
  }
  public static void SetIchorSafeSpots(int glyphIdx, List<HexIndex> simSafeSpots) {
    perDynGlyphSimSafeSpots[glyphIdx] = simSafeSpots;
  }

  internal static void InputDoIchor(int glyphIdx, Part part, List<Molecule> outputRaw) {
    var safeSpots = outputRaw
      .SelectMany(m => m.ShiftedBy(part).method_1100())
      .Where(kv => kv.Value.field_2275.QuintAtomType == "Extransmutations:ichor")
      .Select(kv => kv.Key).ToList();
    SetIchorSafeSpots(glyphIdx,safeSpots);
  }

  internal static void OutputDoIchor(int glyphIdx, Part part, List<Molecule> molAcceptsRaw, Queue<Molecule>? maybeQ) {
    if (maybeQ is not null && maybeQ.Count > 0) {
      var shiftedQ = maybeQ.Peek().ShiftedBy(part).method_1100()
        .Where(kv => kv.Value.field_2275.QuintAtomType == "Extransmutations:ichor")
        .Select(kv => kv.Key)
        .ToList();
      SetIchorSafeSpots(glyphIdx, shiftedQ);
    }
    else {
      var safeSpots = molAcceptsRaw
        .SelectMany(m => m.ShiftedBy(part).method_1100())
        .Where(kv => kv.Value.field_2275.QuintAtomType == "Extransmutations:ichor")
        .Select(kv => kv.Key).ToList();
      SetIchorSafeSpots(glyphIdx, safeSpots);
    }
  } 
}