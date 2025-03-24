using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steampunks.Domain
{
    internal class User
    {
        private static int userId;
        private static string username;
        private static string userEmail;
        private static float userBalance;
        private static int pointBalance;
        private static bool isDeveloper = false;

        // getters and setters
        public static int getSetUserId
        {
            get { return userId; }
            set { userId = value; }
        }

        public static string getSetUsername
        {
            get { return username; }
            set { username = value; }
        }

        public static string getSetUserEmail
        {
            get { return userEmail; }
            set { userEmail = value; }
        }

        public static float getSetUserBalance
        {
            get { return userBalance; }
            set { userBalance = value; }
        }

        public static int getSetPointBalance
        {
            get { return pointBalance; }
            set { pointBalance = value; }
        }

        public static bool getSetIsDeveloper
        {
            get { return isDeveloper; }
            set { isDeveloper = value; }
        }
    }
}
