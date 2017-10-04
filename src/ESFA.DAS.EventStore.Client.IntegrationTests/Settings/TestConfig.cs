using SFA.DAS.Events.Api.Client.Configuration;

namespace ESFA.DAS.EventStore.Client.IntegrationTests.Settings
{
    public class TestConfig : IEventsApiClientConfiguration
    {
        private readonly IProvideSettings _settings;

        public TestConfig() : this(new AppConfigSettingsProvider(new MachineSettings()))
        {
        }

        public TestConfig(IProvideSettings settings)
        {
            _settings = settings;
        }

        public string BaseUrl => _settings.GetSetting("EventApi:BaseUrl");
        public string ClientToken => _settings.GetSetting("EventApi:ClientToken");
    }
}