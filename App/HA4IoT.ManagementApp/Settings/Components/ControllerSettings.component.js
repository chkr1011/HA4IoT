(function () {
    var module = angular.module("app");

    function createController(controllerProxyService) {

        var ctrl = this;

        ctrl.Model = {
            Caption: "",
            Description: "",
            Language: "EN"
        }

        ctrl.$onInit = function () {
            controllerProxyService.get("Service/ISettingsService/Settings", { "Uri": "ControllerSettings" }, function (response) {
                ctrl.Model = response;
            });
        }

        ctrl.save = function () {
            var payload = {
                Uri: "ControllerSettings",
                Settings: ctrl.Model
            }

            controllerProxyService.execute("Service/ISettingsService/Replace", payload);

            alert("Saved controller settings");
        }
    }

    module.component("controllerSettings", {
        templateUrl: "Settings/Components/ControllerSettings.component.html",
        controllerAs: "csCtrl",
        controller: ["controllerProxyService", createController]
    });

})();