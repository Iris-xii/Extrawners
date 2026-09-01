



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
  public static readonly Texture pipe_ring = normal.ring;
  public static readonly Texture pipe_base = normal.bg;
  public static readonly Texture pipe_bond = normal.bond;
  public static readonly Texture blue_pipe_ring = blue.ring;
  public static readonly Texture blue_pipe_base = blue.bg;
  public static readonly Texture blue_pipe_bond = blue.bond;
  public static readonly Texture crimson_pipe_ring = crimson.ring;
  public static readonly Texture crimson_pipe_base = crimson.bg;
  public static readonly Texture crimson_pipe_bond = crimson.bond;
  public static readonly Texture spawner_pipe_ring = spawner.ring;
  public static readonly Texture spawner_pipe_base = spawner.bg;
  public static readonly Texture spawner_pipe_bond = spawner.bond;
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

  public static readonly HoleGlyph normal = new() {
    ring = class_235.method_615("textures/parts/pipe_ring"),
    bg = class_235.method_615("textures/parts/pipe_base"),
    bond = class_235.method_615("textures/parts/pipe_bond"),
  };
  public static readonly HoleGlyph blue = new() {
    ring = class_235.method_615("textures/ioglyphs/blue_pipe_ring"),
    bg = class_235.method_615("textures/ioglyphs/blue_pipe_base"),
    bond = class_235.method_615("textures/ioglyphs/blue_pipe_bond"),
  };
  public static readonly HoleGlyph crimson = new() {
    ring = class_235.method_615("textures/ioglyphs/crimson_pipe_ring"),
    bg = class_235.method_615("textures/ioglyphs/crimson_pipe_base"),
    bond = class_235.method_615("textures/ioglyphs/crimson_pipe_bond"),
  };
  public static readonly HoleGlyph spawner = new() {
    ring = class_235.method_615("textures/ioglyphs/spawner_pipe_ring"),
    bg = class_235.method_615("textures/ioglyphs/spawner_pipe_base"),
    bond = class_235.method_615("textures/ioglyphs/spawner_pipe_bond"),
  };
  public record struct HoleGlyph {
    public Texture ring;
    public Texture bg; //`base` but it's a keyword, so.
    public Texture bond;
  }
}
