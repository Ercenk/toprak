(function () {
    "use strict";

    function adminController($rootScope, $scope, $timeout, services) {
        var vm = this;

        /* jshint validthis:true */
        vm.title = "adminController";

        function loadData() {

            vm.topGonullu = {};
            vm.topSeenTutanak = {};
            vm.remainingTutanak = {};
            vm.gonulluLocation = {};
            //vm.mapControl = {};

            //vm.map = {
            //    center: { latitude: 45, longitude: -73 },
            //    zoom: 3,
            //    options: { scrollwheel: true },
            //    locations: [],
            //    events: { tilesloaded: function(map, event, args) {
            //        google.maps.event.trigger(map, "resize");
            //        console.info("event");
            //    },
            //        idle: function (map, event, args) {
            //            google.maps.event.trigger(map, "resize");
            //            console.info("event");
            //        }
            //    },
            //    refresh: false
            //};

            vm.gonulluLocationGridOptions = {
                data: null,
                enableSorting: true,
                columnDefs: [
                    { field: "Country", displayName: "Ulke" },
                    { field: "City", displayName: "Sehir" },
                    {
                        field: "Analiz", displayName: "Analiz",
                        sortingAlgorithm: function (a, b) {
                            var first = parseInt(a);
                            var second = parseInt(b);
                            if (first < second) {
                                return -1;
                            } else if (first > second) {
                                return 1;
                            } else {
                                return 0;
                            }
                        }
                    }
                ]
            };

            vm.gonulluGridOptions = {
                data: null,
                enableSorting: true,
                columnDefs: [
                    { field: "FirstName", displayName: "Ad" },
                    { field: "LastName", displayName: "Soyad" },
                    { field: "Email", displayName: "Email" },
                    { field: "TotalResponses", displayName: "Tutanaklar" }
                ]
            };
            vm.tutanakGridOptions = {
                data: null,
                enableSorting: true,
                columnDefs: [
                    { field: "Count", displayName: "Goruntulenme" },
                    {
                        field: "Image", displayName: "Tutanak",
                        cellTemplate: "<div class=\"ngCellText\" ng-class=\"col.colIndex()\"><a href=\"{{ COL_FIELD }}\" target=\"_blank\">imaj</a></div>"
                    }
                ]
            };

            vm.unreadableTutanak = {
                data: null,
                enableSorting: true,
                columnDefs: [
                    { field: "Count", displayName: "Goruntulenme" },
                    {
                        field: "Image", displayName: "Tutanak",
                        cellTemplate: "<div class=\"ngCellText\" ng-class=\"col.colIndex()\"><a href=\"{{ COL_FIELD }}\" target=\"_blank\">imaj</a></div>"
                    }
                ]
            };

            vm.kalanTutanakGridOptions = {
                data: null,
                enableSorting: true,
                columnDefs: [
                    { field: "Uri", displayName: "Tutanak", cellTemplate: "<div class=\"ngCellText\" ng-class=\"col.colIndex()\"><a href=\"{{ COL_FIELD }}\" target=\"_blank\">imaj</a></div>" }
                ]
            };

            if ($rootScope.user && !$rootScope.user.isAdmin) {
                return;
            }


            //vm.map.waitPromise = services.userLocations().then(function (result) {
            //    for (var i = 0; i < result.LocationStats.length; i++) {
            //        var location = result.LocationStats[i];
            //        var locationCircle = {
            //            id: i,
            //            stroke: {
            //                color: "#CC0033",
            //                weight: 2,
            //                opacity: 0.5
            //            },
            //            fill: {
            //                color: "#CC0033",
            //                opacity: 0.35
            //            },
            //            geodesic: false, // optional: defaults to false
            //            draggable: false, // optional: defaults to false
            //            clickable: false, // optional: defaults to true
            //            editable: false, // optional: defaults to false
            //            visible: true, // optional: defaults to true
            //            center: { latitude: location.Latitude, longitude: location.Longitude },
            //            radius: Math.sqrt(location.Analiz) * 15000,
            //            control: {},
            //            title: location.Analiz
            //        };
            //        vm.map.locations.push(locationCircle);
            //    }
            //});

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


            vm.gonulluLocation.waitPromise = services.userLocations().then(function (result) {
                vm.gonulluLocationGridOptions.data = result.LocationStats;
            });

            vm.topGonullu.waitPromise = services.topGonullu().then(function (result) {
                vm.gonulluGridOptions.data = result.Users;
            });

            vm.topSeenTutanak.waitPromise = services.topSeenTutanak().then(function (result) {
                vm.tutanakGridOptions.data = result.Result;
            });


            vm.unreadableTutanak.waitPromise = services.unreadableTutanak().then(function (result) {
                vm.unreadableTutanak.data = result.Result;
            });

            vm.remainingTutanak.waitPromise = services.remainingTutanak().then(function (result) {
                vm.kalanTutanakGridOptions.data = result.Uris;
                vm.kalanTutanakQueueLength = result.QueueLength;
            });
        }

        loadData();

        vm.magicButton = function () {
            services.addToCirculation();
        }

        $scope.$on("login_success", function (event, args) {
            vm.firstName = args;
            vm.isAdmin = $rootScope.user.isAdmin;
            vm.$apply();
        });

        $scope.$on("logout_success", function (event, args) {
            vm.firstName = "";
            vm.$apply();
        });

        function activate() { }

        activate();
    }

    angular
        .module("toprakWeb")
        .controller("adminController", adminController);

    adminController.$inject = ["$rootScope", "$scope", "$timeout", "services"];
})();
