using System;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace api.GraphQL {
  [ApiController]
  public class ContactQueries : ControllerBase {
    private readonly AppDbContext _context;
    private readonly ILogger _log;

    public ContactQueries(AppDbContext context, ILogger log) {
      _context = context;
      _log = log;
    }
    [HttpGet("/api/site/{siteId:min(1)}/contacts")]
    [Authorize]
    public async Task<ActionResult> GetSiteContacts(int siteId, CancellationToken cancellationToken) {
      var site = await _context.Sites
        .Include(x => x.Account)
        .Include(x => x.Contacts)
        .Where(site => site.Id == siteId)
        .FirstOrDefaultAsync(cancellationToken);

      var contacts = site?.Contacts.Select(contact => new {
        contact.Id,
        contact.FirstName,
        contact.LastName,
        contact.ContactType,
        contact.Email,
        contact.PhoneNumber,
        contact.Organization,
        contact.MailingAddress,
        contact.City,
        contact.State,
        contact.ZipCode
      }).ToArray();

      return Ok(new {
        Name = site?.Name ?? "unknown",
        Owner = new AccountPayload(site?.Account),
        contacts
      });
    }
  }
}
