function getUILocalization(key) {
    return getLocalizationFromSource(key, uiLocalizations);
}

function getActuatorLocalization(key) {
    return getLocalizationFromSource(key, actuatorLocalizations);
}

function getLocalizationFromSource(key, source) {
    var result = "#" + key;

    $.each(source, function (i, entry) {
        if (entry.key === key) {
            result = entry.value;
            return;
        }
    });

    return result;
}