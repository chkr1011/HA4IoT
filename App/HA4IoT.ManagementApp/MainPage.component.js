(function () {
    var module = angular.module("app");

    function createController(controllerProxyService, $compile, $scope) {

        var ctrl = this;

        ctrl.ActiveTab = "home";

        ctrl.switchTab = function (tab) {
            var content = $compile("<" + tab + "></" + tab + ">")($scope);
            $("#mainView").html(content);

            ctrl.ActiveTab = tab;
        };
    }

    module.component("mainPage", {
        templateUrl: "MainPage.component.html",
        controllerAs: "mpCtrl",
        controller: ["controllerProxyService", "$compile", "$scope", createController]
    });

})();