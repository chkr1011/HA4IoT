(function () {
    var module = angular.module("app");

    function createController(controllerProxyService, modalService) {
        var c = this;

        c.Model = {
            Latitude: "",
            Longitude: "",
            AppId: "",
            UseTemperature: true,
            UseHumidity: true,
            UseSunriseSunset: true,
            UseWeather: true,
            IsEnabled: true
        }

        c.$onInit = function () {
            controllerProxyService.get("Service/ISettingsService/GetSettings", { "Uri": "OpenWeatherMapServiceSettings" }, function (response) {
                c.Model = response;
            });
        }

        c.openLocation = function ()
        {
            var url = "http://maps.google.com/maps?q=" + c.Model.Latitude + "," + c.Model.Longitude;
            window.open(url);
        }

        c.save = function () {
            var payload = {
                Uri: "OpenWeatherMapServiceSettings",
                Settings: c.Model
            }

            controllerProxyService.execute("Service/ISettingsService/Replace", payload);

            modalService.show("Info", "Open Weahter Map settings successfully saved.");
        }
    }

    module.component("openWeatherMapServiceSettings", {
        templateUrl: "Settings/OpenWeatherMapServiceSettings.component.html",
        controllerAs: "owmsCtrl",
        controller: ["controllerProxyService", "modalService", createController]
    });
   
})();