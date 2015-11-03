function getUILocalization(key) {
    var result = uiLocalizations[key];

    if (result === undefined) {
        result = "#" + key;
    }

    return result;
}

function getActuatorLocalization(key) {
    var result = actuatorLocalizations[key];

    if (result === undefined) {
        result = "#" + key;
    }

    return result;
}