namespace HA4IoT.Contracts.Services.Settings
{
    public interface ISettingsContainer<out TSettings>
    {
        string Uri { get; }

        TSettings Settings { get; }

        void CommitChanges();
    }
}
