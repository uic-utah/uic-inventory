using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using api.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using Serilog;

namespace api.Features;
public static class InventoryNotifications {
    public record EditNotification : INotification {
        public EditNotification(int inventoryId) {
            Id = inventoryId;
        }

        public int Id { get; }
    }
    public record SubmitNotification : INotification {
        public SubmitNotification(Site site, Inventory inventory, Account account) {
            Inventory = inventory;
            Site = site;
            Account = account;
        }

        public Inventory Inventory { get; }
        public Site Site { get; set; }
        public Account Account { get; set; }
        public NotificationTypes NotificationType { get; } = NotificationTypes.inventory_submission;
    }
    public record RejectNotification : INotification {
        public RejectNotification(Site site, Inventory inventory, Account account, IEnumerable<string?> contacts) {
            Inventory = inventory;
            Site = site;
            Account = account;
            Contacts = contacts?.Where(x => !string.IsNullOrEmpty(x))
              .Select(c => new EmailAddress(c)).ToList() ?? [];
        }

        public Inventory Inventory { get; }
        public Site Site { get; set; }
        public Account Account { get; set; }
        public List<EmailAddress> Contacts { get; }
    }
    public record DeleteNotification : INotification {
        public DeleteNotification(Inventory inventory) {
            InventoryId = inventory.Id;
            SiteId = inventory.SiteFk;
        }

        public int InventoryId { get; }
        public int SiteId { get; set; }
    }
    public record UnderReviewNotification : INotification {
        public UnderReviewNotification(int siteId, int inventoryId, Account account) {
            InventoryId = inventoryId;
            SiteId = siteId;
            Account = account;
        }

        public int InventoryId { get; }
        public int SiteId { get; set; }
        public Account Account { get; set; }
        public NotificationTypes NotificationType { get; } = NotificationTypes.inventory_under_review;
    }
    public record ApproveNotification : INotification {
        public ApproveNotification(int siteId, int inventoryId, Account account) {
            InventoryId = inventoryId;
            SiteId = siteId;
            Account = account;
        }

        public int InventoryId { get; }
        public int SiteId { get; set; }
        public Account Account { get; set; }
        public NotificationTypes NotificationType { get; } = NotificationTypes.inventory_approved;
    }
    public record AuthorizeNotification : INotification {
        public AuthorizeNotification(int siteId, int inventoryId, Account account) {
            InventoryId = inventoryId;
            SiteId = siteId;
            Account = account;
        }

        public int InventoryId { get; }
        public int SiteId { get; set; }
        public Account Account { get; set; }
        public NotificationTypes NotificationType { get; } = NotificationTypes.inventory_authorized;
    }
    public record CompleteNotification : INotification {
        public CompleteNotification(int siteId, int inventoryId, Account account) {
            InventoryId = inventoryId;
            SiteId = siteId;
            Account = account;
        }

        public int InventoryId { get; }
        public int SiteId { get; set; }
        public Account Account { get; set; }
        public NotificationTypes NotificationType { get; } = NotificationTypes.inventory_completed;
    }
    public class EditNotificationHandler(AppDbContext context, ILogger log) : INotificationHandler<EditNotification> {
        private readonly AppDbContext _context = context;
        private readonly ILogger _log = log;

        private static bool GetWellContactStatus(Inventory entity) {
            if (entity.IsSerInventory()) {
                return entity.Site?.Contacts.Any(x => x.SerContact) ?? false;
            }

            return true;
        }

