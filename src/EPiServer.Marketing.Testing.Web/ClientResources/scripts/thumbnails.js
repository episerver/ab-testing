define([
    "epi/dependency",
],
function (dependency) {
    return {
        _setThumbnail: function (canvasId, url) {
            var me = this;
            this.thumbstore = this.thumbstore || dependency.resolve("epi.storeregistry").get("marketing.thumbnailstore");
            this.thumbstore.get(url.replace(/\//g, "$")).then(function (result) {
                var thumbnail = new Image();
                canvasId.height = 768;
                canvasId.width = 1024;
                thumbnail.src = '../episerver.marketing.testing/ABCapture/' + result;
                thumbnail.onload = function () {
                    var context = canvasId.getContext('2d');
                    context.drawImage(thumbnail, 0, 0);
                    me.thumbstore.remove(result);
                    document.getElementById(canvasId.id + "-spinner").style.display = "none";
                    document.getElementById(canvasId.id + "-error").style.display = "none";
                    canvasId.style.display = "block";
                }
            }).otherwise(function () {
                document.getElementById(canvasId.id + "-spinner").style.display = "none";
                document.getElementById(canvasId.id + "-error").style.display = "block";
                canvasId.style.display = "none";
            });
        },
    }
});