(function () {
    var module = angular.module("app");

    function createController(controllerProxyService) {

        this.Model = {
            AccessTokenSecret: "123",
            AccessToken: "456",
            ConsumerSecret: "789",
            ConsumerKey: "XYZ"
        }

        this.save = function () {
            var payload = {
                Uri: "Service/ITwitterClientService",
                Settings: this.Model
            }

            controllerProxyService.invoke("Command", "Service/ISettingsService/Replace", payload)

            alert("Saved Twitter client settings");
        }
    }

    module.component("twitterClientSettings", {
        templateUrl: "Settings/Components/TwitterClientSettings.component.html",
        controllerAs: "tcsCtrl",
        controller: ["controllerProxyService", createController]
    });
    
})();