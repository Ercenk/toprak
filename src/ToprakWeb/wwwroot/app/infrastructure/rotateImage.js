(function() {
    "use strict";

    function rotateImage () {
        function linkFunction(scope, element, attrs) {
            scope.$watch(attrs.orientation, function (rotateDegrees) {
                if (!rotateDegrees) {
                    rotateDegrees = 0;
                }
                console.log(rotateDegrees);
                var r = "rotate(" + rotateDegrees + "deg)";
                element.css({
                    '-moz-transform': r,
                    '-webkit-transform': r,
                    '-o-transform': r,
                    '-ms-transform': r
                });
            });
        }
        var directive = {
            link: linkFunction,
            restrict: "A"
        };
        return directive;
    }

    angular
        .module("common")
        .directive("rotateImage", rotateImage);
})();