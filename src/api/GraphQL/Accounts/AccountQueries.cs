using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using api.Infrastructure;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace api.GraphQL {
  public class AccountQueries {
    private readonly IHttpContextAccessor _accessor;
    private readonly ILogger _log;

    public AccountQueries(IHttpContextAccessor accessor, ILogger log) {
      _accessor = accessor;
      _log = log;
    }

    public Account? GetMe(
      AppDbContext context) {
      if (_accessor.HttpContext?.User.HasClaim(x => x.Type == ClaimTypes.NameIdentifier) != true) {
        _log.ForContext("claims", _accessor.HttpContext?.User.Claims)
           .Warning("User is missing name identifier claim");

        throw new System.Exception("user is missing required claims");
      }

      var utahIdClaim = _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
      if (utahIdClaim is null) {
        _log.ForContext("claims", _accessor.HttpContext?.User.Claims)
           .Warning("Name identifier claim is empty");

        throw new System.Exception("user is missing required claims");
      }

      return context.Accounts.SingleOrDefault(x => x.UtahId == utahIdClaim.Value);
    }

    public IQueryable<Account> GetAccounts(AppDbContext context)
      => context.Accounts.OrderBy(x => x.LastName);

    public Task<Account> GetAccountById( int id, CancellationToken cancellationToken) {
      return null;
    }

    public Task<IReadOnlyList<Account>> GetAccountsById( int[] ids, CancellationToken cancellationToken) {
      return null;
    }
  }
}
