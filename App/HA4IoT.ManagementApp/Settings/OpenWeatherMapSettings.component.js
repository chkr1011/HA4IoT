(function () {
    var module = angular.module("app");

    function createController(controllerProxyService, modalService) {

        var ctrl = this;

        ctrl.Model = {
            Latitude: "",
            Longitude: "",
            AppId: "",
            UseTemperature: true,
            UseHumidity: true,
            UseSunriseSunset: true,
            UseWeather: true,
            IsEnabled: true
        }

        ctrl.$onInit = function () {
            controllerProxyService.get("Service/ISettingsService/GetSettings", { "Uri": "OpenWeatherMapServiceSettings" }, function (response) {
                ctrl.Model = response;
            });
        }

        ctrl.save = function () {
            var payload = {
                Uri: "OpenWeatherMapServiceSettings",
                Settings: ctrl.Model
            }

            controllerProxyService.execute("Service/ISettingsService/Replace", payload);

            modalService.show("Info", "Open Weahter Map settings successfully saved.");
        }
    }

    module.component("openWeatherMapSettings", {
        templateUrl: "Settings/OpenWeatherMapSettings.component.html",
        controllerAs: "owmsCtrl",
        controller: ["controllerProxyService", "modalService", createController]
    });
    
})();