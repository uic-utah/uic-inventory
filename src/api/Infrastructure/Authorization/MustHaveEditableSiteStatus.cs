using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using api.Features;
using MediatR.Behaviors.Authorization;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace api.Infrastructure;
public class MustHaveEditableSiteStatus : IAuthorizationRequirement {
    private class Handler(AppDbContext context, HasRequestMetadata metadata, ILogger log) : IAuthorizationHandler<MustHaveEditableSiteStatus> {
        private readonly ILogger _log = log;
        private readonly HasRequestMetadata _metadata = metadata;
        private readonly AppDbContext _context = context;
        private readonly InventoryStatus[] _draftInventoryStatus = [InventoryStatus.Incomplete, InventoryStatus.Complete];

        public async Task<AuthorizationResult> Handle(
          MustHaveEditableSiteStatus requirement,
          CancellationToken token = default) {

            var site = await _context.Sites
            .Include(x => x.Inventories)
            .SingleAsync(x => x.Id == _metadata.Site.Id, token);


            if (site.Inventories.All(x => _draftInventoryStatus.Contains(x.Status))) {
                return AuthorizationResult.Succeed();
            }

            return AuthorizationResult.Fail("S03:You cannot make changes to a site with approved inventories.");
        }
    }
}
