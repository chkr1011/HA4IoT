using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using HA4IoT.Components;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware.Interrupts;
using HA4IoT.Contracts.Hardware.RemoteSockets;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Contracts.Scripting;
using HA4IoT.Contracts.Settings;
using HA4IoT.Settings;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Core
{
    public class Controller : IController
    {
        private readonly Container _container;
        private readonly ControllerOptions _options;

        private BackgroundTaskDeferral _deferral;
        private ILogger _log;

        public Controller(ControllerOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _container = new Container(options);

            StoragePath.Initialize(ApplicationData.Current.LocalFolder.Path, ApplicationData.Current.LocalFolder.Path);
        }

        public static bool IsRunningInUnitTest { get; set; }

        public IContainer Container => _container;

        public event EventHandler<StartupCompletedEventArgs> StartupCompleted;
        public event EventHandler<StartupFailedEventArgs> StartupFailed;

        public Task RunAsync(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance?.GetDeferral() ?? throw new ArgumentNullException(nameof(taskInstance));
            return RunAsync();
        }

        public Task RunAsync()
        {
            return Task.Run(RunAsyncInternal);
        }

        private async Task RunAsyncInternal()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _container.RegisterSingleton<IController>(() => this);
                _container.RegisterServices();
                _options.ContainerConfigurator?.ConfigureContainer(_container);
                _container.Verify();

                _log = _container.GetInstance<ILogService>().CreatePublisher(nameof(Controller));
                
                _container.GetInstance<IInterruptMonitorService>().RegisterInterrupts();
                _container.GetInstance<IDeviceRegistryService>().RegisterDevices();
                _container.GetInstance<IRemoteSocketService>().RegisterRemoteSockets();

                _container.StartupServices(_log);
                _container.ExposeRegistrationsToApi();

                await TryConfigureAsync();

                StartupCompleted?.Invoke(this, new StartupCompletedEventArgs(stopwatch.Elapsed));

                _container.GetInstance<IScriptingService>().TryExecuteStartupScripts();
            }
            catch (Exception exception)
            {
                StartupFailed?.Invoke(this, new StartupFailedEventArgs(stopwatch.Elapsed, exception));
                _deferral?.Complete();
            }
        }

        private async Task TryConfigureAsync()
        {
            try
            {
                _container.GetInstance<IApiDispatcherService>().ConfigurationRequested += (s, e) =>
                {
                    e.ApiContext.Result["Controller"] = JObject.FromObject(_container.GetInstance<ISettingsService>().GetSettings<ControllerSettings>());
                };

                await TryApplyCodeConfigurationAsync();

                _log.Info("Resetting all components");
                var componentRegistry = _container.GetInstance<IComponentRegistryService>();
                foreach (var component in componentRegistry.GetComponents())
                {
                    component.TryReset();
                }

            }
            catch (Exception exception)
            {
                _log.Error(exception, "Error while configuring");
                _container.GetInstance<INotificationService>().CreateError("Error while configuring.");
            }
        }

        private async Task TryApplyCodeConfigurationAsync()
        {
            try
            {
                if (_options.ConfigurationType == null)
                {
                    _log.Verbose("No configuration type is set.");
                    return;
                }

                var configuration = _container.GetInstance(_options.ConfigurationType) as IConfiguration;
                if (configuration == null)
                {
                    _log.Warning("Configuration is set but does not implement 'IConfiguration'.");
                    return;
                }

                _log.Info("Applying configuration");
                await configuration.ApplyAsync();
            }
            catch (Exception exception)
            {
                _log.Error(exception, "Error while applying code configuration");
                _container.GetInstance<INotificationService>().CreateError("Configuration code has failed.");
            }
        }
    }
}

