using System;
using EPiServer.Marketing.KPI.Results;
using System.Collections.Generic;
using EPiServer.Marketing.KPI.Manager.DataClass.Enums;

namespace EPiServer.Marketing.KPI.Manager.DataClass
{
    /// <summary>
    /// KeyPerformanceIndicator object that is used to define a test characteristic(i.e. page scroll, page click, etc.)
    /// </summary>
    public interface IKpi
    {
        /// <summary>
        /// ID of KPI.
        /// </summary>
        Guid Id { get; set; }

        /// <summary>
        /// Indicates which result should be considered the winning result.
        /// </summary>
        ResultComparison ResultComparison { get; }

        /// <summary>
        /// Indicates the expected result type used by the KPI instance.
        /// </summary>
        string KpiResultType { get; }

        /// <summary>
        /// Name displayed in the UI, default displays class type name.
        /// </summary>
        string FriendlyName { get; }

        /// <summary>
        /// Optional description for the UI.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Call by the UI to get the markup for the configuration UI for the control. There are two ways you can use this, 
        /// 1) decorate your class with the UIMarkupAttribute and specify the config markup resource found in your assembly.
        /// 2) overide and return your markup string directly.
        /// </summary>
        string UiMarkup { get; }

        /// <summary>
        /// Call by the UI to get the markup for the configuration UI for the control. There are two ways you can use this, 
        /// 1) decorate your class with the UIMarkupAttribute and specify the config markup resource found in your assembly.
        /// 2) overide and return your markup string directly.
        /// </summary>
        string UiReadOnlyMarkup { get; }

        /// <summary>
        /// Date the KPI was created.
        /// </summary>
        DateTime CreatedDate { get; set; }

        /// <summary>
        /// The last time the KPI was modified.
        /// </summary>
        DateTime ModifiedDate { get; set; }

        /// <summary>
        /// Provides specific validation of data prior to creating the KPI.
        /// </summary>
        /// <param name="responseData">Dictionary of data used to validate and save an instance of a KPI.</param>
        void Validate(Dictionary<string,string> responseData);

        /// <summary>
        /// Determines if a conversion has happened.  Each KPI will decide this differently based on the sender, event args, and the purpose of the KPI.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">The expected Event Argument which contains the necessary info used to decide if a conversion has occured.</param>
        /// <returns>A result containing the necessary data to record a conversion.</returns>
        IKpiResult Evaluate(object sender, EventArgs e);

        /// <summary>
        /// The event to trigger the evaluate from. Return any event from the evironment that 
        /// can be used to trigger the Evaluate method.
        /// </summary>
        event EventHandler EvaluateProxyEvent;

        /// <summary>
        /// Override to initalize any internal data.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Overided for any internal KPI instance cleanup.
        /// </summary>
        void Uninitialize();
    }
}
