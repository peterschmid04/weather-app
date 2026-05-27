namespace weatherAPI.Security
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _expectedApiKey;
        private const string ApiKeyHeaderName = "X-API-KEY";

        public ApiKeyMiddleware(RequestDelegate next, IConfiguration config)
        {
            _next = next;
            _expectedApiKey = config["Auth:ApiKey"]
                              ?? throw new InvalidOperationException("Auth:ApiKey missing in configuration.");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;
            
            if (path.StartsWith("/login") || path.StartsWith("/swagger") )
            {
                await _next(context);
                return;
            }
            
            if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var provided) ||
                provided != _expectedApiKey)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Missing or invalid API key.");
                return;
            }

            await _next(context);
        }
    }
}