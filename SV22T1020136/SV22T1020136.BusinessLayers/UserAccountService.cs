using SV22T1020136.DataLayers.Interfaces;
using SV22T1020136.DataLayers.SQLServer;
using SV22T1020136.Models.Security;

namespace SV22T1020136.BusinessLayers
{
    public static class UserAccountService
    {
        private static readonly IUserAccountRepository userAccountDB;

        static UserAccountService()
        {
            userAccountDB = new UserAccountRepository(Configuration.ConnectionString);
        }

        public static async Task<UserAccount?> Authorize(string userName, string password)
        {
            return await userAccountDB.AuthorizeAsync(userName, password);
        }

        public static async Task<bool> ChangePassword(string userName, string password)
        {
            return await userAccountDB.ChangePasswordAsync(userName, password);
        }
    }
}