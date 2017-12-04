using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Esfa.Das.Eventstore.Client.Models;
using Newtonsoft.Json;
using SFA.DAS.Events.Api.Client;
using SFA.DAS.Events.Api.Types;

namespace Esfa.Das.Eventstore.Client
{
    public static class EventsApiClientExtensions
    {
        public static async Task SendCreationEvent<T>(this IEventsApi client, T model, object id) where T : class
        {
            await client.CreateGenericEvent(new ResourceCreationEvent<T>(model, id.ToString()));
        }

        /// <summary>
        /// Send a change event if there's a difference
        /// </summary>
        /// <typeparam name="T">The type of the model</typeparam>
        /// <param name="client">The events api client</param>
        /// <param name="original">The orginal model</param>
        /// <param name="updated">The updated model to diff</param>
        /// <param name="id">The id of the model</param>
        /// <returns></returns>
        public static async Task SendChangeEvent<T>(this IEventsApi client, T original, T updated, object id) where T : class
        {
            var ger = Compare(original, updated, id);
            if (ger != null)
            {
                await client.CreateGenericEvent(ger);
            }
        }

        public static async Task<ResourceLifeEvents<T>> GetResourceLifecycle<T>(this IEventsApi client, object id, int pageSize = 1000, int pageNumber = 1) where T : class
        {
            var events = await client.GetGenericEventsByResourceId(typeof(T).FullName, id.ToString(), pageSize: pageSize, pageNumber: pageNumber);
            return MapEvents<T>(events);
        }

        public static async Task<List<ResourceCreationEvent<T>>> GetCreations<T>(this IEventsApi client, long fromEventId = 0, int pageSize = 1000, int pageNumber = 1) where T : class
        {
            var genericEvents = await client.GetGenericEventsById(typeof(ResourceCreationEvent<T>).FullName, fromEventId, pageSize, pageNumber);
            return genericEvents.Select(MapCreateEvent<T>).ToList();
        }

        public static async Task<List<ResourceChangeEvent<T>>> GetChanges<T>(this IEventsApi client, Expression<Func<T, object>> property = null, long fromEventId = 0, int pageSize = 1000, int pageNumber = 1) where T : class
        {
            var genericEvents = await client.GetGenericEventsById(typeof(ResourceChangeEvent<T>).FullName, fromEventId, pageSize, pageNumber);
            var mapped = genericEvents.Select(MapPropertyChangeEvent<T>);
            return (property != null ? mapped.Where(x => x.Payload.Any(y => y.PropertyName == GetPropertyName(property))) : mapped).ToList();
        }

        private static ResourceLifeEvents<T> MapEvents<T>(List<GenericEvent> events)
        {
            return new ResourceLifeEvents<T>
            {
                Creation = events.Where(x => x.Type == typeof(ResourceCreationEvent<T>).FullName).Select(MapCreateEvent<T>).FirstOrDefault(),
                Changes = events.Where(x => x.Type == typeof(ResourceChangeEvent<T>).FullName).Select(MapPropertyChangeEvent<T>)
            };
        }

        private static ResourceCreationEvent<T> MapCreateEvent<T>(GenericEvent arg)
        {
            return new ResourceCreationEvent<T>
            {
                Payload = JsonConvert.DeserializeObject<T>(arg.Payload),
                CreatedOn = arg.CreatedOn,
                Id = arg.Id,
                ResourceId = arg.ResourceId
            };
        }

        private static ResourceChangeEvent<T> MapPropertyChangeEvent<T>(GenericEvent arg)
        {
            return new ResourceChangeEvent<T>
            {
                Payload = JsonConvert.DeserializeObject<IEnumerable<PropertyChange>>(arg.Payload),
                CreatedOn = arg.CreatedOn,
                Id = arg.Id,
                ResourceId = arg.ResourceId
            };
        }
        private static string GetPropertyName<T>(Expression<Func<T, object>> action) where T : class
        {
            var expression = (MemberExpression)action.Body;
            return expression.Member.Name;
        }

        private static ResourceChangeEvent<T> Compare<T>(T original, T updated, object id)
        {
            var changes = FindChanges(original, updated).ToList();
            if (!changes.Any())
            {
                return null;
            }

            return new ResourceChangeEvent<T>()
            {
                Payload = changes,
                ResourceId = id.ToString()
            };
        }

        private static IEnumerable<PropertyChange> FindChanges<T>(T original, T updated)
        {
            foreach (var property in typeof(T).GetProperties().Where(x => x.GetMethod != null && x.GetMethod.IsPublic && (x.PropertyType.IsValueType || x.PropertyType == typeof(string))))
            {
                var isNullable = property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>);

                var originalValue = property.GetMethod.Invoke(original, isNullable?null:new object[] { });
                var updatedValue = property.GetMethod.Invoke(updated, isNullable?null:new object[] { });

                if (originalValue== null && updatedValue != null || originalValue != null && updatedValue != null && !originalValue.Equals(updatedValue))
                {
                    yield return new PropertyChange
                    {
                        PropertyName = property.Name,
                        PropertyType = property.PropertyType,
                        OriginalValue = originalValue,
                        NewValue = updatedValue
                    };
                }
            }
        }
    }
}