using System;
using SFA.DAS.Events.Api.Types;

namespace Esfa.Das.Eventstore.Client.Models
{
    public class ResourceCreationEvent<T> : IGenericEvent<T>
    {
        public long Id { get; set; }
        public string Type => typeof(ResourceCreationEvent<T>).FullName;

        public DateTime CreatedOn { get; set; }

        public T Payload { get; set; }
        public string ResourceUri { get; set; }

        public string ResourceType
        {
            get { return typeof(T).FullName; }
            set { }
        }

        public string ResourceId { get; set; }

        public ResourceCreationEvent()
        {
        }

        public ResourceCreationEvent(T payload, string resourceId)
        {
            Payload = payload;
            ResourceId = resourceId;
        }
    }
}