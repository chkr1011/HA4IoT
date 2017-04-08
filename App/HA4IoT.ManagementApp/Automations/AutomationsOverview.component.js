(function () {
    var module = angular.module("app");

    function createController(controllerProxyService, modalService) {

        var ctrl = this;

        ctrl.Areas = [];
        ctrl.SelectedArea = null;
        ctrl.SelectedAutomation = null;

        ctrl.selectAutomation = function (automation) {
            ctrl.SelectedAutomation = automation;
        }

        ctrl.fetchAutomations = function () {

            controllerProxyService.get("GetConfiguration", null, function (response) {
                ctrl.loadAutomations(response);
            });
        }

        ctrl.loadAutomations = function (source) {

            var areas = [];
            $.each(source.Areas, function (areaId, areaItem) {

                var area = {
                    Id: areaId,
                    Caption: areaItem.Settings.Caption,
                    Automations: []
                };

                $.each(areaItem.Automations,
                    function (automationId, automationItem) {

                        var automation = {
                            Id: automationId,
                            Type: automationItem.Type,
                            Settings: automationItem.Settings
                        };

                        area.Automations.push(automation);
                    });

                area.Automations = area.Automations.sort(function (a, b) {
                    return a.Id - b.Id;
                });

                areas.push(area);
            });

            areas = areas.sort(function (a, b) {
                return a.SortValue - b.SortValue;
            });

            ctrl.Areas = areas;
        }

        ctrl.close = function () {
            ctrl.SelectedAutomation = null;
        }

        ctrl.save = function () {
            var source = ctrl.SelectedAutomation;

            var payload = {
                Uri: "Automation/" + source.Id,
                Settings: source.Settings
            }

            var timestampPattern = /^-?\d{1,2}[:][0-5]?\d[:][0-5]?\d$/;

            if (source.Type === "TurnOnAndOffAutomation") {
                if (!timestampPattern.test(source.Settings.Duration)) {
                    modalService.show("Error", "Format of field 'Duration' is invalid. Must be '00:01:00' for example.");
                    return;
                }
            }
            else if (source.Type === "RollerShutterAutomation") {
                if (!timestampPattern.test(source.Settings.OpenOnSunriseOffset)) {
                    modalService.show("Error", "Format of field 'OpenOnSunriseOffset' is invalid. Must be '00:01:00' for example.");
                    return;
                }

                if (!timestampPattern.test(source.Settings.CloseOnSunsetOffset)) {
                    modalService.show("Error", "Format of field 'CloseOnSunsetOffset' is invalid. Must be '00:01:00' for example.");
                    return;
                }
            }

            controllerProxyService.execute("Service/ISettingsService/Import", payload, function() {
                modalService.show("Info", "Settings for automation '" + source.Id + "' successfully saved.");
            });
            
            ctrl.close();
        }

        ctrl.fetchAutomations();
    }

    module.component("automations", {
        templateUrl: "Automations/AutomationsOverview.component.html",
        controllerAs: "aoCtrl",
        controller: ["controllerProxyService", "modalService", createController]
    });

})();