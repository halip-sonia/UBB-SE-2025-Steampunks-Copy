using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using NUnit.Framework;
using Steampunks.Repository.Inventory;
using Steampunks.Domain.Entities;
using Steampunks.DataLink;
using Steampunks.Utils;

namespace Steampunks.Repository.InventoryTests
{
    [TestFixture]
    public class InventoryRepositoryIntegrationTests
    {
        private InventoryRepository repository;
        private IDatabaseConnector databaseConnector;
        private const string TestConnectionString = Configuration.CONNECTIONSTRINGDARIUS;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            databaseConnector = new DatabaseConnector();
            repository = new InventoryRepository(databaseConnector);

            // Clean the database – disable constraints in order to avoid deletion conflicts.
            using (var connection = databaseConnector.GetConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        ALTER TABLE dbo.UserInventory NOCHECK CONSTRAINT ALL;
                        ALTER TABLE dbo.Items NOCHECK CONSTRAINT ALL;
                        ALTER TABLE dbo.Games NOCHECK CONSTRAINT ALL;
                        ALTER TABLE dbo.Users NOCHECK CONSTRAINT ALL;
                    ";
                    await command.ExecuteNonQueryAsync();

                    // Delete data from dependent tables first.
                    command.CommandText = @"
                        DELETE FROM dbo.UserInventory;
                        DELETE FROM dbo.Items;
                        DELETE FROM dbo.Games;
                        DELETE FROM dbo.Users;
                    ";
                    await command.ExecuteNonQueryAsync();

                    //command.CommandText = @"
                    //    ALTER TABLE dbo.UserInventory WITH CHECK CHECK CONSTRAINT ALL;
                    //    ALTER TABLE dbo.Items WITH CHECK CHECK CONSTRAINT ALL;
                    //    ALTER TABLE dbo.Games WITH CHECK CHECK CONSTRAINT ALL;
                    //    ALTER TABLE dbo.Users WITH CHECK CHECK CONSTRAINT ALL;
                    //";
                    //await command.ExecuteNonQueryAsync();
                }
                connection.Close();
            }
        }

        [SetUp]
        public async Task Setup()
        {
            // Additional per-test cleanup if needed – for example, clear Items.
            using (var connection = databaseConnector.GetConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM dbo.Items;";
                    await command.ExecuteNonQueryAsync();
                }
                connection.Close();
            }
        }

        [Test]
        public void GetItemsFromInventoryAsync_NullGame_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await repository.GetItemsFromInventoryAsync(null));
        }

        [Test]
        public async Task GetItemsFromInventoryAsync_ValidGame_ReturnsCorrectItems()
        {
            // Arrange: Create a Game.
            var game = new Game("Test Game", 9.99f, "Adventure", "Test Description");
            game.SetGameId(100); // This value will be used in the Items table via CorrespondingGameId.

            // Create a dummy user as well if needed by your business logic.
            var user = new User("testuser");
            user.SetUserId(50);

            // Insert a dummy item row into dbo.Items using the column "CorrespondingGameId".
            using (var connection = databaseConnector.GetConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        INSERT INTO dbo.Items (CorrespondingGameId, ItemName, Price, Description, IsListed)
                        VALUES (@gameId, @itemName, @price, @description, @isListed);
                    ";
                    command.Parameters.Add(new SqlParameter("@gameId", game.GameId));
                    command.Parameters.Add(new SqlParameter("@itemName", "Test Item"));
                    command.Parameters.Add(new SqlParameter("@price", 19.99));
                    command.Parameters.Add(new SqlParameter("@description", "Item Description"));
                    command.Parameters.Add(new SqlParameter("@isListed", false));
                    await command.ExecuteNonQueryAsync();
                }
                connection.Close();
            }

            // Act: Retrieve items via the repository.
            var items = await repository.GetItemsFromInventoryAsync(game);

            // Assert: Verify that the returned item data is correct.
            Assert.IsNotNull(items, "Returned item list should not be null");
            Assert.AreEqual(1, items.Count, "There should be one item returned");

            var item = items[0];
            Assert.AreEqual("Test Item", item.ItemName, "ItemName should match");
            Assert.AreEqual(19.99f, item.Price, "Price should match");
            Assert.AreEqual("Item Description", item.Description, "Description should match");
            Assert.IsFalse(item.IsListed, "IsListed should be false as per the inserted data");
        }

        [Test]
        public async Task AddItemToInventoryAsync_ValidParameters_InsertsItem()
        {
            // Arrange: Create a Game, a User, and an Item.
            var game = new Game("Test Game", 9.99f, "RPG", "Game Description");
            game.SetGameId(101);
            var user = new User("newUser");
            user.SetUserId(200);
            var item = new Item("New Item", game, 5.99f, "Item Desc");

            // Insert the dummy Game into dbo.Games.
            using (var connection = databaseConnector.GetConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SET IDENTITY_INSERT dbo.Games ON;
                        INSERT INTO dbo.Games (GameId, Title, Price, Genre, Description, Status, RecommendedSpecs, MinimumSpecs)
                        VALUES (@GameId, @Title, @Price, @Genre, @Description, @Status, NULL, NULL);
                        SET IDENTITY_INSERT dbo.Games OFF;
                    ";
                    command.Parameters.Add(new SqlParameter("@GameId", game.GameId));
                    command.Parameters.Add(new SqlParameter("@Title", game.Title));
                    command.Parameters.Add(new SqlParameter("@Price", game.Price));
                    command.Parameters.Add(new SqlParameter("@Genre", game.Genre));
                    command.Parameters.Add(new SqlParameter("@Description", game.Description));
                    command.Parameters.Add(new SqlParameter("@Status", "Active"));
                    await command.ExecuteNonQueryAsync();
                }

                // Insert the dummy User into dbo.Users.
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SET IDENTITY_INSERT dbo.Users ON;
                        INSERT INTO dbo.Users (UserId, Username, WalletBalance, PointBalance, IsDeveloper)
                        VALUES (@UserId, @Username, 0, 0, 0);
                        SET IDENTITY_INSERT dbo.Users OFF;
                    ";
                    command.Parameters.Add(new SqlParameter("@UserId", user.UserId));
                    command.Parameters.Add(new SqlParameter("@Username", user.Username));
                    await command.ExecuteNonQueryAsync();
                }
                connection.Close();
            }

            // Act: Call the repository method to add the item.
            // (Assume that inside AddItemToInventoryAsync, the repository inserts into dbo.Items using "CorrespondingGameId" and
            // inserts into the linking table (dbo.UserInventory) to reference the user.)
            await repository.AddItemToInventoryAsync(game, item, user);

            // Assert: Query the dbo.Items table (which uses "CorrespondingGameId") to verify insertion.
            using (var connection = databaseConnector.GetConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT COUNT(*) 
                        FROM dbo.UserInventory 
                        WHERE UserId = @UserId AND GameId = @GameId AND ItemId = @ItemId;
                    ";
                    command.Parameters.Add(new SqlParameter("@UserId", user.UserId));
                    command.Parameters.Add(new SqlParameter("@gameId", game.GameId));
                    command.Parameters.Add(new SqlParameter("@ItemId", item.ItemId));

                    int count = (int)await command.ExecuteScalarAsync();
                    Assert.AreEqual(1, count, "Item should have been inserted into dbo.Items.");
                }
                connection.Close();
            }
        }

        [Test]
        public async Task SellItemAsync_ValidItem_CompletesSale()
        {
            // Arrange: Create a Game and an Item.
            var game = new Game("Sell Game", 15.99f, "Action", "Some Description");
            game.SetGameId(300);
            var item = new Item("Sellable Item", game, 9.99f, "To Sell");

            // Insert a dummy item row into dbo.Items using "CorrespondingGameId".
            // Note: We insert with IsListed = 0 (unsold) so that the sale operation can update it.
            int insertedItemId;
            using (var connection = databaseConnector.GetConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                INSERT INTO dbo.Items (CorrespondingGameId, ItemName, Price, Description, IsListed)
                OUTPUT INSERTED.ItemId
                VALUES (@gameId, @itemName, @price, @description, @isListed);
            ";
                    command.Parameters.Add(new SqlParameter("@gameId", game.GameId));
                    command.Parameters.Add(new SqlParameter("@itemName", "Sellable Item"));
                    command.Parameters.Add(new SqlParameter("@price", 9.99));
                    command.Parameters.Add(new SqlParameter("@description", "To Sell"));
                    // Initially unsold.
                    command.Parameters.Add(new SqlParameter("@isListed", false));
                    insertedItemId = (int)await command.ExecuteScalarAsync();
                }
                connection.Close();
            }
            item.SetItemId(insertedItemId);

            // Act: Call SellItemAsync – the repository should update the item
            // to mark it as sold (i.e. set IsListed to 1).
            bool result = await repository.SellItemAsync(item);

            // Assert: Verify that SellItemAsync returns true.
            Assert.IsTrue(result, "SellItemAsync should return true upon successful sale.");

            // Assert: Verify that the item is marked as sold (IsListed = 1).
            using (var connection = databaseConnector.GetConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT IsListed FROM dbo.Items WHERE ItemId = @itemId;";
                    command.Parameters.Add(new SqlParameter("@itemId", insertedItemId));

                    bool isListed = (bool)await command.ExecuteScalarAsync();
                    Assert.IsTrue(isListed, "Item should have been marked as sold (IsListed = true).");
                }
                connection.Close();
            }
        }



        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            // Optionally, clean up test data.
            using (var connection = databaseConnector.GetConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        DELETE FROM dbo.UserInventory;
                        DELETE FROM dbo.Items;
                        DELETE FROM dbo.Games;
                        DELETE FROM dbo.Users;
                    ";
                    await command.ExecuteNonQueryAsync();

                    command.CommandText = @"
                        ALTER TABLE dbo.UserInventory WITH CHECK CHECK CONSTRAINT ALL;
                        ALTER TABLE dbo.Items WITH CHECK CHECK CONSTRAINT ALL;
                        ALTER TABLE dbo.Games WITH CHECK CHECK CONSTRAINT ALL;
                        ALTER TABLE dbo.Users WITH CHECK CHECK CONSTRAINT ALL;
                    ";
                    await command.ExecuteNonQueryAsync();
                }
                connection.Close();
            }
        }
    }
}
