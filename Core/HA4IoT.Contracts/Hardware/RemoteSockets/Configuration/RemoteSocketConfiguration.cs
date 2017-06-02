namespace HA4IoT.Contracts.Hardware.RemoteSockets.Configuration
{
    public sealed class RemoteSocketConfiguration
    {
        public RemoteSocketAdapterConfiguration Adapter { get; set; } = new RemoteSocketAdapterConfiguration();

        public RemoteSocketCodeGeneratorConfiguration CodeGenerator { get; set; } = new RemoteSocketCodeGeneratorConfiguration();
    }
}
