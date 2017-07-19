using System;

namespace Esfa.Das.Eventstore.Client.Models
{
    public class PropertyChange
    {
        public string PropertyName { get; set; }
        public object OriginalValue { get; set; }
        public object NewValue { get; set; }
        public Type PropertyType { get; set; }
    }
}