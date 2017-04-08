function createApiService() {
    var srv = this;

    srv.statusHash = "";

    srv.apiStatus =
        {
            isApiReachable: false,
            activeCalls: 0,
            errorMessage: null
        };

    srv.apiStatusUpdatedCallback = null;
    srv.newStatusReceivedCallback = null;

    srv.pollStatus = function () {
        var successHandler = function (response) {

            if (srv.statusHash === response.ResultHash) {
                setTimeout(function () { srv.pollStatus(); }, 500);
                return;
            }

            srv.statusHash = response.ResultHash;
            console.log("New status received");

            if (srv.newStatusReceivedCallback != null) {
                srv.newStatusReceivedCallback(response.Result);
            }

            srv.pollStatus();
        };

        var errorHandler = function () {
            setTimeout(function () { srv.pollStatus(); }, 1000);
        }

        srv.executeApi("GetStatus", {}, srv.statusHash, successHandler, errorHandler);
    };

    srv.executeCommand = function (componentId, commandType, parameter, doneCallback) {
        var payload = parameter;
        payload.ComponentId = componentId;
        payload.CommandType = commandType;

        srv.executeApi("Service/IComponentRegistryService/ExecuteCommand", payload, null, doneCallback);
    }

    srv.getConfiguration = function (doneCallback) {
        srv.executeApi("GetConfiguration", {}, null, doneCallback);
    }

    srv.updateComponentSettings = function (componentId, settings) {
        var parameter = {
            Uri: "Component/" + componentId,
            Settings: settings
        };

        srv.executeApi("Service/ISettingsService/Import", parameter, null);
    }

    srv.executeApi = function (action, parameter, resultHash, doneCallback, failCallback) {
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

        srv.apiStatus.activeCalls++;
        $.ajax(options).done(function (response) {
            srv.apiStatus.isApiReachable = true;
            srv.apiStatus.errorMessage = null;
            srv.apiStatus.activeCalls--;

            if (srv.apiStatusUpdatedCallback != null) {
                srv.apiStatusUpdatedCallback(srv.apiStatus);
            }

            if (doneCallback != null) {
                doneCallback(response);
            }
        }).fail(function (jqXhr, textStatus, errorThrown) {
            srv.apiStatus.isApiReachable = false;
            srv.apiStatus.errorMessage = textStatus;
            srv.apiStatus.activeCalls--;

            if (srv.apiStatusUpdatedCallback != null) {
                srv.apiStatusUpdatedCallback(srv.apiStatus);
            }

            if (failCallback != null) {
                failCallback();
            }
        });
    }

    return this;
}