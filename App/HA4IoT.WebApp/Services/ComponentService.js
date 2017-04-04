function createComponentService(apiService) {
    var srv = this;

    srv.togglePowerState = function (component) {

        var commandType = "TurnOnCommand";
        if (component.State.PowerState === "On") {
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

    return this;
}