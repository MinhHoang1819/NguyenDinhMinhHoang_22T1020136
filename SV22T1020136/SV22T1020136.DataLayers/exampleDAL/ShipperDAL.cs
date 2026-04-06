using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SV22T1020136.Models;

namespace SV22T1020136.DataLayers
{
    public static class ShipperDAL
    {
        public static List<Shipper> List(IConfiguration configuration, out int rowCount, string searchValue = "", int page = 1, int pageSize = 25)
        {
            List<Shipper> data = new List<Shipper>();
            rowCount = 0;

            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string countSql = @"
                    SELECT COUNT(*) 
                    FROM Shippers 
                    WHERE (@SearchValue = '' OR 
                           ShipperName LIKE '%' + @SearchValue + '%' OR
                           Phone LIKE '%' + @SearchValue + '%')";

                using (var cmd = new SqlCommand(countSql, connection))
                {
                    cmd.Parameters.AddWithValue("@SearchValue", searchValue ?? "");
                    rowCount = Convert.ToInt32(cmd.ExecuteScalar());
                }

                string sql = @"
                    SELECT * FROM (
                        SELECT *, ROW_NUMBER() OVER (ORDER BY ShipperID) AS RowNumber
                        FROM Shippers
                        WHERE (@SearchValue = '' OR 
                               ShipperName LIKE '%' + @SearchValue + '%' OR
                               Phone LIKE '%' + @SearchValue + '%')
                    ) AS T
                    WHERE RowNumber BETWEEN (@Page - 1) * @PageSize + 1 AND @Page * @PageSize
                    ORDER BY ShipperID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@SearchValue", searchValue ?? "");
                    cmd.Parameters.AddWithValue("@Page", page);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            data.Add(new Shipper
                            {
                                ShipperID = Convert.ToInt32(reader["ShipperID"]),
                                ShipperName = reader["ShipperName"]?.ToString() ?? "",
                                Phone = reader["Phone"]?.ToString() ?? ""
                            });
                        }
                    }
                }
            }

            return data;
        }

        public static Shipper? Get(IConfiguration configuration, int shipperID)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = "SELECT * FROM Shippers WHERE ShipperID = @ShipperID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@ShipperID", shipperID);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Shipper
                            {
                                ShipperID = Convert.ToInt32(reader["ShipperID"]),
                                ShipperName = reader["ShipperName"]?.ToString() ?? "",
                                Phone = reader["Phone"]?.ToString() ?? ""
                            };
                        }
                    }
                }
            }

            return null;
        }

        public static int Add(IConfiguration configuration, Shipper shipper)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = @"
                    INSERT INTO Shippers (ShipperName, Phone)
                    VALUES (@ShipperName, @Phone);
                    SELECT SCOPE_IDENTITY();";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@ShipperName", shipper.ShipperName);
                    cmd.Parameters.AddWithValue("@Phone", shipper.Phone);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public static bool Update(IConfiguration configuration, Shipper shipper)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = @"
                    UPDATE Shippers 
                    SET ShipperName = @ShipperName,
                        Phone = @Phone
                    WHERE ShipperID = @ShipperID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@ShipperID", shipper.ShipperID);
                    cmd.Parameters.AddWithValue("@ShipperName", shipper.ShipperName);
                    cmd.Parameters.AddWithValue("@Phone", shipper.Phone);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public static bool Delete(IConfiguration configuration, int shipperID)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = "DELETE FROM Shippers WHERE ShipperID = @ShipperID";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@ShipperID", shipperID);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}
