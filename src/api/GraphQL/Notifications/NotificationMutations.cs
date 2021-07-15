using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.GraphQL {
  [ApiController]
  public class NotificationMutations : ControllerBase {
    private readonly AppDbContext _context;

    public NotificationMutations(AppDbContext context) {
      _context = context;
    }

    [HttpPut("/api/notification")]
    [Authorize]
    public async Task<ActionResult> UpdateNotification([FromBody] NotificationInput input) {
      //! TODO check if the user owns the notification
      var receipt = await _context.NotificationReceipts.FirstOrDefaultAsync(x => x.Id == input.Id);

      if (receipt is null) {
        return NotFound(new NotificationMutationResponse(new[] {
          new UserError("Notification not found", "MISSING_NOTIFICATION")
        }));
      }

      var now = DateTime.Now;

      if (input.Read == true) {
        receipt.ReadAt = now;
      }

      if (input.Deleted == true) {
        receipt.DeletedAt = now;

        if (!receipt.ReadAt.HasValue) {
          receipt.ReadAt = now;
        }
      }

      await _context.SaveChangesAsync();

      return Accepted(new NotificationMutationResponse(receipt));
    }
  }

  public class NotificationMutationResponse : Payload {
    public NotificationMutationResponse(NotificationReceipt receipt) {
      ReadAt = receipt.ReadAt;
      Read = receipt.ReadAt.HasValue;
      DeletedAt = receipt.DeletedAt;
      Deleted = receipt.DeletedAt.HasValue;
      Id = receipt.Id;
    }

    public NotificationMutationResponse(IReadOnlyList<UserError> errors)
        : base(errors) {
    }

    public bool Read { get; set; }
    public bool Deleted { get; set; }
    public int Id { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime? DeletedAt { get; set; }
  }
}
