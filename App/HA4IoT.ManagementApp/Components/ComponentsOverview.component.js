(function () {
    var module = angular.module("app");

    function createController(controllerProxyService, modalService) {

        var ctrl = this;

        ctrl.Areas = [];
        ctrl.SelectedArea = null;
        ctrl.SelectedComponent = null;

        ctrl.moveComponent = function (component, direction) {
            var sourceIndex = ctrl.SelectedArea.Components.indexOf(component);
            ctrl.SelectedArea.Components.moveItem(sourceIndex, direction);
        }

        ctrl.selectComponent = function (component) {
            ctrl.SelectedComponent = component;
        }

        ctrl.fetchComponents = function () {

            controllerProxyService.get("configuration", null, function (response) {
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

                area.Components = area.Components.sort(function (a, b) {
                    return a.SortValue - b.SortValue;
                });

                areas.push(area);
            });

            areas = areas.sort(function (a, b) {
                return a.SortValue - b.SortValue;
            });

            ctrl.Areas = areas;
        }

        ctrl.save = function () {
            var settings = {};

            $.each(ctrl.SelectedArea.Components,
                function (j, componentItem) {
                    settings["Component/" + componentItem.Id] = {
                        IsEnabled: componentItem.IsEnabled,
                        AppSettings: {
                            Caption: componentItem.Caption,
                            OverviewCaption: componentItem.OverviewCaption,
                            IsVisible: componentItem.IsVisible,
                            Image: componentItem.Image,
                            SortValue: j
                        }
                    };
                });

            controllerProxyService.execute("Service/ISettingsService/ImportMultiple", settings, function () {
                modalService.show("Info", "Component settings successfully saved.");
            });
        }

        ctrl.fetchComponents();
    }

    module.component("components", {
        templateUrl: "Components/ComponentsOverview.component.html",
        controllerAs: "coCtrl",
        controller: ["controllerProxyService", "modalService", createController]
    });

})();