namespace TeklaBodyBracketRecognition.Core.Domain;

public enum RuntimePartType
{
    Other,
    ContourPlate,
    Beam,
    PolyBeam,
    BentPlate
}

public enum PartSemanticRole
{
    Unknown,
    Other,
    WebCandidate,
    FlangeCandidate,
    WallCandidate,
    EndPlateCandidate,
    StiffenerCandidate,
    BracketCandidate
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
    StandardSection,
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

public enum BodyCandidatePartitionClass
{
    Unknown,
    BodyCandidate,
    BodyAccessoryCandidate,
    EndConnectionCandidate,
    LocalStiffenerCandidate,
    TinyPart,
    SpecialShape
}

public enum StableBodyZoneKind
{
    Stable,
    EndComplex,
    LocalComplex,
    Unstable
}

public enum SectionStationKind
{
    Core,
    Transition,
    LowPriority
}
