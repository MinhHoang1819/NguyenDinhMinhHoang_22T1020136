using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020136.BusinessLayers;
using SV22T1020136.DataLayers;
using SV22T1020136.Models.HR;


namespace SV22T1020136.Admin.Controllers
{
    [Authorize]
    public class EmployeeController : Controller
    {
        private readonly IConfiguration _configuration;

        public EmployeeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index(string searchValue = "", int page = 1, int pageSize = 0)
        {
            pageSize = pageSize > 0 ? pageSize : ApplicationContext.PageSize;
            ViewData["Title"] = "Quản lý Nhân Viên";
            ViewBag.SearchValue = searchValue;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            int rowCount = 0;
            var employees = EmployeeDAL.List(_configuration, out rowCount, searchValue, page, pageSize);

            var totalRecords = rowCount;
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            ViewBag.TotalRecords = totalRecords;
            ViewBag.TotalPages = totalPages;
            ViewBag.HasPrevious = page > 1;
            ViewBag.HasNext = page < totalPages;

            return View(employees);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung nhân viên";
            var model = new Employee()
            {
                EmployeeID = 0,
                IsWorking = true
            };
            return View("Edit", model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật thông tin nhân viên";
            var model = await HRDataService.GetEmployeeAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }

        /// <summary>
        /// Lưu nhân viên (bổ sung / cập nhật), có hỗ trợ upload ảnh.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveData(Employee data, IFormFile? uploadPhoto)
        {
            try
            {
                if (data == null)
                {
                    ViewBag.Title = "Bổ sung nhân viên";
                    ModelState.AddModelError(string.Empty, "Không đọc được dữ liệu gửi lên. Kiểm tra lại form.");
                    return View("Edit", new Employee { EmployeeID = 0, IsWorking = true });
                }

                ViewBag.Title = data.EmployeeID == 0 ? "Bổ sung nhân viên" : "Cập nhật thông tin nhân viên";

                if (string.IsNullOrWhiteSpace(data.FullName))
                    ModelState.AddModelError(nameof(data.FullName), "Vui lòng nhập họ tên nhân viên");

                if (string.IsNullOrWhiteSpace(data.Email))
                    ModelState.AddModelError(nameof(data.Email), "Vui lòng nhập email nhân viên");
                else if (!await HRDataService.ValidateEmployeeEmailAsync(data.Email, data.EmployeeID))
                    ModelState.AddModelError(nameof(data.Email), "Email đã được sử dụng bởi nhân viên khác");

                if (!ModelState.IsValid)
                    return View("Edit", data);

                if (uploadPhoto != null && uploadPhoto.Length > 0)
                {
                    var wwwroot = ApplicationContext.WWWRootPath;
                    if (string.IsNullOrWhiteSpace(wwwroot))
                    {
                        ModelState.AddModelError(string.Empty, "Thư mục wwwroot chưa được cấu hình; không thể lưu ảnh.");
                        return View("Edit", data);
                    }

                    var employeesDir = Path.Combine(wwwroot, "images", "employees");
                    if (!Directory.Exists(employeesDir))
                        Directory.CreateDirectory(employeesDir);

                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(uploadPhoto.FileName)}";
                    var filePath = Path.Combine(employeesDir, fileName);
                    await using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadPhoto.CopyToAsync(stream);
                    }
                    data.Photo = fileName;
                }

                if (string.IsNullOrEmpty(data.Address)) data.Address = "";
                if (string.IsNullOrEmpty(data.Phone)) data.Phone = "";
                if (string.IsNullOrEmpty(data.Photo)) data.Photo = "nophoto.png";

                if (data.EmployeeID == 0)
                {
                    await HRDataService.AddEmployeeAsync(data);
                    TempData["SuccessMessage"] = "Thêm nhân viên thành công!";
                }
                else
                {
                    if (await HRDataService.UpdateEmployeeAsync(data))
                        TempData["SuccessMessage"] = "Cập nhật nhân viên thành công!";
                }

                return RedirectToAction("Index");
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang bận hoặc dữ liệu không hợp lệ. Vui lòng kiểm tra dữ liệu hoặc thử lại sau");
                return View("Edit", data ?? new Employee { EmployeeID = 0, IsWorking = true });
            }
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            ViewData["Title"] = "Xóa nhân viên";
            var employee = EmployeeDAL.Get(_configuration, id);
            if (employee == null)
            {
                return RedirectToAction("Index");
            }

            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (EmployeeDAL.Delete(_configuration, id))
                TempData["SuccessMessage"] = "Xóa nhân viên thành công!";

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult ChangePassword(int id)
        {
            ViewData["Title"] = "Đổi Mật Khẩu Nhân Viên";
            var employee = EmployeeDAL.Get(_configuration, id);
            if (employee == null)
            {
                return RedirectToAction("Index");
            }
            ViewBag.EmployeeID = id;
            ViewBag.FullName = employee.FullName;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(int id, string newPassword, string confirmPassword)
        {
            var employee = EmployeeDAL.Get(_configuration, id);
            if (employee == null)
            {
                return RedirectToAction("Index");
            }

            if (string.IsNullOrEmpty(newPassword))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập mật khẩu mới.");
                ViewBag.EmployeeID = id;
                ViewBag.FullName = employee.FullName;
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu mới và xác nhận mật khẩu không khớp.");
                ViewBag.EmployeeID = id;
                ViewBag.FullName = employee.FullName;
                return View();
            }

            if (EmployeeDAL.ChangePassword(_configuration, id, newPassword))
            {
                TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult ChangeRoles(int id)
        {
            ViewData["Title"] = "Phân Quyền Nhân Viên";
            var employee = EmployeeDAL.Get(_configuration, id);
            if (employee == null)
            {
                return RedirectToAction("Index");
            }
            ViewBag.EmployeeID = id;
            ViewBag.FullName = employee.FullName;

            // Danh sách các quyền có sẵn
            var availableRoles = new List<string> { "Admin", "Manager", "Employee", "Staff", "Sale", "Warehouse" };
            ViewBag.AvailableRoles = availableRoles;

            // Lấy danh sách quyền hiện tại
            var currentRoles = (employee.RoleNames ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(r => r.Trim())
                .ToList();
            ViewBag.CurrentRoles = currentRoles;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangeRoles(int id, List<string> selectedRoles)
        {
            var employee = EmployeeDAL.Get(_configuration, id);
            if (employee == null)
            {
                return RedirectToAction("Index");
            }

            // Chuyển đổi danh sách quyền thành chuỗi phân cách bởi dấu phẩy
            string roleNames = selectedRoles != null && selectedRoles.Any()
                ? string.Join(",", selectedRoles)
                : "";

            if (EmployeeDAL.ChangeRoles(_configuration, id, roleNames))
            {
                TempData["SuccessMessage"] = "Cập nhật phân quyền thành công!";
            }
            return RedirectToAction("Index");
        }
    }
}