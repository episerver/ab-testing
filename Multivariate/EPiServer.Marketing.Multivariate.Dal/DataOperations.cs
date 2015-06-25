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
    public class DataOperations : IDataOperations
    {
        private string _connectionString;

        public DataOperations(string connectionString)
        {
            if (!string.IsNullOrWhiteSpace(connectionString))
                _connectionString = connectionString;
            else
            {
                throw new Exception("Connection String cannot be empty");
            }
        }

        public void ExecuteNonQuery(string commandText, CommandType commandType, SqlParameter[] parameters)
        {
            using (SqlConnection Connection = new SqlConnection(this._connectionString))
            {
                using (SqlCommand Command = Connection.CreateCommand())
                {
                    Command.CommandText = commandText;
                    Command.CommandType = commandType;
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                           Command.Parameters.Add(param);
                        }
                    }

                    Connection.Open();
                    Command.ExecuteNonQuery();
                }
            }
        }

        public DbDataReader ExecuteReader(string commandText, CommandType commandType, SqlParameter[] parameters)
        {
            using (SqlConnection Connection = new SqlConnection(this._connectionString))
            {
                using (SqlCommand Command = Connection.CreateCommand())
                {
                    Command.CommandText = commandText;
                    Command.CommandType = commandType;
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            Command.Parameters.Add(param);
                        }
                    }

                    Connection.Open();
                    return Command.ExecuteReader();
                }
            }
        }
    }
}
