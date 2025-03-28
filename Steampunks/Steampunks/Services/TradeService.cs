using Steampunks.DataLink;
using Steampunks.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Steampunks.Services
{
    public class TradeService
    {
        private readonly DatabaseConnector _databaseConnector;

        public TradeService(DatabaseConnector databaseConnector)
        {
            _databaseConnector = databaseConnector;
        }

        public async Task<List<ItemTrade>> GetActiveTradesAsync(int userId)
        {
            return await Task.Run(() => _databaseConnector.GetActiveItemTrades(userId));
        }

        public async Task<List<ItemTrade>> GetTradeHistoryAsync(int userId)
        {
            return await Task.Run(() => _databaseConnector.GetItemTradeHistory(userId));
        }

        public async Task CreateTradeAsync(ItemTrade trade)
        {
            await Task.Run(() =>
            {
                try
                {
                    _databaseConnector.CreateItemTrade(trade);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error creating trade: {ex.Message}");
                    throw;
                }
            });
        }

        public async Task UpdateTradeAsync(ItemTrade trade)
        {
            await Task.Run(() =>
            {
                try
                {
                    _databaseConnector.UpdateItemTrade(trade);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error updating trade: {ex.Message}");
                    throw;
                }
            });
        }

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

                    _databaseConnector.UpdateItemTrade(trade);

                    // If both users have accepted, complete the trade
                    if (trade.AcceptedBySourceUser && trade.AcceptedByDestinationUser)
                    {
                        CompleteTrade(trade);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error accepting trade: {ex.Message}");
                    throw;
                }
            });
        }

        public async Task DeclineTradeAsync(ItemTrade trade)
        {
            await Task.Run(() =>
            {
                try
                {
                    trade.Decline();
                    _databaseConnector.UpdateItemTrade(trade);
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
                    _databaseConnector.TransferItem(item.ItemId, trade.SourceUser.UserId, trade.DestinationUser.UserId);
                }

                // Transfer destination user items to source user
                foreach (var item in trade.DestinationUserItems)
                {
                    _databaseConnector.TransferItem(item.ItemId, trade.DestinationUser.UserId, trade.SourceUser.UserId);
                }

                trade.Complete();
                _databaseConnector.UpdateItemTrade(trade);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error completing trade: {ex.Message}");
                throw;
            }
        }
    }
} 