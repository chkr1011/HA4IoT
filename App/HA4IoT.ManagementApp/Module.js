var app;

(function () {
    app = angular.module('app', []);
    app.factory("translationService", ['$http', createTranslationService]);
    app.factory("controllerProxyService", ['$http', createControllerProxyService]);
})();