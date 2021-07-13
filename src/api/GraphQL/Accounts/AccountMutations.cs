using System;
using System.Linq;
using System.Threading.Tasks;
using api.Exceptions;
using api.Infrastructure;

namespace api.GraphQL {
  public class AccountMutations {
    public async Task<AccountPayload> UpdateAccountAsync(AppDbContext context, AccountInput input) {
      Account account;
      try {
        account = context.Accounts.Single(account => account.Id == input.Id);
      } catch (ArgumentNullException ex) {
        throw new AccountNotFoundException(input.Id.ToString(), ex);
      }

      account = input.UpdateAccount(account);

      try {
        await context.SaveChangesAsync();
      } catch (Exception ex) {
        throw new AccountNotFoundException(input.Id.ToString(), ex);
      }

      return new AccountPayload(account);
    }
  }
}
