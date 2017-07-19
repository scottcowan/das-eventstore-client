using System.Collections.Generic;

namespace Esfa.Das.Eventstore.Client.Models
{
    public class ResourceLifeEvents<T>
    {
        public ResourceCreationEvent<T> Creation { get; set; }
        public IEnumerable<ResourceChangeEvent<T>> Changes { get; set; }
    }
}