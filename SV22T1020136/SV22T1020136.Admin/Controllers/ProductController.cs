using Microsoft.AspNetCore.Mvc;
using SV22T1020136.DataLayers;
using SV22T1020136.Models;

namespace SV22T1020136.Admin.Controllers
{
    public class ProductController : Controller
    {
        private readonly IConfiguration _configuration;

        public ProductController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index(string searchValue = "", int? supplierID = null, int? categoryID = null,
            decimal? minPrice = null, decimal? maxPrice = null, int page = 1, int pageSize = 0)
        {
            pageSize = pageSize > 0 ? pageSize : ApplicationContext.PageSize;
            ViewData["Title"] = "Quản lý Mặt Hàng";
            ViewBag.SearchValue = searchValue;
            ViewBag.SupplierID = supplierID;
            ViewBag.CategoryID = categoryID;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            // Get suppliers and categories for filter dropdowns
            var suppliers = SupplierDAL.GetAll(_configuration);
            var categories = DataLayers.CategoryDALHelpers.GetAll(_configuration);
            ViewBag.Suppliers = suppliers;
            ViewBag.Categories = categories;

            // Get products from database
            int rowCount = 0;
            var products = ProductDAL.List(_configuration, out rowCount, searchValue, supplierID, categoryID, minPrice, maxPrice, page, pageSize);

            // Get supplier and category names for each product
            var suppliersDict = suppliers.ToDictionary(s => s.SupplierID, s => s.SupplierName);
            var categoriesDict = categories.ToDictionary(c => c.CategoryID, c => c.CategoryName);

            var totalRecords = rowCount;
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            ViewBag.TotalRecords = totalRecords;
            ViewBag.TotalPages = totalPages;
            ViewBag.HasPrevious = page > 1;
            ViewBag.HasNext = page < totalPages;
            ViewBag.SupplierNames = suppliersDict;
            ViewBag.CategoryNames = categoriesDict;

            return View(products);
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            ViewData["Title"] = "Chi tiết Mặt Hàng";
            var product = ProductDAL.Get(_configuration, id);
            if (product == null)
            {
                return RedirectToAction("Index");
            }

            var supplier = SupplierDAL.Get(_configuration, product.SupplierID);
            var category = CategoryDALHelpers.Get(_configuration, product.CategoryID);
            var attributes = ProductAttributeDAL.GetByProductID(_configuration, id);

            ViewBag.SupplierName = supplier?.SupplierName ?? "";
            ViewBag.CategoryName = category?.CategoryName ?? "";
            ViewBag.Attributes = attributes;

            return View(product);
        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            ViewData["Title"] = id.HasValue ? "Chỉnh sửa Mặt Hàng" : "Thêm Mặt Hàng";

            var suppliers = SupplierDAL.GetAll(_configuration);
            var categories = DataLayers.CategoryDALHelpers.GetAll(_configuration);
            ViewBag.Suppliers = suppliers;
            ViewBag.Categories = categories;

            if (id.HasValue)
            {
                var product = ProductDAL.Get(_configuration, id.Value);
                if (product == null)
                {
                    return RedirectToAction("Index");
                }
                return View(product);
            }

            return View(new Product { IsSelling = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(Product product, IFormFile? uploadedMainPhoto)
        {
            if (string.IsNullOrWhiteSpace(product.ProductName))
                ModelState.AddModelError(nameof(product.ProductName), "Vui lòng nhập tên mặt hàng.");

            if (product.CategoryID <= 0)
                ModelState.AddModelError(nameof(product.CategoryID), "Vui lòng chọn loại hàng.");

            if (product.SupplierID <= 0)
                ModelState.AddModelError(nameof(product.SupplierID), "Vui lòng chọn nhà cung cấp.");

            if (string.IsNullOrWhiteSpace(product.Unit))
                ModelState.AddModelError(nameof(product.Unit), "Vui lòng nhập đơn vị tính.");

            if (product.Price <= 0)
                ModelState.AddModelError(nameof(product.Price), "Vui lòng nhập giá bán.");

            if (string.IsNullOrWhiteSpace(product.ProductDescription))
                ModelState.AddModelError(nameof(product.ProductDescription), "Vui lòng nhập mô tả.");

            if (!ModelState.IsValid)
            {
                var suppliers = SupplierDAL.GetAll(_configuration);
                var categories = DataLayers.CategoryDALHelpers.GetAll(_configuration);
                ViewBag.Suppliers = suppliers;
                ViewBag.Categories = categories;

                ViewData["Title"] = product.ProductID == 0 ? "Thêm Mặt Hàng" : "Chỉnh sửa Mặt Hàng";
                return View("Edit", product);
            }

            if (uploadedMainPhoto != null && uploadedMainPhoto.Length > 0)
            {
                product.Photo = await SaveUploadedProductImage(uploadedMainPhoto);
            }
            else if (!string.IsNullOrWhiteSpace(product.Photo))
            {
                product.Photo = product.Photo.Trim();
            }

            bool success;
            if (product.ProductID == 0)
            {
                success = ProductDAL.Add(_configuration, product);
                TempData["SuccessMessage"] = success ? "Thêm mặt hàng thành công!" : "Thêm mặt hàng thất bại!";
            }
            else
            {
                // Preserve existing photo when no new upload and no manual value.
                if (string.IsNullOrWhiteSpace(product.Photo))
                {
                    var existing = ProductDAL.Get(_configuration, product.ProductID);
                    if (existing != null)
                    {
                        product.Photo = existing.Photo;
                    }
                }
                success = ProductDAL.Update(_configuration, product);
                TempData["SuccessMessage"] = success ? "Cập nhật mặt hàng thành công!" : "Cập nhật mặt hàng thất bại!";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            ViewData["Title"] = "Xóa mặt hàng";
            var product = ProductDAL.Get(_configuration, id);
            if (product == null)
            {
                return RedirectToAction("Index");
            }

            var supplier = SupplierDAL.Get(_configuration, product.SupplierID);
            var category = CategoryDALHelpers.Get(_configuration, product.CategoryID);
            ViewBag.SupplierName = supplier?.SupplierName ?? "";
            ViewBag.CategoryName = category?.CategoryName ?? "";

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (ProductDAL.Delete(_configuration, id))
                TempData["SuccessMessage"] = "Xóa mặt hàng thành công!";

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult AddAttribute(int id)
        {
            ViewData["Title"] = "Thêm Thuộc Tính";
            var product = ProductDAL.Get(_configuration, id);
            if (product == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.ProductID = id;
            ViewBag.ProductName = product.ProductName;

            return View(new ProductAttribute { ProductID = id, DisplayOrder = 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveAttribute(ProductAttribute attribute)
        {
            if (string.IsNullOrEmpty(attribute.AttributeName))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập tên thuộc tính.");
                var product = ProductDAL.Get(_configuration, attribute.ProductID);
                ViewBag.ProductID = attribute.ProductID;
                ViewBag.ProductName = product?.ProductName ?? "";
                return View("AddAttribute", attribute);
            }

            if (ProductAttributeDAL.Add(_configuration, attribute))
            {
                TempData["SuccessMessage"] = "Thêm thuộc tính thành công!";
            }

            return RedirectToAction("Details", new { id = attribute.ProductID });
        }

        [HttpGet]
        public IActionResult EditAttribute(int id, int attributeId)
        {
            ViewData["Title"] = "Chỉnh sửa Thuộc Tính";
            var attribute = ProductAttributeDAL.Get(_configuration, attributeId);
            if (attribute == null || attribute.ProductID != id)
            {
                return RedirectToAction("Details", new { id });
            }

            var product = ProductDAL.Get(_configuration, id);
            ViewBag.ProductID = id;
            ViewBag.ProductName = product?.ProductName ?? "";

            return View(attribute);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateAttribute(ProductAttribute attribute)
        {
            if (string.IsNullOrEmpty(attribute.AttributeName))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập tên thuộc tính.");
                var product = ProductDAL.Get(_configuration, attribute.ProductID);
                ViewBag.ProductID = attribute.ProductID;
                ViewBag.ProductName = product?.ProductName ?? "";
                return View("EditAttribute", attribute);
            }

            if (ProductAttributeDAL.Update(_configuration, attribute))
            {
                TempData["SuccessMessage"] = "Cập nhật thuộc tính thành công!";
            }

            return RedirectToAction("Details", new { id = attribute.ProductID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAttribute(int id, int attributeId)
        {
            if (ProductAttributeDAL.Delete(_configuration, attributeId))
            {
                TempData["SuccessMessage"] = "Xóa thuộc tính thành công!";
            }

            return RedirectToAction("Details", new { id });
        }

        // ProductPhotos Actions
        [HttpGet]
        public IActionResult ListPhotos(int id)
        {
            ViewData["Title"] = "Quản lý Hình Ảnh";
            var product = ProductDAL.Get(_configuration, id);
            if (product == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.ProductID = id;
            ViewBag.ProductName = product.ProductName;
            ViewBag.MainPhoto = product.Photo;

            var photos = ProductPhotoDAL.GetByProductID(_configuration, id);
            return View(photos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetMainPhoto(int id, int photoId)
        {
            var product = ProductDAL.Get(_configuration, id);
            if (product == null)
            {
                return RedirectToAction("Index");
            }

            var photo = ProductPhotoDAL.Get(_configuration, photoId);
            if (photo == null || photo.ProductID != id || string.IsNullOrWhiteSpace(photo.Photo))
            {
                TempData["SuccessMessage"] = "Không thể đặt ảnh đại diện (hình ảnh không hợp lệ).";
                return RedirectToAction("ListPhotos", new { id });
            }

            if (ProductDAL.UpdateMainPhoto(_configuration, id, photo.Photo.Trim()))
            {
                TempData["SuccessMessage"] = "Đã đặt ảnh đại diện thành công!";
            }

            return RedirectToAction("ListPhotos", new { id });
        }

        [HttpGet]
        public IActionResult AddPhoto(int id)
        {
            ViewData["Title"] = "Thêm Hình Ảnh";
            var product = ProductDAL.Get(_configuration, id);
            if (product == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.ProductID = id;
            ViewBag.ProductName = product.ProductName;

            return View(new ProductPhoto { ProductID = id, DisplayOrder = 0, IsHidden = false });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePhoto(int productId, SV22T1020136.Models.ProductPhoto photoData, IFormFile? uploadedFile)
        {
            // Nếu ProductID từ form không có, lấy từ parameter
            if (photoData.ProductID == 0 && productId > 0)
            {
                photoData.ProductID = productId;
            }

            ArgumentNullException.ThrowIfNull(photoData);

            if (uploadedFile != null && uploadedFile.Length > 0)
            {
                photoData.Photo = await SaveUploadedProductImage(uploadedFile);
            }
            else
            {
                // URL mode: in some cases model binding may not populate photo.Photo (prefix mismatch, cached view, etc.)
                // So we read it directly from the posted form as a fallback.
                if (string.IsNullOrWhiteSpace(photoData.Photo))
                {
                    string? postedUrl = Request.Form["Photo"].FirstOrDefault();
                    if (string.IsNullOrWhiteSpace(postedUrl))
                    {
                        postedUrl = Request.Form["photo.Photo"].FirstOrDefault();
                    }
                    if (string.IsNullOrWhiteSpace(postedUrl))
                    {
                        postedUrl = Request.Form["PhotoUrl"].FirstOrDefault();
                    }

                    if (!string.IsNullOrWhiteSpace(postedUrl))
                    {
                        photoData.Photo = postedUrl.Trim();
                    }
                }

                // Trim whitespace from photo URL if no file uploaded
                if (!string.IsNullOrEmpty(photoData.Photo))
                {
                    photoData.Photo = photoData.Photo.Trim();
                }
            }

            // Validate ProductID
            if (photoData.ProductID <= 0)
            {
                ModelState.AddModelError(string.Empty, $"ProductID không hợp lệ: {photoData.ProductID}");
                ViewBag.ProductID = photoData.ProductID;
                ViewBag.ProductName = "";
                return View("AddPhoto", photoData);
            }

            // Validate Product exists
            var existingProduct = ProductDAL.Get(_configuration, photoData.ProductID);
            if (existingProduct == null)
            {
                ModelState.AddModelError(string.Empty, $"Không tìm thấy sản phẩm với ID: {photoData.ProductID}");
                ViewBag.ProductID = photoData.ProductID;
                ViewBag.ProductName = "";
                return View("AddPhoto", photoData);
            }

            if (string.IsNullOrWhiteSpace(photoData.Photo))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng chọn file ảnh hoặc nhập URL hình ảnh.");
                ViewBag.ProductID = photoData.ProductID;
                ViewBag.ProductName = existingProduct?.ProductName ?? "";
                return View("AddPhoto", photoData);
            }

            if (ProductPhotoDAL.Add(_configuration, photoData))
            {
                TempData["SuccessMessage"] = "Thêm hình ảnh thành công!";
            }

            return RedirectToAction("ListPhotos", new { id = photoData.ProductID });
        }

        [HttpGet]
        public IActionResult EditPhoto(int id, int photoId)
        {
            ViewData["Title"] = "Chỉnh sửa Hình Ảnh";
            var photo = ProductPhotoDAL.Get(_configuration, photoId);
            if (photo == null || photo.ProductID != id)
            {
                return RedirectToAction("ListPhotos", new { id });
            }

            var product = ProductDAL.Get(_configuration, id);
            ViewBag.ProductID = id;
            ViewBag.ProductName = product?.ProductName ?? "";

            return View(photo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePhoto(SV22T1020136.Models.ProductPhoto photoData, IFormFile? uploadedFile)
        {
            photoData.Description = (photoData.Description ?? string.Empty).Trim();

            // Trim whitespace from photo URL
            if (!string.IsNullOrEmpty(photoData.Photo))
            {
                photoData.Photo = photoData.Photo.Trim();
            }

            // Handle file upload
            if (uploadedFile != null && uploadedFile.Length > 0)
            {
                photoData.Photo = await SaveUploadedProductImage(uploadedFile);
            }

            if (string.IsNullOrWhiteSpace(photoData.Photo))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng chọn file ảnh hoặc nhập URL hình ảnh.");
                var product = ProductDAL.Get(_configuration, photoData.ProductID);
                ViewBag.ProductID = photoData.ProductID;
                ViewBag.ProductName = product?.ProductName ?? "";
                return View("EditPhoto", photoData);
            }

            if (!ProductPhotoDAL.Update(_configuration, photoData))
            {
                return RedirectToAction("ListPhotos", new { id = photoData.ProductID });
            }
            TempData["SuccessMessage"] = "Cập nhật hình ảnh thành công!";

            return RedirectToAction("ListPhotos", new { id = photoData.ProductID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePhoto(int id, int photoId)
        {
            if (ProductPhotoDAL.Delete(_configuration, photoId))
            {
                TempData["SuccessMessage"] = "Xóa hình ảnh thành công!";
            }

            return RedirectToAction("ListPhotos", new { id });
        }

        private static async Task<string> SaveUploadedProductImage(IFormFile uploadedFile)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
            Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(uploadedFile.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await uploadedFile.CopyToAsync(fileStream);
            }

            return $"/images/products/{uniqueFileName}";
        }
    }
}
