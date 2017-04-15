function getVersion(callback) {
    $.get("cache.manifest", function (data) {
        var parser = new RegExp("# Version ([0-9|.]*)", "");
        var results = parser.exec(data);

        callback(results[1]);
    });
}

function createAppController($http, $scope, modalService, apiService, localizationService, componentService, notificationService) {
    var c = this;

    $scope.getNumbers = function (num) {
        var a = new Array(num + 1);
        for (var i = 0; i < num + 1; i++) {
            a[i] = i;
        }

        return a;
    }

    $scope.getStateCaption = function (component, id) {
        if (component == undefined) {
            return id;
        }

        if (component.Settings == undefined) {
            return id;
        }

        if (component.Settings.SupportedStates == undefined) {
            return id;
        }

        var settings = component.Settings.SupportedStates.find(function (i) { return i.Id === id });
        if (settings == undefined) {
            return id;
        }

        if (settings.Caption == undefined) {
            return id;
        }

        return settings.Caption;
    }

    c.isInitialized = false;
    c.appConfiguration = { showWeatherStation: true, showSensorsOverview: true, showRollerShuttersOverview: true, showMotionDetectorsOverview: true, showWindowsOverview: true }

    c.areas = [];

    c.weatherStation = {}

    c.sensors = [];
    c.rollerShutters = [];
    c.motionDetectors = [];
    c.windows = [];

    c.version = "-";

    c.notificationService = notificationService;
    c.componentService = componentService;
    c.localizationService = localizationService;
    c.apiService = apiService;

    apiService.apiStatusUpdatedCallback = function (s) {
        c.apiStatus = s;
        $scope.$apply(function () { $scope.msgs = s; });
    }

    getVersion(function (version) {
        c.version = version;
    });

    c.showInfoPopover = function () {
        $("#infoIcon").popover({
            html: true,
            title: "HA4IoT",
            placement: "top",
            content: function () {
                return $('#infoPopoverContent').html();
            }
        });
    }

    c.showSetColorPopover = function (component) {
        if (component.State.ColorState == undefined) {
            return;
        }

        if (component.showColorSelector === true) {
            component.showColorSelector = false;
            return;
        }

        component.colorSelector.hue = component.State.ColorState.Hue;
        component.colorSelector.saturation = component.State.ColorState.Saturation;
        component.colorSelector.value = component.State.ColorState.Value;

        component.showColorSelector = true;
    }

    c.hideSetColorPopover = function (component) {
        component.showColorSelector = false;
    }

    c.loadConfiguration = function () {
        apiService.getConfiguration(function (response) {

            localizationService.load(response.Result.Controller.Language);

            $.each(response.Result.Areas, function (areaId, area) {

                var components = area.Components;

                area.Id = areaId;
                configureArea(area);

                if (!area.IsVisible) {
                    return;
                }

                $.each(components, function (componentId, component) {
                    component.Id = componentId;
                    configureComponent(area, component);

                    if (!component.IsVisible) {
                        return;
                    }

                    if (component.Type === "TemperatureSensor" ||
                        component.Type === "HumiditySensor") {
                        c.sensors.push(component);
                    } else if (component.Type === "RollerShutter") {
                        c.rollerShutters.push(component);
                    } else if (component.Type === "MotionDetector") {
                        c.motionDetectors.push(component);
                    } else if (component.Type === "Window") {
                        c.windows.push(component);
                    }

                    area.Components.push(component);
                });

                c.areas.push(area);
            });

            c.appConfiguration.showSensorsOverview = c.sensors.length > 0;
            c.appConfiguration.showRollerShuttersOverview = c.rollerShutters.length > 0;
            c.appConfiguration.showMotionDetectorsOverview = c.motionDetectors.length > 0;
            c.appConfiguration.showWindowsOverview = c.windows.length > 0;

            if (c.areas.length === 1) {
                c.setActivePanel(c.areas[0].Id);
            }

            c.apiService.newStatusReceivedCallback = c.applyNewStatus;
            c.apiService.pollStatus();
            c.isInitialized = true;
        },
            function () {
                modalService.show("Configuration not available", "Unable to load the configuration. Please try again later.");
            });
    };

    c.setActivePanel = function (id) {
        if (c.activePanel === id) {
            c.activePanel = "";
        } else {
            c.activePanel = id;
        }

        setTimeout(function () {
            $("html, body").animate({
                scrollTop: $("#" + id).offset().top
            }, 250);
        }, 100);
    }

    c.applyNewStatus = function (status) {
        console.log("Updating UI due to state changes");

        $.each(status.Components, function (id, component) {
            c.updateComponentState(id, component);
        });

        c.weatherStation.temperature = status.OutdoorTemperature;
        c.weatherStation.humidity = status.OutdoorHumidity;
        c.weatherStation.sunrise = status.Sunrise;
        c.weatherStation.sunset = status.Sunset;
        c.weatherStation.weather = status.Weather;

        updateOnStateCounters(c.areas);
    };

    c.updateComponentState = function (componentId, updatedComponent) {
        $.each(c.areas, function (i, area) {
            $.each(area.Components, function (j, component) {
                if (component.Id === componentId) {
                    component.Settings = updatedComponent.Settings;
                    component.State = updatedComponent.State;

                    if (component.onStateChangedCallback != undefined) {
                        component.onStateChangedCallback(component);
                    }
                }
            });
        });
    };

    c.toggleIsEnabled = function (component) {
        if (component.Settings.IsEnabled) {
            componentService.disable(component);
        } else {
            componentService.enable(component);
        }
    };

    c.loadConfiguration();
}

