using Microsoft.Data.SqlClient;
using Azure.Identity;
using app.Models;

namespace app.Services
{
    public class DatabaseService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DatabaseService> _logger;
        public string? LastError { get; private set; }
        public string? LastErrorLocation { get; private set; }

        public DatabaseService(IConfiguration configuration, ILogger<DatabaseService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private SqlConnection GetConnection()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            }

            return new SqlConnection(connectionString);
        }

        private List<Expense> GetDummyExpenses()
        {
            return new List<Expense>
            {
                new Expense
                {
                    ExpenseId = 1,
                    UserId = 1,
                    UserName = "John Doe",
                    CategoryId = 1,
                    CategoryName = "Travel",
                    Amount = 150.00m,
                    Description = "Train ticket to London",
                    ExpenseDate = DateTime.Now.AddDays(-5),
                    StatusId = 1,
                    StatusName = "Pending",
                    CreatedAt = DateTime.Now.AddDays(-5)
                },
                new Expense
                {
                    ExpenseId = 2,
                    UserId = 1,
                    UserName = "John Doe",
                    CategoryId = 2,
                    CategoryName = "Meals",
                    Amount = 45.50m,
                    Description = "Business lunch",
                    ExpenseDate = DateTime.Now.AddDays(-3),
                    StatusId = 2,
                    StatusName = "Approved",
                    CreatedAt = DateTime.Now.AddDays(-3)
                },
                new Expense
                {
                    ExpenseId = 3,
                    UserId = 2,
                    UserName = "Jane Smith",
                    CategoryId = 3,
                    CategoryName = "Office Supplies",
                    Amount = 25.00m,
                    Description = "Stationery",
                    ExpenseDate = DateTime.Now.AddDays(-2),
                    StatusId = 1,
                    StatusName = "Pending",
                    CreatedAt = DateTime.Now.AddDays(-2)
                }
            };
        }

        public async Task<List<Expense>> GetExpensesAsync()
        {
            try
            {
                LastError = null;
                LastErrorLocation = null;

                using var connection = GetConnection();
                await connection.OpenAsync();

                var query = @"
                    SELECT 
                        e.ExpenseId, e.UserId, u.UserName, e.CategoryId, 
                        ec.CategoryName, e.Amount, e.Description, 
                        e.ExpenseDate, e.StatusId, es.StatusName,
                        e.ManagerId, m.UserName as ManagerName,
                        e.CreatedAt, e.UpdatedAt
                    FROM dbo.Expenses e
                    INNER JOIN dbo.Users u ON e.UserId = u.UserId
                    INNER JOIN dbo.ExpenseCategories ec ON e.CategoryId = ec.CategoryId
                    INNER JOIN dbo.ExpenseStatus es ON e.StatusId = es.StatusId
                    LEFT JOIN dbo.Users m ON e.ManagerId = m.UserId
                    ORDER BY e.CreatedAt DESC";

                using var command = new SqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                var expenses = new List<Expense>();
                while (await reader.ReadAsync())
                {
                    expenses.Add(new Expense
                    {
                        ExpenseId = reader.GetInt32(0),
                        UserId = reader.GetInt32(1),
                        UserName = reader.GetString(2),
                        CategoryId = reader.GetInt32(3),
                        CategoryName = reader.GetString(4),
                        Amount = reader.GetDecimal(5),
                        Description = reader.IsDBNull(6) ? null : reader.GetString(6),
                        ExpenseDate = reader.GetDateTime(7),
                        StatusId = reader.GetInt32(8),
                        StatusName = reader.GetString(9),
                        ManagerId = reader.IsDBNull(10) ? null : reader.GetInt32(10),
                        ManagerName = reader.IsDBNull(11) ? null : reader.GetString(11),
                        CreatedAt = reader.GetDateTime(12),
                        UpdatedAt = reader.IsDBNull(13) ? null : reader.GetDateTime(13)
                    });
                }

                return expenses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching expenses from database");
                
                // Set detailed error message
                LastError = $"Database Connection Error: {ex.Message}";
                
                if (ex is SqlException sqlEx)
                {
                    LastError += $" | SQL Error Code: {sqlEx.Number}";
                    
                    if (sqlEx.Number == 18456 || sqlEx.Number == 0)
                    {
                        var connectionString = _configuration.GetConnectionString("DefaultConnection");
                        if (connectionString?.Contains("Managed Identity") == true)
                        {
                            LastError += " | Managed Identity authentication failed. ";
                            LastError += "Fix: Ensure the managed identity has been granted db_datareader and db_datawriter roles on the database. ";
                            LastError += "Run the script.sql file using run-sql-dbrole.py to configure permissions.";
                        }
                    }
                }
                
                LastErrorLocation = "DatabaseService.GetExpensesAsync (line 111)";
                
                // Return dummy data on error
                return GetDummyExpenses();
            }
        }

        public async Task<List<User>> GetUsersAsync()
        {
            try
            {
                LastError = null;
                LastErrorLocation = null;

                using var connection = GetConnection();
                await connection.OpenAsync();

                var query = @"
                    SELECT u.UserId, u.UserName, u.Email, u.RoleId, 
                           r.RoleName, u.ManagerId, u.IsActive
                    FROM dbo.Users u
                    INNER JOIN dbo.Roles r ON u.RoleId = r.RoleId
                    WHERE u.IsActive = 1";

                using var command = new SqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                var users = new List<User>();
                while (await reader.ReadAsync())
                {
                    users.Add(new User
                    {
                        UserId = reader.GetInt32(0),
                        UserName = reader.GetString(1),
                        Email = reader.GetString(2),
                        RoleId = reader.GetInt32(3),
                        RoleName = reader.GetString(4),
                        ManagerId = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                        IsActive = reader.GetBoolean(6)
                    });
                }

                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching users from database");
                LastError = $"Database Error: {ex.Message}";
                LastErrorLocation = "DatabaseService.GetUsersAsync (line 167)";
                return new List<User>();
            }
        }

        public async Task<List<ExpenseCategory>> GetCategoriesAsync()
        {
            try
            {
                LastError = null;
                LastErrorLocation = null;

                using var connection = GetConnection();
                await connection.OpenAsync();

                var query = "SELECT CategoryId, CategoryName, Description FROM dbo.ExpenseCategories";

                using var command = new SqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                var categories = new List<ExpenseCategory>();
                while (await reader.ReadAsync())
                {
                    categories.Add(new ExpenseCategory
                    {
                        CategoryId = reader.GetInt32(0),
                        CategoryName = reader.GetString(1),
                        Description = reader.IsDBNull(2) ? null : reader.GetString(2)
                    });
                }

                return categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching categories from database");
                LastError = $"Database Error: {ex.Message}";
                LastErrorLocation = "DatabaseService.GetCategoriesAsync (line 208)";
                return new List<ExpenseCategory>();
            }
        }
    }
}
