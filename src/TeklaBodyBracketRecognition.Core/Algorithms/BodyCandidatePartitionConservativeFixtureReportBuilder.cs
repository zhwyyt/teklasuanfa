namespace TeklaBodyBracketRecognition.Core.Algorithms;

public static class BodyCandidatePartitionConservativeFixtureReportBuilder
{
    public static string BuildMarkdown(
        BodyCandidatePartitionConservativeFixtureResult[] results)
    {
        var builder = new System.Text.StringBuilder();
        var total = results.Length;
        var passed = results.Count(result => result.IsSuccess);
        var failed = total - passed;

        builder.AppendLine("# Body Candidate Partition Conservative Fixture Report");
        builder.AppendLine();
        builder.AppendLine($"Total: {total}");
        builder.AppendLine($"Passed: {passed}");
        builder.AppendLine($"Failed: {failed}");
        builder.AppendLine();
        builder.AppendLine("| Fixture | Expected Lane | Actual Lane | Needs Review | Result |");
        builder.AppendLine("| --- | --- | --- | --- | --- |");

        foreach (var result in results)
        {
            builder.Append("| ");
            builder.Append(result.FixtureName);
            builder.Append(" | ");
            builder.Append(result.ExpectedLane);
            builder.Append(" | ");
            builder.Append(result.Decision.Lane);
            builder.Append(" | ");
            builder.Append(result.Decision.NeedsReview ? "yes" : "no");
            builder.Append(" | ");
            builder.Append(result.IsSuccess ? "PASS" : "FAIL");
            builder.AppendLine(" |");
        }

        foreach (var result in results)
        {
            builder.AppendLine();
            builder.AppendLine($"## {result.FixtureName}");
            builder.AppendLine();
            builder.AppendLine($"Expected lane: `{result.ExpectedLane}`");
            builder.AppendLine();
            builder.AppendLine($"Actual lane: `{result.Decision.Lane}`");
            builder.AppendLine();
            builder.AppendLine($"Needs review: `{result.Decision.NeedsReview}`");
            builder.AppendLine();
            builder.AppendLine("Evidence codes:");
            builder.AppendLine();

            foreach (var evidenceCode in result.Decision.EvidenceCodes)
            {
                builder.AppendLine($"- `{evidenceCode}`");
            }

            if (result.Decision.ReviewReasons.Length > 0)
            {
                builder.AppendLine();
                builder.AppendLine("Review reasons:");
                builder.AppendLine();

                foreach (var reviewReason in result.Decision.ReviewReasons)
                {
                    builder.AppendLine($"- {reviewReason}");
                }
            }

            if (result.MissingEvidenceCodes.Length > 0)
            {
                builder.AppendLine();
                builder.AppendLine("Missing expected evidence codes:");
                builder.AppendLine();

                foreach (var evidenceCode in result.MissingEvidenceCodes)
                {
                    builder.AppendLine($"- `{evidenceCode}`");
                }
            }

            if (result.MissingReviewReasonSubstrings.Length > 0)
            {
                builder.AppendLine();
                builder.AppendLine("Missing expected review reason fragments:");
                builder.AppendLine();

                foreach (var missingReviewReasonSubstring in result.MissingReviewReasonSubstrings)
                {
                    builder.AppendLine($"- `{missingReviewReasonSubstring}`");
                }
            }

            if (result.IsSuccess)
            {
                builder.AppendLine();
                builder.AppendLine("Status: PASS");
            }
            else
            {
                builder.AppendLine();
                builder.AppendLine("Status: FAIL");
            }
        }

        return builder.ToString().TrimEnd();
    }
}