function configureArea(area) {

    area.Caption = getAppSetting(area, "Caption", "#" + area.Id);
    area.SortValue = getAppSetting(area, "SortValue", 0);
    area.IsVisible = getAppSetting(area, "IsVisible", true);
    area.Components = [];
    area.OnStateCount = 0;
}

function configureComponent(area, component) {

    component.Image = getAppSetting(component, "Image", "DefaultActuator");

    component.Caption = getAppSetting(component, "Caption", "#" + component.Id);
    component.OverviewCaption = area.Caption + " / " + component.Caption;

    component.SortValue = getAppSetting(component, "SortValue", 0);
    component.IsVisible = getAppSetting(component, "IsVisible", true);

    component.DisplayVertical = getAppSetting(component, "DisplayVertical", false);
    component.IsPartOfOnStateCounter = getAppSetting(component, "IsPartOfOnStateCounter", false);
    component.OnStateId = getAppSetting(component, "OnStateId", "On");

    component.State = {};

    switch (component.Type) {
        case "Lamp":
            component.template = "Views/LampTemplate.html";
            component.colorSelector = {};
            break;
        case "Socket":
            component.template = "Views/ToggleTemplate.html";
            break;
        case "Fan":
            component.template = "Views/FanTemplate.html";
            break;
        case "RollerShutter":
            component.template = "Views/RollerShutterTemplate.html";
            break;
        case "Window":
            component.template = "Views/WindowTemplate.html";
            break;
        case "StateMachine":
            component.template = "Views/StateMachineTemplate.html";
            break;
        case "TemperatureSensor":
            component.template = "Views/TemperatureSensorTemplate.html";
            break;
        case "HumiditySensor":
            component.template = "Views/HumiditySensorTemplate.html";
            component.DangerValue = getAppSetting(component, "DangerValue", 70);
            component.WarningValue = getAppSetting(component, "WarningValue", 60);
            break;
        case "MotionDetector":
            component.template = "Views/MotionDetectorTemplate.html";
            break;
        case "Button":
            component.template = "Views/ButtonTemplate.html";
            break;

        default:
            {
                component.IsVisible = false;
                return;
            }
    }
}

function getAppSetting(component, name, defaultValue) {
    if (component.Settings === undefined) {
        return defaultValue;
    }

    var value = component.Settings[name];

    if (value === undefined || value === null) {
        return defaultValue;
    }

    return value;
}

function updateOnStateCounters(areas) {
    areas.forEach(function (area) {
        var count = 0;

        area.Components.forEach(function (component) {
            if (component.IsPartOfOnStateCounter) {
                if (component.OnStateId === component.state.state) {
                    count++;
                }
            }
        });

        area.OnStateCount = count;
    });
}