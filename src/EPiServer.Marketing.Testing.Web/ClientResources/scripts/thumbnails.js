define([
    "epi/dependency",
    'marketing-testing/scripts/html2canvas-1.0.0-alpha.11.min', // Page rendering tool
    'marketing-testing/scripts/es6-promise-4.2.6.auto.min'      // Polyfill 'Promise' for older browsers
],
function (dependency, html2canvas) {
    return {
        _setThumbnail: function (canvasForPreviewImage, url) {
            var me = this;
            this._setPreviewState(canvasForPreviewImage, "block", "none", "none");
            this._renderPreview(canvasForPreviewImage, url);                
        },
            
        _renderPreview: function (canvasForPreviewImage, url) {
            var me = this,
                previewHeight = 768,
                previewWidth = 1024;

            // Create a hidden iframe with the target aspect ratio.

            var iframeToLoadPagePreview = document.createElement('iframe');
            iframeToLoadPagePreview.style.cssText = 'position: absolute; opacity:0; z-index: -9999';
            iframeToLoadPagePreview.width = previewWidth;
            iframeToLoadPagePreview.height = previewHeight;
            iframeToLoadPagePreview.src = url;

            // Render the preview to the canvas that was specified
            // as a parameter to this function.

            var renderingOptions = {
                canvas: canvasForPreviewImage,
                height: previewHeight,
                width: previewWidth,
                windowHeight: previewHeight,
                windowWidth: previewWidth,
                allowTaint: true,
                useCORS: true
            };
            
            // The content of the iframe is the page that we're attempting to preview. 
            // Render it to the canvas after it loads.

            iframeToLoadPagePreview.onload = function (e) {
                var elementToRender = iframeToLoadPagePreview.contentDocument.documentElement;
                html2canvas(elementToRender, renderingOptions).then(function (canvas) {
                    canvasForPreviewImage.style.width = "100%";     // The rendering tool applies it's own aspect ratio to the canvas,
                    canvasForPreviewImage.style.height = "100%";    // override that to ensure that it fits properly within our UI.
                    me._setPreviewState(canvasForPreviewImage, "none", "block", "none");
                }).catch(function (error) {
                    me._setPreviewState(canvasForPreviewImage, "none", "none", "block");
                }).finally(function () {
                    document.body.removeChild(iframeToLoadPagePreview); // Remove the hidden iframe from the DOM.
                });
            }

            // Append the hidden iframe to the DOM so that the preview loads.

            document.body.appendChild(iframeToLoadPagePreview);
        },

        _setPreviewState: function (canvasForPreviewImage, spinnerDisplayState, previewDisplayState, errorDisplayState) {
            document.getElementById(canvasForPreviewImage.id + "-spinner").style.display = spinnerDisplayState;
            canvasForPreviewImage.style.display = previewDisplayState;
            document.getElementById(canvasForPreviewImage.id + "-error").style.display = errorDisplayState;
        }
    };
});