namespace ESFA.DAS.EventStore.Client.IntegrationTests.Settings
{
    public interface IProvideSettings
    {
        string GetSetting(string settingKey);
    }
}