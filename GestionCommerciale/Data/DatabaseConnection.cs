using MySql.Data.MySqlClient;
using System;
using System.Data;

namespace GestionCommerciale.Data
{
    public static class DatabaseConnection
    {
        private const string SERVER = "localhost";
        private const string DATABASE = "gestion_commerciale";
        private const string USER = "root";
        private const string PORT = "3306";
        private static readonly string PASSWORD =
            Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";

        private static string ConnectionString =>
            $"Server={SERVER};Port={PORT};Database={DATABASE};Uid={USER};Pwd={PASSWORD};Charset=utf8;";

        // AJOUT : Méthode manquante pour récupérer la chaîne de connexion
        public static string GetConnectionString()
        {
            return ConnectionString;
        }

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }

        public static void TestConnection()
        {
            using (var conn = GetConnection())
            {
                try
                {
                    Console.WriteLine("Tentative de connexion à MySQL...");
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                        Console.WriteLine("SUCCÈS : La connexion est établie !");
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine("ERREUR MYSQL :");
                    Console.WriteLine($"- Code : {ex.Number}");
                    Console.WriteLine($"- Message : {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERREUR INATTENDUE : {ex.Message}");
                }
            }
        }
    }
}