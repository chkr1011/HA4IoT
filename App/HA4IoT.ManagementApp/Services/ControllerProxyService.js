function createControllerProxyService($http) {
    return {

        execute: function (uri, payload, successCallback) {
            var fullUri = "/api/" + uri;

            console.log("COMMAND@" + fullUri);
            console.log(payload);

            $http.post(fullUri, payload).then(successCallback);
        },

        get: function (uri, payload, callback) {
            var fullUri = "/api/" + uri;

            if (payload != null) {
                fullUri += "?body=" + JSON.stringify(payload);
            }

            console.log("GET@" + fullUri);
            console.log(payload);

            $http.get(fullUri).then(function (response) {
                console.log("Response data:");
                console.log(response.data);

                if (callback != null) {
                    callback(response.data);
                }
            });
        }

        // new Promise()
    };
}