        private static bool GetPaymentStatus(Inventory entity) {
            IReadOnlyList<int> subClasses = [-1, 5047, 5002, 5101, 5026];

            return (entity.OrderNumber?.Length ?? 0) == 8 && subClasses.Contains(entity.SubClass);
        }
        private static bool GetLocationStatus(Inventory entity) {
            var hasAtLeastOneWell = entity.Wells.Count > 0;

            if (!hasAtLeastOneWell) {
                return false;
            }

            return entity.Wells.All(x =>
              !string.IsNullOrEmpty(x.WellName) &&
              !string.IsNullOrEmpty(x.Status) && x.Quantity > 0 &&
              !string.IsNullOrEmpty(x.Geometry)
            );
        }
        private static bool GetWellDetailsStatus(Inventory entity) {
            var hasAtLeastOneWell = entity.Wells.Count > 0;

            if (!hasAtLeastOneWell) {
                return false;
            }

            var hasConstructionDetails = entity.Wells.All(x => !string.IsNullOrEmpty(x.ConstructionDetails));

            if (entity.Wells.Any(x => x.SubClass == 5002)) {
                var serHasInjectate = entity.Wells
                  .Where(x => x.SubClass == 5002)
                  .Any(w => !string.IsNullOrEmpty(w.InjectateCharacterization));

                return hasConstructionDetails && serHasInjectate;
            }

            return hasConstructionDetails;
        }
        private static bool GetSignatureStatus(Inventory entity) => !string.IsNullOrEmpty(entity.Signature);
        private static InventoryStatus GetInventoryStatus(Inventory entity) {
            var currentStatus = new List<bool> { entity.DetailStatus, entity.LocationStatus, entity.SignatureStatus };

            if (entity.IsSerInventory()) {
                currentStatus.Add(entity.ContactStatus);
            }

            var status = currentStatus.All(x => x) ? InventoryStatus.Complete : InventoryStatus.Incomplete;

            if (status == InventoryStatus.Complete && entity.SubmittedOn.HasValue) {
                status = InventoryStatus.Submitted;
            }

            if (status == InventoryStatus.Submitted && entity.UnderReviewOn.HasValue) {
                status = InventoryStatus.UnderReview;
            }


            if (status == InventoryStatus.UnderReview && entity.ApprovedOn.HasValue) {
                status = InventoryStatus.Approved;
            }

            if (status == InventoryStatus.Approved && entity.AuthorizedOn.HasValue) {
                status = InventoryStatus.Authorized;
            }

            if (status == InventoryStatus.Authorized && entity.CompletedOn.HasValue) {
                status = InventoryStatus.Completed;
            }

            return status;
        }
        public async Task Handle(EditNotification notification, CancellationToken token) {
            _log.ForContext("notification", notification)
              .Debug("Handling Inventory notification");

            var entity = await _context.Inventories
              .Include(x => x.Wells)
              .Include(x => x.Site)
              .Include(x => x.Site!.Contacts)
              .AsSplitQuery()
              .SingleOrDefaultAsync(s => s.Id == notification.Id, token);

            if (entity == null) {
                return;
            }

            entity.PaymentStatus = GetPaymentStatus(entity);
            entity.LocationStatus = GetLocationStatus(entity);
            entity.ContactStatus = GetWellContactStatus(entity);
            entity.DetailStatus = GetWellDetailsStatus(entity);
            entity.SignatureStatus = GetSignatureStatus(entity);

            entity.Status = GetInventoryStatus(entity);

            _log.ForContext("inventory", entity)
              .Debug("Updating inventory status");

            await _context.SaveChangesAsync(token);
        }
    }
    public class CreateSubmissionNotificationHandler(AppDbContext context, ILogger log) : INotificationHandler<SubmitNotification> {
        private readonly AppDbContext _context = context;
        private readonly ILogger _log = log;

        private Notification CreateNotifications(SubmitNotification metadata) {
            var notification = NotificationHelpers.CreateBasicNotification(_context, metadata.NotificationType);
            var initials = NotificationHelpers.GetInitials(metadata.Account);

            notification.AdditionalData = new Dictionary<string, object> {
                    { "name", initials },
                    { "inventoryId", metadata.Inventory.Id },
                };
            notification.Url = $"/review/site/{metadata.Site.Id}/inventory/{metadata.Inventory.Id}";

            return notification;
        }
        public async Task Handle(SubmitNotification metadata, CancellationToken token) {
            _log.ForContext("notification", metadata)
              .Debug("Handling inventory submission notification");

            var notifications = CreateNotifications(metadata);

            _context.Notifications.Add(notifications);
            await _context.SaveChangesAsync(token);
        }
    }
    public class CreateUnderReviewNotificationHandler(AppDbContext context, ILogger log) : INotificationHandler<UnderReviewNotification> {
        private readonly AppDbContext _context = context;
        private readonly ILogger _log = log;

