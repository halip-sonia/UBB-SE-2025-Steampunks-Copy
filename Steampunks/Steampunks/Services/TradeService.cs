// <copyright file="TradeService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Steampunks.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Steampunks.DataLink;
    using Steampunks.Domain.Entities;

    /// <summary>
    /// Service for trading operations.
    /// </summary>
    public class TradeService : ITradeService
    {
        private readonly DatabaseConnector databaseConnector;

        /// <summary>
        /// Initializes a new instance of the <see cref="TradeService"/> class.
        /// </summary>
        /// <param name="databaseConnector">The connector to the database.</param>
        public TradeService(DatabaseConnector databaseConnector)
        {
            this.databaseConnector = databaseConnector;
        }

        /// <inheritdoc/>
        public async Task<List<ItemTrade>> GetActiveTradesAsync(int userId)
        {
            return await Task.Run(() => this.databaseConnector.GetActiveItemTrades(userId));
        }

        /// <inheritdoc/>
        public async Task<List<ItemTrade>> GetTradeHistoryAsync(int userId)
        {
            return await Task.Run(() => this.databaseConnector.GetItemTradeHistory(userId));
        }

        /// <inheritdoc/>
        public async Task CreateTradeAsync(ItemTrade trade)
        {
            await Task.Run(() =>
            {
                try
                {
                    this.databaseConnector.CreateItemTrade(trade);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error creating trade: {ex.Message}");
                    throw;
                }
            });
        }

        /// <inheritdoc/>
        public async Task UpdateTradeAsync(ItemTrade trade)
        {
            await Task.Run(() =>
            {
                try
                {
                    this.databaseConnector.UpdateItemTrade(trade);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error updating trade: {ex.Message}");
                    throw;
                }
            });
        }

        /// <inheritdoc/>
        public async Task AcceptTradeAsync(ItemTrade trade, bool isSourceUser)
        {
            await Task.Run(() =>
            {
                try
                {
                    if (isSourceUser)
                    {
                        trade.AcceptBySourceUser();
                    }
                    else
                    {
                        trade.AcceptByDestinationUser();
                    }

                    this.databaseConnector.UpdateItemTrade(trade);

                    // If both users have accepted, complete the trade
                    if (trade.AcceptedBySourceUser && trade.AcceptedByDestinationUser)
                    {
                        this.CompleteTrade(trade);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error accepting trade: {ex.Message}");
                    throw;
                }
            });
        }

        /// <inheritdoc/>
        public async Task DeclineTradeAsync(ItemTrade trade)
        {
            await Task.Run(() =>
            {
                try
                {
                    trade.Decline();
                    this.databaseConnector.UpdateItemTrade(trade);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error declining trade: {ex.Message}");
                    throw;
                }
            });
        }

        private void CompleteTrade(ItemTrade trade)
        {
            try
            {
                // Transfer source user items to destination user
                foreach (var item in trade.SourceUserItems)
                {
                    this.databaseConnector.TransferItem(item.ItemId, trade.SourceUser.UserId, trade.DestinationUser.UserId);
                }

                // Transfer destination user items to source user
                foreach (var item in trade.DestinationUserItems)
                {
                    this.databaseConnector.TransferItem(item.ItemId, trade.DestinationUser.UserId, trade.SourceUser.UserId);
                }

                trade.Complete();
                this.databaseConnector.UpdateItemTrade(trade);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error completing trade: {ex.Message}");
                throw;
            }
        }
    }
}