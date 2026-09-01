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

using static LogicWhen;
#nullable enable
public enum LogicWhen {
  PRE_CYCLE,
  FIRST_HALF,
  SECOND_HALF,
  MID_CYCLE_BEFORE_ANIM,
}
public static class LogicWhenExt {
  public static bool FireGlyph(this LogicWhen when) => when == FIRST_HALF || when == SECOND_HALF;
} 
public delegate void PartTypeModify(PartType[] partTypes, Solution s);
public delegate void RenderFn(int glyphIndex,
    Part part,
    Vector2 pos,
    SolutionEditorBase seb,
    class_195 renderer);
public delegate void LogicFn(Sim sim, LogicWhen when);