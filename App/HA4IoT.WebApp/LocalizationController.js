app.controller("LocalizationController", ["$scope", "$http", localizationController]);

function localizationController($scope, $http) {
    var c = this;
    c.uiLocalizations = {};
    c.isInitialized = false;

    c.getUILocalization = function (key) {
        var result = c.uiLocalizations[key];

        if (result === undefined) {
            result = "#" + key;
        }

        return result;
    };

    c.loadUILocalizations = function (language) {
        console.log("Loading localization '" + language + "'");

        $http.get("UILocalizations-" + language + ".json").then(function (response) {
            if (response.status !== 200) {
                alert("Error while loading localization '" + language + "' (" + response.status + ").");
            } else {
                c.uiLocalizations = response.data;
                c.isInitialized = true;
            }
        });
    };

    $scope.$on("configurationLoaded", function (event, args) { c.loadUILocalizations(args.language) });
}