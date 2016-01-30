(function () {
    "use strict";
    var controllerId = "layoutController";

    function layoutController($rootScope, $scope, $location, $interval, services, common,
        //uiGmapIsReady, 
        $timeout) {
        /* jshint validthis:true */
        var vm = this;
        vm.title = "layoutController";

        $location.path('/tutanak');

        vm.userStats = {};

        vm.buttonText = "Admin";

        vm.mapControl = {};

        //vm.map = {
        //    center: { latitude: 45, longitude: -73 },
        //    zoom: 3,
        //    options: { scrollwheel: true },
        //    locations: [],
        //    mapLocations: [],
        //    events: {
        //    //    tilesloaded: function (map, event, args) {
        //    //    google.maps.event.trigger(map, "resize");
        //    //    console.info("event");
        //    //},
        //    //    idle: function (map, event, args) {
        //    //        google.maps.event.trigger(map, "resize");
        //    //        console.info("event");
        //    //    }
        //    },
        //    refresh: false
        //};      

        //uiGmapIsReady.promise().then(function (maps) {
        //    var mapObject = vm.mapControl.getGMap();
        //    if (mapObject) {
        //        $timeout(function () {
        //            google.maps.event.trigger(mapObject, "resize");
        //            var newCenter = new google.maps.LatLng(vm.map.center.latitude, vm.map.center.longitude);
        //            mapObject.setCenter(newCenter);
        //            console.info("timeout");
        //            vm.map.refresh = true;

        //        }, 2000);
        //    }
        //});

        vm.toggleView = function() {
            if (vm.buttonText === "Tutanak") {
                $location.path("/tutanak");
                vm.buttonText = "Admin";
            } else {
                $location.path("/admin");
                vm.buttonText = "Tutanak";
            }
        }

        function updateUserStats()
        {
            var promise = services.getUserStats();
            promise.then(function(result) {
                vm.userStats = result;
            }, function(error) {
                console.info(error);
            });

            return promise;
        }

        $interval(function () {
            vm.waitPromise = updateUserStats();
        }.bind(this), 120000);

        vm.remove = function() {
            vm.map.locations = [];
        }

        var ha = [
            { radius: 2714856, center: [41.878113, -87.629798] },
            { radius: 8405837, position: [40.714352, -74.005973] },
            { radius: 3857799, position: [34.052234, -118.243684] },
            { radius: 603502, position: [49.25, -123.1] }
        ];

        $scope.$on("login_success", function(event, args) {
            vm.firstName = args;
            services.authorize().then(function (role) {
                 if (role.Value === "Admin") {
                     vm.isAdmin = $rootScope.user.isAdmin = true;                    
                     //vm.map.waitPromise = services.userLocations().then(function (result) {
                     //    if (vm.map.locations.length > 0) {
                     //        vm.map.locations = [];
                     //    }
                     //        for (var i = 0; i < result.LocationStats.length; i++) {
                     //            var location = result.LocationStats[i];
                     //            var locationCircle = {
                     //                id: i,
                     //                stroke: {
                     //                    color: "#CC0033",
                     //                    weight: 2,
                     //                    opacity: 0.5
                     //                },
                     //                fill: {
                     //                    color: "#CC0033",
                     //                    opacity: 0.35
                     //                },
                     //                geodesic: false, // optional: defaults to false
                     //                draggable: false, // optional: defaults to false
                     //                clickable: false, // optional: defaults to true
                     //                editable: false, // optional: defaults to false
                     //                visible: true, // optional: defaults to true
                     //                center: { latitude: location.Latitude, longitude: location.Longitude },
                     //                radius: Math.sqrt(location.Analiz) * 15000,
                     //                control: {},
                     //                title: location.Analiz
                     //            };
                     //            vm.map.locations.push(locationCircle);

                     //            var locationCircle1 = {
                     //                id: i,
                     //                center: [location.Latitude, location.Longitude],
                     //                radius: Math.sqrt(location.Analiz) * 15000
                     //            };
                     //            vm.map.mapLocations.push(locationCircle1);
                     //        }                         
                     //});

                 } else {
                    vm.isAdmin = $rootScope.user.isAdmin = false;
                }
                vm.waitPromise = updateUserStats();
            });
        });

        $scope.$on("logout_success", function (event, args) {
            vm.firstName = "";
            $scope.$apply();
        });

        $scope.$on("update_stats", function (event, args) {
            vm.waitPromise = updateUserStats();
        });


        function activate() {
            vm.waitPromise = common.activateController([updateUserStats()], controllerId);
        }

        activate();
    }

    angular
        .module("toprakWeb")
        .controller(controllerId, layoutController);

    layoutController.$inject = ["$rootScope", "$scope", "$location", "$interval", "services", "common",
        //"uiGmapIsReady",
        "$timeout"];
})();
