(function () {
    "use strict";

    function logger($log) {
        var showToast = true;

        toastr.options.timeOut = 4000;
        toastr.options.positionClass = "toast-bottom-right";
        toastr.options.closeButton = true;

        function logIt(message, data, source, toastType) {
            var write = (toastType === "error") ? $log.error : $log.log;
            source = source ? "[" + source + "] " : "";
            write(source, message, data);
            if (showToast) {
                if (toastType === "error") {
                    console.log(message);
                    toastr.error(message);
                } else if (toastType === "warning") {
                    toastr.warning(message);
                } else if (toastType === "success") {
                    toastr.success(message);
                } else {
                    toastr.info(message);
                }
            }
        }

        function log(message, data, source) {
            logIt(message, data, source, "info");
        }

        function logWarning(message, data, source) {
            logIt(message, data, source, "warning");
        }

        function logSuccess(message, data, source) {
            logIt(message, data, source, "success");
        }

        function logError(message, data, source) {
            logIt(message, data, source, "error");
        }

        var service = {
            log: log,
            logError: logError,
            logSuccess: logSuccess,
            logWarning: logWarning
        };

        return service;
    }

    angular
        .module("common")
        .factory("logger", logger);

    logger.$inject = ["$log"];
})();