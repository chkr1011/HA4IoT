function createControllerProxyService($http) {
    return {

        execute: function (action, parameter, successCallback) {
            var request = {
                "Action": action,
                "Parameter": parameter
            }

            var uri = "/api/Execute";
            console.log("COMMAND@" + uri);
            console.log(request);

            $http.post(uri, request).then(function (response) {
                console.log("Response data:");
                console.log(response.data.Result);

                if (successCallback != null) {
                    successCallback(response.data.Result);
                }
            });
        },

        get: function (action, parameter, successCallback) {
            var request = {
                "Action": action,
                "Parameter": parameter
            }

            var uri = "/api/Execute?body=" + JSON.stringify(request);
            console.log("GET@" + uri);

            $http.get(uri).then(function (response) {
                console.log("Response data:");
                console.log(response.data.Result);

                if (successCallback != null) {
                    successCallback(response.data.Result);
                }
            });
        }

        // new Promise()
    };
}