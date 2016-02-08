using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using EPiServer.Core;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.UI.WebControls.ContentDataSource;
using EPiServer.Web;
using EPiServer.Web.WebControls;

namespace EPiServer.Marketing.Testing.Web.Models
{
    [PersistChildren(false)]
    [ParseChildren(ChildrenAsProperties = true)]
    public class PageDataSource : Control,
    IDataSource, IDataSourceMethods, IHierarchicalContentDataSource, IContentSource, IHierarchicalDataSource
    {
        private const string SESSION_KEY = "_SessionId";
        private const string DEFAULT_VIEW = "DefaultView";
        private GenericDataSourceView<ContentDataSource> _view;
        private PartialList<int> _contentIds;
        private IContentSource _contentSource;
        private bool _useFallbackLanguage = true;
        private IContentRepository _dataFactory;
        internal Injected<LanguageSelectorFactory> LanguageSelectorFactory { get; set; }

        #region Accessors

        /// <summary>
        /// Gets or sets the required access to the content items.
        /// </summary>
        public AccessLevel AccessLevel
        {
            get
            {
                //if (SelectValues["AccessLevel"] != null)
                //{
                //    return (AccessLevel)DataSourceHelper.ConvertToEnum(SelectValues["AccessLevel"], typeof(AccessLevel));
                //}
                return this.ViewState["AccessLevel"] != null ? (AccessLevel)this.ViewState["AccessLevel"] : AccessLevel.Read;
            }
            set
            {
                this.ViewState["AccessLevel"] = value;
                OnDataSourceChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the root item for a hierarchical view or the parent item for the listed items in a tabular view.
        /// </summary>
        public ContentReference ContentLink
        {
            get
            {
                //if (SelectValues["ContentLink"] != null)
                //{
                //    return DataSourceHelper.ConvertToContentReference(SelectValues["ContentLink"]);
                //}
                return this.ViewState["ContentLink"] != null ? (ContentReference)this.ViewState["ContentLink"] : ContentReference.EmptyReference;
            }
            set
            {
                this.ViewState["ContentLink"] = value;
                OnDataSourceChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// The property that contains the root item to read data from if different from current
        /// </summary>
        [System.ComponentModel.DefaultValue(null)]
        public string ContentLinkProperty
        {
            get
            {
                //if (SelectValues["ContentLinkProperty"] != null)
                //{
                //    return (string)SelectValues["ContentLinkProperty"];
                //}
                return (string)this.ViewState["ContentLinkProperty"];
            }
            set
            {
                this.ViewState["ContentLinkProperty"] = value;
                OnDataSourceChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Return the IContentSource implementation that this property control uses to read content data. 
        /// </summary>
        /// <value>An IContentSource implementation.</value>
        /// <remarks>
        /// The returned instance will usually be the base class for the aspx-page. 
        /// </remarks>
        [System.ComponentModel.Browsable(false)]
        public IContentSource ContentSource
        {
            get
            {
                if (_contentSource == null)
                    SetupContentSource();
                return _contentSource;
            }
            set
            {
                _contentSource = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to include the content assets in a hierarchical view.
        /// </summary>
        public bool IncludeContentAssets
        {
            get
            {
                //if (SelectValues["IncludeContentAssets"] != null)
                //{
                //    return DataSourceHelper.ConvertToBool(SelectValues["IncludeContentAssets"]);
                //}
                //return this.ViewState["IncludeContentAssets"] != null ? (bool)this.ViewState["IncludeContentAssets"] : false;
                return false;
            }
            set
            {
                this.ViewState["IncludeContentAssets"] = value;
                OnDataSourceChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets whether to include the root item in a hierarchical view.
        /// </summary>
        public bool IncludeRootItem
        {
            get
            {
                //if (SelectValues["IncludeRootItem"] != null)
                //{
                //    return DataSourceHelper.ConvertToBool(SelectValues["IncludeRootItem"]);
                //}
                //return this.ViewState["IncludeRootItem"] != null ? (bool)this.ViewState["IncludeRootItem"] : true;
                return false;
            }
            set
            {
                this.ViewState["IncludeRootItem"] = value;
                OnDataSourceChanged(EventArgs.Empty);
            }
        }


        public HierarchicalDataSourceView GetHierarchicalView(string viewPath)
        {
            return new ContentHierarchicalView(this, viewPath);
        }

        /// <summary>
        /// Gets or sets whether the hierarchical view should preevaluate the existens of child item or always return true to let the HierarchicalDataBoundControl evaluate it as needed.
        /// </summary>
        public bool EvaluateHasChildren
        {
            get
            {
                //if (SelectValues["EvaluateHasChildren"] != null)
                //{
                //    return DataSourceHelper.ConvertToBool(SelectValues["EvaluateHasChildren"]);
                //}
                return this.ViewState["EvaluateHasChildren"] != null ? (bool)this.ViewState["EvaluateHasChildren"] : true;
            }
            set
            {
                this.ViewState["EvaluateHasChildren"] = value;
                OnDataSourceChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets the list representing the IDs of the partially loaded child items
        /// </summary>
        protected internal PartialList<int> Items
        {
            get
            {
                if (_contentIds == null)
                {
                    _contentIds = (PartialList<int>)CacheManager.Get(SessionId.ToString());
                    if (_contentIds == null)
                    {
                        CacheManager.Insert(SessionId.ToString(), new PartialList<int>(
                            new PartialList<int>.LoadCallback(GetChildrenCallback),
                            new PartialList<int>.FilterCallback(FilterContentCallback)));
                        _contentIds = (PartialList<int>)CacheManager.Get(SessionId.ToString());
                    }
                }
                return _contentIds;
            }
        }

        /// <summary>
        /// Gets the session id used as key for caching the list of partially loaded child items
        /// </summary>
        protected Guid SessionId
        {
            get
            {
                if (this.ViewState[SESSION_KEY] == null)
                {
                    this.ViewState[SESSION_KEY] = Guid.NewGuid();
                }
                return (Guid)this.ViewState[SESSION_KEY];
            }
        }

        /// <summary>
        /// Gets the view for tabular controls.
        /// </summary>
        protected virtual GenericDataSourceView<ContentDataSource> View
        {
            get
            {
                if (_view == null)
                {
                    _view = new GenericDataSourceView<ContentDataSource>(this, DEFAULT_VIEW, false, true, false, true, true);
                }
                return _view;
            }
        }

        /// <summary>
        /// Gets a dictionary containing entries defined by SelectParameters
        /// </summary>
        //protected IDictionary SelectValues
        //{
        //    get
        //    {
        //        return SelectParameters.GetValues(HttpContext.Current, this);
        //    }
        //}

        /// <summary>
        /// Gets or sets wether fallback to master language should be used when fetching items.
        /// </summary>
        /// <value><c>true</c> if fallback language should be used; otherwise, <c>false</c>.</value>
        public bool UseFallbackLanguage
        {
            get { return _useFallbackLanguage; }
            set { _useFallbackLanguage = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="PropertyResolver"/> that should be used by the current control instance.
        /// </summary>
        [System.ComponentModel.Browsable(false)]
        protected Injected<PropertyResolver> PropertyResolver { get; set; }

        #endregion

        #region ParameterCollection Properties

        /// <summary>
        /// Gets the parameters collection that contains the parameters that are used when selecting items.
        /// </summary>
        //[PersistenceMode(PersistenceMode.InnerProperty)]
        //[System.ComponentModel.Category("Data")]
        //[System.ComponentModel.DefaultValue((string)null)]
        //[System.ComponentModel.MergableProperty(false)]
        //[System.ComponentModel.Editor("System.Web.UI.Design.WebControls.ParameterCollectionEditor, System.Design", "System.Drawing.Design.UITypeEditor, System.Drawing")]
        //public ParameterCollection SelectParameters
        //{
        //    get
        //    {
        //        return ((GenericDataSourceView<ContentDataSource>)View).SelectParameters;
        //    }
        //}
        #endregion


        /// <summary>
        /// Default constructor
        /// </summary>
        /// 
        public PageDataSource()
        {
            //SelectParameters.ParametersChanged += delegate { OnDataSourceChanged(EventArgs.Empty); };
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load"></see> event and invalidates the SessionId in the cache.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs"></see> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                CacheManager.Remove(SessionId.ToString());
            }

            base.OnLoad(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"></see> event and attaches an event handler for the <see cref="E:System.Web.UI.Page.LoadComplete"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (this.Page != null)
            {
                this.Page.LoadComplete += new EventHandler(LoadCompleteEventHandler);
            }
        }

        #region IDataSource Members

        public virtual DataSourceView GetView(string viewName)
        {
            switch (viewName)
            {
                case "":
                case DEFAULT_VIEW:
                    return this.View;

                default:
                    throw new NotSupportedException(String.Format("'{0}' is not a supported view", viewName));
            }
        }

        public System.Collections.ICollection GetViewNames()
        {
            return new string[] { DEFAULT_VIEW };
        }

        #endregion

        public event EventHandler DataSourceChanged;

        /// <summary>
        /// Raises the DataSourceChanged event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDataSourceChanged(EventArgs e)
        {
            if (DataSourceChanged != null)
            {
                DataSourceChanged(this, e);
            }
        }

        #region Loading of children
        /// <summary>
        /// Gets the paged and filtered children of a specified page 
        /// Used when retreiving data for a paging data bound control.
        /// </summary>
        /// <param name="pageLink">The page for which the children should be loaded</param>
        /// <param name="startIndex"></param>
        /// <param name="maxRows"></param>
        /// <param name="loaded"></param>
        /// <returns>A filtered PageDataCollection</returns>
        protected internal IList<IContentData> GetChildren(ContentReference pageLink, int startIndex, int maxRows, out int loaded)
        {
            IList<IContentData> children = null;
            if (maxRows > 0)
            {
                List<int> range = Items.GetRange(startIndex, maxRows);
                loaded = Items.Count;
                children = new List<IContentData>();
                foreach (int pageId in range)
                {
                    IContentData page = this.DataFactory.Get<PageData>(new ContentReference(pageId), LanguageSelector.AutoDetect(UseFallbackLanguage));
                    if (page != null)
                    {
                        children.Add(page);
                    }
                }
            }
            else
            {
                children = GetFilteredChildren(pageLink);
                loaded = children.Count;
            }
            return children;
        }

        /// <summary>
        /// Callback method used by the PartialList of Pages to fill non populated ranges
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="maxRows"></param>
        /// <returns></returns>
        private List<int> GetChildrenCallback(int startIndex, int maxRows)
        {
            var pdc = DataFactory.GetChildren<PageData>(this.GetContentLink(),
                                                                          LanguageSelector.AutoDetect(
                                                                              UseFallbackLanguage), startIndex, maxRows);
            return pdc.Select(cd => ((IContent)cd).ContentLink.ID).ToList();
        }

        /// <summary>
        /// Callback method used for filtering pages retreived through the PartialList
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        private bool FilterContentCallback(int contentId)
        {
            IContentData content = DataFactory.Get<PageData>(new ContentReference(contentId), LanguageSelector.AutoDetect(UseFallbackLanguage));

            if (content == null)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region IDataSourceMethods members

        /// <summary>
        /// Returns the actual data of the data source.
        /// </summary>
        /// <returns>an enumeration of IContentData objects</returns>
        public virtual IEnumerable Select(DataSourceSelectArguments arguments)
        {
            int loaded;
            IList<IContentData> items = this.GetChildren(this.GetContentLink(), arguments.StartRowIndex, arguments.MaximumRows, out loaded);
            if (arguments.RetrieveTotalRowCount)
            {
                arguments.TotalRowCount = loaded;
            }
            return items;
        }

        /// <summary>
        /// Deletes a Page (and any children of the page) as specified by the PageLink entry in the keys collection or the DeleteParameters collection.
        /// </summary>
        /// <param name="values">The values passed to delete. Should include a param PageLink of type PageReference</param>
        /// <returns>a value</returns>
        public virtual int Delete(IDictionary values)
        {
            System.Collections.IDictionary deleteParams = values;

            this.DataFactory.Delete((PageReference)deleteParams["PageLink"], true, AccessLevel.Delete);

            OnDataSourceChanged(EventArgs.Empty);

            // TODO: the value returned from this method indicates that there was atleast on page deleted. The API states that we should return the total count of deleted pages which is not supported by the DataFactory.
            return 1;
        }

        public virtual int Insert(IDictionary values)
        {
            throw new System.NotSupportedException("Insert is not supported.");
        }

        public virtual int Update(IDictionary values)
        {
            throw new System.NotSupportedException("Update is not supported.");
        }

        #endregion

        /// <summary>
        /// Gets the current ContentLink based on the settings of the ContentLink and PageLinkProperty properties.
        /// </summary>
        /// <returns>A PageReference.</returns>
        protected virtual ContentReference GetContentLink()
        {
            if (!ContentReference.IsNullOrEmpty(ContentLink))
            {
                return ContentLink;
            }
            if (!String.IsNullOrEmpty(ContentLinkProperty))
            {
                IContentData dataSource = FindCurrentPropertyDataContainer();
                if (dataSource != null)
                {
                    var propertyData = PropertyResolver.Service.ResolveProperty<PropertyContentReference>(dataSource.Property, ContentLinkProperty);
                    if (propertyData != null && !propertyData.IsNull)
                    {
                        return propertyData.ContentLink;
                    }
                }
            }
            return ContentReference.EmptyReference;
        }

        private IContentData FindCurrentPropertyDataContainer()
        {
            for (Control c = Parent; c != null; c = c.Parent)
            {
                var bc = c as IContentDataControl;
                if (bc != null)
                {
                    return bc.CurrentData;
                }

            }
            return ContentSource != null ? ContentSource.CurrentContent : null;
        }


        /// <summary>
        /// Initializes the ContentSource property if necessary.
        /// </summary>
        private void SetupContentSource()
        {
            if (_contentSource != null)
            {
                return;
            }

            if (_contentSource == null)
                _contentSource = Page as IContentSource;
            if (_contentSource == null)
                _contentSource = ServiceLocation.ServiceLocator.Current.GetInstance<IContentSource>();
        }

        /// <summary>
        /// Event handler for the Page.LoadComplete event that forces the select parameters to be updated so that a subsequent ExecuteSelect on the DataSourceView is called.
        /// </summary>
        /// <param name="sender">The page object.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void LoadCompleteEventHandler(object sender, EventArgs e)
        {
            //this.SelectParameters.UpdateValues(this.Context, this);
        }

        [System.ComponentModel.Browsable(false)]
        public IContentRepository DataFactory
        {
            get
            {
                if (_dataFactory == null)
                {
                    _dataFactory = EPiServer.DataFactory.Instance;
                }

                return _dataFactory;
            }
            set
            {
                _dataFactory = value;
            }
        }

        #region IHierarchicalContentDataSource Members

        /// <summary>
        /// Performs the selection of pages based on the viewPath parameter.
        /// </summary>
        /// <remarks>Override this method to implement custom hierarchical behavior.</remarks>
        /// <param name="viewPath">The hierarchical path of the node to enumerate.</param>
        /// <returns>An IHierarchicalEnumerable representing the child structure of the specified path.</returns>
        public virtual ContentHierarchicalEnumerable HierarchicalSelect(string viewPath)
        {
            if (String.IsNullOrEmpty(viewPath))
            {
                if (ContentReference.IsNullOrEmpty(ContentLink))
                {
                    return new ContentHierarchicalEnumerable(Enumerable.Empty<PageData>(), this, 0);
                }
                if (IncludeRootItem)
                {
                    var items = new List<PageData>()
                    {
                        DataFactory.Get<PageData>(ContentLink)
                    };
                    return new ContentHierarchicalEnumerable(items, this, 0);
                }
                else
                {
                    return new ContentHierarchicalEnumerable(ContentLink, this, 0);
                }
            }
            return new ContentHierarchicalEnumerable(ContentReference.Parse(viewPath), this, 0);
        }

        /// <summary>
        /// Gets the filtered children of a specified content item.
        /// Used when retreiving data for a hierarchical view.
        /// </summary>
        /// <param name="contentLink">The current node in the tree view</param>
        /// <returns>A collection of child items</returns>
        public IList<IContentData> GetFilteredChildren(ContentReference contentLink)
        {
            var children = new List<IContentData>();
            foreach (var child in DataFactory.GetChildren<PageData>(contentLink, LanguageSelectorFactory.Service.AutoDetect(true)))
            {
                var content = child as IContent;
                if (!IncludeContentAssets && content != null && content.ContentLink.CompareToIgnoreWorkID(SiteDefinition.Current.GlobalAssetsRoot))
                {
                    continue;
                }

                children.Add(child);
            }

            return children;
        }

        /// <summary>
        /// Determines if a IContentData instance represents a root in the current hierarchicla view.
        /// </summary>
        /// <param name="content">The IContentData instance to match with.</param>
        /// <returns><b>true</b> if instance is a root node in the hierarchy, otherwise false.</returns>
        public virtual bool IsRoot(IContentData content)
        {
            return (IncludeRootItem && ContentLink == ((IContent)content).ContentLink) || (!IncludeRootItem && ContentLink == ((IContent)content).ParentLink);
        }

        #endregion

        #region IContentSource Members

        T IContentSource.Get<T>(ContentReference contentLink)
        {
            return DataFactory.Get<T>(contentLink);
        }

        IEnumerable<T> IContentSource.GetChildren<T>(ContentReference contentLink)
        {
            return DataFactory.GetChildren<T>(contentLink);
        }

        IContent IContentSource.CurrentContent
        {
            get { return null; }
        }

        #endregion
    }
}


