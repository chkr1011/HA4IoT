function createComponentService(apiService) {
    var srv = this;

    srv.togglePowerState = function (component) {

        var commandType = "TurnOnCommand";
        if (component.State.PowerState === "On") {
            commandType = "TurnOffCommand";
        }

        apiService.executeCommand(component.Id, commandType, {});
    }

    srv.pressButton = function (component, duration) {
        apiService.executeCommand(component.Id, "PressCommand", { Duration: duration });
    }

    return this;
}