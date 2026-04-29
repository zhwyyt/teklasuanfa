using System;
using System.Collections.Generic;
using System.Linq;

namespace TeklaBodyBracketRecognition.App;

internal static class BodyCandidatePartitionConservativeFixtureDispatcherOutputFormatter
{
    public static void WriteHeader(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title must not be empty.", nameof(title));
        }

        Console.WriteLine(title);
    }

    public static void WriteValue(string label, string? value)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            throw new ArgumentException("Label must not be empty.", nameof(label));
        }

        Console.WriteLine($"{label}: {value ?? string.Empty}");
    }

    public static void WriteOptionalValue(string label, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        WriteValue(label, value);
    }

    public static void WriteValue(string label, bool value)
    {
        WriteValue(label, value.ToString());
    }

    public static void WriteValue(string label, int value)
    {
        WriteValue(label, value.ToString());
    }

    public static void WriteOptionalJoinedValues(
        string label,
        IEnumerable<string> values)
    {
        if (values is null)
        {
            return;
        }

        var filteredValues = values
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .ToArray();
        if (filteredValues.Length == 0)
        {
            return;
        }

        WriteValue(label, string.Join(", ", filteredValues));
    }
}
