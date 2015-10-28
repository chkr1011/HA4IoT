setupController();

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
      "$scope", "$http",
      function ($scope, $http) {
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

          getVersion(function (version) {
              c.version = version;
          });

          c.getUILocalization = function(key) {
              return getUILocalization(key);
          };

          c.generateRooms = function () {
              $http.get(getApiUrl() + "configuration").success(function (data) {

                  $.each(data, function (roomIndex, room) {
                      configureRoom(room);

                      if (!room.hide) {
                          for (var i = room.actuators.length - 1; i >= 0; i--) {
                              var actuator = room.actuators[i];

                              configureActuator(room, actuator, i);

                              if (actuator.type === "TemperatureSensor" ||
                                actuator.type === "HumiditySensor") {
                                  c.sensors.push(actuator);
                              } else if (actuator.type === "RollerShutter") {
                                  c.rollerShutters.push(actuator);
                              } else if (actuator.type === "MotionDetector") {
                                  c.motionDetectors.push(actuator);
                              } else if (actuator.type === "Window") {
                                  c.windows.push(actuator);
                              }
                          }

                          c.rooms.push(room);
                      }
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

                  if (c.rooms.length === 1) {
                      c.setActivePanel(c.rooms[0].id);
                  }

                  c.pollStatus();
                  c.isReady = true;

                  $("#content").removeClass("hidden");
                  $("body").css("background", "white");
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
              getJSON(c, getApiUrl() + "status?" + c.latestStateHash, function (
                data) {

                  if (c.latestStateHash === data._hash) {
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

              setTimeout(function () {
                  c.pollStatus();
              }, c.appConfiguration.pollInterval);
          };

          $scope.toggleState = function (actuator) {
              var newState = !actuator.state.stateBool;

              var tag = "off";
              if (newState === true) {
                  tag = "on";
              }

              invokeActuator(actuator.id, { state: tag }, function () { actuator.state.stateBool = newState; });
          };

          $scope.invokeVirtualButton = function (actuator) {
              invokeActuator(actuator.id, {});
              c.pollStatus();
          }

          $scope.invokeVirtualButtonGroup = function (actuator, button) {
              invokeActuator(actuator, { button: button });
              c.pollStatus();
          }

          $scope.toggleIsEnabled = function (actuator) {
              var newState = !actuator.state.isEnabled;

              invokeActuator(actuator.id, {
                  isEnabled: newState
              }, function () {
                  actuator.state.isEnabled = newState;
              });
          };

          $scope.setState = function (actuator, newState) {
              invokeActuator(actuator.id, {
                  state: newState
              }, function () {
                  actuator.state.state = newState;
              });
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
    room.caption = getActuatorLocalization(room.id);
    room.sortValue = -1;
    room.hide = false;

    appConfiguration.roomExtender(room);
}

function binaryActuatorStateUpdater(actuator, newState) {
    if (newState.state === "On") {
        actuator.state.stateBool = true;
    } else {
        actuator.state.stateBool = false;
    }
}

function configureActuator(room, actuator, i) {
    actuator.caption = getActuatorLocalization(actuator.id);
    actuator.sortValue = 0;
    actuator.hide = false;
    actuator.image = actuator.type;

    actuator.state = {};
    actuator.displayVertical = false;

    switch (actuator.type) {
        case "Lamp":
            {
                actuator.template = "Views/ToggleTemplate.html";
                actuator.updateState = binaryActuatorStateUpdater;
                actuator.sortValue = -7;
                break;
            }
        case "Socket":
            {
                actuator.template = "Views/ToggleTemplate.html";
                actuator.updateState = binaryActuatorStateUpdater;
                actuator.sortValue = -6;
                break;
            }

        case "RollerShutter":
            {
                actuator.caption = getUILocalization("UI.RollerShutter");
                actuator.template = "Views/RollerShutterTemplate.html";
                actuator.sortValue = -4;
                break;
            }

        case "Window":
            {
                actuator.caption = getUILocalization("UI.Window");
                actuator.template = "Views/WindowTemplate.html";
                actuator.sortValue = -4;
                break;
            }

        case "StateMachine":
            {
                actuator.template = "Views/StateMachineTemplate.html";
                actuator.sortValue = -5;

                var extendedStates = [];
                $.each(actuator.states, function (i, state) {
                    extendedStates.push({ value: state, caption: getActuatorLocalization(actuator.id + "." + state) });
                });

                actuator.states = extendedStates;
                break;
            }

        case "TemperatureSensor":
            {
                actuator.template = "Views/TemperatureSensorTemplate.html";
                actuator.caption = getUILocalization("UI.Temperature");
                actuator.sortValue = -10;
                break;
            }

        case "HumiditySensor":
            {
                actuator.template = "Views/HumiditySensorTemplate.html";
                actuator.caption = getUILocalization("UI.Humidity");
                actuator.sortValue = -9;
                break;
            }

        case "MotionDetector":
            {
                actuator.template = "Views/MotionDetectorTemplate.html";
                actuator.caption = getUILocalization("UI.MotionDetector");
                actuator.sortValue = -8;
                break;
            }

        case "VirtualButton":
            {
                actuator.template = "Views/VirtualButtonTemplate.html";
                actuator.image = "Button";
                actuator.sortValue = -1;
                break;
            }

        case "VirtualButtonGroup":
            {
                actuator.template = "Views/VirtualButtonGroupTemplate.html";
                actuator.image = "Button";
                actuator.sortValue = -1;

                var extendedButtons = [];
                $.each(actuator.buttons, function (i, button) {
                    extendedButtons.push({ id: button, caption: getActuatorLocalization(actuator.id + "." + button) });
                });

                actuator.buttons = extendedButtons;
                break;
            }

        default:
            {
                room.actuators.splice(i, 1);
                return;
            }
    }

    actuator.overviewCaption = actuator.caption;

    appConfiguration.actuatorExtender(actuator);
}

function getJSON(controller, url, callback) {
    //$.getJSON({
    //    url: url,
    //    timeout: 2500
    //}).success(function(result) {
    //    controller.errorMessage = "";
    //    callback(result);
    //}).fail(function(jqXHR, textStatus, errorThrown) {
    //    alert(textStatus);
    //});

    $.ajax({
        method: "GET",
        url: url,
        timeout: 2500
    }).success(function (result) {
        controller.errorMessage = "";
        callback(result);
    }).fail(function (jqXHR, textStatus, errorThrown) {
        controller.errorMessage = textStatus;
    });
};

function invokeActuator(id, request, successCallback) {
    var url = getApiUrl() + "actuator/" + id + "?body=" + JSON.stringify(request);

    // The hack with the body as query is required to allow cross site calls.
    $.ajax({ method: "POST", url: url, timeout: 2500 }).success(function () {
        if (successCallback != null) {
            successCallback();
        }
    }).fail(function (jqXHR, textStatus, errorThrown) {
        alert(textStatus);
    });
}
