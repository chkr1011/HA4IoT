using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using HA4IoT.Components;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Scripting;
using HA4IoT.Contracts.Services.Notifications;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Net.Http;
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

        public event EventHandler StartupCompleted;
        public event EventHandler StartupFailed;

        public Task RunAsync(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance?.GetDeferral() ?? throw new ArgumentNullException(nameof(taskInstance));
            return RunAsync();
        }

        public Task RunAsync()
        {
            return Task.Run(() => StartupAsync());
        }

        private async void StartupAsync()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();

                RegisterServices();

                var httpServer = _container.GetInstance<HttpServer>();
                await httpServer.BindAsync(_options.HttpServerPort);

                _container.StartupServices(_log);
                _container.ExposeRegistrationsToApi();

                await TryConfigureAsync();

                StartupCompleted?.Invoke(this, EventArgs.Empty);
                stopwatch.Stop();

                _container.GetInstance<IApiDispatcherService>().ConfigurationRequested += (s, e) =>
                {
                    e.ApiContext.Result["Controller"] = JObject.FromObject(_container.GetInstance<ISettingsService>().GetSettings<ControllerSettings>());
                };

                _log.Info("Startup completed after " + stopwatch.Elapsed);

                _container.GetInstance<ISystemInformationService>().Set("Health/StartupDuration", stopwatch.Elapsed);
                _container.GetInstance<ISystemInformationService>().Set("Health/StartupTimestamp", _container.GetInstance<IDateTimeService>().Now);
            }
            catch (Exception exception)
            {
                _log?.Error(exception, "Failed to initialize.");
                StartupFailed?.Invoke(this, EventArgs.Empty);

                _deferral?.Complete();
            }
        }

        private void RegisterServices()
        {
            _container.RegisterSingleton<IController>(() => this);
            _container.RegisterSingleton(() => _options);

            _container.RegisterServices();
            _options.ContainerConfigurator?.ConfigureContainer(_container);

            _container.Verify();
            _log = _container.GetInstance<ILogService>().CreatePublisher(nameof(Controller));

            _log.Info("Services registered.");
        }

        private async Task TryConfigureAsync()
        {
            try
            {
                await TryApplyCodeConfigurationAsync();
                TryApplyScriptConfiguration();
                
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

        private void TryApplyScriptConfiguration()
        {
            try
            {
                var configurationScriptFile = Path.Combine(StoragePath.StorageRoot, "Configuration.lua");
                if (!File.Exists(configurationScriptFile))
                {
                    _log.Verbose("Configuration script not found.");
                    return;
                }

                var scriptCode = File.ReadAllText(configurationScriptFile);
                _container.GetInstance<IScriptingService>().ExecuteScript(scriptCode, "applyConfiguration");
            }
            catch (Exception exception)
            {
                _log.Error(exception, "Error while applying script configuration");
                _container.GetInstance<INotificationService>().CreateError("Configuration script has failed.");
            }
        }
    }
}

