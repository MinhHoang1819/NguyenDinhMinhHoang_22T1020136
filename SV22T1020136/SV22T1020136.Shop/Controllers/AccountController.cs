using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020136.BusinessLayers;
using SV22T1020136.Models.Partner;
using System.Security.Claims;

namespace SV22T1020136.Shop.Controllers
{
    public class AccountController : Controller
    {
        /// <summary>
        /// Hiển thị trang đăng nhập. Nếu người dùng đã đăng nhập sẽ được chuyển hướng về trang chủ.
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Login(string? returnUrl)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        /// <summary>
        /// Xử lý đăng nhập. Kiểm tra thông tin đăng nhập và nếu hợp lệ sẽ tạo cookie xác thực cho người dùng. Nếu có returnUrl hợp lệ sẽ chuyển hướng về đó, ngược lại sẽ về trang chủ.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui lòng nhập email và mật khẩu.";
                return View();
            }

            var customer = await PartnerDataService.AuthorizeCustomerAsync(email, password);
            if (customer == null)
            {
                ViewBag.Error = "Email hoặc mật khẩu không đúng, hoặc tài khoản đã bị khóa.";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, customer.CustomerID.ToString()),
                new Claim(ClaimTypes.Name, customer.CustomerName),
                new Claim(ClaimTypes.Email, customer.Email),
                new Claim("ContactName", customer.ContactName ?? ""),
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Hiển thị trang đăng ký. Nếu người dùng đã đăng nhập sẽ được chuyển hướng về trang chủ. Trang này cho phép người dùng mới tạo tài khoản bằng cách nhập thông tin cần thiết. Sau khi đăng ký thành công, người dùng sẽ được chuyển hướng đến trang đăng nhập để đăng nhập vào tài khoản mới tạo.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");
            return View();
        }

        /// <summary>
        /// Xử lý đăng ký. Kiểm tra thông tin nhập vào, nếu hợp lệ sẽ tạo tài khoản mới cho người dùng và chuyển hướng đến trang đăng nhập. Nếu có lỗi sẽ hiển thị lại form đăng ký với thông báo lỗi tương ứng.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="confirmPassword"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Register(Customer data, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(data.CustomerName))
                ModelState.AddModelError("CustomerName", "Vui lòng nhập tên.");
            if (string.IsNullOrWhiteSpace(data.Email))
                ModelState.AddModelError("Email", "Vui lòng nhập email.");
            if (string.IsNullOrWhiteSpace(data.Password) || data.Password.Length < 6)
                ModelState.AddModelError("Password", "Mật khẩu phải có ít nhất 6 ký tự.");
            if (data.Password != confirmPassword)
                ModelState.AddModelError("confirmPassword", "Mật khẩu xác nhận không khớp.");

            if (!string.IsNullOrWhiteSpace(data.Email))
            {
                bool emailOk = await PartnerDataService.ValidateCustomerEmailAsync(data.Email, 0);
                if (!emailOk)
                    ModelState.AddModelError("Email", "Email này đã được sử dụng.");
            }

            if (!ModelState.IsValid)
                return View(data);

            data.IsLocked = false;
            data.ContactName = data.ContactName ?? data.CustomerName;
            int id = await PartnerDataService.AddCustomerAsync(data);
            if (id > 0)
            {
                TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }

            ViewBag.Error = "Đã có lỗi xảy ra, vui lòng thử lại.";
            return View(data);
        }

        /// <summary>
        /// Đăng xuất người dùng. Phương thức này sẽ xóa session và cookie xác thực của người dùng, sau đó chuyển hướng về trang đăng nhập. Đây là cách để người dùng kết thúc phiên làm việc hiện tại và đảm bảo rằng các thông tin xác thực không còn hiệu lực nữa.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        /// <summary>
        /// Hiển thị trang thông tin cá nhân của người dùng đã đăng nhập. Phương thức này sẽ lấy thông tin khách hàng từ cơ sở dữ liệu dựa trên ID được lưu trong cookie xác thực, sau đó truyền dữ liệu này vào view để hiển thị. Người dùng có thể xem và cập nhật thông tin cá nhân của mình trên trang này. Nếu không tìm thấy khách hàng hoặc người dùng chưa đăng nhập, sẽ được chuyển hướng về trang đăng nhập hoặc đăng xuất tương ứng.
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            int customerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var customer = await PartnerDataService.GetCustomerAsync(customerId);
            if (customer == null)
                return RedirectToAction("Logout");

            ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
            return View(customer);
        }

        /// <summary>
        /// Xử lý cập nhật thông tin cá nhân của người dùng. Phương thức này sẽ nhận dữ liệu từ form, kiểm tra tính hợp lệ của dữ liệu, sau đó cập nhật thông tin khách hàng trong cơ sở dữ liệu. Nếu cập nhật thành công, sẽ tạo lại cookie xác thực với thông tin mới và chuyển hướng về trang thông tin cá nhân. Nếu có lỗi, sẽ hiển thị lại form với thông báo lỗi tương ứng.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Profile(Customer data)
        {
            int customerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            data.CustomerID = customerId;

            var existing = await PartnerDataService.GetCustomerAsync(customerId);
            if (existing == null)
                return RedirectToAction("Logout");

            data.IsLocked = existing.IsLocked;
            data.Password = existing.Password;

            if (string.IsNullOrWhiteSpace(data.CustomerName))
                ModelState.AddModelError("CustomerName", "Vui lòng nhập tên.");
            if (string.IsNullOrWhiteSpace(data.Email))
                ModelState.AddModelError("Email", "Vui lòng nhập email.");

            if (!string.IsNullOrWhiteSpace(data.Email) && data.Email != existing.Email)
            {
                bool emailOk = await PartnerDataService.ValidateCustomerEmailAsync(data.Email, customerId);
                if (!emailOk)
                    ModelState.AddModelError("Email", "Email này đã được sử dụng.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
                return View(data);
            }

            await PartnerDataService.UpdateCustomerAsync(data);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, data.CustomerID.ToString()),
                new Claim(ClaimTypes.Name, data.CustomerName),
                new Claim(ClaimTypes.Email, data.Email),
                new Claim("ContactName", data.ContactName ?? ""),
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            TempData["Success"] = "Cập nhật thông tin thành công!";
            return RedirectToAction("Profile");
        }

        /// <summary>
        /// Hiển thị trang đổi mật khẩu cho người dùng đã đăng nhập. Trang này sẽ cho phép người dùng nhập mật khẩu cũ, mật khẩu mới và xác nhận mật khẩu mới. Khi người dùng gửi form, hệ thống sẽ kiểm tra tính hợp lệ của dữ liệu, xác thực mật khẩu cũ và nếu mọi thứ hợp lệ sẽ cập nhật mật khẩu mới cho tài khoản của người dùng. Nếu có lỗi sẽ hiển thị lại form với thông báo lỗi tương ứng.
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        /// <summary>
        /// Xử lý đổi mật khẩu cho người dùng đã đăng nhập. Phương thức này sẽ nhận dữ liệu từ form, kiểm tra tính hợp lệ của dữ liệu, xác thực mật khẩu cũ bằng cách gọi dịch vụ xác thực, và nếu mọi thứ hợp lệ sẽ cập nhật mật khẩu mới cho tài khoản của người dùng. Sau khi đổi mật khẩu thành công, sẽ chuyển hướng về trang đổi mật khẩu với thông báo thành công. Nếu có lỗi, sẽ hiển thị lại form với thông báo lỗi tương ứng.
        /// </summary>
        /// <param name="oldPassword"></param>
        /// <param name="newPassword"></param>
        /// <param name="confirmPassword"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
                return View();
            }
            if (newPassword.Length < 6)
            {
                ViewBag.Error = "Mật khẩu mới phải có ít nhất 6 ký tự.";
                return View();
            }
            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                return View();
            }

            int customerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            string email = User.FindFirstValue(ClaimTypes.Email) ?? "";

            var check = await PartnerDataService.AuthorizeCustomerAsync(email, oldPassword);
            if (check == null)
            {
                ViewBag.Error = "Mật khẩu cũ không đúng.";
                return View();
            }

            await PartnerDataService.ChangeCustomerPasswordAsync(customerId, newPassword);
            TempData["Success"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("ChangePassword");
        }

        /// <summary>
        /// Trang hiển thị khi người dùng không có quyền truy cập vào tài nguyên.
        /// </summary>
        /// <returns>View AccessDenied.</returns>
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
