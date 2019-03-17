define([
    "epi/dependency",
    'marketing-testing/scripts/html2canvas',
    'marketing-testing/scripts/es6-promise.auto.min'
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
            
            var iframeToLoadPagePreview = document.createElement('iframe');
            iframeToLoadPagePreview.style.cssText = 'position: absolute; opacity:0; z-index: -9999';
            iframeToLoadPagePreview.width = 1024;
            iframeToLoadPagePreview.height = 768;  
            iframeToLoadPagePreview.src = url;

            var renderingOptions = {
                canvas: canvasForPreviewImage
            };

            iframeToLoadPagePreview.onload = function (e) {
                var elementToRender = iframeToLoadPagePreview.contentDocument.documentElement;
                html2canvas(elementToRender, renderingOptions).then(function (canvas) {
                    canvasForPreviewImage.style.width = "100%";
                    canvasForPreviewImage.style.height = "100%";
                    me._setPreviewState(canvasForPreviewImage, "none", "block", "none");
                }).catch(function (error) {
                    me._setPreviewState(canvasForPreviewImage, "none", "none", "block");
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