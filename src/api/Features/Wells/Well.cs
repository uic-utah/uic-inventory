using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace api.Features {
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
    public string? ConstructionDetails { get; set; }
    public string? InjectateCharacterization { get; set; }
    public string? HydrogeologicCharacterization { get; set; }
  }
  public class WellCreationPayload : WellPayload {
    public WellCreationPayload(UnauthorizedAccessException error) : base(error) {
    }

    public WellCreationPayload(Exception error) : base(error) {
    }

    public WellCreationPayload(Well well, Site site) : base(well) {
      Site = new SitePayload(site);
    }

    public SitePayload Site { get; }
  }
  public class WellPayload : ResponseContract {
    public WellPayload(UnauthorizedAccessException error) : base(error.Message) { }
    public WellPayload(Exception error) : base("WTF01:Something went terribly wrong that we did not expect.") { }
    public WellPayload(Well well) {
      Id = well.Id;
      WellName = well.WellName;
      SubClass = well.SubClass;
      Status = WellLookup.OperatingStatus(well.Status);
      Count = well.Quantity ?? 0;
      Geometry = well.Geometry ?? "{}";
      ConstructionDetails = well.ConstructionDetails;
      InjectateCharacterization = well.InjectateCharacterization;
      HydrogeologicCharacterization = well.HydrogeologicCharacterization;
    }

    public int Id { get; }
    public string? WellName { get; }
    public int SubClass { get; }
    public int Count { get; }
    public string Geometry { get; }
    public string Status { get; }
    public string? ConstructionDetails { get; }
    public string? InjectateCharacterization { get; }
    public string? HydrogeologicCharacterization { get; }
    public bool WellDetailsComplete {
      get {
        if (SubClass == 5002) {
            return !string.IsNullOrEmpty(ConstructionDetails) && !string.IsNullOrEmpty(InjectateCharacterization) && !string.IsNullOrEmpty(HydrogeologicCharacterization);
        }

        return !string.IsNullOrEmpty(ConstructionDetails);
      }
    }
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

  public class WellDetailInput : WellInput {
    public int[] SelectedWells { get; set; } = Array.Empty<int>();
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
        {"PA", "Abandoned ‐ Approved"},
        {"PR", "Proposed Under Authorization By Rule"},
        {"OT", "Other"},
      };

      return lookup.GetValueOrDefault(key, missingValue);
    }
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
}
