(function () {
    var module = angular.module("app");

    function createController(controllerProxyService, modalService) {

        var ctrl = this;

        ctrl.Model = {
            IsEnabled: false,
            AuthenticationToken: "",
            AllowAllClients: false
        }

        ctrl.$onInit = function () {
            controllerProxyService.get("Service/ISettingsService/GetSettings", { "Uri": "TelegramBotServiceSettings" }, function (response) {
                ctrl.Model = response;
            });
        }

        ctrl.save = function () {
            var payload = {
                Uri: "TelegramBotServiceSettings",
                Settings: ctrl.Model
            }

            controllerProxyService.execute("Service/ISettingsService/Replace", payload);

            modalService.show("Info", "Telegram Bot settings successfully saved.");
        }
    }

    module.component("telegramBotSettings", {
        templateUrl: "Settings/TelegramBotSettings.component.html",
        controllerAs: "tbsCtrl",
        controller: ["controllerProxyService", "modalService", createController]
    });
    
})();