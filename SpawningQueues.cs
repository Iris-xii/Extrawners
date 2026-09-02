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
using System.Collections;

internal sealed record class SpawningLists {
  private List<List<Molecule>> inner = new();

  public IEnumerable<KeyValuePair<int, List<Molecule>>> Enumerate() {
    for (int i = 0; i < inner.Count; i++) {
      yield return new(i, inner[i]);
    }
  }

  public void Set(List<Molecule> listToInsert, int at) {
    if (at < inner.Count) {
      inner[at] = listToInsert;
    }
    else {
      while (at >= inner.Count) {
        inner.Add(new());
      }
      inner[at] = listToInsert;
    }
  }

  public List<Molecule> Get(int idx) {
    if (idx < inner.Count) {
      return inner[idx];
    }
    else {
      Set(new(), idx);
      return inner[idx];
    }
  }
}