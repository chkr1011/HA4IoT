(function () {
    var module = angular.module("app");

    function createController(controllerProxyService, modalService) {

        var ctrl = this;

        ctrl.Model = {
            IsEnabled: false,
            ServerAddress: "",
            ControllerId: "",
            ApiKey: ""
        }

        ctrl.$onInit = function () {
            controllerProxyService.get("Service/ISettingsService/GetSettings", { "Uri": "CloudConnectorServiceSettings" }, function (response) {
                ctrl.Model = response;
            });
        }

        ctrl.save = function () {
            var payload = {
                Uri: "CloudConnectorServiceSettings",
                Settings: ctrl.Model
            }

            controllerProxyService.execute("Service/ISettingsService/Replace", payload);

            modalService.show("Info", "Cloud Connector service settings successfully saved.");
        }
    }

    module.component("cloudConnectorServiceSettings", {
        templateUrl: "Settings/CloudConnectorServiceSettings.component.html",
        controllerAs: "ccsCtrl",
        controller: ["controllerProxyService", "modalService", createController]
    });
})();