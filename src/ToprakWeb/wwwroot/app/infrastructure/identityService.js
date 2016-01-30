(function () {
    "use strict";

    function identityService($rootScope, $timeout, facebook, common) {
        var $q = common.$q;

        function emptyUser() {
            return {
                isAuthenticated: false,
                isAdmin: false,
                accessToken: "",
                authType: "none",
                firstName: "",
                email: ""
            }
        };

        $rootScope.user = emptyUser();

        function updateUserWithFacebookAndBroadcast(response, apiResponse) {
            $rootScope.user.isAuthenticated = true;
            $rootScope.user.accessToken = response.authResponse.accessToken;
            $rootScope.user.authType = "facebook";
            $rootScope.user.firstName = apiResponse.first_name;
            $rootScope.user.email = apiResponse.email;
            $rootScope.$broadcast("login_success", apiResponse.first_name);
        }

        function facebookLoginStatusResponse(response) {

        }

        function updateStatus() {
            var deferred = $q.defer();

            facebook.getLoginStatus(function (response) {
                if (response.status === "connected") {
                    facebook.api("/me", { fields: "first_name,email" }, function (apiResponse) {
                        updateUserWithFacebookAndBroadcast(response, apiResponse);
                        deferred.resolve();
                    });
                } else {
                    deferred.resolve();
                }
            }, function (error) {
                console.info(error);
                deferred.resolve();
            });

            return deferred.promise;
        }

        function facebookLogin() {
            if ($rootScope.user && !$rootScope.user.isAuthenticated) {
                return $q(function (resolve, reject) {
                    facebook.login(function (response) {

                        if (response.status === "connected") {
                            facebook.api("/me", { fields: "first_name,email" }, function (apiResponse) {
                                updateUserWithFacebookAndBroadcast(response, apiResponse);
                                resolve($rootScope.user);
                            });
                        } else {
                            $rootScope.user.isAdmin = false;
                            $rootScope.user.isAuthenticated = false;
                            reject($rootScope.user);
                        }
                    }, { scope: "email" });
                });
            } else if (!$rootScope.user && $rootScope.user.isAuthenticated) {
                return $q(function (resolve, reject) {
                    resolve($rootScope.user);
                });
            }

            return $q(function (resolve, reject) {
                reject($rootScope.user);
            });
        };

        function googleLogin() {

        };

        function logout() {
            if ($rootScope.user.authType === "facebook") {
                return $q(function (resolve, reject) {
                    facebook.logout(
                        function () {
                            $rootScope.user = emptyUser();
                            $rootScope.$broadcast("logout_success");
                            resolve($rootScope.user);
                        },
                        function () {
                            console.log("cannot log out from Facebook!");
                            reject("cannot logout");
                        });
                });
            } else if ($rootScope.user.authType === "google") {
                return $q(function (resolve, reject) { });
            }
            return $q(function (resolve, reject) { });
        }

        function login(loginType) {
            if (loginType === "facebook") {
                return facebookLogin();
            } else if (loginType === "google") {
                return googleLogin();
            }
            return $q(function (resolve, reject) {
                $rootScope.user = emptyUser();
                resolve($rootScope.user);
            });
        }


        function unsubscribe() {

        }

        function getHttpHeader() {
            var deferred = $q.defer();

            var authType = "none";
            var token = "";

            if (!$rootScope.user ||
                !$rootScope.user.authType ||
                !$rootScope.user.accessToken ||
                $rootScope.user.authType === "none" ||
                $rootScope.user.accessToken === "") {
                updateStatus();
            }

            if ($rootScope.user) {
                if ($rootScope.user.authType) {
                    authType = $rootScope.user.authType;
                }
                if ($rootScope.user.accessToken) {
                    token = $rootScope.user.accessToken;
                }
            }
            deferred.resolve(authType + ";" + token);
            return deferred.promise;
        }

        $rootScope.$on("Facebook:statusChange", function (ev, data) {
            console.log("Status: ", data);
            if (data.status === "connected") {
                $rootScope.user.accessToken = data.authResponse.accessToken;
                $rootScope.user.authType = "facebook";
            } else {
                $rootScope.user.accessToken = "";
                $rootScope.user.authType = "none";
            }
        });

        var service = {
            login: login,
            logout: logout,
            udateStatus: updateStatus,
            unsubscribe: unsubscribe,
            getHttpHeader: getHttpHeader
        };

        return service;
    }

    angular
        .module("toprakWeb")
        .factory("identityService", identityService);

    identityService.$inject = ["$rootScope", "$timeout", "Facebook", "common"];
})();