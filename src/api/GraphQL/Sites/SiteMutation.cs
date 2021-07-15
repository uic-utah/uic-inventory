using System;
using System.Threading;
using System.Threading.Tasks;
using api.Exceptions;
using api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace api.GraphQL {
  [ApiController]
  public class SiteMutations : ControllerBase {
    private readonly AppDbContext _context;
    private readonly ILogger _log;

    public SiteMutations(AppDbContext context, ILogger log) {
      _context = context;
      _log = log;
    }

    [HttpPost("/api/site")]
    [Authorize]
    public async Task<ActionResult> CreateSiteAsync(SiteInput input, CancellationToken token) {
      var site = await _context.Sites.AddAsync(input.Update(new()), token);

      try {
        await _context.SaveChangesAsync(token);
      } catch (Exception ex) {
        _log.Error(ex, "Error saving site");

        return Problem(input.Id.ToString());
      }

      return Created($"site/{site.Entity.Id}/add-contacts", new SitePayload(site.Entity));
    }

    [HttpPut("/api/site")]
    [Authorize]
    public async Task<SitePayload> AddLocationAsync(SiteLocationInput input, CancellationToken token) {
      var item = await _context.Sites.FirstOrDefaultAsync(x => x.Id == input.Id, token);

      if (item is null) {
        return new SitePayload(new[] {
          new UserError("Site not found", "MISSING_SITE")
        });
      }

      item.Address = input.Address;
      item.Geometry = input.Geometry;

      try {
        await _context.SaveChangesAsync(token);
      } catch (Exception ex) {
        throw new AccountNotFoundException(input.Id.ToString(), ex);
      }

      return new SitePayload(item);
    }
  }
}
