// <copyright file="IDatabaseConnector.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Steampunks.DataLink
{
    using System.Threading.Tasks;
    using Microsoft.Data.SqlClient;
    using Steampunks.Domain.Entities;

    public interface IDatabaseConnector
    {
        SqlConnection GetConnection();

        Task OpenConnectionAsync();

        void CloseConnection();

        string GetItemImagePath(Item item);

        public SqlConnection GetNewConnection();

        User? GetCurrentUser();
    }
}
