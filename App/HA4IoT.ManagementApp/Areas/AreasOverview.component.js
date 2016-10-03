(function () {
    var module = angular.module("app");

    function createController(controllerProxyService, modalService) {

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

            controllerProxyService.get("configuration", null,
                function (response) {
                    ctrl.loadAreas(response);
                });
        }

        ctrl.loadAreas = function (source) {

            var areas = [];
            $.each(source.Areas, function (id, item) {
                var row = {
                    Id: id,
                    Caption: item.Settings.Caption,
                    SortValue: item.Settings.SortValue,
                    IsVisible: item.Settings.IsVisible
                };

                areas.push(row);
            });

            areas = areas.sort(function (a, b) {
                return a.SortValue - b.SortValue;
            });

            ctrl.Areas = areas;
        }

        ctrl.save = function () {
            $.each(ctrl.Areas, function (i, item) {
                var payload = {
                    Uri: "Area/" + item.Id,
                    Settings: {
                        Caption: item.Caption,
                        IsVisible: item.IsVisible,
                        SortValue: i
                    }
                }

                controllerProxyService.execute("Service/ISettingsService/Import", payload);
            });

            modalService.show("Info", "Area settings successfully saved.");
        }

        ctrl.fetchAreas();
    }

    module.component("areas", {
        templateUrl: "Areas/AreasOverview.component.html",
        controllerAs: "aoCtrl",
        controller: ["controllerProxyService", "modalService", createController]
    });

})();