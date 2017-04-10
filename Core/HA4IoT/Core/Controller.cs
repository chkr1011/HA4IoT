using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using HA4IoT.Api;
using HA4IoT.Components;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services.Notifications;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Networking.Http;
using HA4IoT.Settings;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Core
{
    public class Controller : IController
    {
        private readonly Container _container = new Container();
        private readonly ControllerOptions _options;

        private BackgroundTaskDeferral _deferral;
        private ILogger _log;

        public Controller(ControllerOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            StoragePath.Initialize(ApplicationData.Current.LocalFolder.Path, ApplicationData.Current.LocalFolder.Path);
        }

        public Task RunAsync(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance?.GetDeferral() ?? throw new ArgumentNullException(nameof(taskInstance));
            return RunAsync();
        }

        public Task RunAsync()
        {
            var task = Task.Factory.StartNew(
                Startup,
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

            task.ConfigureAwait(false);
            return task;
        }

        public event EventHandler StartupCompleted;
        public event EventHandler StartupFailed;
        public event EventHandler Shutdown; 

        private void Startup()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();

                RegisterServices();
                StartHttpServer();

                _container.StartupServices(_log);
                _container.ExposeRegistrationsToApi();

                TryConfigure();
                
                StartupCompleted?.Invoke(this, EventArgs.Empty);
                stopwatch.Stop();

                _container.GetInstance<IApiDispatcherService>().ConfigurationRequested += (s, e) =>
                {
                    var controllerSettings = _container.GetInstance<ISettingsService>().GetSettings<ControllerSettings>();
                    e.ApiContext.Result["Controller"] = JObject.FromObject(controllerSettings);
                };

                _log.Info("Startup completed after " + stopwatch.Elapsed);

                _container.GetInstance<ISystemInformationService>().Set("Health/StartupDuration", stopwatch.Elapsed);
                _container.GetInstance<ISystemInformationService>().Set("Health/StartupTimestamp", _container.GetInstance<IDateTimeService>().Now);

                _container.GetInstance<ITimerService>().Run();
            }
            catch (Exception exception)
            {
                _log?.Error(exception, "Failed to initialize.");
                StartupFailed?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
                Shutdown?.Invoke(this, EventArgs.Empty);
                _deferral?.Complete();
            }
        }

        private void StartHttpServer()
        {
            var httpServer = _container.GetInstance<HttpServer>();
            
            new HttpDirectoryController("app", StoragePath.AppRoot, httpServer).Enable();
            new HttpDirectoryController("managementApp", StoragePath.ManagementAppRoot, httpServer).Enable();

            httpServer.Bind(_options.HttpServerPort);
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
        
        private void TryConfigure()
        {
            try
            {
                if (_options.ConfigurationType == null)
                {
                    _log.Warning("No configuration is set.");
                    return;
                }

                var configuration = _container.GetInstance(_options.ConfigurationType) as IConfiguration;
                if (configuration == null)
                {
                    _log.Warning("Configuration is set but does not implement 'IConfiguration'.");
                    return;
                }

                _log.Info("Applying configuration");
                configuration.ApplyAsync().Wait();

                _log?.Info("Resetting all components");
                var componentService = _container.GetInstance<IComponentRegistryService>();
                foreach (var component in componentService.GetComponents())
                {
                    component.TryReset();
                }
            }
            catch (Exception exception)
            {
                _log?.Error(exception, "Error while configuring");

                _container.GetInstance<INotificationService>().CreateError("Configuration is invalid");
            }
        }
    }
}

