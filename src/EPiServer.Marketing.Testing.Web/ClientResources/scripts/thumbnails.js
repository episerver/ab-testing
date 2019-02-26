define([
    "epi/dependency",
    'marketing-testing/scripts/rasterizeHTML'
],
    function (dependency, rasterizehtml) {
        return {
            _setThumbnail: function (canvasId, url) {
                var me = this;
                this._setThumbState(canvasId, "block", "none", "none");

                if (this._isMicrosoftBrowser()) {
                    this._renderServersideThumbnail(canvasId, url);
                } else {
                    this._renderClientsideThumbnail(canvasId, url);
                }
            },

            _isMicrosoftBrowser: function () {
                return navigator.appName === 'Microsoft Internet Explorer'
                    || !!(navigator.userAgent.match(/Trident/)
                        || navigator.userAgent.match(/rv:11/)
                        || navigator.userAgent.match(/Edge/))
                    || (typeof $.browser !== "undefined" && $.browser.msie === 1);
            },

            _renderServersideThumbnail: function (canvasId, url) {
                var me = this;
                this.thumbstore = this.thumbstore || dependency.resolve("epi.storeregistry").get("marketing.thumbnailstore");
                this.thumbstore.get(url.replace(/\//g, "$")).then(function (result) {
                    canvasId.height = 768;
                    canvasId.width = 1024;
                    var thumbnail = new Image();
                    thumbnail.src = "data:image/png;base64," + result;
                    thumbnail.onload = function () {
                        var context = canvasId.getContext('2d');
                        context.drawImage(thumbnail, 0, 0);
                        me._setThumbState(canvasId, "none", "block", "none");
                    };
                }).otherwise(function () {
                    me._setThumbState(canvasId, "none", "none", "block");
                });
            },

            _renderClientsideThumbnail: function (canvasId, url) {
                var me = this;
                canvasId.height = 768;
                canvasId.width = 1024;
                rasterizehtml.drawURL(url, canvasId, { height: 768, width: 1024 }).then(function success(renderResult) {
                    me._setThumbState(canvasId, "none", "block", "none");
                });
            },

            _setThumbState: function (canvasId, spinnerState, previewState, errorState) {
                document.getElementById(canvasId.id + "-spinner").style.display = spinnerState;
                canvasId.style.display = previewState;
                document.getElementById(canvasId.id + "-error").style.display = errorState;
            }
        };
    });