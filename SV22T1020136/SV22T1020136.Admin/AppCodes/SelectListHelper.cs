using SV22T1020136.BusinessLayers;
using SV22T1020136.Models.Common;
using SV22T1020136.Models.Sales;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace SV22T1020136.Admin
{
    /// <summary>
    /// Lớp cung cấp các hàm tiện ích dùng cho SelectList (DropDownList)
    /// </summary>
    public static class SelectListHelper
    {
        /// <summary>
        /// Tỉnh thành (đồng bộ, dùng được trong asp-items).
        /// </summary>
        public static List<SelectListItem> Provinces()
        {
            var list = new List<SelectListItem>()
            {
                new SelectListItem() { Value = "", Text = "-- Tỉnh/Thành phố --"}
            };
            var result = DictionaryDataService.ListProvincesAsync().GetAwaiter().GetResult();
            foreach (var item in result)
            {
                list.Add(new SelectListItem()
                {
                    Value = item.ProvinceName,
                    Text = item.ProvinceName
                });
            }
            return list;
        }

        /// <summary>
        /// Loại hàng (đồng bộ, dùng được trong asp-items).
        /// </summary>
        public static List<SelectListItem> Categories()
        {
            var list = new List<SelectListItem>()
            {
                new SelectListItem() { Value = "0", Text = "-- Loại hàng --"}
            };
            var input = new PaginationSearchInput() { Page = 1, PageSize = 0, SearchValue = "" };
            var result = CatalogDataService.ListCategoriesAsync(input).GetAwaiter().GetResult();
            foreach (var item in result.DataItems)
            {
                list.Add(new SelectListItem()
                {
                    Value = item.CategoryID.ToString(),
                    Text = item.CategoryName
                });
            }
            return list;
        }

        /// <summary>
        /// Nhà cung cấp (đồng bộ, dùng được trong asp-items).
        /// </summary>
        public static List<SelectListItem> Suppliers()
        {
            var list = new List<SelectListItem>()
            {
                new SelectListItem() { Value = "0", Text = "-- Nhà cung cấp --"}
            };
            var input = new PaginationSearchInput() { Page = 1, PageSize = 0, SearchValue = "" };
            var result = PartnerDataService.ListSuppliersAsync(input).GetAwaiter().GetResult();
            foreach (var item in result.DataItems)
            {
                list.Add(new SelectListItem()
                {
                    Value = item.SupplierID.ToString(),
                    Text = item.SupplierName
                });
            }
            return list;
        }

        /// <summary>
        /// Các trạng thái của đơn hàng
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> OrderStatus()
        {
            return new List<SelectListItem>
            {
                new SelectListItem() { Value = "", Text = "-- Trạng thái ---" },
                new SelectListItem() { Value = OrderStatusEnum.New.ToString(), Text = OrderStatusEnum.New.GetDescription() },
                new SelectListItem() { Value = OrderStatusEnum.Accepted.ToString(), Text = OrderStatusEnum.Accepted.GetDescription() },
                new SelectListItem() { Value = OrderStatusEnum.Shipping.ToString(), Text = OrderStatusEnum.Shipping.GetDescription() },
                new SelectListItem() { Value = OrderStatusEnum.Completed.ToString(), Text = OrderStatusEnum.Completed.GetDescription() },
                new SelectListItem() { Value = OrderStatusEnum.Rejected.ToString(), Text = OrderStatusEnum.Rejected.GetDescription() },
                new SelectListItem() { Value = OrderStatusEnum.Cancelled.ToString(), Text = OrderStatusEnum.Cancelled.GetDescription() },
            };
        }
    }
}