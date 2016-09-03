(function () {
    var module = angular.module("app");

    function createController(controllerProxyService) {

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
            controllerProxyService.get("Service/ISettingsService/Settings", { "Uri": "OpenWeatherMapServiceSettings" }, function(response) {
                ctrl.Model = response;
            });
        }

        ctrl.save = function () {
            var payload = {
                Uri: "OpenWeatherMapServiceSettings",
                Settings: ctrl.Model
            }

            controllerProxyService.execute("Service/ISettingsService/Replace", payload);

            alert("Saved Open Weahter Map settings");
        }
    }

    module.component("openWeatherMapSettings", {
        templateUrl: "Settings/OpenWeatherMapSettings.component.html",
        controllerAs: "owmsCtrl",
        controller: ["controllerProxyService", createController]
    });
    
})();