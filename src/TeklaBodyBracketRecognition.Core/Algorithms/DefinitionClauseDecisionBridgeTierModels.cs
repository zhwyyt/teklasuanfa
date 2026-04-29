namespace TeklaBodyBracketRecognition.Core.Algorithms;

public enum DefinitionClauseDecisionBridgeSourceTier
{
    RecognitionOnly = 0,
    DualConsistent = 1,
    RealPrimary = 2,
}

public enum DefinitionClauseDecisionBridgeStationTier
{
    Episodic = 0,
    LocalOnly = 1,
    PriorityMinor = 2,
    PriorityMajority = 3,
}

public enum DefinitionClauseDecisionBridgeTopologyTier
{
    None = 0,
    WeakChange = 1,
    DimensionSwitch = 2,
    RewriteStructural = 3,
    MainContourBreak = 4,
    LostEnvelope = 5,
    LostClosedLoop = 6,
}

public enum DefinitionClauseDecisionBridgeProofCompletenessTier
{
    Unresolved = 0,
    TypeOnly = 1,
    TargetOnly = 2,
    Complete = 3,
}

public enum DefinitionClauseDecisionBridgeLeadClauseTier
{
    Unset = 0,
    Weak = 1,
    Dominant = 2,
    StableFull = 3,
}

public enum DefinitionClauseDecisionBridgeCandidateDirection
{
    Unknown = 0,
    SatisfiedCandidate = 1,
    BrokenCandidate = 2,
    MixedCandidate = 3,
}

public enum DefinitionClauseDecisionBridgeVerdict
{
    InsufficientEvidence = 0,
    Satisfied = 1,
    Broken = 2,
    Mixed = 3,
    ReviewRequired = 4,
}

public enum DefinitionClauseDecisionBridgePromotionReadiness
{
    HoldEffectOnly = 0,
    ReadyForReview = 1,
    ReadyForPromotion = 2,
}

public sealed class DefinitionClauseDecisionBridgeContext
{
    public DefinitionClauseDecisionBridgeSourceTier SourceTier { get; set; }

    public DefinitionClauseDecisionBridgeStationTier StationTier { get; set; }

    public DefinitionClauseDecisionBridgeTopologyTier TopologyTier { get; set; }

    public DefinitionClauseDecisionBridgeProofCompletenessTier ProofCompletenessTier { get; set; }

    public DefinitionClauseDecisionBridgeLeadClauseTier LeadClauseTier { get; set; }

    public bool HasConflict { get; set; }

    public bool HasReviewConflict { get; set; }

    public bool RemovalBreaksClause { get; set; }

    public bool CanBorrowLeadClauseAnchor { get; set; }
}

public sealed class DefinitionClauseDecisionBridgeRawInputs
{
    public bool HasRealInputEvidence { get; set; }

    public bool HasRecognitionInputEvidence { get; set; }

    public int PriorityStationCount { get; set; }

    public int PriorityStationHitCount { get; set; }

    public bool HasLocalOnlyEvidence { get; set; }

    public bool HasLostClosedLoop { get; set; }

    public bool HasLostEnvelopeSupport { get; set; }

    public bool HasStructuralTopologyRewrite { get; set; }

    public bool HasDominantDimensionSwitch { get; set; }

    public bool HasWeakTopologyChange { get; set; }

    public bool HasMainContourBreak { get; set; }

    public bool HasProofType { get; set; }

    public bool HasFamilyProofTarget { get; set; }

    public double LeadClauseShare { get; set; }

    public bool HasLeadClause { get; set; }

    public bool HasConflict { get; set; }

    public bool HasReviewConflict { get; set; }

    public bool RemovalBreaksClause { get; set; }

    public bool CanBorrowLeadClauseAnchor { get; set; }
}

public sealed class DefinitionClauseDecisionBridgeMapperResult
{
    public DefinitionClauseDecisionBridgeCandidateDirection CandidateDirection { get; set; }

    public DefinitionClauseDecisionBridgeVerdict Verdict { get; set; }

    public DefinitionClauseDecisionBridgePromotionReadiness PromotionReadiness { get; set; }
}