        private Notification CreateNotifications(UnderReviewNotification metadata) {
            var notification = NotificationHelpers.CreateBasicNotification(_context, metadata.NotificationType);
            var initials = NotificationHelpers.GetInitials(metadata.Account);

            notification.AdditionalData = new Dictionary<string, object> {
                    { "name", initials },
                    { "inventoryId", metadata.InventoryId },
                };
            notification.Url = $"/review/site/{metadata.SiteId}/inventory/{metadata.InventoryId}";

            return notification;
        }
        public async Task Handle(UnderReviewNotification metadata, CancellationToken token) {
            _log.ForContext("notification", metadata)
              .Debug("Handling inventory under review notification");

            var notifications = CreateNotifications(metadata);

            _context.Notifications.Add(notifications);
            await _context.SaveChangesAsync(token);
        }
    }
    public class CreateApprovedNotificationHandler(AppDbContext context, ILogger log) : INotificationHandler<ApproveNotification> {
        private readonly AppDbContext _context = context;
        private readonly ILogger _log = log;

        private Notification CreateNotifications(ApproveNotification metadata) {
            var notification = NotificationHelpers.CreateBasicNotification(_context, metadata.NotificationType);
            var initials = NotificationHelpers.GetInitials(metadata.Account);

            notification.AdditionalData = new Dictionary<string, object> {
                    { "name", initials },
                    { "inventoryId", metadata.InventoryId },
                };
            notification.Url = $"/review/site/{metadata.SiteId}/inventory/{metadata.InventoryId}";

            return notification;
        }
        public async Task Handle(ApproveNotification metadata, CancellationToken token) {
            _log.ForContext("notification", metadata)
              .Debug("Handling inventory approval notification");

            var notifications = CreateNotifications(metadata);

            _context.Notifications.Add(notifications);
            await _context.SaveChangesAsync(token);
        }
    }
    public class CreateAuthorizedNotificationHandler(AppDbContext context, ILogger log) : INotificationHandler<AuthorizeNotification> {
        private readonly AppDbContext _context = context;
        private readonly ILogger _log = log;

        private Notification CreateNotifications(AuthorizeNotification metadata) {
            var notification = NotificationHelpers.CreateBasicNotification(_context, metadata.NotificationType);
            var initials = NotificationHelpers.GetInitials(metadata.Account);

            notification.AdditionalData = new Dictionary<string, object> {
                    { "name", initials },
                    { "inventoryId", metadata.InventoryId },
                };
            notification.Url = $"/review/site/{metadata.SiteId}/inventory/{metadata.InventoryId}";

            return notification;
        }
        public async Task Handle(AuthorizeNotification metadata, CancellationToken token) {
            _log.ForContext("notification", metadata)
              .Debug("Handling inventory authorized notification");

            var notifications = CreateNotifications(metadata);

            _context.Notifications.Add(notifications);
            await _context.SaveChangesAsync(token);
        }
    }
    public class CreateCompletedNotificationHandler(AppDbContext context, ILogger log) : INotificationHandler<CompleteNotification> {
        private readonly AppDbContext _context = context;
        private readonly ILogger _log = log;

        private Notification CreateNotifications(CompleteNotification metadata) {
            var notification = NotificationHelpers.CreateBasicNotification(_context, metadata.NotificationType);
            var initials = NotificationHelpers.GetInitials(metadata.Account);

            notification.AdditionalData = new Dictionary<string, object> {
                    { "name", initials },
                    { "inventoryId", metadata.InventoryId },
                };
            notification.Url = $"/review/site/{metadata.SiteId}/inventory/{metadata.InventoryId}";

            return notification;
        }
        public async Task Handle(CompleteNotification metadata, CancellationToken token) {
            _log.ForContext("notification", metadata)
              .Debug("Handling inventory completed notification");

            var notifications = CreateNotifications(metadata);

            _context.Notifications.Add(notifications);
            await _context.SaveChangesAsync(token);
        }
    }
    public class SendRegulatorySubmissionEmailHandler(EmailService client, ILogger log) : INotificationHandler<SubmitNotification> {
        private readonly EmailService _client = client;
        private readonly ILogger _log = log;

