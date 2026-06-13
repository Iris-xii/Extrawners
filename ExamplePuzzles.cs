

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

using static LogicWhen;

public sealed partial class ExtrawnersMod {

  public static void DoExamplePuzzles() {
    {
      var m0 = new Molecule()
        .Atom("extransmutations:ichor", 0, 0)
        .Atom("extransmutations:ichor", 1, 0)
        .Atom("extransmutations:ichor", 2, 0)
        .Bond(BondKinds.normal, 1, 0, 2, 0);
      var m1 = new Molecule()
        .Atom("salt", 0, 0)
        .Atom("salt", 1, 0)
        .Bond(BondKinds.triplex_ogr, 0, 0, 1, 0);
      var m2 = new Molecule()
        .Atom("water", 0, 0)
        .Atom("water", 0, 1)
        .Atom("water", 1, 1)
        .Atom("water", 2, 0)
        .Atom("water", 1, -1)
        .Atom("water", 2, -1)
        .Bond(BondKinds.normal, 0, 0, 0, 1)
        .Bond(BondKinds.normal, 0, 1, 1, 1)
        .Bond(BondKinds.normal, 1, 1, 2, 0)
        .Bond(BondKinds.normal, 2, 0, 2, -1)
        .Bond(BondKinds.normal, 2, -1, 1, -1)
        .Bond(BondKinds.normal, 1, -1, 0, 0);
      var m3 = new Molecule()
        .Atom("water", 0, 0)
        .Atom("water", 0, 1)
        .Atom("water", 1, 1)
        .Atom("water", 2, 0)
        .Atom("water", 1, -1)
        .Atom("water", 2, -1)
        .Atom("salt", 2, -2)
        .Bond(BondKinds.normal, 0, 0, 0, 1)
        .Bond(BondKinds.normal, 0, 1, 1, 1)
        .Bond(BondKinds.normal, 1, 1, 2, 0)
        .Bond(BondKinds.normal, 2, 0, 2, -1)
        .Bond(BondKinds.normal, 2, -1, 1, -1)
        .Bond(BondKinds.normal, 1, -1, 0, 0)
        .Bond(BondKinds.normal, 2, -1, 2, -2)
        .Bond(BondKinds.normal, 1, -1, 2, -2);
      m2 = m3;

      puzzleGlyphData.Add("c248888215006990", new() {
        origins = new() { new(0, 0), new(3, 3), new(-4, 0), new(4, 2) },
        partTypeModify = (partTypes) => {
          partTypes[0].field_1540 = m0.method_1100().Select(a => a.Key).ToArray();
          partTypes[1].field_1540 = m1.method_1100().Select(a => a.Key).ToArray();
          partTypes[2].field_1540 = m2.method_1100().Select(a => a.Key).ToArray();
          partTypes[3].field_1540 = m3.method_1100().Select(a => a.Key).ToArray();
        },
        partRenderer = (glyphIndex, part, pos, seb, renderer) => {
          var pss = PSS(seb, part);
          if (glyphIndex == 0) {
            SpawnerGlyph.DrawFullBaseFromMol(part, pos, seb, renderer, m0);
            SpawnerGlyph.DrawMol(m0, PSS(seb, part), pos, part, alpha: 0.3f);
          }
          else if (glyphIndex == 1) {
            SpawnerGlyph.DrawFullBaseFromMol(part, pos, seb, renderer, m1);
            SpawnerGlyph.DrawMol(m1, PSS(seb, part), pos, part, alpha: 0.3f);
          }
          else if (glyphIndex == 2) {
            SpawnerGlyph.DrawFullBaseFromMol(part, pos, seb, renderer, m2);
            SpawnerGlyph.DrawMolAsIfInput(m2, seb, PSS(seb, part), pos, part);
          }
          else if (glyphIndex == 3) {
            SpawnerGlyph.DrawFullBaseFromMol(part, pos, seb, renderer, m3);
            SpawnerGlyph.DrawMolAsIfOutput(m3, seb, PSS(seb, part), pos, part);
          }
        },
        logicFn = (sim, when) => {
          foreach (var part in sim.PartList()) {
            var pss = PSS(sim.SEB(), part);
            if (part.Type() == SpawnerGlyph.partTypes[2]) {
              SpawnerGlyph.AsIfInput(sim, m2, pss, part, when);
            }
            if (part.Type() == SpawnerGlyph.partTypes[3]) {
              SpawnerGlyph.AsIfOutput(sim, m3, pss, part, when);
            }
          }
        },
      });
    }
  }
}