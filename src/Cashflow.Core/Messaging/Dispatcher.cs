using System.Reflection;
using Cashflow.Core.Results;
using Microsoft.Extensions.DependencyInjection;

namespace Cashflow.Core.Messaging;

public sealed class Dispatcher : IDispatcher
{
    private static readonly MethodInfo InvokeMethod = typeof(Dispatcher).GetMethod(nameof(Invoke), BindingFlags.NonPublic | BindingFlags.Static)!;
    private IServiceProvider Services { get; }

    public Dispatcher(IServiceProvider services)
    {
        Services = services;
    }

    public async Task<Result<TResponse>> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var method = InvokeMethod.MakeGenericMethod(request.GetType(), typeof(TResponse));
        return await (Task<Result<TResponse>>)method.Invoke(null, [Services, request, cancellationToken])!;
    }

    private static async Task<Result<TResponse>> Invoke<TRequest, TResponse>(IServiceProvider services, TRequest request, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        var handler = services.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
        if (handler is null)
        {
            return Result.Failure<TResponse>(new ResultError("dispatcher.no_handler", $"No handler registered for {typeof(TRequest).Name}"));
        }

        return await handler.HandleAsync(request, cancellationToken);
    }
}