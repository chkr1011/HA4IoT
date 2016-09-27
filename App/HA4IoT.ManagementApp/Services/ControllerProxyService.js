function createControllerProxyService($http) {
    return {

        execute: function (uri, payload, successCallback) {
            var fullUri = "/api/" + uri;

            console.log("COMMAND@" + fullUri);
            console.log(payload);

            $http.post(fullUri, payload).then(successCallback);
        },

        get: function (uri, callback) {
            var fullUri = "/api/" + uri;

            console.log("GET@" + fullUri);

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