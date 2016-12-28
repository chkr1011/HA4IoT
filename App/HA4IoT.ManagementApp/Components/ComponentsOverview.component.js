(function () {
    var module = angular.module("app");

    function createController(controllerProxyService, modalService) {

        var ctrl = this;

        ctrl.Areas = [];
        ctrl.SelectedArea = null;
        ctrl.SelectedComponent = null;

        ctrl.moveComponent = function (component, direction, event) {
            event.stopPropagation();
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
                    Caption: areaItem.Settings.Caption,
                    Components: []
                };

                $.each(areaItem.Components,
                    function (componentId, componentItem) {
                        var component = ctrl.createComponent(componentId, componentItem);
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

        ctrl.close = function () {
            ctrl.SelectedComponent = null;
        }

        ctrl.save = function () {
            var settings = {};
            $.each(ctrl.SelectedArea.Components,
                function (j, component) {
                    settings["Component/" + component.Id] = ctrl.createSettings(component, j);
                });

            controllerProxyService.execute("Service/ISettingsService/ImportMultiple", settings, function () {
                modalService.show("Info", "Component settings successfully saved.");
            });
        }

        ctrl.createComponent = function (componentId, source) {
            var component = {
                Id: componentId,
                Type: source.Type,
                Caption: source.Settings.Caption,
                OverviewCaption: source.Settings.OverviewCaption,
                Keywords: source.Settings.Keywords,
                SortValue: source.Settings.SortValue,
                Image: source.Settings.Image,
                IsEnabled: source.Settings.IsEnabled,
                IsVisible: source.Settings.IsVisible
            };

            if (component.Type === "StateMachine") {
                component.DisplayVertical = source.Settings.DisplayVertical;
                component.SupportedStates = [];

                if (source.Settings.SupportedStates === undefined || source.Settings.SupportedStates === null) {
                    source.Settings.SupportedStates = [];
                }

                source.SupportedStates.forEach(function (supportedState) {

                    var settings = source.Settings.SupportedStates.find(function (i) {
                        return i.Id === supportedState;
                    });

                    if (settings === undefined || settings === null) {
                        settings = {
                            Caption: ""
                        }
                    }

                    var vm = {
                        Id: supportedState,
                        Caption: settings.Caption
                    }

                    component.SupportedStates.push(vm);
                });
            }

            if (component.Type === "RollerShutter") {
                component.AutoOffTimeout = source.Settings.AutoOffTimeout;
                component.MaxPosition = source.Settings.MaxPosition;
            }

            if (component.Type === "HumiditySensor") {
                component.DangerValue = source.Settings.DangerValue;
                component.WarningValue = source.Settings.WarningValue;
            }

            return component;
        }

        ctrl.createSettings = function (component, sortValue) {
            var settings = {
                IsEnabled: component.IsEnabled,
                Caption: component.Caption,
                OverviewCaption: component.OverviewCaption,
                Keywords: component.Keywords,
                IsVisible: component.IsVisible,
                Image: component.Image,
                SortValue: sortValue
            }

            if (component.Type === "StateMachine") {
                settings.DisplayVertical = component.DisplayVertical;
                settings.SupportedStates = [];

                component.SupportedStates.forEach(function (supportedState) {
                    settings.SupportedStates.push(supportedState);
                });
            }

            if (component.Type === "RollerShutter") {
                settings.AutoOffTimeout = component.AutoOffTimeout;
                settings.MaxPosition = component.MaxPosition;
            }

            if (component.Type === "HumiditySensor") {
                settings.DangerValue = component.DangerValue;
                settings.WarningValue = component.WarningValue;
            }

            return settings;
        }

        ctrl.fetchComponents();

    }

    module.component("components", {
        templateUrl: "Components/ComponentsOverview.component.html",
        controllerAs: "coCtrl",
        controller: ["controllerProxyService", "modalService", createController]
    });

})();