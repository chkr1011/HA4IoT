(function () {
    var module = angular.module("app");

    function createController(controllerProxyService) {

        var ctrl = this;

        ctrl.Model = {
            IsEnabled: false,
            AuthenticationToken: "",
            AllowAllClients: false
        }

        ctrl.$onInit = function () {
            controllerProxyService.get("Service/ISettingsService/Settings", { "Uri": "TelegramBotServiceSettings" }, function (response) {
                ctrl.Model = response;
            });
        }

        ctrl.save = function () {
            var payload = {
                Uri: "TelegramBotServiceSettings",
                Settings: ctrl.Model
            }

            controllerProxyService.execute("Service/ISettingsService/Replace", payload);

            alert("Saved Telegram Bot settings");
        }
    }

    module.component("telegramBotSettings", {
        templateUrl: "Settings/Components/TelegramBotSettings.component.html",
        controllerAs: "tbsCtrl",
        controller: ["controllerProxyService", createController]
    });
    
})();