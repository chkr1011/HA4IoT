function createTranslationService($http)
{
    return {
        getTranslationValue: function(name)
        {
            return '#' + name;
        }
    };
}