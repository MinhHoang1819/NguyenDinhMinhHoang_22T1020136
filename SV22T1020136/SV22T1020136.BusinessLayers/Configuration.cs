namespace SV22T1020136.BusinessLayers
{
   public static class Configuration
    {
        private static string _connectionString = string.Empty;
        public static void Initialize(string connectionString)
        {
            _connectionString = connectionString;
        }
        public static string ConnectionString => _connectionString;
    }
}
