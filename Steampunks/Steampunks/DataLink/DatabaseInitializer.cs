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
            try
            {
                string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataLink", "Scripts", "InitializeDatabase.sql");
                
                if (!File.Exists(scriptPath))
                {
                    System.Diagnostics.Debug.WriteLine($"SQL script not found at path: {scriptPath}");
                    throw new FileNotFoundException("Database initialization script not found.", scriptPath);
                }

                string script = File.ReadAllText(scriptPath);
                System.Diagnostics.Debug.WriteLine("Successfully read SQL script file.");

                using (var connection = new SqlConnection(MasterConnectionString))
                {
                    try
                    {
                        connection.Open();
                        System.Diagnostics.Debug.WriteLine("Successfully opened connection to master database.");

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
                                        System.Diagnostics.Debug.WriteLine($"Error executing SQL command: {ex.Message}");
                                        System.Diagnostics.Debug.WriteLine($"Command text: {command}");
                                        throw;
                                    }
                                }
                            }
                        }
                        System.Diagnostics.Debug.WriteLine("Successfully executed all SQL commands.");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error connecting to database: {ex.Message}");
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database initialization failed: {ex.Message}");
                throw;
            }
        }
    }
} 