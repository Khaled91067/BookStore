using BookStore.DTOs.Paymob;
using BookStore.ViewModels;

namespace BookStore.Services.Interfaces
{
    public interface IPaymobService
    {
        /// <summary>
        /// Creates a Paymob payment intention for the given order and returns the hosted checkout URL.
        /// </summary>
        Task<string> CreateIntentionAsync(int orderId);

        /// <summary>
        /// Processes an asynchronous webhook notification sent by Paymob after a transaction.
        /// This is the authoritative update path — the GET callback (<see cref="UpdatePaymentStatusFromCallbackAsync"/>) is a best-effort fallback.
        /// </summary>
        Task ProcessWebhookAsync(PaymobWebHookDto webhook);

        /// <summary>
        /// Returns the current payment status and method for the result page.
        /// </summary>
        Task<OrderVM?> GetPaymentResultAsync(int orderId);

        /// <summary>
        /// Updates payment status from the Paymob GET redirect callback.
        /// Only updates status when the payment is still <see cref="PaymentStatus.Pending"/> and not a Cash order,
        /// to avoid overwriting a webhook update that may have already arrived.
        /// </summary>
        /// <param name="success">"true" if Paymob reports the transaction as successful.</param>
        /// <param name="id">Paymob's transaction ID.</param>
        /// <param name="pending">"true" if the transaction is still pending (e.g., 3DS in progress).</param>
        Task<bool> UpdatePaymentStatusFromCallbackAsync(int orderId, string? success, string? id, string? pending);
    }
}
