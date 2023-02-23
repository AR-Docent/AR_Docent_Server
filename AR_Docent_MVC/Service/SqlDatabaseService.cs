using Dapper;
using System.Reflection;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System;

namespace AR_Docent_MVC.Service
{
    public class SqlDatabaseService<T>
    {
        private SqlConnection connection;
        public SqlDatabaseService(AzureKeyVaultService azurekey)
        {
            connection = new SqlConnection(azurekey.sqlConnectionString);
        }

        public List<T> GetItems(string dbName)
        {
            string _query = $"SELECT * FROM {dbName}";

            return connection.Query<T>(_query).ToList();
        }

        public T GetItemById(string dbName, int id)
        {
            string _query = $"SELECT * FROM {dbName} WHERE id={id}";
            Debug.WriteLine($"query:{_query}");
            return connection.QuerySingle<T>(_query);
        }
        //dapper 활용 array
        public void AddItem(string dbName, T obj)
        {
            //var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);

            Type type = obj.GetType();
            PropertyInfo[] properties = type.GetProperties();

            string q_k = "(", q_v = "(";
            for (int i = 0; i < properties.Length; i++)
            {
                if (properties[i].PropertyType == typeof(string) ||
                    (properties[i].PropertyType == typeof(int) && properties[i].Name != "id"))
                {
                    q_k += $"{properties[i].Name}";
                    q_v += $"@{properties[i].Name}";
                    if (i < properties.Length - 1)
                    {
                        q_k += ", ";
                        q_v += ", ";
                    }
                }
            }
            q_k += ")";
            q_v += ")";

            //(id, name, )
            string _query = $"INSERT INTO {dbName} {q_k} VALUES {q_v};";
            Debug.WriteLine("query: " + _query);
            
            connection.Execute(_query, obj);
        }

        public void DeleteByID(string dbName, int id)
        {
            string _query = $"DELETE FROM {dbName} WHERE id={id}";

            connection.Execute(_query);
        }
    }
}
