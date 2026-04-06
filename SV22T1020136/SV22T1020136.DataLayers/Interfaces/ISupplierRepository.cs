using SV22T1020136.Models.Common;
using SV22T1020136.Models.Partner;

namespace SV22T1020136.DataLayers.Interfaces
{
    /// <summary>
    /// Định nghĩa các phép xử lý dữ liệu liên quan đến nhà cung cấp
    /// </summary>
    public interface ISupplierRepository
    {
        /// <summary>
        /// Tìm kiếm và lấy danh sách nhà cung cấp dưới dạng phân trang
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<PagedResult<Supplier>> ListAsync(PaginationSearchInput input);
        /// <summary>
        /// Lấy thông tin của một nhà cung cấp
        /// </summary>
        /// <param name="id">Mã nhà cung cấp</param>
        /// <returns></returns>
        Task<Supplier?> GetAsync(int id);
        /// <summary>
        /// Thêm một nhà cung cấp mới vào CSDL
        /// </summary>
        /// <param name="data"></param>
        /// <returns>Mã nhà cung cấp được bổ sung (lấy từ giá trị IDENTITY)</returns>
        Task<int> AddAsync(Supplier data);
        /// <summary>
        /// Cập nhật thông tin nhà cung cấp
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<bool> UpdateAsync(Supplier data);
        /// <summary>
        /// Xóa nhà cung cấp
        /// </summary>
        /// <param name="id">Mã nhà cung cấp cần xóa</param>
        /// <returns></returns>
        Task<bool> DeleteAsync(int id);
        /// <summary>
        /// Kiểm tra xem nhà cung cấp có dữ liệu liên quan hay không?
        /// </summary>
        /// <param name="id">Mã nhà cung cấp</param>
        /// <returns></returns>
        Task<bool> IsUsed(int id);
    }
}