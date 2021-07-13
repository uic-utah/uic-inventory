using System.Linq;
using api.Infrastructure;

namespace api.GraphQL {
  public class ContactQueries {
    public IQueryable<Site> GetSites(AppDbContext context)
      => context.Sites;
  }
}
