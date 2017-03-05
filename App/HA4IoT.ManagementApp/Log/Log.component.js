(function () {
    var module = angular.module("app");

    function createController(controllerProxyService, modalService) {

        var ctrl = this;

        ctrl.offset = 0;
        ctrl.sessionId = null;

        ctrl.AutoScroll = true;
        ctrl.LogEntries = [];

        ctrl.fetchLog = function () {

            controllerProxyService.get("Service/ILogService/GetLogEntries", { "Offset": ctrl.offset, "SessionId": ctrl.sessionId },
                function (response) {
                    ctrl.appendLog(response);
                });
        }

        ctrl.appendLog = function (source) {
            ctrl.sessionId = source.SessionId;

            $.each(source.LogEntries, function (id, item) {
                item.Text = "[" + item.Id + "] [" + item.Timestamp + "] [" + item.ThreadId + "] [" + item.Source + "]: " + item.Message;
                if (item.Exception != null) {
                    item.Text += "\r\n" + item.Exception;
                }

                ctrl.LogEntries.push(item);
                ctrl.offset = item.Id;
            });

            if (source.LogEntries.length === 0) {
                setTimeout(function () { ctrl.fetchLog(); }, 1000);
            } else {
                if (ctrl.AutoScroll) {
                    ctrl.scrollToEnd("logView");
                }

                ctrl.fetchLog();
            }
        }

        ctrl.clear = function () {
            ctrl.LogEntries = [];
        }

        ctrl.scrollToEnd = function (id) {
            var div = document.getElementById(id);
            $('#' + id).animate({
                scrollTop: div.scrollHeight - div.clientHeight
            }, 500);
        }

        ctrl.fetchLog();
    }

    module.component("log", {
        templateUrl: "Log/Log.component.html",
        controllerAs: "lCtrl",
        controller: ["controllerProxyService", "modalService", createController]
    });

})();