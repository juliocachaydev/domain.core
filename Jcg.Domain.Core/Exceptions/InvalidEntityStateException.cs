using System;

namespace Jcg.Domain.Core.Exceptions;

public class InvalidEntityStateException : InvalidOperationException
{
    public InvalidEntityStateException(string errorMessage) : base(errorMessage)
    {
        
    }
}