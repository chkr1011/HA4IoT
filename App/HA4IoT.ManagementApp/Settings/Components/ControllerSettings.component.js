(function () {
    var module = angular.module("app");

    function createController(controllerProxyService) {

        this.Model = {
            Caption: "HA4IoT",
            Description: "HA4IoT controller for my home.",
            Language: "EN"
        }

        this.$onInit = function() {
            
        }

        this.save = function () {
            alert("SAVE");
        }
    }

    module.component("controllerSettings", {
        templateUrl: "Settings/Components/ControllerSettings.component.html",
        controllerAs: "csCtrl",
        controller: ["controllerProxyService", createController]
    });

})();