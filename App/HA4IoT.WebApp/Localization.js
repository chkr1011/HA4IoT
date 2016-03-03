function getUILocalization(key) {
    var result = uiLocalizations[key];

    if (result === undefined) {
        result = "#" + key;
    }

    return result;
}