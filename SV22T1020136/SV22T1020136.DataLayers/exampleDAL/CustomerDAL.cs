using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SV22T1020136.Models;

namespace SV22T1020136.DataLayers
{
    public static class CustomerDAL
    {
        public static List<Customer> List(IConfiguration configuration, out int rowCount, string searchValue = "", int page = 1, int pageSize = 25)
        {
            List<Customer> data = new List<Customer>();
            rowCount = 0;

            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string countSql = @"
                    SELECT COUNT(*) 
                    FROM Customers 
                    WHERE (@SearchValue = '' OR 
                           CustomerName LIKE '%' + @SearchValue + '%' OR
                           ContactName LIKE '%' + @SearchValue + '%' OR
                           Province LIKE '%' + @SearchValue + '%' OR
                           Phone LIKE '%' + @SearchValue + '%' OR
                           Email LIKE '%' + @SearchValue + '%')";

                using (var cmd = new SqlCommand(countSql, connection))
                {
                    cmd.Parameters.AddWithValue("@SearchValue", searchValue ?? "");
                    rowCount = Convert.ToInt32(cmd.ExecuteScalar());
                }

                string sql = @"
                    SELECT * FROM (
                        SELECT *, ROW_NUMBER() OVER (ORDER BY CustomerID) AS RowNumber
                        FROM Customers
                        WHERE (@SearchValue = '' OR 
                               CustomerName LIKE '%' + @SearchValue + '%' OR
                               ContactName LIKE '%' + @SearchValue + '%' OR
                               Province LIKE '%' + @SearchValue + '%' OR
                               Phone LIKE '%' + @SearchValue + '%' OR
                               Email LIKE '%' + @SearchValue + '%')
                    ) AS T
                    WHERE RowNumber BETWEEN (@Page - 1) * @PageSize + 1 AND @Page * @PageSize
                    ORDER BY CustomerID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@SearchValue", searchValue ?? "");
                    cmd.Parameters.AddWithValue("@Page", page);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            data.Add(new Customer
                            {
                                CustomerID = Convert.ToInt32(reader["CustomerID"]),
                                CustomerName = reader["CustomerName"]?.ToString() ?? "",
                                ContactName = reader["ContactName"]?.ToString() ?? "",
                                Province = reader["Province"]?.ToString() ?? "",
                                Address = reader["Address"]?.ToString() ?? "",
                                Phone = reader["Phone"]?.ToString() ?? "",
                                Email = reader["Email"]?.ToString() ?? "",
                                Password = reader["Password"]?.ToString() ?? "",
                                IsLocked = Convert.ToBoolean(reader["IsLocked"])
                            });
                        }
                    }
                }
            }

            return data;
        }

        public static Customer? Get(IConfiguration configuration, int customerID)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = "SELECT * FROM Customers WHERE CustomerID = @CustomerID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@CustomerID", customerID);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Customer
                            {
                                CustomerID = Convert.ToInt32(reader["CustomerID"]),
                                CustomerName = reader["CustomerName"]?.ToString() ?? "",
                                ContactName = reader["ContactName"]?.ToString() ?? "",
                                Province = reader["Province"]?.ToString() ?? "",
                                Address = reader["Address"]?.ToString() ?? "",
                                Phone = reader["Phone"]?.ToString() ?? "",
                                Email = reader["Email"]?.ToString() ?? "",
                                Password = reader["Password"]?.ToString() ?? "",
                                IsLocked = Convert.ToBoolean(reader["IsLocked"])
                            };
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>Tìm khách hàng theo email (đăng nhập cửa hàng).</summary>
        public static Customer? GetByEmail(IConfiguration configuration, string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                const string sql = @"
                    SELECT * FROM Customers
                    WHERE LOWER(LTRIM(RTRIM(Email))) = LOWER(LTRIM(RTRIM(@Email)))";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@Email", email.Trim());

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Customer
                            {
                                CustomerID = Convert.ToInt32(reader["CustomerID"]),
                                CustomerName = reader["CustomerName"]?.ToString() ?? "",
                                ContactName = reader["ContactName"]?.ToString() ?? "",
                                Province = reader["Province"]?.ToString() ?? "",
                                Address = reader["Address"]?.ToString() ?? "",
                                Phone = reader["Phone"]?.ToString() ?? "",
                                Email = reader["Email"]?.ToString() ?? "",
                                Password = reader["Password"]?.ToString() ?? "",
                                IsLocked = Convert.ToBoolean(reader["IsLocked"])
                            };
                        }
                    }
                }
            }

            return null;
        }

        public static int Add(IConfiguration configuration, Customer customer)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = @"
                    INSERT INTO Customers (CustomerName, ContactName, Province, Address, Phone, Email, Password, IsLocked)
                    VALUES (@CustomerName, @ContactName, @Province, @Address, @Phone, @Email, @Password, @IsLocked);
                    SELECT SCOPE_IDENTITY();";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@CustomerName", customer.CustomerName);
                    cmd.Parameters.AddWithValue("@ContactName", customer.ContactName);
                    cmd.Parameters.AddWithValue("@Province", customer.Province);
                    cmd.Parameters.AddWithValue("@Address", customer.Address);
                    cmd.Parameters.AddWithValue("@Phone", customer.Phone);
                    cmd.Parameters.AddWithValue("@Email", customer.Email);
                    // Always provide @Password param (SQL exception can happen if parameter is missing)
                    cmd.Parameters.AddWithValue("@Password", (object?)customer.Password ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsLocked", customer.IsLocked);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public static bool Update(IConfiguration configuration, Customer customer)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = @"
                    UPDATE Customers 
                    SET CustomerName = @CustomerName,
                        ContactName = @ContactName,
                        Province = @Province,
                        Address = @Address,
                        Phone = @Phone,
                        Email = @Email,
                        IsLocked = @IsLocked
                    WHERE CustomerID = @CustomerID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@CustomerID", customer.CustomerID);
                    cmd.Parameters.AddWithValue("@CustomerName", customer.CustomerName);
                    cmd.Parameters.AddWithValue("@ContactName", customer.ContactName);
                    cmd.Parameters.AddWithValue("@Province", customer.Province);
                    cmd.Parameters.AddWithValue("@Address", customer.Address);
                    cmd.Parameters.AddWithValue("@Phone", customer.Phone);
                    cmd.Parameters.AddWithValue("@Email", customer.Email);
                    cmd.Parameters.AddWithValue("@IsLocked", customer.IsLocked);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public static bool Delete(IConfiguration configuration, int customerID)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = "DELETE FROM Customers WHERE CustomerID = @CustomerID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@CustomerID", customerID);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public static bool ChangePassword(IConfiguration configuration, int customerID, string newPassword)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = "UPDATE Customers SET Password = @Password WHERE CustomerID = @CustomerID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@CustomerID", customerID);
                    cmd.Parameters.AddWithValue("@Password", (object?)newPassword ?? DBNull.Value);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}
