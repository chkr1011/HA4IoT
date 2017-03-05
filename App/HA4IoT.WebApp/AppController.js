function getVersion(callback) {
    $.get("cache.manifest", function (data) {
        var parser = new RegExp("# Version ([0-9|.]*)", "");
        var results = parser.exec(data);

        callback(results[1]);
    });
}

function createAppController($http, $scope, modalService, apiService, localizationService, componentService, notificationService) {
    var c = this;

    c.isInitialized = false;
    c.appConfiguration = { showWeatherStation: true, showSensorsOverview: true, showRollerShuttersOverview: true, showMotionDetectorsOverview: true, showWindowsOverview: true }

    c.Areas = [];
    c.Status = null;
    c.StatusHash = "";

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

    c.loadConfiguration = function () {
        apiService.executeApi("GetConfiguration", {}, null, function (response) {

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

                c.Areas.push(area);
            });

            if (c.sensors.length === 0) {
                c.appConfiguration.showSensorsOverview = false;
            }

            if (c.rollerShutters.length === 0) {
                c.appConfiguration.showRollerShuttersOverview = false;
            }

            if (c.motionDetectors.length === 0) {
                c.appConfiguration.showMotionDetectorsOverview = false;
            }

            if (c.windows.length === 0) {
                c.appConfiguration.showWindowsOverview = false;
            }

            if (c.Areas.length === 1) {
                c.setActivePanel(c.Areas[0].id);
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
        c.Status = status;
        console.log("Updating UI due to state changes");

        $.each(status.Components,
            function (id, component) {
                c.updateComponentState(id, component);
            });

        updateOnStateCounters(c.Areas);

        $scope.$apply(function () { $scope.msgs = status; });
    };

    $scope.toggleIsEnabled = function (component) {
        var isEnabled = !component.Settings.IsEnabled;

        updateComponentSettings(component.Id, {
            IsEnabled: isEnabled
        }, function () {
            component.Settings.IsEnabled = isEnabled;
        });
    };

    c.updateComponentState = function (componentId, updatedComponent) {
        $.each(c.Areas, function (i, area) {
            $.each(area.Components, function (j, component) {
                if (component.Id === componentId) {
                    component.Settings = updatedComponent.Settings;
                    component.State = updatedComponent.State;
                }
            });
        });
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
    component.OverviewCaption = getAppSetting(component, "OverviewCaption", "#" + component.Id);

    component.SortValue = getAppSetting(component, "SortValue", 0);
    component.IsVisible = getAppSetting(component, "IsVisible", true);

    component.DisplayVertical = getAppSetting(component, "DisplayVertical", false);
    component.IsPartOfOnStateCounter = getAppSetting(component, "IsPartOfOnStateCounter", false);
    component.OnStateId = getAppSetting(component, "OnStateId", "On");

    component.State = {};

    switch (component.Type) {
        case "Lamp":
        case "Socket":
            {
                component.Template = "Views/ToggleTemplate.html";
                break;
            }

        case "RollerShutter":
            {
                component.Template = "Views/RollerShutterTemplate.html";
                break;
            }

        case "Window":
            {
                component.Template = "Views/WindowTemplate.html";
                break;
            }

        case "StateMachine":
            {
                component.Template = "Views/StateMachineTemplate.html";

                var extendedSupportedStates = [];
                component.SupportedStates.forEach(function (supportedState) {

                    if (component.Settings.SupportedStates === null || component.Settings.SupportedStates === undefined) {
                        component.Settings.SupportedStates = [];
                    }

                    var stateSettings = component.Settings.SupportedStates.find(function (i) {
                        return i.Id === supportedState;
                    });

                    if (stateSettings === null || stateSettings === undefined) {
                        stateSettings = {
                            Id: supportedState,
                            Caption: supportedState
                        }
                    }

                    extendedSupportedStates.push(stateSettings);
                });

                component.SupportedStates = extendedSupportedStates;
                break;
            }

        case "TemperatureSensor":
            {
                component.Template = "Views/TemperatureSensorTemplate.html";
                break;
            }

        case "HumiditySensor":
            {
                component.Template = "Views/HumiditySensorTemplate.html";
                component.DangerValue = getAppSetting(component, "DangerValue", 70);
                component.WarningValue = getAppSetting(component, "WarningValue", 60);
                break;
            }

        case "MotionDetector":
            {
                component.Template = "Views/MotionDetectorTemplate.html";
                break;
            }

        case "Button":
            {
                component.Template = "Views/ButtonTemplate.html";
                break;
            }

        case "VirtualButtonGroup":
            {
                component.Template = "Views/VirtualButtonGroupTemplate.html";

                var extendedButtons = [];
                $.each(component.buttons, function (i, button) {
                    var key = "Caption." + button;
                    var buttonCaption = getAppSetting(component, key, key);

                    extendedButtons.push({ id: button, caption: buttonCaption });
                });

                component.buttons = extendedButtons;
                break;
            }

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

        area.OnStateCount = count + 10; // TEST!
    });
}

function invokeComponent(id, payload, successCallback) {

    payload.ComponentId = id;

    var url = "/api/Service/IComponentService/Invoke?body=" + JSON.stringify(payload);
    var options = {
        method: "POST",
        url: url,
        timeout: 2500
    };

    $.ajax(options).done(function () {
        if (successCallback != null) {
            successCallback();
        }
    }).fail(function (jqXHR, textStatus, errorThrown) {
        alert(textStatus);
    });
}

function updateComponentSettings(id, newSettings, successCallback) {

    var payload = {
        Uri: id,
        Settings: newSettings
    };

    var url = "/api/Service/ISettingsService/Import?body=" + JSON.stringify(payload);
    var options = {
        method: "POST",
        url: url,
        timeout: 2500
    };

    $.ajax(options).done(function () {
        if (successCallback != null) {
            successCallback();
        }
    }).fail(function (jqXHR, textStatus, errorThrown) {
        alert(textStatus);
    });
}