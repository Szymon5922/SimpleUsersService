using Application.Exceptions;
using Npgsql;

namespace API.Middleware
{
    public class ErrorHandlingMiddleware : IMiddleware
    {
        private readonly ILogger _logger;
        public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger)
        {
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next.Invoke(context);
            }
            catch (NotFoundException exception)
            {
                _logger.LogError(exception,exception.Message);

                context.Response.StatusCode = 404;
                await context.Response.WriteAsync(exception.Message);
            }
            catch (BadRequestException exception)
            {
                _logger.LogError(exception, exception.Message);

                context.Response.StatusCode = 400;
                await context.Response.WriteAsync(exception.Message);
            }
            catch (NpgsqlException exception)
            {
                _logger.LogError(exception, exception.Message);

                context.Response.StatusCode = 503;
                await context.Response.WriteAsync(exception.Message);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, exception.Message);

                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Something went wrong");
            }
        }
    }
}