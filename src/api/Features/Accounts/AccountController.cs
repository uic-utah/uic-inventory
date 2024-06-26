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
public class AccountController(IMediator mediator, ILogger log) : ControllerBase {
    private readonly IMediator _mediator = mediator;
    private readonly ILogger _log = log;

    [HttpGet("/api/me")]
    [Authorize(CookieAuthenticationDefaults.AuthenticationScheme)]
    public async Task<ActionResult<AuthPayload>> GetMeAsync(CancellationToken token) {
        try {
            var payload = await _mediator.Send(new GetMyAccount.Query(Request.HttpContext.User), token);

            return Ok(new AuthPayload(payload));
        } catch (UnauthorizedException ex) {
            _log.ForContext("endpoint", "GET:api/me")
              .ForContext("requirement", ex.Message)
              .Warning("GetMeAsync requirements failure");

            return Unauthorized(new AuthPayload(ex));
        } catch (Exception ex) {
            _log.ForContext("endpoint", "GET:api/me")
              .Fatal(ex, "Unhandled exception");

            return StatusCode(500, new AuthPayload(ex));
        }
    }

    [HttpGet("/api/account/{id}")]
    [Authorize(CookieAuthenticationDefaults.AuthenticationScheme)]
    public async Task<ActionResult<AccountPayload>> GetAccountByIdAsync(int id, CancellationToken token) {
        try {
            var payload = await _mediator.Send(new GetAccountById.Query(id), token);

            return Ok(new AccountPayload(payload));
        } catch (UnauthorizedException ex) {
            _log.ForContext("endpoint", $"GET:api/account/{id}")
              .ForContext("requirement", ex.Message)
              .Warning("GetAccountByIdAsync requirements failure");

            return Unauthorized(new AccountPayload(ex));
        } catch (Exception ex) {
            _log.ForContext("endpoint", $"GET:api/account/{id}")
              .Fatal(ex, "Unhandled exception");

            return StatusCode(500, new AccountPayload(ex));
        }
    }

    [HttpGet("/api/accounts")]
    [Authorize(CookieAuthenticationDefaults.AuthenticationScheme)]
    public async Task<ActionResult<AuthPayload>> GetAccountsAsync(CancellationToken token) {
        try {
            var payload = await _mediator.Send(new GetAllAccounts.Query(), token);

            return Ok(new MinimalAccountPayload(payload));
        } catch (UnauthorizedException ex) {
            _log.ForContext("endpoint", "GET:api/accounts")
              .ForContext("requirement", ex.Message)
              .Warning("GetMeAsync requirements failure");

            return Unauthorized(new MinimalAccountPayload(ex));
        } catch (Exception ex) {
            _log.ForContext("endpoint", "GET:api/accounts")
              .Fatal(ex, "Unhandled exception");

            return StatusCode(500, new MinimalAccountPayload(ex));
        }
    }

    [HttpPut("/api/account")]
    [Authorize(CookieAuthenticationDefaults.AuthenticationScheme)]
    public async Task<ActionResult<AccountPayload>> UpdateAccountAsync(AccountInput input, CancellationToken token) {
        try {
            var payload = await _mediator.Send(new UpdateAccount.Command(input), token);

            return Accepted(new AccountPayload(payload));
        } catch (UnauthorizedAccessException ex) {
            _log.ForContext("endpoint", "PUT:api/account")
              .ForContext("input", input)
              .Warning(ex, "Unauthorized access");

            return Unauthorized(new AccountPayload(ex));
        } catch (UnauthorizedException ex) {
            _log.ForContext("endpoint", "GET:api/account")
              .ForContext("requirement", ex.Message)
              .Warning("UpdateAccountAsync requirements failure");

            return Unauthorized(new AccountPayload(ex));
        } catch (ArgumentNullException ex) {
            _log.ForContext("endpoint", "PUT:api/account")
              .ForContext("input", input)
              .Warning(ex, "Account not found");

            return NotFound(new AccountPayload(ex));
        } catch (Exception ex) {
            _log.ForContext("endpoint", "PUT:api/account")
              .Error(ex, "Unhandled exception");

            return StatusCode(500, new AccountPayload(ex));
        }
    }

    [HttpPatch("/api/admin/account")]
    [Authorize(CookieAuthenticationDefaults.AuthenticationScheme)]
    public async Task<ActionResult<AccountPayload>> AdminUpdateAccountAsync(AdminAccountInput input, CancellationToken token) {
        try {
            var payload = await _mediator.Send(new AdminUpdateAccount.Command(input), token);

            return Accepted(new AccountPayload(payload));
        } catch (UnauthorizedAccessException ex) {
            _log.ForContext("endpoint", "PATCH:/api/admin/account")
              .ForContext("input", input)
              .Warning(ex, "Unauthorized access");

            return Unauthorized(new AccountPayload(ex));
        } catch (UnauthorizedException ex) {
            _log.ForContext("endpoint", "Patch:/api/admin/account")
              .ForContext("requirement", ex.Message)
              .Warning("UpdateAccountAsync requirements failure");

            return Unauthorized(new AccountPayload(ex));
        } catch (ArgumentNullException ex) {
            _log.ForContext("endpoint", "PATCH:/api/admin/account")
              .ForContext("input", input)
              .Warning(ex, "Account not found");

            return NotFound(new AccountPayload(ex));
        } catch (Exception ex) {
            _log.ForContext("endpoint", "PATCH:/api/admin/account")
              .Error(ex, "Unhandled exception");

            return StatusCode(500, new AccountPayload(ex));
        }
    }

    [HttpDelete("/api/account/{id}")]
    [Authorize(CookieAuthenticationDefaults.AuthenticationScheme)]
    public async Task<ActionResult> DeleteAccount(int id, CancellationToken token) {
        try {
            await _mediator.Send(new DeleteAccount.Command(id), token);

            return Accepted();
        } catch (UnauthorizedException ex) {
            _log.ForContext("endpoint", "DELETE:api/account")
              .ForContext("requirement", ex.Message)
              .Warning("DeleteAccount requirements failure");

            return Unauthorized(new AccountPayload(ex));
        } catch (Exception ex) {
            _log.ForContext("endpoint", "DELETE:api/account")
              .Fatal(ex, "Unhandled exception");

            return StatusCode(500, new AccountPayload(ex));
        }
    }
}
