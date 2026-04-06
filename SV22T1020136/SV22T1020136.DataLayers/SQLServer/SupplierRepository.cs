using Dapper;
using SV22T1020136.DataLayers.Interfaces;
using SV22T1020136.Models.Partner;
using SV22T1020136.Models.Common;

namespace SV22T1020136.DataLayers.SQLServer
{
    public class SupplierRepository : BaseSQLDAL, ISupplierRepository
    {
        public SupplierRepository(string connectionString) : base(connectionString) { }

        public async Task<int> AddAsync(Supplier data)
        {
            const string sql = @"INSERT INTO Suppliers (SupplierName, ContactName, Address, Phone, Email)
VALUES (@SupplierName, @ContactName, @Address, @Phone, @Email); SELECT CAST(SCOPE_IDENTITY() as int);";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var id = await cn.QuerySingleAsync<int>(sql, data);
            return id;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = "DELETE FROM Suppliers WHERE SupplierID = @id";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var affected = await cn.ExecuteAsync(sql, new { id });
            return affected > 0;
        }

        public async Task<Supplier?> GetAsync(int id)
        {
            const string sql = "SELECT SupplierID, SupplierName, ContactName, Address, Phone, Email FROM Suppliers WHERE SupplierID = @id";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var item = await cn.QueryFirstOrDefaultAsync<Supplier>(sql, new { id });
            return item;
        }

        public async Task<PagedResult<Supplier>> ListAsync(PaginationSearchInput input)
        {
            var whereClauses = new List<string>();
            var parameters = new DynamicParameters();
            if (!string.IsNullOrWhiteSpace(input.SearchValue))
            {
                whereClauses.Add("(SupplierName LIKE @q OR ContactName LIKE @q OR Email LIKE @q)");
                parameters.Add("q", "%" + input.SearchValue + "%");
            }
            var where = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : string.Empty;

            var sqlCount = $"SELECT COUNT(*) FROM Suppliers {where}";

            string sql;
            if (input.PageSize == 0)
            {
                sql = $"SELECT SupplierID, SupplierName, ContactName, Address, Phone, Email FROM Suppliers {where} ORDER BY SupplierID";
            }
            else
            {
                sql = $"SELECT SupplierID, SupplierName, ContactName, Address, Phone, Email FROM Suppliers {where} ORDER BY SupplierID OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                parameters.Add("Offset", input.Offset);
                parameters.Add("PageSize", input.PageSize);
            }

            using var cn = GetConnection();
            await cn.OpenAsync();
            var rowCount = await cn.ExecuteScalarAsync<int>(sqlCount, parameters);
            var items = (await cn.QueryAsync<Supplier>(sql, parameters)).ToList();

            return new PagedResult<Supplier>
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = items
            };
        }

        public async Task<bool> UpdateAsync(Supplier data)
        {
            const string sql = @"UPDATE Suppliers SET SupplierName = @SupplierName, ContactName = @ContactName, Address = @Address, Phone = @Phone, Email = @Email WHERE SupplierID = @SupplierID";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var affected = await cn.ExecuteAsync(sql, data);
            return affected > 0;
        }

        public async Task<bool> IsUsed(int id)
        {
            const string sql = "SELECT CASE WHEN EXISTS(SELECT 1 FROM Products WHERE SupplierID = @id) THEN 1 ELSE 0 END";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var used = await cn.ExecuteScalarAsync<int>(sql, new { id });
            return used == 1;
        }
    }
}
