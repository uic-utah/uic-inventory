using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediatR.Behaviors.Authorization.Exceptions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace api.Features;
[ApiController]
public class NotificationController(IMediator mediator, ILogger log) : ControllerBase {
    private readonly ILogger _log = log;
    private readonly IMediator _mediator = mediator;

    [HttpGet("/api/notifications/mine")]
    [Authorize(CookieAuthenticationDefaults.AuthenticationScheme)]
    public async Task<ActionResult<ProfileNotificationPayload>> GetNotificationsAsync(CancellationToken token) {
        try {
            var notifications = await _mediator.Send(new GetNotifications.Query(), token);

            return Ok(notifications);
        } catch (UnauthorizedAccessException ex) {
            _log.ForContext("endpoint", "GET:api/notifications/mine")
              .Warning(ex, "Unauthorized access");

            return Unauthorized(new ProfileNotificationPayload(ex));
        } catch (UnauthorizedException ex) {
            _log.ForContext("endpoint", "GET:api/notifications/mine")
              .ForContext("requirement", ex.Message)
              .Warning("GetNotificationsAsync requirements failure");

            return Unauthorized(new NotificationMutationPayload(ex));
        } catch (Exception ex) {
            _log.ForContext("endpoint", "GET:api/notifications/mine")
              .Error(ex, "Error getting notifications");

            return StatusCode(500, new ProfileNotificationPayload(ex));
        }
    }

    [HttpPut("/api/notification")]
    [Authorize(CookieAuthenticationDefaults.AuthenticationScheme)]
    public async Task<ActionResult> UpdateNotificationAsync([FromBody] NotificationInput input, CancellationToken token) {
        try {
            var result = await _mediator.Send(new UpdateNotification.Command(input), token);

            return Accepted(result);
        } catch (UnauthorizedException ex) {
            _log.ForContext("endpoint", "PUT:api/site")
              .ForContext("input", input)
              .ForContext("requirement", ex.Message)
              .Warning("UpdateNotificationAsync requirements failure");

            return Unauthorized(new NotificationMutationPayload(ex));
        } catch (Exception ex) {
            _log.ForContext("endpoint", "PUT:api/site")
              .ForContext("input", input)
              .Fatal(ex, "Unhandled exception");

            return StatusCode(500, new ProfileNotificationPayload(ex));
        }
    }
}
