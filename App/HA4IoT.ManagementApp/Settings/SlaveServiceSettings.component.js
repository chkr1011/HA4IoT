(function () {
    var module = angular.module("app");

    function createController(controllerProxyService, modalService) {

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

            controllerProxyService.get("Service/ISettingsService/Replace", payload);

            modalService.show("Info", "Controller Slave settings successfully saved.");
        }
    }

    module.component("slaveServiceSettings", {
        templateUrl: "Settings/SlaveServiceSettings.component.html",
        controllerAs: "sssCtrl",
        controller: ["controllerProxyService", "modalService", createController]
    });
    
})();