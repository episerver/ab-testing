define([
    "dojo/_base/declare",
    "dijit/_WidgetBase",
    "dijit/_TemplatedMixin",
    "dojo/text!marketing-testing/views/MarketingTestDetailsView.html",
    "dijit/_WidgetsInTemplateMixin",
    "epi/datetime",
    'xstyle/css!marketing-testing/css/style.css',
    'xstyle/css!marketing-testing/css/GridForm.css',
    "dijit/form/DropDownButton",
    "dijit/TooltipDialog",
    "dijit/form/Button"
    
], function (
    declare,
    widgetBase,
    templatedMixin,
    template,
    widgetsInTemplateMixin,
    datetime

) {
    return declare([widgetBase, templatedMixin, widgetsInTemplateMixin], {
        templateString: template,

        constructor: function() {
            var contextService = epi.dependency.resolve("epi.shell.ContextService"), me=this;
            me.context = contextService.currentContext;
        },

        postCreate:function() {
            this.DetailsHeader.textContent = this.context.data.test.title;

            if (this.context.data.daysElapsed.indexOf("Test") !== -1) {
                this.StartedText.textContent = this.context.data.daysElapsed;
                this.timeRemaining.textContent = this.context.data.daysRemaining;
            } else {
                this.StartedText.textContent = datetime.toUserFriendlyString(this.context.data.test.StartDate);
                this.Owner.textContent = this.context.data.test.Owner;
                this.timeRemaining.textContent = this.context.data.daysRemaining + "days";
            }
            
            this.VersionAText.textContent = this.context.data.publishedVersionName;
            this.VersionAContentLink.textContent = this.context.data.publishedVersionContentLink;

            this.VersionBText.textContent = this.context.data.draftVersionName;
            this.VersionBContentLink.textContent = this.context.data.draftVersionContentLink;
            this.DraftUser.textContent = this.context.data.draftVersionChangedBy;
            this.DraftSavedDate.textContent = datetime.toUserFriendlyString(this.context.data.draftVersionChangedDate);
            this.visitorPercentage.textContent = this.context.data.visitorPercentage;
            this.totalParticipants.textContent = this.context.data.totalParticipantCount;
            this.TestDescription.textContent = this.context.data.test.Description;
        }
    });
});