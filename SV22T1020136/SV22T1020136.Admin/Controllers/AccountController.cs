using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020136.DataLayers;
using SV22T1020136.Models;

namespace SV22T1020136.Admin.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;

        public AccountController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private static bool PasswordMatchesStored(string? storedHashOrPlain, string? plainPassword)
        {
            if (string.IsNullOrEmpty(storedHashOrPlain) || plainPassword == null)
                return false;

            var md5 = CryptHelper.HashMD5(plainPassword);
            if (string.Equals(storedHashOrPlain.Trim(), md5, StringComparison.OrdinalIgnoreCase))
                return true;

            return string.Equals(storedHashOrPlain, plainPassword, StringComparison.Ordinal);
        }

        private static List<string> RoleNamesToClaims(string? roleNames)
        {
            var parts = (roleNames ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(s => s.Length > 0)
                .ToList();

            if (parts.Count == 0)
                return new List<string> { WebUserRoles.Administrator };

            var claims = new List<string>();
            foreach (var role in parts)
            {
                var claim = role switch
                {
                    "Admin" => WebUserRoles.Administrator,
                    "Sale" => WebUserRoles.Sales,
                    "Manager" => WebUserRoles.DataManager,
                    _ => role.ToLowerInvariant()
                };

                if (!claims.Contains(claim))
                    claims.Add(claim);
            }

            return claims;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var email = username?.Trim() ?? "";
            var employee = EmployeeDAL.GetByEmail(_configuration, email);

            if (employee != null && employee.IsWorking && PasswordMatchesStored(employee.Password, password))
            {
                var photo = string.IsNullOrWhiteSpace(employee.Photo) ? "nophoto.png" : employee.Photo.Trim();
                var webUser = new WebUserData
                {
                    UserId = employee.EmployeeID.ToString(),
                    UserName = employee.Email,
                    DisplayName = string.IsNullOrWhiteSpace(employee.FullName) ? employee.Email : employee.FullName,
                    Email = employee.Email,
                    Photo = photo,
                    Roles = RoleNamesToClaims(employee.RoleNames)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    webUser.CreatePrincipal());

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Sai email hoặc mật khẩu, hoặc tài khoản đã ngừng làm việc.");
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập đầy đủ thông tin.");
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu mới và xác nhận mật khẩu không khớp.");
                return View();
            }

            var userData = User.GetUserData();
            if (userData == null || !int.TryParse(userData.UserId, out var employeeId))
                return RedirectToAction("Login");

            var employee = EmployeeDAL.Get(_configuration, employeeId);
            if (employee == null || !employee.IsWorking)
                return RedirectToAction("Login");

            if (!PasswordMatchesStored(employee.Password, currentPassword))
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu hiện tại không đúng.");
                return View();
            }

            EmployeeDAL.ChangePassword(_configuration, employeeId, CryptHelper.HashMD5(newPassword));
            TempData["SuccessMessage"] = "Đổi mật khẩu thành công. Vui lòng đăng nhập lại.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public IActionResult Register(string fullName, string email, string username, string password, string confirmPassword, bool agreeTerms = false)
        {
            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập đầy đủ thông tin bắt buộc.");
                return View();
            }

            if (password != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu và xác nhận mật khẩu không khớp.");
                return View();
            }

            if (!agreeTerms)
            {
                ModelState.AddModelError(string.Empty, "Bạn phải đồng ý với điều khoản sử dụng.");
                return View();
            }

            email = email.Trim();
            username = username.Trim();

            if (!string.Equals(username, email, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập phải trùng với email để đăng nhập quản trị.");
                return View();
            }

            if (EmployeeDAL.GetByEmail(_configuration, email) != null)
            {
                ModelState.AddModelError(string.Empty, "Email này đã tồn tại trong hệ thống quản trị.");
                return View();
            }

            var employee = new Employee
            {
                FullName = fullName.Trim(),
                Email = email,
                Password = CryptHelper.HashMD5(password),
                Address = "",
                Phone = "",
                Photo = "nophoto.png",
                IsWorking = true,
                RoleNames = "Admin,Sale"
            };

            EmployeeDAL.Add(_configuration, employee);
            TempData["SuccessMessage"] = "Đăng ký tài khoản quản trị thành công. Bạn có thể đăng nhập ngay.";
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return RedirectToAction("Login");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
