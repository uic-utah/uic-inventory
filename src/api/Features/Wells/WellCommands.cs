using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using api.Infrastructure;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace api.Features {
  public static class CreateWell {
    public class Command : IRequest<Well> {
      public Command(WellInput input) {
        Input = input;
      }

      public WellInput Input { get; }
    }
    public class Handler : IRequestHandler<Command, Well> {
      private readonly IAppDbContext _context;
      private readonly IPublisher _publisher;
      private readonly ILogger _log;
      private readonly HasRequestMetadata _metadata;

      public Handler(
        IAppDbContext context,
        IPublisher publisher,
        HasRequestMetadata metadata,
        ILogger log) {
        _context = context;
        _publisher = publisher;
        _log = log;
        _metadata = metadata;
      }
      public async Task<Well> Handle(Command message, CancellationToken cancellationToken) {
        _log.ForContext("input", message)
          .ForContext("verb", "POST")
          .Debug("/api/well");

        var well = new Well {
          AccountFk = message.Input.AccountId,
          SiteFk = message.Input.SiteId,
          InventoryFk = message.Input.InventoryId,
          WellName = message.Input.Construction,
          Status = message.Input.Status,
          Description = message.Input.Description,
          Quantity = message.Input.Quantity,
          Geometry = message.Input.Geometry,
          SubClass = _metadata.Inventory.SubClass,
          RemediationType = message.Input.RemediationType,
          RemediationDescription = message.Input.RemediationDescription,
          RemediationProjectId = message.Input.RemediationProjectId,
        };

        var result = await _context.Wells.AddAsync(well, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        well.Id = result.Entity.Id;

        // await _publisher.Publish(new WellNotifications.EditNotification(well.Id), cancellationToken);

        return well;
      }
    }
  }

  public static class UpdateWell {
    public class Command : IRequest<Well> {
      public Command(WellDetailInput input) {
        Wells = input;
      }

      public WellDetailInput Wells { get; }
    }

    public class Handler : IRequestHandler<Command, Well> {
      private readonly string [] _acceptableFileTypes = new[] { "pdf", "png", "doc", "docx", "jpg", "jpeg" };
      private readonly IAppDbContext _context;
      private readonly IPublisher _publisher;
      private readonly ILogger _log;
      private readonly HasRequestMetadata _metadata;

      public Handler(
        IAppDbContext context,
        IPublisher publisher,
        HasRequestMetadata metadata,
        ILogger log) {
        _context = context;
        _publisher = publisher;
        _log = log;
        _metadata = metadata;
      }
      public async Task<Well> Handle(Command request, CancellationToken cancellationToken) {
        _log.ForContext("input", request)
          .ForContext("verb", "PUT")
          .Debug("/api/well");

        var errors = new List<string>(2);

        var wells = await _context.Wells
          .Where(x =>  request.Wells.SelectedWells.Contains(x.Id))
          .ToListAsync(cancellationToken);

        if (request.Wells.ConstructionDetailsFile != null || request.Wells.InjectateCharacterizationFile != null) {
          using var stream = new FileStream("./secrets/cloud-storage-sa", FileMode.Open, FileAccess.Read);
          var client = await StorageClient.CreateAsync(GoogleCredential.FromStream(stream));

          if (request.Wells.ConstructionDetailsFile != null) {
            var fileType = request.Wells.ConstructionDetailsFile.FileName.Split('.').Last();

            if (_acceptableFileTypes.Contains(fileType)) {
              try {
                var constructionFile = $"site_{request.Wells.SiteId}_inventory_{request.Wells.InventoryId}_user_{request.Wells.AccountId}_construction.{fileType}";
                await client.UploadObjectAsync("ut-dts-agrc-uic-inventory-dev-documents",
                  constructionFile,
                  request.Wells.ConstructionDetailsFile.ContentType,
                  request.Wells.ConstructionDetailsFile.OpenReadStream(), cancellationToken: cancellationToken);

                request.Wells.ConstructionDetails = $"file::{constructionFile}";
              } catch (Exception e) {
                const string message = "Error uploading construction file";

                _log.ForContext("site", request.Wells.SiteId)
                    .ForContext("inventory", request.Wells.InventoryId)
                    .ForContext("account", request.Wells.AccountId)
                    .ForContext("fileType", fileType)
                    .Error(e, message);

                errors.Add(message);
              }
            }
          }

          if (request.Wells.InjectateCharacterizationFile != null) {
            var fileType = request.Wells.InjectateCharacterizationFile.FileName.Split('.').Last();

            if (_acceptableFileTypes.Contains(fileType)) {
              try {
                var injectateFile = $"site_{request.Wells.SiteId}_inventory_{request.Wells.InventoryId}_user_{request.Wells.AccountId}_injectate.{fileType}";
                await client.UploadObjectAsync("ut-dts-agrc-uic-inventory-dev-documents",
                  injectateFile,
                  request.Wells.InjectateCharacterizationFile.ContentType,
                  request.Wells.InjectateCharacterizationFile.OpenReadStream(), cancellationToken: cancellationToken);

                request.Wells.InjectateCharacterization = $"file::{injectateFile}";
              } catch (Exception e) {
                const string message = "Error uploading injectate file";

                _log.ForContext("site", request.Wells.SiteId)
                    .ForContext("inventory", request.Wells.InventoryId)
                    .ForContext("account", request.Wells.AccountId)
                    .ForContext("fileType", fileType)
                    .Error(e, message);

                errors.Add(message);
              }
            }
          }
        }

        foreach (var well in wells) {
          request.Wells.Update(well);
        }

        await _context.SaveChangesAsync(cancellationToken);

        if (errors.Count > 0){
          throw new Exception(string.Join("\n", errors));
        }

        // await _publisher.Publish(new SiteNotifications.EditNotification(site.Id), cancellationToken);

        return new Well();
      }
    }
  }

  public static class DeleteWell {
    public class Command : IRequest {
      public Command(WellInput input) {
        AccountId = input.AccountId;
        SiteId = input.SiteId;
        InventoryId = input.InventoryId;
        WellId = input.WellId;
      }

      public int AccountId { get; set; }
      public int SiteId { get; set; }
      public int InventoryId { get; }
      public int WellId { get; set; }
    }

    public class Handler : IRequestHandler<Command> {
      private readonly IAppDbContext _context;
      private readonly ILogger _log;

      public Handler(IAppDbContext context, ILogger log) {
        _context = context;
        _log = log;
      }
      async Task<Unit> IRequestHandler<Command, Unit>.Handle(Command request, CancellationToken cancellationToken) {
        var well = await _context.Wells
          .FirstAsync(s => s.Id == request.WellId, cancellationToken);

        _context.Wells.Remove(well);

        //! TODO: create requirement that well cannot be deleted when authorized status

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
      }
    }
  }
}
