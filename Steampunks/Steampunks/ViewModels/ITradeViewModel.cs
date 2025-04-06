﻿// <copyright file="ITradeViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Steampunks.ViewModels
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using Steampunks.Domain.Entities;

    /// <summary>
    /// Interface for TradeViewModel.
    /// </summary>
    public interface ITradeViewModel
    {
        /// <summary>
        /// Event for property changes.
        /// </summary>
        event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets activeTrades.
        /// </summary>
        ObservableCollection<ItemTrade> ActiveTrades { get; }

        /// <summary>
        /// Gets a list of all available users.
        /// </summary>
        ObservableCollection<User> AvailableUsers { get; }

        /// <summary>
        /// Gets a value indicating whether or not the trade can be accepted or declined.
        /// </summary>
        bool CanAcceptOrDeclineTrade { get; }

        /// <summary>
        /// Gets a value indicating whether or not a trade offer can be sent.
        /// </summary>
        bool CanSendTradeOffer { get; }

        /// <summary>
        /// Gets or sets currentUser.
        /// </summary>
        User? CurrentUser { get; set; }

        /// <summary>
        /// Gets or sets destinationUserItems.
        /// </summary>
        ObservableCollection<Item> DestinationUserItems { get; set; }

        /// <summary>
        /// Gets or sets games.
        /// </summary>
        ObservableCollection<Game> Games { get; set; }

        /// <summary>
        /// Gets or sets selectedDestinationItems.
        /// </summary>
        ObservableCollection<Item> SelectedDestinationItems { get; set; }

        /// <summary>
        /// Gets or sets selectedGame.
        /// </summary>
        Game? SelectedGame { get; set; }

        /// <summary>
        /// Gets or sets selectedSourceItems.
        /// </summary>
        ObservableCollection<Item> SelectedSourceItems { get; set; }

        /// <summary>
        /// Gets or sets selectedTrade.
        /// </summary>
        ItemTrade? SelectedTrade { get; set; }

        /// <summary>
        /// Gets or sets selectedUser.
        /// </summary>
        User? SelectedUser { get; set; }

        /// <summary>
        /// Gets or sets sourceUserItems.
        /// </summary>
        ObservableCollection<Item> SourceUserItems { get; set; }

        /// <summary>
        /// Gets or sets tradeDescription.
        /// </summary>
        string? TradeDescription { get; set; }

        /// <summary>
        /// Gets tradeHistory.
        /// </summary>
        ObservableCollection<ItemTrade> TradeHistory { get; }

        /// <summary>
        /// Gets or sets users.
        /// </summary>
        ObservableCollection<User> Users { get; set; }

        /// <summary>
        /// Accepts the given trade.
        /// </summary>
        /// <param name="trade">The trade to be accepted.</param>
        void AcceptTrade(ItemTrade trade);

        /// <summary>
        /// Moves an item from the destination user items list to the list of selected items.
        /// </summary>
        /// <param name="item">The item in the destination user items list.</param>
        void AddDestinationItem(Item item);

        /// <summary>
        /// Moves an item from the source user items list to the list of selected items.
        /// </summary>
        /// <param name="item">The item in the source user items list.</param>
        void AddSourceItem(Item item);

        /// <summary>
        /// Creates a trade from the current selections.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task CreateTradeOffer();

        /// <summary>
        /// Declines the given trade.
        /// </summary>
        /// <param name="trade">The trade to be declined.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<bool> DeclineTrade(ItemTrade trade);

        /// <summary>
        /// Populates the Games list with data from the database.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task LoadGamesAsync();

        /// <summary>
        /// Populates the Users list with data from the dabase.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task LoadUsersAsync();

        /// <summary>
        /// Moves an item from the list of selected items back to destination user items list.
        /// </summary>
        /// <param name="item">The item in the selected list.</param>
        void RemoveDestinationItem(Item item);

        /// <summary>
        /// Moves an item from the list of selected items back to source user items list.
        /// </summary>
        /// <param name="item">The item in the selected list.</param>
        void RemoveSourceItem(Item item);
    }
}