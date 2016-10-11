using EPiServer.Marketing.KPI.Common.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using EPiServer.Marketing.KPI.Common;
using EPiServer.Marketing.KPI.Results;

namespace EPiServer.Marketing.KPI.Manager.DataClass
{
    /// <summary>
    /// KeyPerformanceIndicator object that is used to define a test characteristic(i.e. page scroll, page click, etc.)
    /// </summary>
    [DataContract]
    public class Kpi : IKpi
    {
        public Kpi()
        {
            CreatedDate = DateTime.UtcNow;
            ModifiedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Id of Kpi.
        /// </summary>
        [DataMember]
        public Guid Id { get; set; }

        /// <summary>
        /// Overide to specify the FriendlyName to be displayed in the UI.
        /// </summary>
        [DataMember]
        public string FriendlyName { get {
                if (Attribute.IsDefined(GetType(), typeof(UIMarkupAttribute)))
                {
                    var attr = (UIMarkupAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(UIMarkupAttribute));
                    return attr.text;
                }
                else
                {
                    return GetType().Name;
                }
            }
        }

        [DataMember]
        public string Description {
            get
            {
                if (Attribute.IsDefined(GetType(), typeof(UIMarkupAttribute)))
                {
                    var attr = (UIMarkupAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(UIMarkupAttribute));
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
        public string UiMarkup
        {
            get
            {
                string value;
                if (Attribute.IsDefined(GetType(), typeof(UIMarkupAttribute)))
                {
                    var attr = (UIMarkupAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(UIMarkupAttribute));
                    if (!TryGetResourceString(attr.configmarkup, out value))
                    {
                        value = "failed to load " + attr.configmarkup + ":" + value;
                    }
                }
                else
                {
                    value = "UIMarkupAttribute class attribute not defined.";
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
        public string UiReadOnlyMarkup
        {
            get
            {
                string value;
                if (Attribute.IsDefined(GetType(), typeof(UIMarkupAttribute)))
                {
                    var attr = (UIMarkupAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(UIMarkupAttribute));
                    if( !TryGetResourceString(attr.readonlymarkup, out value) )
                    {
                        value = "failed to load " + attr.readonlymarkup + ":" + value;
                    }
                }
                else
                {
                    value = "UIMarkupAttribute class attribute not defined.";
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
        private bool TryGetResourceString(string key, out string value)
        {
            bool retval = false;
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var text = new StreamReader( assembly.GetManifestResourceStream(key) );
                value = text.ReadToEnd();
                retval = true;
            }
            catch(Exception e)
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
        /// <returns></returns>
        public virtual bool Validate(Dictionary<string,string> kpiData)
        {
            return true;
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
    }
}
