using GadecCAD.Core;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GadecCAD.Middleware;

public class ExceptionHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly ILogger _logger;

    public ExceptionHandlingBehavior(ILoggerFactory loggerFactory)
    {
        _logger = Guard.ForNull(loggerFactory).CreateLogger(typeof(ExceptionHandlingBehavior<,>));
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception exception)
        {
            LogException(exception);
            return default!;
        }
    }

    private void LogException(Exception exception)
    {
        _logger.Log(LogLevel.Error, exception, exception.Message);
    }
}
