using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Das.Eventstore.Client;
using ESFA.DAS.EventStore.Client.IntegrationTests.Models;
using ESFA.DAS.EventStore.Client.IntegrationTests.Settings;
using NUnit.Framework;
using SFA.DAS.Events.Api.Client;

namespace ESFA.DAS.EventStore.Client.IntegrationTests
{
    [TestFixture]
    public class EventsApiExtensionsIntegrationTests
    {
        [Test]
        [Explicit]
        public async Task ShouldGetCreationEventsForAResourceTypeSinceLastCheckId()
        {
            var lastCheck = 0;
            var client = new EventsApi(new TestConfig());
            var id = new Random().Next();
            var model = new TestPayload
            {
                Id = id,
                Name = "Test person",
                Title = "title"
            };
            await client.SendCreationEvent(model, model.Id);
            var creations = await client.GetCreations<TestPayload>();
            Assert.IsTrue(creations.Any(x => x.ResourceId == id.ToString()));
        }

        [Test]
        [Explicit]
        public async Task ShouldCreateAPropertyChangeEvent()
        {
            var client = new EventsApi(new TestConfig());
            var id = new Random().Next();
            var model = new TestPayload
            {
                Id = id,
                Name = "Test person",
                Title = "title"
            };
            await client.SendCreationEvent(model, model.Id);

            var model2 = new TestPayload
            {
                Id = id,
                Name = "Test person 1",
                Title = "title1"
            };

            await client.SendChangeEvent(model, model2, model.Id);

            var life = await client.GetResourceLifecycle<TestPayload>(model.Id);
            Assert.IsNotNull(life.Creation);
            Assert.AreEqual(1, life.Changes.Count());
            Assert.AreEqual(2, life.Changes.First().Payload.Count());
        }

    }
}
