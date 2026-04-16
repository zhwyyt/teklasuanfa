namespace TeklaBodyBracketRecognition.Core.Domain;

public enum RuntimePartType
{
    Other,
    ContourPlate,
    Beam,
    PolyBeam,
    BentPlate
}

public enum EdgeType
{
    Weld,
    Contact,
    Boolean,
    Bolt
}

public enum BodyType
{
    Unknown,
    WeldedPlateBody,
    ProfileBody
}

public enum BodyFamily
{
    Unknown,
    BuiltUpH,
    BuiltUpBox,
    BuiltUpT,
    BuiltUpCross,
    Irregular
}

public enum MatchType
{
    None,
    StandardProfile,
    BuiltUp,
    Irregular
}

public enum BracketType
{
    Unknown,
    SimplePlate,
    RibbedBracket,
    BoxBracket,
    IrregularBracket
}

public enum AttachedBodyFaceType
{
    Unknown,
    Web,
    FlangeTop,
    FlangeBottom,
    BoxWall,
    Corner
}
