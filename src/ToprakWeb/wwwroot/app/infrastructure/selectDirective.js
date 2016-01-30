(function() {
    "use strict";

    function selectDirective () {

        function link(scope, element, attrs, ngModel) {
            if (!ngModel) {
                return;
            }
            element.bind("keyup", function() {
                element.triggerHandler("change");
            });
        }

        var directive = {
            link: link,
            restrict: "E",
            require: "?ngModel",
            scope: false
        };
        return directive;
    }

    angular
        .module("common")
        .directive("select", selectDirective);

    selectDirective.$inject = [];
})();