(function () {
    "use strict";



    function services($resource, $rootScope, $http, identityService, common) {
        var $q = common.$q;

        function setHeaders() {
            var deferred = $q.defer();

            identityService.getHttpHeader().then(function (headerValue) {
                $http.defaults.headers.common["ToprakAuth"] = headerValue;
                deferred.resolve();
            }, function (error) {
                deferred.reject();
            });

            return deferred.promise;
        };

        function resourceExecuter(url, parameters) {
            if (!parameters) {
                parameters = {};
            }
            var executer = function () {
                var deferred = $q.defer();
                setHeaders().then(function (result) {
                    var res = $resource(
                        url,
                        parameters);

                    res.get().$promise.then(function (serviceResult) {
                        deferred.resolve(serviceResult);
                    }, function (error) {
                        deferred.reject(error);
                    });
                }, function(headersError) {
                    console.info(headersError);
                    deferred.reject(headersError);
                });
                return deferred.promise;
            };

            return executer;
        }

        var authorize = resourceExecuter("/api/v1/Authorize");

        var postData = function (tutanak) {
            var deferred = $q.defer();
            setHeaders().then(function (result) {
                setHeaders();
                var res = $resource(
                    "/api/v1/Tutanak/Kaydet",
                    {});
                res.save(tutanak).$promise.then(function (serviceResult) {
                    deferred.resolve(serviceResult);
                }, function (error) {
                    deferred.reject(error);
                });
            });
            return deferred.promise;
        };

        var getTutanak = resourceExecuter("/api/v1/Tutanak/GetImageUri");
       
        var getUserStats = resourceExecuter("/api/v1/Tutanak/GetUserStats");
           
        var getUploadUrl = function (fileName) {
            var deferred = $q.defer();
            setHeaders().then(function (result) {
                var res = $resource(
                    "/api/v1/Admin/GetUploadUrl/:fileName",
                    { fileName: "@fileName" });

                res.get({ fileName: fileName }).$promise.then(function(serviceResult) {
                    deferred.resolve(serviceResult);
                }, function(error) {
                    deferred.reject(error);
                });
            });
            return deferred.promise;
        };

        var setupUpload = resourceExecuter("/api/v1/Admin/SetupUpload");
          
        var transferFromStaging = resourceExecuter("/api/v1/Admin/TransferFromStaging");
        var remainingTutanak = resourceExecuter("/api/v1/Admin/RemainingTutanak");
        var topSeenTutanak = resourceExecuter("/api/v1/Admin/TopSeenTutanak");
        var topGonullu = resourceExecuter("/api/v1/Admin/TopGonullu");
        var addToCirculation = resourceExecuter("/api/v1/Admin/AddToCirculation");
        var unreadableTutanak = resourceExecuter("/api/v1/Admin/UnreadableTutanak");
        var userLocations = resourceExecuter("/api/v1/Admin/GetUserLocations");
              

        var getTemsilcilikler = resourceExecuter("/api/v1/Tutanak/GetTemsilcilikler", {"get": {method: "GET", isArray: true}});

        var service = {
            authorize: authorize,
            postData: postData,
            getTutanak: getTutanak,
            getUploadUrl: getUploadUrl,
            setupUpload: setupUpload,
            transferFromStaging: transferFromStaging,
            getUserStats: getUserStats,
            getTemsilcilikler: getTemsilcilikler,
            remainingTutanak: remainingTutanak,
            topSeenTutanak: topSeenTutanak,
            topGonullu: topGonullu,
            addToCirculation: addToCirculation,
            unreadableTutanak: unreadableTutanak,
            userLocations: userLocations
    };

        return service;
    }

    angular
        .module("toprakWeb")
        .factory("services", services);

    services.$inject = ["$resource", "$rootScope", "$http", "identityService", "common"];
})();