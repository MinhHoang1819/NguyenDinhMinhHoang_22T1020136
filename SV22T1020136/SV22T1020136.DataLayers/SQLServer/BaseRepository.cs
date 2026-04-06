namespace SV22T1020136.DataLayers.SqlServer
{
    /// <summary>
    /// Lớp cơ sở cho các lớp cung cấp dữ liệu trên SQL Server
    /// </summary>
    public abstract class BaseSQLDAL
    {
        /// <summary>
        /// Chuỗi kết nối đến cơ sở dữ liệu
        /// </summary>
        protected string _connectionString = string.Empty;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        public BaseSQLDAL(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Tạo một kết nối đến SQL Server
        /// </summary>
        /// <returns></returns>
        protected Microsoft.Data.SqlClient.SqlConnection GetConnection()
        {
            return new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
        }
    }
}
