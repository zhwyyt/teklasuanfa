namespace TeklaBodyBracketRecognition.Core.Algorithms;

public sealed class BodyCandidatePartitionDecision
{
    public BodyCandidatePartitionDecision(
        string candidateId,
        BodyCandidatePartitionLane lane,
        BodyCandidatePartitionEvidenceCode[] evidenceCodes,
        string[] reviewReasons)
    {
        CandidateId = candidateId;
        Lane = lane;
        EvidenceCodes = evidenceCodes;
        ReviewReasons = reviewReasons;
    }

    public string CandidateId { get; }

    public BodyCandidatePartitionLane Lane { get; }

    public BodyCandidatePartitionEvidenceCode[] EvidenceCodes { get; }

    public string[] ReviewReasons { get; }

    public bool NeedsReview => Lane == BodyCandidatePartitionLane.ReviewRequired || ReviewReasons.Length > 0;
}
