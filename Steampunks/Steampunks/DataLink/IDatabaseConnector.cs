using Microsoft.Data.SqlClient;
using Steampunks.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steampunks.DataLink
{
    public interface IDatabaseConnector
    {
        SqlConnection GetConnection();
        Task OpenConnectionAsync();
        void CloseConnection();
        string GetItemImagePath(Item item);
        User GetCurrentUser();
    }

}
