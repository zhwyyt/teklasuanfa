namespace TeklaBodyBracketRecognition.Core.Algorithms;

public static class BodyCandidatePartitionConservativeFixtures
{
    public static BodyCandidatePartitionConservativeFixture[] CreateDefault()
    {
        return
        [
            new BodyCandidatePartitionConservativeFixture(
                name: "CORE_STABLE_SPAN_AND_MAIN_CONTOUR",
                snapshot: new BodyCandidatePartitionSignalSnapshot(
                    candidateId: "fixture-core-001",
                    hasStableZoneSpanControl: true,
                    hasMainContourControl: true,
                    hasClosedLoopControl: false,
                    hasSingleMainPlateContinuity: false,
                    supportsBodyCoreAdjacency: false,
                    matchesBodyAccessoryTopology: false,
                    hasExternalContactOnly: false,
                    hasConflictingTopology: false,
                    hasInsufficientTrace: false,
                    reviewReasons: []),
                expectedLane: BodyCandidatePartitionLane.BodyCoreCandidate,
                expectedEvidenceCodes:
                [
                    BodyCandidatePartitionEvidenceCode.StableZoneSpan,
                    BodyCandidatePartitionEvidenceCode.MainContourControl,
                ],
                expectedNeedsReview: false,
                expectedReviewReasonSubstrings: []),

            new BodyCandidatePartitionConservativeFixture(
                name: "SUPPORT_CONTINUITY_AND_ADJACENCY",
                snapshot: new BodyCandidatePartitionSignalSnapshot(
                    candidateId: "fixture-support-001",
                    hasStableZoneSpanControl: false,
                    hasMainContourControl: false,
                    hasClosedLoopControl: false,
                    hasSingleMainPlateContinuity: true,
                    supportsBodyCoreAdjacency: true,
                    matchesBodyAccessoryTopology: false,
                    hasExternalContactOnly: false,
                    hasConflictingTopology: false,
                    hasInsufficientTrace: false,
                    reviewReasons: []),
                expectedLane: BodyCandidatePartitionLane.BodySupportCandidate,
                expectedEvidenceCodes:
                [
                    BodyCandidatePartitionEvidenceCode.SingleMainPlateContinuity,
                    BodyCandidatePartitionEvidenceCode.BodySupportAdjacency,
                ],
                expectedNeedsReview: false,
                expectedReviewReasonSubstrings: []),

            new BodyCandidatePartitionConservativeFixture(
                name: "ACCESSORY_TOPOLOGY_ONLY",
                snapshot: new BodyCandidatePartitionSignalSnapshot(
                    candidateId: "fixture-accessory-001",
                    hasStableZoneSpanControl: false,
                    hasMainContourControl: false,
                    hasClosedLoopControl: false,
                    hasSingleMainPlateContinuity: false,
                    supportsBodyCoreAdjacency: false,
                    matchesBodyAccessoryTopology: true,
                    hasExternalContactOnly: false,
                    hasConflictingTopology: false,
                    hasInsufficientTrace: false,
                    reviewReasons: []),
                expectedLane: BodyCandidatePartitionLane.BodyAccessory,
                expectedEvidenceCodes:
                [
                    BodyCandidatePartitionEvidenceCode.BodyAccessoryTopology,
                ],
                expectedNeedsReview: false,
                expectedReviewReasonSubstrings: []),

            new BodyCandidatePartitionConservativeFixture(
                name: "EXTERNAL_CONTACT_ONLY",
                snapshot: new BodyCandidatePartitionSignalSnapshot(
                    candidateId: "fixture-external-001",
                    hasStableZoneSpanControl: false,
                    hasMainContourControl: false,
                    hasClosedLoopControl: false,
                    hasSingleMainPlateContinuity: false,
                    supportsBodyCoreAdjacency: false,
                    matchesBodyAccessoryTopology: false,
                    hasExternalContactOnly: true,
                    hasConflictingTopology: false,
                    hasInsufficientTrace: false,
                    reviewReasons: []),
                expectedLane: BodyCandidatePartitionLane.ExternalContext,
                expectedEvidenceCodes:
                [
                    BodyCandidatePartitionEvidenceCode.ExternalContactOnly,
                ],
                expectedNeedsReview: false,
                expectedReviewReasonSubstrings: []),

            new BodyCandidatePartitionConservativeFixture(
                name: "REVIEW_ACCESSORY_CORE_CONFLICT",
                snapshot: new BodyCandidatePartitionSignalSnapshot(
                    candidateId: "fixture-review-001",
                    hasStableZoneSpanControl: false,
                    hasMainContourControl: true,
                    hasClosedLoopControl: false,
                    hasSingleMainPlateContinuity: false,
                    supportsBodyCoreAdjacency: false,
                    matchesBodyAccessoryTopology: true,
                    hasExternalContactOnly: false,
                    hasConflictingTopology: false,
                    hasInsufficientTrace: false,
                    reviewReasons: []),
                expectedLane: BodyCandidatePartitionLane.ReviewRequired,
                expectedEvidenceCodes:
                [
                    BodyCandidatePartitionEvidenceCode.MainContourControl,
                    BodyCandidatePartitionEvidenceCode.BodyAccessoryTopology,
                    BodyCandidatePartitionEvidenceCode.ConflictingTopology,
                ],
                expectedNeedsReview: true,
                expectedReviewReasonSubstrings:
                [
                    "Accessory topology conflicts",
                ]),

            new BodyCandidatePartitionConservativeFixture(
                name: "REVIEW_INSUFFICIENT_TRACE",
                snapshot: new BodyCandidatePartitionSignalSnapshot(
                    candidateId: "fixture-review-002",
                    hasStableZoneSpanControl: false,
                    hasMainContourControl: false,
                    hasClosedLoopControl: false,
                    hasSingleMainPlateContinuity: false,
                    supportsBodyCoreAdjacency: false,
                    matchesBodyAccessoryTopology: false,
                    hasExternalContactOnly: false,
                    hasConflictingTopology: false,
                    hasInsufficientTrace: true,
                    reviewReasons: []),
                expectedLane: BodyCandidatePartitionLane.ReviewRequired,
                expectedEvidenceCodes:
                [
                    BodyCandidatePartitionEvidenceCode.InsufficientTrace,
                ],
                expectedNeedsReview: true,
                expectedReviewReasonSubstrings: []),
        ];
    }
}
