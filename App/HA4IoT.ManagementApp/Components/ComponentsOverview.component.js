(function () {
    var module = angular.module("app");

    function createController(controllerProxyService, $http) {

        ctrl = this;

        ctrl.Model = [];

        ctrl.moveComponent = function (component, direction) {
            var sourceIndex = ctrl.Model.indexOf(component);
            ctrl.Model.moveItem(sourceIndex, direction);
        }

        ctrl.loadDemoData = function () {

            $http.get("Components/DemoData.json").then(function (response) {
                ctrl.loadComponents(response.data);
            });
        }

        ctrl.loadComponents = function (source) {

            var components = [];
            $.each(source, function (id, item) {

                var row = {
                    Id: id,
                    Type: item.Type,
                    Caption: item.Settings.AppSettings.Caption,
                    SortValue: item.Settings.AppSettings.SortValue,
                    Image: item.Settings.AppSettings.Image,
                    IsVisible: item.Settings.AppSettings.IsVisible
                };

                components.push(row);
            });

            components = components.sort(function (a, b) {
                return a.SortValue - b.SortValue;
            });

            ctrl.Model = components;
        }

        ctrl.loadDemoData();
    }

    module.component("components", {
        templateUrl: "Components/ComponentsOverview.component.html",
        controllerAs: "coCtrl",
        controller: ["controllerProxyService", "$http", createController]
    });

})();