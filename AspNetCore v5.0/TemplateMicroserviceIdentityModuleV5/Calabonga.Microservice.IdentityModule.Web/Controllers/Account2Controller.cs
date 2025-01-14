﻿using System;
using System.Threading.Tasks;
using $safeprojectname$.Infrastructure.Auth;
using $safeprojectname$.Infrastructure.Services;
using $safeprojectname$.ViewModels.AccountViewModels;
using Calabonga.Microservices.Core.Exceptions;
using Calabonga.OperationResults;
using Calabonga.UnitOfWork.Controllers.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace $safeprojectname$.Controllers
{
    /// <summary>
    /// Account Controller
    /// </summary>
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = AuthData.AuthSchemes)]
    public class Account2Controller : OperationResultController
    {
        private readonly IAccountService _accountService;

        /// <summary>
        /// Register controller
        /// </summary>
        /// <param name="accountService"></param>
        public Account2Controller(IAccountService accountService) => _accountService = accountService;

        /// <summary>
        /// Register new user. Success registration returns UserProfile for new user.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        [AllowAnonymous]
        [ProducesResponseType(200, Type = typeof(OperationResult<UserProfileViewModel>))]
        public async Task<ActionResult<OperationResult<UserProfileViewModel>>> Register([FromBody]RegisterViewModel model) =>
            !ModelState.IsValid 
                ? ValidationProblem(ModelState) 
                : OperationResultResponse(await _accountService.RegisterAsync(model));

        /// <summary>
        /// Returns profile information for authenticated user
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        [ProducesResponseType(200, Type = typeof(OperationResult<UserProfileViewModel>))]
        public async Task<ActionResult<OperationResult<UserProfileViewModel>>> Profile()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return OperationResultError<UserProfileViewModel>(null, new MicroserviceUnauthorizedException());
            }

            var userId = _accountService.GetCurrentUserId();
            if (Guid.Empty == userId)
            {
                return BadRequest();
            }

            var claimsOperationResult = await _accountService.GetProfileByIdAsync(userId.ToString());
            return Ok(claimsOperationResult);
        }
    }
}