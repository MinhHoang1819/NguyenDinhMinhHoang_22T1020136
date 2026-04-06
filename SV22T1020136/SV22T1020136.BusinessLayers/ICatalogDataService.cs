using SV22T1020136.Models.Catalog;
using SV22T1020136.Models.Common;

namespace SV22T1020136.BusinessLayers
{
    public interface ICatalogDataService
    {
        // --- PRODUCT ---
        Task<PagedResult<Product>> ListProductsAsync(ProductSearchInput input);
        Task<Product?> GetProductAsync(int productID);
        Task<int> AddProductAsync(Product data);
        Task<bool> UpdateProductAsync(Product data);
        Task<bool> DeleteProductAsync(int productID);
        Task<bool> IsUsedProductAsync(int productID);

        // --- PRODUCT PHOTOS & ATTRIBUTES ---
        Task<List<ProductPhoto>> ListPhotosAsync(int productID);
        Task<List<ProductAttribute>> ListAttributesAsync(int productID);

        // --- CATEGORY ---
        Task<PagedResult<Category>> ListCategoriesAsync(PaginationSearchInput input);
        Task<Category?> GetCategoryAsync(int categoryID);
        Task<int> AddCategoryAsync(Category data);
        Task<bool> UpdateCategoryAsync(Category data);
        Task<bool> DeleteCategoryAsync(int categoryID);
        Task<bool> IsUsedCategoryAsync(int categoryID); // THÊM DÒNG NÀY
    }
}