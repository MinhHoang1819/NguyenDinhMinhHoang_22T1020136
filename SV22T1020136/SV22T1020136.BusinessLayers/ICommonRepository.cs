using SV22T1020136.Models.Catalog;
using SV22T1020136.Models.Common;

namespace SV22T1020136.BusinessLayers
{
    internal interface ICommonRepository<T>
    {
        Task<PagedResult<Category>> ListAsync(PaginationSearchInput input);
        Task<int> AddAsync(T data);
        Task<bool> UpdateAsync(T data);
        Task<bool> DeleteAsync(int id);
        Task<T?> GetAsync(int id);
        Task<bool> IsUsedAsync(int id);
    }
}