using System;
using System.IO;
using Microsoft.Data.SqlClient;

namespace Steampunks.DataLink
{
    public static class DatabaseInitializer
    {
        private static readonly string MasterConnectionString = @"Server=localhost;Database=master;Trusted_Connection=True;TrustServerCertificate=True;";

        public static void InitializeDatabase()
        {
            string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataLink", "Scripts", "InitializeDatabase.sql");
            string script = File.ReadAllText(scriptPath);

            using (var connection = new SqlConnection(MasterConnectionString))
            {
                connection.Open();

                // Split the script on GO statements
                string[] commands = script.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string command in commands)
                {
                    if (!string.IsNullOrWhiteSpace(command))
                    {
                        using (var cmd = new SqlCommand(command, connection))
                        {
                            try
                            {
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error executing command: {ex.Message}");
                                throw;
                            }
                        }
                    }
                }
            }
        }
    }
} 