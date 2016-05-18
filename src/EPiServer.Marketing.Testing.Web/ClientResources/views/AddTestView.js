define([
     'dojo/_base/declare',
    'dijit/_WidgetBase',
    'dijit/_TemplatedMixin',
    'dojo/text!marketing-testing/views/AddTestView.html',
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
    'epi-cms/widget/Breadcrumb'

], function (
    declare,
    _WidgetBase,
    _TemplatedMixin,
    template,
    AddTestViewModel,
    _WidgetsInTemplateMixin,
    _ModelBindingMixing,
    datetime,
    username,
    topic,
    html,
    dom

) {
    viewPublishedVersion: null;
    viewCurrentVersion: null;

    return declare([_WidgetBase, _TemplatedMixin, _WidgetsInTemplateMixin, _ModelBindingMixing], {
        templateString: template,

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

            if (this.startTimeSelector) {
                this.startTimeSelector.reset();
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

        //EVENT HANDLERS

        //Start and Cancel Events

        _onStartButtonClick: function () {
            var description = dom.byId("testDescription"); //Description is not a dojo widget so setting it on save rather than onchange.
            this.model.testDescription = description.value;
            this.model.createTest();
        },

        _onCancelButtonClick: function () {
            topic.publish("/epi/shell/action/changeview/back");
        },

        // Form Field Events

        _onTestTitleChanged: function (event) {
            this.model.testTitle = event;
        },
      
        _onConversionPageChanged: function (event) {
            this.model.conversionPage = event;
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
            } else {
                startButton.innerText = "Start Test";
                scheduleText.innerText = "not scheduled, and will start right away";
                this.model.startDate = new Date(Date.now()).toUTCString();
            }
        }
    });
});