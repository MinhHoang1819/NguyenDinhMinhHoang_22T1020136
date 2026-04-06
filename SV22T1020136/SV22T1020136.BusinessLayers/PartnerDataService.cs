using SV22T1020136.DataLayers.Interfaces;
using SV22T1020136.DataLayers.SQLServer;
using SV22T1020136.Models.Common;
using SV22T1020136.Models.Partner;

namespace SV22T1020136.BusinessLayers
{
    /// <summary>
    /// Cung cấp các chức năng xử lý dữ liệu liên quan đến đối tác của hệ thống,
    /// bao gồm: nhà cung cấp (Supplier), khách hàng (Customer) và người giao hàng (Shipper).
    /// </summary>
    public static class PartnerDataService
    {
        private static readonly ISupplierRepository supplierDB;
        private static readonly ICustomerRepository customerDB;
        private static readonly IGenericRepository<Shipper> shipperDB;

        /// <summary>
        /// Constructor
        /// </summary>
        static PartnerDataService()
        {
            supplierDB = new SupplierRepository(Configuration.ConnectionString);
            customerDB = new CustomerRepository(Configuration.ConnectionString);
            shipperDB = (IGenericRepository<Shipper>)new ShipperRepository(Configuration.ConnectionString);
        }

        #region Supplier

        /// <summary>
        /// Tìm kiếm và lấy danh sách nhà cung cấp dưới dạng phân trang.
        /// </summary>
        /// <param name="input">
        /// Thông tin tìm kiếm và phân trang (từ khóa tìm kiếm, trang cần hiển thị, số dòng mỗi trang).
        /// </param>
        /// <returns>
        /// Kết quả tìm kiếm dưới dạng danh sách nhà cung cấp có phân trang.
        /// </returns>
        public static async Task<PagedResult<Supplier>> ListSupplierAsync(PaginationSearchInput input)
        {
            return await supplierDB.ListAsync(input);
        }

