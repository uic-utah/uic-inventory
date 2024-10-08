using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Serilog;

namespace api.Infrastructure;
public class PerformanceLogger<TRequest, TResponse>(ILogger log) : IPipelineBehavior<TRequest, TResponse>
  where TRequest : IRequest<TResponse> {
    private readonly Stopwatch _timer = new();
    private readonly ILogger _log = log.ForContext<PerformanceLogger<TRequest, TResponse>>();

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken) {
        _timer.Start();

        var response = await next();

        _timer.Stop();

        if (_timer.ElapsedMilliseconds > 500) {
            var name = typeof(TRequest).Name;

            _log.Warning("Long Running Request: {Name} ({ElapsedMilliseconds} ms) {@Request}", name, _timer.ElapsedMilliseconds, request);
        }

        return response;
    }
}
