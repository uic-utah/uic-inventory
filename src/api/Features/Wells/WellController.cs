using System;
using System.Threading;
using System.Threading.Tasks;
using api.Infrastructure;
using MediatR;
using MediatR.Behaviors.Authorization.Exceptions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace api.Features {
  [ApiController]
  public class WellController : ControllerBase {
    private readonly IMediator _mediator;
    private readonly HasRequestMetadata _requestMetadata;
    private readonly ILogger _log;

    public WellController(IMediator mediator, HasRequestMetadata requestMetadata, ILogger log) {
      _mediator = mediator;
      _requestMetadata = requestMetadata;
      _log = log;
    }

    [HttpGet("/api/well/{wellId}/site/{siteId:min(1)}/")]
    [Authorize(CookieAuthenticationDefaults.AuthenticationScheme)]
    public async Task<ActionResult<WellCreationPayload>> CreateWell(int siteId, int wellId, CancellationToken token) {
      try {
        var result = await _mediator.Send(new GetWellById.Query(siteId, wellId), token);

        return Ok(new WellCreationPayload(result, _requestMetadata.Site));
      } catch (UnauthorizedException ex) {
        _log.ForContext("endpoint", $"GET:api/site/{siteId}/well/{wellId}")
          .Warning(ex, "requirements failure");

        return Unauthorized(new WellPayload(ex));
      } catch (Exception ex) {
        _log.ForContext("endpoint", $"GET:api/site/{siteId}/well/{wellId}")
          .Fatal(ex, "unhandled exception");

        return StatusCode(500, new WellPayload(ex));
      }
    }

    [HttpPost("/api/well")]
    [Authorize(CookieAuthenticationDefaults.AuthenticationScheme)]
    public async Task<ActionResult<WellPayload>> CreateWell(WellInput input, CancellationToken token) {
      try {
        var result = await _mediator.Send(new CreateWell.Command(input), token);

        return Created($"api/well/{result.Id}", new WellPayload(result));
      } catch (UnauthorizedException ex) {
        _log.ForContext("endpoint", "POST:api/well")
          .ForContext("input", input)
          .Warning(ex, "requirements failure");

        return Unauthorized(new WellPayload(ex));
      } catch (Exception ex) {
        _log.ForContext("endpoint", "POST:api/well")
          .ForContext("input", input)
          .Fatal(ex, "unhandled exception");

        return StatusCode(500, new WellPayload(ex));
      }
    }
  }
}