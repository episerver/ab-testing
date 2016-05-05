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
    'dijit/form/SimpleTextarea',
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
            // Widget will update itself using the new context.
            this.model = new AddTestViewModel({ contentData: caller.contentData });

            this.contentData = caller.contentData;
            _setViewCurrentVersionAttr();
        },

        //sets default values once everything is loaded
        postCreate: function () {
            //set view model properties to default form values.
            this.model.testDescription = this.descriptionText.value;
            this.model.participationPercent = this.participationPercentText.value;
            this.model.testDuration = this.durationText.value;
            this.model.testTitle = "Default Test Title";
            var _startDate = Date();
            this.model.startDate = new Date(_startDate).toUTCString();
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
            if (this.breadcrumbWidget) {
                this.breadcrumbWidget.set("contentLink", this.contentData.contentLink);    
            }
        },

        //EVENT HANDLERS

        //Start and Cancel Events

        _onStartButtonClick: function () {
            this.model.createTest();
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
            var _startDate;

            if (event !== null) {
                startButton.innerText = "Test Scheduled";
                scheduleText.innerText = "is scheduled to begin " + event;
                this.model.startDate = new Date(event).toUTCString();
            } else {
                startButton.innerText = "Start Test";
                scheduleText.innerText = "not scheduled, and will start right away";
                _startDate = Date();
                this.model.startDate = new Date(_startDate).toUTCString();
            }
        }
    });
});