using System;

namespace Jcg.Domain.Core.Exceptions;

/// <summary>
/// An invariant was broken.
/// </summary>
public class InvalidEntityStateException : InvalidOperationException
{
    public InvalidEntityStateException(string errorMessage) : base(errorMessage)
    {
    }
}