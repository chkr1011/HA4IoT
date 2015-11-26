setupController();

var actuatorLocalizations = [];
var uiLocalizations = [];

function getVersion(callback) {
    $.get("cache.manifest", function (data) {
        var parser = new RegExp("# Version ([0-9|.]*)", "");
        var results = parser.exec(data);

        callback(results[1]);
    });
}

function loadUILocalizations(callback) {
    $.getJSON("/app/UILocalizations.json").success(function (result) {
        uiLocalizations = result;
    }).fail(function (jqXHR, textStatus, errorThrown) {
        alert(textStatus);
    }).always(function () { callback(); });
}

function loadActuatorLocalizations(callback) {
    $.getJSON("/app/ActuatorLocalizations.json").success(function (result) {
        actuatorLocalizations = result;
    }).fail(function (jqXHR, textStatus, errorThrown) {
        alert(textStatus);
    }).always(function () { callback(); });
}

function setupController() {

    var app = angular.module("app", []);
    app.controller("HomeAutomationController", [
      "$scope", "$http",
      function ($scope, $http) {
          var c = this;

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
          c.notifications = [];

          getVersion(function (version) {
              c.version = version;
          });

          c.getUILocalization = function (key) {
              return getUILocalization(key);
          };

          c.generateRooms = function () {

              $http.get("/api/configuration").success(function (data) {

                  $.each(data.rooms, function (roomId, room) {
                      if (room.hide) {
                          return true;
                      }

                      var roomControl = {};
                      roomControl.id = roomId;
                      roomControl.caption = getActuatorLocalization(roomId);
                      roomControl.actuators = [];

                      $.each(room.actuators, function (actuatorId, actuator) {

                          actuator.id = actuatorId;

                          configureActuator(room, actuator);

                          if (actuator.hide) {
                              return true;
                          }

                          if (actuator.type === "HA4IoT.Actuators.TemperatureSensor" ||
                              actuator.type === "HA4IoT.Actuators.HumiditySensor") {
                              c.sensors.push(actuator);
                          } else if (actuator.type === "HA4IoT.Actuators.RollerShutter") {
                              c.rollerShutters.push(actuator);
                          } else if (actuator.type === "HA4IoT.Actuators.MotionDetector") {
                              c.motionDetectors.push(actuator);
                          } else if (actuator.type === "HA4IoT.Actuators.Window") {
                              c.windows.push(actuator);
                          }

                          roomControl.actuators.push(actuator);
                      });

                      c.rooms.push(roomControl);
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
              getJSON(c, "/api/status", function (data) {

                  $.each(data.status, function (id, state) {
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
              var newState = "On";
              if (actuator.state.state === "On") {
                  newState = "Off";
              }

              invokeActuator(actuator.id, { state: newState }, function () { actuator.state.state = newState; });
          };

          $scope.loadNotifications = function () {
              $.getJSON("/api/notifications", function (data) {
                  c.notifications = data.notifications;
                  c.notifications.reverse();
              });
          }

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

          loadUILocalizations(function () { loadActuatorLocalizations(function () { c.generateRooms(); }) });
      }
    ]);
}

function configureActuator(room, actuator) {
    actuator.image = actuator.type;
    actuator.sortValue = 0;
    actuator.caption = getActuatorLocalization(actuator.id);
    actuator.overviewCaption = getActuatorLocalization(actuator.id + ".Overview");
    actuator.hide = false;
    actuator.displayVertical = false;
    actuator.state = {};

    switch (actuator.type) {
        case "HA4IoT.Actuators.Lamp":
            {
                actuator.template = "Views/ToggleTemplate.html";
                actuator.sortValue = -7;
                break;
            }
        case "HA4IoT.Actuators.Socket":
            {
                actuator.template = "Views/ToggleTemplate.html";
                actuator.sortValue = -6;
                break;
            }

        case "HA4IoT.Actuators.RollerShutter":
            {
                actuator.caption = getUILocalization("UI.RollerShutter");
                actuator.template = "Views/RollerShutterTemplate.html";
                actuator.sortValue = -4;
                break;
            }

        case "HA4IoT.Actuators.Window":
            {
                actuator.caption = getUILocalization("UI.Window");
                actuator.template = "Views/WindowTemplate.html";
                actuator.sortValue = -4;
                break;
            }

        case "HA4IoT.Actuators.StateMachine":
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

        case "HA4IoT.Actuators.TemperatureSensor":
            {
                actuator.template = "Views/TemperatureSensorTemplate.html";
                actuator.caption = getUILocalization("UI.Temperature");
                actuator.sortValue = -10;
                break;
            }

        case "HA4IoT.Actuators.HumiditySensor":
            {
                actuator.template = "Views/HumiditySensorTemplate.html";
                actuator.caption = getUILocalization("UI.Humidity");
                actuator.sortValue = -9;
                break;
            }

        case "HA4IoT.Actuators.MotionDetector":
            {
                actuator.template = "Views/MotionDetectorTemplate.html";
                actuator.caption = getUILocalization("UI.MotionDetector");
                actuator.sortValue = -8;
                break;
            }

        case "HA4IoT.Actuators.VirtualButton":
            {
                actuator.template = "Views/VirtualButtonTemplate.html";
                actuator.sortValue = -1;
                break;
            }

        case "HA4IoT.Actuators.VirtualButtonGroup":
            {
                actuator.template = "Views/VirtualButtonGroupTemplate.html";
                actuator.image = "HA4IoT.Actuators.VirtualButton";
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
                actuator.hide = true;
                return;
            }
    }

    appConfiguration.actuatorExtender(actuator);
}

function getJSON(controller, url, callback) {
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
    var url = "/api/actuator/" + id + "?body=" + JSON.stringify(request);

    // The hack with the body as query is required to allow cross site calls.
    $.ajax({ method: "POST", url: url, timeout: 2500 }).success(function () {
        if (successCallback != null) {
            successCallback();
        }
    }).fail(function (jqXHR, textStatus, errorThrown) {
        alert(textStatus);
    });
}
