(function () {
    "use strict";

    var app = angular.module("toprakWeb");
    
    // Configure the routes and route resolvers
    function routeConfigurator($routeProvider) {
        $routeProvider.when("/tutanak", {
            templateUrl: "app/tutanak/tutanak.html"
        }).when("/admin", {
            templateUrl: "app/admin/admin.html"
        }).otherwise("/tutanak");
    }

    app.config(["$routeProvider", routeConfigurator]);
})();