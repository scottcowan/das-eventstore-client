using System;

namespace ESFA.DAS.EventStore.Client.UnitTests
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