using SV22T1020136.Models.Common;

namespace SV22T1020136.DataLayers.Interfaces
{
    // Common repository contract for DataLayers implementations
    public interface ICommonRepository<T> where T : class
    {
        Task<PagedResult<T>> ListAsync(PaginationSearchInput input);
        Task<int> AddAsync(T data);
        Task<bool> UpdateAsync(T data);
        Task<bool> DeleteAsync(int id);
        Task<T?> GetAsync(int id);
        Task<bool> IsUsedAsync(int id);
    }
}