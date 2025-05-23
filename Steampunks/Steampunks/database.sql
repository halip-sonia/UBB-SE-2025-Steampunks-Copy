USE [master]
GO
/****** Object:  Database [SteampunksDB]    Script Date: 3/28/2025 12:55:08 PM ******/
CREATE DATABASE [SteampunksDB]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'SteampunksDB', FILENAME = N'D:\SE-Testing\Steampunks\DatabaseStuff.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'SteampunksDB_log', FILENAME = N'D:\SE-Testing\Steampunks\DatabaseStuff.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [SteampunksDB] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [SteampunksDB].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [SteampunksDB] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [SteampunksDB] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [SteampunksDB] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [SteampunksDB] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [SteampunksDB] SET ARITHABORT OFF 
GO
ALTER DATABASE [SteampunksDB] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [SteampunksDB] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [SteampunksDB] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [SteampunksDB] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [SteampunksDB] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [SteampunksDB] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [SteampunksDB] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [SteampunksDB] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [SteampunksDB] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [SteampunksDB] SET  ENABLE_BROKER 
GO
ALTER DATABASE [SteampunksDB] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [SteampunksDB] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [SteampunksDB] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [SteampunksDB] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [SteampunksDB] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [SteampunksDB] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [SteampunksDB] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [SteampunksDB] SET RECOVERY FULL 
GO
ALTER DATABASE [SteampunksDB] SET  MULTI_USER 
GO
ALTER DATABASE [SteampunksDB] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [SteampunksDB] SET DB_CHAINING OFF 
GO
ALTER DATABASE [SteampunksDB] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [SteampunksDB] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [SteampunksDB] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [SteampunksDB] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
EXEC sys.sp_db_vardecimal_storage_format N'SteampunksDB', N'ON'
GO
ALTER DATABASE [SteampunksDB] SET QUERY_STORE = ON
GO
ALTER DATABASE [SteampunksDB] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200)
GO
USE [SteampunksDB]
GO
/****** Object:  Table [dbo].[Games]    Script Date: 3/28/2025 12:55:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Games](
	[GameId] [int] IDENTITY(1,1) NOT NULL,
	[Title] [nvarchar](100) NOT NULL,
	[Price] [float] NOT NULL,
	[Genre] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[Status] [nvarchar](20) NOT NULL,
	[RecommendedSpecs] [float] NULL,
	[MinimumSpecs] [float] NULL,
PRIMARY KEY CLUSTERED 
(
	[GameId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[GameTrades]    Script Date: 3/28/2025 12:55:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GameTrades](
	[TradeId] [int] IDENTITY(1,1) NOT NULL,
	[SourceUserId] [int] NULL,
	[DestinationUserId] [int] NULL,
	[GameId] [int] NULL,
	[TradeDate] [datetime] NOT NULL,
	[TradeDescription] [nvarchar](max) NULL,
	[AcceptedBySourceUser] [bit] NOT NULL,
	[AcceptedByDestinationUser] [bit] NOT NULL,
	[TradeStatus] [nvarchar](20) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[TradeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Items]    Script Date: 3/28/2025 12:55:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Items](
	[ItemId] [int] IDENTITY(1,1) NOT NULL,
	[ItemName] [nvarchar](100) NOT NULL,
	[CorrespondingGameId] [int] NULL,
	[Price] [float] NOT NULL,
	[Description] [nvarchar](max) NULL,
	[IsListed] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ItemId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ItemTradeDetails]    Script Date: 3/28/2025 12:55:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ItemTradeDetails](
	[TradeId] [int] NOT NULL,
	[ItemId] [int] NOT NULL,
	[IsSourceUserItem] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[TradeId] ASC,
	[ItemId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ItemTrades]    Script Date: 3/28/2025 12:55:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ItemTrades](
	[TradeId] [int] IDENTITY(1,1) NOT NULL,
	[SourceUserId] [int] NULL,
	[DestinationUserId] [int] NULL,
	[GameOfTradeId] [int] NULL,
	[TradeDate] [datetime] NOT NULL,
	[TradeDescription] [nvarchar](max) NULL,
	[TradeStatus] [nvarchar](20) NOT NULL,
	[AcceptedBySourceUser] [bit] NOT NULL,
	[AcceptedByDestinationUser] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[TradeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Reviews]    Script Date: 3/28/2025 12:55:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Reviews](
	[ReviewId] [int] IDENTITY(1,1) NOT NULL,
	[ReviewerId] [int] NULL,
	[GameId] [int] NULL,
	[Content] [nvarchar](max) NULL,
	[Rating] [int] NOT NULL,
	[ReviewDate] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ReviewId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserInventory]    Script Date: 3/28/2025 12:55:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserInventory](
	[UserId] [int] NOT NULL,
	[ItemId] [int] NOT NULL,
	[GameId] [int] NOT NULL,
	[AcquiredDate] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[ItemId] ASC,
	[GameId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 3/28/2025 12:55:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[UserId] [int] IDENTITY(1,1) NOT NULL,
	[Username] [nvarchar](50) NOT NULL,
	[WalletBalance] [float] NOT NULL,
	[PointBalance] [float] NOT NULL,
	[IsDeveloper] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Username] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[GameTrades] ADD  DEFAULT (getutcdate()) FOR [TradeDate]
GO
ALTER TABLE [dbo].[GameTrades] ADD  DEFAULT ((0)) FOR [AcceptedBySourceUser]
GO
ALTER TABLE [dbo].[GameTrades] ADD  DEFAULT ((0)) FOR [AcceptedByDestinationUser]
GO
ALTER TABLE [dbo].[GameTrades] ADD  DEFAULT ('Pending') FOR [TradeStatus]
GO
ALTER TABLE [dbo].[Items] ADD  DEFAULT ((0)) FOR [IsListed]
GO
ALTER TABLE [dbo].[ItemTrades] ADD  DEFAULT (getutcdate()) FOR [TradeDate]
GO
ALTER TABLE [dbo].[ItemTrades] ADD  DEFAULT ('Pending') FOR [TradeStatus]
GO
ALTER TABLE [dbo].[ItemTrades] ADD  DEFAULT ((0)) FOR [AcceptedBySourceUser]
GO
ALTER TABLE [dbo].[ItemTrades] ADD  DEFAULT ((0)) FOR [AcceptedByDestinationUser]
GO
ALTER TABLE [dbo].[Reviews] ADD  DEFAULT (getutcdate()) FOR [ReviewDate]
GO
ALTER TABLE [dbo].[UserInventory] ADD  DEFAULT (getutcdate()) FOR [AcquiredDate]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT ((0)) FOR [WalletBalance]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT ((0)) FOR [PointBalance]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT ((0)) FOR [IsDeveloper]
GO
ALTER TABLE [dbo].[GameTrades]  WITH CHECK ADD FOREIGN KEY([DestinationUserId])
REFERENCES [dbo].[Users] ([UserId])
GO
ALTER TABLE [dbo].[GameTrades]  WITH CHECK ADD FOREIGN KEY([GameId])
REFERENCES [dbo].[Games] ([GameId])
GO
ALTER TABLE [dbo].[GameTrades]  WITH CHECK ADD FOREIGN KEY([SourceUserId])
REFERENCES [dbo].[Users] ([UserId])
GO
ALTER TABLE [dbo].[Items]  WITH CHECK ADD FOREIGN KEY([CorrespondingGameId])
REFERENCES [dbo].[Games] ([GameId])
GO
ALTER TABLE [dbo].[ItemTradeDetails]  WITH CHECK ADD FOREIGN KEY([ItemId])
REFERENCES [dbo].[Items] ([ItemId])
GO
ALTER TABLE [dbo].[ItemTradeDetails]  WITH CHECK ADD FOREIGN KEY([TradeId])
REFERENCES [dbo].[ItemTrades] ([TradeId])
GO
ALTER TABLE [dbo].[ItemTrades]  WITH CHECK ADD FOREIGN KEY([DestinationUserId])
REFERENCES [dbo].[Users] ([UserId])
GO
ALTER TABLE [dbo].[ItemTrades]  WITH CHECK ADD FOREIGN KEY([GameOfTradeId])
REFERENCES [dbo].[Games] ([GameId])
GO
ALTER TABLE [dbo].[ItemTrades]  WITH CHECK ADD FOREIGN KEY([SourceUserId])
REFERENCES [dbo].[Users] ([UserId])
GO
ALTER TABLE [dbo].[Reviews]  WITH CHECK ADD FOREIGN KEY([GameId])
REFERENCES [dbo].[Games] ([GameId])
GO
ALTER TABLE [dbo].[Reviews]  WITH CHECK ADD FOREIGN KEY([ReviewerId])
REFERENCES [dbo].[Users] ([UserId])
GO
ALTER TABLE [dbo].[UserInventory]  WITH CHECK ADD FOREIGN KEY([GameId])
REFERENCES [dbo].[Games] ([GameId])
GO
ALTER TABLE [dbo].[UserInventory]  WITH CHECK ADD FOREIGN KEY([ItemId])
REFERENCES [dbo].[Items] ([ItemId])
GO
ALTER TABLE [dbo].[UserInventory]  WITH CHECK ADD FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([UserId])
GO
ALTER TABLE [dbo].[Reviews]  WITH CHECK ADD CHECK  (([Rating]>=(1) AND [Rating]<=(5)))
GO
/****** Object:  StoredProcedure [dbo].[sp_AddGameWithItems]    Script Date: 3/28/2025 12:55:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

    CREATE PROCEDURE [dbo].[sp_AddGameWithItems]
        @GameTitle NVARCHAR(100),
        @GamePrice FLOAT,
        @Genre NVARCHAR(50),
        @Description NVARCHAR(MAX),
        @ItemName NVARCHAR(100),
        @ItemPrice FLOAT,
        @ItemDescription NVARCHAR(MAX)
    AS
    BEGIN
        DECLARE @GameId INT;
        
        -- Insert the game
        INSERT INTO Games (Title, Price, Genre, Description, Status)
        VALUES (@GameTitle, @GamePrice, @Genre, @Description, 'Available');
        
        SET @GameId = SCOPE_IDENTITY();
        
        -- Insert the item
        INSERT INTO Items (ItemName, CorrespondingGameId, Price, Description, IsListed)
        VALUES (@ItemName, @GameId, @ItemPrice, @ItemDescription, 0);
        
        SELECT @GameId AS GameId;
    END
    
GO
/****** Object:  StoredProcedure [dbo].[sp_AddToUserInventory]    Script Date: 3/28/2025 12:55:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

    CREATE PROCEDURE [dbo].[sp_AddToUserInventory]
        @UserId INT,
        @ItemId INT,
        @GameId INT
    AS
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM UserInventory WHERE UserId = @UserId AND ItemId = @ItemId AND GameId = @GameId)
        BEGIN
            INSERT INTO UserInventory (UserId, ItemId, GameId)
            VALUES (@UserId, @ItemId, @GameId);
        END
    END
    
GO
/****** Object:  StoredProcedure [dbo].[sp_RegisterUser]    Script Date: 3/28/2025 12:55:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

    CREATE PROCEDURE [dbo].[sp_RegisterUser]
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
            THROW 50000, 'Username already exists', 1;
    END
    
GO
/****** Object:  StoredProcedure [dbo].[sp_RemoveFromUserInventory]    Script Date: 3/28/2025 12:55:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

    CREATE PROCEDURE [dbo].[sp_RemoveFromUserInventory]
        @UserId INT,
        @ItemId INT,
        @GameId INT
    AS
    BEGIN
        DELETE FROM UserInventory 
        WHERE UserId = @UserId AND ItemId = @ItemId AND GameId = @GameId;
    END
    
GO
USE [master]
GO
ALTER DATABASE [SteampunksDB] SET  READ_WRITE 
GO
