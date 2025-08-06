using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adidas.Application.Contracts.RepositoriesContracts.Operation;
using Adidas.Application.Contracts.ServicesContracts.People;
using Adidas.Context;
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Operation.ReviewDTOs.Query;
using Adidas.DTOs.People.Customer_DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models.People;

namespace Adidas.Application.Services.People
{
    public class CustomerService : ICustomerService
    {
        private readonly UserManager<User> _userManager;
        private readonly AdidasDbContext _context;

        public CustomerService(UserManager<User> userManager, AdidasDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<PagedResult<CustomerDto>> GetCustomersAsync(CustomerFilterDto filter)
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

            return new PagedResult<CustomerDto>
            {
                Items = customers,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize)
            };
        }

        public async Task<CustomerDetailsDto?> GetCustomerByIdAsync(string id)
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

            return new CustomerDetailsDto
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
        }


        public async Task<bool> UpdateCustomerAsync(string id, CustomerUpdateDto customerUpdateDto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || user.Role != UserRole.Customer) return false;

            user.Phone = customerUpdateDto.Phone;
            user.Gender = customerUpdateDto.Gender;
            user.DateOfBirth = customerUpdateDto.DateOfBirth;
            user.PreferredLanguage = customerUpdateDto.PreferredLanguage;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> ToggleCustomerStatusAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || user.Role != UserRole.Customer) return false;

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> DeleteCustomerAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || user.Role != UserRole.Customer) return false;

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        public async Task<byte[]> ExportCustomersAsync(CustomerFilterDto filter)
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

            foreach (var customer in customers.Items)
            {
                csv.AppendLine($"{customer.Name},{customer.Email},{customer.JoinDate:yyyy-MM-dd},{customer.Status},{customer.TotalSpent:C},{customer.Phone}");
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId)
        {
            // ✅ Check 1: Validate input
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

            // ✅ Check 2: Ensure user exists and is a customer
            var user = await _context.Users
                .Include(u => u.Orders)
                .FirstOrDefaultAsync(u => u.Id == userId && u.Role == UserRole.Customer);

            if (user == null)
                throw new KeyNotFoundException($"Customer with ID '{userId}' was not found.");

            // ✅ Check 3: Return empty list if no orders
            if (user.Orders == null || !user.Orders.Any())
                return Enumerable.Empty<Order>();

            return user.Orders.ToList();
        }

        public async Task<IEnumerable<Address>> GetAddressesByUserIdAsync(string userId)
        {
            // ✅ Check 1: Validate input
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

            // ✅ Check 2: Ensure user exists and is a customer
            var user = await _context.Users
                .Include(u => u.Addresses)
                .FirstOrDefaultAsync(u => u.Id == userId && u.Role == UserRole.Customer);

            if (user == null)
                throw new KeyNotFoundException($"Customer with ID '{userId}' was not found.");

            // ✅ Check 3: Return empty list if no addresses
            if (user.Addresses == null || !user.Addresses.Any())
                return Enumerable.Empty<Address>();

            return user.Addresses
                .Where(a => !a.IsDeleted)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.CreatedAt)
                .ToList();
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsByUserIdAsync(string userId)
        {
            // ✅ Check 1: Validate input
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

            // ✅ Check 2: Ensure user exists and is a customer
            var userExists = await _context.Users
                .AnyAsync(u => u.Id == userId && u.Role == UserRole.Customer);

            if (!userExists)
                throw new KeyNotFoundException($"Customer with ID '{userId}' was not found.");

            // ✅ Fetch reviews from DB
            var reviews = await _context.Reviews
                .Where(r => r.UserId == userId && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            if (reviews == null || !reviews.Any())
                return Enumerable.Empty<ReviewDto>();

            // ✅ Map to DTO manually (or use AutoMapper if configured)
            return reviews.Select(r => new ReviewDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                UserId = r.UserId,
                Rating = r.Rating,
                ReviewText = r.ReviewText,
                CreatedAt = r.CreatedAt ?? DateTime.MinValue, // Explicitly handle nullable DateTime  
            }).ToList();
        }


    }
}
