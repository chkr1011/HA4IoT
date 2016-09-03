(function () {
    var module = angular.module("app");

    function createController(controllerProxyService, $http) {

        ctrl = this;

        ctrl.Model = [];

        ctrl.moveArea = function (area, direction) {
            var sourceIndex = ctrl.Model.indexOf(area);
            ctrl.Model.moveItem(sourceIndex, direction);
        }

        ctrl.loadDemoData = function () {

            $http.get("Areas/DemoData.json").then(function (response) {
                ctrl.loadAreas(response.data);
            });
        }

        ctrl.loadAreas = function (source) {

            var areas = [];
            $.each(source.Areas, function (id, area) {

                var row = {
                    Id: id,
                    Caption: area.Settings.AppSettings.Caption,
                    SortValue: area.Settings.AppSettings.SortValue,
                    IsVisible: area.Settings.AppSettings.IsVisible
                };

                areas.push(row);
            });

            areas = areas.sort(function (a, b) {
                return a.SortValue - b.SortValue;
            });

            ctrl.Model = areas;
        }

        ctrl.save = function () {
            var payload = {
                Uri: "Area/XYZ",
                Settings: ctrl.Model
            }

            controllerProxyService.invoke("Service/ISettingsService/ReplaceMultiple", payload)

            alert("Saved Controller Slave settings");
        }

        ctrl.loadDemoData();
    }

    module.component("areas", {
        templateUrl: "Areas/AreasOverview.component.html",
        controllerAs: "aoCtrl",
        controller: ["controllerProxyService", "$http", createController]
    });

})();