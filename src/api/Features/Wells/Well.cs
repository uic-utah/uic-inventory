using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using api.Infrastructure;
using MediatR.Behaviors.Authorization.Exceptions;
using Microsoft.AspNetCore.Http;

namespace api.Features;
public class Well {
    public int Id { get; set; }
    public int SiteFk { get; set; }
    public int AccountFk { get; set; }
    public int InventoryFk { get; set; }
    public int SubClass { get; set; }
    public string? WellName { get; set; }
    public string? Status { get; set; }
    public string? Description { get; set; }
    public int? Quantity { get; set; }
    [Column(TypeName = "jsonb")] public string? Geometry { get; set; }
    public string? RemediationDescription { get; set; }
    public int? RemediationType { get; set; }
    public string? RemediationProjectId { get; set; }
    public DateTime? CreatedOn { get; set; }
    public Account? Account { get; set; }
    public Site? Site { get; set; }
    public Inventory? Inventory { get; set; }
    public IList<WaterSystemContacts>? WaterSystemContacts { get; set; }
    public string? ConstructionDetails { get; set; }
    public string? InjectateCharacterization { get; set; }
    public string? HydrogeologicCharacterization { get; set; }
    public string? SurfaceWaterProtection { get; set; }
}
public class WellCreationPayload : WellPayload {
    public WellCreationPayload(UnauthorizedException error) : base(error) {
        Site = new SitePayload(error);
    }

    public WellCreationPayload(Exception error) : base(error) {
        Site = new SitePayload(error);
    }

    public WellCreationPayload(Well well, Site site) : base(well) {
        Site = new SitePayload(site);
    }

    public SitePayload Site { get; }
}
public class WaterSystemContactPayload(WaterSystemContacts contact) {
    public string? Email { get; set; } = contact.Email;
    public string? Name { get; set; } = contact.Name;
    public string? System { get; set; } = contact.System;
}
public class WellPayload : ResponseContract {
    private const string Unknown = "unknown";
    public WellPayload(UnauthorizedException error) : base(error.Message) {
        WaterSystemContacts = [];
        Status = Unknown;
        Geometry = "{}";
        Description = Unknown;
    }
    public WellPayload(Exception _) : base("WTF01:Something went terribly wrong that we did not expect.") {
        WaterSystemContacts = [];
        Status = Unknown;
        Geometry = "{}";
        Description = Unknown;
    }
    public WellPayload(string error) : base($"UP01:{error}") {
        WaterSystemContacts = [];
        Status = Unknown;
        Geometry = "{}";
        Description = Unknown;
    }
    public WellPayload(Well well) {
        Id = well.Id;
        WellName = well.WellName;
        SubClass = well.SubClass;
        Status = WellLookup.OperatingStatus(well.Status);
        Count = well.Quantity ?? 0;
        Geometry = well.Geometry ?? "{}";
        Description = well.Description ?? "";
        RemediationType = WellLookup.RemediationType(well.RemediationType);
        RemediationProjectId = well.RemediationProjectId;
        ConstructionDetails = well.ConstructionDetails;
        InjectateCharacterization = well.InjectateCharacterization;
        HydrogeologicCharacterization = well.HydrogeologicCharacterization;
        SurfaceWaterProtection = well.SurfaceWaterProtection;
        WaterSystemContacts = well.WaterSystemContacts?.Select(x => new WaterSystemContactPayload(x)) ?? [];
    }

