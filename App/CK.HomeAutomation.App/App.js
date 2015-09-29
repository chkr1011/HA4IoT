setupController();

function getFriendlyName(key) {

    var result = key;

    $.each(friendlyNameLookup, function (i, entry) {
        if (entry.key === key) {
            result = entry.value;
            return;
        }
    });

    return result;
}

function getApiUrl() {
	if (appConfiguration.controllerAddress === "") {
		return "/api/";
	} else {
		return "http://" + appConfiguration.controllerAddress + "/api/";
	}
}

function getVersion(callback) {
    $.get("cache.manifest", function (data) {
        var parser = new RegExp("# Version ([0-9|.]*)", "");
        var results = parser.exec(data);

        callback(results[1]);
    });
}

function setupController() {
    var app = angular.module("app", []);
    app.controller("HomeAutomationController", [
        "$scope", function ($scope) {
            var c = this;

            c.latestStateHash = "";

            c.appConfiguration = appConfiguration;
            c.rooms = [];

            c.weatherStation = {};
            c.sensors = [];
            c.rollerShutters = [];
            c.motionDetectors = [];
            c.windows = [];

            c.activeRoom = "";
            c.errorMessage = "";
            c.version = "-";

            getVersion(function (version) { c.version = version });

            c.generateRooms = function () {
                getJSON(c, getApiUrl() + "configuration", function (data) {

                    $.each(data, function (roomIndex, room) {
                       configureRoom(room);

                        if (!room.hide) {
                            for (var i = room.actuators.length - 1; i >= 0; i--) {
                                var actuator = room.actuators[i];

                                configureActuator(room, actuator, i);

                                if (actuator.type === "TemperatureSensor" || actuator.type === "HumiditySensor") {
                                    c.sensors.push(actuator);
                                }
                                else if (actuator.type === "RollerShutter") {
                                    c.rollerShutters.push(actuator);
                                }
                                else if (actuator.type === "MotionDetector") {
                                    c.motionDetectors.push(actuator);
                                }
                                else if (actuator.type === "Window") {
                                    c.windows.push(actuator);
                                }
                            }

                            c.rooms.push(room);
                        }
                    });

                    $scope.$apply(function () {
                        $scope.msgs = c.rooms;
                    });

                    if (c.sensors.length == 0)
                    {
                        c.appConfiguration.showSensorsOverview = false;
                    }

                    if (c.rollerShutters.length == 0)
                    {
                        c.appConfiguration.showRollerShuttersOverview = false;
                    }

                    if (c.motionDetectors.length == 0)
                    {
                        c.appConfiguration.showMotionDetectorsOverview = false;
                    }

                    if (c.windows.length == 0)
                    {
                        c.appConfiguration.showWindowsOverview = false;
                    }

                    c.pollStatus();
                    c.isReady = true;

                    $("body").css("background", "white");
                    $("#content").removeClass("hidden");
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
                getJSON(c, getApiUrl() + "status?" + c.latestStateHash, function (data) {

                    if (c.latestStateHash == data._hash) {
                      // The state has not changed. Skip update.
                      return;
                    }

                    c.latestStateHash = data._hash;

                    $.each(data, function (id, state) {
                        c.updateStatus(id, state);
                    });

                    c.weatherStation = data.weatherStation;

                    $scope.$apply(function () {
                        $scope.msgs = data;
                    });
                });

                setTimeout(function () { c.pollStatus(); }, c.appConfiguration.pollInterval);
            };

            $scope.toggleState = function (actuator) {
                var newState = !actuator.state.stateBool;

                var tag = "off";
                if (newState === true) {
                    tag = "on";
                }

                invokeActuator(actuator.id, { state: tag }, function () { actuator.state.stateBool = newState; });
            };

            $scope.toggleIsEnabled = function (actuator) {
                var newState = !actuator.state.isEnabled;

                invokeActuator(actuator.id, { isEnabled: newState }, function () {
                    actuator.state.isEnabled = newState;
                });
            };

            $scope.setState = function (actuator, newState) {
                invokeActuator(actuator.id, { state: newState }, function () { actuator.state.state = newState; });
            };

            c.updateStatus = function (id, state) {
                $.each(c.rooms, function (i, room) {
                    $.each(room.actuators, function (i, actuator) {

                        if (actuator.id === id) {
                            actuator.state = state;
                        }

                        return;
                    });
                });
            };

            c.generateRooms();
        }
    ]);
}

function configureRoom(room) {
    room.caption = getFriendlyName(room.id);
    room.sortValue = -1;
    room.hide = false;

    appConfiguration.roomExtender(room);
}

function binaryActuatorStateUpdater(actuator, newState)
{
  if (newState.state == "On") {
    actuator.state.stateBool = true;
  }
  else {
    actuator.state.stateBool = false;
  }
}

function configureActuator(room, actuator, i) {
    actuator.caption = getFriendlyName(actuator.id);
    actuator.sortValue = 0;
    actuator.hide = false;
    actuator.image = actuator.type;

    actuator.state = {};
    actuator.displayVertical = false;

    switch (actuator.type) {
        case "Lamp":
            {
                actuator.template = "toggleTemplate";
                actuator.updateState = binaryActuatorStateUpdater;
                actuator.sortValue = -7;
                break;
            }
        case "Socket":
            {
                actuator.template = "toggleTemplate";
                actuator.updateState = binaryActuatorStateUpdater;
                actuator.sortValue = -6;
                break;
            }

        case "RollerShutter":
            {
                actuator.caption = getFriendlyName("RollerShutter");
                actuator.template = "rollerShutterTemplate";
                actuator.sortValue = -4;
                break;
            }

        case "Window":
            {
                actuator.caption = getFriendlyName("Window");
                actuator.template = "windowTemplate";
                actuator.sortValue = -4;
                break;
            }

        case "StateMachine":
            {
                var extendedStates = [];

                $.each(actuator.states, function (stateIndex, state) {
                    extendedStates.push({ value: state, caption: getFriendlyName(state) });
                });

                actuator.states = extendedStates;

                actuator.template = "stateMachineTemplate";
                actuator.sortValue = -5;
                break;
            }

        case "TemperatureSensor":
            {
                actuator.template = "temperatureSensorTemplate";
                actuator.caption = "Temperatur";
                actuator.sortValue = -10;
                break;
            }

        case "HumiditySensor":
            {
                actuator.template = "humiditySensorTemplate";
                actuator.caption = "Luftfeuchtigkeit";
                actuator.sortValue = -9;
                break;
            }

        case "MotionDetector":
            {
                actuator.template = "motionDetectorTemplate";
                actuator.caption = "Bewegung";
                actuator.sortValue = -8;
                break;
            }

        default:
            {
                room.actuators.splice(i, 1);
                return;
            }
    }

    appConfiguration.actuatorExtender(actuator);
}

function getJSON(controller, url, callback) {
    // $.getJSON will not work due to cross site scripting.
    $.ajax(
        {
            method: "GET",
            url: url,
            crossDomain: true,
            timeout: 2500
        })
        .success(function (data) {
            controller.errorMessage = "";
            callback(data);
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            controller.errorMessage = textStatus;
        });
};

function invokeActuator(id, request, successCallback) {
    $.ajax(
        {
            method: "POST",
            url: getApiUrl() + "actuator/" + id + "?body=" + JSON.stringify(request),
            crossDomain: true,
            timeout: 2500
        }).success(function () {
            if (successCallback != null) {
                successCallback();
            }
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            controller.errorMessage = textStatus;
        });
}
