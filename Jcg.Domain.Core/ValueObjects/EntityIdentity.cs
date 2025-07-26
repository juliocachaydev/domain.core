using System;
using Jcg.Domain.Core.Exceptions;

namespace Jcg.Domain.Core.ValueObjects;

/// <summary>
/// Wraps a GUID that cannot be empty.
/// </summary>
public record EntityIdentity
{
    public Guid Value { get; }

    public EntityIdentity(Guid value)
    {
        if (value == Guid.Empty) throw new InvalidEntityStateException("Entity identity cannot be empty.");

        Value = value;
    }

    public static bool TryParse(string value, out EntityIdentity? entityIdentity)
    {
        if (Guid.TryParse(value, out var guidValue))
        {
            entityIdentity = new EntityIdentity(guidValue);
            return true;
        }

        entityIdentity = null;
        return false;
    }

    public static EntityIdentity Parse(string value)
    {
        if (TryParse(value, out var parsed)) return parsed;
        throw new InvalidEntityStateException($"Value: {value} cannot be parsed into a valid GUID.");
    }

    public static EntityIdentity Random => new(Guid.NewGuid());

    public override string ToString()
    {
        return Value.ToString();
    }
}