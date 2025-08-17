using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Operation.PaymentDTOs;
using Adidas.DTOs.Operation.PaymentDTOs.PaypalDtos;

namespace Adidas.Application.Contracts.ServicesContracts.Operation
{
    public interface IPayPalService
    {
        Task<OperationResult<PayPalPaymentDto>> CreatePaymentAsync(PayPalCreatePaymentDto createDto);
        Task<OperationResult<PaymentDto>> ExecutePaymentAsync(string paymentId, string payerId);
        Task<OperationResult<PaymentDto>> GetPaymentDetailsAsync(string paymentId);
        Task<OperationResult<PaymentDto>> RefundPaymentAsync(string transactionId, decimal? amount = null);
    }
}
