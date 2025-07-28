using Adidas.DTOs.Operation.ReviewDTOs.Query;
using Adidas.DTOs.Operation.ReviewDTOs.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Application.Contracts.ServicesContracts.Operation.Review
{
    public interface IReviewQueryService
    {
        Task<PagedReviewDto> GetReviewsPagedWithFiltersAsync(int pageNumber, int pageSize, ReviewFilterDto filters);
        Task<IEnumerable<ReviewDto>> SearchReviewsAsync(string searchTerm);
        Task<IEnumerable<ReviewDto>> GetReviewsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<ReviewDto>> GetRecentReviewsAsync(int count = 10);
        Task<IEnumerable<ReviewDto>> GetTopRatedReviewsAsync(Guid productId, int count = 10);

    }
}
