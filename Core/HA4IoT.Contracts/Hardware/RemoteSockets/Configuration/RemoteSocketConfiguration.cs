namespace HA4IoT.Contracts.Hardware.RemoteSockets.Configuration
{
    public class RemoteSocketConfiguration
    {
        public RemoteSocketAdapterConfiguration Adapter { get; set; } = new RemoteSocketAdapterConfiguration();

        public RemoteSocketCodeGeneratorConfiguration CodeGenerator { get; set; } = new RemoteSocketCodeGeneratorConfiguration();
    }
}
