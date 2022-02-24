using System;
using System.Threading;
using System.Threading.Tasks;
using api.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace api.Features {
  public static class CreateInventory {
    public class Command : IRequest<Inventory> {
      public Command(InventoryCreationInput input) {
        AccountId = input.AccountId;
        SiteId = input.SiteId;
        SubClass = input.SubClass;
        OrderNumber = input.OrderNumber;
      }

      public int AccountId { get; init; }
      public int SiteId { get; init; }
      public int OrderNumber { get; init; }
      public int SubClass { get; init; }
      public DateTime? CreatedOn { get; } = DateTime.UtcNow;
    }
    public class Handler : IRequestHandler<Command, Inventory> {
      private readonly IAppDbContext _context;
      private readonly IPublisher _publisher;
      private readonly ILogger _log;

      public Handler(IAppDbContext context, IPublisher publisher, ILogger log) {
        _context = context;
        _publisher = publisher;
        _log = log;
      }
      public async Task<Inventory> Handle(Command message, CancellationToken cancellationToken) {
        _log.ForContext("input", message)
          .Debug("creating inventory");

        var inventory = new Inventory {
          AccountFk = message.AccountId,
          SiteFk = message.SiteId,
          OrderNumber = message.OrderNumber,
          SubClass = message.SubClass,
        };

        var result = await _context.Inventories.AddAsync(inventory, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        inventory.Id = result.Entity.Id;

        await _publisher.Publish(new InventoryNotifications.EditNotification(inventory.Id), cancellationToken);

        return inventory;
      }
    }
  }
  public static class UpdateInventory {
    public class Command : IRequest<Inventory> {
      public Command(InventoryMutationInput input) {
        AccountId = input.AccountId;
        SiteId = input.SiteId;
        SubClass = input.SubClass;
        Edocs = input.Edocs;
        InventoryId = input.InventoryId;
      }

      public int AccountId { get; init; }
      public int SiteId { get; init; }
      public int InventoryId { get; init; }
      public int? SubClass { get; init; }
      public string? Edocs { get; set; }
    }
    public class Handler : IRequestHandler<Command, Inventory> {
      private readonly IAppDbContext _context;
      private readonly ILogger _log;
      public Handler(IAppDbContext context, ILogger log) {
        _context = context;
        _log = log;
      }

      public async Task<Inventory> Handle(Command request, CancellationToken cancellationToken) {
        _log.ForContext("input", request)
          .Debug("updating inventory");

        var inventory = await _context.Inventories
          .FirstAsync(s => s.Id == request.InventoryId, cancellationToken);

        if (request.SubClass.HasValue) {
          inventory.SubClass = request.SubClass.Value;
        }
        if (request.Edocs != null) {
          inventory.Edocs = request.Edocs;
        }
        if (request.Edocs?.Length == 0) {
          inventory.Edocs = null;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return inventory;
      }
    }
  }

  public static class SubmitInventory {
    public class Command : IRequest {
      public Command(InventorySubmissionInput input) {
        AccountId = input.AccountId;
        SiteId = input.SiteId;
        InventoryId = input.InventoryId;
        Signature = input.Signature;
      }

      public int AccountId { get; set; }
      public int SiteId { get; set; }
      public int InventoryId { get; set; }
      public string Signature { get; set; }
    }

    public class Handler : IRequestHandler<Command> {
      private readonly IAppDbContext _context;
      private readonly IPublisher _publisher;
      private readonly ILogger _log;
      private readonly HasRequestMetadata _metadata;

      public Handler(IAppDbContext context,
              IPublisher publisher,
              HasRequestMetadata metadata,
              ILogger log) {
        _context = context;
        _publisher = publisher;
        _log = log;
        _metadata = metadata;
      }
      async Task<Unit> IRequestHandler<Command, Unit>.Handle(Command request, CancellationToken cancellationToken) {
        _log.ForContext("input", request)
          .Debug("Submitting Inventory");

        if (string.IsNullOrEmpty(request.Signature)) {
          throw new Exception("Signature is required");
        }

        var inventory = await _context.Inventories
          .FirstAsync(s => s.Id == request.InventoryId, cancellationToken);

        inventory.SubmittedOn = DateTime.UtcNow;
        inventory.Signature = request.Signature;

        _context.Inventories.Update(inventory);

        //! TODO: needs requirements

        await _context.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(new InventoryNotifications.EditNotification(request.InventoryId), cancellationToken);
        await _publisher.Publish(new InventoryNotifications.SubmitNotification(_metadata.Site, _metadata.Inventory, _metadata.Account), cancellationToken);

        return Unit.Value;
      }
    }
  }
  public static class DeleteInventory {
    public class Command : IRequest {
      public Command(InventoryDeletionInput input) {
        AccountId = input.AccountId;
        SiteId = input.SiteId;
        InventoryId = input.InventoryId;
      }

      public int AccountId { get; set; }
      public int SiteId { get; set; }
      public int InventoryId { get; set; }
    }

    public class Handler : IRequestHandler<Command> {
      private readonly IAppDbContext _context;
      private readonly ILogger _log;

      public Handler(IAppDbContext context, ILogger log) {
        _context = context;
        _log = log;
      }
      async Task<Unit> IRequestHandler<Command, Unit>.Handle(Command request, CancellationToken cancellationToken) {
        _log.ForContext("input", request)
          .Debug("deleting inventory");

        var inventory = await _context.Inventories
          .Include(w => w.Wells)
          .FirstAsync(s => s.Id == request.InventoryId, cancellationToken);

        _context.Inventories.Remove(inventory);

        //! TODO: create requirement that inventory cannot be deleted when authorized status

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
      }
    }
  }
}
