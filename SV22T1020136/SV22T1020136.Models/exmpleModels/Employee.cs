namespace SV22T1020136.Models
{
    public class Employee
    {
        public int EmployeeID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Photo { get; set; } = string.Empty;
        public bool IsWorking { get; set; }
        public string RoleNames { get; set; } = string.Empty;
    }
}
