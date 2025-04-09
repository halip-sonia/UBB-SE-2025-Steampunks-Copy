using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Moq;
using NUnit.Framework;
using Steampunks.Repository.Inventory;           // Contains InventoryRepository
using Steampunks.Domain.Entities;                  // Contains domain classes: Item, Game, User, etc.
using Steampunks.DataLink;                         // Contains IDatabaseConnector

namespace Steampunks.Repository.Tests
{
    [TestFixture]
    public class InventoryRepositoryTests
    {
        private InventoryRepository repository;
        private Mock<IDatabaseConnector> mockDbConnector;
        private Mock<SqlConnection> mockConnection;
        private Mock<SqlCommand> mockCommand;
        private Mock<SqlDataReader> mockReader;

        [SetUp]
        public void Setup()
        {
            // Arrange: Create a mock for IDatabaseConnector.
            mockDbConnector = new Mock<IDatabaseConnector>();

            // Create a fake SqlConnection using the constructor that accepts a connection string.
            // This ensures that the returned object is of type Microsoft.Data.SqlClient.SqlConnection.
            mockConnection = new Mock<SqlConnection>("FakeConnectionString");

            // Assume the connector returns our fake SqlConnection.
            mockDbConnector.Setup(x => x.GetConnection()).Returns(mockConnection.Object);

            // Simulate connection open and close operations.
            mockDbConnector.Setup(x => x.OpenConnectionAsync()).Returns(Task.CompletedTask);
            mockDbConnector.Setup(x => x.CloseConnection());

            // Instantiate the repository with the injected connector.
            repository = new InventoryRepository(mockDbConnector.Object);
        }

        [Test]
        public void GetItemsFromInventoryAsync_NullGame_ThrowsArgumentNullException()
        {
            // Arrange
            Game nullGame = null;

            // Act & Assert: Expect an ArgumentNullException when null Game is provided.
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await repository.GetItemsFromInventoryAsync(nullGame));
        }

        [Test]
        public async Task GetItemsFromInventoryAsync_ValidGame_ReturnsCorrectItems()
        {
            // Arrange: Create a valid Game object.
            var game = new Game("Test Game", 9.99f, "Adventure", "Test Description");
            game.SetGameId(100);

            // Create and configure a fake current User.
            var user = new User("testuser");
            user.SetUserId(50);
            mockDbConnector.Setup(x => x.GetCurrentUser()).Returns(user);

            // Arrange a fake DbDataReader to simulate one record.
            mockReader = new Mock<SqlDataReader>();
            int readCallCount = 0;
            mockReader.Setup(x => x.ReadAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(() => readCallCount++ == 0); // returns true on first call, then false

            // Set up column ordinal mappings (as used in the repository query).
            mockReader.Setup(x => x.GetOrdinal("ItemId")).Returns(0);
            mockReader.Setup(x => x.GetOrdinal("ItemName")).Returns(1);
            mockReader.Setup(x => x.GetOrdinal("Price")).Returns(2);
            mockReader.Setup(x => x.GetOrdinal("Description")).Returns(3);
            mockReader.Setup(x => x.GetOrdinal("IsListed")).Returns(4);

            // Provide fake data values.
            mockReader.Setup(x => x.GetInt32(0)).Returns(10);
            mockReader.Setup(x => x.GetString(1)).Returns("Test Item");
            mockReader.Setup(x => x.GetDouble(2)).Returns(19.99);
            mockReader.Setup(x => x.GetString(3)).Returns("Item Description");
            mockReader.Setup(x => x.GetBoolean(4)).Returns(false);

            // Arrange a fake command that returns our fake reader.
            mockCommand = new Mock<SqlCommand>();
            mockCommand.Setup(x => x.ExecuteReaderAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(mockReader.Object);

            // Have the fake connection return our fake command.
            mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

            // Act: Call the method under test.
            List<Item> items = await repository.GetItemsFromInventoryAsync(game);

            // Assert: Verify that the returned list contains the expected item.
            Assert.IsNotNull(items, "Returned item list should not be null");
            Assert.AreEqual(1, items.Count, "There should be one item returned");
            var item = items[0];
            Assert.AreEqual(10, item.ItemId, "ItemId should match the fake value");
            Assert.AreEqual("Test Item", item.ItemName, "ItemName should match the fake value");
            Assert.AreEqual(19.99f, item.Price, "Price should match the fake value");
            Assert.AreEqual("Item Description", item.Description, "Description should match the fake value");
            Assert.IsFalse(item.IsListed, "IsListed should be false as per fake data");

            // Verify that the connector’s Open and Close methods were called exactly once.
            mockDbConnector.Verify(x => x.OpenConnectionAsync(), Times.Once);
            mockDbConnector.Verify(x => x.CloseConnection(), Times.Once);
        }

        [Test]
        public async Task AddItemToInventoryAsync_ValidParameters_ExecutesInsertCommand()
        {
            // Arrange: Create dummy objects for Game, Item, and User.
            var game = new Game("Test Game", 9.99f, "RPG", "Game Description");
            game.SetGameId(101);
            var item = new Item("New Item", game, 5.99f, "Item Desc");
            item.SetItemId(20);
            var user = new User("newUser");
            user.SetUserId(200);

            // Arrange a fake command that simulates a successful non-query execution.
            mockCommand = new Mock<SqlCommand>();
            mockCommand.Setup(x => x.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(1);

            // Configure the connection to return our fake command.
            mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

            // Act: Execute the method.
            await repository.AddItemToInventoryAsync(game, item, user);

            // Assert: Confirm that the non-query command was executed and connection methods were invoked.
            mockCommand.Verify(x => x.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()), Times.Once);
            mockDbConnector.Verify(x => x.OpenConnectionAsync(), Times.Once);
            mockDbConnector.Verify(x => x.CloseConnection(), Times.Once);
        }

        [Test]
        public async Task SellItemAsync_ValidItem_CommitsTransactionAndReturnsTrue()
        {
            // Arrange: Create a Game and Item for selling.
            var game = new Game("Sell Game", 15.99f, "Action", "Some Description");
            game.SetGameId(300);
            var item = new Item("Sellable Item", game, 9.99f, "To Sell");
            item.SetItemId(30);

            // Arrange a fake transaction from the connection.
            var mockTransaction = new Mock<SqlTransaction>();
            mockConnection.Setup(x => x.BeginTransaction()).Returns(mockTransaction.Object);

            // Arrange a fake command for the update statement.
            mockCommand = new Mock<SqlCommand>();
            mockCommand.Setup(x => x.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(1);
            // Set the connection to return our fake command.
            mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

            // Act: Call SellItemAsync.
            bool result = await repository.SellItemAsync(item);

            // Assert: The method should return true and the transaction should be committed.
            Assert.IsTrue(result, "SellItemAsync should return true on a successful operation");
            mockDbConnector.Verify(x => x.OpenConnectionAsync(), Times.Once);
            mockDbConnector.Verify(x => x.CloseConnection(), Times.Once);
            mockCommand.Verify(x => x.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()), Times.Once);
            mockTransaction.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        // Additional tests for RemoveItemFromInventoryAsync, GetUserInventoryAsync, GetAllItemsFromInventoryAsync, etc.
        // would follow a similar arrange-act-assert pattern.
    }
}
