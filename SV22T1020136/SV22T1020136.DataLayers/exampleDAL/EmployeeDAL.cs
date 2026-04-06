using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SV22T1020136.Models;

namespace SV22T1020136.DataLayers
{
    public static class EmployeeDAL
    {
        public static List<Employee> List(IConfiguration configuration, out int rowCount, string searchValue = "", int page = 1, int pageSize = 25)
        {
            List<Employee> data = new List<Employee>();
            rowCount = 0;

            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string countSql = @"
                    SELECT COUNT(*) 
                    FROM Employees 
                    WHERE (@SearchValue = '' OR 
                           FullName LIKE '%' + @SearchValue + '%' OR
                           Phone LIKE '%' + @SearchValue + '%' OR
                           Email LIKE '%' + @SearchValue + '%' OR
                           Address LIKE '%' + @SearchValue + '%' OR
                           RoleNames LIKE '%' + @SearchValue + '%')";

                using (var cmd = new SqlCommand(countSql, connection))
                {
                    cmd.Parameters.AddWithValue("@SearchValue", searchValue ?? "");
                    rowCount = Convert.ToInt32(cmd.ExecuteScalar());
                }

                string sql = @"
                    SELECT * FROM (
                        SELECT *, ROW_NUMBER() OVER (ORDER BY EmployeeID) AS RowNumber
                        FROM Employees
                        WHERE (@SearchValue = '' OR 
                               FullName LIKE '%' + @SearchValue + '%' OR
                               Phone LIKE '%' + @SearchValue + '%' OR
                               Email LIKE '%' + @SearchValue + '%' OR
                               Address LIKE '%' + @SearchValue + '%' OR
                               RoleNames LIKE '%' + @SearchValue + '%')
                    ) AS T
                    WHERE RowNumber BETWEEN (@Page - 1) * @PageSize + 1 AND @Page * @PageSize
                    ORDER BY EmployeeID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@SearchValue", searchValue ?? "");
                    cmd.Parameters.AddWithValue("@Page", page);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            data.Add(new Employee
                            {
                                EmployeeID = Convert.ToInt32(reader["EmployeeID"]),
                                FullName = reader["FullName"]?.ToString() ?? "",
                                BirthDate = reader["BirthDate"] != DBNull.Value ? Convert.ToDateTime(reader["BirthDate"]) : null,
                                Address = reader["Address"]?.ToString() ?? "",
                                Phone = reader["Phone"]?.ToString() ?? "",
                                Email = reader["Email"]?.ToString() ?? "",
                                Password = reader["Password"]?.ToString() ?? "",
                                Photo = reader["Photo"]?.ToString() ?? "",
                                IsWorking = Convert.ToBoolean(reader["IsWorking"]),
                                RoleNames = reader["RoleNames"]?.ToString() ?? ""
                            });
                        }
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// Lấy nhân viên đang làm việc theo email (đăng nhập).
        /// </summary>
        public static Employee? GetByEmail(IConfiguration configuration, string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                const string sql = @"
                    SELECT * FROM Employees
                    WHERE LOWER(LTRIM(RTRIM(Email))) = LOWER(LTRIM(RTRIM(@Email)))
                      AND IsWorking = 1";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@Email", email.Trim());

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Employee
                            {
                                EmployeeID = Convert.ToInt32(reader["EmployeeID"]),
                                FullName = reader["FullName"]?.ToString() ?? "",
                                BirthDate = reader["BirthDate"] != DBNull.Value ? Convert.ToDateTime(reader["BirthDate"]) : null,
                                Address = reader["Address"]?.ToString() ?? "",
                                Phone = reader["Phone"]?.ToString() ?? "",
                                Email = reader["Email"]?.ToString() ?? "",
                                Password = reader["Password"]?.ToString() ?? "",
                                Photo = reader["Photo"]?.ToString() ?? "",
                                IsWorking = Convert.ToBoolean(reader["IsWorking"]),
                                RoleNames = reader["RoleNames"]?.ToString() ?? ""
                            };
                        }
                    }
                }
            }

            return null;
        }

        public static Employee? Get(IConfiguration configuration, int employeeID)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = "SELECT * FROM Employees WHERE EmployeeID = @EmployeeID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@EmployeeID", employeeID);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Employee
                            {
                                EmployeeID = Convert.ToInt32(reader["EmployeeID"]),
                                FullName = reader["FullName"]?.ToString() ?? "",
                                BirthDate = reader["BirthDate"] != DBNull.Value ? Convert.ToDateTime(reader["BirthDate"]) : null,
                                Address = reader["Address"]?.ToString() ?? "",
                                Phone = reader["Phone"]?.ToString() ?? "",
                                Email = reader["Email"]?.ToString() ?? "",
                                Password = reader["Password"]?.ToString() ?? "",
                                Photo = reader["Photo"]?.ToString() ?? "",
                                IsWorking = Convert.ToBoolean(reader["IsWorking"]),
                                RoleNames = reader["RoleNames"]?.ToString() ?? ""
                            };
                        }
                    }
                }
            }

            return null;
        }

        public static int Add(IConfiguration configuration, Employee employee)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = @"
                    INSERT INTO Employees (FullName, BirthDate, Address, Phone, Email, Password, Photo, IsWorking, RoleNames)
                    VALUES (@FullName, @BirthDate, @Address, @Phone, @Email, @Password, @Photo, @IsWorking, @RoleNames);
                    SELECT SCOPE_IDENTITY();";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@FullName", employee.FullName);
                    cmd.Parameters.AddWithValue("@BirthDate", employee.BirthDate.HasValue ? (object)employee.BirthDate.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address", employee.Address);
                    cmd.Parameters.AddWithValue("@Phone", employee.Phone);
                    cmd.Parameters.AddWithValue("@Email", employee.Email);
                    // Always provide @Password param (SQL exception can happen if parameter is missing)
                    cmd.Parameters.AddWithValue("@Password", (object?)employee.Password ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Photo", (object?)employee.Photo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsWorking", employee.IsWorking);
                    cmd.Parameters.AddWithValue("@RoleNames", (object?)employee.RoleNames ?? DBNull.Value);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public static bool Update(IConfiguration configuration, Employee employee)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = @"
                    UPDATE Employees 
                    SET FullName = @FullName,
                        BirthDate = @BirthDate,
                        Address = @Address,
                        Phone = @Phone,
                        Email = @Email,
                        Photo = @Photo,
                        IsWorking = @IsWorking,
                        RoleNames = @RoleNames
                    WHERE EmployeeID = @EmployeeID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@EmployeeID", employee.EmployeeID);
                    cmd.Parameters.AddWithValue("@FullName", employee.FullName);
                    cmd.Parameters.AddWithValue("@BirthDate", employee.BirthDate.HasValue ? (object)employee.BirthDate.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address", employee.Address);
                    cmd.Parameters.AddWithValue("@Phone", employee.Phone);
                    cmd.Parameters.AddWithValue("@Email", employee.Email);
                    cmd.Parameters.AddWithValue("@Photo", (object?)employee.Photo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsWorking", employee.IsWorking);
                    cmd.Parameters.AddWithValue("@RoleNames", (object?)employee.RoleNames ?? DBNull.Value);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public static bool Delete(IConfiguration configuration, int employeeID)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = "DELETE FROM Employees WHERE EmployeeID = @EmployeeID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@EmployeeID", employeeID);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public static bool ChangePassword(IConfiguration configuration, int employeeID, string newPassword)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = "UPDATE Employees SET Password = @Password WHERE EmployeeID = @EmployeeID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@EmployeeID", employeeID);
                    cmd.Parameters.AddWithValue("@Password", (object?)newPassword ?? DBNull.Value);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public static bool ChangeRoles(IConfiguration configuration, int employeeID, string roleNames)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = "UPDATE Employees SET RoleNames = @RoleNames WHERE EmployeeID = @EmployeeID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@EmployeeID", employeeID);
                    cmd.Parameters.AddWithValue("@RoleNames", roleNames ?? "");
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}
