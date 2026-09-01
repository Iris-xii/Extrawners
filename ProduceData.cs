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
using Quintessential;
using System;

using static Extrawners.ExtrawnersMod;
using static ExtrawnersExt;
using static LogicWhen;


#nullable enable

internal record struct SpawnChooseMethod() {
  internal enum K { SEQUENCE, RANDOM }
  private K k = K.SEQUENCE;
  internal static SpawnChooseMethod RepeatingSeq() => new() { k = K.SEQUENCE };
  internal static SpawnChooseMethod Random() => new() { k = K.RANDOM };
  internal Molecule? PeekChooseFrom(List<Molecule> mlist,
      Random rng,
      out int indexChosen) {
    indexChosen = -1;
    if (mlist.Count == 0) return null;
    if (k == K.SEQUENCE) {
      indexChosen = 0;
      return mlist[indexChosen];
    }
    else if (k == K.RANDOM) {
      indexChosen = rng.Next(0, mlist.Count); 
      return mlist[indexChosen];
    }
    return null;
  }
}

internal sealed record ProduceData() {
  /// <summary> Fill the spawn queue with these molecules at sim start, just one single time </summary>
  internal List<Molecule> initialSpawnQueue = new();
  /// <summary> Refill the spawn queue with these molecules when it's empty (and not sim start) </summary>
  internal List<Molecule> repeatingRefillQueue = new();
  /// <summary> How to choose the next molecule to output from the spawn queue </summary>
  internal SpawnChooseMethod queueChooseMethod = SpawnChooseMethod.RepeatingSeq();
}