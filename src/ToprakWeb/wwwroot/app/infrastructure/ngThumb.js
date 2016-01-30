(function() {
    "use strict";

    function ngThumb ($window) {
        var helper = {
            support: !!($window.FileReader && $window.CanvasRenderingContext2D),
            isFile: function (item) {
                return angular.isObject(item) && item instanceof $window.File;
            },
            isImage: function (file) {
                var type = '|' + file.type.slice(file.type.lastIndexOf('/') + 1) + '|';
                return '|jpg|png|jpeg|bmp|gif|'.indexOf(type) !== -1;
            }
        };

        function link(scope, element, attributes) {
            var canvas = element.find('canvas');
            var reader = new FileReader();

            function onLoadImage() {
                var width = params.width || this.width / this.height * params.height;
                var height = params.height || this.height / this.width * params.width;
                canvas.attr({ width: width, height: height });
                canvas[0].getContext('2d').drawImage(this, 0, 0, width, height);
            }

            function onLoadFile(event) {
                var img = new Image();
                img.onload = onLoadImage;
                img.src = event.target.result;
            }

            if (!helper.support) return;

            var params = scope.$eval(attributes.ngThumb);

            if (!helper.isFile(params.file)) return;
            if (!helper.isImage(params.file)) return;

            reader.onload = onLoadFile;
            reader.readAsDataURL(params.file);
        }

        var directive = {
            link: link,
            restrict: "A",
            template: "<canvas/>"
        };
        return directive;
    }

    angular
        .module("common")
        .directive("ngThumb", ngThumb);

    ngThumb.$inject = ["$window"];
})();