    public int Id { get; }
    public string? WellName { get; }
    public string Description { get; set; }
    public string? RemediationType { get; }
    public string? RemediationProjectId { get; }
    public int SubClass { get; }
    public int Count { get; }
    public string Geometry { get; }
    public string Status { get; }
    public string? ConstructionDetails { get; }
    public string? InjectateCharacterization { get; }
    public string? HydrogeologicCharacterization { get; }
    public string? SurfaceWaterProtection { get; set; }
    public IEnumerable<WaterSystemContactPayload> WaterSystemContacts { get; set; }
    public bool WellDetailsComplete {
        get {
            if (SubClass == 5002) {
                return !string.IsNullOrEmpty(ConstructionDetails) && !string.IsNullOrEmpty(InjectateCharacterization);
            }

            return !string.IsNullOrEmpty(ConstructionDetails);
        }
    }
}
public class WellFilePayload : ResponseContract {
    public WellFilePayload(UnauthorizedException error) : base(error.Message) { }
    public WellFilePayload(FileNotFoundException error) : base($"DL01:{error.Message}") { }
    public WellFilePayload(ArgumentNullException error) : base($"DL02:{error.Message}") { }
    public WellFilePayload(string error) : base($"DL03:{error}") { }
    public WellFilePayload(Exception _) : base("WTF01:Something went terribly wrong that we did not expect.") { }
}
public class WellInput {
    public int WellId { get; set; }
    public int SiteId { get; set; }
    public int AccountId { get; set; }
    public int InventoryId { get; set; }
    public int SubClass { get; set; }
    public string? Status { get; set; }
    public int Quantity { get; set; }
    public string? Construction { get; set; }
    public string? Description { get; set; }
    public string? Geometry { get; set; }
    public string? RemediationDescription { get; set; }
    public int? RemediationType { get; set; }
    public string? RemediationProjectId { get; set; }
    public string? ConstructionDetails { get; set; }
    public string? InjectateCharacterization { get; set; }
    public string? HydrogeologicCharacterization { get; set; }
}
public class WellFileInput {
    public string WellIdRange { get; set; } = default!;
    public int SiteId { get; set; }
    public int InventoryId { get; set; }
    public string File { get; set; } = default!;
}

public class WellDetailInput : WellInput {
    public int[] SelectedWells { get; set; } = [];
    public IFormFile? ConstructionDetailsFile { get; set; }
    public IFormFile? InjectateCharacterizationFile { get; set; }
}
public static class WellLookup {
    public static string OperatingStatus(string? key, string missingValue = "") {
        if (string.IsNullOrEmpty(key)) {
            return missingValue;
        }

        key = key.ToUpper();

        var lookup = new Dictionary<string, string> {
    {"AC", "Active"},
    {"PA", "Abandoned - Approved"},
    {"PR", "Proposed Under Authorization By Rule"},
    {"OT", "Other"},
  };

        return lookup.GetValueOrDefault(key, missingValue);
    }
    public static string WellType(int type) => type switch {
        -1 => "General wells",
        5047 => "Storm water drainage wells",
        5002 => "Subsurface environmental remediation wells",
        5101 => "UIC - Regulated large underground wastewater disposal systems",
        5026 => "Veterinary, kennel, or pet grooming wastewater disposal systems",
        _ => "Unknown",
    };
    public static string RemediationType(int? type) => type switch {
        1 => "Brownfield",
        2 => "LUST",
        3 => "NPL",
        4 => "RCRA",
        5 => "Superfund",
        6 => "TRI",
        7 => "VPP",
        8 => "DSHW",
        _ => "Other",
    };
}

public static class WellDetailInputExtension {
    public static Well Update(this WellDetailInput input, Well original) {
        if (input.ConstructionDetails != null) {
            if (input.ConstructionDetails == "null") {
                original.ConstructionDetails = null;
            } else {
                original.ConstructionDetails = input.ConstructionDetails;
            }
        }

        if (input.InjectateCharacterization != null) {
            if (input.InjectateCharacterization == "null") {
                original.InjectateCharacterization = null;
            } else {
                original.InjectateCharacterization = input.InjectateCharacterization;
            }
        }

        if (input.HydrogeologicCharacterization != null) {
            if (input.HydrogeologicCharacterization == "null") {
                original.HydrogeologicCharacterization = null;
            } else {
                original.HydrogeologicCharacterization = input.HydrogeologicCharacterization;
            }
        }

        return original;
    }
}

public record ArcGisRestFeatureWell(ArcGisRestWellAttributes Attributes);
public record ArcGisRestWellAttributes(string SysNumber);
public class WellEsriQueryResponse : RestErrorable {
    public int Count { get; set; }
    public IReadOnlyList<ArcGisRestFeatureWell> Features { get; set; } = [];
}
public record WellOperatingStatusInput(int AccountId, int SiteId, int InventoryId, int WellId, string Status, string? Description);
