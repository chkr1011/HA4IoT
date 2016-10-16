(function () {
    var module = angular.module("app");

    function createController(controllerProxyService) {

        var ctrl = this;

        ctrl.ActiveTab = "";
    }

    module.component("settings", {
        templateUrl: "Settings/Settings.component.html",
        controllerAs: "sCtrl",
        controller: ["controllerProxyService", createController]
    });
    
})();