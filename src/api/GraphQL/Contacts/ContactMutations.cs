using System;
using System.Threading.Tasks;
using api.Exceptions;
using api.Infrastructure;

namespace api.GraphQL {
  public class ContactMutations {
    public async Task<ContactPayload> CreateContactAsync(AppDbContext context,
      ContactInput input) {
      var contact = await context.Contacts.AddAsync(input.Update(new()));

      try {
        await context.SaveChangesAsync();
      } catch (Exception ex) {
        throw new AccountNotFoundException(input.Id.ToString(), ex);
      }

      return new ContactPayload(contact.Entity);
    }
  }
}
