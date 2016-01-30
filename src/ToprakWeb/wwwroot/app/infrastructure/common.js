(function () {
    "use strict";

    var commonModule = angular.module("common", []);

    commonModule.provider("commonConfig", function () {

        var events = {
            spinnerToggle: "spinner.toggle",
            userRegistered: "user.registered",
            controllerActivateSuccessEvent: "controller:activsated"
        };

        var keyCodes = {
            backspace: 8,
            tab: 9,
            enter: 13,
            esc: 27,
            space: 32,
            pageup: 33,
            pagedown: 34,
            end: 35,
            home: 36,
            left: 37,
            up: 38,
            right: 39,
            down: 40,
            insert: 45,
            del: 46
        };

        this.config = {
            appErrorPrefix: "ERROR ",
            events: events,
            keyCodes: keyCodes            
        };

        this.$get = function () {
            return {
                config: this.config
            };
        };
    });

    function common($rootScope, $timeout, $q, commonConfig, logger) {

        function $broadcast() {
            return $rootScope.$broadcast.apply($rootScope, arguments);
        }

        function activateController(promises, controllerId) {
            return $q.all(promises).then(function (eventArgs) {
                var data = { controllerId: controllerId };
                $broadcast(commonConfig.config.events.controllerActivateSuccessEvent, data);
            });
        }

        function _extends(_child, _super) {
            for (var p in _child) if (_child.hasOwnProperty(p)) _super[p] = _child[p];
            function __() { this.constructor = _super; }
            __.prototype = _child.prototype;
            _super.prototype = new __();
        }

        var service = {
            // common angular dependencies
            $broadcast: $broadcast,
            $q: $q,
            $timeout: $timeout,
            // generic
            activateController: activateController,
            logger: logger, 
            _extends: _extends
        };

        return service;
    }

    commonModule.factory("common", common);

    common.$inject = ["$rootScope", "$timeout", "$q", "commonConfig", "logger"];

    commonModule.config(["$logProvider", function ($logProvider) {
        // turn debugging off/on (no info or warn)
        if ($logProvider.debugEnabled) {
            $logProvider.debugEnabled(true);
        }
    }]);
})();