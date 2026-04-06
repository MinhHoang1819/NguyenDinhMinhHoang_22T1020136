using Microsoft.AspNetCore.Mvc;
using SV22T1020136.BusinessLayers;
using SV22T1020136.Models.Common;

namespace SV22T1020136.Shop.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// T?i d? li?u c?n thi?t cho trang ch? và tr? v? View.
        /// - L?y danh sách s?n ph?m n?i b?t theo categoryId = 8 và gán vào ViewBag.FeaturedProducts.
        /// - L?y danh sách danh m?c (t?i ?a 20 m?c) và gán vào ViewBag.Categories.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            var products = await CatalogDataService.ListFeaturedProductsByCategoryAsync(8);
            ViewBag.FeaturedProducts = products;

            var categories = await CatalogDataService.ListCategoriesAsync(new PaginationSearchInput
            {
                Page = 1,
                PageSize = 20,
                SearchValue = ""
            });
            ViewBag.Categories = categories.DataItems;

            return View();
        }

        /// <summary>
        /// Hi?n th? trang liên h?. Ph??ng th?c này ch? tr? v? View mà không c?n chu?n b? d? li?u nào ??c bi?t. View s? ch?a thông tin liên h? c?a c?a hàng ho?c m?t form ?? ng??i dùng g?i yêu c?u h? tr?.
        /// </summary>
        /// <returns></returns>
        public IActionResult Contact()
        {
            return View();
        }

        /// <summary>
        /// Hi?n th? trang gi?i thi?u v? c?a hàng. Ph??ng th?c này ch? tr? v? View mà không c?n chu?n b? d? li?u nào ??c bi?t. View s? ch?a thông tin v? l?ch s?, s? m?nh, t?m nhìn ho?c các giá tr? c?t lõi c?a c?a hàng ?? khách hàng hi?u rõ h?n v? th??ng hi?u và cam k?t c?a c?a hàng ??i v?i khách hàng.
        /// </summary>
        /// <returns></returns>
        public IActionResult About()
        {
            return View();
        }

        /// <summary>
        /// Hi?n th? trang chính sách b?o m?t. Ph??ng th?c này ch? tr? v? View mà không c?n chu?n b? d? li?u nào ??c bi?t. View s? ch?a thông tin v? cách c?a hàng thu th?p, s? d?ng và b?o v? thông tin cá nhân c?a khách hàng, c?ng nh? các quy?n c?a khách hàng liên quan ??n d? li?u cá nhân c?a h?. ?ây là m?t ph?n quan tr?ng ?? xây d?ng ni?m tin v?i khách hàng và tuân th? các quy ??nh v? b?o m?t d? li?u.
        /// </summary>
        /// <returns></returns>
        public IActionResult Privacy()
        {
            return View();
        }
    }
}
