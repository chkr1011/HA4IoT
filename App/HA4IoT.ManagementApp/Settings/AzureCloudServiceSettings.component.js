(function () {
    var module = angular.module("app");

    function createController(controllerProxyService, modalService) {

        var ctrl = this;

        ctrl.Model = {
            IsEnabled: false,
            ControllerId: "",
            OutboundQueueAuthorization: "",
            InboundQueueAuthorization: ""
        }

        ctrl.$onInit = function () {
            controllerProxyService.get("Service/ISettingsService/GetSettings", { "Uri": "AzureCloudServiceSettings" }, function (response) {
                ctrl.Model = response;
            });
        }

        ctrl.save = function () {
            var payload = {
                Uri: "AzureCloudServiceSettings",
                Settings: ctrl.Model
            }

            controllerProxyService.execute("Service/ISettingsService/Replace", payload);

            modalService.show("Info", "Azure Cloud service settings successfully saved.");
        }
    }

    module.component("azureCloudServiceSettings", {
        templateUrl: "Settings/AzureCloudServiceSettings.component.html",
        controllerAs: "acsCtrl",
        controller: ["controllerProxyService", "modalService", createController]
    });
    
})();