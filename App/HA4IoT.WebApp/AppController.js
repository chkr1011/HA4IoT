app.controller("AppController", ["$scope", "$http", AppController]);

function getVersion(callback) {
    $.get("cache.manifest", function (data) {
        var parser = new RegExp("# Version ([0-9|.]*)", "");
        var results = parser.exec(data);

        callback(results[1]);
    });
}

function AppController($scope, $http) {
    var c = this;

    c.isInitialized = false;
    c.appConfiguration = { showWeatherStation: true, showSensorsOverview: true, showRollerShuttersOverview: true, showMotionDetectorsOverview: true, showWindowsOverview: true }

    c.Areas = [];

    c.sensors = [];
    c.rollerShutters = [];
    c.motionDetectors = [];
    c.windows = [];

    c.activeRoom = "";
    c.errorMessage = null;
    c.version = "-";

    getVersion(function (version) {
        c.version = version;
    });

    c.notifyConfigurationLoaded = function (configuration) {
        $scope.$broadcast("configurationLoaded", { language: configuration.Controller.Language });
    };

    c.deleteNotification = function (uid) {
        postController("Service/INotificationService/Delete", { "Uid": uid });
    }

    c.generateRooms = function () {

        $http.get("/api/Configuration").success(function (data) {

            c.notifyConfigurationLoaded(data);

            $.each(data.Areas, function (areaId, area) {
                if (!getAppSetting(area, "IsVisible", false)) {
                    return;
                }

                var areaControl = {
                    Id: areaId,
                    Caption: getAppSetting(area, "Caption", areaId),
                    SortValue: getAppSetting(area, "SortValue", 0),
                    Components: [],
                    Automations: [],
                    OnStateCount: 0
                };

                $.each(area.Components, function (componentId, component) {
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

                    areaControl.Components.push(component);
                });

                c.Areas.push(areaControl);
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

            c.pollStatus();
            c.isInitialized = true;
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

    c.pollStatus = function () {
        $.ajax({ method: "GET", url: "/api/status", timeout: 2500 }).done(function (data) {
            c.errorMessage = null;

            if (c.status != null && data.$Hash === c.status.$Hash) {
                return;
            }

            c.status = data;
            console.log("Updating UI due to state changes");

            $.each(data.Components, function (id, state) {
                c.updateStatus(id, state);
            });

            updateOnStateCounters(c.Areas);

            $scope.$apply(function () { $scope.msgs = data; });
        }).fail(function (jqXHR, textStatus, errorThrown) {
            c.errorMessage = textStatus;
        }).always(function () {
            setTimeout(function () { c.pollStatus(); }, 250);
        });
    };

    $scope.toggleState = function (component) {
        var newState = "On";
        if (component.state.State === "On") {
            newState = "Off";
        }

        invokeComponent(component.Id, { State: newState }, function () { component.state.State = newState; });
    };

    $scope.invokeVirtualButton = function (actuator) {
        invokeComponent(actuator.Id, {});
        c.pollStatus();
    }

    $scope.toggleIsEnabled = function (component) {
        var isEnabled = !component.Settings.IsEnabled;

        updateComponentSettings(component.Id, {
            IsEnabled: isEnabled
        }, function () {
            component.Settings.IsEnabled = isEnabled;
        });
    };

    $scope.setState = function (component, newState) {
        invokeComponent(component.Id, {
            State: newState
        }, function () {
            component.state.State = newState;
        });
    };

    c.updateStatus = function (id, state) {
        $.each(c.Areas, function (i, area) {
            $.each(area.Components, function (i, component) {

                if (component.Id === id) {
                    component.state = state;
                }

                return;
            });
        });
    };

    c.generateRooms();
}

function configureComponent(area, component) {

    component.Image = getAppSetting(component, "Image", "DefaultActuator");

    component.Caption = getAppSetting(component, "Caption", component.id);
    component.OverviewCaption = getAppSetting(component, "OverviewCaption", component.id);

    component.SortValue = getAppSetting(component, "SortValue", 0);
    component.IsVisible = getAppSetting(component, "IsVisible", true);

    component.DisplayVertical = getAppSetting(component, "DisplayVertical", false);
    component.IsPartOfOnStateCounter = getAppSetting(component, "IsPartOfOnStateCounter", false);
    component.OnStateId = getAppSetting(component, "OnStateId", "On");

    component.state = {};

    switch (component.Type) {
        case "Lamp":
            {
                component.Template = "Views/ToggleTemplate.html";
                break;
            }
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

                    var stateSettings = component.Settings.SupportedStates.find(function(i) {
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
                component.Template = "Views/VirtualButtonTemplate.html";
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

    if (value === undefined) {
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


function postController(uri, body, successCallback) {
    // This hack is required for Safari because only one Ajax request at the same time is allowed.
    var url = "/api/" + uri + "?body=" + JSON.stringify(body);

    $.ajax({
        method: "POST",
        url: url,
        contentType: "application/json; charset=utf-8",
        timeout: 2500
    }).done(function () {
        if (successCallback != null) {
            successCallback();
        }
    }).fail(function (jqXHR, textStatus, errorThrown) {
        alert(textStatus);
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
