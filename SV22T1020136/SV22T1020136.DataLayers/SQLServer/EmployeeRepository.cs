using Dapper;
using SV22T1020136.DataLayers.Interfaces;
using SV22T1020136.Models.Common;
using SV22T1020136.Models.HR;

namespace SV22T1020136.DataLayers.SQLServer
{
    public class EmployeeRepository : BaseSQLDAL, IEmployeeRepository, IGenericRepository<Employee>
    {
        public EmployeeRepository(string connectionString) : base(connectionString) { }

        private const string SelectColumns =
            "EmployeeID, FullName, BirthDate, Address, Phone, Email, Photo, IsWorking";

        public async Task<int> AddAsync(Employee data)
        {
            const string sql = @"
INSERT INTO Employees (FullName, BirthDate, Address, Phone, Email, Password, Photo, IsWorking, RoleNames)
VALUES (@FullName, @BirthDate, @Address, @Phone, @Email, @Password, @Photo, @IsWorking, @RoleNames);
SELECT CAST(SCOPE_IDENTITY() as int);";

            var dp = new DynamicParameters();
            dp.Add("FullName", data.FullName);
            dp.Add("BirthDate", data.BirthDate);
            dp.Add("Address", (object?)data.Address ?? "");
            dp.Add("Phone", (object?)data.Phone ?? "");
            dp.Add("Email", data.Email);
            // Nhiều script DB đặt Password / RoleNames NOT NULL — dùng chuỗi rỗng thay vì NULL
            dp.Add("Password", "");
            dp.Add("Photo", string.IsNullOrWhiteSpace(data.Photo) ? "nophoto.png" : data.Photo);
            dp.Add("IsWorking", data.IsWorking);
            dp.Add("RoleNames", "");

            using var cn = GetConnection();
            await cn.OpenAsync();
            return await cn.QuerySingleAsync<int>(sql, dp);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = "DELETE FROM Employees WHERE EmployeeID = @id";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var affected = await cn.ExecuteAsync(sql, new { id });
            return affected > 0;
        }

        public async Task<Employee?> GetAsync(int id)
        {
            var sql = $"SELECT {SelectColumns} FROM Employees WHERE EmployeeID = @id";
            using var cn = GetConnection();
            await cn.OpenAsync();
            return await cn.QueryFirstOrDefaultAsync<Employee>(sql, new { id });
        }

        public async Task<PagedResult<Employee>> ListAsync(PaginationSearchInput input)
        {
            var whereClauses = new List<string>();
            var parameters = new DynamicParameters();
            if (!string.IsNullOrWhiteSpace(input.SearchValue))
            {
                whereClauses.Add(
                    "(FullName LIKE @q OR Phone LIKE @q OR Email LIKE @q OR Address LIKE @q OR RoleNames LIKE @q)");
                parameters.Add("q", "%" + input.SearchValue + "%");
            }

            var where = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : string.Empty;

            string sql;
            if (input.PageSize == 0)
            {
                sql = $"SELECT {SelectColumns} FROM Employees {where} ORDER BY EmployeeID";
            }
            else
            {
                sql =
                    $"SELECT {SelectColumns} FROM Employees {where} ORDER BY EmployeeID OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                parameters.Add("Offset", input.Offset);
                parameters.Add("PageSize", input.PageSize);
            }

            using var cn = GetConnection();
            await cn.OpenAsync();
            var items = (await cn.QueryAsync<Employee>(sql, parameters)).ToList();

            var countSql = $"SELECT COUNT(*) FROM Employees {where}";
            var rowCount = await cn.ExecuteScalarAsync<int>(countSql, parameters);

            return new PagedResult<Employee>
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = items
            };
        }

        public async Task<bool> UpdateAsync(Employee data)
        {
            // Không cập nhật Password / RoleNames ở đây (giữ nguyên trong DB như EmployeeDAL.Update)
            const string sql = @"
UPDATE Employees
SET FullName = @FullName,
    BirthDate = @BirthDate,
    Address = @Address,
    Phone = @Phone,
    Email = @Email,
    Photo = @Photo,
    IsWorking = @IsWorking
WHERE EmployeeID = @EmployeeID";

            var dp = new DynamicParameters();
            dp.Add("EmployeeID", data.EmployeeID);
            dp.Add("FullName", data.FullName);
            dp.Add("BirthDate", data.BirthDate);
            dp.Add("Address", (object?)data.Address ?? "");
            dp.Add("Phone", (object?)data.Phone ?? "");
            dp.Add("Email", data.Email);
            dp.Add("Photo", string.IsNullOrWhiteSpace(data.Photo) ? "nophoto.png" : data.Photo);
            dp.Add("IsWorking", data.IsWorking);

            using var cn = GetConnection();
            await cn.OpenAsync();
            var affected = await cn.ExecuteAsync(sql, dp);
            return affected > 0;
        }

        public async Task<bool> ValidateEmailAsync(string email, int id = 0)
        {
            if (id == 0)
            {
                const string sql = "SELECT CASE WHEN EXISTS(SELECT 1 FROM Employees WHERE Email = @email) THEN 1 ELSE 0 END";
                using var cn = GetConnection();
                await cn.OpenAsync();
                var exists = await cn.ExecuteScalarAsync<int>(sql, new { email });
                return exists == 0;
            }

            {
                const string sql =
                    "SELECT CASE WHEN EXISTS(SELECT 1 FROM Employees WHERE Email = @email AND EmployeeID <> @id) THEN 1 ELSE 0 END";
                using var cn = GetConnection();
                await cn.OpenAsync();
                var exists = await cn.ExecuteScalarAsync<int>(sql, new { email, id });
                return exists == 0;
            }
        }

        public async Task<bool> IsUsedAsync(int id)
        {
            const string sql = "SELECT CASE WHEN EXISTS(SELECT 1 FROM Orders WHERE EmployeeID = @id) THEN 1 ELSE 0 END";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var used = await cn.ExecuteScalarAsync<int>(sql, new { id });
            return used == 1;
        }
    }
}
