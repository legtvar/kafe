using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe;

/// <summary>
/// The lower and upper bound for the count of something.
/// </summary>
/// <remarks>
/// Inspired by System.CommandLine.ArgumentArity.
/// </remarks>
public readonly record struct ArgumentArity
{
    public const int MaximumArity = 100_000;

    public ArgumentArity(int min, int max)
    {
        if (min < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(min));
        }

        if (max < min)
        {
            throw new ArgumentException($"'{nameof(max)}' must be greater or equal than '{nameof(min)}'.");
        }

        Min = min;
        Max = max;
    }

    /// <summary>
    /// The inclusive lower bound.
    /// </summary>
    public int Min { get; init; }

    /// <summary>
    /// The inclusive upper bound.
    /// </summary>
    public int Max { get; init; }

    public static ArgumentArity Zero => new(0, 0);
    public static ArgumentArity ZeroOrOne => new(0, 1);
    public static ArgumentArity ExactlyOne => new(1, 1);
    public static ArgumentArity ZeroOrMore => new(0, MaximumArity);
    public static ArgumentArity OneOrMore => new(1, MaximumArity);
}
