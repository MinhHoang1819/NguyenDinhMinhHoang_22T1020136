using SV22T1020136.Models.Sales;

namespace SV22T1020136.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public int? CustomerID { get; set; }
        public string? CustomerName { get; set; }
        public DateTime OrderTime { get; set; }
        public string? DeliveryProvince { get; set; }
        public string? DeliveryAddress { get; set; }
        public int? EmployeeID { get; set; }
        public DateTime? AcceptTime { get; set; }
        public int? ShipperID { get; set; }
        public string? ShipperName { get; set; }
        public DateTime? ShippedTime { get; set; }
        public DateTime? FinishedTime { get; set; }
        public string? Status { get; set; }
        public string? StatusDescription { get; set; }

        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}

