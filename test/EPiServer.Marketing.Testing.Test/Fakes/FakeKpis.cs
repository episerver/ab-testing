using EPiServer.Marketing.KPI.Manager.DataClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.Marketing.KPI.Manager.DataClass.Enums;
using EPiServer.Marketing.KPI.Results;

namespace EPiServer.Marketing.Testing.Test.Fakes
{
    public class FakeClientKpi : IKpi, IClientKpi
    {
        public Guid Id { get; set; }
        public string ClientEvaluationScript { get { return "FakeClientKpi"; } }

        #region Not Implemented
        public DateTime CreatedDate
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public string Description
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string FriendlyName
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        
        public string KpiResultType
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public DateTime ModifiedDate
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public ResultComparison ResultComparison
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string UiMarkup
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string UiReadOnlyMarkup
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public event EventHandler EvaluateProxyEvent;

        public IKpiResult Evaluate(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void Uninitialize()
        {
            throw new NotImplementedException();
        }

        public void Validate(Dictionary<string, string> kpiData)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    public class FakeServerKpi : IKpi
    {
        public Guid Id { get; set; }

        #region Not Implemented
        public DateTime CreatedDate
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public string Description
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string FriendlyName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        

        public string KpiResultType
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public DateTime ModifiedDate
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public ResultComparison ResultComparison
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string UiMarkup
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string UiReadOnlyMarkup
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public event EventHandler EvaluateProxyEvent;

        public IKpiResult Evaluate(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void Uninitialize()
        {
            throw new NotImplementedException();
        }

        public void Validate(Dictionary<string, string> kpiData)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
