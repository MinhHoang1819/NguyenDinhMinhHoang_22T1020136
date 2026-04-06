using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SV22T1020136.Models;

namespace SV22T1020136.DataLayers
{
    public static class ProductDAL
    {
        public static List<Product> List(IConfiguration configuration, out int rowCount, string searchValue = "", 
            int? supplierID = null, int? categoryID = null, decimal? minPrice = null, decimal? maxPrice = null,
            int page = 1, int pageSize = 25)
        {
            List<Product> data = new List<Product>();
            rowCount = 0;

            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                // Build WHERE clause
                var whereConditions = new List<string>();
                var parameters = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(searchValue))
                {
                    whereConditions.Add("(ProductName LIKE '%' + @SearchValue + '%' OR ProductDescription LIKE '%' + @SearchValue + '%')");
                    parameters.Add(new SqlParameter("@SearchValue", searchValue));
                }

                if (supplierID.HasValue && supplierID.Value > 0)
                {
                    whereConditions.Add("SupplierID = @SupplierID");
                    parameters.Add(new SqlParameter("@SupplierID", supplierID.Value));
                }

                if (categoryID.HasValue && categoryID.Value > 0)
                {
                    whereConditions.Add("CategoryID = @CategoryID");
                    parameters.Add(new SqlParameter("@CategoryID", categoryID.Value));
                }

                if (minPrice.HasValue)
                {
                    whereConditions.Add("Price >= @MinPrice");
                    parameters.Add(new SqlParameter("@MinPrice", minPrice.Value));
                }

                if (maxPrice.HasValue)
                {
                    whereConditions.Add("Price <= @MaxPrice");
                    parameters.Add(new SqlParameter("@MaxPrice", maxPrice.Value));
                }

                string whereClause = whereConditions.Count > 0 
                    ? "WHERE " + string.Join(" AND ", whereConditions)
                    : "";

                // Mỗi SqlCommand cần bản sao tham số — không tái sử dụng cùng instance (ArgumentException).
                static void AddParameterCopies(SqlCommand cmd, List<SqlParameter> source)
                {
                    foreach (var p in source)
                        cmd.Parameters.Add(new SqlParameter(p.ParameterName, p.Value ?? DBNull.Value));
                }

                // Count total records
                string countSql = $"SELECT COUNT(*) FROM Products {whereClause}";

