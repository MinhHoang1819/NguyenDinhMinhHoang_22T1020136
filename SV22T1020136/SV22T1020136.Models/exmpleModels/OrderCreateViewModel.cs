using System.ComponentModel.DataAnnotations;

namespace SV22T1020136.Models
{
    public class OrderCreateViewModel
    {
        [Required]
        public int CustomerID { get; set; }

        public string? DeliveryAddress { get; set; }

        public string? DeliveryProvince { get; set; }
    }
}

