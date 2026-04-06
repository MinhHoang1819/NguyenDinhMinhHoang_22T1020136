using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace SV22T1020136.DataLayers
{
    public static class DatabaseHelper
    {
        /// <summary>
        /// Lấy connection string từ configuration
        /// </summary>
        public static string GetConnectionString(IConfiguration configuration)
        {
            return configuration.GetConnectionString("LiteCommerceDB") 
                ?? throw new Exception("Connection string 'LiteCommerceDB' not found in configuration.");
        }

        /// <summary>
        /// Tạo và mở kết nối tới database
        /// </summary>
        public static SqlConnection CreateConnection(IConfiguration configuration)
        {
            var connectionString = GetConnectionString(configuration);
            var connection = new SqlConnection(connectionString);
            return connection;
        }

        /// <summary>
        /// Test kết nối database
        /// </summary>
        public static bool TestConnection(IConfiguration configuration)
        {
            try
            {
                using (var connection = CreateConnection(configuration))
                {
                    connection.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
