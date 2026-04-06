using Microsoft.AspNetCore.Mvc;
using SV22T1020136.DataLayers;
using SV22T1020136.Models;

namespace SV22T1020136.Admin.Controllers
{
    public class SupplierController : Controller
    {
        private readonly IConfiguration _configuration;

        public SupplierController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index(string searchValue = "", int page = 1, int pageSize = 0)
        {
            pageSize = pageSize > 0 ? pageSize : ApplicationContext.PageSize;
            ViewData["Title"] = "Quản lý Nhà Cung Cấp";
            ViewBag.SearchValue = searchValue;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            int rowCount = 0;
            var suppliers = SupplierDAL.List(_configuration, out rowCount, searchValue, page, pageSize);

            var totalRecords = rowCount;
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            ViewBag.TotalRecords = totalRecords;
            ViewBag.TotalPages = totalPages;
            ViewBag.HasPrevious = page > 1;
            ViewBag.HasNext = page < totalPages;

            return View(suppliers);
        }

        [HttpGet]
        public IActionResult Edit(int id = 0)
        {
            ViewData["Title"] = id == 0 ? "Bổ Sung Nhà Cung Cấp" : "Cập nhập thông tin Nhà Cung Cấp";
            var supplier = id == 0 ? new Supplier() : SupplierDAL.Get(_configuration, id);
            if (supplier == null && id != 0)
            {
                return RedirectToAction("Index");
            }
            return View(supplier);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Save(Supplier supplier)
        {
            ArgumentNullException.ThrowIfNull(supplier);
            if (string.IsNullOrWhiteSpace(supplier.SupplierName))
                ModelState.AddModelError(nameof(supplier.SupplierName), "Vui lòng nhập tên nhà cung cấp.");

            if (string.IsNullOrWhiteSpace(supplier.Phone))
                ModelState.AddModelError(nameof(supplier.Phone), "Vui lòng nhập số điện thoại.");

            if (string.IsNullOrWhiteSpace(supplier.Email))
                ModelState.AddModelError(nameof(supplier.Email), "Vui lòng nhập email.");

            if (string.IsNullOrWhiteSpace(supplier.Address))
                ModelState.AddModelError(nameof(supplier.Address), "Vui lòng nhập địa chỉ.");

            if (string.IsNullOrWhiteSpace(supplier.Province))
                ModelState.AddModelError(nameof(supplier.Province), "Vui lòng chọn tỉnh/thành.");

            if (ModelState.IsValid)
            {
                if (supplier.SupplierID == 0)
                {
                    // Add new
                    SupplierDAL.Add(_configuration, supplier);
                    TempData["SuccessMessage"] = "Thêm nhà cung cấp thành công!";
                }
                else
                {
                    // Update existing
                    if (SupplierDAL.Update(_configuration, supplier))
                    {
                        TempData["SuccessMessage"] = "Cập nhật nhà cung cấp thành công!";
                    }
                }
                return RedirectToAction("Index");
            }
            ViewData["Title"] = supplier.SupplierID == 0 ? "Thêm Nhà Cung Cấp" : "Sửa Nhà Cung Cấp";
            return View("Edit", supplier);
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            ViewData["Title"] = "Xóa nhà cung cấp";
            var supplier = SupplierDAL.Get(_configuration, id);
            if (supplier == null)
            {
                return RedirectToAction("Index");
            }

            return View(supplier);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (SupplierDAL.Delete(_configuration, id))
                TempData["SuccessMessage"] = "Xóa nhà cung cấp thành công!";

            return RedirectToAction("Index");
        }
    }
}
