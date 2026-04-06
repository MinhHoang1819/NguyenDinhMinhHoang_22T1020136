using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020136.BusinessLayers;
using SV22T1020136.Models.Sales;
using System.Security.Claims;

namespace SV22T1020136.Shop.Controllers
{
    /// <summary>
    /// Controller xử lý các chức năng đặt hàng dành cho khách hàng (checkout, lịch sử, chi tiết, huỷ).
    /// Yêu cầu người dùng đã xác thực (Authorize).
    /// </summary>
    [Authorize]
    public class OrderController : Controller
    {
        private const int PageSize = 10;

        /// <summary>
        /// Lấy CustomerID từ Claims của người dùng đã xác thực. Phương thức này sẽ truy cập vào Claims của người dùng hiện tại và tìm claim có loại ClaimTypes.NameIdentifier, sau đó chuyển giá trị của claim này thành kiểu int để sử dụng làm CustomerID trong các thao tác liên quan đến đơn hàng. Nếu không tìm thấy claim hoặc giá trị không hợp lệ, phương thức sẽ trả về 0 (hoặc có thể xử lý lỗi tùy theo yêu cầu của ứng dụng).
        /// </summary>
        /// <returns></returns>
        private int GetCustomerId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        /// <summary>
        /// Hiển thị trang checkout, nơi khách hàng có thể xem lại giỏ hàng, nhập thông tin giao hàng và xác nhận đặt hàng. Phương thức này sẽ lấy dữ liệu giỏ hàng từ session, thông tin khách hàng từ PartnerDataService, danh sách tỉnh thành từ DictionaryDataService và truyền chúng vào View để hiển thị. Nếu giỏ hàng trống, người dùng sẽ được chuyển hướng về trang giỏ hàng với thông báo lỗi.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var cart = ApplicationContext.GetSessionData<List<CartItem>>("Cart") ?? new List<CartItem>();
            if (cart.Count == 0)
            {
                TempData["Error"] = "Giỏ hàng trống. Vui lòng thêm sản phẩm trước khi đặt hàng.";
                return RedirectToAction("Index", "Cart");
            }

            var customer = await PartnerDataService.GetCustomerAsync(GetCustomerId());
            ViewBag.Customer = customer;
            ViewBag.Cart = cart;
            ViewBag.CartTotal = cart.Sum(c => c.TotalPrice);
            ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
            return View();
        }

        /// <summary>
        /// Xử lý POST Checkout, nhận dữ liệu từ form checkout, kiểm tra tính hợp lệ của dữ liệu, tạo đơn hàng mới và chi tiết đơn hàng dựa trên giỏ hàng hiện tại. Nếu đặt hàng thành công, giỏ hàng sẽ được xóa khỏi session và người dùng sẽ được chuyển hướng đến trang chi tiết đơn hàng với thông báo thành công. Nếu có lỗi xảy ra trong quá trình đặt hàng, người dùng sẽ được chuyển hướng lại trang checkout với thông báo lỗi.
        /// </summary>
        /// <param name="deliveryProvince"></param>
        /// <param name="deliveryAddress"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Checkout(string deliveryProvince, string deliveryAddress)
        {
            var cart = ApplicationContext.GetSessionData<List<CartItem>>("Cart") ?? new List<CartItem>();
            if (cart.Count == 0)
            {
                TempData["Error"] = "Giỏ hàng trống.";
                return RedirectToAction("Index", "Cart");
            }

            if (string.IsNullOrWhiteSpace(deliveryProvince) || string.IsNullOrWhiteSpace(deliveryAddress))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ thông tin giao hàng.";
                return RedirectToAction("Checkout");
            }

            int customerId = GetCustomerId();

            var order = new Order
            {
                CustomerID = customerId,
                DeliveryProvince = deliveryProvince,
                DeliveryAddress = deliveryAddress
            };

            int orderId = await SalesDataService.AddOrderAsync(order);

            if (orderId > 0)
            {
                foreach (var item in cart)
                {
                    await SalesDataService.AddDetailAsync(new OrderDetail(orderId, item.ProductID, item.Quantity, item.Price)
                    {
                    });
                }

                ApplicationContext.SetSessionData("Cart", new List<CartItem>());
                TempData["Success"] = "Đặt hàng thành công!";
                return RedirectToAction("Detail", new { id = orderId });
            }

