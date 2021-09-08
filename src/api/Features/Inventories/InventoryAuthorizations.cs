using System.Security.Claims;
using api.Infrastructure;
using MediatR.Behaviors.Authorization;
using Microsoft.AspNetCore.Http;

namespace api.Features {
  // public class GetInventorysAuthorizer : AbstractRequestAuthorizer<GetSites.Query> {
  //   private readonly IHttpContextAccessor _context;

  //   public GetInventorysAuthorizer(IHttpContextAccessor context) {
  //     _context = context;
  //   }
  //   public override void BuildPolicy(GetSites.Query request) =>
  //     UseRequirement(new MustHaveAccount(_context.HttpContext?.User ?? new ClaimsPrincipal()));
  // }

  public class GetInventoryByIdAuthorizer : AbstractRequestAuthorizer<GetInventoryById.Query> {
    private readonly IHttpContextAccessor _context;

    public GetInventoryByIdAuthorizer(IHttpContextAccessor context) {
      _context = context;
    }

    public override void BuildPolicy(GetInventoryById.Query request) {
      UseRequirement(new MustHaveAccount(_context.HttpContext?.User ?? new ClaimsPrincipal()));
      UseRequirement(new MustHaveCompleteProfile());
      UseRequirement(new MustOwnSite(request.SiteId));
      UseRequirement(new MustHaveCompleteSite());
    }
  }

  public class CreateInventoryAuthorizer : AbstractRequestAuthorizer<CreateInventory.Command> {
    private readonly IHttpContextAccessor _context;

    public CreateInventoryAuthorizer(IHttpContextAccessor context) {
      _context = context;
    }
    public override void BuildPolicy(CreateInventory.Command request) {
      UseRequirement(new MustHaveAccount(_context.HttpContext?.User ?? new ClaimsPrincipal()));
      UseRequirement(new MustHaveCompleteProfile());
      UseRequirement(new MustOwnSite(request.SiteId));
      UseRequirement(new MustHaveCompleteSite());
    }
  }

  // public class UpdateInventoryAuthorizer : AbstractRequestAuthorizer<UpdateInventory.Command> {
  //   private readonly IHttpContextAccessor _context;

  //   public UpdateInventoryAuthorizer(IHttpContextAccessor context) {
  //     _context = context;
  //   }
  //   public override void BuildPolicy(UpdateInventory.Command request) {
  //     UseRequirement(new MustHaveAccount(_context.HttpContext?.User ?? new ClaimsPrincipal()));
  //     UseRequirement(new MustOwnSite(request.Site.SiteId));
  //     UseRequirement(new MustHaveCompleteProfile());
  //     UseRequirement(new MustHaveEditableSiteStatus());
  //   }
  // }

  // public class DeleteInventoryAuthorizer : AbstractRequestAuthorizer<DeleteInventory.Command> {
  //   private readonly IHttpContextAccessor _context;

  //   public DeleteInventoryAuthorizer(IHttpContextAccessor context) {
  //     _context = context;
  //   }
  //   public override void BuildPolicy(DeleteInventory.Command request) {
  //     UseRequirement(new MustHaveAccount(_context.HttpContext?.User ?? new ClaimsPrincipal()));
  //     UseRequirement(new MustOwnSite(request.SiteId));
  //     UseRequirement(new MustHaveCompleteProfile());
  //     UseRequirement(new MustHaveEditableSiteStatus());
  //   }
  // }
}