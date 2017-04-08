function createComponentService(apiService) {
    var srv = this;

    srv.togglePowerState = function (component) {

        var commandType = "TurnOnCommand";
        if (component.State.PowerState.Value === "On") {
            commandType = "TurnOffCommand";
        }

        apiService.executeCommand(component.Id, commandType, {});
    }

    srv.invokeRollerShutter = function (component, action) {
        var commandType = "TurnOffCommand";
        if (action === "MoveUp") {
            commandType = "MoveUpCommand";
        }
        else if (action === "MoveDown") {
            commandType = "MoveDownCommand";
        }

        apiService.executeCommand(component.Id, commandType, {});
    }

    srv.setColor = function (component, hue, saturation, value) {

        var command = { Hue: hue, Saturation: saturation, Value: value };
        apiService.executeCommand(component.Id, "SetColorCommand", command);
    }

    srv.pressButton = function (component, duration) {
        apiService.executeCommand(component.Id, "PressCommand", { Duration: duration });
    }

    srv.setState = function (component, state) {
        apiService.executeCommand(component.Id, "SetStateCommand", { Id: state });
    }

    srv.setLevel = function (component, level) {
        apiService.executeCommand(component.Id, "SetLevelCommand", { Level: level });
    }

    srv.increaseLevel = function (component) {
        apiService.executeCommand(component.Id, "IncreaseLevelCommand", {});
    }

    srv.decreaseLevel = function (component) {
        apiService.executeCommand(component.Id, "DecreaseLevelCommand", {});
    }

    srv.disable = function (component) {
        apiService.updateComponentSettings(component.Id, { IsEnabled: false });
    }

    srv.enable = function (component) {
        apiService.updateComponentSettings(component.Id, { IsEnabled: true });
    }

    return this;
}