            TempData["Error"] = "Đã có lỗi xảy ra, vui lòng thử lại.";
            return RedirectToAction("Checkout");
        }

        /// <summary>
        /// Hiển thị lịch sử đơn hàng của khách hàng, cho phép lọc theo trạng thái đơn hàng và phân trang. Phương thức này sẽ lấy tất cả đơn hàng của khách hàng từ SalesDataService, sau đó lọc theo trạng thái nếu có và sắp xếp theo thời gian đặt hàng giảm dần. Kết quả sẽ được phân trang dựa trên PageSize và truyền vào View để hiển thị. Người dùng có thể chọn trạng thái đơn hàng để xem các đơn hàng tương ứng hoặc chuyển sang trang khác để xem thêm đơn hàng trong lịch sử.
        /// </summary>
        /// <param name="status"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public async Task<IActionResult> History(int status = 0, int page = 1)
        {
            int customerId = GetCustomerId();
            var input = new OrderSearchInput
            {
                Page = 1,
                PageSize = 200,
                SearchValue = "",
                Status = (OrderStatusEnum)status
            };

            var data = await SalesDataService.ListOrdersAsync(input);
            var filtered = data.DataItems
                .Where(x => x.CustomerID == customerId)
                .Where(x => status == 0 || (int)x.Status == status)
                .OrderByDescending(x => x.OrderTime)
                .ToList();

            var pagedItems = filtered
                .Skip((Math.Max(page, 1) - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            ViewBag.CurrentStatus = status;
            return View(new SV22T1020136.Models.Common.PagedResult<OrderViewInfo>
            {
                DataItems = pagedItems,
                RowCount = filtered.Count,
                Page = Math.Max(page, 1),
                PageSize = PageSize
            });
        }

        /// <summary>
        /// Hiển thị chi tiết đơn hàng, bao gồm thông tin đơn hàng và danh sách chi tiết sản phẩm trong đơn hàng. Phương thức này sẽ lấy thông tin đơn hàng từ SalesDataService dựa trên id đơn hàng, sau đó kiểm tra xem đơn hàng có tồn tại và thuộc về khách hàng hiện tại hay không. Nếu không hợp lệ, người dùng sẽ được chuyển hướng về trang lịch sử đơn hàng. Nếu hợp lệ, phương thức sẽ tiếp tục lấy danh sách chi tiết đơn hàng và truyền cả thông tin đơn hàng và chi tiết vào View để hiển thị. Người dùng có thể xem thông tin chi tiết của từng sản phẩm trong đơn hàng cũng như trạng thái và thông tin giao hàng của đơn hàng đó.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Detail(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null || order.CustomerID != GetCustomerId())
                return RedirectToAction("History");

            var details = await SalesDataService.ListDetailsAsync(id);
            ViewBag.Details = details;
            return View(order);
        }

        /// <summary>
        /// Yêu cầu hủy đơn hàng, chỉ cho phép hủy nếu đơn hàng thuộc về khách hàng hiện tại và đang ở trạng thái có thể hủy (ví dụ: chưa được chấp nhận hoặc chưa được giao). Phương thức này sẽ lấy thông tin đơn hàng từ SalesDataService dựa trên id đơn hàng, sau đó kiểm tra tính hợp lệ của đơn hàng. Nếu không hợp lệ, người dùng sẽ được chuyển hướng về trang lịch sử đơn hàng. Nếu hợp lệ, phương thức sẽ gọi SalesDataService để hủy đơn hàng và trả về kết quả thành công hoặc lỗi thông qua TempData. Cuối cùng, người dùng sẽ được chuyển hướng lại trang chi tiết đơn hàng để xem kết quả của hành động hủy.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null || order.CustomerID != GetCustomerId())
                return RedirectToAction("History");

            bool result = await SalesDataService.CancelOrderAsync(id);
            TempData[result ? "Success" : "Error"] = result
                ? "Đã hủy đơn hàng thành công."
                : "Không thể hủy đơn hàng này.";

            return RedirectToAction("Detail", new { id });
        }
    }
}
