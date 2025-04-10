using System;

namespace Steampunks.Domain.Entities
{
    public class User
    {
        public int UserId { get; private set; }
        public string Username { get; private set; }
        public float WalletBalance { get; private set; }
        public float PointBalance { get; private set; }
        public bool IsDeveloper { get; private set; }

        private User() { }

        public User(string username)
        {
            Username = username;
            WalletBalance = 0;
            PointBalance = 0;
            IsDeveloper = false;
        }

        public void SetUserId(int id)
        {
            UserId = id;
        }

        public void SetUsername(string username)
        {
            Username = username;
        }

        public float GetWalletBalance()
        {
            return WalletBalance;
        }

        public float GetPointBalance()
        {
            return PointBalance;
        }

        public void SetWalletBalance(float balance)
        {
            WalletBalance = balance;
        }

        public void SetPointBalance(float balance)
        {
            PointBalance = balance;
        }

        public void SetIsDeveloper(bool state)
        {
            IsDeveloper = state;
        }
    }
} 