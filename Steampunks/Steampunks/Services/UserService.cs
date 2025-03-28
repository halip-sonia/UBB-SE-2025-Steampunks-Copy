using Steampunks.DataLink;
using Steampunks.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Steampunks.Services
{
    public class UserService
    {
        private readonly DatabaseConnector _dbConnector;

        public UserService(DatabaseConnector dbConnector)
        {
            _dbConnector = dbConnector;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await Task.Run(() => _dbConnector.GetAllUsers());
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            return await Task.Run(() => _dbConnector.GetUserById(userId));
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            return await Task.Run(() => _dbConnector.UpdateUser(user));
        }
    }
} 