                using (var cmd = new SqlCommand(countSql, connection))
                {
                    AddParameterCopies(cmd, parameters);
                    rowCount = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Get data with pagination
                string sql = $@"
                    SELECT * FROM (
                        SELECT *, ROW_NUMBER() OVER (ORDER BY ProductID) AS RowNumber
                        FROM Products
                        {whereClause}
                    ) AS T
                    WHERE RowNumber BETWEEN (@Page - 1) * @PageSize + 1 AND @Page * @PageSize
                    ORDER BY ProductID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    AddParameterCopies(cmd, parameters);
                    cmd.Parameters.Add(new SqlParameter("@Page", page));
                    cmd.Parameters.Add(new SqlParameter("@PageSize", pageSize));

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var product = new Product
                            {
                                ProductID = Convert.ToInt32(reader["ProductID"]),
                                ProductName = reader["ProductName"]?.ToString() ?? "",
                                ProductDescription = reader["ProductDescription"]?.ToString() ?? "",
                                SupplierID = Convert.ToInt32(reader["SupplierID"]),
                                CategoryID = Convert.ToInt32(reader["CategoryID"]),
                                Unit = reader["Unit"]?.ToString() ?? "",
                                Price = Convert.ToDecimal(reader["Price"]),
                                Photo = reader["Photo"]?.ToString() ?? "",
                                IsSelling = Convert.ToBoolean(reader["IsSelling"])
                            };

                            data.Add(product);
                        }
                    }
                }
            }

            return data;
        }

        public static Product? Get(IConfiguration configuration, int productID)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = "SELECT * FROM Products WHERE ProductID = @ProductID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@ProductID", productID);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Product
                            {
                                ProductID = Convert.ToInt32(reader["ProductID"]),
                                ProductName = reader["ProductName"]?.ToString() ?? "",
                                ProductDescription = reader["ProductDescription"]?.ToString() ?? "",
                                SupplierID = Convert.ToInt32(reader["SupplierID"]),
                                CategoryID = Convert.ToInt32(reader["CategoryID"]),
                                Unit = reader["Unit"]?.ToString() ?? "",
                                Price = Convert.ToDecimal(reader["Price"]),
                                Photo = reader["Photo"]?.ToString() ?? "",
                                IsSelling = Convert.ToBoolean(reader["IsSelling"])
                            };
                        }
                    }
                }
            }

            return null;
        }

        public static bool Add(IConfiguration configuration, Product product)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = @"INSERT INTO Products 
                    (ProductName, ProductDescription, SupplierID, CategoryID, Unit, Price, Photo, IsSelling) 
                    VALUES 
                    (@ProductName, @ProductDescription, @SupplierID, @CategoryID, @Unit, @Price, @Photo, @IsSelling)";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@ProductName", product.ProductName);
                    cmd.Parameters.AddWithValue("@ProductDescription", product.ProductDescription);
                    cmd.Parameters.AddWithValue("@SupplierID", product.SupplierID);
                    cmd.Parameters.AddWithValue("@CategoryID", product.CategoryID);
                    cmd.Parameters.AddWithValue("@Unit", product.Unit);
                    cmd.Parameters.AddWithValue("@Price", product.Price);
                    cmd.Parameters.AddWithValue("@Photo", product.Photo);
                    cmd.Parameters.AddWithValue("@IsSelling", product.IsSelling);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public static bool Update(IConfiguration configuration, Product product)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = @"UPDATE Products 
                    SET ProductName = @ProductName, 
                        ProductDescription = @ProductDescription, 
                        SupplierID = @SupplierID, 
                        CategoryID = @CategoryID, 
                        Unit = @Unit, 
                        Price = @Price, 
                        Photo = @Photo, 
                        IsSelling = @IsSelling
                    WHERE ProductID = @ProductID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@ProductID", product.ProductID);
                    cmd.Parameters.AddWithValue("@ProductName", product.ProductName);
                    cmd.Parameters.AddWithValue("@ProductDescription", product.ProductDescription);
                    cmd.Parameters.AddWithValue("@SupplierID", product.SupplierID);
                    cmd.Parameters.AddWithValue("@CategoryID", product.CategoryID);
                    cmd.Parameters.AddWithValue("@Unit", product.Unit);
                    cmd.Parameters.AddWithValue("@Price", product.Price);
                    cmd.Parameters.AddWithValue("@Photo", product.Photo);
                    cmd.Parameters.AddWithValue("@IsSelling", product.IsSelling);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public static bool UpdateMainPhoto(IConfiguration configuration, int productID, string photo)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = "UPDATE Products SET Photo = @Photo WHERE ProductID = @ProductID";
                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@ProductID", productID);
                    cmd.Parameters.AddWithValue("@Photo", photo ?? string.Empty);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public static bool Delete(IConfiguration configuration, int productID)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                // Delete product photos first
                string deletePhotosSql = "DELETE FROM ProductPhotos WHERE ProductID = @ProductID";
                using (var cmd = new SqlCommand(deletePhotosSql, connection))
                {
                    cmd.Parameters.AddWithValue("@ProductID", productID);
                    cmd.ExecuteNonQuery();
                }

                // Delete product attributes
                string deleteAttrSql = "DELETE FROM ProductAttributes WHERE ProductID = @ProductID";
                using (var cmd = new SqlCommand(deleteAttrSql, connection))
                {
                    cmd.Parameters.AddWithValue("@ProductID", productID);
                    cmd.ExecuteNonQuery();
                }

                // Then delete product
                string sql = "DELETE FROM Products WHERE ProductID = @ProductID";
                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@ProductID", productID);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

    }
}
