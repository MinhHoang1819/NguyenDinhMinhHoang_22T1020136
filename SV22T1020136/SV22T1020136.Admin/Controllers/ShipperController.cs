using Microsoft.AspNetCore.Mvc;
using SV22T1020136.DataLayers;
using SV22T1020136.Models;

namespace SV22T1020136.Admin.Controllers
{
    public class ShipperController : Controller
    {
        private readonly IConfiguration _configuration;

        public ShipperController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index(string searchValue = "", int page = 1, int pageSize = 0)
        {
            pageSize = pageSize > 0 ? pageSize : ApplicationContext.PageSize;
            ViewData["Title"] = "Quản lý Người Giao Hàng";
            ViewBag.SearchValue = searchValue;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            int rowCount = 0;
            var shippers = ShipperDAL.List(_configuration, out rowCount, searchValue, page, pageSize);

            var totalRecords = rowCount;
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            ViewBag.TotalRecords = totalRecords;
            ViewBag.TotalPages = totalPages;
            ViewBag.HasPrevious = page > 1;
            ViewBag.HasNext = page < totalPages;

            return View(shippers);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Title"] = "Thêm Người Giao Hàng";
            return View("Edit", new Shipper());
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            ViewData["Title"] = "Sửa Người Giao Hàng";
            var shipper = ShipperDAL.Get(_configuration, id);
            if (shipper == null)
            {
                return RedirectToAction("Index");
            }
            return View(shipper);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Save(Shipper shipper)
        {
            if (string.IsNullOrWhiteSpace(shipper.ShipperName))
                ModelState.AddModelError(nameof(shipper.ShipperName), "Vui lòng nhập tên người giao hàng.");

            if (string.IsNullOrWhiteSpace(shipper.Phone))
                ModelState.AddModelError(nameof(shipper.Phone), "Vui lòng nhập số điện thoại.");

            if (ModelState.IsValid)
            {
                if (shipper.ShipperID == 0)
                {
                    // Add new
                    ShipperDAL.Add(_configuration, shipper);
                    TempData["SuccessMessage"] = "Thêm người giao hàng thành công!";
                }
                else
                {
                    // Update existing
                    if (ShipperDAL.Update(_configuration, shipper))
                    {
                        TempData["SuccessMessage"] = "Cập nhật người giao hàng thành công!";
                    }
                }
                return RedirectToAction("Index");
            }
            ViewData["Title"] = shipper.ShipperID == 0 ? "Thêm Người Giao Hàng" : "Sửa Người Giao Hàng";
            return View("Edit", shipper);
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            ViewData["Title"] = "Xóa người giao hàng";
            var shipper = ShipperDAL.Get(_configuration, id);
            if (shipper == null)
            {
                return RedirectToAction("Index");
            }

            return View(shipper);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (ShipperDAL.Delete(_configuration, id))
                TempData["SuccessMessage"] = "Xóa người giao hàng thành công!";

            return RedirectToAction("Index");
        }
    }
}
