namespace BookStore.Middleware
{
    /// <summary>
    /// Catches unhandled exceptions from any layer, logs them as errors, and redirects
    /// to the error page — keeping controllers free of try/catch boilerplate.
    /// </summary>
    public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Unhandled exception on {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                // In dev, let the default developer exception page handle it;
                // in production redirect gracefully.
                if (!context.Response.HasStarted)
                    context.Response.Redirect("/Home/Error");
            }
        }
    }
}
