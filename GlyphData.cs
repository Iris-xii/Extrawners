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

using static LogicWhen;

#nullable enable
public enum LogicWhen {
  PRE_CYCLE,
  FIRST_HALF,
  SECOND_HALF,
  WELL_AFTER_CYCLE,
}
public static class LogicWhenExt {
  public static bool FireGlyph(this LogicWhen when) => when == FIRST_HALF || when == SECOND_HALF;
}


public sealed record class GlyphData {
  /// <summary>
  /// List of origins for every glyph. An entry here implies the existence of said glyph. (Otherwise
  /// it won't spawn)
  /// </summary>
  public List<HexIndex> origins = new(); 
  public delegate void PartTypeModify(PartType[] partTypes,Solution s);
  public PartTypeModify partTypeModify = (_t,_) => {};
  public delegate void RenderFn(int glyphIndex,
      Part part,
      Vector2 pos,
      SolutionEditorBase seb,
      class_195 renderer);
  public RenderFn partRenderer = (_,_,_,_,_) => {};

  public delegate void LogicFn(Sim sim,LogicWhen when);
  public LogicFn logicFn = (_,_) => {};

  internal Action<int,int,Sim> multiOutputSuccessfulOutputCallbacks = (_,_,_) => {}; 
}