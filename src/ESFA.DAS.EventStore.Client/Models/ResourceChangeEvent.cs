using System;
using System.Collections.Generic;
using SFA.DAS.Events.Api.Types;

namespace Esfa.Das.Eventstore.Client.Models
{
    public class ResourceChangeEvent<T> : IGenericEvent<IEnumerable<PropertyChange>>
    {
        public long Id { get; set; }
        public string Type => typeof(ResourceChangeEvent<T>).FullName;
        public DateTime CreatedOn { get; set; }
        public IEnumerable<PropertyChange> Payload { get; set; }
        public string ResourceUri { get; set; }
        public string ResourceType => typeof(T).FullName;
        public string ResourceId { get; set; }
    }
}