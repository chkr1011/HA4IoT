(function () {
    var module = angular.module("app");

    function createController(controllerProxyService, modalService) {

        var ctrl = this;

        ctrl.Model = {
            IsEnabled: false,
            AccessTokenSecret: "",
            AccessToken: "",
            ConsumerSecret: "",
            ConsumerKey: ""
        }

        ctrl.$onInit = function () {
            controllerProxyService.get("Service/ISettingsService/GetSettings", { "Uri": "TwitterClientServiceSettings" }, function (response) {
                ctrl.Model = response;
            });
        }

        ctrl.save = function () {
            var payload = {
                Uri: "TwitterClientServiceSettings",
                Settings: ctrl.Model
            }

            controllerProxyService.execute("Service/ISettingsService/Replace", payload);

            modalService.show("Info", "Twitter client settings successfully saved.");
        }
    }

    module.component("twitterClientSettings", {
        templateUrl: "Settings/TwitterClientSettings.component.html",
        controllerAs: "tcsCtrl",
        controller: ["controllerProxyService", "modalService", createController]
    });
    
})();