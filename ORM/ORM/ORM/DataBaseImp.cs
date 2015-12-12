using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace ORM
{
    public class DataBaseImp : DataBase
    {

        public string StringConnection { get; private set; }
        private TransactionScope TransactionScope { get; set; }

        public DataBaseImp(string stringConnection)
        {
            StringConnection = stringConnection;
        }
        
        public void BeginTransaction()
        {
            if(TransactionScope !=null)
                throw new ApplicationException("BeginTransaction try to use without commit or rollback last transaction");
            TransactionScope = new TransactionScope {Connection = new SqlConnection(StringConnection)};
            TransactionScope.Connection.Open();
            TransactionScope.Transaction = TransactionScope.Connection.BeginTransaction();
            TransactionScope.Command = TransactionScope.Connection.CreateCommand();
            TransactionScope.Command.Transaction = TransactionScope.Transaction;
        }

        private void DisposeTransaction()
        {
            TransactionScope.Command.Dispose();
            TransactionScope.Connection.Dispose();
            if (TransactionScope.Connection.State == ConnectionState.Open)
                TransactionScope.Connection.Close();
            TransactionScope = null;
        }

        public void CommitTransaction()
        {
            if(TransactionScope==null)
                throw new ApplicationException("TransactionScope is null while you try to do commit");
            TransactionScope.Transaction.Commit();
            DisposeTransaction();
        }

        public void RollBackTransaction()
        {
            if (TransactionScope == null)
                throw new ApplicationException("TransactionScope is null while you try to do rollback");
            TransactionScope.Transaction.Rollback();
            DisposeTransaction();
        }

        public void RunMerge(string mergeStatement)
        {
            if (TransactionScope == null)
                throw new ApplicationException("TransactionScope is null");
            TransactionScope.Command.CommandText = mergeStatement;
            TransactionScope.Command.ExecuteNonQuery();
        }

        public List<T> RunQuery<T>(string queryStatement)
        {
            SqlConnection sqlConnection = new SqlConnection(StringConnection);
            using (sqlConnection)
            {
                SqlCommand sqlCommand = null;
                IDataReader dataReader = null;
                List<T> objects;
                try
                {
                    sqlConnection.Open();
                    sqlCommand = new SqlCommand(queryStatement, sqlConnection);
                    sqlCommand.CommandTimeout = 600;
                    dataReader = sqlCommand.ExecuteReader();
                    objects = MapObject<T>(dataReader);
                }
                finally
                {
                    if (sqlCommand != null) 
                        sqlCommand.Dispose();
                    if (dataReader != null && !dataReader.IsClosed) 
                        dataReader.Close();
                    if(sqlConnection.State == ConnectionState.Open)
                        sqlConnection.Close();
                }
                return objects;
            }
        }

        internal List<T> MapObject<T>(IDataReader dataReader)
        {
            List<T> objects = new List<T>();
            Type type = typeof(T);
            while (dataReader.Read())
            {
                T obj = (T)Activator.CreateInstance(type);
                for (int i = 0; i < dataReader.FieldCount; i++)
                {
                    if (dataReader.IsDBNull(i)) continue;
                    PropertyInfo property = type.GetProperty(dataReader.GetName(i), BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (property != null)
                        property.SetValue(obj, dataReader[i], null);
                }
                objects.Add(obj);
            }
            return objects;
        }
        
    }
}
