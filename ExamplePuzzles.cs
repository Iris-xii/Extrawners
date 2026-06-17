

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

      var salt = new Molecule().Atom("salt", 0, 0);
      var bisalt = new Molecule().Atom("salt", 0, 0).Atom("salt", 0, 1).Bond(enum_126.Standard, 0, 0, 0, 1);
      var fire = new Molecule().Atom("fire", 0, 0);
      var bifire = new Molecule().Atom("fire", 0, 0).Atom("fire", 0, 1).Bond(enum_126.Standard, 0, 0, 0, 1);
      var saltAndFire = new Molecule().Atom("salt", 0, 0).Atom("fire", 0, 1).Bond(enum_126.Standard, 0, 0, 0, 1);

      Presets.Add("c248888215006990", new List<Presets.Preset>() {
        Presets.RandomInputRule(new() {salt,bisalt,fire,bifire}, customName: "Salt/Bisalt/Fire/Bifire",
          dependentOutputs: new Presets.DependentOutput[] {
            new() {outputGlyphIndex = 1, molecules = new Molecule[][] {
              new Molecule[]{salt},
              new Molecule[]{salt,salt},
              new Molecule[]{fire},
              new Molecule[]{fire,fire},
            }}
          }),
        Presets.MultiOutput(new() {salt,fire},customName: "Elemental Salt / Elemental Fire")
      });

      Presets.Add("c994277786629428", new() {
        Presets.RandomInputRule(new() {saltAndFire}, dependentOutputs: new Presets.DependentOutput[] {
          new() {outputGlyphIndex = 1, molecules = new Molecule[][] {
            new Molecule[]{salt,fire}
          }}
        }),
        Presets.MultiOutput(new() {salt,fire})
      });

      Presets.presetsTable.Add("c019087729916591", new() {
        Presets.RandomInputRule(new() {
          new Molecule().Atom("iron",0,0).Atom("quicksilver",0,-1).Atom("quicksilver",-1,1).Atom("quicksilver",1,0).Bond((enum_126)1,0,-1,0,0).Bond((enum_126)1,-1,1,0,0).Bond((enum_126)1,0,0,1,0),
          new Molecule().Atom("earth",-2,2).Atom("earth",-1,1).Atom("fire",0,0).Bond((enum_126)1,-1,1,0,0).Bond((enum_126)1,-2,2,-1,1),
          new Molecule().Atom("earth",0,0).Atom("earth",-2,2).Atom("salt",-1,1).Atom("quicksilver",-3,2).Bond((enum_126)1,-1,1,0,0).Bond((enum_126)1,-2,2,-1,1).Bond((enum_126)1,-3,2,-2,2),
          new Molecule().Atom("water",0,0).Atom("quicksilver",-1,1).Bond((enum_126)1,-1,1,0,0),
        }),
        Presets.MultiOutput(new() {
          new Molecule().Atom("iron",0,0).Atom("quicksilver",0,-1).Atom("quicksilver",-1,1).Atom("quicksilver",1,0).Bond((enum_126)1,0,-1,0,0).Bond((enum_126)1,-1,1,0,0).Bond((enum_126)1,0,0,1,0),
          new Molecule().Atom("earth",-2,2).Atom("earth",-1,1).Atom("fire",0,0).Bond((enum_126)1,-1,1,0,0).Bond((enum_126)1,-2,2,-1,1),
          new Molecule().Atom("earth",0,0).Atom("earth",-2,2).Atom("salt",-1,1).Atom("quicksilver",-3,2).Bond((enum_126)1,-1,1,0,0).Bond((enum_126)1,-2,2,-1,1).Bond((enum_126)1,-3,2,-2,2),
          new Molecule().Atom("water",0,0).Atom("quicksilver",-1,1).Bond((enum_126)1,-1,1,0,0),
        },requiredProducts: 16),
        Presets.MultiOutput(new() {
          new Molecule().Atom("iron",0,0).Atom("quicksilver",0,-1).Atom("quicksilver",-1,1).Atom("quicksilver",1,0).Bond((enum_126)1,0,-1,0,0).Bond((enum_126)1,-1,1,0,0).Bond((enum_126)1,0,0,1,0),
          new Molecule().Atom("earth",-2,2).Atom("earth",-1,1).Atom("fire",0,0).Bond((enum_126)1,-1,1,0,0).Bond((enum_126)1,-2,2,-1,1),
          new Molecule().Atom("earth",0,0).Atom("earth",-2,2).Atom("salt",-1,1).Atom("quicksilver",-3,2).Bond((enum_126)1,-1,1,0,0).Bond((enum_126)1,-2,2,-1,1).Bond((enum_126)1,-3,2,-2,2),
          new Molecule().Atom("water",0,0).Atom("quicksilver",-1,1).Bond((enum_126)1,-1,1,0,0),
        },requiredProducts: 16,sinkAny: true),
        Presets.MultiOutput(new() {
          new Molecule().Atom("iron",0,0).Atom("quicksilver",0,-1).Atom("quicksilver",-1,1).Atom("quicksilver",1,0).Bond((enum_126)1,0,-1,0,0).Bond((enum_126)1,-1,1,0,0).Bond((enum_126)1,0,0,1,0),
          new Molecule().Atom("earth",-2,2).Atom("earth",-1,1).Atom("fire",0,0).Bond((enum_126)1,-1,1,0,0).Bond((enum_126)1,-2,2,-1,1),
          new Molecule().Atom("earth",0,0).Atom("earth",-2,2).Atom("salt",-1,1).Atom("quicksilver",-3,2).Bond((enum_126)1,-1,1,0,0).Bond((enum_126)1,-2,2,-1,1).Bond((enum_126)1,-3,2,-2,2),
          new Molecule().Atom("water",0,0).Atom("quicksilver",-1,1).Bond((enum_126)1,-1,1,0,0),
        },requiredProducts: 16,sinkAny: true, wrongMolCrashesSim: true),
      });
      m2 = m3;
      puzzleGlyphData.Add("disabled-c248888215006990", new() {
        origins = new() { new(0, 0), new(3, 3), new(-4, 0), new(4, 2) },
        partTypeModify = (partTypes, sol) => {
          partTypes[0].field_1540 = m0.method_1100().Select(a => a.Key).ToArray();
          partTypes[1].field_1540 = m1.method_1100().Select(a => a.Key).ToArray();
          partTypes[2].field_1540 = m2.method_1100().Select(a => a.Key).ToArray();
          partTypes[3].field_1540 = m3.method_1100().Select(a => a.Key).ToArray();
        },
        partRenderer = (glyphIndex, part, pos, seb, renderer) => {
          var pss = PSS(seb, part);
          if (glyphIndex == 0) {
            SpawnerGlyph.DrawFullBaseFromMol(renderer, m0);
            SpawnerGlyph.DrawMol(m0, PSS(seb, part), pos, part, alpha: 0.3f);
          }
          else if (glyphIndex == 1) {
            SpawnerGlyph.DrawFullBaseFromMol(renderer, m1);
            SpawnerGlyph.DrawMol(m1, PSS(seb, part), pos, part, alpha: 0.3f);
          }
          else if (glyphIndex == 2) {
            SpawnerGlyph.DrawFullBaseFromMol(renderer, m2);
            SpawnerGlyph.DrawMolAsIfInput(m2, seb, PSS(seb, part), pos, part);
          }
          else if (glyphIndex == 3) {
            SpawnerGlyph.DrawFullBaseFromMol(renderer, m3);
            SpawnerGlyph.DrawMolAsIfOutput(m3, seb, PSS(seb, part), renderer, pos, part);
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

/*
{
            var rth = new RenderTargetHandle();
            rth.field_2987 = new(160, 160);
            class_95 rth_96 = rth.method_1352(out var shouldRender);
            if (shouldRender) {
              Bounds2 bounds = Bounds2.Undefined;
              foreach (HexIndex key in partTypes[3].field_1540) {
                Bounds2 bounds2 = Bounds2.CenteredOn(class_187.field_1742.method_491(key, Vector2.Zero), class_187.field_1742.field_1744.X, class_187.field_1742.field_1744.Y * 1.3f);
                bounds = bounds.UnionedWith(bounds2);
              }
              Vector2 param_4594 = Vector2.Zero; 

              //float shrink =
              //(!(bounds.Width < param_4594.X / 0.7f) || !(bounds.Height < param_4594.Y / 0.7f)) ? Math.Min(param_4594.X / bounds.Width, param_4594.Y / bounds.Height) : 0.7f;
              float shrink = 0.8f;
              class_195 renderer = new((-new Vector2(160/2*shrink,160)+((bounds.Size)/2*shrink)), 1f, Vector2.Zero);
              using (class_226.method_597(rth_96, Matrix4.method_1075(shrink))) {
                class_226.method_600(Color.Transparent);
                SpawnerGlyph.DrawFullBaseFromMol(renderer, m3);
                //Editor.method_925(m3,
                //  renderer.field_1797,
                //  new(), 
                //  0.0f   
                //  1.0f  
                //  1f  
                //  1f  
              }

              var tex = rth_96.field_937;
              var t2 = Renderer.method_1313(tex);
              var retex = Renderer.method_1310(t2);

              SpawnerGlyph.partTypes[3].field_1547 = retex;
              SpawnerGlyph.partTypes[3].field_1548 = retex;
            }
          }
*/