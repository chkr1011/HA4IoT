(function () {
    var module = angular.module("app");

    function createController(controllerProxyService, modalService) {

        var ctrl = this;

        ctrl.downloadBackup = function () {
            controllerProxyService.get("Service/ISettingsService/Backup",
                null,
                function (response) {
                    var link = document.createElement("a");
                    link.download = "HA4IoT-Backup-" + ctrl.generateTimestamp() + ".json";
                    link.href = "data:," + JSON.stringify(response);
                    link.click();
                });
        }

        ctrl.generateTimestamp = function() {
            var today = new Date();
            var dd = today.getDate();
            var mm = today.getMonth() + 1;

            var yyyy = today.getFullYear();

            if (dd < 10) {
                dd = "0" + dd;
            }

            if (mm < 10) {
                mm = "0" + mm;
            }

            return yyyy + "-" + mm + "-" + dd;
        }

        ctrl.restoreBackup = function () {
            //controllerProxyService.execute();
        }
    }

    module.component("backup", {
        templateUrl: "Backup/Backup.component.html",
        controllerAs: "bCtrl",
        controller: ["controllerProxyService", "modalService", createController]
    });

})();