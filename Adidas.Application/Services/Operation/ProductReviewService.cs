using Adidas.Application.Contracts.RepositoriesContracts.Operation;

namespace Adidas.Application.Services
{
    public class ProductReviewService
    {
        private readonly IReviewRepository _reviewRepository;

        public ProductReviewService(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository ?? throw new ArgumentNullException(nameof(reviewRepository));
        }

        public async Task<ProductReviewResultDto> GetProductReviewsWithSummaryAsync(
            Guid productId,
            int pageNumber = 1,
            int pageSize = 10,
            bool? isApproved = true)
        {
            // Get paginated reviews
            var (reviews, totalCount) = await _reviewRepository.GetReviewsPagedAsync(
                productId, pageNumber, pageSize, isApproved);

            // Get all approved reviews for summary calculation
            var allApprovedReviews = await _reviewRepository.GetApprovedReviewsAsync(productId);

            // Generate summary
            var summary = GenerateReviewSummary(allApprovedReviews);

            return new ProductReviewResultDto
            {
                Reviews = reviews.Select(r => new ProductReviewDto
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Title = r.Title,
                    ReviewText = r.ReviewText,
                    IsVerifiedPurchase = r.IsVerifiedPurchase,
                    IsApproved = r.IsApproved,
                    CreatedAt = r.CreatedAt,
                    ProductId = r.ProductId,
                    UserId = r.UserId,
                    UserName = r.User?.FirstName + " " + r.User?.LastName,
                    UserEmail = r.User?.Email
                }),
                Summary = summary,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }

        private ReviewSummaryDto GenerateReviewSummary(IEnumerable<Review> reviews)
        {
            var reviewList = reviews.ToList();

            if (!reviewList.Any())
            {
                return new ReviewSummaryDto
                {
                    AverageRating = 0,
                    TotalReviews = 0,
                    RatingDistribution = new Dictionary<int, int>
                    {
                        { 5, 0 }, { 4, 0 }, { 3, 0 }, { 2, 0 }, { 1, 0 }
                    },
                    VerifiedPurchaseCount = 0,
                    VerifiedPurchasePercentage = 0,
                    MostCommonWords = new List<string>(),
                    SentimentAnalysis = "Neutral"
                };
            }

            var averageRating = Math.Round(reviewList.Average(r => r.Rating), 1);
            var totalReviews = reviewList.Count;
            var verifiedCount = reviewList.Count(r => r.IsVerifiedPurchase);

            // Rating distribution
            var ratingDistribution = new Dictionary<int, int>();
            for (int i = 1; i <= 5; i++)
            {
                ratingDistribution[i] = reviewList.Count(r => r.Rating == i);
            }

            // Most common words (simple implementation)
            var mostCommonWords = ExtractMostCommonWords(reviewList);

            // Simple sentiment analysis based on ratings
            var sentimentAnalysis = CalculateSentiment(averageRating);

            return new ReviewSummaryDto
            {
                AverageRating = averageRating,
                TotalReviews = totalReviews,
                RatingDistribution = ratingDistribution,
                VerifiedPurchaseCount = verifiedCount,
                VerifiedPurchasePercentage = totalReviews > 0
                    ? Math.Round((double)verifiedCount / totalReviews * 100, 1)
                    : 0,
                MostCommonWords = mostCommonWords,
                SentimentAnalysis = sentimentAnalysis
            };
        }

        private List<string> ExtractMostCommonWords(List<Review> reviews)
        {
            var allText = string.Join(" ", reviews
                .Where(r => !string.IsNullOrEmpty(r.ReviewText))
                .Select(r => r.ReviewText));

            if (string.IsNullOrEmpty(allText))
                return new List<string>();

            // Simple word extraction (you might want to use a more sophisticated approach)
            var words = allText.ToLower()
                .Split(new char[] { ' ', '.', ',', '!', '?', ';', ':', '\n', '\r' },
                    StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 3) // Filter out short words
                .Where(w => !IsStopWord(w)) // Filter out common stop words
                .GroupBy(w => w)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => g.Key)
                .ToList();

            return words;
        }

        private bool IsStopWord(string word)
        {
            var stopWords = new HashSet<string>
            {
                "the", "and", "for", "are", "but", "not", "you", "all", "can",
                "had", "her", "was", "one", "our", "out", "day", "get", "has",
                "him", "his", "how", "its", "may", "new", "now", "old", "see",
                "two", "who", "boy", "did", "she", "use", "her", "way", "many",
                "been", "have", "this", "that", "with", "will", "your", "from",
                "they", "know", "want", "been", "good", "much", "some", "time",
                "very", "when", "come", "here", "just", "like", "long", "make",
                "over", "such", "take", "than", "them", "well", "were", "what"
            };

            return stopWords.Contains(word);
        }

        private string CalculateSentiment(double averageRating)
        {
            return averageRating switch
            {
                >= 4.0 => "Very Positive",
                >= 3.5 => "Positive",
                >= 2.5 => "Neutral",
                >= 1.5 => "Negative",
                _ => "Very Negative"
            };
        }
    }

    // DTOs
    public class ProductReviewResultDto
    {
        public IEnumerable<ProductReviewDto> Reviews { get; set; } = new List<ProductReviewDto>();
        public ReviewSummaryDto Summary { get; set; } = new ReviewSummaryDto();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class ProductReviewDto
    {
        public Guid Id { get; set; }
        public int Rating { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ReviewText { get; set; }
        public bool IsVerifiedPurchase { get; set; }
        public bool IsApproved { get; set; }
        public DateTime? CreatedAt { get; set; }
        public Guid ProductId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
    }

    public class ReviewSummaryDto
    {
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } = new Dictionary<int, int>();
        public int VerifiedPurchaseCount { get; set; }
        public double VerifiedPurchasePercentage { get; set; }
        public List<string> MostCommonWords { get; set; } = new List<string>();
        public string SentimentAnalysis { get; set; } = "Neutral";
    }
}