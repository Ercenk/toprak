(function () {
    "use strict";

    var controllerId = "loginController";

    function loginController($rootScope, $scope, $timeout, facebook, common, identityService) {
        var vm = this;
        var logger = common.logger;

        $rootScope.$on("login_success", function (event, args) {
            vm.isAuthenticated = true;
            $scope.$apply();
        });

        function facebookLogin() {
            identityService.login("facebook").then(function(result) {
                vm.isAuthenticated = $rootScope.user.isAuthenticated;
            }, function(error) {});
        }

        function googleLogin() {
            identityService.login("google").then(function (result) {
                vm.isAuthenticated = $rootScope.user.isAuthenticated;
            }, function (error) { });
        }

        function activate() {
            common.activateController([], controllerId);
        }

        function logout() {
            identityService.logout().then(function(result) {}, function(error) {});
        }

        activate();

        vm.facebookLogin = facebookLogin;
        vm.googleLogin = googleLogin;
        vm.logout = logout;
        vm.unsubscribe = identityService.unsubscribe;
        vm.isAuthenticated = false;
    }

    angular.module("toprakWeb").controller(controllerId, loginController);
    loginController.$inject = ["$rootScope", "$scope", "$timeout", "Facebook", "common", "identityService"];
})();