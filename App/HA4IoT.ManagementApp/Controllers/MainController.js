app.controller("MainController", ['$scope', 'translationService', function ($scope, translationService) {
    return {
        getTranslationValue: function getTranslationValue(name) {
            return translationService.getTranslationValue(name);
        }
    };
}]);