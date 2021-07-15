using System;
using System.Linq;
using System.Threading.Tasks;
using api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.GraphQL {
  [ApiController]
  public class AccountMutations : ControllerBase {
    private readonly AppDbContext _context;

    public AccountMutations(AppDbContext context) {
      _context = context;
    }

    [HttpPut("/api/account")]
    [Authorize]
    public async Task<ActionResult> UpdateAccountAsync(AccountInput input) {
      Account account;
      try {
        account = _context.Accounts.Single(account => account.Id == input.Id);
      } catch (ArgumentNullException) {
        return NotFound(input.Id.ToString());
      }

      account = input.UpdateAccount(account);

      try {
        await _context.SaveChangesAsync();
      } catch (Exception) {
        return Problem(input.Id.ToString());
      }

      return Accepted(new AccountPayload(account));
    }
  }
}
