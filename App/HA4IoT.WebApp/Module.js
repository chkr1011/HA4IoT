var app;

(function () {
    app = angular.module("app", []);

    app.factory("localizationService", ["$http", createLocalizationService]);
    app.factory("apiService", ["$http", createApiService]);
    app.factory("componentService", ["apiService", createComponentService]);
    app.factory("modalService", [createModalService]);

    app.controller("AppController", ["$http", "$scope", "modalService", "apiService", "localizationService", "componentService", createAppController]);

})();