namespace SV22T1020136.Models.Sales
{
    /// <summary>
    /// DTO hiển thị thông tin chi tiết của mặt hàng trong đơn hàng
    /// </summary>
    public class OrderDetailViewInfo : OrderDetail
    {
        public OrderDetailViewInfo()
        {
        }

        public OrderDetailViewInfo(object value1, object value2, object value3, object value4) : base(value1, value2, value3, value4)
        {
        }

        /// <summary>
        /// Tên hàng
        /// </summary>
        public string ProductName { get; set; } = "";
        /// <summary>
        /// Đơn vị tính
        /// </summary>
        public string Unit { get; set; } = "";
        /// <summary>
        /// Tên file ảnh
        /// </summary>
        public string Photo { get; set; } = "";
    }
}
