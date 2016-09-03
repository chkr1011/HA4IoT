(function () {
    var module = angular.module("app");

    function createController(controllerProxyService) {

        ctrl = this;

        ctrl.Model = [];

        loadDemoData();
    }

    function moveArea(area, direction) {
        if (area.SortValue == 0 && direction == 'up')
        {
            return;
        }

        if (area.SortValue == ctrl.Mode.length - 1 && direction == 'down')
        {
            return;
        }

        if (direction = "up") area.SortValue++;
        if (direction = "down") area.SortValue--;
    }

    function loadDemoData() {

        var demoData = {
            Areas: {
                Bedroom: {
                    Settings: {
                        AppSettings: {
                            "Image": "Air",
                            "Caption": "Schlafzimmer",
                            "SortValue": 0
                        }
                    }
                }
            }
        };

        $.each(demoData.Areas, function (id, area) {
            ctrl.Model.push({
                Id: id,
                Caption: area.Settings.AppSettings.Caption,
                SortValue: area.Settings.AppSettings.SortValue
            });
        });
    }

    module.component("areas", {
        templateUrl: "Areas/AreasOverview.component.html",
        controllerAs: "aoCtrl",
        controller: ["controllerProxyService", createController]
    });

})();