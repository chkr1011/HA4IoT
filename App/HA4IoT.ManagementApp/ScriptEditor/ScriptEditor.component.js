(function () {
    var module = angular.module("app");

    function createController(controllerProxyService, modalService) {
        var c = this;

        c.proxyMethods = {}
        c.result = {
            Duration: 0,
            Exception: null,
            Value: null
        };

        c.loadProxyMethods = function () {
            controllerProxyService.execute(
                "Service/IScriptingService/GetProxyMethods",
                {},
                function (response) {

                    var proxyMethods = [];

                    $.each(response,
                        function (moduleId, module) {
                            $.each(module,
                                function (methodId, method) {
                                    var methodText = method.ReturnType + " " + moduleId + "." + methodId + "(";

                                    var parametersText = "";
                                    $.each(method.Parameters,
                                        function (parameterId, parameter) {
                                            parametersText += parameter.Type + " " + parameterId + ", ";
                                        });

                                    methodText += parametersText + ")";
                                    
                                    proxyMethods.push(methodText);
                                });
                        });

                    c.proxyMethods = proxyMethods;
                });
        };

        c.executeScript = function () {
            var scriptCode = editor.getValue();

            controllerProxyService.execute(
                "Service/IScriptingService/ExecuteScriptCode",
                { "ScriptCode": scriptCode },
                function (response) {
                    c.result = response;

                    if (c.result.Value === null) {
                        c.result.Value = "<nil>";
                    }
                });
        }

        c.loadProxyMethods();
    }

    module.component("scriptEditor", {
        templateUrl: "ScriptEditor/ScriptEditor.component.html",
        controllerAs: "seCtrl",
        controller: ["controllerProxyService", "modalService", createController]
    });

})();