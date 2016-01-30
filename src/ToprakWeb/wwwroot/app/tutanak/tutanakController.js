(function () {
    "use strict";

    var controllerId = "tutanakController";

    function tutanakController($rootScope, $scope, services, common) {
        /* jshint validthis:true */
        var vm = this;
        var $q = common.$q;

        vm.temsilciliklerPromise = services.getTemsilcilikler().then(function(result) {
            vm.ulkeler = result.Value;
        });
        
        vm.title = "tutanakController";
        vm.tutanak = { seenBy: $rootScope.user && $rootScope.user.email ? $rootScope.user.email : null };
        vm.tutanak.seenByDisabled = false;
        vm.tutanakImageUrl = "";
        vm.denizBitti = false;
        vm.numberPattern = function () { return "/(^0$)|(^[1-9]\d{0,8}$)/"; }

        $scope.$on("login_success", function (event, args) {
            vm.tutanak.seenBy = $rootScope.user && $rootScope.user.email ? $rootScope.user.email : null;
            vm.tutanak.seenByDisabled = $rootScope.user && $rootScope.user.email ? true : false;
        });

        function checkError(field) {
            if (field.$error.required) {
                return true;
            }
            else if (field.$error.pattern) {
                return true;
            }

            return false;
        }

        function toplamHesapla() {
            var total = parseInt(vm.tutanak.akp) + parseInt(vm.tutanak.hdp) + parseInt(vm.tutanak.chp) + parseInt(vm.tutanak.sp) + parseInt(vm.tutanak.mhp) + parseInt(vm.tutanak.others);
            return isNaN(total) ? 0 : total;
        }

        function toplamHesapla9() {
            var total = parseInt(vm.tutanak.itirazolmadangecerlioy7) + parseInt(vm.tutanak.itirazuzerinegecerlioy8);
            return isNaN(total) ? 0 : total;
        }

        function toplamHesapla14() {
            var total = parseInt(vm.tutanak.gecersizzarf10) + parseInt(vm.tutanak.boszarf11) + parseInt(vm.tutanak.gercersizsayilanoy12) + parseInt(vm.tutanak.hesabakatilmayanoy13);
            return isNaN(total) ? 0 : total;
        }

        function tutanakImageUrl() {
            var deferred = $q.defer();

            var promise = services.getTutanak();
            promise.then(function (uri) {
                vm.tutanak.seenBy = $rootScope.user && $rootScope.user.email ? $rootScope.user.email : null;
                vm.tutanak.seenByDisabled = $rootScope.user && $rootScope.user.email ? true : false;
                if (!uri.Value) {
                    vm.denizBitti = true;
                } else {
                    vm.tutanakImageUrl = uri.Value;
                    vm.messageId = uri.MessageId;
                    vm.receipt = uri.Receipt;
                    vm.denizBitti = false;
                }
                deferred.resolve();
            }, function () {
                common.logger.logError("Cannot get image URI");
                deferred.reject();
            });

            return deferred.promise;
        }

        var resetForm = function () {
            var seenBy = vm.tutanak.seenBy;
            vm.tutanak = angular.copy({});
            vm.tutanak.seenBy = seenBy;
            vm.temsilcilik = null;
            vm.ulke = null;
            vm.waitPromise = tutanakImageUrl();
        };

        function update(tutanak) {
            
            tutanak.Image = vm.tutanakImageUrl;
            tutanak.Receipt = vm.receipt;
            tutanak.MessageId = vm.messageId;
            if (vm.temsilcilik) {
                tutanak.Temsilcilik = vm.temsilcilik.Id;
            }
            vm.waitPromise = services.postData(tutanak);
            vm.waitPromise.then(function () {
                console.log("post success");
                $rootScope.$broadcast("update_stats");
                resetForm();
            });
      
        }

        function formSubmit(isValid) {
            if (isValid) {
                update(vm.tutanak);
            }
        }

        var rd = 1;
        function rotateImage() {
            vm.orientation = 90 * rd;
            if (rd++ === 4) {
                rd = 0;
            }
        }

        vm.rotateImage = rotateImage;
        vm.toplamHesapla = toplamHesapla;
        vm.toplamHesapla9 = toplamHesapla9;
        vm.toplamHesapla14 = toplamHesapla14;
        vm.formSubmit = formSubmit;
        vm.checkError = checkError;

        function activate() {
            vm.waitPromise = common.activateController([tutanakImageUrl()], controllerId);
        }

        activate();
    }
    
    angular
        .module("toprakWeb")
        .controller(controllerId, tutanakController);

    tutanakController.$inject = ["$rootScope", "$scope", "services", "common"];
})();
