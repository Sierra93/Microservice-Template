﻿using Calabonga.AspNetCore.Controllers.Records;
using Calabonga.Microservice.Module.Entities.Core;
using Calabonga.Microservice.Module.Web.Infrastructure.Attributes;
using Calabonga.Microservices.Core;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Calabonga.Microservice.Module.Web.Features.Account;

[Authorize]
[Produces("application/json")]
[Route("api/account")]
[FeatureGroupName("Account")]
public class GetRolesController: Controller
{
    private readonly IMediator _mediator;

    public GetRolesController(IMediator mediator) => _mediator = mediator;

    [HttpGet("[action]")]
    [Authorize(Policy = "Logs:UserRoles:View", Roles = AppData.SystemAdministratorRoleName)]
    public async Task<IActionResult> GetRoles() =>
        Ok(await _mediator.Send(new GetRolesRequest(), HttpContext.RequestAborted));

}

/// <summary>
/// Request: Returns user roles 
/// </summary>
public record GetRolesRequest : RequestBase<string>;

public class GetRolesRequestHandler : RequestHandler<GetRolesRequest, string>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetRolesRequestHandler(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    protected override string Handle(GetRolesRequest request)
    {
        var user = _httpContextAccessor.HttpContext!.User;
        var roles = ClaimsHelper.GetValues<string>((ClaimsIdentity)user.Identity!, "role");
        return $"Current user ({user.Identity!.Name}) have following roles: {string.Join("|", roles)}";
    }
}