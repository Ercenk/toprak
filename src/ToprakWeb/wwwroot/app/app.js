(function () {
    "use strict";

    var app = angular.module("toprakWeb", [
        // Angular modules 
        "ngRoute",
        "ngResource",
        "cgBusy",
        "ui.bootstrap",
        "ui.grid",
        //"uiGmapgoogle-maps",
        "ngMap",

        // Third parties
        "facebook",
        "angularFileUpload",
        "angular-cache",

        // Application modules
        "common"
    ]);

    function appRun($rootScope, $route) {
        $route.reload();
        $rootScope.user = {};
    }

    app.run(["$rootScope", "$route", appRun]);;
})();