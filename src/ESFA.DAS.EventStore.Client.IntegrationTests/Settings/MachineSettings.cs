using System;
using System.Globalization;

namespace ESFA.DAS.EventStore.Client.IntegrationTests.Settings
{
    public sealed class MachineSettings : IProvideSettings
    {
        public string GetSetting(string settingKey)
        {
            return Environment.GetEnvironmentVariable($"DAS_{settingKey.ToUpper(CultureInfo.InvariantCulture)}", EnvironmentVariableTarget.User);
        }
    }
}