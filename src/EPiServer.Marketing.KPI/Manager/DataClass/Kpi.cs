using EPiServer.Marketing.KPI.Common.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using EPiServer.ServiceLocation;
using EPiServer.Framework.Localization;
using EPiServer.Marketing.KPI.Results;
using EPiServer.Marketing.KPI.Manager.DataClass.Enums;
using System.Globalization;

namespace EPiServer.Marketing.KPI.Manager.DataClass
{
    /// <summary>
    /// KeyPerformanceIndicator object that is used to define a test characteristic(i.e. page scroll, page click, etc.)
    /// </summary>
    [DataContract]
    public class Kpi : IKpi
    {
        protected IServiceLocator _servicelocator;

        public Kpi()
        {
            CreatedDate = DateTime.UtcNow;
            ModifiedDate = DateTime.UtcNow;
            _servicelocator = ServiceLocator.Current;
        }

        /// <summary>
        /// Id of Kpi.
        /// </summary>
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        /// <summary>
        /// Indicates which result should be considered the "winner"
        /// Overide to specify a different result comparison
        /// </summary>
        public virtual ResultComparison resultComparison
        {
            get
            {
                return ResultComparison.Greater;
            }
        }

        /// <summary>
        /// Indicates which result should be considered the "winner"
        /// Overide to specify a different result comparison
        /// </summary>
        public virtual ResultComparison resultComparison
        {
            get
            {
                return ResultComparison.Greater;
            }
        }

        /// <summary>
        /// Overide to specify the FriendlyName to be displayed in the UI.
        /// </summary>
        [DataMember]
        public string FriendlyName {
            get {
                if (Attribute.IsDefined(GetType(), typeof(UIMarkupAttribute)))
                {
                    var attr = (UIMarkupAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(UIMarkupAttribute));
                    if( attr.text_id != null )
                    {
                        return _servicelocator.GetInstance<LocalizationService>().GetString(attr.text_id);
                    }
                    return attr.text;
                }
                else
                {
                    return GetType().Name;
                }
            }
        }

        /// <summary>
        /// Indicates the result type used by the kpi.
        /// Override to properly display values in the UI.
        /// </summary>
        [DataMember]
        public virtual string kpiResultType
        {
            get
            {
                return typeof(KpiConversionResult).Name.ToString();
            }
        }


        [DataMember]
        public virtual string Description
        {
            get
            {
                if (Attribute.IsDefined(GetType(), typeof(UIMarkupAttribute)))
                {
                    var attr = (UIMarkupAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(UIMarkupAttribute));
                    if (attr.text_id != null)
                    {
                        return _servicelocator.GetInstance<LocalizationService>().GetString(attr.description_id);
                    }
                    return attr.description;
                }
                else
                {
                    return "";
                }
            }
        }

        /// <summary>
        /// Call by the UI to get the markup for the configuration UI for the control. There are two ways you can use this, 
        /// 1) decorate your class with the UIMarkupAttribute and specify the config markup resource found in your assembly or
        /// 2) overide and return your markup string directly
        /// </summary>
        [DataMember]
        public virtual string UiMarkup
        {
            get
            {
                string value;
                if (Attribute.IsDefined(GetType(), typeof(UIMarkupAttribute)))
                {
                    var attr = (UIMarkupAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(UIMarkupAttribute));
                    if (!TryGetResourceString(attr.configmarkup, out value))
                    {
                        value = _servicelocator.GetInstance<LocalizationService>().GetString("/kpi/kpi_messaging/failed_to_load") + attr.readonlymarkup + ":" + value;
                    }
                }
                else
                {
                    value = _servicelocator.GetInstance<LocalizationService>().GetString("/kpi/kpi_messaging/UIMarkup_not_defined");
                }
                return value;
            }
        }

        /// <summary>
        /// Call by the UI to get the markup for the configuration UI for the control. There are two ways you can use this, 
        /// 1) decorate your class with the UIMarkupAttribute and specify the config markup resource found in your assembly or
        /// 2) overide and return your markup string directly
        /// </summary>
        [DataMember]
        public virtual string UiReadOnlyMarkup
        {
            get
            {
                string value;
                if (Attribute.IsDefined(GetType(), typeof(UIMarkupAttribute)))
                {
                    var attr = (UIMarkupAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(UIMarkupAttribute));
                    if (!TryGetResourceString(attr.readonlymarkup, out value))
                    {
                        value = _servicelocator.GetInstance<LocalizationService>().GetString("/kpi/kpi_messaging/failed_to_load") + attr.readonlymarkup + ":" + value;
                    }
                }
                else
                {
                    value = _servicelocator.GetInstance<LocalizationService>().GetString("/kpi/kpi_messaging/UIMarkup_not_defined");
                }
                return value;
            }
        }

        /// <summary>
        /// Given the specified Namespace.filename key we will load the string from the file found in this assembly. If this fails 
        /// its probably because the key is wrong or the resources is not in the assembly. See 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>true if loaded, else false. If false value contains the exception message.</returns>
        internal bool TryGetResourceString(string key, out string value)
        {
            bool retval = false;
            try
            {
                var assembly = this.GetType().Assembly;
                var text = new StreamReader(assembly.GetManifestResourceStream(key));
                value = text.ReadToEnd();
                retval = true;
            }
            catch (Exception e)
            {
                value = e.Message;
            }
            return retval;
        }

        /// <summary>
        /// Date the kpi was created.
        /// </summary>
        [DataMember]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The last time the kpi was modified.
        /// </summary>
        [DataMember]
        public DateTime ModifiedDate { get; set; }
        
        /// <summary>
        /// Provides specific validation of data prior to creating the KPI
        /// </summary>
        /// <param name="kpiData"></param>
        public virtual void Validate(Dictionary<string, string> kpiData)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Determines if a conversion has happened.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Event Argument</param>
        public virtual IKpiResult Evaluate(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public virtual NumberFormatInfo numberFormat
        {
            get
            {
                return CultureInfo.GetCultureInfo("en-US").NumberFormat;
            }
        }

        public virtual event EventHandler EvaluateProxyEvent;
    }
}