        /// <summary>
        /// Tìm kiếm và lấy danh sách nhà cung cấp dưới dạng phân trang (bí danh của ListSupplierAsync).
        /// </summary>
        /// <param name="input">Thông tin tìm kiếm và phân trang.</param>
        /// <returns>Kết quả tìm kiếm dưới dạng danh sách nhà cung cấp có phân trang.</returns>
        public static async Task<PagedResult<Supplier>> ListSuppliersAsync(PaginationSearchInput input)
        {
            return await ListSupplierAsync(input);
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một nhà cung cấp dựa vào mã nhà cung cấp.
        /// </summary>
        /// <param name="supplierID">Mã nhà cung cấp cần tìm.</param>
        /// <returns>
        /// Đối tượng Supplier nếu tìm thấy, ngược lại trả về null.
        /// </returns>
        public static async Task<Supplier?> GetSupplierAsync(int supplierID)
        {
            return await supplierDB.GetAsync(supplierID);
        }

        /// <summary>
        /// Bổ sung một nhà cung cấp mới vào hệ thống.
        /// </summary>
        /// <param name="data">Thông tin nhà cung cấp cần bổ sung.</param>
        /// <returns>Mã nhà cung cấp được tạo mới.</returns>
        public static async Task<int> AddSupplierAsync(Supplier data)
        {
            return await supplierDB.AddAsync(data);
        }

        /// <summary>
        /// Cập nhật thông tin của một nhà cung cấp.
        /// </summary>
        /// <param name="data">Thông tin nhà cung cấp cần cập nhật.</param>
        /// <returns>
        /// True nếu cập nhật thành công, ngược lại False.
        /// </returns>
        public static async Task<bool> UpdateSupplierAsync(Supplier data)
        {
            return await supplierDB.UpdateAsync(data);
        }

        /// <summary>
        /// Xóa một nhà cung cấp dựa vào mã nhà cung cấp.
        /// </summary>
        /// <param name="supplierID">Mã nhà cung cấp cần xóa.</param>
        /// <returns>
        /// True nếu xóa thành công, False nếu nhà cung cấp đang được sử dụng
        /// hoặc việc xóa không thực hiện được.
        /// </returns>
        public static async Task<bool> DeleteSupplierAsync(int supplierID)
        {
            if (await supplierDB.IsUsed(supplierID))
                return false;
            return await supplierDB.DeleteAsync(supplierID);
        }

        /// <summary>
        /// Kiểm tra xem một nhà cung cấp có đang được sử dụng trong dữ liệu hay không.
        /// </summary>
        /// <param name="supplierID">Mã nhà cung cấp cần kiểm tra.</param>
        /// <returns>
        /// True nếu nhà cung cấp đang được sử dụng, ngược lại False.
        /// </returns>
        public static async Task<bool> IsUsedSupplierAsync(int supplierID)
        {
            return await supplierDB.IsUsed(supplierID);
        }

        #endregion

        #region Customer

        /// <summary>
        /// Tìm kiếm và lấy danh sách khách hàng dưới dạng phân trang.
        /// </summary>
        /// <param name="input">
        /// Thông tin tìm kiếm và phân trang (từ khóa tìm kiếm, trang cần hiển thị, số dòng mỗi trang).
        /// </param>
        /// <returns>
        /// Kết quả tìm kiếm dưới dạng danh sách khách hàng có phân trang.
        /// </returns>
        public static async Task<PagedResult<Customer>> ListCustomerAsync(PaginationSearchInput input)
        {
            return await customerDB.ListAsync(input);
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một khách hàng dựa vào mã khách hàng.
        /// </summary>
        /// <param name="customerID">Mã khách hàng cần tìm.</param>
        /// <returns>
        /// Đối tượng Customer nếu tìm thấy, ngược lại trả về null.
        /// </returns>
        public static async Task<Customer?> GetCustomerAsync(int customerID)
        {
            return await customerDB.GetAsync(customerID);
        }

        /// <summary>
        /// Bổ sung một khách hàng mới vào hệ thống.
        /// </summary>
        /// <param name="data">Thông tin khách hàng cần bổ sung.</param>
        /// <returns>Mã khách hàng được tạo mới.</returns>
        public static async Task<int> AddCustomerAsync(Customer data)
        {
            return await customerDB.AddAsync(data);
        }

        /// <summary>
        /// Cập nhật thông tin của một khách hàng.
        /// </summary>
        /// <param name="data">Thông tin khách hàng cần cập nhật.</param>
        /// <returns>
        /// True nếu cập nhật thành công, ngược lại False.
        /// </returns>
        public static async Task<bool> UpdateCustomerAsync(Customer data)
        {
            return await customerDB.UpdateAsync(data);
        }

        /// <summary>
        /// Xóa một khách hàng dựa vào mã khách hàng.
        /// </summary>
        /// <param name="customerID">Mã khách hàng cần xóa.</param>
        /// <returns>
        /// True nếu xóa thành công, ngược lại False.
        /// </returns>
        public static async Task<bool> DeleteCustomerAsync(int customerID)
        {
            return await customerDB.DeleteAsync(customerID);
        }

        /// <summary>
        /// Kiểm tra xem khách hàng có đang được sử dụng trong dữ liệu hay không (ví dụ: có tồn tại đơn hàng).
        /// </summary>
        /// <param name="customerID">Mã khách hàng cần kiểm tra.</param>
        /// <returns>
        /// True nếu khách hàng đang được sử dụng, ngược lại False.
        /// </returns>
        public static async Task<bool> IsUsedCustomerAsync(int customerID)
        {
            return await customerDB.IsUsedAsync(customerID);
        }

        /// <summary>
        /// Kiểm tra xem email của khách hàng có hợp lệ không
        /// (không bị trùng với email của khách hàng khác).
        /// </summary>
        /// <param name="email">Địa chỉ email cần kiểm tra.</param>
        /// <param name="customerID">
        /// Nếu customerID = 0: kiểm tra email đối với khách hàng mới.
        /// Nếu customerID khác 0: kiểm tra email của khách hàng có mã là customerID.
        /// </param>
        /// <returns>
        /// True nếu email hợp lệ (không trùng), ngược lại False.
        /// </returns>
        public static async Task<bool> ValidateCustomerEmailAsync(string email, int customerID = 0)
        {
            return await customerDB.ValidateEmailAsync(email, customerID);
        }

        /// <summary>
        /// Đổi mật khẩu của khách hàng.
        /// </summary>
        /// <param name="customerID">Mã khách hàng.</param>
        /// <param name="newPassword">Mật khẩu mới.</param>
        /// <returns>
        /// True nếu đổi mật khẩu thành công, ngược lại False.
        /// </returns>
        public static async Task<bool> ChangeCustomerPasswordAsync(int customerID, string newPassword)
        {
            return await customerDB.ChangePasswordAsync(customerID, newPassword);
        }
        /// <summary>
        /// Thử xác thực (đăng nhập) khách hàng bằng email và mật khẩu.
        /// </summary>
        /// <param name="email">Địa chỉ email sử dụng làm tên đăng nhập.</param>
        /// <param name="password">
        /// Mật khẩu ở dạng plain-text do người dùng cung cấp.
        /// Phần xử lý (repository) phải so sánh an toàn với giá trị hash đã lưu (không lưu hoặc so sánh mật khẩu dạng plain-text).
        /// </param>
        /// <returns>
        /// Task trả về đối tượng <see cref="Customer"/> khi thông tin hợp lệ (xác thực thành công);
        /// nếu email hoặc mật khẩu không đúng thì trả về null.
        /// </returns>
        /// <remarks>
        /// Phương thức này chỉ chuyển tiếp yêu cầu tới lớp dữ liệu (repository) — <c>customerDB.AuthorizeAsync</c>.
        /// Repository phải xử lý xác thực một cách bảo mật (hashing, salted hash, chống timing attack).
        /// </remarks>
        public static async Task<Customer?> AuthorizeCustomerAsync(string email, string password)
        {
            return await customerDB.AuthorizeAsync(email, password);
        }

        #endregion

        #region Shipper

        /// <summary>
        /// Tìm kiếm và lấy danh sách người giao hàng dưới dạng phân trang.
        /// </summary>
        /// <param name="input">
        /// Thông tin tìm kiếm và phân trang (từ khóa tìm kiếm, trang cần hiển thị, số dòng mỗi trang).
        /// </param>
        /// <returns>
        /// Kết quả tìm kiếm dưới dạng danh sách người giao hàng có phân trang.
        /// </returns>
        public static async Task<PagedResult<Shipper>> ListShipperAsync(PaginationSearchInput input)
        {
            return await shipperDB.ListAsync(input);
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một người giao hàng dựa vào mã người giao hàng.
        /// </summary>
        /// <param name="shipperID">Mã người giao hàng cần tìm.</param>
        /// <returns>
        /// Đối tượng Shipper nếu tìm thấy, ngược lại trả về null.
        /// </returns>
        public static async Task<Shipper?> GetShipperAsync(int shipperID)
        {
            return await shipperDB.GetAsync(shipperID);
        }

        /// <summary>
        /// Bổ sung một người giao hàng mới vào hệ thống.
        /// </summary>
        /// <param name="data">Thông tin người giao hàng cần bổ sung.</param>
        /// <returns>Mã người giao hàng được tạo mới.</returns>
        public static async Task<int> AddShipperAsync(Shipper data)
        {
            return await shipperDB.AddAsync(data);
        }

        /// <summary>
        /// Cập nhật thông tin của một người giao hàng.
        /// </summary>
        /// <param name="data">Thông tin người giao hàng cần cập nhật.</param>
        /// <returns>
        /// True nếu cập nhật thành công, ngược lại False.
        /// </returns>
        public static async Task<bool> UpdateShipperAsync(Shipper data)
        {
            return await shipperDB.UpdateAsync(data);
        }

        /// <summary>
        /// Xóa một người giao hàng dựa vào mã người giao hàng.
        /// </summary>
        /// <param name="shipperID">Mã người giao hàng cần xóa.</param>
        /// <returns>
        /// True nếu xóa thành công, ngược lại False.
        /// </returns>
        public static async Task<bool> DeleteShipperAsync(int shipperID)
        {
            return await shipperDB.DeleteAsync(shipperID);
        }

        #endregion
    }
}
