using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Multivariate.Dal
{
    public interface IDataOperations
    {
        void ExecuteNonQuery(string commandText, CommandType commandType, SqlParameter[] parameters);
        void ExecuteReader(string commandText, CommandType commandType, SqlParameter[] parameters, Action<IDataReader> callBack);
    }
}
