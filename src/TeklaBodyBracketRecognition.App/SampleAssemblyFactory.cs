using TeklaBodyBracketRecognition.Core.Domain;

namespace TeklaBodyBracketRecognition.App;

internal static class SampleAssemblyFactory
{
    public static AssemblyInput CreateBuiltUpHWithBracket()
    {
        var parts = new[]
        {
            new PartInput(101, RuntimePartType.ContourPlate, "Web", "Q355B", "PL10", true, false, new Vector3(0, 0, 0), new Vector3(6000, 12, 500), new BoundingBox(new Vector3(-3000, -6, -250), new Vector3(3000, 6, 250)), 36_000_000, 6_100_000, 12, new Vector3(0, 1, 0), new Vector3(1, 0, 0), new Vector3(0, 0, 1), 4, 0, 0, 0, 0, 2.5, 0.1),
            new PartInput(102, RuntimePartType.ContourPlate, "TopFlange", "Q355B", "PL16", true, false, new Vector3(0, 0, 250), new Vector3(6000, 300, 16), new BoundingBox(new Vector3(-3000, -150, 242), new Vector3(3000, 150, 258)), 28_800_000, 4_500_000, 16, new Vector3(0, 0, 1), new Vector3(1, 0, 0), new Vector3(0, 1, 0), 4, 0, 0, 0, 0, 2.2, 0.1),
            new PartInput(103, RuntimePartType.ContourPlate, "BottomFlange", "Q355B", "PL16", true, false, new Vector3(0, 0, -250), new Vector3(6000, 300, 16), new BoundingBox(new Vector3(-3000, -150, -258), new Vector3(3000, 150, -242)), 28_800_000, 4_500_000, 16, new Vector3(0, 0, 1), new Vector3(1, 0, 0), new Vector3(0, 1, 0), 4, 0, 0, 0, 0, 2.2, 0.1),
            new PartInput(121, RuntimePartType.ContourPlate, "LongitudinalStiffener", "Q355B", "PL8", true, false, new Vector3(0, 120, 0), new Vector3(5200, 8, 120), new BoundingBox(new Vector3(-2600, 116, -60), new Vector3(2600, 124, 60)), 4_992_000, 1_350_000, 8, new Vector3(0, 1, 0), new Vector3(1, 0, 0), new Vector3(0, 0, 1), 4, 0, 0, 0, 0, 1.6, 0.0),
            new PartInput(201, RuntimePartType.ContourPlate, "BracketPlate", "Q355B", "PL20", true, false, new Vector3(500, 250, 210), new Vector3(300, 220, 20), new BoundingBox(new Vector3(350, 140, 200), new Vector3(650, 360, 220)), 1_320_000, 320_000, 20, new Vector3(0, 0, 1), new Vector3(0, 1, 0), new Vector3(1, 0, 0), 6, 1, 0, 1, 0, 1.1, 0.0),
            new PartInput(202, RuntimePartType.ContourPlate, "BracketRib", "Q355B", "PL12", true, false, new Vector3(520, 180, 120), new Vector3(180, 12, 200), new BoundingBox(new Vector3(430, 174, 20), new Vector3(610, 186, 220)), 432_000, 180_000, 12, new Vector3(0, 1, 0), new Vector3(0, 0, 1), new Vector3(1, 0, 0), 5, 0, 0, 0, 0, 0.9, 0.0)
        };

        var relationships = new[]
        {
            new RelationshipInput(101, 102, EdgeType.Weld, 0.95, 0.9, "web-topflange"),
            new RelationshipInput(101, 103, EdgeType.Weld, 0.95, 0.9, "web-bottomflange"),
            new RelationshipInput(101, 121, EdgeType.Weld, 0.70, 0.75, "web-stiffener"),
            new RelationshipInput(102, 201, EdgeType.Weld, 0.85, 0.8, "topflange-bracket"),
            new RelationshipInput(201, 202, EdgeType.Weld, 0.80, 0.7, "bracket-rib"),
            new RelationshipInput(102, 202, EdgeType.Contact, 0.45, 0.6, "rib-to-flange")
        };

        return new AssemblyInput("A-000123", 101, parts, relationships);
    }
}
