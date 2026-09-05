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
#nullable enable

internal sealed record CrashSim {
  internal string message = "";
  internal ApiHexIdx location;
}

public sealed record Produce {
  public bool isSimInitialization;
  public int currentCycle;
  /// <summary> In absolute (sim) co-ords! </summary>
  public List<Molec> allMolecsInSim = new();
  public List<ExtrawnersGlyphState> extrawnersGlyphs = new();
  public bool ichorSuppressionActive = false;

  /// <summary> Could this molec be spawned into the sim, or would it be blocked by something? </summary> 
  public bool CouldSpawn(Molec molecAbsolute) => DoesNotOverlap(sim, null, molecAbsolute.OM());
  /// <summary> Try to spawn/produce this molecule into the sim. Check out 
  /// <see cref="CouldSpawn"/> to know in advance if it will be successful.</summary> 
  /// <returns>Whether the molecule could be spawned in.</returns>
  public bool TryToSpawn(Molec molecAbsolute) {
    var could = CouldSpawn(molecAbsolute);
    if (could) { produceMolecs.Add(molecAbsolute); }
    return could;
  }
  /// <summary>Crash the sim, displaying an error message on the transmutation engine.</summary> 
  public void CrashSim(string message,ApiHexIdx location) => crashSim = new() {message = message,location = location};
  /// <summary> 'Progress' the given glyph towards sim completion by progress += amount. </summary> 
  public void ProgressBy(ExtrawnersGlyphState glyph,int amount) => progressBy.Add(new(glyph,amount)); 
  public void Log(string s) => ExtrawnersMod.Log(s);

  //
  internal Sim sim = null!;
  internal List<Molec> produceMolecs = new();
  internal CrashSim? crashSim = null;
  internal List<ApiPair<ExtrawnersGlyphState, int>> progressBy = new();
} 
public sealed record Sink {
  public int currentCycle;
  /// <summary> In absolute (sim) co-ords! </summary>
  public List<Molec> allMolecsInSim = new();
  public List<ExtrawnersGlyphState> extrawnersGlyphs = new();
  public bool ichorSuppressionActive = false;

  public void Log(string s) => ExtrawnersMod.Log(s);  
  /// <summary> Could the given molec be sunk, or is it grabbed? </summary> 
  public bool CouldSink(Molec molecAbsolute) {
    Molecule? simMolSunk = sim.field_3823
        .Where(simMol => new Molec(simMol).MatchesExact(molecAbsolute))
        .FirstOrDefault();
    if (simMolSunk is null) return true;
    return !sim.MoleculeHeld(simMolSunk);
  }
  /// <summary> Try to sink this molecule. Check out 
  /// <see cref="CouldSink"/> to know in advance if it will be successful.</summary> 
  /// <returns>Whether the molecule was able to be sunk/output.</returns>
  public bool TryToSink(Molec molecAbsolute) {
    var could = CouldSink(molecAbsolute);
    if(could) {sinkMolecules.Add(molecAbsolute);}
    return could;
  }
  /// <summary> 'Progress' the given glyph towards sim completion by progress += amount. </summary> 
  public void ProgressBy(ExtrawnersGlyphState glyph,int amount) => progressBy.Add(new(glyph,amount));  
  /// <summary>Crash the sim, displaying an error message on the transmutation engine.</summary> 
  public void CrashSim(string message,ApiHexIdx location) => crashSim = new() {message = message,location = location};
  //
  internal Sim sim = null!;
  internal List<Molec> sinkMolecules = new();
  internal List<ApiPair<ExtrawnersGlyphState, int>> progressBy = new();
  internal CrashSim? crashSim = null;
} 
public sealed record Display {
  /// <summary> A float that increases over time, for animation purposes </summary>
  public float accumulatedTime;
  public ExtrawnersGlyphState extrawnersGlyphBeingRendered = null!;
  public bool ichorSuppressionActive = false;

  public void Log(string s) => ExtrawnersMod.Log(s);

  /// <summary> This molecule will be rendered "darkened", like an output preview. </summary>
  public void RenderAsSink(Molec molecRelative) => renderAsIfSinkRelativeToGlyph.Add(molecRelative);
  internal List<Molec> renderAsIfSinkRelativeToGlyph = new();
} 
public sealed record MakeGlyphData {
  public string puzzleId = "";
}

/// <summary>
/// Entrypoint for an Extrawners puzzle.
/// </summary>
public interface IExtrawnersPuzzle {
  /// <summary> Called on sim start/reset, must return a 'fresh' copy with brand new state </summary> 
  public IExtrawnersPuzzle MakeNew();
  public IEnumerable<ExtrawnersGlyphData> MakeExtrawnersGlyphs(MakeGlyphData args); 
  /// <summary> Called once at sim start, and then every time molecs may be produced </summary> 
  public void Produce(Produce args);
  /// <summary> Called to decide what molecules should be sunk/output </summary> 
  public void Sink(Sink args);
  /// <summary> Called to decide what molecules to render during the sim, such as 
  /// previews over the outputs/sinks matching what they expect. <br/>
  /// Unlike other functions, this is called once per individual glyph! </summary> 
  public void Display(Display args);
  
  public List<int> InputsToRemove();
  public List<int> OutputsToRemove();
}