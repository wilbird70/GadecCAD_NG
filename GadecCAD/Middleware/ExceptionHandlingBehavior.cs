using MediatR;

namespace GadecCAD.Middleware;

public class ExceptionHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            LogException(ex);
            return default!;
        }
    }

    private void LogException(Exception ex)
    {
    }
}
