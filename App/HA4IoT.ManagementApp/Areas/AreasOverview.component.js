(function () {
    var module = angular.module("app");

    function createController(controllerProxyService, $http) {

        var ctrl = this;

        ctrl.Areas = [];
        ctrl.SelectedArea = null;

        ctrl.moveArea = function (area, direction) {
            var sourceIndex = ctrl.Areas.indexOf(area);
            ctrl.Areas.moveItem(sourceIndex, direction);
        }

        ctrl.selectArea = function (area) {
            ctrl.SelectedArea = area;
        }

        ctrl.fetchAreas = function () {

            controllerProxyService.get("configuration",
                function (response) {
                    ctrl.loadAreas(response);
                });
        }

        ctrl.loadAreas = function (source) {

            var areas = [];
            $.each(source.Areas, function (id, item) {

                if (item.Settings.AppSettings === undefined) {
                    item.Settings.AppSettings = {};
                }

                var row = {
                    Id: id,
                    Caption: item.Settings.AppSettings.Caption,
                    SortValue: item.Settings.AppSettings.SortValue,
                    IsVisible: item.Settings.AppSettings.IsVisible
                };

                areas.push(row);
            });

            areas = areas.sort(function (a, b) {
                return a.SortValue - b.SortValue;
            });

            ctrl.Areas = areas;
        }

        ctrl.save = function () {

            $.each(ctrl.Areas, function (id, item) {
                var payload = {
                    Uri: "Area/" + item.Id,
                    Settings: {
                        AppSettings: {
                            Caption: item.Caption,
                            IsVisible: item.IsVisible,
                            SortValue: id
                        }
                    }
                }

                controllerProxyService.execute("Service/ISettingsService/Import", payload);
            });

            alert("Saved Controller Slave settings");
        }

        ctrl.fetchAreas();
    }

    module.component("areas", {
        templateUrl: "Areas/AreasOverview.component.html",
        controllerAs: "aoCtrl",
        controller: ["controllerProxyService", "$http", createController]
    });

})();