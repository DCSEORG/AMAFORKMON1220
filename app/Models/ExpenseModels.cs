using Microsoft.Data.SqlClient;
using Azure.Identity;

namespace app.Models
{
    public class Expense
    {
        public int ExpenseId { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime ExpenseDate { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; }
        public int? ManagerId { get; set; }
        public string? ManagerName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class User
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public int RoleId { get; set; }
        public string? RoleName { get; set; }
        public int? ManagerId { get; set; }
        public bool IsActive { get; set; }
    }

    public class ExpenseCategory
    {
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
    }

    public class ExpenseStatus
    {
        public int StatusId { get; set; }
        public string? StatusName { get; set; }
        public string? Description { get; set; }
    }
}
