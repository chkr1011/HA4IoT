(function () {
    var module = angular.module("app");

    function createController(controllerProxyService) {

        var ctrl = this;
    }

    module.component("home", {
        templateUrl: "Home/Home.component.html",
        controllerAs: "hCtrl",
        controller: ["controllerProxyService", createController]
    });

})();