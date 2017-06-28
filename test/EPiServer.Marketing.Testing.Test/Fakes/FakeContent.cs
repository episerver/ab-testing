using System;
using EPiServer.Core;
using EPiServer.Data.Entity;

namespace EPiServer.Marketing.Testing.Test.Fakes
{
    /// <summary>
    /// Fake implementation of IContent and IReadOnly where all properties and methods can be mocked 
    /// </summary>
    public class FakeContent : IContent, IReadOnly, IChangeTrackable
    {
        #region IContent
        private PropertyDataCollection _propertyData;

        public virtual Guid ContentGuid { get; set; }
        public virtual ContentReference ContentLink { get; set; }
        public virtual int ContentTypeID { get; set; }
        public virtual bool IsDeleted { get; set; }
        public virtual string Name { get; set; }
        public virtual ContentReference ParentLink { get; set; }
        public virtual PropertyDataCollection Property
        {
            get { return _propertyData ?? (_propertyData = new PropertyDataCollection()); }
            set { _propertyData = value; }
        }

        #endregion

        #region IReadOnly

        public virtual object CreateWritableClone()
        {
            return this;
        }

        public virtual bool IsReadOnly { get; set; }
        public virtual void MakeReadOnly() { }

        #endregion

        #region IChangeTrackable

        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Changed { get; set; }
        public bool SetChangedOnPublish { get; set; }
        public string ChangedBy { get; set; }
        public DateTime Saved { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? Deleted { get; set; }

        #endregion
    }

    public class FakeLocalizableContent : FakeContent, ILocalizable, IVersionable
    {
        public System.Collections.Generic.IEnumerable<System.Globalization.CultureInfo> ExistingLanguages { get; set; }

        public System.Globalization.CultureInfo MasterLanguage
        {
            get;
            set;
        }

        public System.Globalization.CultureInfo Language
        {
            get;
            set;
        }

        public bool IsPendingPublish
        {
            get;
            set;
        }

        public DateTime? StartPublish
        {
            get;
            set;
        }

        public VersionStatus Status
        {
            get;
            set;
        }

        public DateTime? StopPublish
        {
            get;
            set;
        }
    }
}