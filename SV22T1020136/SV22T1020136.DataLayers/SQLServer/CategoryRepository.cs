using Dapper;
using SV22T1020136.DataLayers.Interfaces;
using SV22T1020136.Models.Catalog;
using SV22T1020136.Models.Common;

namespace SV22T1020136.DataLayers.SQLServer
{
    public class CategoryRepository : BaseSQLDAL, IGenericRepository<Category>
    {
        public CategoryRepository(string connectionString) : base(connectionString) { }

        public async Task<int> AddAsync(Category data)
        {
            const string sql = @"INSERT INTO Categories (CategoryName, Description)
                                VALUES (@CategoryName, @Description); SELECT CAST(SCOPE_IDENTITY() as int);";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var id = await cn.QuerySingleAsync<int>(sql, data);
            return id;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = "DELETE FROM Categories WHERE CategoryID = @id";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var affected = await cn.ExecuteAsync(sql, new { id });
            return affected > 0;
        }

        public async Task<Category?> GetAsync(int id)
        {
            const string sql = "SELECT CategoryID, CategoryName, Description FROM Categories WHERE CategoryID = @id";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var item = await cn.QueryFirstOrDefaultAsync<Category>(sql, new { id });
            return item;
        }

        public async Task<PagedResult<Category>> ListAsync(PaginationSearchInput input)
        {
            var whereClauses = new List<string>();
            var parameters = new DynamicParameters();
            if (!string.IsNullOrWhiteSpace(input.SearchValue))
            {
                whereClauses.Add("(CategoryName LIKE @q OR Description LIKE @q)");
                parameters.Add("q", "%" + input.SearchValue + "%");
            }
            var where = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : string.Empty;

            var sqlCount = $"SELECT COUNT(*) FROM Categories {where}";

            string sql;
            if (input.PageSize == 0)
            {
                sql = $"SELECT CategoryID, CategoryName, Description FROM Categories {where} ORDER BY CategoryID";
            }
            else
            {
                sql = $"SELECT CategoryID, CategoryName, Description FROM Categories {where} ORDER BY CategoryID OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                parameters.Add("Offset", input.Offset);
                parameters.Add("PageSize", input.PageSize);
            }

            using var cn = GetConnection();
            await cn.OpenAsync();
            var rowCount = await cn.ExecuteScalarAsync<int>(sqlCount, parameters);
            var items = (await cn.QueryAsync<Category>(sql, parameters)).ToList();

            return new PagedResult<Category>
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = items
            };
        }

        public async Task<bool> UpdateAsync(Category data)
        {
            const string sql = @"UPDATE Categories SET CategoryName = @CategoryName, Description = @Description WHERE CategoryID = @CategoryID";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var affected = await cn.ExecuteAsync(sql, data);
            return affected > 0;
        }

        public async Task<bool> IsUsedAsync(int id)
        {
            const string sql = "SELECT CASE WHEN EXISTS(SELECT 1 FROM Products WHERE CategoryID = @id) THEN 1 ELSE 0 END";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var used = await cn.ExecuteScalarAsync<int>(sql, new { id });
            return used == 1;
        }
    }
}
