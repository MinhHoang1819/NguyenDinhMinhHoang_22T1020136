using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ProductAttribute = SV22T1020136.Models.ProductAttribute;

namespace SV22T1020136.DataLayers
{
    public static class ProductAttributeDAL
    {
        public static List<ProductAttribute> GetByProductID(IConfiguration configuration, int productID)
        {
            List<ProductAttribute> data = new List<ProductAttribute>();

            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = "SELECT * FROM ProductAttributes WHERE ProductID = @ProductID ORDER BY DisplayOrder, AttributeID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@ProductID", productID);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            data.Add(new ProductAttribute
                            {
                                AttributeID = Convert.ToInt32(reader["AttributeID"]),
                                ProductID = Convert.ToInt32(reader["ProductID"]),
                                AttributeName = reader["AttributeName"]?.ToString() ?? "",
                                AttributeValue = reader["AttributeValue"]?.ToString() ?? "",
                                DisplayOrder = Convert.ToInt32(reader["DisplayOrder"])
                            });
                        }
                    }
                }
            }

            return data;
        }

        public static ProductAttribute? Get(IConfiguration configuration, int attributeID)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = "SELECT * FROM ProductAttributes WHERE AttributeID = @AttributeID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@AttributeID", attributeID);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new ProductAttribute
                            {
                                AttributeID = Convert.ToInt32(reader["AttributeID"]),
                                ProductID = Convert.ToInt32(reader["ProductID"]),
                                AttributeName = reader["AttributeName"]?.ToString() ?? "",
                                AttributeValue = reader["AttributeValue"]?.ToString() ?? "",
                                DisplayOrder = Convert.ToInt32(reader["DisplayOrder"])
                            };
                        }
                    }
                }
            }

            return null;
        }

        public static bool Add(IConfiguration configuration, ProductAttribute attribute)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = @"INSERT INTO ProductAttributes 
                    (ProductID, AttributeName, AttributeValue, DisplayOrder) 
                    VALUES 
                    (@ProductID, @AttributeName, @AttributeValue, @DisplayOrder)";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@ProductID", attribute.ProductID);
                    cmd.Parameters.AddWithValue("@AttributeName", attribute.AttributeName);
                    cmd.Parameters.AddWithValue("@AttributeValue", attribute.AttributeValue);
                    cmd.Parameters.AddWithValue("@DisplayOrder", attribute.DisplayOrder);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public static bool Update(IConfiguration configuration, ProductAttribute attribute)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = @"UPDATE ProductAttributes 
                    SET AttributeName = @AttributeName, 
                        AttributeValue = @AttributeValue, 
                        DisplayOrder = @DisplayOrder
                    WHERE AttributeID = @AttributeID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@AttributeID", attribute.AttributeID);
                    cmd.Parameters.AddWithValue("@AttributeName", attribute.AttributeName);
                    cmd.Parameters.AddWithValue("@AttributeValue", attribute.AttributeValue);
                    cmd.Parameters.AddWithValue("@DisplayOrder", attribute.DisplayOrder);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public static bool Delete(IConfiguration configuration, int attributeID)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = "DELETE FROM ProductAttributes WHERE AttributeID = @AttributeID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@AttributeID", attributeID);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public static bool Add(IConfiguration configuration, Models.Catalog.ProductAttribute attribute)
        {
            throw new NotImplementedException();
        }

        public static bool Update(IConfiguration configuration, Models.Catalog.ProductAttribute attribute)
        {
            throw new NotImplementedException();
        }
    }
}
