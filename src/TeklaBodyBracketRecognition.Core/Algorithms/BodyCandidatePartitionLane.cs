namespace TeklaBodyBracketRecognition.Core.Algorithms;

public enum BodyCandidatePartitionLane
{
    BodyCoreCandidate = 0,
    BodySupportCandidate = 1,
    BodyAccessory = 2,
    ExternalContext = 3,
    ReviewRequired = 4,
}
