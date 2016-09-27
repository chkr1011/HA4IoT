(function () {
    var module = angular.module("app");

    function createController(controllerProxyService) {

        var ctrl = this;

        ctrl.Model = {
            MasterAddress: "127.0.0.1",
            UseTemperature: false,
            UseHumidity: false,
            UseSunriseSunset: false,
            UseWeather: false,
            IsEnabled: false
        }

        ctrl.save = function () {
            var payload = {
                Uri: "Service/ControllerSlaveServiceSettings",
                Settings: ctrl.Model
            }

            controllerProxyService.execute("Service/ISettingsService/Replace", payload)

            alert("Saved Controller Slave settings");
        }
    }

    module.component("slaveServiceSettings", {
        templateUrl: "Settings/SlaveServiceSettings.component.html",
        controllerAs: "sssCtrl",
        controller: ["controllerProxyService", createController]
    });
    
})();