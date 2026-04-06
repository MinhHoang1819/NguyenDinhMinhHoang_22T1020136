namespace SV22T1020136.Models.Sales
{
    /// <summary>
    /// Thông tin chi tiết của mặt hàng được bán trong đơn hàng
    /// </summary>
    /// <param name="OrderID"> Mã đơn hàng </param>
    /// <param name="ProductID"> Mã mặt hàng </param>
    /// <param name="Quantity"> Số lượng </param>
    /// <param name="SalePrice"> Giá bán </param>
    public record OrderDetailRecord(int OrderID, int ProductID, int Quantity, decimal SalePrice)
    {
        /// <summary>
        /// Tổng số tiền
        /// </summary>
        public decimal TotalPrice => Quantity * SalePrice;
    }
}
