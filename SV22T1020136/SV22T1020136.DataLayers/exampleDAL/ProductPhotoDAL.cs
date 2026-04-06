using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SV22T1020136.Models;

namespace SV22T1020136.DataLayers
{
    public static class ProductPhotoDAL
    {
        public static List<ProductPhoto> GetByProductID(IConfiguration configuration, int productID)
        {
            List<ProductPhoto> data = new List<ProductPhoto>();

            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = "SELECT * FROM ProductPhotos WHERE ProductID = @ProductID ORDER BY DisplayOrder, PhotoID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@ProductID", productID);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            data.Add(new ProductPhoto
                            {
                                PhotoID = Convert.ToInt32(reader["PhotoID"]),
                                ProductID = Convert.ToInt32(reader["ProductID"]),
                                Photo = reader["Photo"]?.ToString() ?? "",
                                Description = reader["Description"]?.ToString() ?? "",
                                DisplayOrder = Convert.ToInt32(reader["DisplayOrder"]),
                                IsHidden = Convert.ToBoolean(reader["IsHidden"])
                            });
                        }
                    }
                }
            }

            return data;
        }

        public static ProductPhoto? Get(IConfiguration configuration, int photoID)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = "SELECT * FROM ProductPhotos WHERE PhotoID = @PhotoID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@PhotoID", photoID);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new ProductPhoto
                            {
                                PhotoID = Convert.ToInt32(reader["PhotoID"]),
                                ProductID = Convert.ToInt32(reader["ProductID"]),
                                Photo = reader["Photo"]?.ToString() ?? "",
                                Description = reader["Description"]?.ToString() ?? "",
                                DisplayOrder = Convert.ToInt32(reader["DisplayOrder"]),
                                IsHidden = Convert.ToBoolean(reader["IsHidden"])
                            };
                        }
                    }
                }
            }

            return null;
        }

        public static bool Add(IConfiguration configuration, ProductPhoto photo)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = @"INSERT INTO ProductPhotos 
                    (ProductID, Photo, Description, DisplayOrder, IsHidden) 
                    VALUES 
                    (@ProductID, @Photo, @Description, @DisplayOrder, @IsHidden)";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@ProductID", photo.ProductID);
                    cmd.Parameters.Add("@Photo", System.Data.SqlDbType.NVarChar, 500).Value =
                        string.IsNullOrWhiteSpace(photo.Photo) ? string.Empty : photo.Photo.Trim();
                    cmd.Parameters.Add("@Description", System.Data.SqlDbType.NVarChar, 400).Value =
                        string.IsNullOrWhiteSpace(photo.Description) ? string.Empty : photo.Description.Trim();
                    cmd.Parameters.AddWithValue("@DisplayOrder", photo.DisplayOrder);
                    cmd.Parameters.AddWithValue("@IsHidden", photo.IsHidden);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public static bool Update(IConfiguration configuration, ProductPhoto photo)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = @"UPDATE ProductPhotos 
                    SET Photo = @Photo, 
                        Description = @Description, 
                        DisplayOrder = @DisplayOrder, 
                        IsHidden = @IsHidden
                    WHERE PhotoID = @PhotoID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@PhotoID", photo.PhotoID);
                    cmd.Parameters.Add("@Photo", System.Data.SqlDbType.NVarChar, 500).Value =
                        string.IsNullOrWhiteSpace(photo.Photo) ? string.Empty : photo.Photo.Trim();
                    cmd.Parameters.Add("@Description", System.Data.SqlDbType.NVarChar, 400).Value =
                        string.IsNullOrWhiteSpace(photo.Description) ? string.Empty : photo.Description.Trim();
                    cmd.Parameters.AddWithValue("@DisplayOrder", photo.DisplayOrder);
                    cmd.Parameters.AddWithValue("@IsHidden", photo.IsHidden);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public static bool Delete(IConfiguration configuration, int photoID)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = "DELETE FROM ProductPhotos WHERE PhotoID = @PhotoID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@PhotoID", photoID);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}
