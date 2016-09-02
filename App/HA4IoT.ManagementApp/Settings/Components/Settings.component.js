(function () {
    var module = angular.module("app");

    function createController(controllerProxyService) {

        ctrl = this;

        ctrl.ActiveTab = "";
    }

    module.component("settings", {
        templateUrl: "Settings/Components/Settings.component.html",
        controllerAs: "sCtrl",
        controller: ["controllerProxyService", createController]
    });
    
})();