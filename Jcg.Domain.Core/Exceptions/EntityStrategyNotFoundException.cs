using System;

namespace Jcg.Domain.Core.Exceptions
{
    public class EntityStrategyNotFoundException : Exception
    {
        public EntityStrategyNotFoundException(Type requestedEntityType)
            : base(
               $"No IEntityStrategy implementation was found for type '{requestedEntityType.FullName}'. " +
                $"This may be because no class implements IEntityStrategy for this type, or the assembly containing such a class has not been scanned.")

        {

        }
    }
    
   
}