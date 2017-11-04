using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Esfa.Das.Eventstore.Client;
using Esfa.Das.Eventstore.Client.Models;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Events.Api.Client;
using SFA.DAS.Events.Api.Client.Configuration;
using SFA.DAS.Events.Api.Types;

namespace ESFA.DAS.EventStore.Client.UnitTests
{
    [TestFixture]
    public class EventsApiExtensionsTests
    {
        private readonly TestPayload _example = new TestPayload
        {
            Id = 1,
            Name = "Example1",
            Title = "Example 1 Title"
        };

        private readonly TestPayload _exampleNameChange = new TestPayload
        {
            Id = 1,
            Name = "Example 1",
            Title = "Example 1 Title"
        };

        private readonly NullablePayload _nullablePayload = new NullablePayload
        {
            Id = 1,
            Name = "Nullable1",
            Title = "NullableTitle",
            StartDate = null
        };

        private readonly NullablePayload _nullablePayloadChange = new NullablePayload
        {
            Id = 1,
            Name = "Nullable1",
            Title = "NullableTitle",
            StartDate = DateTime.Today
        };

        [Test]
        public async Task ShouldSendACreateEvent()
        {
            // Arrange
            var mockClient = new Mock<IEventsApi>();
            mockClient.Setup(x => x.CreateGenericEvent(It.IsAny<IGenericEvent<TestPayload>>()))
                .Returns(Task.FromResult(0));

            // Act
            await mockClient.Object.SendCreationEvent(_example, _example.Id);

            // Assert
            mockClient.VerifyAll();
        }

        [Test]
        public async Task ShouldSendAChangeEventIfTheNameChanged()
        {
            // Arrange
            var mockClient = new Mock<IEventsApi>();
            mockClient.Setup(x => x.CreateGenericEvent(It.IsAny<IGenericEvent<IEnumerable<PropertyChange>>>()))
                .Returns(Task.FromResult(0));

            // Act
            await mockClient.Object.SendChangeEvent(_example, _exampleNameChange, _example.Id);

            // Assert
            mockClient.VerifyAll();
            mockClient.Verify(
                x =>
                    x.CreateGenericEvent(It.Is<IGenericEvent<IEnumerable<PropertyChange>>>(y => y.Payload.Count() == 1)),
                Times.Once);
        }

        [Test]
        public async Task ShouldntSendAChangeEventIfThereIsNoChange()
        {
            // Arrange
            var mockClient = new Mock<IEventsApi>();

            // Act
            await mockClient.Object.SendChangeEvent(_example, _example, _example.Id);

            // Assert
            mockClient.VerifyAll();
            mockClient.Verify(x => x.CreateGenericEvent(It.IsAny<IGenericEvent<IEnumerable<PropertyChange>>>()),
                Times.Never);
        }

        [Test]
        public async Task ShouldGetCreationEvents()
        {
            // Arrange
            var mockClient = new Mock<IEventsApi>();
            mockClient.Setup(
                x => x.GetGenericEventsById(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<GenericEvent> { CreationEvent(_example, _example.Id) });

            // Act
            var events = await mockClient.Object.GetCreations<TestPayload>();

            // Assert
            mockClient.VerifyAll();
            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(_example.Name, events[0].Payload.Name);
        }

        [Test]
        public async Task ShouldGetChangeEvents()
        {
            // Arrange
            var mockClient = new Mock<IEventsApi>();
            var change = new PropertyChange { OriginalValue = _example.Name, NewValue = _exampleNameChange.Name, PropertyName = "Name"};
            mockClient.Setup(
                x => x.GetGenericEventsById(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<GenericEvent> { ChangeEvent<TestPayload>(new List<PropertyChange>() { change }, _example.Id) });

            // Act
            var events = await mockClient.Object.GetChanges<TestPayload>();

            // Assert
            mockClient.VerifyAll();
            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(1, events[0].Payload.Count());
            Assert.AreEqual(change.NewValue, events[0].Payload.First().NewValue);
        }

        [Test]
        public async Task ShouldRetrieveAllTheEventsRelatedToAResource()
        {
            // Arrange
            var id = It.IsAny<int>();
            var change = new PropertyChange { OriginalValue = _example.Name, NewValue = _exampleNameChange.Name, PropertyName = "Name" };

            var mockClient = new Mock<IEventsApi>();
            mockClient.Setup(x => x.GetGenericEventsByResourceId(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<GenericEvent> { CreationEvent(_example, _example.Id), ChangeEvent<TestPayload>(new List<PropertyChange> {change}, _example.Id) });

            // Act
            var events = await mockClient.Object.GetResourceLifecycle<TestPayload>(id);

            // Assert
            mockClient.VerifyAll();
            Assert.IsNotNull(events.Creation);
            Assert.AreEqual(1, events.Changes.Count());
            Assert.AreEqual(change.PropertyName, events.Changes.First().Payload.First().PropertyName);
        }

        [Test]
        public async Task ShouldHandleNullableProperties()
        {
            var client = new Mock<IEventsApi>();

            client.Setup(x => x.CreateGenericEvent(It.IsAny<IGenericEvent<IEnumerable<PropertyChange>>>())).Returns(Task.FromResult(0))
                .Verifiable();

            await client.Object.SendChangeEvent(_nullablePayload, _nullablePayloadChange, _nullablePayload.Id);

            client.Verify(
                x =>
                    x.CreateGenericEvent(It.Is<IGenericEvent<IEnumerable<PropertyChange>>>(y => y.Payload.Count() == 1)),
                Times.Once);
        }

        private GenericEvent CreationEvent<T>(T payload, object resourceId)
        {
            return new GenericEvent
            {
                Id = 123,
                CreatedOn = DateTime.Now,
                Payload = JsonConvert.SerializeObject(payload),
                ResourceId = resourceId.ToString(),
                ResourceType = typeof(T).FullName,
                Type = typeof(ResourceCreationEvent<T>).FullName,
            };
        }

        private GenericEvent ChangeEvent<T>(IEnumerable<PropertyChange> payload, object resourceId)
        {
            return new GenericEvent
            {
                Id = 123,
                CreatedOn = DateTime.Now,
                Payload = JsonConvert.SerializeObject(payload),
                ResourceId = resourceId.ToString(),
                ResourceType = typeof(T).FullName,
                Type = typeof(ResourceChangeEvent<T>).FullName,
            };
        }
    }
}
