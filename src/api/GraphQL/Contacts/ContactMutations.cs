using System;
using System.Threading.Tasks;
using api.Exceptions;
using api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace api.GraphQL {
  [ApiController]
  public class ContactMutations : ControllerBase {
    private readonly AppDbContext _context;
    private readonly ILogger _log;

    public ContactMutations(AppDbContext context, ILogger log) {
      _context = context;
      _log = log;
    }

    [HttpPost("/api/contact")]
    [Authorize]
    public async Task<ActionResult> CreateContactAsync(ContactInput input) {
      var contact = await _context.Contacts.AddAsync(input.Update(new()));

      try {
        await _context.SaveChangesAsync();
      } catch (Exception ex) {
        _log.Error(ex, "Error saving contact");

        return NotFound(input.Id.ToString());
      }

      return Created("/api/site", new ContactPayload(contact.Entity));
    }
  }
}
