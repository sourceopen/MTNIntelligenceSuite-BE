using System;
using System.Collections.Generic;
using Npgsql;

namespace DBFramework
{
    public class DBConnection
    {
        public DBConnection()
        {
            var cs = "Host=localhost;Username=postgres;Password=P@ssw0rd;Database=MIS-DB";
            int error = 0;
            /*
            using var con = new NpgsqlConnection(cs);
            con.Open();

            NpgsqlCommand sqlCommand = new NpgsqlCommand();
            sqlCommand.Connection = con;

            sqlCommand.CommandText = "SELECT * from models";
            NpgsqlDataReader npgsqlDataReader = null;
            try
            {
                npgsqlDataReader = sqlCommand.ExecuteReader();
            } catch(PostgresException ex)
            {
                if (ex.Code == PostgresErrorCodes.UndefinedTable)
                    error = -1;
            }

            if (error == -1) //table does not exist
            {
                sqlCommand.CommandText = @"CREATE TABLE models(id SERIAL PRIMARY KEY, name VARCHAR(255), modelDate DATETIME)";
                sqlCommand.ExecuteNonQuery();
            }

            List<string> modelNames = new List<string>();

            while (npgsqlDataReader!= null && npgsqlDataReader.Read())
            {
                modelNames.Add(npgsqlDataReader.GetString(1));
            }*/
        }
    }
}