        public async Task Handle(SubmitNotification notification, CancellationToken token) {
            _log.ForContext("notification", notification)
              .Debug("Handling inventory submission client email");

            var message = new SendGridMessage {
                From = new EmailAddress("no-reply@utah.gov", "UIC Inventory App"),
                Subject = "UIC Inventory App: Inventory Submission Received",
            };
            message.AddContent(
              MimeType.Html,
              $@"<h1>We thank you for your submission 🙏</h1>
          <p>The inventory has been received by the UIC Inventory Staff. The staff have been notified and someone will
           review your submission in the order it was received.</p>
          <h2>Submission Summary</h2>
          <ul>
            <li>{WellLookup.WellType(notification.Inventory.SubClass)} - Inventory #{notification.Inventory.Id} (Order #{notification.Inventory.OrderNumber})</li>
            <li>{notification.Site.Name} - Site #{notification.Site.Id}</li>
            <li>Submission Date: {notification.Inventory.SubmittedOn:yyyy/MM/dd}</li>
          </ul>
          <p>Have a great day,</p>
          <p>The UIC Staff</p>
          Division of Water Quality
          <a href=""https://deq.utah.gov/division-water-quality"">waterquality.utah.gov</a>"
            );

            message.AddTo(new EmailAddress(notification.Account.Email, $"{notification.Account.FirstName} {notification.Account.LastName}"));

            var response = await _client.SendEmailAsync(message, token);

            if (!response.IsSuccessStatusCode) {
                _log.ForContext("message", message.PlainTextContent)
                .ForContext("response", await response.Body.ReadAsStringAsync(token))
                  .Error("Failed to send client inventory submission email");
            } else {
                _log.ForContext("message", message.PlainTextContent)
                  .ForContext("response", await response.Body.ReadAsStringAsync(token))
                  .Information("Sent inventory submission client email");
            }
        }
    }
    public class SendAdminSubmissionEmailHandler(AppDbContext context, EmailService client, ILogger log) : INotificationHandler<SubmitNotification> {
        private readonly EmailService _client = client;
        private readonly ILogger _log = log;
        private readonly AppDbContext _context = context;

        public async Task Handle(SubmitNotification notification, CancellationToken token) {
            _log.ForContext("notification", notification)
              .Debug("Handling inventory submission admin email");

            var message = new SendGridMessage {
                From = new EmailAddress("noreply@utah.gov", "UIC Inventory App"),
                Subject = "UIC Inventory App: Inventory Submission Notification",
            };
            message.AddContent(
              MimeType.Html,
              $@"<h1>There is a submission ready for your review 🔬</h1>
          <p>{notification.Account.FirstName} {notification.Account.LastName} submitted the following inventory:</p>
          <h2>Submission Summary</h2>
          <ul>
            <li>{WellLookup.WellType(notification.Inventory.SubClass)} - Inventory #{notification.Inventory.Id} (Order #{notification.Inventory.OrderNumber})</li>
            <li>{notification.Site.Name} - Site #{notification.Site.Id}</li>
            <li>Submission Date: {notification.Inventory.SubmittedOn:yyyy/MM/dd}</li>
          </ul>
          <p>🎉 Have a great day! 🎉</p>"
            );

            var adminAccounts = await _context.Accounts.Where(x => x.Access == AccessLevels.elevated && x.ReceiveNotifications == true)
            .Select(x => new EmailAddress(x.Email, $"{x.FirstName} {x.LastName}")).ToListAsync(cancellationToken: token);

            if (adminAccounts.Count == 0) {
                _log.Warning("No admin accounts found to send inventory submission email");

                return;
            }

            message.AddTos(adminAccounts);

            var response = await _client.SendEmailAsync(message, token);

            if (!response.IsSuccessStatusCode) {
                _log.ForContext("message", message.PlainTextContent)
                  .ForContext("to", string.Join(',', adminAccounts.Select(x => x.Email)))
                  .ForContext("status", response.StatusCode)
                  .ForContext("response", await response.Body.ReadAsStringAsync(token))
                  .Error("Failed to send admin inventory submission email");
            } else {
                _log.ForContext("message", message.PlainTextContent)
                  .ForContext("to", string.Join(',', adminAccounts.Select(x => x.Email)))
                  .ForContext("status", response.StatusCode)
                  .ForContext("response", await response.Body.ReadAsStringAsync(token))
                  .Information("Sent inventory submission admin email");
            }
        }
    }
    public class SendSubmissionRejectionEmailHandler(EmailService client, IConfiguration configuration, ILogger log) : INotificationHandler<RejectNotification> {
        private readonly EmailService _client = client;
        private readonly ILogger _log = log;
        public readonly string _email = configuration.GetSection("App").GetValue<string>("AdminEmail") ?? string.Empty;

