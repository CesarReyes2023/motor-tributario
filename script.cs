using System;
using Microsoft.Data.Sqlite;

class Program
{
    static void Main()
    {
        var connStr = "Data Source=librofiscal.db";
        using var connection = new SqliteConnection(connStr);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = "SELECT Resumen FROM Dtes LIMIT 5";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var json = reader.IsDBNull(0) ? "NULL" : reader.GetString(0);
            Console.WriteLine(json);
        }
    }
}
