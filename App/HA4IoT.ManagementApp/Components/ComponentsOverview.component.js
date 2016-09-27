(function () {
    var module = angular.module("app");

    function createController(controllerProxyService, $http) {

        var ctrl = this;

        ctrl.Areas = [];
        ctrl.SelectedArea = null;
        ctrl.SelectedComponent = null;

        ctrl.moveComponent = function (component, direction) {
            var sourceIndex = ctrl.Model.indexOf(component);
            ctrl.Model.moveItem(sourceIndex, direction);
        }

        ctrl.selectComponent = function (component) {
            ctrl.SelectedComponent = component;
        }

        ctrl.fetchComponents = function () {

            controllerProxyService.get("configuration", function (response) {
                ctrl.loadComponents(response);
            });
        }

        ctrl.loadComponents = function (source) {

            var areas = [];
            $.each(source.Areas, function (areaId, areaItem) {

                var area = {
                    Id: areaId,
                    Caption: areaItem.Settings.AppSettings.Caption,
                    Components: []
                };

                $.each(areaItem.Components,
                    function (componentId, componentItem) {
                        if (componentItem.Settings.AppSettings === undefined) {
                            componentItem.Settings.AppSettings = {};
                        }

                        var component = {
                            Id: componentId,
                            Type: componentItem.Type,
                            Caption: componentItem.Settings.AppSettings.Caption,
                            OverviewCaption: componentItem.Settings.AppSettings.OverviewCaption,
                            SortValue: componentItem.Settings.AppSettings.SortValue,
                            Image: componentItem.Settings.AppSettings.Image,
                            IsEnabled: componentItem.Settings.IsEnabled,
                            IsVisible: componentItem.Settings.AppSettings.IsVisible,
                            SupportedStates: componentItem.SupportedStates
                        };

                        area.Components.push(component);
                    });

                areas.push(area);
            });

            areas = areas.sort(function (a, b) {
                return a.SortValue - b.SortValue;
            });

            ctrl.Areas = areas;
        }

        ctrl.fetchComponents();
    }

    module.component("components", {
        templateUrl: "Components/ComponentsOverview.component.html",
        controllerAs: "coCtrl",
        controller: ["controllerProxyService", "$http", createController]
    });

})();