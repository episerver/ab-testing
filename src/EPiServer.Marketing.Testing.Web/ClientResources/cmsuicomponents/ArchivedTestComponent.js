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

        postMixInProperties: function () {
            // summary:
            //		Called after constructor parameters have been mixed-in; sets default values for parameters that have not been initialized.
            // tags:
            //		protected

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
            // summary:
            //		Construct the UI for this widget with this.domNode initialized as a dgrid.
            // tags:
            //		protected

            this.inherited(arguments);

            var customGridClass = declare([OnDemandGrid, Selection]);
            this.grid = new customGridClass({
                columns: {
                    savedDate: {
                        label: resources.archivedtestcomponent.header.archiveddate,
                        formatter: this._localizeDate
                    },
                    savedBy: {
                        label: resources.archivedtestcomponent.header.by,
                        formatter: this._createUserFriendlyUsername
                    },
//                    status: {
//                        label: resources.archivedtestcomponent.header.status,
//                        formatter: this._createUserFriendlyUsername
//                    },
                    winner: {
                        label: resources.archivedtestcomponent.header.winner,
                        formatter: this._createUserFriendlyUsername
                    },
                },
                selectionMode: "single",
                selectionEvents: "click,dgrid-cellfocusin",
                sort: [{ attribute: "savedDate", descending: true }]
            }, this.domNode);
        },

        postCreate: function () {
            this.inherited(arguments);

            this._deleteVersionCommand = new DeleteVersion();

            this.own(
                // Refresh after delete successfully
                aspect.after(this._deleteVersionCommand, "execute", lang.hitch(this, function (deferred) {
                    deferred.then(lang.hitch(this, this._onDeleteVersionSuccess), lang.hitch(this, this._onDeleteVersionFailure));
                }))
            );

            this.add("commands", withConfirmation(this._deleteVersionCommand, null, {
                title: resources.archivedtestcomponent.deleteconfirm.title,
                heading: resources.archivedtestcomponent.deleteconfirm.note,
                description: resources.archivedtestcomponent.deleteconfirm.description
            }));
        },

        startup: function () {
            // summary: Overridden to connect a store to a DataGrid.

            if (this._started) {
                return;
            }

            this.inherited(arguments);

            this.own(aspect.around(this.grid, "renderRow", lang.hitch(this, this._aroundRenderRow)));

            this.fetchData();
        },

        contextChanged: function (context, callerData) {
            // summary:
            //		When context change remove all data from list when its not content data.
            // context: Object
            // callerData: Object
            // tags:
            //      protected

            this.inherited(arguments);
            !this._isContentContext(context) && this.grid.set("store", null);
        },

        fetchData: function () {
            // summary:
            //		Updates the grid with the new data.
            // tags:
            //		private
            var tests;
            var contentLink;
            this.getCurrentContent()
                .then(function(currentContent) {
                    contentLink = new ContentReference(currentContent.contentLink);
                });

            this._abTestStore.get(contentLink).then(function (archived) { console.log(archived); tests = archived; });
            when(this.getCurrentContent(), lang.hitch(this, function (item) {
                if (!item) {
                    return;
                }

                this._setQuery(item.contentLink, true);
            }));
        },

        _aroundRenderRow: function (original) {
            // summary:
            //		Called 'around' the renderRow method in order to add a class which indicates published state.
            // tags:
            //		private

            return lang.hitch(this, function (item) {
                // If item is a common draft add it to the hash map.
                if (item.isCommonDraft) {
                    var common = this._commonDrafts[item.language];
                    if (common && common !== item.contentLink) {
                        this._removeCommonDraft(common);
                    }
                    this._commonDrafts[item.language] = item.contentLink;
                }

                this._versionsByLanguage[item.language] = this._versionsByLanguage[item.language] || [];
                if (array.every(this._versionsByLanguage[item.language], function (ver) {
                    return ver !== item.contentLink;
                })) {
                    this._versionsByLanguage[item.language].push(item.contentLink);
                }

                // Call original method
                var row = original.apply(this.grid, arguments);

                // Add state specific classes
                domClass.toggle(row, "dgrid-row-published", item.status === ContentActionSupport.versionStatus.Published);

                return row;
            });
        },

        _onError: function (e) {
            // summary:
            //		Shows an error message to the user when failing to load data.
            // tags:
            //		protected, extension

            if (e.error && e.error.status === 403) {
                when(this.getCurrentContent(), lang.hitch(this, function (item) {
                    this._showErrorMessage(epi.resources.messages.nopermissiontoviewdata);
                    this._updateMenu(item);
                }));
            } else {
                this.inherited(arguments);
            }
        },

        _onSelect: function (e) {
            var item = e.rows[0].data;

            this._updateMenu(item);

            this.inherited(arguments);
        },

        _updateMenu: function (item) {
            this._deleteVersionCommand.set("model", item);

            when(this._abTestStore.get(item.contentLink), lang.hitch(this, function (content) {
                var x = 1;
            }));
            

//            when(this._contentStore.get(item.contentLink), lang.hitch(this, function (content) {
//            }));
        },

        _showNotificationMessage: function (notification) {
            // summary:
            //     Show a notification message
            // notification:
            //     The notification message
            var dialog = new Alert({
                title: resources.notificationtitle,
                description: notification
            });
            dialog.show();
        },

        _selectCommonDraftVersion: function (uri, forceReload) {
            // summary:
            //      Selects the common draft version in the list.
            // tags:
            //      private
            var callerData = {
                sender: this
            };

            if (forceReload) {
                callerData.forceReload = forceReload;
            }

            topic.publish("/epi/shell/context/request", { uri: uri }, callerData);
        },

        _onDeleteVersionSuccess: function () {
            // summary:
            //      Update the current context and display notification.
            // tags:
            //      private

            var self = this;
            return when(this.getCurrentContent(), function (currentContent) {
                var contentLink = new ContentReference(currentContent.contentLink);
                var referenceId = contentLink.createVersionUnspecificReference().toString();

                self._selectCommonDraftVersion("epi.cms.contentdata:///" + referenceId, true);
            });
        },

        _onDeleteVersionFailure: function (response) {
            // summary:
            //      Display alert with the delete failure message.
            // tags:
            //      private

            //If the user clicks cancel in the confirmation dialog we will end up here
            if (!response || !response.status) {
                return;
            } else if (response.status === 403) {
                this._showNotificationMessage(resources.deleteversion.cannotdeletepublished);
            } else {
                this._showNotificationMessage(resources.deleteversion.cannotdelete);
            }
        },

        _removeCommonDraft: function (reference) {
            // Refresh the previous common draft item, as it might be changed on the server side.
            if (reference) {
                var item = this.grid.row(reference).data;
                if (item) {
                    item.isCommonDraft = false;
                }
            }
        },

        _setQuery: function (contentLink ) {
            var query = {
                contentLink: new ContentReference(contentLink).createVersionUnspecificReference().toString()
            };

            this.grid.set("query", query);

            // The server side store requires the contentLink to be set.
            if (!this.grid.store) {
                this.grid.set("store", this.store);
            }

            this._versionsByLanguage = {};
        }
    });
});
