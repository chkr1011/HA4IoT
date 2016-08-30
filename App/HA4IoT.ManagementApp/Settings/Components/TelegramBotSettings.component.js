(function () {
    var module = angular.module("app");

    function createController(controllerProxyService) {

        this.Model = {
            AuthenticationToken: "ABC"
        }

        this.save = function () {
            var payload = {
                Uri: "Service/TelegramBotService",
                Settings: this.Model
            }

            controllerProxyService.invoke("Command", "Service/ISettingsService/Replace", payload)

            alert("Saved Telegram Bot settings");
        }
    }

    module.component("telegramBotSettings", {
        templateUrl: "Settings/Components/TelegramBotSettings.component.html",
        controllerAs: "tbsCtrl",
        controller: ["controllerProxyService", createController]
    });
    
})();