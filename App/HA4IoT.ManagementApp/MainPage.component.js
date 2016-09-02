(function () {
    var module = angular.module("app");

    function createController(controllerProxyService, $compile, $scope) {

        ctrl = this;

        ctrl.switchTab = function (tab) {
            var content = $compile("<" + tab + "></" + tab + ">")($scope);
            $("#mainView").html(content);
        };
    }

    module.component("mainPage", {
        templateUrl: "MainPage.component.html",
        controllerAs: "mpCtrl",
        controller: ["controllerProxyService", "$compile", "$scope", createController]
    });

})();