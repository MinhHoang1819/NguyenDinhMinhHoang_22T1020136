using Microsoft.AspNetCore.Mvc;
using SV22T1020136.Admin;
using SV22T1020136.BusinessLayers;
using SV22T1020136.Models.Common;
using Customer = SV22T1020136.Models.Partner.Customer;

namespace SV22T1020590.Admin.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IConfiguration _configuration;

        public CustomerController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> Index(string searchValue = "", int page = 1, int pageSize = 0)
        {
            pageSize = pageSize > 0 ? pageSize : ApplicationContext.PageSize;
            ViewData["Title"] = "Quản lý Khách Hàng";
            ViewBag.SearchValue = searchValue;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            var input = new PaginationSearchInput
            {
                Page = page,
                PageSize = pageSize,
                SearchValue = searchValue
            };
            var result = await PartnerDataService.ListCustomerAsync(input);
            var customers = result.DataItems?.ToList() ?? new List<Customer>();
            var totalRecords = result.RowCount;
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            ViewBag.TotalRecords = totalRecords;
            ViewBag.TotalPages = totalPages;
            ViewBag.HasPrevious = page > 1;
            ViewBag.HasNext = page < totalPages;

            // If a customer was just added, temporarily show it at top of page 1.
            // On browser reload, TempData expires so list returns to DB order.
            if (page == 1 && TempData.TryGetValue("NewCustomerId", out var newIdObj) && newIdObj is int newId)
            {
                var newCustomer = await PartnerDataService.GetCustomerAsync(newId);
                if (newCustomer != null && !customers.Any(c => c.CustomerID == newId))
                {
                    customers.Insert(0, newCustomer);
                    if (pageSize > 0 && customers.Count > pageSize)
                    {
                        customers.RemoveAt(customers.Count - 1);
                    }
                }
            }

            return View(customers);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Title"] = "Bổ sung Khách Hàng";
            var model = new Customer()
            {
                CustomerID = 0,
            };
            return View("Edit", model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Title"] = "Cập nhật thông tin Khách Hàng";
            var model = await PartnerDataService.GetCustomerAsync(id);
            if (model == null)
            {
                return RedirectToAction("Index");
            }
            return View(model);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Save(Customer customer)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        if (customer.CustomerID == 0)
        //        {
        //            // Add new
        //            await PartnerDataService.AddCustomerAsync(customer);
        //            TempData["SuccessMessage"] = "Thêm khách hàng thành công!";
        //        }
        //        else
        //        {
        //            // Update existing
        //            if (await PartnerDataService.UpdateCustomerAsync(customer))
        //            {
        //                TempData["SuccessMessage"] = "Cập nhật khách hàng thành công!";
        //            }
        //        }
        //        return RedirectToAction("Index");
        //    }
        //    ViewData["Title"] = customer.CustomerID == 0 ? "Thêm Khách Hàng" : "Sửa Khách Hàng";
        //    return View("Edit", customer);
        //}
        /// <summary>
        /// Lưu dữ liệu
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SaveData(Customer data)
        {
            ViewBag.Tittle = data.CustomerID == 0 ? "Bổ sung khách hàng" : "Cập nhập thông tin khách hàng";
            //TODO: Kiểm tra dữ liệu đầu vào có hợp lệ hay không?
            //Sử dụng modelState để kiểm soát lỗi và thông báo lỗi
            if (string.IsNullOrWhiteSpace(data.CustomerName))
            {
                ModelState.AddModelError(nameof(data.CustomerName), "Vui lòng nhập tên khách hàng.");
            }
            if (string.IsNullOrWhiteSpace(data.Email))
            {
                ModelState.AddModelError(nameof(data.Email), "Vui lòng nhập email khách hàng.");
            }
            else if (!await PartnerDataService.ValidateCustomerEmailAsync(data.Email, data.CustomerID))
            {
                ModelState.AddModelError(nameof(data.Email), "Email đã tồn tại. Vui lòng sử dụng email khác.");
            }
            if (string.IsNullOrWhiteSpace(data.Province))
            {
                ModelState.AddModelError(nameof(data.Province), "Vui lòng chọn tỉnh/thành.");
            }
            //Điều chỉnh dữ liệu theo logic/qui ước của hệ thống
            if (string.IsNullOrEmpty(data.ContactName)) data.ContactName = "";
            if (string.IsNullOrEmpty(data.Phone)) data.Phone = "";
            if (string.IsNullOrEmpty(data.Address)) data.Address = "";

            //Nếu có lỗi thì thông báo cho người dùng qua View, không lưu dữ liệu vào CSDL
            if (!ModelState.IsValid)
            {
                return View("Edit", data);
            }

            //Lưu dữ liệu vào CSDL
            if (data.CustomerID == 0)
            {
                var newCustomerId = await PartnerDataService.AddCustomerAsync(data);
                TempData["NewCustomerId"] = newCustomerId;
                PaginationSearchInput input = new PaginationSearchInput
                {
                    Page = 1,
                    PageSize = ApplicationContext.PageSize,
                    SearchValue = data.CustomerName
                };
                ApplicationContext.SetSessionData("CustomerSearchInput", input);
            }
            else
            {
                await PartnerDataService.UpdateCustomerAsync(data);
            }
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Trang xác nhận xóa khách hàng.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            ViewData["Title"] = "Xóa khách hàng";
            var model = await PartnerDataService.GetCustomerAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            ViewBag.AllowDelete = !(await PartnerDataService.IsUsedCustomerAsync(id));
            return View(model);
        }

        /// <summary>
        /// Thực hiện xóa khách hàng sau khi đã xác nhận.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (await PartnerDataService.IsUsedCustomerAsync(id))
                return RedirectToAction("Delete", new { id });

            await PartnerDataService.DeleteCustomerAsync(id);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword(int id)
        {
            ViewData["Title"] = "Đổi Mật Khẩu Khách Hàng";
            var customer = await PartnerDataService.GetCustomerAsync(id);
            if (customer == null)
            {
                return RedirectToAction("Index");
            }
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(int id, string newPassword, string confirmPassword)
        {
            var customer = await PartnerDataService.GetCustomerAsync(id);
            if (customer == null)
            {
                return RedirectToAction("Index");
            }

            if (string.IsNullOrEmpty(newPassword))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập mật khẩu mới.");
                return View(customer);
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu mới và xác nhận mật khẩu không khớp.");
                return View(customer);
            }

            var hashedPassword = CryptHelper.HashMD5(newPassword);
            if (await PartnerDataService.ChangeCustomerPasswordAsync(id, hashedPassword))
            {
                TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError(string.Empty, "Không thể đổi mật khẩu. Vui lòng thử lại.");
            return View(customer);

        }
    }
}
