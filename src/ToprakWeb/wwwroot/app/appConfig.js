(function () {
    "use strict";

    var app = angular.module("toprakWeb");

    app.config(["commonConfigProvider", function (cfg) {
        cfg.config.dataEndpoint = window.location.protocol + "//" + window.location.host + "/api/v1/";
        cfg.config.version = "1.0.0";
    }]);

    app.config(["FacebookProvider", function (facebook) {
        if (window.location.host.indexOf("localhost") > -1) {
            facebook.init("");
        } else {
            facebook.init("");
        }
    }]);

    app.config([
        "CacheFactoryProvider", function(cacheFactoryProvider) {
            angular.extend(cacheFactoryProvider.defaults, { maxAge: 15 * 60 * 1000 });
        }
    ]);

})();