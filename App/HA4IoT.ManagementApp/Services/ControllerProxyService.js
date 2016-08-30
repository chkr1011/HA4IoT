function createControllerProxyService($http) {
    return {
        mode: "WebSocket",
        address: "192.168.1.15",

        execute: function (uri, payload) {
            var fullUri = "http://" + this.address + "/api/" + uri;

            console.log("COMMAND@" + fullUri);
            console.log(payload);

            $http.post(fullUri, payload);
        },

        get: function (uri, payload, callback) {
            var fullUri = "http://" + this.address + "/api/" + uri + "?body=" + JSON.stringify(payload);

            console.log("GET@" + fullUri);
            console.log(payload);

            $http.get(fullUri).then(function (response) {
                console.log("Response data:");
                console.log(response.data);

                callback(response.data);
            });
        }
    };
}