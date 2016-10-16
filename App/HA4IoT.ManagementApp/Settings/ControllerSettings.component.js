(function () {
    var module = angular.module("app");

    function createController(controllerProxyService, modalService) {

        var ctrl = this;

        ctrl.Model = {
            Caption: "",
            Description: "",
            Language: "EN"
        }

        ctrl.$onInit = function () {
            controllerProxyService.get("Service/ISettingsService/GetSettings", { "Uri": "ControllerSettings" }, function (response) {
                ctrl.Model = response;
            });
        }

        ctrl.save = function () {
            var payload = {
                Uri: "ControllerSettings",
                Settings: ctrl.Model
            }

            controllerProxyService.execute("Service/ISettingsService/Replace", payload);

            modalService.show("Info", "Controller settings successfully saved.");
        }
    }

    module.component("controllerSettings", {
        templateUrl: "Settings/ControllerSettings.component.html",
        controllerAs: "csCtrl",
        controller: ["controllerProxyService", "modalService", createController]
    });

})();