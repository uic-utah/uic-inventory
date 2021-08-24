using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using api.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace api.Features {
  public static class SiteNotifications {
    public class EditNotification : INotification {
      public EditNotification(int siteId) {
        SiteId = siteId;
      }

      public int SiteId { get; }
    }

    public class EditNotificationHandler : INotificationHandler<EditNotification> {
      private readonly IAppDbContext _context;

      public EditNotificationHandler(IAppDbContext context) {
        _context = context;
      }
      private static bool GetContactStatus(ICollection<Contact> contacts) =>
        contacts.Any(x => RequiredContactTypes.Types.Contains(x.ContactType));
      private static bool GetLocationStatus(Site site) => site.Address is not null && site.Geometry is not null;
      private static bool GetDetailStatus(Site site) =>
        site.Name is not null &&
        site.Ownership is not null &&
        site.NaicsPrimary is not null &&
        site.NaicsTitle is not null;
      private static SiteStatus GetSiteStatus(Site site) =>
        site.ContactStatus && site.DetailStatus && site.LocationStatus ?
          SiteStatus.Complete :
          SiteStatus.Incomplete;
      public async Task Handle(EditNotification notification, CancellationToken token) {
        var site = await _context.Sites
          .Include(x => x.Contacts)
          .SingleOrDefaultAsync(s => s.Id == notification.SiteId, token);

        site.LocationStatus = GetLocationStatus(site);
        site.DetailStatus = GetDetailStatus(site);
        site.ContactStatus = GetContactStatus(site.Contacts);

        site.Status = GetSiteStatus(site);

        await _context.SaveChangesAsync(token);
      }
    }
  }
}