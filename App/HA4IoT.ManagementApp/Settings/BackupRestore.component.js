(function () {
    var module = angular.module("app");

    function createController(controllerProxyService) {

        var ctrl = this;

        ctrl.createBackup = function () {
            controllerProxyService.get("Service/ISettingsService/Backup", {});
        }

        ctrl.restoreBackup = function() {
            //controllerProxyService.execute();
        }
    }

    module.component("backupRestoreSettings", {
        templateUrl: "Settings/BackupRestore.component.html",
        controllerAs: "brCtrl",
        controller: ["controllerProxyService", createController]
    });

})();