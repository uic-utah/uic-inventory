using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace api.GraphQL {
  [ApiController]
  public class NotificationQueries : ControllerBase {
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _accessor;
    private readonly ILogger _log;

    public NotificationQueries(AppDbContext context, IHttpContextAccessor accessor, ILogger log) {
      _context = context;
      _accessor = accessor;
      _log = log;
    }

    [HttpGet("/api/user/{id}/notifications")]
    [Authorize]
    public async Task<ActionResult> GetNotifications(int id) {
      if (_accessor.HttpContext?.User.HasClaim(x => x.Type == ClaimTypes.NameIdentifier) != true) {
        _log.ForContext("claims", _accessor.HttpContext?.User.Claims)
           .Fatal("User is missing name identifier claim");

        return Unauthorized();
      }

      var utahIdClaim = _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
      if (utahIdClaim is null) {
        _log.ForContext("claims", _accessor.HttpContext?.User.Claims)
           .Fatal("Name identifier claim is empty");

        return Unauthorized();
      }

      var user = _context.Accounts.Where(x => x.UtahId == utahIdClaim.Value).Select(x => new { x.Id, x.Access, x.FirstName, x.LastName, x.Email }).Single();

      if (user.Access == AccessLevels.standard && user.Id != id) {
        _log.ForContext("user", utahIdClaim.Value)
           .Warning("User tried to access protected data");

        return Unauthorized();
      }

      var items = await _context.NotificationReceipts
       .Include(x => x.Notification)
       .Where(x => x.RecipientId == id)
       .Select(x => new NotificationPayload(x.Notification, x)).ToListAsync();

      return Ok(new { firstName = user.FirstName, lastName = user.LastName, email = user.Email, notifications = items });
    }
  }
}
