using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Configuration;
using System.Reflection;

namespace SessionLogin.Factories
{
    public abstract class AutoFactory<T>
    {
        // Local reference to the ConnectionString set in the WebConfig Root file.
        private string connectionString = "";

        // Creating a List of Properties, containing information about the current Type's properties
        private List<PropertyInfo> properties = new List<PropertyInfo>();

        public AutoFactory()
        {
            // Get the ConnectionString from the WebConfig
            connectionString = ConfigurationManager.ConnectionStrings["String"].ConnectionString;
            // Get a list of properties from the current Type
            properties.AddRange(GetGenericType().GetType().GetProperties());
        }

        /// <summary>
        /// Used to create a temp instance of the current Type
        /// </summary>
        /// <returns>The temp instance of current Type</returns>
        private T GetGenericType()
        {
            T t;
            return t = Activator.CreateInstance<T>();
        }

        /// <summary>
        /// Adds entity to the database table
        /// </summary>
        /// <param name="entity">Entity to Add</param>
        public void Add(T entity)
        {
            // Sql insert query - {0} is the table name (typeof(T).Name)
            string sqlQuery = string.Format("INSERT INTO [{0}] (", typeof(T).Name);

            // Loops through the properties of the current Type
            for (int i = 0; i < properties.Count; i++)
            {
                // For each property, we add the property name to the sql query
                // where the property name refers to a column in the table
                PropertyInfo property = properties[i];
                if (property.Name.ToLower().Contains("id") && i == 0) continue;

                sqlQuery += property.Name;
                sqlQuery += (i + 1 == properties.Count ? "" : ", ");
            }

            // Ending the Insert statement
            sqlQuery += ") ";
            // Starting the Values statement
            sqlQuery += "VALUES (";

            // Loops through the properties of the current Type
            for (int i = 0; i < properties.Count; i++)
            {
                // We add parameterized queries to the sql query
                // A parameterized query starts with @ and then the column name
                PropertyInfo property = properties[i];
                if (property.Name.ToLower().Contains("id") && i == 0) continue;

                sqlQuery += "@" + property.Name + (i + 1 == properties.Count ? "" : ", ");
            }

            // Ending the Values statement
            sqlQuery += ")";

            // We open a connection with the current connectionstring
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            // Generating the Sql Command to run on the database
            SqlCommand cmd = new SqlCommand(sqlQuery, connection);

            // Loops through the properties of the current Type
            for (int i = 0; i < properties.Count; i++)
            {
                // Passing in values for the properties
                if (properties[i].Name.ToLower().Contains("id") && i == 0) continue;
                cmd.Parameters.AddWithValue("@" + properties[i].Name, properties[i].GetValue(entity));
            }

            // Executing the SQL statement
            cmd.ExecuteNonQuery();


            // Disposing and closing connection
            cmd.Dispose();
            connection.Dispose();
            connection.Close();
        }

        /// <summary>
        /// Update entity
        /// </summary>
        /// <param name="entity">The entity to update, must contain ID</param>
        public void Update(T entity)
        {
            string sqlQuery = string.Format("UPDATE  [{0}] SET ", typeof(T).Name);

            // Loops through the properties of the current Type
            for (int i = 0; i < properties.Count; i++)
            {
                // For each property, we add the property name to the sql query
                // where the property name refers to a column in the table
                PropertyInfo property = properties[i];
                if (property.Name.ToLower().Contains("id") && i == 0) continue;

                sqlQuery += property.Name + "=@" + property.Name;
                sqlQuery += (i + 1 == properties.Count ? "" : ", ");
            }

            // Adding Filtering with WHERE statement
            sqlQuery += string.Format(" WHERE ID = {0} ", properties[0].GetValue(entity));

            // We open a connection with the current connectionstring
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            // Generating the Sql Command to run on the database
            SqlCommand cmd = new SqlCommand(sqlQuery, connection);

            // Loops through the properties of the current Type
            for (int i = 0; i < properties.Count; i++)
            {
                // Passing in values for the properties
                if (properties[i].Name.ToLower().Contains("id") && i == 0) continue;
                cmd.Parameters.AddWithValue("@" + properties[i].Name, properties[i].GetValue(entity));
            }

            // Executing the SQL statement
            cmd.ExecuteNonQuery();

            // Disposing and closing connection
            cmd.Dispose();
            connection.Dispose();
            connection.Close();
        }

