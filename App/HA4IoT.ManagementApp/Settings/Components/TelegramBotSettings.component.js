(function () {
    var module = angular.module("app");

    function createController(controllerProxyService) {

        var ctrl = this;

        ctrl.Model = {
            IsEnabled: false,
            AuthenticationToken: ""
        }

        ctrl.save = function () {
            var payload = {
                Uri: "TelegramBotServiceSettings",
                Settings: ctrl.Model
            }

            controllerProxyService.invoke("Command", "Service/ISettingsService/Replace", payload);

            alert("Saved Telegram Bot settings");
        }
    }

    module.component("telegramBotSettings", {
        templateUrl: "Settings/Components/TelegramBotSettings.component.html",
        controllerAs: "tbsCtrl",
        controller: ["controllerProxyService", createController]
    });
    
})();