namespace Steampunks.DataLink
{
    using System;
    using System.IO;
    using Microsoft.Data.SqlClient;

    public static class DatabaseInitializer
    {
        private static readonly string MasterConnectionString = @"Server=localhost;Database=master;Trusted_Connection=True;TrustServerCertificate=True;";

        public static void InitializeDatabase()
        {
            try
            {
                // First check if the database is already initialized
                using (var databaseConnection = new SqlConnection(MasterConnectionString))
                {
                    databaseConnection.Open();
                    using (var checkIfDbExistscommand = new SqlCommand("SELECT COUNT(*) FROM sys.databases WHERE name = 'SteampunksDB'", databaseConnection))
                    {
                        var doesDbExists = (int)checkIfDbExistscommand.ExecuteScalar() > 0;
                        if (doesDbExists)
                        {
                            // Check if tables exist
                            using (var steampunksConnection = new SqlConnection(@"Server=localhost;Database=SteampunksDB;Trusted_Connection=True;TrustServerCertificate=True;"))
                            {
                                steampunksConnection.Open();
                                using (var checkIfTablesExistCommand = new SqlCommand(@" SELECT COUNT(*) FROM sys.tables WHERE name IN ('Users', 'Games', 'Items', 'UserInventory')", steampunksConnection))
                                {
                                    var doTablesExist = (int)checkIfTablesExistCommand.ExecuteScalar() == 4;
                                    if (doTablesExist)
                                    {
                                        System.Diagnostics.Debug.WriteLine("Database is already initialized.");
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }

                string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataLink", "Scripts", "InitializeDatabase.sql");

                if (!File.Exists(scriptPath))
                {
                    System.Diagnostics.Debug.WriteLine($"SQL script not found at path: {scriptPath}");
                    throw new FileNotFoundException("Database initialization script not found.", scriptPath);
                }

                string script = File.ReadAllText(scriptPath);
                System.Diagnostics.Debug.WriteLine("Successfully read SQL script file.");

                using (var dbConnection = new SqlConnection(MasterConnectionString))
                {
                    try
                    {
                        dbConnection.Open();
                        System.Diagnostics.Debug.WriteLine("Successfully opened connection to master database.");

                        // Split the script on GO statements
                        string[] commands = script.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (string command in commands)
                        {
                            if (!string.IsNullOrWhiteSpace(command))
                            {
                                using (var sqlCommand = new SqlCommand(command, dbConnection))
                                {
                                    try
                                    {
                                        sqlCommand.ExecuteNonQuery();
                                    }
                                    catch (Exception sqlCommandException)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"Error executing SQL command: {sqlCommandException.Message}");
                                        System.Diagnostics.Debug.WriteLine($"Command text: {command}");
                                        throw;
                                    }
                                }
                            }
                        }

                        System.Diagnostics.Debug.WriteLine("Successfully executed all SQL commands.");
                    }
                    catch (Exception dbConnectionException)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error connecting to database: {dbConnectionException.Message}");
                        throw;
                    }
                }
            }
            catch (Exception dbInitializationException)
            {
                System.Diagnostics.Debug.WriteLine($"Database initialization failed: {dbInitializationException.Message}");
                throw;
            }
        }

        public static void AddSampleData()
        {
            var db = new DatabaseConnector();

            // Add CS2 with skins
            db.AddGameWithItems(
                gameTitle: "Counter-Strike 2",
                gamePrice: 0.0f,
                genre: "FPS",
                description: "The next evolution of Counter-Strike, featuring updated graphics and refined gameplay.",
                items: new[] { ("AK-47 | Asiimov", 50.0f, "A sci-fi themed skin for the AK-47"), ("M4A4 | Howl", 1500.0f, "A rare and coveted M4A4 skin"), ("AWP | Dragon Lore", 10000.0f, "The legendary Dragon Lore skin"), ("Desert Eagle | Blaze", 450.0f, "A blazing hot Desert Eagle skin"), ("Butterfly Knife | Fade", 2000.0f, "A rainbow-colored butterfly knife") });

            // Add Dota 2 with items
            db.AddGameWithItems(
                gameTitle: "Dota 2",
                gamePrice: 0.0f,
                genre: "MOBA",
                description: "A complex MOBA game with over 100 unique heroes.",
                items: new[] { ("Dragonclaw Hook", 750.0f, "A legendary hook for Pudge"), ("Arcana: Demon Eater", 35.0f, "Special effects for Shadow Fiend"), ("Baby Roshan", 150.0f, "A cute courier skin"), ("Ethereal Flames War Dog", 1000.0f, "A rare courier with special effects") });

            System.Diagnostics.Debug.WriteLine("Sample data added successfully!");
        }
    }
}