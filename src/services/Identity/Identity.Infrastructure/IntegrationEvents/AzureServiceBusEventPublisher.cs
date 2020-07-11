using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Laso.Identity.Core.Extensions;
using Laso.Identity.Core.IntegrationEvents;
using Laso.Identity.Infrastructure.Extensions;
using Microsoft.Azure.ServiceBus;

namespace Laso.Identity.Infrastructure.IntegrationEvents
{
    public class AzureServiceBusEventPublisher : IEventPublisher
    {
        private readonly AzureServiceBusTopicProvider _topicProvider;

        public AzureServiceBusEventPublisher(AzureServiceBusTopicProvider topicProvider)
        {
            _topicProvider = topicProvider;
        }

        public async Task Publish<T>(T @event) where T : IIntegrationEvent
        {
            var client = await _topicProvider.GetTopicClient(@event.GetType());

            var bytes = JsonSerializer.SerializeToUtf8Bytes(@event);

            var message = new Message(bytes);

            GetUserProperties(@event, new HashSet<object>()).ForEach(x => message.UserProperties.Add(x.Name, x.Value));

            await client.SendAsync(message);
        }

        private static IEnumerable<(string Name, string Value)> GetUserProperties(object value, ISet<object> visitedObjects)
        {
            if (value == null || visitedObjects.Contains(value))
                yield break;

            visitedObjects.Add(value);

            foreach (var property in value.GetType().GetProperties())
            {
                var attribute = property.GetCustomAttribute<EnvelopePropertyAttribute>();

                if (attribute != null)
                    yield return (attribute.Name ?? property.Name, property.GetValue(value).ToString()); //TODO: use FilterPropertyMapper.MapToQueryParameter?
                else if (!property.PropertyType.IsValueType() && !property.PropertyType.Closes(typeof(IEnumerable<>)))
                    foreach (var userProperty in GetUserProperties(property.GetValue(value), visitedObjects))
                        yield return userProperty;
            }
        }
    }
}
