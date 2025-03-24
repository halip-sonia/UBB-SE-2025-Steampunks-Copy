-- Create Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'SteampunksDB')
BEGIN
    CREATE DATABASE SteampunksDB;
END
GO

USE SteampunksDB;
GO

-- Create Users table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        UserId INT PRIMARY KEY IDENTITY(1,1),
        Username NVARCHAR(50) NOT NULL UNIQUE,
        WalletBalance FLOAT NOT NULL DEFAULT 0,
        PointBalance FLOAT NOT NULL DEFAULT 0,
        IsDeveloper BIT NOT NULL DEFAULT 0
    );
END
GO

-- Create Games table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Games')
BEGIN
    CREATE TABLE Games (
        GameId INT PRIMARY KEY IDENTITY(1,1),
        Title NVARCHAR(100) NOT NULL,
        Price FLOAT NOT NULL,
        Genre NVARCHAR(50) NOT NULL,
        Description NVARCHAR(MAX),
        Status NVARCHAR(20) NOT NULL,
        RecommendedSpecs FLOAT,
        MinimumSpecs FLOAT
    );
END
GO

-- Create Items table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Items')
BEGIN
    CREATE TABLE Items (
        ItemId INT PRIMARY KEY IDENTITY(1,1),
        ItemName NVARCHAR(100) NOT NULL,
        CorrespondingGameId INT FOREIGN KEY REFERENCES Games(GameId),
        Price FLOAT NOT NULL,
        Description NVARCHAR(MAX),
        IsListed BIT NOT NULL DEFAULT 0
    );
END
GO

-- Create Reviews table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Reviews')
BEGIN
    CREATE TABLE Reviews (
        ReviewId INT PRIMARY KEY IDENTITY(1,1),
        ReviewerId INT FOREIGN KEY REFERENCES Users(UserId),
        GameId INT FOREIGN KEY REFERENCES Games(GameId),
        Content NVARCHAR(MAX),
        Rating INT NOT NULL CHECK (Rating BETWEEN 1 AND 5),
        ReviewDate DATETIME NOT NULL DEFAULT GETUTCDATE()
    );
END
GO

-- Create GameTrades table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'GameTrades')
BEGIN
    CREATE TABLE GameTrades (
        TradeId INT PRIMARY KEY IDENTITY(1,1),
        SourceUserId INT FOREIGN KEY REFERENCES Users(UserId),
        DestinationUserId INT FOREIGN KEY REFERENCES Users(UserId),
        GameId INT FOREIGN KEY REFERENCES Games(GameId),
        TradeDate DATETIME NOT NULL DEFAULT GETUTCDATE(),
        TradeDescription NVARCHAR(MAX),
        AcceptedBySourceUser BIT NOT NULL DEFAULT 0,
        AcceptedByDestinationUser BIT NOT NULL DEFAULT 0,
        TradeStatus NVARCHAR(20) NOT NULL DEFAULT 'Pending'
    );
END
GO

-- Create ItemTrades table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ItemTrades')
BEGIN
    CREATE TABLE ItemTrades (
        TradeId INT PRIMARY KEY IDENTITY(1,1),
        SourceUserId INT FOREIGN KEY REFERENCES Users(UserId),
        DestinationUserId INT FOREIGN KEY REFERENCES Users(UserId),
        GameOfTradeId INT FOREIGN KEY REFERENCES Games(GameId),
        TradeDate DATETIME NOT NULL DEFAULT GETUTCDATE(),
        TradeDescription NVARCHAR(MAX),
        TradeStatus NVARCHAR(20) NOT NULL DEFAULT 'Pending',
        AcceptedBySourceUser BIT NOT NULL DEFAULT 0,
        AcceptedByDestinationUser BIT NOT NULL DEFAULT 0
    );
END
GO

-- Create ItemTradeDetails table for many-to-many relationship between ItemTrades and Items
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ItemTradeDetails')
BEGIN
    CREATE TABLE ItemTradeDetails (
        TradeId INT FOREIGN KEY REFERENCES ItemTrades(TradeId),
        ItemId INT FOREIGN KEY REFERENCES Items(ItemId),
        IsSourceUserItem BIT NOT NULL, -- True if item is from source user, False if from destination user
        PRIMARY KEY (TradeId, ItemId)
    );
END
GO

-- Create stored procedure for user registration
IF NOT EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_RegisterUser')
BEGIN
    EXEC('
    CREATE PROCEDURE sp_RegisterUser
        @Username NVARCHAR(50),
        @IsDeveloper BIT = 0
    AS
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = @Username)
        BEGIN
            INSERT INTO Users (Username, IsDeveloper)
            VALUES (@Username, @IsDeveloper);
            SELECT SCOPE_IDENTITY() AS UserId;
        END
        ELSE
            THROW 50000, ''Username already exists'', 1;
    END
    ');
END
GO 