        public async Task Handle(RejectNotification notification, CancellationToken token) {
            _log.ForContext("notification", notification)
              .Debug("Handling inventory rejection email");

            var message = new SendGridMessage {
                From = new EmailAddress(_email, "UIC Administrators"),
                Subject = $"UIC Class V Inventory Submittal Rejected - {notification.Site.Name}",
            };
            message.AddContent(
              MimeType.Html,
              $@"<h1>The submission for {notification.Site.Name} has been rejected</h1>
          <p>The UIC Class V inventory submittal with the Utah Division of Water Quality has been rejected.</p>
          <h2>Contact us</h2>
          <p>If you have any questions please reply to this email ({_email}).</p>"
            );

            message.AddTos(notification.Contacts);

            var response = await _client.SendEmailAsync(message, token);

            if (!response.IsSuccessStatusCode) {
                _log.ForContext("message", message.PlainTextContent)
                .ForContext("response", await response.Body.ReadAsStringAsync(token))
                  .Error("Failed to send inventory rejection email");
            } else {
                _log.ForContext("message", message.PlainTextContent)
                  .ForContext("response", await response.Body.ReadAsStringAsync(token))
                  .Information("Sent inventory rejection email");
            }
        }
    }
    public class RemoveCloudStorageHandler(CloudStorageService cloud, IConfiguration configuration, ILogger log) : INotificationHandler<DeleteNotification> {
        private readonly ILogger _log = log;
        private readonly string _bucket = configuration.GetValue<string>("STORAGE_BUCKET") ?? string.Empty;
        private readonly CloudStorageService _client = cloud;

        public async Task Handle(DeleteNotification notification, CancellationToken token) {
            _log.ForContext("notification", notification)
              .Debug("Handling inventory deletion cloud storage removal");

            await _client.RemoveObjectsAsync(_bucket, $"site_{notification.SiteId}/inventory_{notification.InventoryId}/", token);
        }
    }
    public class GroundWaterProtectionsHandler(GroundWaterService groundWaterService, AppDbContext context, ILogger log) : INotificationHandler<SubmitNotification> {
        private readonly GroundWaterService _groundWaterService = groundWaterService;
        private readonly AppDbContext _context = context;
        private readonly ILogger _log = log;

        public async Task Handle(SubmitNotification notification, CancellationToken token) {
            _log.ForContext("notification", notification)
              .Debug("Handling well ground water protection intersections");

            var wells = notification.Inventory.Wells.Select(well => new GroundWaterService.GroundWaterInput(well.Id, well.Geometry));
            var waterContacts = await _groundWaterService.GetWaterSystemContactsAsync(wells, token);

            foreach (var (wellId, metadata) in waterContacts) {
                _log.Debug("Updating contacts for well {@id}", wellId);

                var well = notification.Inventory.Wells.SingleOrDefault(x => x.Id == wellId);

                if (well == null) {
                    _log.Warning("Well {Id} not found in database. unable to update surface water protection", wellId);
                    continue;
                }

                well.SurfaceWaterProtection = metadata.SurfaceWaterProtection;

                if (metadata.Contacts.Count > 0) {
                    _log.Debug("Adding new contact records");

                    foreach (var contact in metadata.Contacts) {
                        _context.WaterSystemContacts.Add(new WaterSystemContacts {
                            SiteFk = well.SiteFk,
                            InventoryFk = well.InventoryFk,
                            WellFk = well.Id,
                            AccountFk = well.AccountFk,
                            Name = contact.Name,
                            Email = contact.Email,
                            System = contact.System
                        });
                    }
                }

                _context.Wells.Update(well);
            }

            await _context.SaveChangesAsync(token);
        }
    }
}
