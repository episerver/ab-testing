///<reference path="../jquery-1.3.2-vsdoc.js" />
(function ($, epi) {
    if (!epi.cmo) {
        epi.cmo = {};
    }

    epi.cmo.lpoReportGadget = function () {

        var temp = {};

        var updateView = function (gadgetInstance, html) {
            var updatedView = $('<div>' + html + '</div>');
            var gadgetIsNotConfigured = $('input[name=InvalidConfiguration]', gadgetInstance.element).val();
            var gadgetIsNotConfiguredUpdated = updatedView.find('input[name=InvalidConfiguration]').val();

            if (gadgetIsNotConfigured && gadgetIsNotConfiguredUpdated) {
                gadgetIsNotConfigured = gadgetIsNotConfigured.toLowerCase();
                gadgetIsNotConfiguredUpdated = gadgetIsNotConfiguredUpdated.toLowerCase();
            }
            else {
                gadgetIsNotConfigured = null;
                gadgetIsNotConfiguredUpdated = null;
            }

            var testID = $('input[name=TestID]', gadgetInstance.element).val();
            var testIDUpdated = updatedView.find('input[name=TestID]').val();
            
            if (gadgetIsNotConfigured != gadgetIsNotConfiguredUpdated || testID != testIDUpdated) {
                gadgetInstance.loadView(null);
                return;
            }

            // Validation summary update
            var errors = updatedView.find('.epi-gadgetError', gadgetInstance.element);
            if (errors.length > 0) {
                var existingErrors = $(".epi-gadgetError", gadgetInstance.element);
                if (existingErrors.length > 0) {
                    existingErrors.html(errors.html());
                }
                else {
                    errors.insertBefore($('.epi-gadgetContent', gadgetInstance.element).children(":first"));
                }
            }
            else {
                $('.epi-gadgetError', gadgetInstance.element).remove();
            }


            // Header update
            updateBlock('.epi-gadget-header', gadgetInstance.element, updatedView);

            // Toolbar update 
            updateBlock('.epi-gadget-toolbar-container', gadgetInstance.element, updatedView);

            // Warning area update
            var newWarnings = updatedView.find('.EP-systemMessage');
            if (newWarnings.length > 0) {
                var existingWarnings = $('.EP-systemMessage', gadgetInstance.element);
                if (existingWarnings.length > 0) {
                    existingWarnings.html(newWarnings.html());
                }
                else {
                    newWarnings.insertAfter($('.epi-gadget-toolbar', gadgetInstance.element));
                }
            }
            else {
                $('.EP-systemMessage', gadgetInstance.element).remove();
            }

            // Main view update
            $('.epi-LPOGadget-item', gadgetInstance.element).each(function (index, elem) {
                updateBlock('.epi-LPOGadget-item-name', elem, updatedView, index);
                updateBlock('.epi-LPOGadget-item-thumb', elem, updatedView, index);
                updateBlock('.epi-LPOGadget-item-stats', elem, updatedView, index);
                updateBlock('.epi-LPOGadget-barBlock', elem, updatedView, index);
                updateGauge(elem, updatedView, index);
                updateStyles(elem, '.epi-LPOGadget-item', updatedView, index);
            });
            // Compact view update
            $('.epi-LPOGadget-itemCompact', gadgetInstance.element).each(function (index, elem) {
                updateBlock('.epi-LPOGadget-item-nameCompact', elem, updatedView, index);
                updateBlock('.epi-LPOGadget-item-mainValue', elem, updatedView, index);
                updateGauge(elem, updatedView, index);
                updateStyles(elem, '.epi-LPOGadget-itemCompact', updatedView, index);
            });

            initActionButtons(gadgetInstance);
        };

        var updateBlock = function (selector, context, updated, index) {
            var newContent;
            if (index) {
                newContent = updated.find(selector).eq(index).html();
            }
            else {
                newContent = updated.find(selector).html();
            }
            var oldContent = $(selector, context).html();
            if (newContent != oldContent) {
                $(selector, context).html(newContent);
            }
        };

        var updateGauge = function (context, updated, index) {
            var data = eval('(' + updated.find('input[name=GaugeData]').eq(index).val() + ')');
            var slGauge = $('object[name=gaugeObject]', context)[0];
            if (slGauge != undefined && slGauge.Content != undefined) {
                slGauge.Content.LpoGauge.UpdateGauge(data.IsOriginal, data.ConversionRate, data.ConversionRateRange,
                            data.OriginalPageConversionRate, data.OriginalPageConversionRateRange, data.ComparePageResult, data.ConversionRateWithRangeString, false);
            }

        };

        var updateStyles = function (element, selector, updated, index) {
            var newClasses, newStyles;
            if (index) {
                newStyles = updated.find(selector).eq(index).attr("style");
                newClasses = updated.find(selector).eq(index).attr("class");
            }
            else {
                newStyles = updated.find(selector).attr("style");
                newClasses = updated.find(selector).attr("class");
            }
            var oldStyles = $(element).attr("style");
            var oldClasses = $(element).attr("class");
            if (newStyles != oldStyles) {
                $(element).attr("style", newStyles);
            }
            if (newClasses != oldClasses) {
                $(element).attr("class", newClasses);
            }
        };

        var callAction = function (gadgetInstance, routeParams) {
            if (routeParams) {
                gadgetInstance.routeParams = routeParams;
            }
            else {
                gadgetInstance.routeParams = gadgetInstance.defaultRouteParams;
            }
            var url = gadgetInstance.getActionPath(gadgetInstance.routeParams);
            gadgetInstance.ajax({
                url: url,
                type: 'POST',
                dataType: "HTML",
                success: function (data) {
                    updateView(gadgetInstance, data);
                }
            });
        };

        var updateData = function (gadgetInstance) {
            var options = {
                url: gadgetInstance.getActionPath({ action: "Index" }),
                type: 'GET',
                dataType: 'html',
                success: function (data) {
                    updateView(gadgetInstance, data);
                }
            };
            gadgetInstance.ajax(options);
        };

        var runUpdateData = function (gadgetInstance) {
            if (!gadgetInstance.scheduleID) {
                gadgetInstance.scheduleID = 0;
            }
            var updateTimeout = 30000;
            if (gadgetInstance.scheduleID != 0) {
                updateData(gadgetInstance);
            }
            gadgetInstance.scheduleID = setTimeout(function () { runUpdateData(gadgetInstance); }, updateTimeout);
        };

        var clearUpdateSchedule = function (gadgetInstance) {
            if (gadgetInstance.scheduleID) {
                clearTimeout(gadgetInstance.scheduleID);
            }
            gadgetInstance.scheduleID = 0;
        };

        var initActionButtons = function (gadgetInstance) {
            $("input[name=startButton]", gadgetInstance.element).unbind("click").filter(":enabled")
                .click(function () { callAction(gadgetInstance, { action: "Start" }); });
            $("input[name=stopButton]", gadgetInstance.element).unbind("click").filter(":enabled")
                .click(function () { callAction(gadgetInstance, { action: "Stop" }); });
            $("input[name=finalizeButton]", gadgetInstance.element).unbind("click").filter(":enabled")
                .click(function () { $("button[name=setAsWinnerButton]", gadgetInstance.element).toggle(); });
            $("button[name=setAsWinnerButton]", gadgetInstance.element).toggle(false).unbind("click").filter(":enabled")
                .click(function () {
                    var pageId = $(this).next().val();
                    callAction(gadgetInstance, { action: "Finalize", winnerTestPageID: pageId });
                });
        };

        var updateStyleForMobile = function (gadgetInstance) {
            var iPhonePanel = $("div.iPhone", gadgetInstance.element);
            if (iPhonePanel && iPhonePanel.length > 0) {
                $("div.epi-gadgetFeedback").addClass("epi-gadgetFeedback-iPhone");
            }
        };

        var initTestSelector = function (gadgetInstance) {
            // Settings view
            var testSelectControl = $("select[name=LpoTestID]", gadgetInstance.element);
            if (testSelectControl.length > 0) {
                if (testSelectControl.val() == null) {
                    testSelectControl.val($("option:first", testSelectControl).val());
                    testSelectControl.change();
                }
            }
        };

        var checkAutoupdateRequired = function (gadgetInstance) {
            var invalidConfigurationIndicator = $('input[name=InvalidConfiguration]', gadgetInstance.element);
            return invalidConfigurationIndicator && invalidConfigurationIndicator.length > 0;
        };

        var onloaded = function (e, gadgetInstance) {
            initTestSelector(gadgetInstance);
            initActionButtons(gadgetInstance);
            clearUpdateSchedule(gadgetInstance);
            if (checkAutoupdateRequired(gadgetInstance)) {
                runUpdateData(gadgetInstance);
            }
            updateStyleForMobile(gadgetInstance);
        };

        var onunload = function (e, gadgetInstance) {
            clearUpdateSchedule(gadgetInstance);
        };

        temp.init = function (e, gadgetInstance) {
            $(this).bind("epigadgetloaded", onloaded);
            $(this).bind("epigadgetunload", onunload);
        };

        return temp;
    } ();
} (epiJQuery, epi));
