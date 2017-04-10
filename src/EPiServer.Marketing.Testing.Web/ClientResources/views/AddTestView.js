define([
     'dojo/_base/declare',
     'dojo/Evented',
     'dojo/aspect',
     'dijit/_WidgetBase',
     'dijit/_TemplatedMixin',
     'dojo/text!marketing-testing/views/AddTestView.html',
     'epi/i18n!marketing-testing/nls/abtesting',
     'marketing-testing/viewmodels/AddTestViewModel',
     'marketing-testing/viewmodels/KpiViewModel',
     'dijit/_WidgetsInTemplateMixin',
     'epi/shell/widget/_ModelBindingMixin',
     'epi/datetime',
     'epi/username',
     'dojo/topic',
     'dojo/html',
     'dojo/dom',
     'dojo/dom-class',
     'dojo/dom-style',
     'dojo/query',
     'dijit/registry',
     'epi/dependency',
     'marketing-testing/scripts/rasterizeHTML',
     'dojo/dom-form',
     'dojo/json',
     'dojox/layout/ContentPane',
     'marketing-testing/widgets/KpiWidget',
     'marketing-testing/widgets/KpiWeightWidget',
     'xstyle/css!marketing-testing/css/ABTesting.css',
     'dijit/form/Button',
     'dijit/form/NumberSpinner',
     'dijit/form/Textarea',
     'dijit/form/RadioButton',
     'epi/shell/widget/DateTimeSelectorDropDown',
     'dijit/form/TextBox',
     'epi-cms/widget/Breadcrumb',
     'dijit/layout/AccordionContainer',
     'dijit/layout/ContentPane',
     'dijit/form/Select'
],
    function (
    declare,
    Evented,
    aspect,
    _WidgetBase,
    _TemplatedMixin,
    template,
    resources,
    AddTestViewModel,
    KpiViewModel,
    _WidgetsInTemplateMixin,
    _ModelBindingMixing,
    datetime,
    username,
    topic,
    html,
    dom,
    domStyle,
    domClass,
    query,
    registry,
    dependency,
   rasterizehtml,
   domForm,
   JSON,
   ContentPane,
   KpiWidget,
   KpiWeightWidget
) {
        viewPublishedVersion: null;
        viewCurrentVersion: null;
        viewParticipationPercent: null;
        viewTestDuration: null;
        viewConfidenceLevel: null;
        startButtonClickCounter: 0;

        return declare([Evented, _WidgetBase, _TemplatedMixin, _WidgetsInTemplateMixin, _ModelBindingMixing],
        {
            templateString: template,

            resources: resources,

            isMultiKpiTest: true,

            kpiEntries: 0,

            kpiList: new Array(),

            kpiFormData: null,

            // DOJO WIDGET METHODS

            //set bindings to view model properties
            modelBindingMap: {
                publishedVersion: ["viewPublishedVersion"],
                currentVersion: ["viewCurrentVersion"],
                participationPercent: ["viewParticipationPercent"],
                testDuration: ["viewTestDuration"],
                confidenceLevel: ["viewConfidenceLevel"],
            },

            //sets views starting data from view model
            postMixInProperties: function () {
                var me = this;
                this.model = this.model || new AddTestViewModel({ contentData: this.contentData });
                this.kpiModel = this.kpiModel || new KpiViewModel();
                if (this.kpiModel.availableKpis) {
                    this._setKpiSelectList(this.kpiModel.availableKpis);
                };

                this.kpiModel.watch("availableKpis",
                    function (name, oldvalue, value) {
                        me._setKpiSelectList(value);
                    });
                this._contextChangedHandler = dojo.subscribe('/epi/marketing/updatestate', this, this._onContextChange);
            },

            _onContextChange: function (context, caller) {
                this.contentData = caller.contentData;
                this.kpiEntries = 0;
                this._adjustKpiSelectorCombo();
                this.reset();
            },

            //sets default values once everything is loaded
            postCreate: function () {
                var me = this;
                me.reset();
                me.inherited(arguments);
                document.addEventListener('destroyed', function () {
                    me.decrementKpiEntries();
                });
            },

            decrementKpiEntries: function () {
                this.kpiEntries--;
                if (this.kpiEntries == 0) {
                    this.isMultiKpiTest = true;
                }
                this._adjustKpiSelectorCombo();
            },

            startup: function () {
                if (this.breadcrumbWidget) {
                    this.breadcrumbWidget.set("contentLink", this.contentData.contentLink);
                    this.contentNameNode.innerText = this.contentData.name;
                    this.breadcrumbWidget._addResizeListener();
                    this.breadcrumbWidget.layout();
                }
                this._resetView();
            },

            // TEST DATA MODEL & FORM SETTERS
            _setViewPublishedVersionAttr: function (viewPublishedVersion) {
                //do dom stuff
                if (!viewPublishedVersion) {
                    return;
                }
                this.publishedBy.textContent = username.toUserFriendlyString(this.contentData.publishedBy);
                this.datePublished.textContent = datetime.toUserFriendlyString(this.contentData.lastPublished);
                this.model.testContentId = this.contentData.contentGuid;
                var pubThumb = document.getElementById("publishThumbnail");

                if (pubThumb) {
                    var isCatalogContent = this.contentData.previewUrl.toLowerCase().indexOf('catalogcontent') !== -1; // Check if the content is a product page

                    //Hack to build published versions preview link below
                    var publishContentVersion = this.model.publishedVersion.contentLink.split('_'),
                       previewUrlEnd = isCatalogContent ? publishContentVersion[1] + '_CatalogContent' + '/?epieditmode=False' : publishContentVersion[1] + '/?epieditmode=False',
                        previewUrlStart = this.contentData.previewUrl.split('_'),
                        previewUrl = previewUrlStart[0] + '_' + previewUrlEnd;

                    pubThumb.height = 768;
                    pubThumb.width = 1024;
                    rasterizehtml.drawURL(previewUrl, pubThumb, { height: 768, width: 1024 }).then(function success(renderResult) {
                        query('.versiona').addClass('hide-bg');
                    });
                }
            },

            _setViewCurrentVersionAttr: function () {
                if (!this.contentData) {
                    return;
                }
                this.savedBy.textContent = username.toUserFriendlyString(this.contentData.changedBy);
                this.dateSaved.textContent = datetime.toUserFriendlyString(this.contentData.saved);
                this.pageName.textContent = this.contentData.name + " A/B Test";

                var pubThumb = document.getElementById("draftThumbnail");

                if (pubThumb) {
                    var previewUrl = this.model.contentData.previewUrl;

                    pubThumb.height = 768;
                    pubThumb.width = 1024;
                    rasterizehtml.drawURL(previewUrl, pubThumb, { height: 768, width: 1024 }).then(function success(renderResult) {
                        query('.versionb').addClass('hide-bg');
                    });
                }
            },

            _setViewParticipationPercentAttr: function (viewParticipationPercent) {
                this.participationPercentText.set("Value", viewParticipationPercent);
            },

            _setViewTestDurationAttr: function (viewTestDuration) {
                this.durationText.set("Value", viewTestDuration);
            },

            _setViewConfidenceLevelAttr: function (viewConfidenceLevel) {
                var rbs = [
                    { val: 99, label: "99%" },
                    { val: 98, label: "98%" },
                    { val: 95, label: "95%" },
                    { val: 90, label: "90%" }
                ];
                var confidenceSelectWidget = registry.byId("confidence"), selectOption, defaultOption;
                dijit.byId('confidence').removeOption(dijit.byId('confidence').getOptions());
                if (confidenceSelectWidget) {
                    for (var i = 0; i < rbs.length; i++) {
                        if (viewConfidenceLevel) {
                            if (rbs[i].val === viewConfidenceLevel) {
                                selectOption = { value: rbs[i].val, label: rbs[i].label + " (Default)" };
                                confidenceSelectWidget.addOption(selectOption);
                                defaultOption = rbs[i].val;
                            } else {
                                selectOption = { value: rbs[i].val, label: rbs[i].label };
                                confidenceSelectWidget.addOption(selectOption);
                            }
                        }
                        confidenceSelectWidget.setValue(defaultOption);
                    }
                }
            },

            // DATA GETTERS
            _getConfidenceLevel: function () {
                var confidenceSelectWidget = dijit.byId("confidence");
                return confidenceSelectWidget.value;
            },

            // Transforms custom KPI form data into json for processing
            _getKpiFormData: function () {
                var me = this;
                var formDataArray = new Array();

                for (var x = 1; x < document.forms.length; x++) {
                    var kpiFormObject = dojo.formToObject(document.forms[x]);
                    var formData = dojo.toJson(kpiFormObject, true);
                    var formattedFormData = formData.replace(/(\r\n|\n|\r|\t)/gm, "");
                    formDataArray.push(formattedFormData);
                }
                var formDataJSon = dojo.toJson(formDataArray, true);
                return formDataJSon;
            },

            // FORM ELEMENT CONTROL METHODS

            //retrieves KPIs and displays them in a select control
            _setKpiSelectList: function (kpiList) {
                var me = this;
                var kpiuiElement = registry.byId("kpiSelector");
                if (kpiuiElement && kpiList) {
                    kpiuiElement.set("value", "");
                    dijit.byId('kpiSelector').removeOption(dijit.byId('kpiSelector').getOptions());

                    var defaultOption = null;

                    if (this.kpiEntries == 0) {
                        defaultOption = {
                            value: "default",
                            label: me.resources.addtestview.goals_selectlist_default,
                            selected: true
                        };
                    } else {
                        defaultOption = {
                            value: "default",
                            label: me.resources.addtestview.goals_selectlist_additional_kpi,
                            selected: true
                        };
                    }

                    kpiuiElement.addOption(defaultOption);
                    for (var x = 0; x < kpiList.length; x++) {
                        var option = { value: x.toString(), label: kpiList[x].kpi.friendlyName };
                        kpiuiElement.addOption(option);
                    }
                }
            },

            //DATA VALIDATION
            // Master validations for all form fields. Used when hitting the start button and will pickup
            // errors not triggered by onChangeEvents.
            _isValidFormData: function () {
                return this._isValidPercentParticipation() &
                    this._isValidDuration() &
                    this._isValidStartDate();
            },

            //Validates the participation percent entered is between 1 and 100
            _isValidPercentParticipation: function () {
                var errorTextNode = dom.byId("participationErrorText");
                var errorIconNode = dom.byId("participationErrorIcon");
                var participationPercentage = dom.byId("percentageSpinner").value;

                if (!this._isUnsignedNumeric(participationPercentage) ||
                    participationPercentage <= 0 ||
                    participationPercentage > 100) {
                    this._setError(resources.addtestview.error_participation_percentage, errorTextNode, errorIconNode);
                    return false;
                }

                this._setError("", errorTextNode, errorIconNode);
                return true;
            },

            //Validates the duration entered is not less than 1 day
            _isValidDuration: function () {
                var errorTextNode = dom.byId("durationErrorText");
                var errorIconNode = dom.byId("durationErrorIcon");
                var duration = dom.byId("durationSpinner").value;

                if (!this._isUnsignedNumeric(duration) || duration < 1 || duration > 365) {
                    this._setError(resources.addtestview.error_duration, errorTextNode, errorIconNode);
                    return false;
                }

                this._setError("", errorTextNode, errorIconNode);
                return true;
            },

            //Validates the date information is A) a valid date format and B) not in the past
            _isValidStartDate: function () {
                var errorTextNode = dom.byId("datePickerErrorText");
                var errorIconNode = dom.byId("datePickerErrorIcon");
                var scheduleText = dom.byId("ScheduleText");
                var start = this.startDatePicker.get("value");
                var now = new Date();

                if (start !== "") {
                    if (isNaN(new Date(start))) {
                        this._setError(resources.addtestview.error_invalid_date_time_value,
                            errorTextNode,
                            errorIconNode);
                        scheduleText.innerText = resources.addtestview.error_test_not_schedulded_or_started;
                        return false;
                    } else if (new Date(start).toLocaleString < now.toLocaleString) {
                        this._setError(resources.addtestview.error_date_in_the_past, errorTextNode, errorIconNode);
                        scheduleText.innerText = resources.addtestview.error_test_not_schedulded_or_started;
                        return false;
                    }
                }

                this._setError("", errorTextNode, errorIconNode);
                return true;
            },

            //Validates the supplied string as a positive integer
            _isUnsignedNumeric: function (string) {
                if (string.match(/^[0-9]+$/) === null) {
                    return false;
                }
                return true;
            },

            // FORM DATA CLEANUP
            reset: function () {
                // reset the start button click counter
                this.startButtonClickCounter = 0;
                this.kpiEntries = 0;

                //set view model properties to default form values.
                if (this.descriptionText) {
                    this.descriptionText.value = this.model.testDescription = "";
                }

                if (this.participationPercentText) {
                    this.participationPercentText.reset();
                    this.model.participationPercent = this.participationPercentText.value;
                }

                if (this.durationText) {
                    this.durationText.reset();
                    this.model.testDuration = this.durationText.value;
                }

                if (this.startDatePicker) {
                    this.startDatePicker.reset();
                    this.model.startDate = new Date(Date.now()).toUTCString();
                }

                if (this.breadcrumbWidget) {
                    this.breadcrumbWidget.set("contentLink", this.contentData.contentLink);
                    this.contentNameNode.innerText = this.contentData.name;
                    this.breadcrumbWidget.layout();
                }

                if (this.conversionPageWidget) {
                    this.conversionPageWidget.reset();
                }

                if (this.start) {
                    this.start.set("checked", true);
                }

                if (this.delay) {
                    this.delay.set("checked", false);
                }

                if (this.scheduleDiv) {
                    this.scheduleDiv.style.visibility = "hidden";
                }


                var kpiSelector = dom.byId("kpiSelectorCombo");
                if (kpiSelector) {
                    kpiSelector.style.display = "block";
                }

                this._setViewConfidenceLevelAttr();
                this._setViewPublishedVersionAttr(true);
                this._setViewCurrentVersionAttr();
                this._clearConversionErrors();
                this._clearCustomKpiMarkup();
                this._clearKpiWeightWidgets();
                this._resetView();
            },

            //forces both add test view containers to return to the top
            _resetView: function () {
                var abTestBody = dom.byId("abTestBody");
                var toolbarGroup = dom.byId("toolbarGroup");
                if (abTestBody) {
                    abTestBody.scrollIntoView(true);
                    toolbarGroup.scrollIntoView(true);
                }
            },

            //Clears the KPI Error text and icon
            _clearConversionErrors: function () {
                var errorTextElements = query("span[id*='kpiErrorText']");
                var errorIconElements = query("span[id*='kpiErrorIcon']");

                if (errorTextElements) {
                    errorTextElements.forEach(function (element) {
                        element.innerText = "";
                        element.style.visibility = "hidden";
                    });
                }

                if (errorIconElements) {
                    errorIconElements.forEach(function (element) {
                        element.style.visibility = "hidden";
                    })
                }
            },

            //Removes the custom KPI markup from the view & widget registry
            _clearCustomKpiMarkup: function () {
                this.kpiEntries = 0;
                this.isMultiKpiTest = true;
                var kpiuiElement = dom.byId("kpiWidgets");
                if (kpiuiElement) {
                    var contentPane = dojo.query('#kpiWidgets > *');
                    if (contentPane[0]) {
                        dojo.forEach(dijit.findWidgets(contentPane)), function (w) {
                            w.destroyRecursive();
                        };
                        var dijitContentPane = dijit.byId(contentPane[0].id);
                        dijitContentPane.destroy();
                        kpiuiElement.innerHTML = "";
                    }
                }
            },

            _clearKpiWeightWidgets: function () {
                var kpiWeightWidgetElement = dom.byId("kpiWeightSelectors");
                if (kpiWeightWidgetElement) {
                    var weightSelectors = dojo.query('#kpiWeightSelectors');
                    if (weightSelectors[0]) {
                        dojo.forEach(dijit.findWidgets(weightSelectors)), function (w) {
                            w.destroyRecursive();
                        };
                        kpiWeightWidgetElement.innerHTML = "";
                    }
                }
            },

            // UI UTILITIES
            _toggleTimeSelector: function () {
                var dateSelector = dom.byId("dateSelector");

                if (dateSelector.style.visibility === "hidden") {
                    dateSelector.style.visibility = "visible";
                } else {
                    this.startDatePicker.reset();
                    dateSelector.style.visibility = "hidden";
                }
            },

            //Toggles an errors text content and icon visibitlity based on the error and nodes supplied
            _setError: function (errorText, errorNode, iconNode) {
                if (errorText) {
                    errorNode.innerText = errorText;
                    errorNode.style.visibility = "visible";
                    iconNode.style.visibility = "visible";
                } else {
                    errorNode.innerText = "";
                    errorNode.style.visibility = "hidden";
                    iconNode.style.visibility = "hidden";
                }
            },

            //EVENT HANDLERS
            //Start and Cancel Events
            _onStartButtonClick: function () {
                this._clearConversionErrors();
                if (this.startButtonClickCounter > 0) { return false; } // Use click counter to prevent double-click
                this.startButtonClickCounter++; // Increment click count
                var me = this;

                this.kpiErrorTextNode = dom.byId("kpiErrorText");
                this.kpiErrorIconNode = dom.byId("kpiErrorIcon");

                this.model.testDescription = dom.byId("testDescription").value;
                var startDateSelector = dom.byId("StartDateTimeSelector");
                var utcNow = new Date(Date.now()).toUTCString();
                if (startDateSelector.value === "") {
                    this.model.startDate = utcNow;
                }

                this.model.confidencelevel = this._getConfidenceLevel();
                this.model.testTitle = me.pageName.textContent;

                this.kpiFormData = this._getKpiFormData();

                this.kpiModel.createKpi(this);
            },

            createTest: function (kpiIds) {
                this._clearConversionErrors();
                var jsonKpis = dojo.toJson(kpiIds, true);
                this.model.kpiId = jsonKpis.replace(/(\r\n|\n|\r|\t)/gm, "");
                if (this._isValidFormData()) {
                    this.model.createTest();
                    this._clearConversionErrors();
                    this._setKpiSelectList(this.kpiModel.availableKpis);
                } else {
                    this.startButtonClickCounter = 0;
                }
            },

            setKpiError: function (ret) {
                var errors = this._isObject(ret);
                this.kpiFormData = null;
                if (errors) {
                    var keys = Object.keys(errors);
                    for (var x = 0; x < keys.length; x++) {
                        var errorIconNode = dom.byId(keys[x] + "_kpiErrorIcon");
                        var errorTextNode = dom.byId(keys[x] + "_kpiErrorText");
                        errorText = errors[keys[x]];
                        this._setError(errorText, errorTextNode, errorIconNode);
                        this.startButtonClickCounter = 0;
                    };
                }
                else {
                    this._setError(ret, this.kpiErrorTextNode, this.kpiErrorIconNode);
                    this.startButtonClickCounter = 0;
                }
            },

            _isObject: function (jsonString) {
                try {
                    var obj = JSON.parse(jsonString);
                    if (obj && typeof obj === "object") {
                        return obj;
                    }
                } catch (e) { }
                return false;
            },

            _onCancelButtonClick: function () {
                var me = this;
                this.kpiEntries = 0;
                this._clearCustomKpiMarkup();
                this._clearKpiWeightWidgets();
                this._adjustKpiSelectorCombo();
                me.contextParameters = {
                    uri: "epi.cms.contentdata:///" + this.model.currentVersion.contentLink
                };
                topic.publish("/epi/shell/context/request", me.contextParameters);
            },

            _onGoalSelectChange: function (evt) {
                var me = this;
                var kpiuiElement = dom.byId("kpiui");
                var kpiWidget = dom.byId("kpiWidgets");
                var kpiSelector = dom.byId("kpiSelectorCombo");
                var kpiWeightWidget = dom.byId("kpiWeightSelectors");

                if (evt !== "default") {
                    var kpiObject = this.kpiModel.getKpiByIndex(evt);

                    var kpiWidgetInstance = new KpiWidget({
                        label: kpiObject.kpi.friendlyName,
                        markup: kpiObject.kpi.uiMarkup,
                        description: kpiObject.kpi.description,
                        kpiType: kpiObject.kpiType
                    });

                    kpiWidgetInstance.placeAt(kpiWidget);
                    aspect.after(kpiWidgetInstance,
                        'destroy',
                        function() {
                            me.decrementKpiEntries();
                        });

                    var weightWidget = new KpiWeightWidget({
                        label: kpiObject.kpi.friendlyName,
                        kpiWidgetId: kpiWidgetInstance.id,
                        value: "Medium"
                    }).placeAt(kpiWeightWidget);
                    kpiWidgetInstance._setlinkedWidgetIdAttr(weightWidget.id);

                    if (kpiObject.kpi.kpiResultType != "KpiConversionResult") {
                        this.isMultiKpiTest = false;
                    }
                    this.kpiEntries++;

                    this._adjustKpiSelectorCombo(kpiWidgetInstance.id);
                }
            },

            _isScrolledIntoView: function (el) {
                var elemTop = el.getBoundingClientRect().top;
                var elemBottom = el.getBoundingClientRect().bottom;

                var isVisible = (elemTop >= 0) && (elemBottom <= window.innerHeight);
                return isVisible;
            },

            // Form Field Events
            _onTestDescriptionChanged: function (event) {
                this.model.testDescription = event;
            },

            _onConversionPageChanged: function (event) {
                if (this._isValidConversionPage()) {
                    this.model.conversionPage = event;
                }
            },

            _onPercentageSpinnerChanged: function (event) {
                if (this._isValidPercentParticipation()) {
                    this.model.participationPercent = event;
                }
            },

            _onDurationSpinnerChanged: function (event) {
                if (this._isValidDuration()) {
                    this.model.testDuration = event;
                }
            },

            _onDateTimeChange: function (event) {
                var startButton = registry.byId("StartButton");
                var scheduleText = dom.byId("ScheduleText");
                var startDateSelector = dom.byId("StartDateTimeSelector");

                if (event === null) {
                    event = startDateSelector.value;
                }

                if (this._isValidStartDate(event)) {
                    if (event !== "") {
                        var dojoLocale = dojo.locale;
                        var localDate = new Date(event).toLocaleString(dojoLocale);
                        startButton.set("label", resources.addtestview.schedule_test);
                        scheduleText.innerText = resources.addtestview.schedule_tobegin_on + localDate;
                        this.model.startDate = new Date(event).toUTCString();
                        this.model.start = false;
                    } else {
                        startButton.set("label", resources.addtestview.start_default);
                        scheduleText.innerText = resources.addtestview.notscheduled_text;
                        this.model.start = true;
                    }
                }
            },

            _adjustKpiSelectorCombo: function (kpiId) {
                var dijitSelector = dijit.byId("kpiSelector");
                dijitSelector.set("value", "default");
                var kpiSelector = dom.byId("kpiSelectorCombo");

                if (kpiId) {
                    var kpiWidget = dom.byId(kpiId);
                    if (!this._isScrolledIntoView(kpiWidget)) {
                        kpiWidget.scrollIntoView(true);
                    }
                }
                if (this.kpiEntries == this.model.kpiLimit || this.isMultiKpiTest != true) {
                    kpiSelector.style.display = "none";
                } else {
                    kpiSelector.style.display = "block";

                    if (this.kpiEntries == 0) {
                        this._setKpiSelectList(this.kpiModel.refreshKpis());
                    }
                    if (this.kpiEntries == 1 && this.isMultiKpiTest == true) {
                        var allowableKpis = new Array();
                        this.kpiModel.availableKpis.forEach(function (entry) {
                            if (entry.kpi.kpiResultType == "KpiConversionResult") {
                                allowableKpis.push(entry);
                            }
                        });
                        this.kpiModel.availableKpis = allowableKpis;
                        this._setKpiSelectList(this.kpiModel.availableKpis);
                    }
                }
            },

            _showAdvancedOptions: function (evt) {
                var advancedOptionsElement = dom.byId("advancedOptions");
                if (dojo.style(advancedOptionsElement, "display") == "none") {
                    dojo.style(advancedOptionsElement, "display", "block");
                    advancedOptionsElement.scrollIntoView(true);
                }
                else {
                    dojo.style(advancedOptionsElement, "display", "none");
                    if (!evt.srcElement.id) {
                        this.advancedOptions.reset();
                    }
                }
            }
        });
    });