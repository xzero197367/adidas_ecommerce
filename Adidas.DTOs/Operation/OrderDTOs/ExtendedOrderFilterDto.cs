namespace Adidas.DTOs.Operation.OrderDTOs
{
    public class ExtendedOrderFilterDto : OrderFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// true = guest, false = registered customer, null = all
        /// </summary>
        public bool? IsGuest { get; set; }
    }
}