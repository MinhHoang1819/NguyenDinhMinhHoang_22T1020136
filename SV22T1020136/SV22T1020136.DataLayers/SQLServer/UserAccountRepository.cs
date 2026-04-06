using Dapper;
using SV22T1020136.DataLayers.Interfaces;
using SV22T1020136.Models.Security;
using SV22T1020136.Models.Common;

namespace SV22T1020136.DataLayers.SQLServer
{
    public class UserAccountRepository : BaseSQLDAL, IUserAccountRepository
    {
        public UserAccountRepository(string connectionString) : base(connectionString) { }

        public async Task<UserAccount?> AuthorizeAsync(string userName, string password)
        {
            const string sql = "SELECT UserName, FullName, Email FROM UserAccounts WHERE UserName = @userName AND PasswordHash = @password";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var item = await cn.QueryFirstOrDefaultAsync<UserAccount>(sql, new { userName, password });
            return item;
        }

        public async Task<bool> ChangePasswordAsync(string userName, string password)
        {
            const string sql = "UPDATE UserAccounts SET PasswordHash = @password WHERE UserName = @userName";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var affected = await cn.ExecuteAsync(sql, new { userName, password });
            return affected > 0;
        }

        // Implement basic IUserAccountRepository members as needed. Placeholder implementations follow.

        public Task<int> AddAsync(object data) => Task.FromResult(0);
        public Task<bool> DeleteAsync(int id) => Task.FromResult(false);
        public Task<object?> GetAsync(int id) => Task.FromResult<object?>(null);
        public Task<List<object>> ListAsync(PaginationSearchInput input) => Task.FromResult(new List<object>());
        public Task<bool> UpdateAsync(object data) => Task.FromResult(false);
        public Task<bool> IsUsed(int id) => Task.FromResult(false);
    }
}
