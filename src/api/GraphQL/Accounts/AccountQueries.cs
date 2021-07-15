using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using api.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace api.GraphQL {
  [ApiController]
  public class AccountQueries : ControllerBase {
    private readonly IHttpContextAccessor _accessor;
    private readonly ILogger _log;

    private readonly AppDbContext _context;

    public AccountQueries(AppDbContext context, IHttpContextAccessor accessor, ILogger log) {
      _context = context;
      _accessor = accessor;
      _log = log;
    }

    [HttpGet("/api/me")]
    [Authorize(CookieAuthenticationDefaults.AuthenticationScheme)]
    public ActionResult GetMe() {
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

      return Ok(new AuthPayload(_context.Accounts.SingleOrDefault(x => x.UtahId == utahIdClaim.Value)));
    }

    [HttpGet("/api/accounts")]
    public async Task<List<Account>> GetAccounts(CancellationToken cancellationToken)
      => await _context.Accounts.OrderBy(x => x.LastName).ToListAsync(cancellationToken);

    [HttpGet("/api/account/{id}")]
    [Authorize]
    public async Task<Account> GetAccountById(int id, CancellationToken cancellationToken) => await _context.Accounts.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
  }
}
