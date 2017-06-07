define([
// Dojo
    "dojo/_base/array",
    "dojo/_base/declare",
    "dojo/_base/lang",
    "dojo/aspect",
    "dojo/dom-class",
    "dojo/string",
    "dojo/topic",
    "dojo/when",
    "dojox/html/entities",

// DGrid
    "dgrid/OnDemandGrid",
    "dgrid/Selection",
    "epi-cms/dgrid/formatters",

// EPi Framework
    "epi",
    "epi/dependency",
    "epi/shell/TypeDescriptorManager",
    "epi/shell/command/_WidgetCommandProviderMixin",
    "epi/shell/command/withConfirmation",
    "epi/shell/widget/_FocusableMixin",
    "epi/shell/widget/dialog/Alert",

// EPi CMS
    "epi-cms/_MultilingualMixin",
    "epi-cms/ApplicationSettings",
    "epi-cms/core/ContentReference",
    "epi-cms/component/command/DeleteVersion",
    "epi-cms/contentediting/ContentActionSupport",
    "epi-cms/widget/_GridWidgetBase",

// Resources
    "epi/i18n!marketing-testing/nls/abtesting/"

], function (
// Dojo
    array,
    declare,
    lang,
    aspect,
    domClass,
    string,
    topic,
    when,
    entities,

// DGrid
    OnDemandGrid,
    Selection,
    formatters,

// EPi Framework
    epi,
    dependency,
    TypeDescriptorManager,
    _WidgetCommandProviderMixin,
    withConfirmation,
    _FocusableMixin,
    Alert,

// EPi CMS
    _MultilingualMixin,
    ApplicationSettings,
    ContentReference,
    DeleteVersion,
    ContentActionSupport,
    _GridWidgetBase,

// Resources
    resources
) {

    return declare([_GridWidgetBase, _WidgetCommandProviderMixin, _FocusableMixin, _MultilingualMixin], {
        // summary:
        //      This component will list all versions of a content item.
        //
        // tags:
        //      internal

        dataGrid: null,
        gridData: [],

        postScript: function () {
            this._abTestStore = this._abTestStore ||
                dependency.resolve("epi.storeregistry").get("marketing.abarchives");
        },

        postMixInProperties: function () {

            this._commonDrafts = {};

            this.storeKeyName = "epi.cms.contentversion";
            this.ignoreVersionWhenComparingLinks = false;
            this.forceContextReload = true;

            this.inherited(arguments);

            this._currentContentLanguage = ApplicationSettings.currentContentLanguage;

            var registry = dependency.resolve("epi.storeregistry");
            this._contentStore = registry.get("epi.cms.contentdata");
            this._abTestStore = this._abTestStore ||
                dependency.resolve("epi.storeregistry").get("marketing.abarchives");
        },

        buildRendering: function () {

            this.inherited(arguments);

            var customGridClass = declare([OnDemandGrid, Selection]);

            dataGrid = new customGridClass({
                columns: [
                     {
                         field: "endDate",
                         label: resources.archivedtestcomponent.header.archiveddate,
                         formatter: this._localizeDate
                     },
                    {
                        field: "owner",
                        label: resources.archivedtestcomponent.header.by,
                        formatter: this._createUserFriendlyUsername
                    },
                    {
                        field: "id",
                        label: resources.archivedtestcomponent.header.winner,
                        get: function (object) { return object.title; }
                    }
                ]
            }, this.domNode);
        },

        startup: function () {
            var data = [];
            var me = this;
            var tests;
            var contentLink;

            dataGrid.on('.dgrid-content .dgrid-cell:click', function (event) {
                var cell = dataGrid.cell(event);
                me.viewTest(cell.row.data.id);
            });
            this.getCurrentContent()
                .then(function (currentContent) {
                    me.setGridData(currentContent.contentLink);
                });

            dataGrid.styleColumn("testId", "display:none;");
        },

        contextChanged: function (context, callerData) {
            var me = this;
            var tests;
            var contentLink;
            var x = context;
            if (context.type === "epi.cms.contentdata") {
                this.getCurrentContent()
                    .then(function (currentContent) {
                        me.setGridData(currentContent.contentLink);
                    });
            }
        },

        viewTest(testId) {
            topic.publish("/epi/shell/context/request", {
                uri: "epi.marketing.testing:///testid=" + testId + "/archive"
            });
        },

        setGridData: function (contentLink) {
            var data = [];
            var me = this;
            this._abTestStore.get(contentLink).then(function (tests) {
                for (var x = 0; x < tests.length; x++) {
                    var dataPoint = tests[x];
                    data.push(dataPoint);
                }

                dataGrid.refresh();
                dataGrid.renderArray(data);
            });
        },
    });
});

