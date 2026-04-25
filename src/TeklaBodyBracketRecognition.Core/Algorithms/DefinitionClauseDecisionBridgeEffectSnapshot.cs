using System.Collections.Generic;

namespace TeklaBodyBracketRecognition.Core.Algorithms;

public sealed class DefinitionClauseDecisionBridgeEffectSnapshot
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

    public string? LeadClauseCode { get; set; }

    public double LeadClauseShare { get; set; }

    public bool HasConflict { get; set; }

    public bool HasReviewConflict { get; set; }

    public bool RemovalBreaksClause { get; set; }

    public bool CanBorrowLeadClauseAnchor { get; set; }

    public IReadOnlyList<string> ConflictReasons { get; set; } = [];
}

public sealed class DefinitionClauseDecisionBridgeAdaptedResult
{
    public DefinitionClauseDecisionBridgeRawInputs RawInputs { get; set; } = new();

    public DefinitionClauseDecisionBridgeContext Context { get; set; } = new();

    public DefinitionClauseDecisionBridgeMapperResult Result { get; set; } = new();
}
