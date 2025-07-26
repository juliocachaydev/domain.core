using System;
using Jcg.Domain.Core.Exceptions;

namespace Jcg.Domain.Core.ValueObjects;

/// <summary>
/// Wraps a positive integer (greater than zero)
/// </summary>
public record PositiveInteger
{
    public int Value { get; }

    public PositiveInteger(int value)
    {
        if (value < 1) throw new InvalidEntityStateException("Value must be greater than zero.");

        Value = value;
    }

    /// <summary>
    /// Generates a random PositiveInteger within the specified range, inclusive (includes min and max)
    /// </summary>
    public static PositiveInteger Random(int min = 1, int max = 1000)
    {
        if (min < 1 || max < min)
            throw new ArgumentOutOfRangeException(
                "Invalid range for PositiveInteger. min must be at least 1, max must be greater than or equal to min.");

        var random = new Random();
        var value = random.Next(min, max + 1);
        return new PositiveInteger(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}