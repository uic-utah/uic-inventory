using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace api.GraphQL {
  [ApiController]
  public class SiteQueries : ControllerBase {
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _accessor;
    private readonly ILogger _log;

    public SiteQueries(AppDbContext context, IHttpContextAccessor accessor, ILogger log) {
      _context = context;
      _accessor = accessor;
      _log = log;
    }

    private IQueryable<Site> GetSites(AppDbContext context)
      => context.Sites;

    [HttpGet("/api/site/{id:min(1)}")]
    [Authorize]
    public async Task<ActionResult> GetSiteById(int id, CancellationToken token) {
      if (_accessor.HttpContext?.User.HasClaim(x => x.Type == ClaimTypes.NameIdentifier) != true) {
        _log.ForContext("claims", _accessor.HttpContext?.User.Claims)
           .Warning("User is missing name identifier claim");

        return BadRequest("user is missing required claims");
      }

      var utahIdClaim = _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
      if (utahIdClaim is null) {
        _log.ForContext("claims", _accessor.HttpContext?.User.Claims)
           .Warning("Name identifier claim is empty");

        return BadRequest("user is missing required claims");
      }

      var account = _context.Accounts.SingleOrDefault(x => x.UtahId == utahIdClaim.Value);
      if (account is null) {
        return BadRequest("account does not exist");
      }

      return Ok(await _context.Sites
        .Where(s => s.Id == id && s.AccountFk == account.Id)
        .Select(s => new {s.Id, s.Name, s.Ownership, s.NaicsPrimary, s.NaicsTitle, s.Address, s.Geometry})
        .SingleAsync(token));
    }

    [HttpGet("/api/user/{id}/sites")]
    [Authorize]
    public async Task<ActionResult> MySites() {
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

      var account = _context.Accounts.SingleOrDefault(x => x.UtahId == utahIdClaim.Value);
      if (account is null) {
        throw new System.Exception("account does not exist");
      }

      return Ok(await _context.Sites
        .Where(s => s.AccountFk == account.Id)
        .Select(x => new { x.Id, x.Name, x.NaicsTitle })
        .ToListAsync());
    }
  }
}
