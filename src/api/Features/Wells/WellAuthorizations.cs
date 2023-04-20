using System.Security.Claims;
using api.Infrastructure;
using MediatR.Behaviors.Authorization;
using Microsoft.AspNetCore.Http;

namespace api.Features;
// public class GetWellsAuthorizer : AbstractRequestAuthorizer<GetSites.Query> {
//   private readonly IHttpContextAccessor _context;

//   public GetWellsAuthorizer(IHttpContextAccessor context) {
//     _context = context;
//   }
//   public override void BuildPolicy(GetSites.Query request) =>
//     UseRequirement(new MustHaveAccount(_context.HttpContext?.User ?? new ClaimsPrincipal()));
// }

public class GetWellByIdAuthorizer : AbstractRequestAuthorizer<GetWellById.Query> {
    private readonly IHttpContextAccessor _context;

    public GetWellByIdAuthorizer(IHttpContextAccessor context) {
        _context = context;
    }

    public override void BuildPolicy(GetWellById.Query request) {
        UseRequirement(new MustHaveAccount(_context.HttpContext?.User ?? new ClaimsPrincipal()));
        UseRequirement(new MustHaveCompleteProfile());
        UseRequirement(new MustOwnSite(request.SiteId));
        UseRequirement(new MustHaveCompleteSite());
        UseRequirement(new MustOwnInventory(request.InventoryId));
    }
}

public class CreateWellAuthorizer : AbstractRequestAuthorizer<CreateWell.Command> {
    private readonly IHttpContextAccessor _context;

    public CreateWellAuthorizer(IHttpContextAccessor context) {
        _context = context;
    }
    public override void BuildPolicy(CreateWell.Command request) {
        UseRequirement(new MustHaveAccount(_context.HttpContext?.User ?? new ClaimsPrincipal()));
        UseRequirement(new MustHaveCompleteProfile());
        UseRequirement(new MustOwnSite(request.Input.SiteId));
        UseRequirement(new MustHaveCompleteSite());
        UseRequirement(new MustOwnInventory(request.Input.InventoryId));
    }
}

// public class UpdateWellAuthorizer : AbstractRequestAuthorizer<UpdateWell.Command> {
//   private readonly IHttpContextAccessor _context;

//   public UpdateWellAuthorizer(IHttpContextAccessor context) {
//     _context = context;
//   }
//   public override void BuildPolicy(UpdateWell.Command request) {
//     UseRequirement(new MustHaveAccount(_context.HttpContext?.User ?? new ClaimsPrincipal()));
//     UseRequirement(new MustOwnSite(request.Site.SiteId));
//     UseRequirement(new MustHaveCompleteProfile());
//     UseRequirement(new MustHaveEditableSiteStatus());
//   }
// }

public class DeleteWellAuthorizer : AbstractRequestAuthorizer<DeleteWell.Command> {
    private readonly IHttpContextAccessor _context;

    public DeleteWellAuthorizer(IHttpContextAccessor context) {
        _context = context;
    }
    public override void BuildPolicy(DeleteWell.Command request) {
        UseRequirement(new MustHaveAccount(_context.HttpContext?.User ?? new ClaimsPrincipal()));
        UseRequirement(new MustOwnSite(request.SiteId));
        UseRequirement(new MustHaveCompleteProfile());
        UseRequirement(new MustHaveEditableSiteStatus());
        UseRequirement(new MustOwnInventory(request.InventoryId));
        UseRequirement(new MustOwnWell(request.WellId));
    }
}

public class GetWellFilesAuthorizer : AbstractRequestAuthorizer<GetWellFiles.Command> {
    private readonly IHttpContextAccessor _context;

    public GetWellFilesAuthorizer(IHttpContextAccessor context) {
        _context = context;
    }
    public override void BuildPolicy(GetWellFiles.Command request) {
        UseRequirement(new MustHaveAccount(_context.HttpContext?.User ?? new ClaimsPrincipal()));
        UseRequirement(new MustOwnSite(request.SiteId));
        UseRequirement(new MustHaveCompleteProfile());
        UseRequirement(new MustOwnInventory(request.InventoryId));
    }
}
