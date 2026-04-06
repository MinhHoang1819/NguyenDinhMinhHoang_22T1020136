using Microsoft.AspNetCore.Mvc;
using SV22T1020136.DataLayers;
using Microsoft.AspNetCore.Authorization;
using SV22T1020136.Models.Sales;
using ProductModel = SV22T1020136.Models.Product;
using OrderModel = SV22T1020136.Models.Order;

namespace SV22T1020136.Admin.Controllers
{
    [Authorize(Roles = $"{WebUserRoles.Sales},{WebUserRoles.Administrator}")]
    public class OrderController : Controller
    {
        private readonly IConfiguration _configuration;
        private const string SessionCartKey = "OrderCart";
        /// <summary>Danh sách tìm mặt hàng trên màn Lập đơn hàng — cố định, không theo PageSize appsettings.</summary>
        private const int OrderCreateProductPageSize = 5;

        public OrderController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private List<OrderDetail> GetCart()
        {
            return ApplicationContext.GetSessionData<List<OrderDetail>>(SessionCartKey) ?? new List<OrderDetail>();
        }

        private void SaveCart(List<OrderDetail> cart)
        {
            ApplicationContext.SetSessionData(SessionCartKey, cart ?? new List<OrderDetail>());
        }

        // GET: Order
        public IActionResult Index(string q = "", int page = 1, int pageSize = 0)
        {
            pageSize = pageSize > 0 ? pageSize : ApplicationContext.PageSize;
            ViewData["Title"] = "Quản lý Đơn Hàng";
            ViewBag.Query = q;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            int rowCount = 0;
            // OrderDAL.List đã phân trang trong SQL; không dùng Skip/Take thêm (trước đây làm sai tổng số và ẩn đơn mới).
            var items = OrderDAL.List(_configuration, out rowCount, q, page, pageSize);

            var totalPages = rowCount > 0 ? (int)System.Math.Ceiling((double)rowCount / pageSize) : 1;
            if (totalPages < 1) totalPages = 1;

            ViewBag.TotalRecords = rowCount;
            ViewBag.TotalPages = totalPages;
            ViewBag.HasPrevious = page > 1;
            ViewBag.HasNext = page < totalPages;

            return View(items);
        }

        // GET: Order/Create
        public IActionResult Create(string searchValue = "", int page = 1)
        {
            ViewData["Title"] = "Tạo Đơn Hàng";

            var pageSize = OrderCreateProductPageSize;

            int prodRow = 0;
            var products = ProductDAL.List(_configuration, out prodRow, searchValue ?? "", null, null, null, null, page, pageSize)
                .Where(p => p.IsSelling)
                .ToList();
            int custRow = 0;
            var customers = CustomerDAL.List(_configuration, out custRow, "", 1, 200);

            ViewBag.Products = products;
            ViewBag.Customers = customers;
            ViewBag.SearchValue = searchValue ?? "";
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.RowCount = prodRow;

            var cart = GetCart();
            ViewBag.Cart = cart;

            var cartProducts = new Dictionary<int, ProductModel>();
            foreach (var item in cart)
            {
                if (cartProducts.ContainsKey(item.ProductID)) continue;
                var p = ProductDAL.Get(_configuration, item.ProductID);
                if (p != null) cartProducts[item.ProductID] = p;
            }
            ViewBag.CartProducts = cartProducts;

            return View();
        }

        // POST: Order/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(OrderModel order)
        {
            if (order == null)
            {
                ModelState.AddModelError(string.Empty, "Dữ liệu đơn hàng không hợp lệ.");
            }

            var cart = GetCart();
            if (cart.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Vui lòng thêm ít nhất một sản phẩm vào đơn hàng.");
            }

            if (!ModelState.IsValid)
            {
                // reload products/customers for re-render
                int prodRow = 0;
                var products = ProductDAL.List(_configuration, out prodRow, "", null, null, null, null, 1, 200);
                int custRow = 0;
                var customers = CustomerDAL.List(_configuration, out custRow, "", 1, 200);
                ViewBag.Products = products;
                ViewBag.Customers = customers;
                ViewBag.Cart = cart;
                return View(order);
            }

            if (order == null)
                return RedirectToAction(nameof(Create));

            order.OrderTime = DateTime.Now;
            order.Status = order.Status ?? ((int)OrderStatusEnum.New).ToString();
            order.OrderDetails = cart;

            var userData = User.GetUserData();
            if (userData != null && int.TryParse(userData.UserId, out var empId))
            {
                order.EmployeeID = empId;
            }

            var newId = OrderDAL.Add(_configuration, order);
            SaveCart(new List<OrderDetail>());
            return RedirectToAction(nameof(Detail), new { id = newId });
        }

