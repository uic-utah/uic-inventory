using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using MediatR.Behaviors.Authorization.Exceptions;

namespace api.Features;
public class ProfileNotificationPayload : ResponseContract {
    public ProfileNotificationPayload(UnauthorizedException error) : base(error.Message) {
        FirstName = string.Empty;
        LastName = string.Empty;
        Email = string.Empty;
        Notifications = [];
    }
    public ProfileNotificationPayload(Exception _) : base("WTF01:Something went terribly wrong that we did not expect.") {
        FirstName = string.Empty;
        LastName = string.Empty;
        Email = string.Empty;
        Notifications = [];
    }

    public ProfileNotificationPayload(Account account, List<NotificationPayload> notifications) {
        FirstName = account?.FirstName ?? string.Empty;
        LastName = account?.LastName ?? string.Empty;
        Email = account?.Email ?? string.Empty;
        Notifications = notifications;
    }

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public IReadOnlyCollection<NotificationPayload> Notifications { get; set; } = [];
}
public class Notification {
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Url { get; set; }
    [Column(TypeName = "jsonb")] public Dictionary<string, object>? AdditionalData { get; set; }
    public NotificationTypes? NotificationType { get; set; }
    public virtual ICollection<NotificationReceipt> NotificationReceipts { get; set; } = new HashSet<NotificationReceipt>();
}
public class NotificationReceipt {
    public int Id { get; set; }

    public DateTime? ReadAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int RecipientId { get; set; }
    public int NotificationFk { get; set; }

    public virtual Notification Notification { get; set; } = default!;
    public virtual Account Recipient { get; set; } = default!;
}
public enum NotificationTypes {
    new_user_account_registration,
    admin_promotion,
    inventory_submission,
    inventory_under_review,
    inventory_approved,
    inventory_authorized,
    inventory_completed,
    facility_contact_modified,
    approved_well_status_edit,
    approved_inventory_well_addition,
    approved_site_contact_addition,
    approved_site_contact_deletion,
}
public class NotificationInput {
    public int Id { get; set; }
    public bool? Read { get; set; }
    public bool? Deleted { get; set; }
}
public class NotificationPayload {
    public NotificationPayload(Notification notification, NotificationReceipt receipt) {
        if (receipt.ReadAt.HasValue) {
            Read = true;
        }

        Id = receipt.Id;
        CreatedAt = notification.CreatedAt;
        ReadAt = receipt.ReadAt;
        NotificationType = notification.NotificationType ?? NotificationTypes.new_user_account_registration;
        Url = notification.Url ?? "#";
        AdditionalData = notification.AdditionalData ?? [];
        Deleted = receipt.DeletedAt.HasValue;

        foreach (var key in AdditionalData.Keys) {
            AdditionalData[key] = AdditionalData[key].ToString() ?? "null";
        }
    }

    public int Id { get; set; }
    [JsonPropertyName("event")] public NotificationTypes NotificationType { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Url { get; set; }
    public IDictionary<string, object> AdditionalData { get; set; }
    public DateTime? ReadAt { get; set; }
    public bool Read { get; set; }
    public bool Deleted { get; set; }
}
public class NotificationMutationPayload : ResponseContract {
    public NotificationMutationPayload(UnauthorizedException error) : base(error.Message) { }
    public NotificationMutationPayload(Exception _) : base("WTF01:Something went terribly wrong that we did not expect.") { }

    public NotificationMutationPayload(NotificationReceipt receipt) {
        ReadAt = receipt.ReadAt;
        Read = receipt.ReadAt.HasValue;
        DeletedAt = receipt.DeletedAt;
        Deleted = receipt.DeletedAt.HasValue;
        Id = receipt.Id;
    }

    public NotificationMutationPayload(IReadOnlyList<ApiError> errors)
        : base(errors) {
    }

    public bool Read { get; set; }
    public bool Deleted { get; set; }
    public int Id { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
