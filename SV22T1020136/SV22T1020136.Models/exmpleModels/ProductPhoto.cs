namespace SV22T1020136.Models
{
    public class ProductPhoto
    {
        public int PhotoID { get; set; }
        public int ProductID { get; set; }
        public string Photo { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsHidden { get; set; }
    }
}
