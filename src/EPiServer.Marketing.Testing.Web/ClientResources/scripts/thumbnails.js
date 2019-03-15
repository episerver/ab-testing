define([
    "epi/dependency",
    'marketing-testing/scripts/html2canvas'
],
    function (dependency, rasterizehtml) {
        return {
            _setThumbnail: function (canvasId, url) {
                var me = this;
                this._setThumbState(canvasId, "block", "none", "none");
                this._renderClientsideThumbnail(canvasId, url);                
            },
            
            _renderClientsideThumbnail: function (canvasForThumbnail, url) {
                var me = this;
                canvasForThumbnail.height = 768;
                canvasForThumbnail.width = 1024;

                var iframeToLoadPreview = document.createElement('iframe');
                iframeToLoadPreview.src = url;
                iframeToLoadPreview.width = 1024;
                iframeToLoadPreview.height = 768;                
                iframeToLoadPreview.style.cssText = 'position: absolute; opacity:0; z-index: -9999';

                iframeToLoadPreview.onload = function (e) {
                    html2canvas(iframeToLoadPreview.contentDocument.documentElement, { canvas: canvasForThumbnail }).then(function (canvas) {
                        document.body.removeChild(iframeToLoadPreview);
                    });
                }

                document.body.appendChild(iframeToLoadPreview);
            },

            _setThumbState: function (canvasId, spinnerState, previewState, errorState) {
                document.getElementById(canvasId.id + "-spinner").style.display = spinnerState;
                canvasId.style.display = previewState;
                document.getElementById(canvasId.id + "-error").style.display = errorState;
            }
        };
    });