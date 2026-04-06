namespace SV22T1020136.Models.Sales
{
    public class OrderDetail
    {
        public OrderDetail()
        {
        }

        public OrderDetail(object value1, object value2, object value3, object value4)
        {
            if (value1 is not null && value1 != DBNull.Value)
                OrderID = Convert.ToInt32(value1);
            if (value2 is not null && value2 != DBNull.Value)
                ProductID = Convert.ToInt32(value2);
            if (value3 is not null && value3 != DBNull.Value)
                Quantity = Convert.ToInt32(value3);
            if (value4 is not null && value4 != DBNull.Value)
                SalePrice = Convert.ToDecimal(value4);
        }

        public int OrderID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public decimal SalePrice { get; set; }
        public decimal ToTalPrice => Quantity * SalePrice;
    }
}

