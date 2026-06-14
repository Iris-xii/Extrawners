



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
using static ExtrawnersExt;

public static class Resources { 
  public static readonly Texture pipe_ring = class_235.method_615("textures/parts/pipe_ring");
  public static readonly Texture pipe_base = class_235.method_615("textures/parts/pipe_base");
  public static readonly Texture pipe_bond = class_235.method_615("textures/parts/pipe_bond");
  internal static Texture[] genericBase = new Texture[] {
    class_235.method_615("textures/i_give_up/0"),
    class_235.method_615("textures/i_give_up/1"),
    class_235.method_615("textures/i_give_up/2"),
    class_235.method_615("textures/i_give_up/3"),
    class_235.method_615("textures/i_give_up/4"),
    class_235.method_615("textures/i_give_up/5"),
    class_235.method_615("textures/i_give_up/6"),
    class_235.method_615("textures/i_give_up/7"),
    class_235.method_615("textures/i_give_up/8"),
    class_235.method_615("textures/i_give_up/9"),
    class_235.method_615("textures/i_give_up/10"),
    class_235.method_615("textures/i_give_up/11"),
    class_235.method_615("textures/i_give_up/12"),
    class_235.method_615("textures/i_give_up/13"),
    class_235.method_615("textures/i_give_up/14"),
    class_235.method_615("textures/i_give_up/15"),
  }; 


}
