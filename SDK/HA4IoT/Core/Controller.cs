using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Notifications;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Logger;
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

        public Controller(ControllerOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            _options = options;

            StoragePath.Initialize(ApplicationData.Current.LocalFolder.Path);

            try
            {
                Debug.WriteLine(ApplicationData.Current.RoamingFolder.Path); // Sync with all accounts. Maybe best choice.
                File.AppendAllText(Path.Combine(ApplicationData.Current.RoamingFolder.Path, "Test.txt"), "Test");
                Debug.WriteLine("Content appended.");

                Debug.WriteLine(ApplicationData.Current.SharedLocalFolder.Path); // Activate in GP
                Debug.WriteLine(Windows.Storage.KnownFolders.RemovableDevices.Path); // Maybe?
                Debug.WriteLine(Windows.Storage.KnownFolders.DocumentsLibrary.Path); // Extensions must be known.
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.ToString());
            }

        }

        public Task RunAsync(IBackgroundTaskInstance taskInstance)
        {
            if (taskInstance == null) throw new ArgumentNullException(nameof(taskInstance));

            _deferral = taskInstance.GetDeferral();
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

                SetupLogger();

                Log.Info("Starting...");

                RegisterServices();
                TryConfigure();

                StartupServices();
                ExposeRegistrationsToApi();
                StartHttpServer();

                StartupCompleted?.Invoke(this, EventArgs.Empty);
                stopwatch.Stop();

                Log.Info("Startup completed after " + stopwatch.Elapsed);

                _container.GetInstance<ISystemInformationService>().Set("Health/StartupDuration", stopwatch.Elapsed);
                _container.GetInstance<ISystemInformationService>().Set("Health/StartupTimestamp", _container.GetInstance<IDateTimeService>().Now);

                _container.GetInstance<ITimerService>().Run();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Failed to initialize.");
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
            
            new MappedFolderController("App", StoragePath.AppRoot, httpServer).Enable();
            new MappedFolderController("ManagementApp", StoragePath.ManagementAppRoot, httpServer).Enable();

            httpServer.Bind(_options.HttpServerPort);
        }

        private void SetupLogger()
        {
            if (Log.Instance != null)
            {
                return;
            }

            var udpLogger = new UdpLogger();
            udpLogger.Start();
            
            Log.Instance = udpLogger;
        }

        private void StartupServices()
        {
            foreach (var registration in _container.GetRegistrationsOf<IService>())
            {
                ((IService)registration.GetInstance()).Startup();
            }
        }

        private void ExposeRegistrationsToApi()
        {
            var apiService = _container.GetInstance<IApiDispatcherService>();
            var settingsService = _container.GetInstance<ISettingsService>();

            foreach (var registration in _container.GetCurrentRegistrations())
            {
                apiService.Expose(registration.GetInstance());
            }

            apiService.ConfigurationRequested += (s, e) =>
            {
                var controllerSettings = settingsService.GetSettings<ControllerSettings>();
                e.Context.Response["Controller"] = JObject.FromObject(controllerSettings);
            };
        }

        private void RegisterServices()
        {
            _container.RegisterSingleton<IController>(() => this);
            _container.RegisterSingleton(() => _options);
            

            _container.RegisterServices();
            _options.ContainerConfigurator?.ConfigureContainer(_container);

            _container.Verify();
        }
        
        private void TryConfigure()
        {
            try
            {
                if (_options.ConfigurationType == null)
                {
                    Log.Warning("No configuration is set.");
                    return;
                }

                var configuration = _container.GetInstance(_options.ConfigurationType) as IConfiguration;
                if (configuration == null)
                {
                    Log.Warning("Configuration is set but does not implement 'IConfiguration'.");
                    return;
                }
                
                Log.Info("Applying configuration");
                configuration.ApplyAsync().Wait();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error while configuring");

                _container.GetInstance<INotificationService>().CreateError("Configuration is invalid");
            }
        }
    }
}

