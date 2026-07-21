using BookStore.DTOs.Paymob;
using BookStore.ViewModels;

namespace BookStore.Services.Interfaces
{
    public interface IPaymobService
    {
        Task<string> CreateIntentionAsync(int orderId);

        Task ProcessWebhookAsync(PaymobWebHookDto webhook);

        Task<OrderVM> GetPaymentResultAsync(int orderId);

        Task<bool> UpdatePaymentStatusFromCallbackAsync(int orderId, string? success, string? id, string? pending);
    }
}
