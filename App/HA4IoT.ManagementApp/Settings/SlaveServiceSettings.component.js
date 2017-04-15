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

        ctrl.$onInit = function () {
            controllerProxyService.get("Service/ISettingsService/GetSettings", { "Uri": "ControllerSlaveServiceSettings" }, function (response) {
                ctrl.Model = response;
            });
        }

        ctrl.save = function () {
            var payload = {
                Uri: "ControllerSlaveServiceSettings",
                Settings: ctrl.Model
            }

            controllerProxyService.execute("Service/ISettingsService/Replace", payload);

            modalService.show("Info", "Controller Slave settings successfully saved.");
        }
    }

    module.component("slaveServiceSettings", {
        templateUrl: "Settings/SlaveServiceSettings.component.html",
        controllerAs: "sssCtrl",
        controller: ["controllerProxyService", "modalService", createController]
    });

})();