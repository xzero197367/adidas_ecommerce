using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adidas.DTOs.Tracker;

namespace Adidas.Application.Contracts.ServicesContracts.Static
{
    public interface INotificationService
    {
        Task SendOrderConfirmationAsync(Guid orderId);
        Task SendOrderStatusUpdateAsync(Guid orderId);
        Task SendLowStockAlertAsync(IEnumerable<LowStockAlertDto> alerts);
        Task SendWelcomeEmailAsync(string userId);
        Task SendPasswordResetEmailAsync(string email, string resetToken);
        Task SendEmailVerificationAsync(string userId, string verificationToken);
    }
}
