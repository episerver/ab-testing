define([
     'dojo/_base/declare',
    'dijit/_WidgetBase',
    'dijit/_TemplatedMixin',
    'dojo/text!marketing-testing/views/AddTestView.html',
    'dojo/i18n!marketing-testing/nls/MarketingTestingLabels',
    'marketing-testing/viewmodels/AddTestViewModel',
    'dijit/_WidgetsInTemplateMixin',
    'epi/shell/widget/_ModelBindingMixin',
    'epi/datetime',
    'epi/username',
    'dojo/topic',
    'dojo/html',
    'dojo/dom',
    'xstyle/css!marketing-testing/css/style.css',
    'xstyle/css!marketing-testing/css/GridForm.css',
    'xstyle/css!marketing-testing/css/dijit.css',
    'dijit/form/Button',
    'dijit/form/NumberSpinner',
    'dijit/form/Textarea',
    'epi-cms/widget/Breadcrumb',    
    'epi-cms/widget/ContentSelector',
    'epi/shell/widget/DateTimeSelectorDropDown',
    'dijit/form/TextBox',
    'epi-cms/widget/Breadcrumb',
    'epi/dependency',

], function (
    declare,
    _WidgetBase,
    _TemplatedMixin,
    template,
    resources,
    AddTestViewModel,
    _WidgetsInTemplateMixin,
    _ModelBindingMixing,
    datetime,
    username,
    topic,
    html,
    dom,
    dependency


) {
    viewPublishedVersion: null;
    viewCurrentVersion: null;

    return declare([_WidgetBase, _TemplatedMixin, _WidgetsInTemplateMixin, _ModelBindingMixing], {
        templateString: template,

        resources: resources,

        //set bindings to view model properties
        modelBindingMap: {
            publishedVersion: ["viewPublishedVersion"],
            currentVersion: ["viewCurrentVersion"],
        },

        //sets views starting data from view model
        postMixInProperties: function () {

            this.model = this.model || new AddTestViewModel({ contentData: this.contentData });
            this._contextChangedHandler = dojo.subscribe('/epi/marketing/updatestate', this, this._onContextChange);
        },

        _onContextChange: function (context, caller) {
            this.contentData = caller.contentData;
            this.reset();
        },

        //sets default values once everything is loaded
        postCreate: function () {
            this.reset();
        },

        reset: function () {
            //set view model properties to default form values.
            if (this.titleText) {
                this.titleText.reset();
                this.model.testTitle = this.titleText.value;
            }

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
            }
            if (this.conversionPageWidget) {
                this.conversionPageWidget.reset();
            }

            this._setViewPublishedVersionAttr(true);
            this._setViewCurrentVersionAttr();

            this._clearErrors();
        },

        //setters for bound properties
        _setViewPublishedVersionAttr: function (viewPublishedVersion) {
            //do dom stuff
            if (!viewPublishedVersion) {
                return;
            }
            this.publishedVersionReference.textContent = viewPublishedVersion.name + "[" + viewPublishedVersion.contentLink + "]";
            this.publishedBy.textContent = username.toUserFriendlyString(this.contentData.publishedBy);
            this.datePublished.textContent = datetime.toUserFriendlyString(this.contentData.lastPublished);
            this.model.testContentId = this.contentData.contentGuid;
        },

        _setViewCurrentVersionAttr: function () {
            if (!this.contentData) {
                return;
            }
            this.currentVersionReference.textContent = this.contentData.name + "[" + this.contentData.contentLink + "]";
            this.savedBy.textContent = username.toUserFriendlyString(this.contentData.changedBy);
            this.dateSaved.textContent = datetime.toUserFriendlyString(this.contentData.saved);
            this.pageName.textContent = this.contentData.name + " A/B Test";
        },

        _clearErrors: function () {
            var errorText = dom.byId("pickerErrorText");
            if (!errorText) {
                return;
            }
            errorText.innerText = "";
            errorText.style.visibility = "hidden";
            var et2 = dom.byId("pickerErrorIcon");
            et2.style.visibility = "hidden";

            errorText = dom.byId("titleErrorText");
            errorText.innerText = "";
            errorText.style.visibility = "hidden";
            et2 = dom.byId("titleErrorIcon");
            et2.style.visibility = "hidden";
        },

        _setPickerError: function( error ) {
            var errorText = dom.byId("pickerErrorText");
            if (!errorText) {
                return;
            }
            errorText.innerText = error;
            errorText.style.visibility = "visible";
            var et2 = dom.byId("pickerErrorIcon");
            et2.style.visibility = "visible";
        },

        _setTitleError: function (error) {
            var errorText = dom.byId("titleErrorText");
            if (!errorText) {
                return;
            }
            errorText.innerText = error;
            errorText.style.visibility = "visible";
            var et2 = dom.byId("titleErrorIcon");
            et2.style.visibility = "visible";
        },

        _getConfidenceLevel: function () {
            var rbs = ["confidence_99", "confidence_98", "confidence_95", "confidence_90"];
            for (i = 0; i < rbs.length; i++) {
                var rb = dom.byId(rbs[i]);
                if (!rb) {
                    return;
                } else if (rb.checked) {
                    this.model.confidencelevel = rb.value;
                    return;
                }
            }
        },

        //EVENT HANDLERS

        //Start and Cancel Events

        _onStartButtonClick: function () {
            var description = dom.byId("testDescription");
            this.model.testDescription = description.value;

            this._clearErrors();
            this._getConfidenceLevel();

            var title = dom.byId("textTitle");
            if( !this.titleText.value ) {
                // TODO: use localized resources.
                this._setTitleError("You must enter a title before you can save the A/B test.");
            }
            else if (!this.model.conversionPage) {
                this._setPickerError("You must select a conversion goal page before you can save the A/B test.");
            } else {
                this._contentVersionStore = this._contentVersionStore || epi.dependency.resolve("epi.storeregistry").get("epi.cms.contentversion");
                this._contentVersionStore
                    .query({ contentLink: this.model.conversionPage, language: this.languageContext ? this.languageContext.language : "", query: "getpublishedversion" })
                    .then(function (result) {
                        var errorText = dom.byId("errorText");
                        var publishedVersion = result;
                        if (result) {
                            if (result.contentLink != this.model.publishedVersion.contentLink) {
                                this.model.createTest();
                            } else {
                                this._setPickerError("You cannot select the page you are testing as the conversion goal page. Select another page.");
                            }
                        } else {
                            this._setPickerError("You cannot select an unpublished page as your conversion goal page. Select a published page.");
                        }
                    }.bind(this))
                    .otherwise(function (result) {
                        console.log("Query failed, we cannot tell if this page is a valid page or not.");
                    });
            }
        },

        _onCancelButtonClick: function () {
            topic.publish("/epi/shell/action/changeview/back");
        },

        // Form Field Events

        _onTestTitleChanged: function (event) {
            this.model.testTitle = event;
        },

        _onTestDescriptionChanged: function (event) {
            this.model.testDescription = event;
        },

        _onConversionPageChanged: function (event) {
            this.model.conversionPage = event;
            this._clearErrors();
        },

        _onPercentageSpinnerChanged: function (event) {
            this.model.participationPercent = event;
        },

        _onDurationSpinnerChanged: function (event) {
            this.model.testDuration = event;
        },

        _onDateTimeChange: function (event) {
            var startButton = dom.byId("StartButton");
            var scheduleText = dom.byId("ScheduleText");

            if (event !== null) {
                startButton.innerText = "Schedule Test";
                scheduleText.innerText = "scheduled to begin on " + event;
                this.model.startDate = new Date(event).toUTCString();
                this.model.start = false;
            } else {
                startButton.innerText = "Start Test";
                scheduleText.innerText = "not scheduled, and will start right away";
                this.model.startDate = new Date(Date.now()).toUTCString();
                this.model.start = true;
            }
        }
    });
});