        /// <summary>
        /// Get entity from the database
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The requested Entity by ID filtering</returns>
        public T Get(int id)
        {
            // Creating the SELECT SQL Statement, with {0} as Table name and {1} as the ID
            string sqlQuery = string.Format("SELECT * FROM [{0}] WHERE ID = {1}", typeof(T).Name, id);

            // We open a connection with the current connectionstring
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            // Generating the Sql Command to run on the database
            SqlCommand cmd = new SqlCommand(sqlQuery, connection);

            // Creating a Reader to contain the response from the database
            SqlDataReader reader = cmd.ExecuteReader();

            // Creating a result object to hold the response from the database
            T result = GetGenericType();

            // Does the server got a respond for us?
            if (reader.HasRows)
            {
                // As long as there is rows to read, do this
                while (reader.Read())
                {
                    // Loops through the properties of the current type
                    for (int i = 0; i < properties.Count; i++)
                    {
                        // If the value from the database is Null, we continue
                        if (reader[i] == DBNull.Value) continue;
                        // Setting the property value as the value from the database
                        properties[i].SetValue(result, reader[i]);
                    }
                }
            }

            // Disposing and closing connection
            cmd.Dispose();
            connection.Dispose();
            connection.Close();

            // returning result
            return result;
        }

        /// <summary>
        /// Gets all entities from the database
        /// </summary>
        /// <returns>Returns a list of T where T is a table from the database</returns>
        public List<T> GetAll()
        {
            // Creating the SELECT SQL Statement, with {0} as Table name
            string sqlQuery = string.Format("SELECT * FROM [{0}]", typeof(T).Name);

            // We open a connection with the current connectionstring
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            // Generating the Sql Command to run on the database
            SqlCommand cmd = new SqlCommand(sqlQuery, connection);

            // Creating a Reader to contain the _response_ from the database
            SqlDataReader reader = cmd.ExecuteReader();

            // Creating a entity holder object to hold the response from the database
            T entity = default(T);

            // Creating a result list to hold the _responses_ from the database
            List<T> result = new List<T>();

            // Does the server got a respond for us?
            if (reader.HasRows)
            {
                // As long as there is rows to read, do this
                while (reader.Read())
                {
                    // Creating the Entity object, it can now be used to set data
                    entity = GetGenericType();
                    // Loops through the properties of the current type
                    for (int i = 0; i < properties.Count; i++)
                    {
                        // If the value from the database is Null, we continue
                        if (reader[i] == DBNull.Value) continue;
                        // Setting the property value as the value from the database
                        properties[i].SetValue(entity, reader[i], null);
                    }
                    // Adding the entity to the List and continuing to the next field
                    result.Add(entity);
                }
            }

            // disposning and closing connection
            cmd.Dispose();
            connection.Dispose();
            connection.Close();

            // returning result
            return result;
        }

        /// <summary>
        /// Deletes entity by ID
        /// </summary>
        /// <param name="id">ID reference in the database</param>
        public void Delete(int id)
        {
            // Creating the Delete statement, where {0} is the table name, and {1} is the ID parameter from this method
            string sqlQuery = string.Format("DELETE FROM [{0}] WHERE ID = {1}", typeof(T).Name, id);

            // We open a connection with the current connectionstring
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            // Generating the Sql Command to run on the database
            SqlCommand cmd = new SqlCommand(sqlQuery, connection);

            // Executing the Delete statement
            cmd.ExecuteNonQuery();

            // disposning and closing connection
            cmd.Dispose();
            connection.Dispose();
            connection.Close();
        }
    }
}