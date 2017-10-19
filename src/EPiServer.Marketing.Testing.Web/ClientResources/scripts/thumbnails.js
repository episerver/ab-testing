define([
    "epi/dependency",
],
function (dependency) {
    return {
        _setThumbnail: function (canvasId, url) {
            var me = this;
            this._setThumbState(canvasId, "block", "none", "none");
            this.thumbstore = this.thumbstore || dependency.resolve("epi.storeregistry").get("marketing.thumbnailstore");
            this.thumbstore.get(url.replace(/\//g, "$")).then(function (result) {
                console.log("called: " + url);
                var thumbnail = new Image();
                canvasId.height = 768;
                canvasId.width = 1024;
                thumbnail.src = '../episerver.marketing.testing/ABCapture/' + result;
                thumbnail.onload = function () {
                    var context = canvasId.getContext('2d');
                    context.drawImage(thumbnail, 0, 0);
                    me.thumbstore.remove(result);
                    me._setThumbState(canvasId, "none", "block", "none");

                }
            }).otherwise(function () {
                me._setThumbState(canvasId, "none", "none", "block");
            });
        },

        _setThumbState(canvasId, spinnerState, previewState, errorState) {
            document.getElementById(canvasId.id + "-spinner").style.display = spinnerState;
            canvasId.style.display = previewState;
            document.getElementById(canvasId.id + "-error").style.display = errorState;
        }
    }
});