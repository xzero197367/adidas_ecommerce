
using System.Text;
using Adidas.Application.Contracts.ServicesContracts.People;
using Adidas.Context;
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Operation.ReviewDTOs.Query;
using Adidas.DTOs.People.Customer_DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models.People;

namespace Adidas.Application.Services.People
{
    public class CustomerService : ICustomerService
    {
        private readonly UserManager<User> _userManager;
        private readonly AdidasDbContext _context;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(UserManager<User> userManager, ILogger<CustomerService> logger, AdidasDbContext context)
        {
            _userManager = userManager;
            _logger = logger;
            _context = context;
        }

        public async Task<OperationResult<PagedResultDto<CustomerDto>>> GetCustomersAsync(CustomerFilterDto filter)
        {
            try
            {
                var query = _context.Users
                    .Where(u => u.Role == UserRole.Customer)
                    .Include(u => u.Orders)
                    .AsQueryable();

                // Apply search filter
                if (!string.IsNullOrEmpty(filter.Search))
                {
                    var searchLower = filter.Search.ToLower();
                    query = query.Where(u =>
                        u.UserName.ToLower().Contains(searchLower) ||
                        u.Email.ToLower().Contains(searchLower));
                }

                // Apply status filter
                if (!string.IsNullOrEmpty(filter.Status) && filter.Status != "All Status")
                {
                    var isActive = filter.Status == "Active";
                    query = query.Where(u => u.IsActive == isActive);
                }


                var totalCount = await query.CountAsync();

                var customers = await query
                    .OrderByDescending(u => u.CreatedAt)
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(u => new CustomerDto
                    {
                        Id = u.Id,
                        Name = u.UserName,
                        Email = u.Email,
                        JoinDate = u.CreatedAt,
                        IsActive = u.IsActive,
                        Status = u.IsActive ? "Active" : "Suspended",
                        TotalSpent = u.Orders.Sum(o => o.TotalAmount),
                        Phone = u.Phone,
                        Gender = u.Gender.ToString(),
                        DateOfBirth = u.DateOfBirth,
                        PreferredLanguage = u.PreferredLanguage,
                        TotalOrders = u.Orders.Count,
                    })
                    .ToListAsync();

                var result = new PagedResultDto<CustomerDto>
                {
                    Items = customers,
                    TotalCount = totalCount,
                    PageNumber = filter.Page,
                    PageSize = filter.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize)
                };
                return OperationResult<PagedResultDto<CustomerDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customers");
                return OperationResult<PagedResultDto<CustomerDto>>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<CustomerDetailsDto>> GetCustomerByIdAsync(string id)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Addresses)
                    .Include(u => u.Orders)
                    .FirstOrDefaultAsync(u => u.Id == id && u.Role == UserRole.Customer);

                if (user == null) return null;

                // ✅ Fetch Reviews
                var reviews = await _context.Reviews
                    .Where(r => r.UserId == id && !r.IsDeleted)
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => new ReviewDto
                    {
                        Id = r.Id,
                        ProductId = r.ProductId,
                        UserId = r.UserId,
                        Rating = r.Rating,
                        ReviewText = r.ReviewText,
                        CreatedAt = r.CreatedAt ?? DateTime.MinValue
                    })
                    .ToListAsync();

