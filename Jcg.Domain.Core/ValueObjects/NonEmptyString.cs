using System;
using Jcg.Domain.Core.Exceptions;

namespace Jcg.Domain.Core.ValueObjects;

/// <summary>
/// Wraps a String that cannot be empty nor whitespace, and is trimmed on both ends.
/// </summary>
public record NonEmptyString
{
    public string Value { get; }

    public NonEmptyString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidEntityStateException("Value cannot be empty nor whitespace.");

        Value = value.Trim();
    }

    public static NonEmptyString Random(int length = 10)
    {
        if (length < 1)
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be at least 1.");

        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        var result = new char[length];

        for (var i = 0; i < length; i++) result[i] = chars[random.Next(chars.Length)];

        return new NonEmptyString(new string(result));
    }

    public override string ToString()
    {
        return Value;
    }
}