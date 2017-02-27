function createApiService() {
    var srv = this;

    srv.executeCommand = function (componentId, commandType, parameter, doneCallback) {
        var payload = parameter;
        payload.ComponentId = componentId;
        payload.CommandType = commandType;

        var action = {
            Action: "Service/IComponentRegistryService/ExecuteCommand",
            Parameter: payload
        };

        var url = "/api/Execute?body=" + JSON.stringify(action);
        var options = {
            method: "POST",
            url: url,
            timeout: 2500
        };

        $.ajax(options).done(function () {
            if (doneCallback != null) {
                doneCallback();
            }
        }).fail(function (jqXHR, textStatus, errorThrown) {
            alert(textStatus);
        });
    }

    srv.executeApi = function (action, parameter, resultHash, doneCallback) {
        var request = {
            Action: action,
            Parameter: parameter,
            ResultHash: resultHash
        }

        var options = {
            method: "POST",
            url: "/api/Execute?body=" + JSON.stringify(request),
            timeout: 2500
        };

        $.ajax(options).done(function (response) {
            if (doneCallback != null) {
                doneCallback(response);
            }
        }).fail(function (jqXHR, textStatus, errorThrown) {
            alert(textStatus);
        });;
    }

    return this;
}