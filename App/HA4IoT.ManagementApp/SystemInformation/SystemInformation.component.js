(function () {
    var module = angular.module("app");

    function createController(controllerProxyService, modalService) {

        var ctrl = this;

        ctrl.values = [];

        ctrl.$onInit = function () {
            ctrl.refresh();
        };

        ctrl.refresh = function () {
            controllerProxyService.execute("Service/ISystemInformationService/GetStatus", {},
                function (response) {
                    ctrl.values = [];

                    $.each(response,
                        function (item, i) {
                            ctrl.values.push({ key: item, value: response[item] });
                        });

                    setTimeout(function () { ctrl.refresh(); }, 1000);
                });
        };
    }

    module.component("systemInformation", {
        templateUrl: "SystemInformation/SystemInformation.component.html",
        controllerAs: "siCtrl",
        controller: ["controllerProxyService", "modalService", createController]
    });

})();