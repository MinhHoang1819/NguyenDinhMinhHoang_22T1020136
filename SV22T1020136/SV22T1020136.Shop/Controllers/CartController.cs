using Microsoft.AspNetCore.Mvc;
using SV22T1020136.BusinessLayers;
using SV22T1020136.Models.Sales;

namespace SV22T1020136.Shop.Controllers
{
    public class CartController : Controller
    {
        /// <summary>
        /// Lấy giỏ hàng từ Session, hiển thị và xử lý các thao tác thêm, cập nhật số lượng, xóa sản phẩm và xóa toàn bộ giỏ hàng.
        /// </summary>
        private List<CartItem> GetCart()
        {
            return ApplicationContext.GetSessionData<List<CartItem>>("Cart") ?? new List<CartItem>();
        }

        /// <summary>
        /// Lưu giỏ hàng vào Session sau khi có sự thay đổi về số lượng hoặc sản phẩm trong giỏ hàng.
        /// </summary>
        /// <param name="cart"></param>
        private void SaveCart(List<CartItem> cart)
        {
            ApplicationContext.SetSessionData("Cart", cart);
        }

        /// <summary>
        /// Hiển thị nội dung giỏ hàng hiện tại. Lấy dữ liệu giỏ hàng từ Session và truyền vào view để hiển thị. View sẽ hiển thị danh sách sản phẩm trong giỏ hàng, số lượng, giá cả và tổng tiền. Người dùng có thể thực hiện các thao tác như cập nhật số lượng hoặc xóa sản phẩm từ view này.
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        /// <summary>
        /// Bổ sung sản phẩm vào giỏ hàng. Khi người dùng chọn thêm một sản phẩm vào giỏ hàng, phương thức này sẽ được gọi với ID sản phẩm và số lượng mong muốn. Phương thức sẽ kiểm tra xem sản phẩm đã tồn tại trong giỏ hàng hay chưa. Nếu đã tồn tại, nó sẽ cập nhật số lượng của sản phẩm đó. Nếu chưa tồn tại, nó sẽ truy vấn thông tin chi tiết của sản phẩm từ cơ sở dữ liệu và thêm một mục mới vào giỏ hàng với thông tin sản phẩm và số lượng. Sau khi cập nhật giỏ hàng, phương thức sẽ lưu lại vào Session để đảm bảo rằng giỏ hàng được duy trì trong suốt phiên làm việc của người dùng.
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            if (quantity < 1) quantity = 1;
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductID == productId);
            if (item != null)
            {
                item.Quantity += quantity;
            }
            else
            {
                var product = await CatalogDataService.GetProductAsync(productId);
                if (product != null)
                {
                    cart.Add(new CartItem
                    {
                        ProductID = product.ProductID,
                        ProductName = product.ProductName,
                        Photo = product.Photo ?? "",
                        Unit = product.Unit,
                        Price = product.Price,
                        Quantity = quantity
                    });
                }
            }
            SaveCart(cart);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { count = cart.Sum(c => c.Quantity) });

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Cập nhật số lượng sản phẩm trong giỏ hàng. Khi người dùng thay đổi số lượng của một sản phẩm đã có trong giỏ hàng, phương thức này sẽ được gọi với ID sản phẩm và số lượng mới. Phương thức sẽ tìm mục tương ứng trong giỏ hàng và cập nhật số lượng. Nếu số lượng mới là 0 hoặc âm, mục đó sẽ bị xóa khỏi giỏ hàng. Sau khi cập nhật, phương thức sẽ lưu lại giỏ hàng vào Session để đảm bảo rằng các thay đổi được duy trì.
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            if (quantity > 999) quantity = 999;
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductID == productId);
            if (item != null)
            {
                if (quantity <= 0)
                    cart.Remove(item);
                else
                    item.Quantity = quantity;
            }
            SaveCart(cart);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Xoá sản phẩm khỏi giỏ hàng. Khi người dùng chọn xóa một sản phẩm khỏi giỏ hàng, phương thức này sẽ được gọi với ID sản phẩm cần xóa. Phương thức sẽ tìm tất cả mục trong giỏ hàng có ID sản phẩm tương ứng và loại bỏ chúng khỏi giỏ hàng. Sau khi xóa, phương thức sẽ lưu lại giỏ hàng vào Session để đảm bảo rằng các thay đổi được duy trì.
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult RemoveItem(int productId)
        {
            var cart = GetCart();
            cart.RemoveAll(c => c.ProductID == productId);
            SaveCart(cart);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Xoá toàn bộ giỏ hàng. Khi người dùng chọn xóa toàn bộ giỏ hàng, phương thức này sẽ được gọi để loại bỏ tất cả mục khỏi giỏ hàng. Phương thức sẽ tạo một danh sách giỏ hàng mới rỗng và lưu lại vào Session để đảm bảo rằng giỏ hàng đã được xóa hoàn toàn.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Clear()
        {
            SaveCart(new List<CartItem>());
            return RedirectToAction("Index");
        }
    }
}
