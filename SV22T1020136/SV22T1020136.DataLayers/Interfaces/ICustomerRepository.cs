using SV22T1020136.Models.Partner;

namespace SV22T1020136.DataLayers.Interfaces
{
    /// <summary>
    /// Định nghĩa các phép xử lý dữ liệu trên Customer
    /// </summary>
    public interface ICustomerRepository : IGenericRepository<Customer>
    {
        /// <summary>
        /// Kiểm tra xem một địa chỉ email có hợp lệ hay không?
        /// </summary>
        /// <param name="email">Email cần kiểm tra</param>
        /// <param name="id">
        /// Nếu id = 0: Kiểm tra email của khách hàng mới.
        /// Nếu id <> 0: Kiểm tra email đối với khách hàng đã tồn tại
        /// </param>
        /// <returns></returns>
        Task<bool> ValidateEmailAsync(string email, int id = 0);
        /// <summary>
        /// Đổi mật khẩu cho khách hàng
        /// </summary>
        /// <param name="customerID"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        Task<bool> ChangePasswordAsync(int customerID, string newPassword);
        /// <summary>
        /// Thử xác thực (đăng nhập) khách hàng bằng email và mật khẩu.
        /// 
        /// Mô tả:
        /// - <paramref name="email"/>: địa chỉ email dùng làm tên đăng nhập.
        /// - <paramref name="password"/>: mật khẩu ở dạng plain-text do người dùng cung cấp.
        /// 
        /// Yêu cầu triển khai:
        /// - Hàm triển khai phải kiểm tra mật khẩu này đối chiếu với giá trị đã lưu (ví dụ so sánh với hash mật khẩu).
        /// - Việc so sánh phải thực hiện an toàn (so sánh hash, không lưu hoặc trả về mật khẩu plain-text).
        /// 
        /// Giá trị trả về:
        /// - Trả về đối tượng <see cref="Customer"/> khi thông tin hợp lệ (xác thực thành công).
        /// - Trả về null khi email hoặc mật khẩu không hợp lệ.
        /// </summary>
        Task<Customer?> AuthorizeAsync(string email, string password);
    }
}
