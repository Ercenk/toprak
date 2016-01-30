(function () {
    "use strict";

    function uploadController($rootScope, $http, FileUploader, services, common) {
        /* jshint validthis:true */
        var vm = this;
        var logger = common.logger;
        vm.isAdmin = 

        FileUploader.prototype._xhrTransport = function (item) {

            var that = this;
            this._onBeforeUploadItem(item);

            if(typeof(item._file.size) != "number") {
                throw new TypeError("The file specified is no longer valid");
            }

            services.getUploadUrl(item.file.name + "/").then(function (url) {

                var xhr = new XMLHttpRequest();

                xhr.upload.onprogress = function (event) {
                    var progress = Math.round(event.lengthComputable ? event.loaded * 100 / event.total : 0);
                    that._onProgressItem(item, progress);
                };

                xhr.withCredentials = item.withCredentials;

                xhr.onload = function() {
                    var headers = that._parseHeaders(xhr.getAllResponseHeaders());
                    var response = that._transformResponse(xhr.response, headers);
                    var gist = that._isSuccessCode(xhr.status) ? 'Success' : 'Error';
                    var method = '_on' + gist + 'Item';
                    that[method](item, response, xhr.status, headers);
                    that._onCompleteItem(item, response, xhr.status, headers);
                };

                xhr.onerror = function() {
                    var headers = that._parseHeaders(xhr.getAllResponseHeaders());
                    var response = that._transformResponse(xhr.response, headers);
                    that._onErrorItem(item, response, xhr.status, headers);
                    that._onCompleteItem(item, response, xhr.status, headers);
                };

                xhr.onabort = function() {
                    var headers = that._parseHeaders(xhr.getAllResponseHeaders());
                    var response = that._transformResponse(xhr.response, headers);
                    that._onCancelItem(item, response, xhr.status, headers);
                    that._onCompleteItem(item, response, xhr.status, headers);
                };
 
                try {

                    xhr.open("PUT", url.Value, true); 
                    xhr.setRequestHeader("Content-Type", item._file.type);
                    xhr.setRequestHeader("x-ms-blob-type", "BlockBlob");
                    xhr.setRequestHeader("x-ms-blob-content-type", item._file.type);
                    xhr.send(item._file);
                } 
                catch (e) { 
                    alert("can't upload the image to server.\n" + e.toString()); 
                }

                that._render();
            }, function (error) {
                logger.logError("cannot get SAS url for file " + item.file.name);
            });
        }

        var uploader = vm.uploader = new FileUploader({ url: "api/v1/Admin/UploadDone", removeAfterUpload: false});

        uploader.filters.push({
            name: "imageFilter",
            fn: function (item /*{File|FileLikeObject}*/, options) {
                var type = "|" + item.type.slice(item.type.lastIndexOf("/") + 1) + "|";
                return "|jpg|png|jpeg|bmp|gif|".indexOf(type) !== -1;
            }
        });

        uploader.onAfterAddingAll = function (addedFileItems) {
            services.setupUpload().then(function(success) {
                console.info(("CORS setup"));
            }, function (error) {
                console.info(("CORS setup error " + error));
            });
        };

        uploader.onCompleteAll = function () {
            services.transferFromStaging();
        };

        uploader.onBeforeUploadItem = function (item) {
            console.info(item);
        };

        uploader.onSuccessItem = function (fileItem, response, status, headers) {
            //fileItem.remove();
        };
        uploader.onErrorItem = function (fileItem, response, status, headers) {
            console.info("onErrorItem", fileItem, response, status, headers);
        };

        uploader.onCompleteItem = function (fileItem, response, status, headers) {
            console.info("onCompleteItem", fileItem, response, status, headers);
        };

        vm.canUpload = function() {
            return vm.uploader.getNotUploadedItems().length > 0 && $rootScope.user.isAdmin;
        };
        vm.title = "uploadController";

        function activate() { }

        activate();
    }

    angular
        .module("toprakWeb")
        .controller("uploadController", uploadController);

    uploadController.$inject = ["$rootScope", "$http", "FileUploader", "services", "common"];
})();
