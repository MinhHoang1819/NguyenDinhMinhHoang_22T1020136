using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SV22T1020136.Models;

namespace SV22T1020136.DataLayers
{
    public static class CategoryDALHelpers
    {

        public static int Add(IConfiguration configuration, Category category)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = @"
                    INSERT INTO Categories (CategoryName, Description)
                    VALUES (@CategoryName, @Description);
                    SELECT SCOPE_IDENTITY();";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@CategoryName", category.CategoryName);
                    cmd.Parameters.AddWithValue("@Description", (object?)category.Description ?? DBNull.Value);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public static bool Delete(IConfiguration configuration, int categoryID)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = "DELETE FROM Categories WHERE CategoryID = @CategoryID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@CategoryID", categoryID);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public static Category? Get(IConfiguration configuration, int categoryID)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = "SELECT * FROM Categories WHERE CategoryID = @CategoryID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@CategoryID", categoryID);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Category
                            {
                                CategoryID = Convert.ToInt32(reader["CategoryID"]),
                                CategoryName = reader["CategoryName"]?.ToString() ?? "",
                                Description = reader["Description"]?.ToString() ?? ""
                            };
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Lấy tất cả categories (cho dropdown)
        /// </summary>
        public static List<Category> GetAll(IConfiguration configuration)
        {
            List<Category> data = new List<Category>();

            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = "SELECT * FROM Categories ORDER BY CategoryName";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            data.Add(new Category
                            {
                                CategoryID = Convert.ToInt32(reader["CategoryID"]),
                                CategoryName = reader["CategoryName"]?.ToString() ?? "",
                                Description = reader["Description"]?.ToString() ?? ""
                            });
                        }
                    }
                }
            }

            return data;
        }
        public static List<Category> List(IConfiguration configuration, out int rowCount, string searchValue = "", int page = 1, int pageSize = 25)
        {
            List<Category> data = new List<Category>();
            rowCount = 0;

            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string countSql = @"
                    SELECT COUNT(*) 
                    FROM Categories 
                    WHERE (@SearchValue = '' OR 
                           CategoryName LIKE '%' + @SearchValue + '%' OR
                           Description LIKE '%' + @SearchValue + '%')";

                using (var cmd = new SqlCommand(countSql, connection))
                {
                    cmd.Parameters.AddWithValue("@SearchValue", searchValue ?? "");
                    rowCount = Convert.ToInt32(cmd.ExecuteScalar());
                }

                string sql = @"
                    SELECT * FROM (
                        SELECT *, ROW_NUMBER() OVER (ORDER BY CategoryID) AS RowNumber
                        FROM Categories
                        WHERE (@SearchValue = '' OR 
                               CategoryName LIKE '%' + @SearchValue + '%' OR
                               Description LIKE '%' + @SearchValue + '%')
                    ) AS T
                    WHERE RowNumber BETWEEN (@Page - 1) * @PageSize + 1 AND @Page * @PageSize
                    ORDER BY CategoryID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@SearchValue", searchValue ?? "");
                    cmd.Parameters.AddWithValue("@Page", page);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            data.Add(new Category
                            {
                                CategoryID = Convert.ToInt32(reader["CategoryID"]),
                                CategoryName = reader["CategoryName"]?.ToString() ?? "",
                                Description = reader["Description"]?.ToString() ?? ""
                            });
                        }
                    }
                }
            }

            return data;
        }

        public static bool Update(IConfiguration configuration, Category category)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = @"
                    UPDATE Categories 
                    SET CategoryName = @CategoryName,
                        Description = @Description
                    WHERE CategoryID = @CategoryID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@CategoryID", category.CategoryID);
                    cmd.Parameters.AddWithValue("@CategoryName", category.CategoryName);
                    cmd.Parameters.AddWithValue("@Description", (object?)category.Description ?? DBNull.Value);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}