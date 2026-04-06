using Dapper;
using SV22T1020136.DataLayers.Interfaces;
using SV22T1020136.Models.Partner;
using SV22T1020136.Models.Common;

namespace SV22T1020136.DataLayers.SQLServer
{
    public class ShipperRepository : BaseSQLDAL, IGenericRepository<Shipper>
    {
        public ShipperRepository(string connectionString) : base(connectionString) { }

        public async Task<int> AddAsync(Shipper data)
        {
            const string sql = @"INSERT INTO Shippers (ShipperName, Phone)
VALUES (@ShipperName, @Phone); SELECT CAST(SCOPE_IDENTITY() as int);";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var id = await cn.QuerySingleAsync<int>(sql, data);
            return id;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = "DELETE FROM Shippers WHERE ShipperID = @id";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var affected = await cn.ExecuteAsync(sql, new { id });
            return affected > 0;
        }

        public async Task<Shipper?> GetAsync(int id)
        {
            const string sql = "SELECT ShipperID, ShipperName, Phone FROM Shippers WHERE ShipperID = @id";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var item = await cn.QueryFirstOrDefaultAsync<Shipper>(sql, new { id });
            return item;
        }

        public async Task<PagedResult<Shipper>> ListAsync(PaginationSearchInput input)
        {
            var whereClauses = new List<string>();
            var parameters = new DynamicParameters();
            if (!string.IsNullOrWhiteSpace(input.SearchValue))
            {
                whereClauses.Add("(ShipperName LIKE @q OR Phone LIKE @q)");
                parameters.Add("q", "%" + input.SearchValue + "%");
            }
            var where = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : string.Empty;

            var sqlCount = $"SELECT COUNT(*) FROM Shippers {where}";

            string sql;
            if (input.PageSize == 0)
            {
                sql = $"SELECT ShipperID, ShipperName, Phone FROM Shippers {where} ORDER BY ShipperID";
            }
            else
            {
                sql = $"SELECT ShipperID, ShipperName, Phone FROM Shippers {where} ORDER BY ShipperID OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                parameters.Add("Offset", input.Offset);
                parameters.Add("PageSize", input.PageSize);
            }

            using var cn = GetConnection();
            await cn.OpenAsync();
            var rowCount = await cn.ExecuteScalarAsync<int>(sqlCount, parameters);
            var items = (await cn.QueryAsync<Shipper>(sql, parameters)).ToList();

            return new PagedResult<Shipper>
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = items
            };
        }

        public async Task<bool> UpdateAsync(Shipper data)
        {
            const string sql = @"UPDATE Shippers SET ShipperName = @ShipperName, Phone = @Phone WHERE ShipperID = @ShipperID";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var affected = await cn.ExecuteAsync(sql, data);
            return affected > 0;
        }

        public async Task<bool> IsUsedAsync(int id)
        {
            const string sql = "SELECT CASE WHEN EXISTS(SELECT 1 FROM Orders WHERE ShipperID = @id) THEN 1 ELSE 0 END";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var used = await cn.ExecuteScalarAsync<int>(sql, new { id });
            return used == 1;
        }
    }
}

