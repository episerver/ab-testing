define([
    "dojo/_base/declare",
    "dojo/topic",
    "dgrid/OnDemandGrid",
    "dgrid/Selection",
    "epi-cms/dgrid/formatters",
    "epi",
    "epi/dependency",
    "epi-cms/widget/_GridWidgetBase",
    "epi/i18n!marketing-testing/nls/abtesting/"

], function (
    declare,
    topic,
    OnDemandGrid,
    Selection,
    formatters,
    epi,
    dependency,
    _GridWidgetBase,
    resources
) {
    return declare([_GridWidgetBase], {

        dataGrid: null,
        gridData: [],

        buildRendering: function () {
            this.inherited(arguments);

            //Setup the grid node used to display archived test information
            var customGridClass = declare([OnDemandGrid, Selection]);

            dataGrid = new customGridClass({
                columns: [
                     {
                         field: "endDate",
                         label: resources.archivedtestcomponent.header.archiveddate,
                         formatter: this._localizeDate
                     },
                     {
                         field: "startDate",
                         label: resources.archivedtestcomponent.header.startdate,
                         formatter: this._localizeDate
                     },
                     {
                         field: "owner",
                         label: resources.archivedtestcomponent.header.owner,
                         formatter: this._createUserFriendlyUsername
                     }
                ]
            }, this.domNode);
        },

        startup: function () {
            var me = this;

            //Define the abteststore
            this._abTestStore = this._abTestStore ||
                           dependency.resolve("epi.storeregistry").get("marketing.abarchives");

            //Connect click event handler to show archive view when a test is selected
            dataGrid.on('.dgrid-content .dgrid-cell:click', function (event) {
                var cell = dataGrid.cell(event);
                me._viewArchivedTest(cell.row.data.id);
            });

            //Set initial data for the grid based on current content
            this.getCurrentContent()
                .then(function (currentContent) {
                    me._setGridData(currentContent.contentLink);
                });
        },

        contextChanged: function (context) {
            var me = this;
            //Update grid data when context is changed
            if (context.type === "epi.cms.contentdata") {
                this.getCurrentContent()
                    .then(function (currentContent) {
                        me._setGridData(currentContent.contentLink);
                    });
            }
        },

        _viewArchivedTest(testId) {
            //Shows the archive view for the selected test
            topic.publish("/epi/shell/context/request", {
                uri: "epi.marketing.testing:///testid=" + testId + "/archive"
            });
        },

        _setGridData: function (contentLink) {
            var me = this;
            var data = [];
            //Get archived tests for the current content and populate the data to be used by the grid
            this._abTestStore.get(contentLink).then(function (tests) {
                for (var x = 0; x < tests.length; x++) {
                    data.push(tests[x]);
                }
                //reset the grid data
                dataGrid.refresh();
                //render the new grid data
                dataGrid.renderArray(data.sort(function (a, b) {
                    return new Date(b.endDate) - new Date(a.endDate);
                }));
            });
        },
    });
});

