using Microsoft.AspNetCore.Mvc;
using SV22T1020136.BusinessLayers;
using SV22T1020136.Models.Catalog;
using SV22T1020136.Models.Common;

namespace SV22T1020136.Shop.Controllers
{
    /// <summary>
    /// Controller xử lý hiển thị danh sách sản phẩm và chi tiết sản phẩm cho giao diện cửa hàng.
    /// </summary>
    public class ProductController : Controller
    {
        private const int PAGE_SIZE = 12;

        /// <summary>
        /// Hiển thị danh sách sản phẩm phân trang, có thể lọc theo từ khóa, danh mục và khoảng giá.
        /// </summary>
        /// <param name="search">Chuỗi tìm kiếm (tên/mô tả). Có thể null.</param>
        /// <param name="categoryId">ID danh mục để lọc (0 = tất cả danh mục).</param>
        /// <param name="minPrice">Giá tối thiểu để lọc (0 = không lọc tối thiểu).</param>
        /// <param name="maxPrice">Giá tối đa để lọc (0 = không lọc tối đa).</param>
        /// <param name="page">Số trang hiện tại (1-based).</param>
        /// <returns>View chứa dữ liệu sản phẩm phân trang và dữ liệu phụ trợ (danh mục, bộ lọc).</returns>
        public async Task<IActionResult> Index(string? search, int categoryId = 0, decimal minPrice = 0, decimal maxPrice = 0, int page = 1)
        {
            // Tạo đối tượng tìm kiếm để truyền xuống Business Layer
            var input = new ProductSearchInput
            {
                Page = page,
                PageSize = PAGE_SIZE,
                SearchValue = search ?? "",
                CategoryID = categoryId,
                MinPrice = minPrice,
                MaxPrice = maxPrice
            };

            // Lấy danh sách sản phẩm phân trang theo bộ lọc
            var data = await CatalogDataService.ListProductsAsync(input);

            // Lưu lại input để view có thể hiển thị bộ lọc hiện tại và paging
            ViewBag.SearchInput = input;

            // Tải danh sách danh mục (sử dụng page size lớn để lấy tất cả)
            var categories = await CatalogDataService.ListCategoriesAsync(new PaginationSearchInput { Page = 1, PageSize = 200 });
            ViewBag.Categories = categories.DataItems;

            // Trả về view với dữ liệu sản phẩm
            return View(data);
        }

        /// <summary>
        /// Hiển thị thông tin chi tiết của một sản phẩm, bao gồm ảnh, thuộc tính và sản phẩm liên quan.
        /// </summary>
        /// <param name="id">ID sản phẩm.</param>
        /// <returns>View chi tiết sản phẩm. Nếu không tìm thấy sản phẩm chuyển hướng về danh sách.</returns>
        public async Task<IActionResult> Detail(int id)
        {
            // Lấy thông tin sản phẩm theo id
            var product = await CatalogDataService.GetProductAsync(id);

            // Nếu không tìm thấy, quay về trang danh sách
            if (product == null)
                return RedirectToAction("Index");

            // Tải ảnh và thuộc tính của sản phẩm để hiển thị
            ViewBag.Photos = await CatalogDataService.ListPhotosAsync(id);
            ViewBag.Attributes = await CatalogDataService.ListAttributesAsync(id);

            // Lấy một số sản phẩm cùng danh mục để hiển thị sản phẩm liên quan
            var related = await CatalogDataService.ListProductsAsync(new ProductSearchInput
            {
                Page = 1,
                PageSize = 4,
                CategoryID = product.CategoryID ?? 0
            });

            // Loại bỏ sản phẩm hiện tại khỏi danh sách liên quan và giới hạn tối đa 4 mục
            ViewBag.RelatedProducts = related.DataItems.Where(p => p.ProductID != id).Take(4).ToList();

            // Trả về view chi tiết với model sản phẩm
            return View(product);
        }
    }
}
