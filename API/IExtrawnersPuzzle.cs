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

using static LogicWhen;

public sealed record ProduceArgs {
  public bool isSimInitialization;
  public int currentCycle;
  /// <summary> In absolute (sim) co-ords! </summary>
  public List<Molec> allMolecsInSim = new();
  public List<ExtrawnersGlyphState> extrawnersGlyphs = new();
}
public sealed record ProduceOut() {
  /// <summary> If present, errors the sim at the specified location with the specified message. </summary>
  public ApiPair<string, ApiHexIdx>? crashSim = null;
}
public sealed record SinkArgs {
  public int currentCycle;
  /// <summary> In absolute (sim) co-ords! </summary>
  public List<Molec> allMolecsInSim = new();
  public List<ExtrawnersGlyphState> extrawnersGlyphs = new();
}
public sealed record SinkOut() {
  /// <summary> Sink these molecules (remove them from the sim) (absolute coords!) </summary>
  public List<Molec> sinkMolecules = new();
  /// <summary> Progress the Glyph's requiredProducts by this amount </summary>
  public List<ApiPair<ExtrawnersGlyphState, int>> progressBy = new();
  /// <summary> If present, errors the sim at the specified location with the specified message. </summary>
  public ApiPair<string, ApiHexIdx>? crashSim = null;
}
public sealed record DisplayArgs { 
  public int currentCycle;
  /// <summary> A 0-1 float you may use to change your output and thus animate the rendered mols </summary>
  public float displayTime;
  /// <summary> In absolute (sim) co-ords! </summary>
  public List<Molec> allMolecsInSim = new();
  public List<ExtrawnersGlyphState> extrawnersGlyphs = new();
}
public sealed record DisplayOut() {
  /// <summary> Absolute (sim) coords.<br/>
  /// All of these molecules will be rendered "darkened", like an output preview. </summary>
  public List<Molec> renderAsIfSink = new();
}

/// <summary>
/// Entrypoint for an Extrawners puzzle.
/// </summary>
public interface IExtrawnersPuzzle {
  public IEnumerator<ExtrawnersGlyphData> MakeExtrawnersGlyphs();
  /// <summary> Allows you to attach a bit of per-glyph state of your choice. </summary> 
  public object? InitializePerGlyphUserData(ExtrawnersGlyphBrief glyph);
  /// <summary> Called once at sim start, and then every time molecs may be produced </summary> 
  public ProduceOut Produce(ProduceArgs args);
  public SinkOut Sink(SinkArgs args);
  /// <summary> Called to decide what molecules to render during the sim, such as 
  /// previews over the outputs/sinks matching what they expect </summary> 
  public DisplayOut Display(DisplayArgs args);
}