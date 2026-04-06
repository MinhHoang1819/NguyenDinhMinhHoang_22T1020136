using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SV22T1020136.Models;

namespace SV22T1020136.DataLayers
{
    public static class SupplierDAL
    {
        /// <summary>
        /// Lấy danh sách suppliers với phân trang và tìm kiếm
        /// </summary>
        public static List<Supplier> List(IConfiguration configuration, out int rowCount, string searchValue = "", int page = 1, int pageSize = 25)
        {
            List<Supplier> data = new List<Supplier>();
            rowCount = 0;

            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                // Đếm tổng số bản ghi
                string countSql = @"
                    SELECT COUNT(*) 
                    FROM Suppliers 
                    WHERE (@SearchValue = '' OR 
                           SupplierName LIKE '%' + @SearchValue + '%' OR
                           ContactName LIKE '%' + @SearchValue + '%' OR
                           Province LIKE '%' + @SearchValue + '%' OR
                           Phone LIKE '%' + @SearchValue + '%' OR
                           Email LIKE '%' + @SearchValue + '%')";

                using (var cmd = new SqlCommand(countSql, connection))
                {
                    cmd.Parameters.AddWithValue("@SearchValue", searchValue ?? "");
                    rowCount = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Lấy dữ liệu với phân trang
                string sql = @"
                    SELECT * FROM (
                        SELECT *, ROW_NUMBER() OVER (ORDER BY SupplierID) AS RowNumber
                        FROM Suppliers
                        WHERE (@SearchValue = '' OR 
                               SupplierName LIKE '%' + @SearchValue + '%' OR
                               ContactName LIKE '%' + @SearchValue + '%' OR
                               Province LIKE '%' + @SearchValue + '%' OR
                               Phone LIKE '%' + @SearchValue + '%' OR
                               Email LIKE '%' + @SearchValue + '%')
                    ) AS T
                    WHERE RowNumber BETWEEN (@Page - 1) * @PageSize + 1 AND @Page * @PageSize
                    ORDER BY SupplierID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@SearchValue", searchValue ?? "");
                    cmd.Parameters.AddWithValue("@Page", page);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            data.Add(new Supplier
                            {
                                SupplierID = Convert.ToInt32(reader["SupplierID"]),
                                SupplierName = reader["SupplierName"]?.ToString() ?? "",
                                ContactName = reader["ContactName"]?.ToString() ?? "",
                                Province = reader["Province"]?.ToString() ?? "",
                                Address = reader["Address"]?.ToString() ?? "",
                                Phone = reader["Phone"]?.ToString() ?? "",
                                Email = reader["Email"]?.ToString() ?? ""
                            });
                        }
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// Lấy supplier theo ID
        /// </summary>
        public static Supplier? Get(IConfiguration configuration, int supplierID)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = "SELECT * FROM Suppliers WHERE SupplierID = @SupplierID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@SupplierID", supplierID);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Supplier
                            {
                                SupplierID = Convert.ToInt32(reader["SupplierID"]),
                                SupplierName = reader["SupplierName"]?.ToString() ?? "",
                                ContactName = reader["ContactName"]?.ToString() ?? "",
                                Province = reader["Province"]?.ToString() ?? "",
                                Address = reader["Address"]?.ToString() ?? "",
                                Phone = reader["Phone"]?.ToString() ?? "",
                                Email = reader["Email"]?.ToString() ?? ""
                            };
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Thêm supplier mới
        /// </summary>
        public static int Add(IConfiguration configuration, Supplier supplier)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = @"
                    INSERT INTO Suppliers (SupplierName, ContactName, Province, Address, Phone, Email)
                    VALUES (@SupplierName, @ContactName, @Province, @Address, @Phone, @Email);
                    SELECT SCOPE_IDENTITY();";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@SupplierName", supplier.SupplierName ?? "");
                    cmd.Parameters.AddWithValue("@ContactName", supplier.ContactName ?? "");
                    cmd.Parameters.AddWithValue("@Province", supplier.Province ?? "");
                    cmd.Parameters.AddWithValue("@Address", (object?)supplier.Address ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Phone", (object?)supplier.Phone ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", supplier.Email ?? "");

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        /// <summary>
        /// Cập nhật supplier
        /// </summary>
        public static bool Update(IConfiguration configuration, Supplier supplier)
        {
            ArgumentNullException.ThrowIfNull(supplier);
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = @"
                    UPDATE Suppliers 
                    SET SupplierName = @SupplierName,
                        ContactName = @ContactName,
                        Province = @Province,
                        Address = @Address,
                        Phone = @Phone,
                        Email = @Email
                    WHERE SupplierID = @SupplierID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@SupplierID", supplier.SupplierID);
                    cmd.Parameters.AddWithValue("@SupplierName", supplier.SupplierName ?? "");
                    cmd.Parameters.AddWithValue("@ContactName", supplier.ContactName ?? "");
                    cmd.Parameters.AddWithValue("@Province", supplier.Province ?? "");
                    cmd.Parameters.AddWithValue("@Address", (object?)supplier.Address ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Phone", (object?)supplier.Phone ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", supplier.Email ?? "");

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        /// <summary>
        /// Xóa supplier
        /// </summary>
        public static bool Delete(IConfiguration configuration, int supplierID)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = "DELETE FROM Suppliers WHERE SupplierID = @SupplierID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@SupplierID", supplierID);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        /// <summary>
        /// Lấy tất cả suppliers (cho dropdown)
        /// </summary>
        public static List<Supplier> GetAll(IConfiguration configuration)
        {
            List<Supplier> data = new List<Supplier>();

            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = "SELECT * FROM Suppliers ORDER BY SupplierName";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            data.Add(new Supplier
                            {
                                SupplierID = Convert.ToInt32(reader["SupplierID"]),
                                SupplierName = reader["SupplierName"]?.ToString() ?? "",
                                ContactName = reader["ContactName"]?.ToString() ?? "",
                                Province = reader["Province"]?.ToString() ?? "",
                                Address = reader["Address"]?.ToString() ?? "",
                                Phone = reader["Phone"]?.ToString() ?? "",
                                Email = reader["Email"]?.ToString() ?? ""
                            });
                        }
                    }
                }
            }

            return data;
        }
    }
}
