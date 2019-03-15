define([
    "epi/dependency",
    'marketing-testing/scripts/html2canvas'
],
function (dependency, html2canvas) {
    return {
        _setThumbnail: function (canvasForPreviewImage, url) {
            var me = this;
            this._setPreviewState(canvasForPreviewImage, "block", "none", "none");
            this._renderPreview(canvasForPreviewImage, url);                
        },
            
        _renderPreview: function (canvasForPreviewImage, url) {
            var me = this;
            canvasForPreviewImage.height = 768;
            canvasForPreviewImage.width = 1024;

            var iframeToLoadPagePreview = document.createElement('iframe');
            iframeToLoadPagePreview.src = url;
            iframeToLoadPagePreview.width = 1024;
            iframeToLoadPagePreview.height = 768;                
            iframeToLoadPagePreview.style.cssText = 'position: absolute; opacity:0; z-index: -9999';

            iframeToLoadPagePreview.onload = function (e) {
                html2canvas(iframeToLoadPagePreview.contentDocument.documentElement, { canvas: canvasForPreviewImage }).then(function (canvas) {
                    me._setPreviewState(canvas, "none", "block", "none");
                }).catch(function (error) {
                    me._setPreviewState(canvas, "none", "none", "block");
                }).finally(function () {
                    document.body.removeChild(iframeToLoadPagePreview);
                });
            }

            document.body.appendChild(iframeToLoadPagePreview);
        },

        _setPreviewState: function (canvasForPreviewImage, spinnerDisplayState, previewDisplayState, errorDisplayState) {
            document.getElementById(canvasForPreviewImage.id + "-spinner").style.display = spinnerDisplayState;
            canvasForPreviewImage.style.display = previewDisplayState;
            document.getElementById(canvasForPreviewImage.id + "-error").style.display = errorDisplayState;
        }
    };
});