        // GET: Order/Detail/5
        [HttpGet]
        public IActionResult Detail(int id)
        {
            ViewData["Title"] = "Chi tiết Đơn Hàng";
            var order = OrderDAL.Get(_configuration, id);
            if (order == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.OrderId = id;
            return View(order);
        }

        // GET: Order/EditCartItem/5?productId=10
        [HttpGet]
        public IActionResult EditCartItem(int id, int productId)
        {
            ViewData["Title"] = "Chỉnh sửa Sản phẩm trong Đơn";
            var p = ProductDAL.Get(_configuration, productId);
            ViewBag.ProductName = p?.ProductName ?? $"Mặt hàng #{productId}";
            ViewBag.ProductPhoto = p?.Photo;

            if (id > 0)
            {
                var order = OrderDAL.Get(_configuration, id);
                if (order == null) return RedirectToAction("Index");

                var item = order.OrderDetails.FirstOrDefault(i => i.ProductID == productId)
                           ?? new OrderDetail(default, default, default, default) { ProductID = productId, Quantity = 1, SalePrice = 0m };
                if (item.SalePrice <= 0 && p != null)
                    item.SalePrice = p.Price;
                ViewBag.OrderId = id;
                return View(item);
            }

            var cart = GetCart();
            var itemInCart = cart.FirstOrDefault(i => i.ProductID == productId)
                             ?? new OrderDetail(default, default, default, default) { ProductID = productId, Quantity = 1, SalePrice = 0m };
            if (itemInCart.SalePrice <= 0 && p != null)
                itemInCart.SalePrice = p.Price;
            ViewBag.OrderId = 0;
            return View(itemInCart);
        }

        // GET: Order/AddCartItem/5?productId=10
        [HttpGet]
        public IActionResult AddCartItem(int id, int productId)
        {
            // default quantity = 1
            var success = OrderDAL.UpdateCartItem(_configuration, id, productId, 1);
            if (success)
            {
                TempData["Success"] = $"Added product {productId} to order {id}.";
            }
            else
            {
                TempData["Error"] = $"Could not add product {productId} to order {id}.";
            }
            return RedirectToAction(nameof(Detail), new { id });
        }

        // POST: Order/AddCartItem/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddCartItem(int id, int productId, int quantity, decimal salePrice = 0m)
        {
            if (id > 0)
            {
                var success = OrderDAL.UpdateCartItem(_configuration, id, productId, quantity);
                TempData[success ? "Success" : "Error"] = success
                    ? $"Added/updated product {productId} in order {id}."
                    : $"Could not add/update product {productId} in order {id}.";
                return RedirectToAction(nameof(Detail), new { id });
            }

            {
                var cart = GetCart();
                var p = ProductDAL.Get(_configuration, productId);
                if (p == null)
                {
                    TempData["Error"] = "Không tìm thấy mặt hàng.";
                    return RedirectToAction(nameof(Create));
                }

                var price = salePrice > 0 ? salePrice : p.Price;
                var existing = cart.FirstOrDefault(x => x.ProductID == productId);
                if (existing == null)
                {
                    if (quantity <= 0) quantity = 1;
                    cart.Add(new OrderDetail(default, default, default, default) { ProductID = productId, Quantity = quantity, SalePrice = price });
                }
                else
                {
                    existing.Quantity = Math.Max(1, existing.Quantity + Math.Max(1, quantity));
                    existing.SalePrice = price;
                }

                SaveCart(cart);
                TempData["Success"] = "Đã thêm vào giỏ hàng.";
                return RedirectToAction(nameof(Create));
            }
        }

        // POST: Order/EditCartItem/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCartItem(int id, int productId, int quantity, decimal salePrice = 0m)
        {
            if (id > 0)
            {
                var success = OrderDAL.UpdateCartItem(_configuration, id, productId, quantity);
                TempData[success ? "Success" : "Error"] = success
                    ? $"Updated product {productId} in order {id}."
                    : $"Could not update product {productId} in order {id}.";
                return RedirectToAction(nameof(Detail), new { id });
            }

            {
                var cart = GetCart();
                var item = cart.FirstOrDefault(x => x.ProductID == productId);
                if (item == null)
                {
                    TempData["Error"] = "Mặt hàng không còn trong giỏ.";
                    return RedirectToAction(nameof(Create));
                }

                item.Quantity = Math.Max(1, quantity);
                if (salePrice > 0) item.SalePrice = salePrice;
                SaveCart(cart);
                TempData["Success"] = "Đã cập nhật giỏ hàng.";
                return RedirectToAction(nameof(Create));
            }
        }

        // GET: Order/DeleteCartItem/5?productId=10 (modal)
        [HttpGet]
        public IActionResult DeleteCartItem(int id, int productId)
        {
            ViewBag.OrderId = id;
            ViewBag.ProductId = productId;
            return View();
        }

        // POST: Order/DeleteCartItem/5?productId=10
        [HttpPost]
        [ActionName("DeleteCartItem")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCartItemPost(int id, int productId)
        {
            if (id > 0)
            {
                var success = OrderDAL.RemoveCartItem(_configuration, id, productId);
                TempData[success ? "Success" : "Error"] = success
                    ? $"Removed product {productId} from order {id}."
                    : $"Could not remove product {productId} from order {id}.";
                return RedirectToAction(nameof(Detail), new { id });
            }

            {
                var cart = GetCart();
                cart = cart.Where(x => x.ProductID != productId).ToList();
                SaveCart(cart);
                TempData["Success"] = "Đã xóa mặt hàng khỏi giỏ.";
                return RedirectToAction(nameof(Create));
            }
        }

        // GET: Order/ClearCart (modal)
        [HttpGet]
        public IActionResult ClearCart(int id)
        {
            ViewBag.OrderId = id;
            return View();
        }

        // POST: Order/ClearCart
        [HttpPost]
        [ActionName("ClearCart")]
        [ValidateAntiForgeryToken]
        public IActionResult ClearCartPost(int id)
        {
            if (id > 0)
            {
                var success = OrderDAL.ClearCart(_configuration, id);
                TempData["Success"] = success ? $"Cleared cart for order {id}." : $"Could not clear cart for order {id}.";
                return RedirectToAction(nameof(Detail), new { id });
            }

            SaveCart(new List<OrderDetail>());
            TempData["Success"] = "Đã xóa giỏ hàng.";
            return RedirectToAction(nameof(Create));
        }

        // POST: Order/Accept/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Accept(int id)
        {
            OrderDAL.SetStatus(_configuration, id, ((int)OrderStatusEnum.Accepted).ToString());
            TempData["Success"] = $"Order {id} accepted.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        // POST: Order/Shipping/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Shipping(int id)
        {
            OrderDAL.SetStatus(_configuration, id, ((int)OrderStatusEnum.Shipping).ToString());
            TempData["Success"] = $"Order {id} marked shipping.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        // POST: Order/Finish/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Finish(int id)
        {
            OrderDAL.SetStatus(_configuration, id, ((int)OrderStatusEnum.Completed).ToString());
            TempData["Success"] = $"Order {id} finished.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        // POST: Order/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reject(int id)
        {
            OrderDAL.SetStatus(_configuration, id, ((int)OrderStatusEnum.Rejected).ToString());
            TempData["Success"] = $"Order {id} rejected.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        // POST: Order/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(int id)
        {
            OrderDAL.SetStatus(_configuration, id, ((int)OrderStatusEnum.Cancelled).ToString());
            TempData["Success"] = $"Order {id} cancelled.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        // POST: Order/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var success = OrderDAL.Delete(_configuration, id);
            if (success)
            {
                TempData["Success"] = $"Order {id} deleted.";
            }
            else
            {
                TempData["Error"] = $"Could not delete order {id}.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
