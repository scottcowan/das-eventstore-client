using System;

namespace ESFA.DAS.EventStore.Client.IntegrationTests.Models
{
    public class NullablePayload : TestPayload
    {
        public DateTime? StartDate
        {
            get;
            set;
        }
    }
}