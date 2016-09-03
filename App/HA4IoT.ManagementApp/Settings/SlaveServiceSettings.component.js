(function () {
    var module = angular.module("app");

    function createController(controllerProxyService) {

        this.Model = {
            MasterAddress: "127.0.0.1",
            UseTemperature: false,
            UseHumidity: false,
            UseSunriseSunset: false,
            UseWeather: false,
            IsEnabled: false
        }

        this.save = function () {
            var payload = {
                Uri: "Service/ControllerSlaveService",
                Settings: this.Model
            }

            controllerProxyService.invoke("Command", "Service/ISettingsService/Replace", payload)

            alert("Saved Controller Slave settings");
        }
    }

    module.component("slaveServiceSettings", {
        templateUrl: "Settings/SlaveServiceSettings.component.html",
        controllerAs: "sssCtrl",
        controller: ["controllerProxyService", createController]
    });
    
})();