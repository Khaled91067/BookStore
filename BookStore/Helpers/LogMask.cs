namespace BookStore.Helpers
{
    /// <summary>
    /// Provides masking utilities for sensitive data in log messages.
    /// </summary>
    public static class LogMask
    {
        /// <summary>
        /// Masks an email address: "user@example.com" → "us***@example.com"
        /// </summary>
        public static string Email(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return "[no-email]";

            var atIndex = email.IndexOf('@');
            if (atIndex <= 0)
                return "***";

            var local = email[..atIndex];
            var domain = email[atIndex..];

            // Keep at most 2 chars of the local part
            var visible = local.Length <= 2 ? local : local[..2];
            return $"{visible}***{domain}";
        }

        /// <summary>
        /// Masks a transaction / provider reference ID, showing only the last 4 characters.
        /// "TXN123456789" → "****6789"
        /// </summary>
        public static string TransactionId(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return "[none]";

            return id.Length <= 4
                ? new string('*', id.Length)
                : $"****{id[^4..]}";
        }
    }
}