                var result = new CustomerDetailsDto
                {
                    Id = user.Id,
                    Name = user.UserName,
                    Email = user.Email,
                    Phone = user.Phone,
                    Gender = user.Gender,
                    DateOfBirth = user.DateOfBirth,
                    PreferredLanguage = user.PreferredLanguage,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    Addresses = user.Addresses.Where(a => !a.IsDeleted).ToList(),
                    Orders = user.Orders.ToList(),
                    Reviews = reviews,
                    TotalOrders = user.Orders.Count,
                    TotalSpent = user.Orders.Sum(o => o.TotalAmount)
                };
                return OperationResult<CustomerDetailsDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer details");
                return OperationResult<CustomerDetailsDto>.Fail(ex.Message);
            }
        }


        public async Task<OperationResult<bool>> UpdateCustomerAsync(string id, CustomerUpdateDto customerUpdateDto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null || user.Role != UserRole.Customer)
                {
                    return OperationResult<bool>.Fail("Customer not found or not a customer"); // Customer not found();
                }

                user.Phone = customerUpdateDto.Phone;
                user.Gender = customerUpdateDto.Gender;
                user.DateOfBirth = customerUpdateDto.DateOfBirth;
                user.PreferredLanguage = customerUpdateDto.PreferredLanguage;
                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                return OperationResult<bool>.Success(result.Succeeded);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer");
                return OperationResult<bool>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<bool>> ToggleCustomerStatusAsync(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null || user.Role != UserRole.Customer)
                {
                    return OperationResult<bool>.Fail("Customer not found or not a customer");
                }

                user.IsActive = !user.IsActive;
                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                return OperationResult<bool>.Success(result.Succeeded);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling customer status");
                return OperationResult<bool>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<bool>> DeleteCustomerAsync(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null || user.Role != UserRole.Customer)
                {
                    return OperationResult<bool>.Fail("Customer not found or not a customer");
                }

                var result = await _userManager.DeleteAsync(user);
                return OperationResult<bool>.Success(result.Succeeded);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer");
                return OperationResult<bool>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<byte[]>> ExportCustomersAsync(CustomerFilterDto filter)
        {
            try
            {
                var customers = await GetCustomersAsync(new CustomerFilterDto
                {
                    Search = filter.Search,
                    Status = filter.Status,
                    Page = 1,
                    PageSize = int.MaxValue
                });

                var csv = new StringBuilder();
                csv.AppendLine("Name,Email,Join Date,Status,Membership Tier,Total Spent,Phone");

                foreach (var customer in customers.Data.Items)
                {
                    csv.AppendLine(
                        $"{customer.Name},{customer.Email},{customer.JoinDate:yyyy-MM-dd},{customer.Status},{customer.TotalSpent:C},{customer.Phone}");
                }
                return OperationResult<byte[]>.Success(Encoding.UTF8.GetBytes(csv.ToString()));

                // return Encoding.UTF8.GetBytes(csv.ToString());
            }catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting customers");
                return OperationResult<byte[]>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<IEnumerable<Order>>> GetOrdersByUserIdAsync(string userId)
        {
            try
            {
                // ✅ Check 1: Validate input
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return OperationResult<IEnumerable<Order>>.Fail("User ID cannot be null or empty.");
                }

                // ✅ Check 2: Ensure user exists and is a customer
                var user = await _context.Users
                    .Include(u => u.Orders)
                    .FirstOrDefaultAsync(u => u.Id == userId && u.Role == UserRole.Customer);

                if (user == null)
                {
                    return OperationResult<IEnumerable<Order>>.Fail("Customer not found.");
                }

                // ✅ Check 3: Return empty list if no orders
                if (user.Orders == null || !user.Orders.Any())
                {
                    return OperationResult<IEnumerable<Order>>.Fail("No orders found.");
                }

                return OperationResult<IEnumerable<Order>>.Success(user.Orders);
            }catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders by user ID");
                return OperationResult<IEnumerable<Order>>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<IEnumerable<Address>>> GetAddressesByUserIdAsync(string userId)
        {
            try
            {
                // ✅ Check 1: Validate input
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return OperationResult<IEnumerable<Address>>.Fail("User ID cannot be null or empty.");
                }

                // ✅ Check 2: Ensure user exists and is a customer
                var user = await _context.Users
                    .Include(u => u.Addresses)
                    .FirstOrDefaultAsync(u => u.Id == userId && u.Role == UserRole.Customer);

                if (user == null)
                {
                    return OperationResult<IEnumerable<Address>>.Fail("Customer not found.");
                }

                // ✅ Check 3: Return empty list if no addresses
                if (user.Addresses == null || !user.Addresses.Any())
                {
                    return OperationResult<IEnumerable<Address>>.Fail("No addresses found.");
                }
            
                var result = user.Addresses
                    .Where(a => !a.IsDeleted)
                    .OrderByDescending(a => a.IsDefault)
                    .ThenByDescending(a => a.CreatedAt)
                    .ToList();

                return OperationResult<IEnumerable<Address>>.Success(result);
            }catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting addresses by user ID");
                return OperationResult<IEnumerable<Address>>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<IEnumerable<ReviewDto>>> GetReviewsByUserIdAsync(string userId)
        {
            try
            {
                // ✅ Check 1: Validate input
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return OperationResult<IEnumerable<ReviewDto>>.Fail("User ID cannot be null or empty.");
                }

                // ✅ Check 2: Ensure user exists and is a customer
                var userExists = await _context.Users
                    .AnyAsync(u => u.Id == userId && u.Role == UserRole.Customer);

                if (!userExists)
                {
                    return OperationResult<IEnumerable<ReviewDto>>.Fail("Customer not found.");
                }

                // ✅ Fetch reviews from DB
                var reviews = await _context.Reviews
                    .Where(r => r.UserId == userId && !r.IsDeleted)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();

                if (reviews == null || !reviews.Any())
                {
                    return OperationResult<IEnumerable<ReviewDto>>.Fail("No reviews found.");
                }

                // ✅ Map to DTO manually (or use AutoMapper if configured)
                var result = reviews.Select(r => new ReviewDto
                {
                    Id = r.Id,
                    ProductId = r.ProductId,
                    UserId = r.UserId,
                    Rating = r.Rating,
                    ReviewText = r.ReviewText,
                    CreatedAt = r.CreatedAt ?? DateTime.MinValue, // Explicitly handle nullable DateTime  
                }).ToList();

                return OperationResult<IEnumerable<ReviewDto>>.Success(result);
            }catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reviews by user ID");
                return OperationResult<IEnumerable<ReviewDto>>.Fail(ex.Message);
            }
        }
    }
}