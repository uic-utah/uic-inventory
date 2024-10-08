using System.Threading;
using System.Threading.Tasks;
using api.Features;
using MediatR.Behaviors.Authorization;
using Serilog;

namespace api.Infrastructure;
public class MustHaveElevatedAccount : IAuthorizationRequirement {
    private class Handler(HasRequestMetadata metadata, ILogger log) : IAuthorizationHandler<MustHaveElevatedAccount> {
        private readonly ILogger _log = log;
        private readonly HasRequestMetadata _metadata = metadata;

        public async Task<AuthorizationResult> Handle(MustHaveElevatedAccount requirement, CancellationToken token = default) {
            if (_metadata.Account.Access == AccessLevels.elevated) {
                return AuthorizationResult.Succeed();
            }

            await Task.FromResult(0);

            _log.ForContext("accessed by", _metadata.Account)
                .ForContext("authorization", "MustHaveElevatedAccount:A05")
                .Warning("access to elevated item not permitted");

            return AuthorizationResult.Fail("A05:Your account access does not permit this action.");
        }
    }
}
