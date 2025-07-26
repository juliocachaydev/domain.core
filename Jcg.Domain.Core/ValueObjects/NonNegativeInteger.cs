using System;
using Jcg.Domain.Core.Exceptions;

namespace Jcg.Domain.Core.ValueObjects;

/// <summary>
/// Wraps a non negative integer (greater than or equal to zero)
/// </summary>
public record NonNegativeInteger
{
    public int Value { get; }

    public NonNegativeInteger(int value)
    {
        if (value < 0) throw new InvalidEntityStateException("Value must be greater than or equal to zero.");

        Value = value;
    }

    /// <summary>
    /// Generates a random Non Negative integer within the specified range, inclusive (includes min and max)
    /// </summary>
    public static NonNegativeInteger Random(int min = 0, int max = 1000)
    {
        if (min < 0 || max < min)
            throw new ArgumentOutOfRangeException(
                "Invalid range for Non Negative Integer. min must be at least 0, max must be greater than or equal to min.");

        var random = new Random();
        var value = random.Next(min, max + 1);
        return new NonNegativeInteger(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}