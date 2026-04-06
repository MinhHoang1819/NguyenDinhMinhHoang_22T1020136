using Dapper;
using SV22T1020136.DataLayers.Interfaces;
using SV22T1020136.Models.DataDictionary;

namespace SV22T1020136.DataLayers.SQLServer
{
    public class ProvinceRepository : BaseSQLDAL, IDataDictionaryRepository<Province>
    {
        public ProvinceRepository(string connectionString) : base(connectionString) { }

        public async Task<List<Province>> ListAsync()
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var sql = "SELECT * FROM Provinces ORDER BY ProvinceName";
                var data = (await connection.QueryAsync<Province>(sql)).ToList();
                return data;
            }
        }
    }
}
