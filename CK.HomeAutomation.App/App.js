setupController();

function extendActuator(actuator) {
    if (actuatorExtender == null) {
        return;
    }

    actuatorExtender(actuator);
}

function extendRoom(room) {
    if (roomExtender == null) {
        return;
    }

    roomExtender(room);
}

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

            c.activeRoom = "";
            c.errorMessage = "";
            c.version = "-";
            getVersion(function (version) { c.version = version });

            c.generateRooms = function () {
                getJSON(c, "http://" + controllerAddress + "/api/configuration", function (data) {

                    $.each(data, function (roomIndex, room) {

                        room.caption = getFriendlyName(room.id);

                        for (var i = room.actuators.length - 1; i >= 0; i--) {
                            var actuator = room.actuators[i];
                            configureActuator(room, actuator, i);
                        }
                    });

                    c.rooms = data;

                    $scope.$apply(function () {
                        $scope.msgs = c.rooms;
                    });

                    c.pollStatus();
                    c.isReady = true;

                    $("body").css("background", "white");
                    $("#content").removeClass("hidden");
                });
            };

            c.setActiveRoom = function (roomId) {
                if (c.activeRoom === roomId) {
                    c.activeRoom = "";
                } else {
                    c.activeRoom = roomId;
                }

                setTimeout(function () {
                    $("html, body").animate({
                        scrollTop: $("#" + roomId).offset().top
                    }, 250);
                }, 100);
            }

            c.pollStatus = function () {
                getJSON(c, "http://" + controllerAddress + "/api/status", function (data) {

                    $.each(data, function (id, state) {
                        c.updateStatus(id, state);
                    });

                    $scope.$apply(function () {
                        $scope.msgs = data;
                    });
                });

                setTimeout(function () { c.pollStatus(); }, pollInterval);
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
                actuator.state.state = newState;
                invokeActuator(actuator.id, { state: newState });
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

function configureActuator(room, actuator, i) {
    actuator.caption = getFriendlyName(actuator.id);
    actuator.image = actuator.type;
    actuator.sortValue = 0;
    actuator.state = {};
    actuator.displayVertical = false;

    switch (actuator.type) {
        case "Lamp":
            {
                actuator.template = "toggleTemplate";
                actuator.sortValue = -7;
                break;
            }
        case "Socket":
            {
                actuator.template = "toggleTemplate";
                actuator.sortValue = -6;
                break;
            }

        case "RollerShutter":
            {
                actuator.template = "rollerShutterTemplate";
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

    extendActuator(actuator);
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
            url: "http://" + controllerAddress + "/api/actuator/" + id + "?body=" + JSON.stringify(request),
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