using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;

namespace api.Infrastructure {
  public interface IComputation<TResult> { }

  public interface IComputationHandler<TComputation, TResult> where TComputation : IComputation<TResult> {
    Task<TResult> Handle(TComputation computation, CancellationToken cancellationToken);
  }

  public abstract class ComputationHandler<TRequest, TResponse> : IComputationHandler<TRequest, TResponse>
      where TRequest : IComputation<TResponse> {
    Task<TResponse> IComputationHandler<TRequest, TResponse>.Handle(TRequest request, CancellationToken cancellationToken)
        => Task.FromResult(Handle(request));

    protected abstract TResponse Handle(TRequest request);
  }

  public interface IComputeMediator {
    Task<TResult> Handle<TResult>(IComputation<TResult> command, CancellationToken cancellationToken);
  }

  public class ComputeMediator : IComputeMediator {
    public ComputeMediator(Func<Type, object> resolver) {
      Resolver = resolver;
    }

    private Func<Type, object> Resolver { get; }

    public Task<TResult> Handle<TResult>(IComputation<TResult> command, CancellationToken cancellationToken) {
      var commandType = command.GetType();

      var handlerType = typeof(IComputationHandler<,>).MakeGenericType(commandType, typeof(TResult));

      var handler = Resolver.Invoke(handlerType);

      var handleMethod = handlerType.GetMethod("Handle");

      var parameters = new object[] { command, cancellationToken };

      return (Task<TResult>)handleMethod.Invoke(handler, parameters);
    }
  }

  public static class ComputeMediatorExtensions {
    public static void AddComputationMediator(this ContainerBuilder builder) {
      // set up computations
      builder.RegisterAssemblyTypes(typeof(Startup).GetTypeInfo().Assembly)
              .Where(x => !x.Name.Contains("Decorator"))
              .AsClosedTypesOf(typeof(IComputationHandler<,>))
              .AsImplementedInterfaces();

      builder.Register(c => new ComputeMediator(c.Resolve<IComponentContext>().Resolve))
               .AsImplementedInterfaces()
               .SingleInstance();
    }
  }
}
